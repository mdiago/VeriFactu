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
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using VeriFactu.Config;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Factu.Anulacion;
using VeriFactu.Xml.Factu.Evento;

namespace VeriFactu.Common
{

    /// <summary>
    /// Algunas utilidades generales.
    /// </summary>
    public static class Utils
    {

        #region Variables Privadas Estáticas

        /// <summary>
        /// Fábricas de algoritmos de digest disponibles. Se crea una instancia
        /// nueva por cálculo: HashAlgorithm mantiene estado interno y compartir
        /// una única instancia entre hilos corrompe los hashes calculados en
        /// paralelo (la huella de la cadena de bloques).
        /// </summary>
        static readonly Dictionary<TipoHuella, Func<HashAlgorithm>> _HashAlgorithms = new Dictionary<TipoHuella, Func<HashAlgorithm>>()
        {

            {TipoHuella.Sha256, () => SHA256.Create() }

        };

        /// <summary>
        /// Codificaciones de texto a binario disponibles.
        /// </summary>
        static readonly Dictionary<string, Encoding> _Encodings = new Dictionary<string, Encoding>()
        {

            {"UTF-8", Encoding.UTF8 }

        };

        /// <summary>
        /// Encoding del texto de entrada para el hash de hash.
        /// </summary>
        internal static Encoding Encoding { get; private set; }

        /// <summary>
        /// Gestor de log.
        /// </summary>
        public static Logger Logger { get; private set; }

        #endregion

        #region Construtores Estáticos

        /// <summary>
        /// Constructor estático clase.
        /// </summary>
        static Utils()
        {

            if (!_HashAlgorithms.ContainsKey(Settings.Current.VeriFactuHashAlgorithm))
                throw new ArgumentException($"El valor de la variable de configuración 'VeriFactuHashAlgorithm'" +
                    $" no puede ser '{Settings.Current.VeriFactuHashAlgorithm}'.");

            if (!_Encodings.ContainsKey(Settings.Current.VeriFactuHashInputEncoding))
                throw new ArgumentException($"El valor de la variable de configuración 'VeriFactuHashInputEncoding'" +
                    $" no puede ser '{Settings.Current.VeriFactuHashInputEncoding}'.");

            Encoding = _Encodings[Settings.Current.VeriFactuHashInputEncoding];

            Logger = new Logger();

        }

        #endregion

        #region Métodos Internos Estáticos

        /// <summary>
        /// Obtiene el valor XML asociado a un valor enumerado.
        /// </summary>
        /// <param name="value">Valor enumerado.</param>
        /// <returns>
        /// Valor definido mediante <see cref="XmlEnumAttribute"/>.
        /// </returns>
        internal static string GetXmlEnumValue(Enum value)
        {

            if (value == null)
                return null;

            var field = value.GetType().GetField(value.ToString());

            if (field == null)
                return null;

            var attribute = field.GetCustomAttribute<XmlEnumAttribute>();

            return attribute?.Name;

        }

        /// <summary>
        /// Obtiene un valor enumerado a partir de su valor XML.
        /// </summary>
        /// <param name="enumType">Tipo enumerado.</param>
        /// <param name="value">Valor XML.</param>
        /// <returns>Valor enumerado correspondiente.</returns>
        internal static object GetEnumFromXmlValue(Type enumType, string value)
        {

            foreach (var field in enumType.GetFields())
            {

                var attribute = field.GetCustomAttribute<XmlEnumAttribute>();

                if (attribute?.Name == value)
                    return field.GetValue(null);

            }

            throw new ArgumentException(
                $"No existe un valor XML '{value}' para el enumerado {enumType.Name}.");

        }

        /// <summary>
        /// Calcula el hash de la entrada con el algoritmo configurado,
        /// usando una instancia nueva por llamada (seguro entre hilos).
        /// </summary>
        /// <param name="input">Bytes de entrada.</param>
        /// <returns>Hash calculado.</returns>
        internal static byte[] ComputeHash(byte[] input)
        {

            using (var hashAlgorithm = _HashAlgorithms[Settings.Current.VeriFactuHashAlgorithm]())
                return hashAlgorithm.ComputeHash(input);

        }

        /// <summary>
        /// Deserializa un registro de facturación a partir de un elemento XML.
        /// </summary>
        /// <typeparam name="T"> Tipo a deserializar.</typeparam>
        /// <param name="element"> Elemento xml.</param>
        /// <returns> Objeto deserializado.</returns>
        internal static T DeserializeRegistro<T>(XmlElement element)
        {

            var serializer = new XmlSerializer(typeof(T));

            using (var reader = new XmlNodeReader(element))
                return (T)serializer.Deserialize(reader);

        }


