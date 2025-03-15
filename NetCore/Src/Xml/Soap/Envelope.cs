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
using System.IO;
using System.Xml.Serialization;

namespace VeriFactu.Xml.Soap
{

    /// <summary>
    /// Representacion de envelope (sobre) para SOAP. 
    /// Sobre: el cual define qué hay en el mensaje y cómo procesarlo.
    /// </summary>
    [Serializable]
    [XmlRoot("Envelope", Namespace = Namespaces.NamespaceSoap)]
    public class Envelope
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor clase Envelope.
        /// </summary>
        public Envelope()
        {
            Header = new Header();
            Body = new Body();
        }

        /// <summary>
        /// Constructor clase Envelope.
        /// </summary>
        /// <param name="xmlPath">Ruta al archivo xml que contiene el mensaje SOAP.</param>
        public Envelope(string xmlPath)
        {

            Envelope envelope = null;

            XmlSerializer serializer = new XmlSerializer(this.GetType());
            if (File.Exists(xmlPath))
            {
                using (StreamReader r = new StreamReader(xmlPath))
                {
                    envelope = serializer.Deserialize(r) as Envelope;
                }
            }

            if (envelope == null)
                throw new Exception("XML SOAP serialization error");

            Header = envelope.Header;
            Body = envelope.Body;

        }

        /// <summary>
        /// Constructor clase Envelope.
        /// </summary>
        /// <param name="stream">Ruta al archivo xml que contiene el mensaje SOAP.</param>
        public Envelope(Stream stream)
        {

            Envelope envelope = null;

            XmlSerializer serializer = new XmlSerializer(this.GetType());

            using (StreamReader r = new StreamReader(stream))
                envelope = serializer.Deserialize(r) as Envelope;

            if (envelope == null)
                throw new Exception("XML SOAP selerailization error");

            Header = envelope.Header;
            Body = envelope.Body;

        }

        /// <summary>
        /// Devuelve un objeto envelope a partir de un
        /// string procedente de una respuesta de la AEAT.
        /// </summary>
        /// <param name="xml">String xml de una instancia de tipo de VERIFACTU.</param>
        /// <returns>Nuevo objeto envelope.</returns>
        public static Envelope FromXml(string xml)
        {
            var streamResponse = new MemoryStream();
            var writer = new StreamWriter(streamResponse);
            writer.Write(xml);
            writer.Flush();
            streamResponse.Position = 0;

            return new Envelope(streamResponse);
        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// SOAP Header.
        /// </summary>
        [XmlElement(Order = 1)]
        public Header Header { get; set; }

        /// <summary>
        /// Body del envelope SOAP.
        /// </summary>
        [XmlElement(Order = 2)]
        public Body Body { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {
            return $"{Body}";
        }

        #endregion

    }

}