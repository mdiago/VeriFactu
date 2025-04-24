/*
    This file is part of the VeriFactu (R) project.
    Copyright (c) 2024-2025 Irene Solutions SL
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
using System.Diagnostics;
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
    /// Representa un lote de facturas a enviar a la AEAT.
    /// </summary>
    public class InvoiceBatch
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Colección de registros a enviar.
        /// </summary>
        List<InvoiceAction> _InvoiceActions;

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        internal InvoiceBatch()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sellerID"></param>
        public InvoiceBatch(string sellerID)
        {

            SellerID = sellerID;
            _InvoiceActions = new List<InvoiceAction>();

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Envío el lote de facturas a la AEAT.
        /// </summary>
        /// <param name="invoiceActions">Lista de acciones para registros de factura.</param>
        /// <returns>Devuelve La respuesta de la AEAT al envío.</returns>
        internal RespuestaRegFactuSistemaFacturacion Send(List<InvoiceAction> invoiceActions)
        {

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


            var respuesta = (envelopeRespuesta.Body.Registro as RespuestaRegFactuSistemaFacturacion);

            return respuesta;

        }

        /// <summary>
        /// Contabiliza los elementos a envíar eliminándolos de la cola
        /// de envío y devolviendolos en una lista.
        /// </summary>
        /// <param name="invoiceActions">Lista de elementos a contabilizar.</param>
        /// <returns>Lista de los elementos a contabilizar.</returns>
        private List<InvoiceAction> Post(List<InvoiceAction> invoiceActions)
        {

            var registros = new List<Registro>();

            foreach (var invoice in invoiceActions)
                registros.Add(invoice.Registro); // Almaceno el registo para contabilizar

            // Ahora obtenemos el controlador de la cadena de bloques del vendedor
            var blockchainManager = Blockchain.Blockchain.Get(SellerID);

            // Añado los registros a la cadena de bloques
            if (registros.Count > 0)
                blockchainManager.Add(registros);

            // Actualizo los cambios
            for (int i = 0; i < invoiceActions.Count; i++)
                invoiceActions[i].SaveBlockchainChanges();

            return invoiceActions;

        }

        /// <summary>
        /// Procesa la respuesta de la AEAT.
        /// </summary>
        /// <param name="aeatResponse">Respuesta a procesar.</param>
        /// <param name="invoiceActions">Elementos a los que corresponde la respuesta.</param>
        /// <returns>Diccionario por número de factura con los resultados.</returns>
        internal Dictionary<string, InvoiceAction> ProcessReponse(RespuestaRegFactuSistemaFacturacion aeatResponse, List<InvoiceAction> invoiceActions)
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
                Debug.Print($"El resultado del envío del documento {invoice} ha sido '{line.EstadoRegistro}'.");

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

            return invoices;

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Identificador del vendedor.
        /// Debe utilizarse el identificador fiscal si existe (NIF, VAT Number...).
        /// En caso de no existir, se puede utilizar el número DUNS 
        /// o cualquier otro identificador acordado.
        /// </summary>        
        public string SellerID { get; private set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Añade una entrada de factura al lote.
        /// </summary>
        /// <param name="invoiceEntry">Entrada de factura a añadir.</param>
        public void Add(InvoiceEntry invoiceEntry)
        {

            if(_InvoiceActions.Count == SellerQueue.MaxRecordNumber)
                throw new ArgumentException($"El lote no puede contener más de {SellerQueue.MaxRecordNumber} entradas de factura.");

            if (invoiceEntry.SellerID != SellerID)
                throw new ArgumentException($"El vendedor de la entrada de factura no coincide con el lote.");

            if (invoiceEntry.IsRetrySend)
                throw new ArgumentException($"No se permite la adición de reenvíos.");

            if (invoiceEntry.Posted)
                throw new ArgumentException($"No se permite la adición de entradas de factura ya contabilizadas.");

            if (invoiceEntry.Posted)
                throw new ArgumentException($"No se permite la adición de entradas de factura ya contabilizadas.");

            if (invoiceEntry.GetBusErrors().Count > 0)
                throw new ArgumentException($"No se puede añadir una entrada de factura con errores en validación: {string.Join("\n", invoiceEntry.GetBusErrors())}");

            _InvoiceActions.Add(invoiceEntry);

        }

        /// <summary>
        /// Contabiliza en la cadena de bloques, envía a la AEAT
        /// y devuelve los resultados del lote.
        /// </summary>
        /// <returns>Resultado del guardado del lote.</returns>
        public Dictionary<string, InvoiceAction> Save()
        {

            var postedInvoiceActions = Post(_InvoiceActions);
            var aeatResponse = Send(postedInvoiceActions);
            return ProcessReponse(aeatResponse, postedInvoiceActions);

        }

        #endregion

    }

}