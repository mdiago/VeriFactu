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
using System.IO;
using System.Text;
using System.Xml;
using VeriFactu.Config;
using VeriFactu.Net;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Factu.Fault;
using VeriFactu.Xml.Factu.Respuesta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business
{

    /// <summary>
    /// Representa una entrada de factura en el sistema.
    /// </summary>
    public class InvoiceEntry
    {

        #region Propiedades Privadas de Instacia

        /// <summary>
        /// Acción para el webservice.
        /// </summary>
        internal virtual string Action => "?op=RegFactuSistemaFacturacion";

        /// <summary>
        /// Sobre SOAP de respuesta de la AEAT.
        /// </summary>
        internal Envelope ResponseEnvelope{ get;set; }

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
        /// <param name="invoice">Instancia de factura de entrada en el sistema.</param>
        public InvoiceEntry(Invoice invoice)
        {

            var errors = GetArgErrors(invoice);

            if (errors.Count > 0)
                throw new ArgumentException(string.Join("\n", errors));

            Invoice = invoice;
            OutboxPath = GetOutBoxPath(Invoice.SellerID);
            InboxPath = GetInBoxPath(Invoice.SellerID);
            InvoiceEntryPath = GetInvoiceEntryPath($"{Invoice.InvoiceDate.Year}");
            ResponsesPath = GetResponsesPath($"{Invoice.InvoiceDate.Year}");

            errors = GetBusErrors();

            if (errors.Count > 0)
                throw new InvalidOperationException(string.Join("\n", errors));

            SetRegistro();

            BlockchainManager = Blockchain.Blockchain.GetInstance(Invoice.SellerID);

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Devuelve una lista con los errores de la
        /// factura como argumento de entrada.
        /// </summary>
        /// <param name="invoice">Instancia de la clase Invlice a verificar.</param>
        /// <returns>Lista con los errores encontrados.</returns>
        internal virtual List<string> GetArgErrors(Invoice invoice)
        {

            var errors = new List<string>();

            if (string.IsNullOrEmpty(invoice.InvoiceID))
                errors.Add($"El objeto Invoice no puede tener como valor de" +
                    $" su propiedad InvoiceID un valor nulo o una cadena vacía.");

            if (string.IsNullOrEmpty(invoice.SellerID))
                errors.Add($"El objeto Invoice no puede tener como valor de" +
                    $" su propiedad SellerID un valor nulo o una cadena vacía.");

            if (invoice.InvoiceDate.Year < 2024)
                errors.Add($"El objeto Invoice no puede tener como valor de" +
                    $" su propiedad InvoiceDate una fecha de años anteriores al 2024.");

            return errors;

        }

        /// <summary>
        /// Devuelve una lista con los errores de la
        /// factura por el incumplimiento de reglas de negocio.
        /// </summary>
        /// <returns>Lista con los errores encontrados.</returns>
        internal List<string> GetBusErrors()
        {

            var errors = new List<string>();

            if (File.Exists(InvoiceEntryFilePath))
                errors.Add($"Ya existe una entrada con SellerID: {Invoice.SellerID}" +
                    $" en el año {Invoice.InvoiceDate.Year} con el número {Invoice.InvoiceID}.");

            return errors;

        }

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

            var dir = $"{Settings.Current.OutboxPath}{sellerID}";

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return $"{dir}{Path.DirectorySeparatorChar}";

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

            var dir = $"{Settings.Current.InboxPath}{sellerID}";

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return $"{dir}{Path.DirectorySeparatorChar}";

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

            var dir = $"{OutboxPath}{year}";

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return $"{dir}{Path.DirectorySeparatorChar}";

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

            var dir = $"{InboxPath}{year}";

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return $"{dir}{Path.DirectorySeparatorChar}";

        }

        /// <summary>
        /// Establece el registro relativo a la entrada
        /// a contabilizar y enviar.
        /// </summary>
        internal virtual void SetRegistro()
        {

            Registro = Invoice.GetRegistroAlta();

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
                        RegistroFactura = new List<object>()
                        {
                            Registro as RegistroAlta
                        }
                    }
                }
            };

        }

        /// <summary>
        /// Contabiliza una entrada.
        /// </summary>
        private void Post()
        {

            // Añadimos el registro de alta
            BlockchainManager.Add(Registro);
            // Creamos el xml de envío SOAP
            Envelope = GetEnvelope();
            // Generamos el xml
            Xml = new XmlParser().GetBytes(Envelope, Namespaces.Items);
            // Guardamos el archivo
            File.WriteAllBytes(InvoiceEntryFilePath, Xml);

        }

        /// <summary>
        /// Envía el registro a la AEAT.
        /// </summary>
        /// <returns>Devuelve las respuesta de la AEAT.</returns>
        private string Send()
        {

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(InvoiceEntryFilePath);

            var url = Settings.Current.VeriFactuEndPointPrefix;
            var action = $"{url}{Action}";

            return Wsd.Call(url, action, xmlDocument);

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Identificador de la factura.
        /// </summary>
        public virtual string InvoiceEntryID => BitConverter.ToString(Encoding.UTF8.GetBytes(Invoice.InvoiceID)).Replace("-", "");

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
        public string InvoiceEntryPath { get; private set; }

        /// <summary>
        /// Path del directorio de archivado de los datos de la
        /// cadena.
        /// </summary>
        public string ResponsesPath { get; private set; }

        /// <summary>
        /// Path del directorio de archivado de los datos de la
        /// cadena.
        /// </summary>
        public virtual string InvoiceEntryFilePath => $"{InvoiceEntryPath}{InvoiceEntryID}.xml";

        /// <summary>
        /// Path del directorio de archivado de los datos de la
        /// cadena.
        /// </summary>
        public virtual string ResponseFilePath => $"{ResponsesPath}{InvoiceEntryID}.xml";

        /// <summary>
        /// Objeto Invoice de la entrada.
        /// </summary>
        public Invoice Invoice { get; private set; }

        /// <summary>
        /// Registro Verifactu.
        /// </summary>
        public Registro Registro { get; protected set; }

        /// <summary>
        /// Gestor de cadena de bloques para el registro.
        /// </summary>
        public Blockchain.Blockchain BlockchainManager { get; private set; }

        /// <summary>
        /// Sobre SOAP.
        /// </summary>
        public Envelope Envelope { get; private set; }

        /// <summary>
        /// Datos binarios del archivo xml de envío.
        /// </summary>
        public byte[] Xml { get; private set; }

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

                if (RespuestaRegFactuSistemaFacturacion.RespuestaLinea != null && 
                    RespuestaRegFactuSistemaFacturacion.RespuestaLinea.Count > 0 &&
                    !string.IsNullOrEmpty(RespuestaRegFactuSistemaFacturacion.RespuestaLinea[0].CodigoErrorRegistro))
                    return RespuestaRegFactuSistemaFacturacion.RespuestaLinea[0].CodigoErrorRegistro;

                    return null;

            } 

        }

        /// <summary>
        /// Código de error.
        /// </summary>
        public string ErrorDesciption
        {

            get
            {

                if (ErrorFault != null)
                    return ErrorFault.faultstring;

                if (RespuestaRegFactuSistemaFacturacion.RespuestaLinea != null &&
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

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Contabiliza y envía a la AEAT el registro.
        /// </summary>
        public void Save()
        {

            Post();
            Response = Send();

            File.WriteAllText(ResponseFilePath, Response);

            ResponseEnvelope = new Envelope(ResponseFilePath);

        }

        #endregion

    }

}
