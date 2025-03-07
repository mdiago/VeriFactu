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

using System.Xml.Serialization;

namespace VeriFactu.Xml.Factu.Consulta.Respuesta
{

    /// <summary>
    /// Cabecera de respuesta.
    /// </summary>
    [XmlType(AnonymousType = true, Namespace = Namespaces.NamespaceTikLRRC)]
    public partial class Cabecera
    {

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
        [XmlElement("IDVersion", Namespace = Namespaces.NamespaceSF)]
        public string IDVersion { get; set; }

        /// <summary>
        /// Obligado tributario emisión.
        /// </summary>
        [XmlElement(Namespace = Namespaces.NamespaceSF)]
        public Interlocutor ObligadoEmision { get; set; }

        /// <summary>
        /// Destinatario.
        /// </summary>
        [XmlElement(Namespace = Namespaces.NamespaceSF)]
        public Interlocutor Destinatario { get; set; }

        /// <summary>
        /// <para>Flag opcional que tendrá valor S si quien realiza la cosulta es el
        /// representante/asesor del obligado tributario.Permite, a quien
        /// realiza la cosulta, obtener los registros de facturación en los que
        /// figura como representante.Este flag solo se puede cumplimentar
        /// cuando esté informado el obligado tributario en la consulta
        /// </para>
        /// <para>Alfanumérico (1) L1C </para>
        /// </summary>
        [XmlElement(Namespace = Namespaces.NamespaceSF)]
        public string IndicadorRepresentante { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representacioón textual de la instancia.
        /// </summary>
        /// <returns>Representacioón textual de la instancia.</returns>
        public override string ToString()
        {

            return $"{ObligadoEmision}, {Destinatario}, {IndicadorRepresentante}";

        }

        #endregion

    }

}