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
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using VeriFactu.Config;

namespace VeriFactu.Xml
{
    /// <summary>
    /// Serializador xml.
    /// </summary>
    public class XmlParser
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        public XmlParser()
        {

            Encoding = Encoding.GetEncoding("UTF-8");

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Codificación de texto a utilizar. UTF8 por defecto.
        /// </summary>
        public Encoding Encoding { get; set; }

        #endregion

        #region Métodos Públicos Estáticos

        /// <summary>
        /// Devuelve una fecha formateada para un campo FechaHoraHuso
        /// de la especificación de VeriFactu.
        /// </summary>
        /// <param name="date">fecha a formatear</param>
        /// <returns>Fecha formateada  (ej: 2024-01-01T19:20:30+01:00) (ISO 8601).</returns>
        public static string GetXmlDateTimeIso8601(DateTime? date)
        {

            var input = date ?? new DateTime(1, 1, 1);
            var local = new DateTime(input.Year, input.Month, input.Day,
                input.Hour, input.Minute, input.Second, DateTimeKind.Local);

            return local.ToString("yyyy-MM-ddTHH:mm:ssK");

        }

        /// <summary>
        /// Devuelve una fecha formateada para un campo de fecha
        /// de la especificación de VeriFactu.
        /// </summary>
        /// <param name="date">fecha a formatear</param>
        /// <returns>Fecha formateada.</returns>
        public static string GetXmlDate(DateTime? date)
        {
            return (date ?? new DateTime(1, 1, 1)).ToString("dd-MM-yyyy");
        }

        /// <summary>
        /// Devuelve un importe formateado para un campo decimal
        /// de la especificación de VeriFactu.
        /// </summary>
        /// <param name="amount">Impote a formatear</param>
        /// <returns>Importe formateado.</returns>
        public static string GetXmlDecimal(decimal amount)
        {
            return amount.ToString(Settings.DefaultNumberFormatInfo);
        }

        /// <summary>
        /// Devuelve un decimal a partir de un string
        /// de un campo de xml que representa un valor decimal.
        /// </summary>
        /// <param name="amount">Impote a convertir</param>
        /// <returns>Importe convertido.</returns>
        public static decimal ToDecimal(string amount)
        {
            return Convert.ToDecimal(amount, Settings.DefaultNumberFormatInfo);
        }

        /// <summary>
        /// Devuelve una fecha a partir de un string
        /// de un campo de xml que representa un valor decimal.
        /// </summary>
        /// <param name="date">Fecha a convertir</param>
        /// <returns>Importe convertido.</returns>
        public static DateTime ToDate(string date)
        {
            return Convert.ToDateTime(date);
        }


        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Serializa el objeto como xml y lo devuelve
        /// como archivo xml en una cadena.
        /// </summary>
        /// <param name="instance">Instancia de objeto a serializar.</param>
        /// <param name="namespaces">Espacios de nombres.</param> 
        /// <param name="indent">Indica si se debe utilizar indentación.</param>
        /// <param name="omitXmlDeclaration">Indica si se se omite la delcaración xml.</param>
        /// <returns>string con el archivo xml.</returns>
        public string GetString(object instance, Dictionary<string, string> namespaces, bool indent = false, bool omitXmlDeclaration = true)
        {

            return Encoding.GetString(GetBytes(instance, namespaces, indent, omitXmlDeclaration));

        }

        /// <summary>
        /// Serializa el objeto como xml y lo devuelve
        /// como archivo xml canonicalizado en una cadena.
        /// </summary>
        /// <param name="instance">Instancia de objeto a serializar.</param>
        /// <param name="namespaces">Espacios de nombres.</param> 
        /// <param name="indent">Indica si se debe utilizar indentación.</param>
        /// <param name="omitXmlDeclaration">Indica si se se omite la delcaración xml.</param>
        /// <returns>string con el archivo xml.</returns>
        public string GetCanonicalString(object instance, Dictionary<string, string> namespaces, bool indent = false, bool omitXmlDeclaration = true)
        {

            var xmlContent = Encoding.GetString(GetBytes(instance, namespaces, indent, omitXmlDeclaration));

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xmlContent);

            XmlDsigC14NTransform xmlTransform = new XmlDsigC14NTransform();
            xmlTransform.LoadInput(xmlDoc);
            MemoryStream ms = (MemoryStream)xmlTransform.GetOutput(typeof(MemoryStream));

            return Encoding.GetString(ms.ToArray());

        }

        /// <summary>
        /// Serializa el objeto como xml y lo devuelve
        /// como archivo xml como cadena de bytes.
        /// </summary>
        /// <param name="instance">Instancia de objeto a serializar.</param>
        /// <param name="namespaces">Espacios de nombres.</param> 
        /// <param name="indent">Indica si se debe utilizar indentación.</param>
        /// <param name="omitXmlDeclaration">Indica si se se omite la delcaración xml.</param>
        /// <returns>string con el archivo xml.</returns>
        public byte[] GetBytes(object instance, Dictionary<string, string> namespaces,
            bool indent = false, bool omitXmlDeclaration = true)
        {

            XmlSerializer serializer = new XmlSerializer(instance.GetType());

            var xmlSerializerNamespaces = new XmlSerializerNamespaces();

            foreach (KeyValuePair<string, string> ns in namespaces)
                xmlSerializerNamespaces.Add(ns.Key, ns.Value);

            var ms = new MemoryStream();
            byte[] result = null;

            var settings = new XmlWriterSettings
            {
                Indent = indent,
                IndentChars = "",
                Encoding = Encoding,
                OmitXmlDeclaration = omitXmlDeclaration
            };

            using (var writer = new StreamWriter(ms))
            {
                using (var xmlWriter = XmlWriter.Create(writer, settings))
                {

                    serializer.Serialize(xmlWriter, instance, xmlSerializerNamespaces);
                    result = ms.ToArray();

                }
            }

            return result;

        }

        /// <summary>
        /// Obtiene una intancia de un tipo determinado
        /// a partir de un string con un xml válido para 
        /// la representación del tipo.
        /// </summary>
        /// <typeparam name="T">Tipo a deserializar.</typeparam>
        /// <param name="xml">XNL de una instancia del tipo.</param>
        /// <returns>Objeto del tipo obtenido del texto XML.</returns>
        public T GetInstance<T>(string xml)
        {

            T result = default(T);

            XmlSerializer serializer =
                new XmlSerializer(typeof(T));

            var instance = new XmlSerializer(typeof(T));

            using (TextReader reader = new StringReader(xml))
                result = (T)serializer.Deserialize(reader);

            return result;

        }

        #endregion

    }

}