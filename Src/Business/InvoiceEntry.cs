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

        #region Variables Privadas Estáticas

        /// <summary>
        /// Bloqueo para thread safe.
        /// </summary>
        private static readonly object _Locker = new object();

        #endregion

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

        /// <summary>
        /// Indica si la entrada ya ha sido guardada.
        /// </summary>
        internal bool IsSaved { get; set; }

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

            if (string.IsNullOrEmpty(Invoice.SellerName))
                errors.Add($"Es necesario que la propiedad Invoice.SellerName tenga un valor.");

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
            // Generamos el xml
            Xml = GetXml();            

        }

        /// <summary>
        /// Devuelve el nombre completo de un archivo xml de respuesta
        /// de la AEAT cuyo envío ha resultado erróneo.
        /// </summary>
        /// <returns>Nombre completo de un archivo xml de respuesta
        /// de la AEAT que ha resultado erróneo.</returns>
        private string GetErrorResponseFilePath()
        {

            return $"{ResponsesPath}{InvoiceEntryID}.ERR.{DateTime.Now:yyyy.MM.dd.HH.mm.ss.ffff}.xml";
        
        }

        /// <summary>
        /// Path de la factura en el directorio de archivado de los datos de la
        /// cadena si el documento a resultado erróneo.
        /// </summary>
        /// <returns>Path de la factura en el directorio de archivado de los datos de la
        /// cadena si el documento a resultado erróneo.</returns>
        private string GeErrorInvoiceEntryFilePath() 
        {

            return $"{InvoiceEntryPath}{InvoiceEntryID}.ERR.{DateTime.Now:yyyy.MM.dd.HH.mm.ss.ffff}.xml";

        }

        /// <summary>
        /// Envía el registro a la AEAT.
        /// </summary>
        /// <returns>Devuelve las respuesta de la AEAT.</returns>
        private string Send()
        {

            XmlDocument xmlDocument = new XmlDocument();

            using (var msXml = new MemoryStream(Xml)) 
                xmlDocument.Load(msXml);

            var url = Settings.Current.VeriFactuEndPointPrefix;
            var action = $"{url}{Action}";

            return Wsd.Call(url, action, xmlDocument);

        }

        /// <summary>
        /// Deshace cambios de guardado de documente eliminando
        /// el elemento de la cadena de bloques y marcando los
        /// archivos relacionados como erróneos.
        /// </summary>
        private void Undo() 
        {

            //Reevierto cambios
            BlockchainManager.Delete(Registro);

            if (File.Exists(InvoiceEntryFilePath)) 
            {

                File.Copy(InvoiceEntryFilePath, GeErrorInvoiceEntryFilePath());
                File.Delete(InvoiceEntryFilePath);

            }

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

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Contabiliza y envía a la AEAT el registro.
        /// </summary>
        public void Save()
        {

            if (IsSaved)
                throw new InvalidOperationException("El objeto InvoiceEntry sólo" +
                    " puede llamar al método Save() una vez.");

            IsSaved = true;
            Exception postException = null;
            Exception sendException = null;
            Exception responseException = null;
            Exception undoException = null;

            lock (_Locker)
            {

                try
                {

                    Post();

                }
                catch (Exception ex)
                {

                    postException = ex;

                }                

                try
                {
                    if(postException == null)
                        Response = Send();

                }
                catch (Exception ex)
                {

                    sendException = ex;

                }

                if (postException == null && sendException == null)
                {

                    try 
                    {

                        ResponseEnvelope = Envelope.FromXml(Response);

                        var invoiceEntryFilePath = string.IsNullOrEmpty(CSV) ? GeErrorInvoiceEntryFilePath() : InvoiceEntryFilePath;
                        var responseFilePath = string.IsNullOrEmpty(CSV) ? GetErrorResponseFilePath() : ResponseFilePath;

                        // Almaceno xml envíado
                        File.WriteAllBytes(invoiceEntryFilePath, Xml);
                        // Almaceno xml de respuesta correcta
                        File.WriteAllText(responseFilePath, Response);

                    }
                    catch (Exception ex) 
                    {

                        responseException = ex;

                    }


                }

                try 
                {

                    if (string.IsNullOrEmpty(CSV) || sendException != null)
                        Undo();

                }
                catch (Exception ex) 
                {

                    undoException = ex;

                }

            }

            if (postException != null)
                throw new Exception($"Se ha producido un error al intentar contabilizar" +
                    $" el envío en la cadena de bloques.", postException);

            if (sendException != null)
                throw new Exception($"Se ha producido un error al intentar realizar el envío.", sendException);

            if (responseException != null)
                throw new Exception($"Se ha producido un error al intentar procesar la respuesta el envío.", responseException);

            if (undoException != null)
                throw new Exception($"Se ha producido un error al deshacer los cambios en la cadena de bloques.", undoException);

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

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {
            return $"{Invoice.SellerID}-{Invoice.InvoiceID}-{Invoice.InvoiceDate:dd/MM/yyyy}";
        }

        #endregion

    }

}
