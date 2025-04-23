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
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Xml.Serialization;
using VeriFactu.Common;
using VeriFactu.DataStore;
using VeriFactu.Net.Rest;
using VeriFactu.Xml.Factu;

namespace VeriFactu.Config
{

    /// <summary>
    /// Configuración.
    /// </summary>
    [Serializable]
    [XmlRoot("Settings")]
    public class Settings
    {

        #region Variables Privadas Estáticas

        /// <summary>
        /// Path separator win="\" and linux ="/".
        /// </summary>
        static readonly char _PathSep = System.IO.Path.DirectorySeparatorChar;

        /// <summary>
        /// Configuración actual.
        /// </summary>
        static Settings _Current;

        /// <summary>
        /// Ruta al directorio de configuración.
        /// </summary>
        static string _Path = Environment.GetFolderPath(
            Environment.SpecialFolder.CommonApplicationData) + $"{_PathSep}VeriFactu{_PathSep}";

        /// <summary>
        /// Ruta al directorio de la cadena de bloques.
        /// </summary>
        string _BlockchainPath;

        #endregion

        #region Propiedades Privadas Estáticas

        /// <summary>
        /// Formato de importes para los xml del sii.
        /// </summary>
        internal static NumberFormatInfo DefaultNumberFormatInfo = new NumberFormatInfo();

        /// <summary>
        /// Seprador decimal.
        /// </summary>
        internal static string DefaultNumberDecimalSeparator = ".";

        /// <summary>
        /// Nombre del fichero de configuración.
        /// </summary>
        internal static string FileName = "Settings.xml";

        /// <summary>
        /// Indicador de si el sistema de cadena de bloques está
        /// inicializado.
        /// </summary>
        internal static bool BlockchainInitialized;

        #endregion

        #region Construtores Estáticos

        /// <summary>
        /// Constructor estático de la clase Settings.
        /// </summary>
        static Settings()
        {

            DefaultNumberFormatInfo.NumberDecimalSeparator =
                DefaultNumberDecimalSeparator;

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            Get();

            BlockchainInitialized = Blockchain.Blockchain.Initialized; // Inicia cadena de bloques
            Current.SistemaInformatico.IndicadorMultiplesOT = Seller.GetSellers().Count > 1 ? "S" : "N"; // Valor multiples OT

            ApiClient.Ct(); 

        }

        #endregion

        #region Métodos Privados Estáticos

        /// <summary>
        /// Inicia estaticos.
        /// </summary>
        /// <returns>La configuración cargada.</returns>
        internal static Settings Get()
        {

            _Current = new Settings();

            string FullPath = $"{Path}{_PathSep}" + FileName;

            XmlSerializer serializer = new XmlSerializer(_Current.GetType());
            
            if (File.Exists(FullPath))
            {

                using (StreamReader r = new StreamReader(FullPath))
                    _Current = serializer.Deserialize(r) as Settings;

            }
            else
            {

                _Current= GetDefault();

            }

            CheckDirectories();

            return _Current;

        }

        /// <summary>
        /// Devuelve MAC address.
        /// </summary>
        /// <returns> MAC local</returns>
        internal static string GetLocalMacAddress()
        {

            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface nic in networkInterfaces)
                if (nic.OperationalStatus == OperationalStatus.Up)
                    return $"{nic.GetPhysicalAddress()}";

            return null;

        }

