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
    serving VeriFactu XML data on the fly in a web application, shipping VeriFactu
    with a closed source product.
    
    For more information, please contact Irene Solutions SL. at this
    address: info@irenesolutions.com
 */

using System;
using System.Collections.Generic;
using System.IO;
using VeriFactu.Business.Operations;
using VeriFactu.Common;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Respuesta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.FlowControl
{

    /// <summary>
    /// Representa la cola de envío de documentos de un emisor
    /// determinado.
    /// </summary>
    public class SellerQueue : SingletonByKey<SellerQueue>
    {

        #region Variables Privadas Estáticas

        /// <summary>
        /// Máximo número de registros a incluir en un envío.
        /// </summary>
        static readonly int MaxRecordNumber = 1000;

        #endregion

        #region Variables Privadas de Instancia

        /// <summary>
        /// Lista de procesamiento. En esta lista
        /// se incluyen todos los elmentos a procesar.
        /// </summary>
        Queue<InvoiceAction> _InvoiceActions = new Queue<InvoiceAction>();

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

        #endregion

        #region Propiedades Privadas de Instancia

        /// <summary>
        /// Identificador del vendedor.
        /// Debe utilizarse el identificador fiscal si existe (NIF, VAT Number...).
        /// En caso de no existir, se puede utilizar el número DUNS 
        /// o cualquier otro identificador acordado.
        /// </summary>        
        internal string SellerID { get; private set; }

        /// <summary>
        /// Momento a partir del cual ya se pueden realizar envíos.
        /// </summary>
        internal DateTime AllowedFrom => _LastProcessMoment.AddSeconds(_CurrentWaitSecods);

        /// <summary>
        /// Si es true indica que el envío ya está permitido
        /// por haberse superado el último tiempo de espera comunicado
        /// por la AEAT.
        /// </summary>
        internal bool IsAllowedFrom => AllowedFrom < DateTime.Now;

        /// <summary>
        /// Si es true indica que el envío ya está permitido
        /// por haberse alcanzado el máximo de registros por lote
        /// establecido por la AEAT.
        /// </summary>
        internal bool IsAllowedMaxRecordNumber => _InvoiceActions.Count >= MaxRecordNumber;

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sellerID">Vendedor al que pertenece la cola de proceso.</param>
        public SellerQueue(string sellerID) : base(sellerID)
        {

            SellerID = sellerID;
            _LastProcessMoment = new DateTime(1, 1, 1);

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Contabiliza los elementos a envíar eliminándolos de la cola
        /// de envío y devolviendolos en una lista.
        /// </summary>
        /// <returns>Lista de los elementos contabilizados.</returns>
        private List<InvoiceAction> Post()
        {

            Utils.Log($"Ejecutando por cola ({SellerID}) tras tiempo espera en segundos:" +
                $" {_CurrentWaitSecods} desde {_LastProcessMoment} hasta {AllowedFrom}");

            var recordCount = 0;
            var registros = new List<Registro>();
            var invoiceActions = new List<InvoiceAction>();

            while (_InvoiceActions.Count > 0 && SellerQueue.MaxRecordNumber > recordCount++)
            {

                var invoiceAction = _InvoiceActions.Dequeue();

                if (!invoiceAction.Posted)
                {

                    invoiceActions.Add(invoiceAction);
                    registros.Add(invoiceAction.Registro);

                }
                else
                {

                    Utils.Log($"Error en el proceso por lotes ({SellerID}) se ha intentado agregar al envío un registro ya contabilizado: {invoiceAction}");

                }

            }

            var blockchainManager = Blockchain.Blockchain.GetInstance(SellerID) as Blockchain.Blockchain;

            // Añado los registros a la cadena de bloques
            blockchainManager.Add(registros);
            Utils.Log($"Actualizando datos de la cadena de bloques ({SellerID}) en {registros.Count} elementos {DateTime.Now}");

            // Actualizo los cambios
            for (int i = 0; i < invoiceActions.Count; i++)
                invoiceActions[i].SaveBlockchainChanges();

            Utils.Log($"Finalizada actualización de datos de la cadena de bloques en {invoiceActions.Count} elementos {DateTime.Now}");

            return invoiceActions;

        }

        /// <summary>
        /// Envío el lote de facturas a la AEAT.
        /// </summary>
        /// <param name="invoiceActions">Lista de acciones para registros de factura.</param>
        /// <returns>Devuelve La respuesta de la AEAT al envío.</returns>
        private RespuestaRegFactuSistemaFacturacion Send(List<InvoiceAction> invoiceActions)
        {

            Utils.Log($"Enviando datos a la AEAT {SellerID} de {invoiceActions.Count} elementos {DateTime.Now}");

            if (invoiceActions == null || invoiceActions.Count == 0)
                throw new ArgumentException("El argumento invoiceActions debe contener elementos.");
  

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

            if (respuesta != null)
                _CurrentWaitSecods = (envelopeRespuesta.Body.Registro as RespuestaRegFactuSistemaFacturacion).TiempoEsperaEnvio;

            Utils.Log($"Finalizado envío de datos {SellerID} a la AEAT de {invoiceActions.Count} elementos (quedan {_InvoiceActions.Count} registros) {DateTime.Now}");
            Utils.Log($"Establecido momento próxima ejecución {SellerID} (LastProcessMoment: {_LastProcessMoment} + " +
                $"CurrentWaitSecods: {_CurrentWaitSecods}) = {_LastProcessMoment.AddSeconds(_CurrentWaitSecods)}");

            return respuesta;

        }

        /// <summary>
        /// Procesa la respuesta de la AEAT.
        /// </summary>
        /// <param name="aeatResponse">Respuesta a procesar.</param>
        /// <param name="invoiceActions">Elementos a los que corresponde la respuesta.</param>
        private void ProcessReponse(RespuestaRegFactuSistemaFacturacion aeatResponse, List<InvoiceAction> invoiceActions) 
        {

            var invoices = new Dictionary<string, InvoiceAction>();

            foreach (var invoiceAction in invoiceActions) 
                if (!invoices.ContainsKey(invoiceAction.Registro.ExternKey))
                    invoices.Add(invoiceAction.Registro.ExternKey, invoiceAction);

            foreach (var line in aeatResponse.RespuestaLinea) 
            {

                var externKey = line.RefExterna;
                var invoice = invoices[externKey];

                var invoiceEnvelope = new Envelope()
                {
                    Body = new Body()
                    {
                        Registro = new RespuestaRegFactuSistemaFacturacion()
                        {
                            Cabecera = aeatResponse.Cabecera,
                            CSV = aeatResponse.CSV,
                            EstadoEnvio = aeatResponse.EstadoEnvio,
                            DatosPresentacion = aeatResponse.DatosPresentacion,
                            RespuestaLinea = new List<RespuestaLinea>() { line }
                        }
                    }
                };

                var respuesta = invoiceEnvelope.Body.Registro as RespuestaRegFactuSistemaFacturacion;

                Utils.Log($"El resultado del envío del documento {invoice} ha sido '{line.EstadoRegistro}'.");

                if (line.EstadoRegistro == "Incorrecto")
                {

                    respuesta.CSV = null;
                    respuesta.EstadoEnvio = line.EstadoRegistro;

                }
                else if (line.EstadoRegistro == "Correcto")
                {
                    
                    respuesta.EstadoEnvio = line.EstadoRegistro;

                }

                invoice.ResponseEnvelope = invoiceEnvelope;
                invoice.ProcessResponse(invoiceEnvelope);

            }
        
        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Si es true indica que el envío ya está permitido
        /// por haberse alcanzado el máximo de registros por lote
        /// establecido por la AEAT.
        /// </summary>
        internal int Count => _InvoiceActions.Count;

        #endregion

        #region Métodos Públicos Estáticos

        /// <summary>
        /// Devuelve la instancia correspondiente a la cola de registros
        /// de un emisor de facturas.
        /// </summary>
        /// <param name="sellerID">Id. del emisor de factura.</param>
        /// <returns>Instancia correspondiente a la cola de registros
        /// de un emisor de facturas.</returns>
        public static SellerQueue Get(string sellerID)
        {

            return GetInstance(sellerID) as SellerQueue;

        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Añade un elemento a la cola de registros
        /// pendientes de envío.
        /// </summary>
        /// <param name="invoiceAction">Acción de registro a añadir.</param>
        internal void Add(InvoiceAction invoiceAction)
        {

            if (invoiceAction.Posted)
                throw new InvalidOperationException($"La operación {invoiceAction}" +
                    $" ya está contabilizada y por lo tanto no se puede agregar a la cola.");

            var busErrors = invoiceAction.GetBusErrors();

            if (busErrors.Count > 0)
                throw new Exception($"No se puede añadir un elemento con errores en validación: {string.Join("\n", busErrors)}");

            // Añado a la cola del emisor
            _InvoiceActions.Enqueue(invoiceAction);

        }

        /// <summary>
        /// Procesa toda la cola emisor a emisor.
        /// </summary>
        /// <returns>Lista de los elementos procesados.</returns>
        internal void Process()
        {

            if (!IsAllowedFrom && !IsAllowedMaxRecordNumber)
                return;

            if (_InvoiceActions.Count == 0)
                return;


            var postedInvoiceActions = Post();
            var aeatResponse = Send(postedInvoiceActions);
            
            ProcessReponse(aeatResponse, postedInvoiceActions);

            // Ejecuto manejador del evento SentFinished en su caso
            if (InvoiceQueue.SentFinished != null)
                InvoiceQueue.SentFinished(postedInvoiceActions, aeatResponse);


        }

        #endregion

    }

}
