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
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using VeriFactu.Config;
using VeriFactu.Qrcode;
using VeriFactu.Qrcode.Exceptions;
using VeriFactu.Xml.Factu;

namespace VeriFactu.Xml
{

    /// <summary>
    /// Representa un registro de Verifactu: Alta, baja, evento...
    /// </summary>
    public class Registro
    {

        #region Variables Privadas Estáticas

        /// <summary>
        /// Algoritmos de dgiest disponibles.
        /// </summary>
        static readonly Dictionary<TipoHuella, HashAlgorithm> _HashAlgorithms = new Dictionary<TipoHuella, HashAlgorithm>()
        {

            {TipoHuella.Sha256, new SHA256Managed() }

        };

        /// <summary>
        /// Algoritmos de digest disponibles.
        /// </summary>
        static readonly Dictionary<string, Encoding> _Encodings = new Dictionary<string, Encoding>()
        {

            {"UTF-8", Encoding.UTF8 }

        };

        /// <summary>
        /// Algoritmo de hash.
        /// </summary>
        protected static HashAlgorithm HashAlgorithm { get; private set; }

        /// <summary>
        /// Encoding del texto de entrada para el hash de hash.
        /// </summary>
        protected static Encoding Encoding { get; private set; }

        #endregion

        #region Construtores Estáticos

        /// <summary>
        /// Constructor estático clase.
        /// </summary>
        static Registro()
        {

            if (!_HashAlgorithms.ContainsKey(Settings.Current.VeriFactuHashAlgorithm))
                throw new ArgumentException($"El valor de la variable de configuración 'VeriFactuHashAlgorithm'" +
                    $" no puede ser '{Settings.Current.VeriFactuHashAlgorithm}'.");

            if (!_Encodings.ContainsKey(Settings.Current.VeriFactuHashInputEncoding))
                throw new ArgumentException($"El valor de la variable de configuración 'VeriFactuHashInputEncoding'" +
                    $" no puede ser '{Settings.Current.VeriFactuHashInputEncoding}'.");

            HashAlgorithm = _HashAlgorithms[Settings.Current.VeriFactuHashAlgorithm];
            Encoding = _Encodings[Settings.Current.VeriFactuHashInputEncoding];

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Devuelve la cadena de entrada para el cálculo
        /// del hash previa conversión mediante UTF-8.
        /// <para> El Artículo 137 de la Orden HAC/1177/2024 de 17 de octubre establece el contenido:</para>
        /// <para> a) Para el registro de facturación de alta.</para>
        /// <para> b) Para el registro de facturación de anulación.</para>
        /// </summary>
        /// <returns>La cadena de entrada para el cálculo
        /// del hash previa conversión mediante UTF-8.</returns>
        internal protected virtual string GetHashTextInput()
        {

            throw new NotImplementedException("La clase base record no implementa el método GetHashInput.\n" +
                "Este método debe implementarse en las clases derivadas.");

        }

        /// <summary>
        /// Devuelve la cadena de parámetros para la url
        /// del servicio de validación con los valores
        /// de los parámetro urlencoded.
        /// </summary>
        /// <returns>Cadena de parámetros para la url
        /// del servicio de validación con los valores
        /// de los parámetro urlencoded.</returns>
        protected virtual string GetValidateUrlParams()
        {

            throw new NotImplementedException("La clase base record no implementa el método GetValidateUrlParams.\n" +
                "Este método debe implementarse en las clases derivadas.");

        }

        /// <summary>
        /// Devuelve el array de bytes sobre el que calcularemos el hash.
        /// </summary>
        /// <returns></returns>
        private byte[] GetHashInput()
        {

            var textInput = GetHashTextInput();
            var binInput = Encoding.GetBytes(textInput);

            return binInput;

        }

        /// <summary>
        /// Devuelve el hash para el registro.
        /// </summary>
        /// <returns>Hash para el registro.</returns>
        private byte[] GetHash()
        {

            var binInput = GetHashInput();
            var hash = HashAlgorithm.ComputeHash(binInput);

            return hash;

        }

        /// <summary>
        /// Obtiene un bitmap con el contenido
        /// codificado en un código QR.
        /// </summary>
        /// <param name="content">Contenido a incluir en el Bitmap.</param>
        /// <returns>Bitmap con el contenido
        /// codificado en un código QR.</returns>
        private byte[] GetQr(string content) 
        {

            byte[] result = null;

            Dictionary<EncodeHintType, object> hints = new Dictionary<EncodeHintType, object>();
            hints.Add(EncodeHintType.CHARACTER_SET, "ISO-8859-1");
            hints.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.M);

            try
            {

                QRCodeWriter qc = new QRCodeWriter();
                ByteMatrix bm = qc.Encode(content, 1, 1, hints);

                QrBitmap qrBm = new QrBitmap(bm);

                result = qrBm.GetBytes();              

            }
            catch (WriterException ex)
            {
                
                throw new ArgumentException(ex.Message, ex.InnerException);

            }

            return result;

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
        [XmlElement("IDVersion", Namespace = Namespaces.NamespaceSF, Order = 1)]
        public virtual string IDVersion { get; set; }

        /// <summary>
        /// Datos de identificación de factura expedida para
        /// operaciones de baja y consulta.
        /// </summary>
        [XmlIgnore()]
        public virtual IDFactura IDFactura { get; set; }

        /// <summary>
        /// <para>Fecha, hora y huso horario de generación del registro de facturación.
        /// El huso horario es el que está usando el sistema informático de facturación
        /// en el momento de generación del registro de facturación.</para>
        /// <para>DateTime. Formato: YYYY-MM-DDThh:mm:ssTZD (ej: 2024-01-01T19:20:30+01:00) (ISO 8601)</para>
        /// </summary>
        [XmlIgnore]
        public virtual string FechaHoraHusoGenRegistro { get; set; }

        /// <summary>
        /// <para>Primeros 64 caracteres de la huella o «hash» del registro
        /// de facturación anterior (sea de alta o de anulación) generado
        /// en este sistema informático.</para>
        /// <para>Alfanumérico(64).</para>
        /// </summary>
        [XmlIgnore]
        public virtual string Huella { get; set; }

        /// <summary>
        /// Encadenamiento con la factura anterior..
        /// </summary>
        [XmlIgnore]
        public virtual Encadenamiento Encadenamiento { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Devuelve la representación del hash en formato hexadecimal.
        /// </summary>
        /// <returns>Hash en formato hexadecimal</returns>
        public string GetHashOutput()
        {

            var hash = GetHash();
            var output = BitConverter.ToString(hash);

            return output.Replace("-", "");

        }

        /// <summary>
        /// Devuelve la url para la validación del documento.
        /// </summary>
        /// <returns>Url para la validación del documento.</returns>
        public string GetUrlValidate() 
        {

            var urlParams = GetValidateUrlParams();
            return $"{Settings.Current.VeriFactuEndPointValidatePrefix}?{urlParams}";
        
        }

        /// <summary>
        /// Obtiene un bitmap con la url de validación
        /// codificada en un código QR.
        /// </summary>
        /// <returns>Bitmap con la url de validación
        /// codificada en un código QR.</returns>
        public byte[] GetValidateQr() 
        {

            var content = GetUrlValidate();
            return GetQr(content);

        }

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {
            return $"{IDFactura}";
        }

        #endregion

    }

}
