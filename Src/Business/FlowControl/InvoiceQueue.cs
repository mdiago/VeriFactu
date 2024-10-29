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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// Almacena el gestor de cola actualmente activo en
        /// el sitema.
        /// </summary>
        static InvoiceQueue ActiveInvoiceQueue;

        #endregion

        #region Variables Privadas de Instancia

        /// <summary>
        /// Almacena los registro pendientes de envío.
        /// </summary>
        Dictionary<string, List<InvoiceAction>> _SellerPendingQueue;

        /// <summary>
        /// Almacena los registros envíados y que han resultado erróneos.
        /// </summary>
        Dictionary<string, List<InvoiceAction>> _SellerErrorQueue;

        /// <summary>
        /// Almacena el momento de finalización del último
        /// envío.
        /// </summary>
        DateTime _LastProcessMoment;

        /// <summary>
        /// Almacena tiempo de espera comunicado por la AEAT
        /// en el últinmo envío.
        /// </summary>
        int _CurrentWaitSecods;

        #endregion

        #region Propiedades Privadas Estáticas
        #endregion

        #region Propiedades Privadas de Instacia
        #endregion

        #region Construtores Estáticos
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

        #region Indexadores
        #endregion

        #region Métodos Privados Estáticos

        /// <summary>
        /// Recupera la instancia que se encarga de gestionar
        /// la cola de facturas pendientes de envío.
        /// </summary>
        /// <returns> Instancia que se encarga de gestionar
        /// la cola de facturas pendientes de envío.</returns>
        public static InvoiceQueue GetInstance() 
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

            string sellerID = null;

            for (int i = 0; i < invoiceActions.Count; i++) 
            {

                if (i > 0 && sellerID != invoiceActions[i].SellerID)
                    throw new InvalidOperationException($"Distintos vendedores encontrados en una misma" +
                        $" cola de envíos pendientes.\n - {invoiceActions[i-1]}.\n - {invoiceActions[i]}");

                if(!invoiceActions[i].Posted)
                    invoiceActions[i].ExecutePost();

                sellerID = invoiceActions[i].SellerID;

            }

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
            InvoiceAction last = null;

            for (int i = 0; i < invoiceActions.Count; i++) 
            {

                if (i == 0)
                    envelope = invoiceActions[0].GetEnvelope();
                else
                    ((envelope.Body.Registro as RegFactuSistemaFacturacion).RegistroFactura as List<object>).Add(invoiceActions[0].Registro);

                last = invoiceActions[i];

            }

            var xml = new XmlParser().GetBytes(envelope, Namespaces.Items);

            File.WriteAllBytes(@"C:\Users\manuel\Downloads\z\z.xml", xml);

            var response = last.Send(xml);

            File.WriteAllText(@"C:\Users\manuel\Downloads\z\zz.xml", response);

            var envelopeRespuesta = last.GetResponseEnvelope(response);

            _LastProcessMoment = DateTime.Now;
            _CurrentWaitSecods = (envelopeRespuesta.Body.Registro as RespuestaRegFactuSistemaFacturacion).TiempoEsperaEnvio;

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

        #region Métodos Públicos Estáticos
        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Añade un elemento a la cola de registros
        /// pendientes de envío.
        /// </summary>
        /// <param name="invoiceAction">Acción de registro a añadir.</param>
        public void Add(InvoiceAction invoiceAction) 
        {

            if (_SellerPendingQueue.ContainsKey(invoiceAction.SellerID))
                _SellerPendingQueue[invoiceAction.SellerID].Add(invoiceAction);
            else
                _SellerPendingQueue.Add(invoiceAction.SellerID, new List<InvoiceAction>() { invoiceAction });


        }

        /// <summary>
        /// Proceso a ejecutar periódicamente entre
        /// intervalos.
        /// </summary>
        public override void Execute()
        {

            try
            {

                if (Allowed)
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

            foreach (KeyValuePair<string, List<InvoiceAction>> kvpInvoiceAction in _SellerPendingQueue)
            {

                var processed = Process(kvpInvoiceAction.Value);

                if (processed)
                    _SellerPendingQueue.Remove(kvpInvoiceAction.Key);
                else
                    ProcessErrors(kvpInvoiceAction.Value);

                if (!Allowed)
                    break;

            }

        }

        #endregion


    }
}
