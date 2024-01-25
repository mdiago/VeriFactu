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
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Factu.Anulacion;

namespace VeriFactu.Net
{

    /// <summary>
    /// Esta clase gestiona las oparaciones con los servicios web de la AEAT para el VeriFactu.
    /// </summary>
    public class Wsd
    {

        /// <summary>
        /// Operaciones disponibles en el webservice por tipo de registro.
        /// </summary>
        static Dictionary<Type, string> _Operations = new Dictionary<Type, string>() 
        {
            { typeof(AltaFactuSistemaFacturacion),  "AltaFactuSistemaFacturacion"},
            { typeof(BajaFactuSistemaFacturacion),  "BajaFactuSistemaFacturacion"}
        };

        /// <summary>
        /// Llama a al web service de la AEAT para el VeriFactu seleccionado.
        /// </summary>
        /// <param name="url">Url destino.</param>
        /// <param name="action">Acción a ejecutar.</param>
        /// <param name="xmlDocument">Documento soap xml.</param>
        /// <returns>Devuelve la respuesta.</returns>
        protected static string Call(string url, string action, XmlDocument xmlDocument)
        {

            HttpWebRequest webRequest = CreateWebRequest(url, action);

            X509Certificate2 certificate = GetCertificate();

            if (certificate == null)
                throw new ArgumentNullException(
                    "Certificate is null. Maybe serial number in configuration was wrong.");

            if (certificate.NotAfter < DateTime.Now)
                throw new ArgumentNullException(
                  $"Certificate is out of date. NotAfter: {certificate.NotAfter}.");

            webRequest.ClientCertificates.Add(certificate);

            using (Stream stream = webRequest.GetRequestStream())
            {
                xmlDocument.Save(stream);
            }

            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

            string statusDescription = response.StatusDescription;

            Stream dataStream = response.GetResponseStream();

            string responseFromServer;

            using (StreamReader reader = new StreamReader(dataStream))
            {
                responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
            }


            return responseFromServer;

        }

        /// <summary>
        /// Devuelve el certificado configurado siguiendo la siguiente
        /// jerarquía de llamadas: En primer lugar prueba a cargar el
        /// certificado desde un archivo, si no prueba por el hash y en
        /// último lugar prueba por el número de serie.
        /// </summary>
        /// <returns>Devuelve el certificado de la 
        /// configuración para las comunicaciones.</returns>
        public static X509Certificate2 GetCertificate()
        {
            var cert = GetCertificateByFile();

            if (cert != null)
                return cert;

            cert = GetCertificateByThumbprint();

            if (cert != null)
                return cert;

            return GetCertificateBySerial();

        }

        /// <summary>
        /// Devuelve el certificado establecido en la configuración
        /// por su número de serie..
        /// </summary>
        /// <returns>Devuelve el certificado de la 
        /// configuración por número de serie para las comunicaciones.
        /// Si no existe devuelve null.</returns>
        public static X509Certificate2 GetCertificateBySerial()
        {
            X509Store store = new X509Store();
            store.Open(OpenFlags.ReadOnly);
            foreach (X509Certificate2 cert in store.Certificates)
                if (cert.SerialNumber == Settings.Current.CertificateSerial)
                    return cert;

            // Probamos en LocalMachine
            X509Store storeLM = new X509Store(StoreLocation.LocalMachine);
            storeLM.Open(OpenFlags.ReadOnly);
            foreach (X509Certificate2 cert in storeLM.Certificates)
                if (cert.SerialNumber == Settings.Current.CertificateSerial)
                    return cert;

            return null;
        }

        /// <summary>
        /// Devuelve el certificado establecido en la configuración por su
        /// hash o huella digital. 
        /// </summary>
        /// <returns>Devuelve el certificado de la 
        /// configuración por hash para las comunicaciones.
        /// Si no existe devuelve null.</returns>
        public static X509Certificate2 GetCertificateByThumbprint()
        {

            X509Store store = new X509Store();
            store.Open(OpenFlags.ReadOnly);
            foreach (X509Certificate2 cert in store.Certificates)
                if (cert.Thumbprint.ToUpper() == $"{Settings.Current?.CertificateThumbprint}".ToUpper())
                    return cert;

            // Probamos en LocalMachine
            X509Store storeLM = new X509Store(StoreLocation.LocalMachine);
            storeLM.Open(OpenFlags.ReadOnly);
            foreach (X509Certificate2 cert in storeLM.Certificates)
                if (cert.Thumbprint.ToUpper() == $"{Settings.Current?.CertificateThumbprint}".ToUpper())
                    return cert;

            return null;

        }

        /// <summary>
        /// Devuelve el certificado establecido en la configuración
        /// mediante una ruta a un fichero de certificado.
        /// </summary>
        /// <returns>Devuelve el certificado de la 
        /// configuración para las comunicaciones.</returns>
        public static X509Certificate2 GetCertificateByFile()
        {

            if (!string.IsNullOrEmpty(Settings.Current.CertificatePath) &&
                File.Exists(Settings.Current.CertificatePath))
                if (string.IsNullOrEmpty(Settings.Current.CertificatePassword))
                    return new X509Certificate2(Settings.Current.CertificatePath);
                else
                    return new X509Certificate2(Settings.Current.CertificatePath,
                        Settings.Current.CertificatePassword);

            return null;

        }

        /// <summary>
        /// Crea la instancia WebRequest para enviar la petición
        /// al web service de la AEAT.
        /// </summary>
        /// <param name="url">Url del web service.</param>
        /// <param name="action">Acción del web service.</param>
        /// <returns></returns>
        private static HttpWebRequest CreateWebRequest(string url, string action)
        {

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);

            webRequest.Headers.Add("SOAPAction", action);

            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;

        }

    }

}
