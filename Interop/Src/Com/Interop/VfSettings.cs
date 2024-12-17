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
using System.Runtime.InteropServices;

namespace Verifactu
{

    #region Interfaz COM

    /// <summary>
    /// Interfaz COM para la clase RectificationItem.
    /// </summary>
    [Guid("A6D145BE-EFFA-4B5B-8322-4BA351C510B5")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    [ComVisible(true)]
    public interface IVfSettings
    {

        /// <summary>
        /// <para>Identificación de la versión actual del esquema o
        /// estructura de información utilizada para la generación y
        /// conservación / remisión de los registros de facturación.
        /// Este campo forma parte del detalle de las circunstancias
        /// de generación de los registros de facturación.</para>
        /// <para>Alfanumérico(3) L15:</para>
        /// <para>1.0: Versión actual (1.0) del esquema utilizado </para>
        /// </summary>
        string IDVersion { get; set; }

        /// <summary>
        /// Ruta al directorio que actuará como bandeja de entrada.
        /// En este directorio se almacenarán todos los mensajes
        /// recibidos de la AEAT mediante VERI*FACTU.
        /// </summary>
        string InboxPath { get; set; }

        /// <summary>
        /// Ruta al directorio que actuará como bandeja de salida.
        /// En este directorio se almacenará una copia de cualquier
        /// envío realizado a la AEAT mediante el VERI*FACTU.
        /// </summary>
        string OutboxPath { get; set; }

        /// <summary>
        /// Ruta al directorio que actuará como almacén
        /// de las distintas cadenas de bloques por emisor.
        /// </summary>
        string BlockchainPath { get; set; }

        /// <summary>
        /// Ruta al directorio que actuará almacenamiento
        /// de las facturas emitidas por emisor.
        /// </summary>
        string InvoicePath { get; set; }

        /// <summary>
        /// Ruta al directorio que actuará almacenamiento
        /// del registro de mensajes del sistema.
        /// </summary>
        string LogPath { get; set; }

        /// <summary>
        /// Número de serie del certificado a utilizar. Mediante este número
        /// de serie se selecciona del almacén de certificados de windows
        /// el certificado con el que realizar las comunicaciones.
        /// </summary>
        string CertificateSerial { get; set; }

        /// <summary>
        /// Hash o Huella digital del certificado a utilizar. Mediante esta
        /// huella digital se selecciona del almacén de certificados de
        /// windows el certificado con el que realizar las comunicaciones.
        /// </summary>
        string CertificateThumbprint { get; set; }

        /// <summary>
        /// Ruta al archivo del certificado a utilizar.
        /// Sólo se utiliza en los certificados cargados desde el sistema de archivos. 
        /// </summary>
        string CertificatePath { get; set; }

        /// <summary>
        /// Password del certificado. Este valor sólo es necesario si
        /// tenemos establecido el valor para 'CertificatePath' y el certificado
        /// tiene clave de acceso. Sólo se utiliza en los certificados
        /// cargados desde el sistema de archivos.
        /// </summary>
        string CertificatePassword { get; set; }

        /// <summary>
        /// EndPoint del web service de la AEAT para envío registros alta y anulación.
        /// </summary>
        string VeriFactuEndPointPrefix { get; set; }

        /// <summary>
        /// EndPoint del web service de la AEAT de validación de Verifactu.
        /// </summary>
        string VeriFactuEndPointValidatePrefix { get; set; }

        /// <summary>
        /// Algoritmo a utilizar para el cálculo de hash.
        /// Clave que identifica Tipo de hash aplicado para
        /// obtener la huella. Alfanumérico(2) L12.
        /// </summary>
        string VeriFactuHashAlgorithm { get; set; }

        /// <summary>
        /// Codificación del texto de entrada para el hash.
        /// </summary>
        string VeriFactuHashInputEncoding { get; set; }

        /// <summary>
        /// <para>Nombre dado por el productor al sistema
        /// informático de facturación utilizado.</para>
        /// <para>Alfanumérico (30).</para>
        /// </summary>
        string SistemaInformaticoNombre { get; set; }

        /// <summary>
        /// <para>Código ID del sistema informático de facturación utilizado.</para>
        /// <para>Alfanumérico(2).</para>
        /// </summary>
        string SistemaId { get; set; }

        /// <summary>
        /// <para>Identificación de la Versión del sistema de facturación utilizado.</para>
        /// <para>Alfanumérico(50).</para>
        /// </summary>
        string SistemaInformaticoVersion { get; set; }

        /// <summary>
        /// <para>Número de instalación del sistema informático de facturación (SIF) utilizado.
        /// Deberá distinguirlo de otros posibles SIF utilizados para realizar la facturación del
        /// obligado a expedir facturas, es decir, de otras posibles instalaciones de SIF pasadas,
        /// presentes o futuras utilizadas para realizar la facturación del obligado a expedir
        /// facturas, incluso aunque en dichas instalaciones se emplee el mismo SIF de un productor.</para>
        /// <para>Alfanumérico(100).</para>
        /// </summary>
        string SistemaInformaticoNumeroInstalacion { get; set; }

        /// <summary>
        /// <para>Especifica si para cumplir el Reglamento el sistema informático
        /// de facturación solo puede funcionar exclusivamente como Veri*Factu.</para>
        /// <para>Alfanumérico (1) L4:</para>
        /// <para>S: Sí</para>
        /// <para>N: No</para>
        /// </summary>
        string SistemaInformaticoTipoUsoPosibleSoloVerifactu { get; set; }

        /// <summary>
        /// <para> Especifica si el sistema informático de facturación permite llevar independientemente
        /// la facturación de varios obligados tributarios (valor "S") o solo de uno (valor "N").
        /// Obligatorio en registros de facturación de alta y de anulación, y opcional en registros
        /// de evento.</para>
        /// <para>Alfanumérico (1) L4:</para>
        /// <para>S: Sí</para>
        /// <para>N: No</para>
        /// </summary>
        string SistemaInformaticoTipoUsoPosibleMultiOT { get; set; }

        /// <summary>
        /// <para> Indicador de que el sistema informático, en el momento de la generación de este registro,
        /// está soportando la facturación de más de un obligado tributario. Este valor deberá obtenerlo automáticamente
        /// el sistema informático a partir del número de obligados tributarios contenidos y/o gestionados en él en ese
        /// momento, independientemente de su estado operativo (alta, baja...), no pudiendo obtenerse a partir de otra
        /// información ni ser introducido directamente por el usuario del sistema informático ni cambiado por él.
        /// El valor "N" significará que el sistema informático solo contiene y/o gestiona un único obligado tributario
        /// (de alta o de baja o en cualquier otro estado), que se corresponderá con el obligado a expedir factura de
        /// este registro de facturación. En cualquier otro caso, se deberá informar este campo con el valor "S".
        /// Obligatorio en registros de facturación de alta y de anulación, y opcional en registros de evento.</para>
        /// <para>Alfanumérico (1) L4:</para>
        /// <para>S: Sí</para>
        /// <para>N: No</para>
        /// </summary>
        string SistemaInformaticoIndicadorMultiplesOT { get; set; }

        /// <summary>
        /// Indica si salta la validación en línea de NIF con la AEAT.
        /// </summary>
        bool SkipNifAeatValidation { get; set; }

        /// <summary>
        /// Indica si salta la validación en línea de en el
        /// censo VIES de los VAT numbers intracomunitarios.
        /// </summary>
        bool SkipViesVatNumberValidation { get; set; }

        /// <summary>
        /// Indica si está activado el log de mensajes
        /// del sistema.
        /// </summary>
        bool LoggingEnabled { get; set; }

        /// <summary>
        /// Guarda la configuración.
        /// </summary>
        void Save();

        /// <summary>
        /// Carga la configuración.
        /// </summary>
        void Load();

        /// <summary>
        /// Esteblece el archivo de configuración con el cual trabajar.
        /// </summary>
        /// <param name="fileName">Nombre del archivo de configuración a utilizar.</param>
        void SetConfigFileName(string fileName);

    }

    #endregion

    #region Clase COM

    /// <summary>
    /// Representa una línea de impuestos.
    /// </summary>
    [Guid("F04D6533-AAC5-453B-A19D-BB76711ED404")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    [ProgId("Verifactu.VfSettings")]
    public class VfSettings : IVfSettings
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor. Para COM necesitamos un constructor
        /// sin parametros.
        /// </summary>
        public VfSettings()
        {
        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Devuelve una instancia de clase Settings creada a partir
        /// de los datos de esta instancia.
        /// </summary>
        /// <returns>Instancia de clase Settings.</returns>
        private VeriFactu.Config.Settings GetSettings()
        {

            var result = new VeriFactu.Config.Settings()
            {
                IDVersion = IDVersion,
                InboxPath = InboxPath,
                OutboxPath = OutboxPath,
                BlockchainPath = BlockchainPath,
                InvoicePath = InvoicePath,
                LogPath = LogPath,
                CertificateSerial = CertificateSerial,
                CertificateThumbprint = CertificateThumbprint,
                CertificatePath = CertificatePath,
                CertificatePassword = CertificatePassword,
                VeriFactuEndPointPrefix = VeriFactuEndPointPrefix,
                VeriFactuEndPointValidatePrefix = VeriFactuEndPointValidatePrefix,
                VeriFactuHashInputEncoding = VeriFactuHashInputEncoding,
                SistemaInformatico = new VeriFactu.Xml.Factu.SistemaInformatico() 
                {
                    NombreSistemaInformatico = SistemaInformaticoNombre,
                    IdSistemaInformatico = SistemaId,
                    Version = SistemaInformaticoVersion,
                    NumeroInstalacion = SistemaInformaticoNumeroInstalacion,
                    TipoUsoPosibleSoloVerifactu = SistemaInformaticoTipoUsoPosibleSoloVerifactu,
                    TipoUsoPosibleMultiOT = SistemaInformaticoTipoUsoPosibleMultiOT,
                    IndicadorMultiplesOT = SistemaInformaticoIndicadorMultiplesOT

                },
                SkipNifAeatValidation = SkipNifAeatValidation,
                SkipViesVatNumberValidation = SkipViesVatNumberValidation,
                LoggingEnabled = LoggingEnabled
            };

            var veriFactuHashAlgorithm = string.IsNullOrEmpty(VeriFactuHashAlgorithm) ? "01" : VeriFactuHashAlgorithm;

            if (!Enum.TryParse(veriFactuHashAlgorithm, out VeriFactu.Xml.Factu.TipoHuella tipoHuella))
                throw new ArgumentException($"El valor de InvoiceType '{veriFactuHashAlgorithm}' no es válido." +
                    $" Consulte en las especificaciones de la AEAT la lista: Clave del tipo de huella (L12)");

            result.VeriFactuHashAlgorithm = tipoHuella;

            return result;

        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Guarda la configuración.
        /// </summary>
        public void Save()
        {

            VeriFactu.Config.Settings.Current = GetSettings();
            VeriFactu.Config.Settings.Save();

        }

        /// <summary>
        /// Carga la configuración.
        /// </summary>
        public void Load()
        {

            var settings = VeriFactu.Config.Settings.Current;

            IDVersion = settings.IDVersion;
                InboxPath = settings.InboxPath;
            OutboxPath = settings.OutboxPath;
            BlockchainPath = settings.BlockchainPath;
            InvoicePath = settings.InvoicePath;
            LogPath = settings.LogPath;
            CertificateSerial = settings.CertificateSerial;
            CertificateThumbprint = settings.CertificateThumbprint;
            CertificatePath = settings.CertificatePath;
            CertificatePassword = settings.CertificatePassword;
            VeriFactuEndPointPrefix = settings.VeriFactuEndPointPrefix;
            VeriFactuEndPointValidatePrefix = settings.VeriFactuEndPointValidatePrefix;
            VeriFactuHashAlgorithm = $"{settings.VeriFactuHashAlgorithm}";
            VeriFactuHashInputEncoding = settings.VeriFactuHashInputEncoding;
            SistemaInformaticoNombre = settings.SistemaInformatico.NombreSistemaInformatico;
            SistemaInformaticoNombre = settings.SistemaInformatico.NombreSistemaInformatico;
            SistemaId = settings.SistemaInformatico.IdSistemaInformatico;
            SistemaInformaticoVersion = settings.SistemaInformatico.Version;
            SistemaInformaticoNumeroInstalacion = settings.SistemaInformatico.NumeroInstalacion;
            SistemaInformaticoTipoUsoPosibleSoloVerifactu = settings.SistemaInformatico.TipoUsoPosibleSoloVerifactu;
            SistemaInformaticoTipoUsoPosibleMultiOT = settings.SistemaInformatico.TipoUsoPosibleMultiOT;
            SistemaInformaticoIndicadorMultiplesOT = settings.SistemaInformatico.IndicadorMultiplesOT;
            SkipNifAeatValidation = settings.SkipNifAeatValidation;
            SkipViesVatNumberValidation = settings.SkipViesVatNumberValidation;
            LoggingEnabled = settings.LoggingEnabled;

        }

        /// <summary>
        /// Esteblece el archivo de configuración con el cual trabajar.
        /// </summary>
        /// <param name="fileName">Nombre del archivo de configuración a utilizar.</param>
        public void SetConfigFileName(string fileName)
        {

            VeriFactu.Config.Settings.SetConfigFileName(fileName);

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
        public string IDVersion { get; set; }

        /// <summary>
        /// Ruta al directorio que actuará como bandeja de entrada.
        /// En este directorio se almacenarán todos los mensajes
        /// recibidos de la AEAT mediante VERI*FACTU.
        /// </summary>
        public string InboxPath { get; set; }

        /// <summary>
        /// Ruta al directorio que actuará como bandeja de salida.
        /// En este directorio se almacenará una copia de cualquier
        /// envío realizado a la AEAT mediante el VERI*FACTU.
        /// </summary>
        public string OutboxPath { get; set; }

        /// <summary>
        /// Ruta al directorio que actuará como almacén
        /// de las distintas cadenas de bloques por emisor.
        /// </summary>
        public string BlockchainPath { get; set; }

        /// <summary>
        /// Ruta al directorio que actuará almacenamiento
        /// de las facturas emitidas por emisor.
        /// </summary>
        public string InvoicePath { get; set; }

        /// <summary>
        /// Ruta al directorio que actuará almacenamiento
        /// del registro de mensajes del sistema.
        /// </summary>
        public string LogPath { get; set; }

        /// <summary>
        /// Número de serie del certificado a utilizar. Mediante este número
        /// de serie se selecciona del almacén de certificados de windows
        /// el certificado con el que realizar las comunicaciones.
        /// </summary>
        public string CertificateSerial { get; set; }

        /// <summary>
        /// Hash o Huella digital del certificado a utilizar. Mediante esta
        /// huella digital se selecciona del almacén de certificados de
        /// windows el certificado con el que realizar las comunicaciones.
        /// </summary>
        public string CertificateThumbprint { get; set; }

        /// <summary>
        /// Ruta al archivo del certificado a utilizar.
        /// Sólo se utiliza en los certificados cargados desde el sistema de archivos. 
        /// </summary>
        public string CertificatePath { get; set; }

        /// <summary>
        /// Password del certificado. Este valor sólo es necesario si
        /// tenemos establecido el valor para 'CertificatePath' y el certificado
        /// tiene clave de acceso. Sólo se utiliza en los certificados
        /// cargados desde el sistema de archivos.
        /// </summary>
        public string CertificatePassword { get; set; }

        /// <summary>
        /// EndPoint del web service de la AEAT para envío registros alta y anulación.
        /// </summary>
        public string VeriFactuEndPointPrefix { get; set; }

        /// <summary>
        /// EndPoint del web service de la AEAT de validación de Verifactu.
        /// </summary>
        public string VeriFactuEndPointValidatePrefix { get; set; }

        /// <summary>
        /// Algoritmo a utilizar para el cálculo de hash.
        /// Clave que identifica Tipo de hash aplicado para
        /// obtener la huella. Alfanumérico(2) L12.
        /// </summary>
        public string VeriFactuHashAlgorithm { get; set; }

        /// <summary>
        /// Codificación del texto de entrada para el hash.
        /// </summary>
        public string VeriFactuHashInputEncoding { get; set; }

        /// <summary>
        /// <para>Nombre dado por el productor al sistema
        /// informático de facturación utilizado.</para>
        /// <para>Alfanumérico (30).</para>
        /// </summary>
        public string SistemaInformaticoNombre { get; set; }

        /// <summary>
        /// <para>Código ID del sistema informático de facturación utilizado.</para>
        /// <para>Alfanumérico(2).</para>
        /// </summary>
        public string SistemaId { get; set; }

        /// <summary>
        /// <para>Identificación de la Versión del sistema de facturación utilizado.</para>
        /// <para>Alfanumérico(50).</para>
        /// </summary>
        public string SistemaInformaticoVersion { get; set; }

        /// <summary>
        /// <para>Número de instalación del sistema informático de facturación (SIF) utilizado.
        /// Deberá distinguirlo de otros posibles SIF utilizados para realizar la facturación del
        /// obligado a expedir facturas, es decir, de otras posibles instalaciones de SIF pasadas,
        /// presentes o futuras utilizadas para realizar la facturación del obligado a expedir
        /// facturas, incluso aunque en dichas instalaciones se emplee el mismo SIF de un productor.</para>
        /// <para>Alfanumérico(100).</para>
        /// </summary>
        public string SistemaInformaticoNumeroInstalacion { get; set; }

        /// <summary>
        /// <para>Especifica si para cumplir el Reglamento el sistema informático
        /// de facturación solo puede funcionar exclusivamente como Veri*Factu.</para>
        /// <para>Alfanumérico (1) L4:</para>
        /// <para>S: Sí</para>
        /// <para>N: No</para>
        /// </summary>
        public string SistemaInformaticoTipoUsoPosibleSoloVerifactu { get; set; }

        /// <summary>
        /// <para> Especifica si el sistema informático de facturación permite llevar independientemente
        /// la facturación de varios obligados tributarios (valor "S") o solo de uno (valor "N").
        /// Obligatorio en registros de facturación de alta y de anulación, y opcional en registros
        /// de evento.</para>
        /// <para>Alfanumérico (1) L4:</para>
        /// <para>S: Sí</para>
        /// <para>N: No</para>
        /// </summary>
        public string SistemaInformaticoTipoUsoPosibleMultiOT { get; set; }

        /// <summary>
        /// <para> Indicador de que el sistema informático, en el momento de la generación de este registro,
        /// está soportando la facturación de más de un obligado tributario. Este valor deberá obtenerlo automáticamente
        /// el sistema informático a partir del número de obligados tributarios contenidos y/o gestionados en él en ese
        /// momento, independientemente de su estado operativo (alta, baja...), no pudiendo obtenerse a partir de otra
        /// información ni ser introducido directamente por el usuario del sistema informático ni cambiado por él.
        /// El valor "N" significará que el sistema informático solo contiene y/o gestiona un único obligado tributario
        /// (de alta o de baja o en cualquier otro estado), que se corresponderá con el obligado a expedir factura de
        /// este registro de facturación. En cualquier otro caso, se deberá informar este campo con el valor "S".
        /// Obligatorio en registros de facturación de alta y de anulación, y opcional en registros de evento.</para>
        /// <para>Alfanumérico (1) L4:</para>
        /// <para>S: Sí</para>
        /// <para>N: No</para>
        /// </summary>
        public string SistemaInformaticoIndicadorMultiplesOT { get; set; }

        /// <summary>
        /// Indica si salta la validación en línea de NIF con la AEAT.
        /// </summary>
        public bool SkipNifAeatValidation { get; set; }

        /// <summary>
        /// Indica si salta la validación en línea de en el
        /// censo VIES de los VAT numbers intracomunitarios.
        /// </summary>
        public bool SkipViesVatNumberValidation { get; set; }

        /// <summary>
        /// Indica si está activado el log de mensajes
        /// del sistema.
        /// </summary>
        public bool LoggingEnabled { get; set; }

        #endregion

    }

    #endregion

}