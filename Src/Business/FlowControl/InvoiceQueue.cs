/*
    This file is part of the VeriFactu (R) project.
    Copyright (c) 2023-2024 Irene Solutions SL
    Authors: Irene Solutions SL.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    IRENE SOLUTIONS SL. IRENE SOLUTIONS SL DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
        http://www.irenesolutions.com/terms-of-use.pdf
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the VeriFactu software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving sii XML data on the fly in a web application, shipping VeriFactu
    with a closed source product.
    
    For more information, please contact Irene Solutions SL. at this
    address: info@irenesolutions.com
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using VeriFactu.Net;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Respuesta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.FlowControl
{

    /// <summary>
    /// Gestiona el envío de registros a la AEAT con los intervalos de
    /// espera establecidos entre envíos.
    /// <para> Según lo establecido en la Orden HAC/1177/2024, de 17 de octubre
    ///  en su Artículo 16.</para>
    /// </summary>
    public class InvoiceQueue : IntervalWorker
    {

        #region Variables Privadas Estáticas

        /// <summary>
        /// Máximo número de registros a incluir en un envío.
        /// </summary>
        static readonly int MaxRecordNumber = 1000;

        #endregion

        #region Variables Privadas de Instancia

        /// <summary>
        /// Almacena los registro pendientes de envío.
        /// </summary>
        readonly Dictionary<string, List<InvoiceAction>> _SellerPendingQueue;

        /// <summary>
        /// Almacena los registros envíados y que han resultado erróneos.
        /// </summary>
        readonly Dictionary<string, List<InvoiceAction>> _SellerErrorQueue;

        /// <summary>
        /// Lista de procesamiento. En esta lista
        /// se incluyen todos los elmentos a procesar
        /// una vez se desencadena el proceso de ejecución.
        /// </summary>
        List<InvoiceAction> _Processing;

        /// <summary>
        /// Bloqueo para thread safe.
        /// </summary>
        private readonly object _Locker = new object();

        /// <summary>
        /// Almacena el momento de finalización del último
        /// envío.
        /// </summary>
        DateTime _LastProcessMoment;

        /// <summary>
        /// Almacena tiempo de espera comunicado por la AEAT
        /// en el últinmo envío.
        /// </summary>
        int _CurrentWaitSecods = 60;

        /// <summary>
        /// Indica si la cola se está procesando.
        /// </summary>
        bool _IsWorking;

        #endregion

        #region Construtores Estáticos

        /// <summary>
        /// Constructor.
        /// </summary>
        static InvoiceQueue() 
        {

            ActiveInvoiceQueue = GetInstance();
            ActiveInvoiceQueue.Start();

        }

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        InvoiceQueue()
        {

            if (ActiveInvoiceQueue != null)
                throw new InvalidOperationException("Ya existe una instancia de gestor de cola actualmente en el sistema." +
                    " Únicamente puede existir una instancia creada de esta clase.");

            _LastProcessMoment = new DateTime(1, 1, 1);
            _SellerPendingQueue = new Dictionary<string, List<InvoiceAction>>();
            _SellerErrorQueue = new Dictionary<string, List<InvoiceAction>>();

            ActiveInvoiceQueue = this;

        }

        #endregion

        #region Métodos Privados Estáticos

        /// <summary>
        /// Recupera la instancia que se encarga de gestionar
        /// la cola de facturas pendientes de envío.
        /// </summary>
        /// <returns> Instancia que se encarga de gestionar
        /// la cola de facturas pendientes de envío.</returns>
        private static InvoiceQueue GetInstance() 
        {

            if (ActiveInvoiceQueue == null)
                ActiveInvoiceQueue = new InvoiceQueue();

            return ActiveInvoiceQueue;

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Procesa la cola de facturas pendientes de un emisor.
        /// </summary>
        /// <param name="invoiceActions">Lista de acciones para registros de factura.</param>
        private bool Process(List<InvoiceAction> invoiceActions) 
        {

            // 1. Contabilizo todos los registros incluyendolos
            // en la cadena de bloques correspondiente.
            var allPosted = Post(invoiceActions);

            if (!allPosted)
                return false;

            var allSent = Send(invoiceActions);

            return true;

        }

        /// <summary>
        /// Contabilizo todos los registros incluyendolos
        /// en la cadena de bloques correspondiente.
        /// </summary>
        /// <param name="invoiceActions">Lista de acciones para registros de factura.</param>
        /// <returns>Devuelve true si todos los registros se han contabilizado
        /// sin problemas.</returns>
        private bool Post(List<InvoiceAction> invoiceActions)
        {

            if (invoiceActions == null || invoiceActions.Count == 0)
                throw new ArgumentException("El parámetro invoiceActions debe" +
                    " ser una lista de instancias de InvoiceAction con elementos.");

            string sellerID = invoiceActions[0].SellerID;
            var blockchainManager = invoiceActions[0].BlockchainManager;
            var registros = new List<Registro>();
            Debug.Print($"Añadiendo registros {invoiceActions.Count} a la cadena de bloques {DateTime.Now}");
            for (int i = 0; i < invoiceActions.Count; i++) 
            {

                if (i > 0 && sellerID != invoiceActions[i].SellerID)
                    throw new InvalidOperationException($"Distintos vendedores encontrados en una misma" +
                        $" cola de envíos pendientes.\n - {invoiceActions[i-1]}.\n - {invoiceActions[i]}");

                if(!invoiceActions[i].Posted)
                    registros.Add(invoiceActions[i].Registro);

                sellerID = invoiceActions[i].SellerID;

            }

            // Añado los registros a la cadena de bloques
            blockchainManager.Add(registros);
            Debug.Print($"Actualizando datos de la cadena de bloques en {invoiceActions.Count} elementos {DateTime.Now}");
            // Actualizo los cambios
            for (int i = 0; i < invoiceActions.Count; i++)
                invoiceActions[i].SaveBlockchainChanges();
            Debug.Print($"Finalizada actualización de datos de la cadena de bloques en {invoiceActions.Count} elementos {DateTime.Now}");
            return true;

        }

        /// <summary>
        /// Envío el lote de facturas a la AEAT.
        /// </summary>
        /// <param name="invoiceActions">Lista de acciones para registros de factura.</param>
        /// <returns>Devuelve true si todos los registros se han envíado
        /// sin problemas.</returns>
        private bool Send(List<InvoiceAction> invoiceActions)
        {
            Debug.Print($"Enviando datos a la AEAT de {invoiceActions.Count} elementos {DateTime.Now}");
            if (invoiceActions == null || invoiceActions.Count == 0)
                throw new ArgumentException("El argumento invoiceActions debe contener elementos.");

            string sellerID = null;

            // Comprobaciones
            for (int i = 0; i < invoiceActions.Count; i++)
            {

                if (!invoiceActions[i].Posted)
                    throw new InvalidOperationException("Se han encontrado en la cola facturas" +
                        " sin contabilizar, por lo que no se puede realizar el envío.");

                if (i > 0 && sellerID != invoiceActions[i].SellerID)
                    throw new InvalidOperationException($"Distintos vendedores encontrados en una misma" +
                        $" cola de envíos pendientes.\n - {invoiceActions[i - 1]}.\n - {invoiceActions[i]}");

                sellerID = invoiceActions[i].SellerID;

            }

            Envelope envelope = null;
            InvoiceAction first = invoiceActions[0];
            InvoiceAction last = null;

            for (int i = 0; i < invoiceActions.Count; i++) 
            {

                if (i == 0)
                    envelope = invoiceActions[i].GetEnvelope();
                else
                    ((envelope.Body.Registro as RegFactuSistemaFacturacion).RegistroFactura as List<RegistroFactura>).Add(new RegistroFactura() { Registro = invoiceActions[i].Registro });

                last = invoiceActions[i];

            }

            var xml = new XmlParser().GetBytes(envelope, Namespaces.Items);            

            var response = last.Send(xml);

            var envelopeRespuesta = last.GetResponseEnvelope(response);

            File.WriteAllBytes($"{first.InvoiceEntryPath}{first.InvoiceEntryID}.{last.InvoiceEntryID}.xml", xml);
            File.WriteAllText($"{first.ResponsesPath}{first.InvoiceEntryID}.{last.InvoiceEntryID}.xml", response);

            _LastProcessMoment = DateTime.Now;

            var respuesta = (envelopeRespuesta.Body.Registro as RespuestaRegFactuSistemaFacturacion);

            if(respuesta != null)
                _CurrentWaitSecods = (envelopeRespuesta.Body.Registro as RespuestaRegFactuSistemaFacturacion).TiempoEsperaEnvio;
            Debug.Print($"Finalizado envío de datos a la AEAT de {invoiceActions.Count} elementos (quedan {_SellerPendingQueue.Count} colas) {DateTime.Now}");
            Debug.Print($"Establecido momento próxima ejecución (LastProcessMoment: {_LastProcessMoment} + " +
                $"CurrentWaitSecods: {_CurrentWaitSecods}) = {_LastProcessMoment.AddSeconds(_CurrentWaitSecods)}");

            return true;

        }

        /// <summary>
        /// Procesa errores.
        /// </summary>
        /// <param name="invoiceActions">Lista de acciones para registros de factura.</param>
        private void ProcessErrors(List<InvoiceAction> invoiceActions) 
        {

            throw new NotImplementedException("ProcessErrors pendiente de implementar");
        
        }

        #endregion

        #region Propiedades Públicas Estáticas

        /// <summary>
        /// Almacena el gestor de cola actualmente activo en
        /// el sitema.
        /// </summary>
        public static InvoiceQueue ActiveInvoiceQueue { get; private set; }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Momento a partir del cual ya se pueden realizar envíos.
        /// </summary>
        public DateTime AllowedFrom => _LastProcessMoment.AddSeconds(_CurrentWaitSecods);

        /// <summary>
        /// Si es true indica que el envío ya está permitido.
        /// </summary>
        public bool Allowed => AllowedFrom < DateTime.Now;

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Añade un elemento a la cola de registros
        /// pendientes de envío.
        /// </summary>
        /// <param name="invoiceAction">Acción de registro a añadir.</param>
        public void Add(InvoiceAction invoiceAction) 
        {

            if (invoiceAction.Posted)
                throw new InvalidOperationException($"La operación {invoiceAction}" +
                    $" ya está contabilizada y por lo tanto no se puede agregar a la cola.");

            var busErrors = invoiceAction.GetBusErrors();

            if (busErrors.Count > 0)
                throw new Exception($"No se puede añadir un elemento con errores en validación: {string.Join("\n", busErrors)}");

            List<InvoiceAction> sellerPendingQueue = null;

            if (_SellerPendingQueue.ContainsKey(invoiceAction.SellerID))
                sellerPendingQueue = _SellerPendingQueue[invoiceAction.SellerID];
            else 
                _SellerPendingQueue[invoiceAction.SellerID] = new List<InvoiceAction>();

            // Añado a la cola del emisor
            _SellerPendingQueue[invoiceAction.SellerID].Add(invoiceAction);

            if (_SellerPendingQueue[invoiceAction.SellerID].Count >= MaxRecordNumber)
            {

                lock (_Locker)
                    _IsWorking = true;

                Exception processException = null;

                // Compruebo el certificado
                var cert = Wsd.GetCheckedCertificate();

                if (cert == null)
                    throw new Exception("Existe algún problema con el certificado.");

                lock (_Locker)
                {

                    try
                    {
                        Debug.Print($"Ejecutando por cola con 1.000 elementos: {sellerPendingQueue.Count}");
                        _Processing = sellerPendingQueue;
                        _SellerPendingQueue.Remove(invoiceAction.SellerID);

                    }
                    catch (Exception ex)
                    {

                        processException = ex;

                    }

                }

                if (processException != null)
                    throw new Exception($"Error proceando cola.", processException);

                var processed = Process(_Processing);

                lock (_Locker)
                    _IsWorking = false;

                if (!processed)
                    throw new Exception("Error al vaciar la cola de documentos pendientes de envío. " +
                        $"El elemento no se puede agregar ya que la cola está llena con {_SellerPendingQueue.Count}" +
                        $" registros que es igual o mayor que el máximo {MaxRecordNumber}");
                else
                    _Processing = null;

            }

        }

        /// <summary>
        /// Proceso a ejecutar periódicamente entre
        /// intervalos.
        /// </summary>
        public override void Execute()
        {

            try
            {

                if (Allowed && !_IsWorking)
                    Process();

            }
            catch (Exception ex)
            {

                Debug.Print($"InvoiceQueue error {ex}.");

            }

        }

        /// <summary>
        /// Procesa toda la cola emisor a emisor.
        /// </summary>
        public void Process()
        {
            Debug.Print($"Ejecutando por cola tras tiempo espera en segundos: {_CurrentWaitSecods} desde {_LastProcessMoment} hasta {AllowedFrom}");
            lock (_Locker)
                _IsWorking = true;

            Exception processException = null;

            // Compruebo el certificado
            var cert = Wsd.GetCheckedCertificate();

            if (cert == null)
                throw new Exception("Existe algún problema con el certificado.");           

            foreach (KeyValuePair<string, List<InvoiceAction>> kvpInvoiceAction in _SellerPendingQueue)
            {                

                lock (_Locker)
                {

                    try
                    {
                        _Processing = kvpInvoiceAction.Value;
                        _SellerPendingQueue.Remove(kvpInvoiceAction.Key);

                    }
                    catch (Exception ex)
                    {

                        processException = ex;

                    }

                }

                if (processException != null)
                    throw new Exception($"Error proceando cola.", processException);

                
                break;

            }

            if (_Processing != null) 
            {

                var processed = Process(_Processing);

                if (!processed)
                    ProcessErrors(_Processing);
                else
                    _Processing = null;

            }           

            lock (_Locker)
                _IsWorking = false;

        }

        #endregion

    }
}
