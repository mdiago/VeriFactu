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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using VeriFactu.Config;

namespace VeriFactu.Net
{

    /// <summary>
    /// Esta clase gestiona las oparaciones con los servicios web de la AEAT para el VeriFactu.
    /// </summary>
    public static class Wsd
    {

        #region Métodos Privados Estáticos

        /// <summary>
        /// Obtiene el certificado configurado y verifica la validez.
        /// </summary>
        /// <returns>Certificado validado.</returns>
        /// <exception cref="ArgumentNullException">Se lanza si no se encuentra el certificado configurado.</exception>
        /// <exception cref="InvalidOperationException">Se lanza si el certificado ha expirado (fecha NotAfter en el pasado).</exception>
        internal static X509Certificate2 GetCheckedCertificate()
        {

            X509Certificate2 certificate = GetCertificate();

            if (certificate == null)
                throw new ArgumentNullException(
                    "Certificate is null. Maybe serial number in configuration was wrong.");

            CheckCertificate(certificate);

            return certificate;
          

        }

        /// <summary>
        /// Verifica la validez del certificado.
        /// </summary>
        /// <param name="certificate">Verifica la validez del certificado.</param>
        /// <exception cref="InvalidOperationException">Se lanza si el certificado ha expirado (fecha NotAfter en el pasado).</exception>
        internal static void CheckCertificate(X509Certificate2 certificate) 
        {

            if (certificate.NotAfter < DateTime.Now)
                throw new InvalidOperationException(
                  $"Certificate is out of date. NotAfter: {certificate.NotAfter}.");

        }

        /// <summary>
        /// Llama al web service de la AEAT para el VeriFactu seleccionado.
        /// </summary>
        /// <param name="url">Url destino.</param>
        /// <param name="action">Acción a ejecutar.</param>
        /// <param name="xmlDocument">Documento soap xml.</param>
        /// <param name="certificate">Certificado para la petición.</param>
        /// <returns>Devuelve la respuesta.</returns>
        internal static string Call(string url, string action, XmlDocument xmlDocument, X509Certificate2 certificate = null)
        {

            if (HttpClients.ContainsKey(url))
                return GetResponse(HttpClients[url], url, action, xmlDocument);

            HttpWebRequest webRequest = CreateWebRequest(url, action);

            X509Certificate2 cert = certificate??GetCheckedCertificate();

            webRequest.ClientCertificates.Add(cert);

            using (Stream stream = webRequest.GetRequestStream())
            {
                xmlDocument.Save(stream);
            }

            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

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
        /// Devuelve la respuesta de una petición POST de envío de un
        /// documento xml a una url determinada realizada con la
        /// instancia de HttpClient facilitada como parámetro.
        /// </summary>
        /// <param name="httpClient">Instancia de HttpClient para
        /// realizar la petición.</param>
        /// <param name="url">Url a la que enviar la petición.</param>
        /// <param name="action">Acción de servicio web.</param>
        /// <param name="xmlDocument">Documento xml.</param>
        /// <returns>Texto de la respuesta.</returns>
        private static string GetResponse(HttpClient httpClient, string url, string action, XmlDocument xmlDocument) 
        {

            var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Version = new Version(1, 1)
            };

            req.Headers.ExpectContinue = false; // evita 100-continue
            req.Content = new StringContent(xmlDocument.OuterXml, Encoding.UTF8, "text/xml");

            var resp = httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false).GetAwaiter().GetResult();
            resp.EnsureSuccessStatusCode();
            return resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();

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

        #endregion

        #region Propiedades Públicas Estáticas

        /// <summary>
        /// Certificado a utilizar en la comunicaciones con la AEAT.
        /// Si su valor no está establecido, se intentará cargar el certificado
        /// con los valores establecidos en la configuración de VeriFactu.
        /// </summary>
        public static X509Certificate2 Certificate { get; set; }

        /// <summary>
        /// Cliente preinstanciados para las peticiones a distintas urls.
        /// </summary>
        public static Dictionary<string, HttpClient> HttpClients = new Dictionary<string, HttpClient>();

        #endregion

        #region Métodos Públicos Estáticos

        /// <summary>
        /// Cliente para llamar al web service de la AEAT para el VeriFactu seleccionado.
        /// </summary>
        /// <param name="url">Url destino.</param>
        /// <param name="action">Acción a ejecutar.</param>
        /// <param name="certificate">Certificado para la petición.</param>
        /// <returns></returns>
        public static HttpClient GetHttpClient(string url, string action, X509Certificate2 certificate = null) 
        {

            X509Certificate2 cert = certificate ?? GetCheckedCertificate();
            HttpClient httpClient = null;

#if !LE_461 && !LE_472 && !LE_480

            var socketsHttpHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(10),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                PreAuthenticate = true,
                AllowAutoRedirect = false,
                UseProxy = false, // si no usas proxy
                Expect100ContinueTimeout = TimeSpan.Zero,
                MaxConnectionsPerServer = 10,
            };

            socketsHttpHandler.SslOptions = new System.Net.Security.SslClientAuthenticationOptions
            {
                ClientCertificates = new X509CertificateCollection { cert }
            };

            AppContext.SetSwitch("System.Net.Http.CheckCertificateRevocationList", false);
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", false);
            ServicePointManager.Expect100Continue = false;

            httpClient = new HttpClient(socketsHttpHandler) 
            {
                Timeout = TimeSpan.FromSeconds(100),
                DefaultRequestVersion = new Version(1, 1)
            };

#elif !LE_461

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ClientCertificates.Add(cert);
            httpClient = new HttpClient(httpClientHandler);

#else

            httpClient = new HttpClient();

#endif

            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Add("SOAPAction", action);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

            return httpClient;

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

            if(Certificate != null) // 1. Valor establecido.
                return Certificate;

            var cert = GetCertificateByFile(); // 2. Valor en Settings por fichero.

            if (cert != null)
                return cert;

            cert = GetCertificateByThumbprint(); // 3. Valor en Settings por huella digital (Almacén Certificados Windows).

            if (cert != null)
                return cert;

            return GetCertificateBySerial(); // 4. Valor en Settings por número de serie (Almacén Certificados Windows).

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

            foreach (var store in new X509Store[] { new X509Store(), new X509Store(StoreLocation.LocalMachine) }) 
            {

                store.Open(OpenFlags.ReadOnly);

                foreach (X509Certificate2 cert in store.Certificates)
                    if (cert.SerialNumber.Equals(Settings.Current.CertificateSerial, StringComparison.OrdinalIgnoreCase))
                        return cert;

            }

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

            foreach (var store in new X509Store[] { new X509Store(), new X509Store(StoreLocation.LocalMachine) })
            {

                store.Open(OpenFlags.ReadOnly);

                foreach (X509Certificate2 cert in store.Certificates)
                    if (cert.Thumbprint.Equals(Settings.Current.CertificateThumbprint, StringComparison.OrdinalIgnoreCase))
                        return cert;

            }         

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
                        Settings.Current.CertificatePassword,
#if LE_461 || LE_472 || LE_480
                        X509KeyStorageFlags.MachineKeySet |
                        X509KeyStorageFlags.PersistKeySet |
#endif
                        X509KeyStorageFlags.Exportable);

            return null;

        }

        #endregion
    
    }

}