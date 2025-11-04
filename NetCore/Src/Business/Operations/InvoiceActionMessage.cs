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
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using VeriFactu.Config;
using VeriFactu.Net;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Fault;
using VeriFactu.Xml.Factu.Respuesta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Operations
{

    /// <summary>
    /// Representa una acción de alta o anulación de registro
    /// en todo lo referente a su envío al web service de la AEAT
    /// y su posterior tratamiento.
    /// </summary>
    public class InvoiceActionMessage : InvoiceActionData
    {

        #region Propiedades Privadas Estáticas

        /// <summary>
        /// Acción para el webservice.
        /// </summary>
        static string _Action = "?op=RegFactuSistemaFacturacion";

        #endregion

        #region Propiedades Privadas de Instacia

        /// <summary>
        /// Acción para el webservice.
        /// </summary>
        internal virtual string Action => _Action;

        /// <summary>
        /// Sobre SOAP de respuesta de la AEAT.
        /// </summary>
        internal Envelope ResponseEnvelope { get; set; }

        /// <summary>
        /// Error Fault.
        /// </summary>
        internal Fault ErrorFault
        {

            get
            {

                if (ResponseEnvelope == null)
                    return null;

                return ResponseEnvelope.Body.Registro as Fault;

            }

        }

        /// <summary>
        /// Respueta AEAT.
        /// </summary>
        internal RespuestaRegFactuSistemaFacturacion RespuestaRegFactuSistemaFacturacion
        {

            get
            {

                if (ResponseEnvelope == null)
                    return null;

                return ResponseEnvelope.Body.Registro as RespuestaRegFactuSistemaFacturacion;

            }

        }

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="invoiceID">Identificador de la factura.</param>
        /// <param name="invoiceDate">Fecha emisión de documento.</param>
        /// <param name="sellerID">Identificador del vendedor.</param>        
        /// <exception cref="ArgumentNullException">Los argumentos invoiceID y sellerID no pueden ser nulos</exception>
        public InvoiceActionMessage(string invoiceID, DateTime invoiceDate, string sellerID) : base(invoiceID, invoiceDate, sellerID)
        {
            OutboxPath = GetOutBoxPath(Invoice.SellerID);
            InboxPath = GetInBoxPath(Invoice.SellerID);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="invoice">Instancia de factura de entrada en el sistema.</param>
        public InvoiceActionMessage(Invoice invoice) : base(invoice)
        {

            OutboxPath = GetOutBoxPath(Invoice.SellerID);
            InboxPath = GetInBoxPath(Invoice.SellerID);

            // Generamos el xml
            Xml = GetXml();

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Devuelve la ruta de almacenamiento de los
        /// registros contabilizados y envíados para un
        /// vendedor en concreto.
        /// </summary>
        /// <param name="sellerID">Emisor al que pertenece el
        /// envío de registro a gestionar.</param>
        /// <returns>Ruta de los registros contabilizados y
        /// envíados para un vendedor en concreto.</returns>
        internal string GetOutBoxPath(string sellerID)
        {

            return GetDirPath($"{Settings.Current.OutboxPath}{sellerID}");

        }

        /// <summary>
        /// Devuelve la ruta de almacenamiento de repuestas de los
        /// registros contabilizados y envíados para un
        /// vendedor en concreto.
        /// </summary>
        /// <param name="sellerID">Emisor al que pertenece el
        /// envío de registro a gestionar.</param>
        /// <returns>Ruta respuestas de los registros contabilizados y
        /// envíados para un vendedor en concreto.</returns>
        internal string GetInBoxPath(string sellerID)
        {

            return GetDirPath($"{Settings.Current.InboxPath}{sellerID}");

        }

        /// <summary>
        /// Devuelve la ruta de almacenamiento de los
        /// registros contabilizados y envíados para un
        /// año en concreto.
        /// </summary>
        /// <param name="year">Año al que pertenece el
        /// envío de registro a gestionar.</param>
        /// <returns>Ruta de los registros contabilizados y
        /// envíados para un vendedor en concreto.</returns>
        internal string GetInvoiceEntryPath(string year)
        {

            return GetDirPath($"{OutboxPath}{year}");

        }

        /// <summary>
        /// Devuelve la ruta de almacenamiento de respuestas de los
        /// registros contabilizados y envíados para un
        /// año en concreto.
        /// </summary>
        /// <param name="year">Año al que pertenece el
        /// envío de registro a gestionar.</param>
        /// <returns>Ruta respuestas de los registros contabilizados y
        /// envíados para un vendedor en concreto.</returns>
        internal string GetResponsesPath(string year)
        {

            return GetDirPath($"{InboxPath}{year}");

        }

        /// <summary>
        /// Genera el sobre SOAP.
        /// </summary>
        /// <returns>Sobre SOAP.</returns>
        internal virtual Envelope GetEnvelope()
        {

            return new Envelope()
            {
                Body = new Body()
                {
                    Registro = new RegFactuSistemaFacturacion()
                    {
                        Cabecera = new Xml.Factu.Cabecera()
                        {
                            ObligadoEmision = new Interlocutor()
                            {
                                NombreRazon = Invoice.SellerName,
                                NIF = Invoice.SellerID
                            }
                        },
                        RegistroFactura = new List<RegistroFactura>()
                        {
                            new RegistroFactura()
                            {
                                Registro = Registro
                            }
                        }
                    }
                }
            };

        }

        /// <summary>
        /// Devuelve el nombre completo de un archivo xml de respuesta
        /// de la AEAT cuyo envío ha resultado erróneo.
        /// </summary>
        /// <returns>Nombre completo de un archivo xml de respuesta
        /// de la AEAT que ha resultado erróneo.</returns>
        internal string GetErrorResponseFilePath()
        {

            return $"{ResponsesPath}{InvoiceEntryID}.ERR.{DateTime.Now:yyyy.MM.dd.HH.mm.ss.ffff}.xml";

        }

        /// <summary>
        /// Path de la entrada factura en el directorio de archivado de los datos de la
        /// cadena si el documento a resultado erróneo.
        /// </summary>
        /// <returns>Path de la factura en el directorio de archivado de los datos de la
        /// cadena si el documento a resultado erróneo.</returns>
        protected string GetErrorInvoiceEntryFilePath()
        {

            return $"{InvoiceEntryPath}{InvoiceEntryID}.ERR.{DateTime.Now:yyyy.MM.dd.HH.mm.ss.ffff}.xml";

        }

        /// <summary>
        /// Envía un xml en formato binario a la AEAT.
        /// </summary>
        /// <param name="xml">Archivo xml en formato binario a la AEAT.</param>
        /// <param name="certificate">Certificado para la petición.</param>
        /// <returns>Devuelve las respuesta de la AEAT.</returns>
        internal string Send(byte[] xml, X509Certificate2 certificate = null)
        {

            return SendXmlBytes(xml, Action, certificate);

        }

        /// <summary>
        /// Envía el registro a la AEAT.
        /// </summary>
        /// <param name="certificate">Certificado para la petición.</param>
        /// <returns>Devuelve las respuesta de la AEAT.</returns>
        protected string Send(X509Certificate2 certificate = null)
        {

            return Send(Xml, certificate);

        }

        /// <summary>
        /// Ejecuta la contabilización del registro.
        /// </summary>
        /// <param name="certificate">Certificado para la petición.</param>
        /// <returns>Si todo funciona correctamente devuelve null.
        /// En caso contrario devuelve una excepción con el error.</returns>
        protected void ExecuteSend(X509Certificate2 certificate = null)
        {

            if (!Posted)
                throw new InvalidOperationException("No se puede enviar un registro no contabilizado (Posted = false).");

            Response = Send(certificate);
            IsSent = true;

        }

        /// <summary>
        /// Procesa y guarda respuesta de la AEAT al envío.
        /// </summary>
        /// <param name="response">Texto del xml de respuesta.</param>
        internal void ProcessResponse(string response)
        {

            ResponseEnvelope = GetResponseEnvelope(response);
            ProcessResponse(ResponseEnvelope);

        }

        /// <summary>
        /// Procesa y guarda respuesta de la AEAT al envío.
        /// </summary>
        /// <param name="envelope">Sobre con la respuesta de la AEAT.</param>
        internal virtual void ProcessResponse(Envelope envelope)
        {

            var invoiceEntryFilePath = string.IsNullOrEmpty(CSV) ? GetErrorInvoiceEntryFilePath() : InvoiceEntryFilePath;
            var responseFilePath = string.IsNullOrEmpty(CSV) ? GetErrorResponseFilePath() : ResponseFilePath;

            // Almaceno xml envíado
            File.WriteAllBytes(invoiceEntryFilePath, Xml);

            // Almaceno xml de respuesta
            if (!string.IsNullOrEmpty(Response))
                File.WriteAllText(responseFilePath, Response);

            // Si la respuesta no ha sido correcta o aceptada con errores renombro archivo de factura
            var notCorrectoOAceptado = GetIsNotCorrectoOAceptado();

            if (notCorrectoOAceptado && File.Exists(InvoiceFilePath))
                File.Move(InvoiceFilePath, GetErrorInvoiceFilePath());

        }

        /// <summary>
        /// Devuelve true si el fichero no está en la AEAT
        /// (es decir si no es 'Correcto' o 'AceptadoConErrores')
        /// </summary>
        /// <returns>Devuelve true si el fichero no está en la AEAT.</returns>
        internal bool GetIsNotCorrectoOAceptado() 
        {

            // Si la respuesta no ha sido correcta o aceptada con errores devuelve true
            string estadoRegistro = null;

            if (ErrorFault == null && RespuestaRegFactuSistemaFacturacion?.RespuestaLinea != null && RespuestaRegFactuSistemaFacturacion.RespuestaLinea.Count > 0)
                estadoRegistro = RespuestaRegFactuSistemaFacturacion.RespuestaLinea[0].EstadoRegistro;

            var correcto = Status == "Correcto";
            var aceptadoConErrores = Status == "ParcialmenteCorrecto" && estadoRegistro == "AceptadoConErrores";
            var notCorrectoOAceptado = !(correcto || aceptadoConErrores);

            return notCorrectoOAceptado;
        }

        /// <summary>
        /// Guarda respuesta de la AEAT al envío.
        /// </summary>
        internal void ProcessResponse()
        {

            ProcessResponse(Response);
            ResponseProcessed = true;

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Identificador del eslabón de la cadena asociado
        /// a la factura.
        /// </summary>
        public virtual string InvoiceEntryID => Registro?.BlockchainLinkID == null ?
            null : $"{Registro.BlockchainLinkID}".PadLeft(20, '0');

        /// <summary>
        /// Path del directorio de archivado de los datos de la
        /// cadena.
        /// </summary>
        public string OutboxPath { get; private set; }

        /// <summary>
        /// Path del directorio de archivado de los datos de la
        /// cadena.
        /// </summary>
        public string InboxPath { get; private set; }

        /// <summary>
        /// Path del directorio de archivado de los datos de la
        /// cadena.
        /// </summary>
        public string InvoiceEntryPath => Registro?.FechaHoraHusoGenRegistro == null ? null : GetInvoiceEntryPath($"{Registro.FechaHoraHusoGenRegistro.Substring(0, 4)}");

        /// <summary>
        /// Path del directorio de archivado de los datos de la
        /// cadena.
        /// </summary>
        public string ResponsesPath => Registro?.FechaHoraHusoGenRegistro == null ? null : GetResponsesPath($"{Registro.FechaHoraHusoGenRegistro.Substring(0, 4)}");

        /// <summary>
        /// Path de la factura en el directorio de archivado de los datos de la
        /// cadena.
        /// </summary>
        public virtual string InvoiceEntryFilePath => $"{InvoiceEntryPath}{InvoiceEntryID}.xml";

        /// <summary>
        /// Path del directorio de archivado de los datos de la
        /// cadena.
        /// </summary>
        public virtual string ResponseFilePath => $"{ResponsesPath}{InvoiceEntryID}.xml";

        /// <summary>
        /// Sobre SOAP.
        /// </summary>
        public Envelope Envelope { get; private set; }

#pragma warning disable CA1819

        /// <summary>
        /// Datos binarios del archivo xml de envío.
        /// </summary>
        public byte[] Xml { get; protected set; }

#pragma warning restore CA1819

        /// <summary>
        /// Respuesta del envío a la AEAT.
        /// </summary>
        public string Response { get; private set; }

        /// <summary>
        /// Código de error.
        /// </summary>
        public string ErrorCode
        {

            get
            {

                if (ErrorFault != null)
                    return ErrorFault.faultcode;

                if (RespuestaRegFactuSistemaFacturacion?.RespuestaLinea != null &&
                    RespuestaRegFactuSistemaFacturacion?.RespuestaLinea.Count > 0 &&
                    !string.IsNullOrEmpty(RespuestaRegFactuSistemaFacturacion.RespuestaLinea[0].CodigoErrorRegistro))
                    return RespuestaRegFactuSistemaFacturacion.RespuestaLinea[0].CodigoErrorRegistro;

                return null;

            }

        }

        /// <summary>
        /// Código de error.
        /// </summary>
        public string ErrorDescription
        {

            get
            {

                if (ErrorFault != null)
                    return ErrorFault.faultstring;

                if (RespuestaRegFactuSistemaFacturacion?.RespuestaLinea != null &&
                    RespuestaRegFactuSistemaFacturacion.RespuestaLinea.Count > 0 &&
                    !string.IsNullOrEmpty(RespuestaRegFactuSistemaFacturacion.RespuestaLinea[0].DescripcionErrorRegistro))
                    return RespuestaRegFactuSistemaFacturacion.RespuestaLinea[0].DescripcionErrorRegistro;

                return null;

            }

        }

        /// <summary>
        /// Código de error.
        /// </summary>
        public string Status
        {

            get
            {

                if (RespuestaRegFactuSistemaFacturacion == null)
                    return null;

                return RespuestaRegFactuSistemaFacturacion.EstadoEnvio;

            }

        }

        /// <summary>
        /// Código de error.
        /// </summary>
        public string CSV
        {

            get
            {

                if (RespuestaRegFactuSistemaFacturacion == null)
                    return null;

                return RespuestaRegFactuSistemaFacturacion.CSV;

            }

        }

        /// <summary>
        /// Indica si el registro ha sido envíado y la respuesta ha
        /// sido procesada de la AEAT.
        /// </summary>
        public bool ResponseProcessed { get; private set; }

        #endregion

        #region Métodos Públicos Estáticos

        /// <summary>
        /// Envía un xml en formato binario a la AEAT.
        /// </summary>
        /// <param name="xml">Archivo xml en formato binario a la AEAT.</param>
        /// <param name="op"> Acción para el webservice.</param>
        /// <param name="certificate">Certificado para la petición.</param>
        /// <returns>Devuelve las respuesta de la AEAT.</returns>
        public static string SendXmlBytes(byte[] xml, string op = null, X509Certificate2 certificate = null)
        {

            if (op == null)
                op = _Action;

            XmlDocument xmlDocument = new XmlDocument();

            using (var msXml = new MemoryStream(xml))
                xmlDocument.Load(msXml);

            var url = Settings.Current.VeriFactuEndPointPrefix;
            var action = $"{url}{op}";

            return Wsd.Call(url, action, xmlDocument, certificate);

        }

        /// <summary>
        /// Envía un sobre SOAP.
        /// </summary>
        /// <param name="envelope">Sobre a enviar.</param>
        /// <param name="op">Acción del webservice.</param>
        /// <param name="certificate">Certificado para la petición.</param>
        /// <returns>Respuesta del servidor.</returns>
        public static string SendEnvelope(Envelope envelope, string op, X509Certificate2 certificate = null) 
        {

            // Generamos el xml
            var xml = new XmlParser().GetBytes(envelope, Namespaces.Items);

            return SendXmlBytes(xml, op, certificate);

        }

        /// <summary>
        /// Envía un sobre SOAP.
        /// </summary>
        /// <param name="envelope">Sobre a enviar.</param>
        /// <param name="certificate">Certificado para la petición.</param>
        /// <returns>Respuesta del servidor.</returns>
        public static Envelope SendEnvelope(Envelope envelope, X509Certificate2 certificate = null)
        {

            // Generamos el xml
            var xml = new XmlParser().GetBytes(envelope, Namespaces.Items);
            var response =  SendXmlBytes(xml, _Action, certificate);

            return Envelope.FromXml(response);

        }

        /// <summary>
        /// Devuelve la acción para el webservice de envío
        /// de registros.
        /// </summary>
        /// <returns>Acción para el webservice de envío
        /// de registros.</returns>
        public static string GetAction() 
        { 
             return _Action;
        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Serializa como sobre soap un string de respuesta
        /// de la AEAT.
        /// </summary>
        /// <param name="response">Texto con la respuesta xml de la AEAT.</param>
        /// <returns>Objeto Envelope con la respuesta.</returns>
        public Envelope GetResponseEnvelope(string response)
        {

            if (string.IsNullOrEmpty(response))
                throw new InvalidOperationException("No existe ninguna respuesta que guardar.");

            return Envelope.FromXml(response);

        }

        /// <summary>
        /// Devuelve los bytes del XML serializado con los 
        /// datos actuales.
        /// </summary>
        /// <returns>Bytes del XML serializado con los 
        /// datos actuales.</returns>
        public byte[] GetXml()
        {

            // Creamos el xml de envío SOAP
            Envelope = GetEnvelope();
            // Generamos el xml
            return new XmlParser().GetBytes(Envelope, Namespaces.Items);

        }

        #endregion

    }

}