        /// <summary>
        /// Devuelve un objeto Settings con las opciones
        /// por defecto de configuración.
        /// </summary>
        /// <returns></returns>
        internal static Settings GetDefault() 
        {

            var numeroInstalacion = "01";

            try 
            {

                var mac = GetLocalMacAddress();

                if (!string.IsNullOrEmpty(mac))
                    numeroInstalacion = mac;

            }
            catch (Exception ex) 
            {

                Utils.Log($"{ex}");

            }

            return new Settings()
            {
                IDVersion = "1.0",
                InboxPath = $"{Path}Inbox{_PathSep}",
                OutboxPath = $"{Path}Outbox{_PathSep}",
                BlockchainPath = $"{Path}Blockchains{_PathSep}",
                InvoicePath = $"{Path}Invoices{_PathSep}",
                LogPath = $"{Path}Log{_PathSep}",
                CertificateSerial = "",
                CertificateThumbprint = "",
                CertificatePath = "",
                CertificatePassword = "",
                VeriFactuEndPointPrefix = VeriFactuEndPointPrefixes.Test,
                VeriFactuEndPointValidatePrefix = VeriFactuEndPointPrefixes.TestValidate,
                VeriFactuHashAlgorithm = TipoHuella.Sha256,
                VeriFactuHashInputEncoding = "UTF-8",
                SistemaInformatico = new SistemaInformatico() 
                { 
                    NIF = "B12959755",
                    NombreRazon = "IRENE SOLUTIONS SL",
                    NombreSistemaInformatico = $"{Assembly.GetExecutingAssembly().GetName().Name}",
                    IdSistemaInformatico = "01",
                    Version = $"{Assembly.GetExecutingAssembly().GetName().Version}",
                    NumeroInstalacion = numeroInstalacion,
                    TipoUsoPosibleSoloVerifactu = "S",
                    TipoUsoPosibleMultiOT = "S",
                    IndicadorMultiplesOT = "S"
                },
                Api = new Api() 
                {
                    EndPointCreate = "https://facturae.irenesolutions.com:8050/Kivu/Taxes/Verifactu/Invoices/Create",
                    EndPointCancel = "https://facturae.irenesolutions.com:8050/Kivu/Taxes/Verifactu/Invoices/Cancel",
                    EndPointGetQrCode = "https://facturae.irenesolutions.com:8050/Kivu/Taxes/Verifactu/Invoices/GetQrCode",
                    EndPointGetSellers = "https://facturae.irenesolutions.com:8050/Kivu/Taxes/Verifactu/Invoices/GetSellers",
                    EndPointGetRecords = "https://facturae.irenesolutions.com:8050/Kivu/Taxes/Verifactu/Invoices/GetFilteredList",
                    EndPointValidateNIF = "https://facturae.irenesolutions.com:8050/Kivu/Taxes/Verifactu/Invoices/ValidateNIF",
                    EndPointGetAeatInvoices = "https://facturae.irenesolutions.com:8050/Kivu/Taxes/Verifactu/Invoices/GetAeatInvoices",
                    ServiceKey = "1234"
                },
                SkipNifAeatValidation = true,
                SkipViesVatNumberValidation = true,
                LoggingEnabled = false
            };

        }

        #endregion

        #region Propiedades Públicas Estáticas

        /// <summary>
        /// Configuración en curso.
        /// </summary>
        public static Settings Current
        {
            get
            {
                return _Current;
            }
            set
            {
                _Current = value;
            }
        }