        /// <summary>
        /// Obtiene el registro de facturación de alta o anulación
        /// almacenado en un archivo XML del modo VERI*FACTU (como sobre SOAP entero) o
        /// no VERI*FACTU como registro firmado.
        /// </summary>
        /// <param name="filePath"> Ruta del archivo XML.</param>
        /// <returns> Registro de facturación almacenado.</returns>
        internal static Registro GetRecord(string filePath)
        {

            var document = new XmlDocument();

            document.PreserveWhitespace = true;
            document.Load(filePath);

            if (document.DocumentElement == null)
                throw new InvalidOperationException($"No se ha encontrado un documento XML válido en '{filePath}'.");

            Registro record;

            switch (document.DocumentElement.LocalName)
            {

                case "Envelope":

                    // Formato VERI*FACTU: el registro está contenido
                    // dentro del sobre SOAP.
                    var envelope = new Xml.Soap.Envelope(filePath);

                    var registroFacturacion = envelope.Body?.Registro as RegFactuSistemaFacturacion;

                    if (registroFacturacion == null ||
                        registroFacturacion.RegistroFactura == null ||
                        registroFacturacion.RegistroFactura.Count != 1)
                        throw new InvalidOperationException(
                            $"No se ha encontrado un único registro de facturación en '{filePath}'.");

                    record = registroFacturacion.RegistroFactura[0].Registro as Registro;

                    break;

                case "RegistroAlta":

                    // Formato NO VERI*FACTU.
                    record = DeserializeRegistro<RegistroAlta>(document.DocumentElement);

                    break;

                case "RegistroAnulacion":

                    // Formato NO VERI*FACTU.
                    record = DeserializeRegistro<RegistroAnulacion>(document.DocumentElement);

                    break;

                default:

                    throw new InvalidOperationException($"Formato de registro de facturación no reconocido en '{filePath}'.");

            }

            if (record == null)
                throw new InvalidOperationException($"No se ha encontrado un registro de facturación válido" +
                    $" en '{filePath}'.");

            // Restituimos los valores de las propiedades auxiliares utilizadas
            // para mantener el orden de serialización XML.
            record.Huella =
                (record as RegistroAlta)?.OrderedHuella ??
                (record as RegistroAnulacion)?.OrderedHuella;

            record.FechaHoraHusoGenRegistro =
                (record as RegistroAlta)?.OrderedFechaHoraHusoGenRegistro ??
                (record as RegistroAnulacion)?.OrderedFechaHoraHusoGenRegistro;

            record.Encadenamiento =
                (record as RegistroAlta)?.OrderedEncadenamiento ??
                (record as RegistroAnulacion)?.OrderedEncadenamiento;

            record.IDFactura =
                (record as RegistroAlta)?.IDFacturaAlta ??
                (record as RegistroAnulacion)?.IDFacturaAnulada;

            return record;

        }

        /// <summary>
        /// Obtiene el registro de evento almacenado en un archivo XML.
        /// </summary>
        /// <param name="filePath"> Ruta del archivo XML.</param>
        /// <returns> Registro de evento almacenado.</returns>
        internal static Evento GetEventRecord(string filePath)
        {

            var serializer =
                new XmlSerializer(typeof(RegistroEvento));

            using (var stream = File.OpenRead(filePath))
            {
                var registro =
                    (RegistroEvento)serializer.Deserialize(stream);

                var evento = registro.Evento;

                evento.EventChainLinkID =
                    Convert.ToUInt64(
                        Path.GetFileNameWithoutExtension(filePath));

                return evento;

            }

        }

        #endregion

        #region Métodos Privados Estáticos

        /// <summary>
        /// Codifica un texto de entrada en una cadena de bytes
        /// utilizando UTF8, y luego devuelve la cadena de bytes
        /// en hexadecimal.
        /// </summary>
        /// <param name="text">Texto a codificar.</param>
        /// <returns>Texto que contiene la codificación hexadecimal.</returns>
        internal static string GetEncodedToHex(string text)
        {

            return BitConverter.ToString(Utils.Encoding.GetBytes(text)).Replace("-", "");

        }

        /// <summary>
        /// Devuelve una cadena que ha sido anteriormente pasada a 
        /// binario con el Encoding establecido en la configración
        /// y posteriomente se ha convertido en un texto hesadecimal.
        /// </summary>
        /// <param name="text">Texto codificado a decodificar.</param>
        /// <returns>Texto decodificado.</returns>
        internal static string GetFromEncodedToHex(string text)
        {

            if (string.IsNullOrEmpty(text) || text.Length % 2 != 0)
                throw new ArgumentException($"La cadena de entrada no es válida.");

            var buff = new List<byte>();

            for (int b = 0; b < text.Length; b = b + 2)
                buff.Add(Convert.ToByte($"{text[b]}{text[b + 1]}", 16));

            return Utils.Encoding.GetString(buff.ToArray());

        }

        /// <summary>
        /// Almacena un mensaje en el log.
        /// </summary>
        /// <param name="msg">Mensaje.</param>
        internal static void Log(string msg)
        {

            if (Settings.Current.LoggingEnabled)
                Logger.Log(msg);

        }

        #endregion

    }

}