        /// <summary>
        /// Ruta al directorio de configuración.
        /// </summary>
        public static string Path
        {
            get
            {

                if(string.IsNullOrEmpty(_Path))
                    return Environment.GetFolderPath(
                        Environment.SpecialFolder.CommonApplicationData) + $"{_PathSep}VeriFactu{_PathSep}";

                return _Path;

            }
            set
            {

                _Path = value;
                Get();

            }

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// <para>Identificación de la versión actual del esquema o
        /// estructura de información utilizada para la generación y
        /// conservación / remisión de los registros de facturación.
        /// Este campo forma parte del detalle de las circunstancias
        /// de generación de los registros de facturación.</para>
        /// <para>Alfanumérico(3) L15:</para>
        /// <para>1.0: Versión actual (1.0) del esquema utilizado </para>
        /// </summary>
        [XmlElement("IDVersion")]
        public string IDVersion { get; set; }

        /// <summary>
        /// Ruta al directorio que actuará como bandeja de entrada.
        /// En este directorio se almacenarán todos los mensajes
        /// recibidos de la AEAT mediante VERI*FACTU.
        /// </summary>
        [XmlElement("InboxPath")]
        public string InboxPath { get; set; }

        /// <summary>
        /// Ruta al directorio que actuará como bandeja de salida.
        /// En este directorio se almacenará una copia de cualquier
        /// envío realizado a la AEAT mediante el VERI*FACTU.
        /// </summary>
        [XmlElement("OutboxPath")]
        public string OutboxPath { get; set; }

        /// <summary>
        /// Ruta al directorio que actuará como almacén
        /// de las distintas cadenas de bloques por emisor.
        /// </summary>
        [XmlElement("BlockchainPath")]
        public string BlockchainPath 
        { 
            get 
            { 

                return _BlockchainPath; 

            } 
            set 
            { 

                if(Current.BlockchainPath != null && Current.BlockchainPath != value 
                    && Directory.GetDirectories(Current.BlockchainPath).Length > 0)
                    throw new InvalidOperationException($"No se puede cambiar el valor" +
                        $" de 'BlockchainPath' si la carpeta no está vacía.");

                _BlockchainPath = value; 

            } 
        }

        /// <summary>
        /// Ruta al directorio que actuará almacenamiento
        /// de las facturas emitidas por emisor.
        /// </summary>
        [XmlElement("InvoicePath")]
        public string InvoicePath { get; set; }

        /// <summary>
        /// Ruta al directorio que actuará almacenamiento
        /// del registro de mensajes del sistema.
        /// </summary>
        [XmlElement("LogPath")]
        public string LogPath { get; set; }

        /// <summary>
        /// Número de serie del certificado a utilizar. Mediante este número
        /// de serie se selecciona del almacén de certificados de windows
        /// el certificado con el que realizar las comunicaciones.
        /// </summary>
        [XmlElement("CertificateSerial")]
        public string CertificateSerial { get; set; }

        /// <summary>
        /// Hash o Huella digital del certificado a utilizar. Mediante esta
        /// huella digital se selecciona del almacén de certificados de
        /// windows el certificado con el que realizar las comunicaciones.
        /// </summary>
        [XmlElement("CertificateThumbprint")]
        public string CertificateThumbprint { get; set; }

        /// <summary>
        /// Ruta al archivo del certificado a utilizar.
        /// Sólo se utiliza en los certificados cargados desde el sistema de archivos. 
        /// </summary>
        [XmlElement("CertificatePath")]
        public string CertificatePath { get; set; }

        /// <summary>
        /// Password del certificado. Este valor sólo es necesario si
        /// tenemos establecido el valor para 'CertificatePath' y el certificado
        /// tiene clave de acceso. Sólo se utiliza en los certificados
        /// cargados desde el sistema de archivos.
        /// </summary>
        [XmlElement("CertificatePassword")]
        public string CertificatePassword { get; set; }

        /// <summary>
        /// EndPoint del web service de la AEAT para envío registros alta y anulación.
        /// </summary>
        [XmlElement("VeriFactuEndPointPrefix")]
        public string VeriFactuEndPointPrefix { get; set; }

        /// <summary>
        /// EndPoint del web service de la AEAT de validación de Verifactu.
        /// </summary>
        [XmlElement("VeriFactuEndPointValidatePrefix")]
        public string VeriFactuEndPointValidatePrefix { get; set; }

        /// <summary>
        /// Algoritmo a utilizar para el cálculo de hash.
        /// Clave que identifica Tipo de hash aplicado para
        /// obtener la huella. Alfanumérico(2) L12.
        /// </summary>
        [XmlElement("VeriFactuHashAlgorithm")]
        public TipoHuella VeriFactuHashAlgorithm { get; set; }

        /// <summary>
        /// Codificación del texto de entrada para el hash.
        /// </summary>
        [XmlElement("VeriFactuHashInputEncoding")]
        public string VeriFactuHashInputEncoding { get; set; }

        /// <summary>
        /// Datos del sistema informático.
        /// </summary>
        [XmlElement("SistemaInformatico")]
        public SistemaInformatico SistemaInformatico { get; set; }

        /// <summary>
        /// Datos del API REST para Verifactu de Irene Solutions.
        /// </summary>
        [XmlElement("Api")] 
        public Api Api { get; set; }

        /// <summary>
        /// Indica si salta la validación en línea de NIF con la AEAT.
        /// </summary>
        [XmlElement("SkipNifAeatValidation")]
        public bool SkipNifAeatValidation { get; set; }

        /// <summary>
        /// Indica si salta la validación en línea de en el
        /// censo VIES de los VAT numbers intracomunitarios.
        /// </summary>
        [XmlElement("SkipViesVatNumberValidation")]
        public bool SkipViesVatNumberValidation { get; set; }

        /// <summary>
        /// Indica si está activado el log de mensajes
        /// del sistema.
        /// </summary>
        [XmlElement("LoggingEnabled")]
        public bool LoggingEnabled { get; set; }

        #endregion

        #region Métodos Públicos Estáticos

        /// <summary>
        /// Guarda la configuración en curso actual.
        /// </summary>
        public static void Save()
        {

            CheckDirectories();

            string FullPath = $"{Path}{_PathSep}" + FileName;

            XmlSerializer serializer = new XmlSerializer(Current.GetType());

            using (StreamWriter w = new StreamWriter(FullPath))
            {
                serializer.Serialize(w, Current);
            }

        }

        /// <summary>
        /// Aseguro existencia de directorios de trabajo.
        /// </summary>
        private static void CheckDirectories()
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            if (!Directory.Exists(_Current.InboxPath))
                Directory.CreateDirectory(_Current.InboxPath);

            if (!Directory.Exists(_Current.OutboxPath))
                Directory.CreateDirectory(_Current.OutboxPath);

            if (!Directory.Exists(_Current.BlockchainPath))
                Directory.CreateDirectory(_Current.BlockchainPath);

            if (!Directory.Exists(_Current.InvoicePath))
                Directory.CreateDirectory(_Current.InvoicePath);

            if (!Directory.Exists(_Current.LogPath))
                Directory.CreateDirectory(_Current.LogPath);

        }

        /// <summary>
        /// Esteblece el archivo de configuración con el cual trabajar.
        /// </summary>
        /// <param name="fileName">Nombre del archivo de configuración a utilizar.</param>
        public static void SetConfigFileName(string fileName)
        {

            FileName = fileName;
            Get();

        }

        #endregion

    }

}