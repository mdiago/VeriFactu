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
    serving VeriFactu XML data on the fly in a web application, shipping VeriFactu
    with a closed source product.
    
    For more information, please contact Irene Solutions SL. at this
    address: info@irenesolutions.com
 */

using System.Xml.Serialization;

namespace VeriFactu.Xml.Factu.Consulta.Respuesta
{

    /// <summary>
    /// Respuesta de la AEAT a una consulta de datos del sistema
    /// VERI*FACTU.
    /// </summary>
    [XmlType(AnonymousType = true, Namespace = Namespaces.NamespaceTikLRRC)]
    public class RespuestaConsultaFactuSistemaFacturacion
    {

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Cabecera de la consulta.
        /// </summary>
        [XmlElement("Cabecera", Namespace = Namespaces.NamespaceTikLRRC)]
        public Cabecera Cabecera { get; set; }

        /// <summary>
        /// Periodo a filtrar.
        /// </summary>
        [XmlElement(Namespace = Namespaces.NamespaceTikLRRC)]
        public PeriodoImputacion PeriodoImputacion { get; set; }

        /// <summary>
        /// <para> Indica si hay más registros de facturación en la consulta realizada 
        /// (Ver 6.4.3 Consulta paginada). Si hay más datos pendientes, este campo 
        /// tendrá valor “S” y se podrán realizar nuevas consultas indicando la
        /// identificación del último registro a partir de la cual se devolverán
        /// los siguientes registros ordenados por fecha de presentación.</para>
        /// <para>Alfanumérico(1) Valores posibles: “S” o “N”</para>
        /// </summary>

        [XmlElement(Namespace = Namespaces.NamespaceTikLRRC)]
        public string IndicadorPaginacion { get; set; }

        /// <summary>
        /// <para> Indica si hay registros de facturación para la consulta realizada.</para>
        /// <para> Alfanumérico(8) Valores posibles: “ConDatos” o “SinDatos”</para>
        /// </summary>
        [XmlElement(Namespace = Namespaces.NamespaceTikLRRC)]
        public string ResultadoConsulta { get; set; }

        /// <summary>
        /// Bloque con los datos de la factura recuperados. Se obtendrán como
        /// máximo 10.000 veces.
        /// </summary>
        [XmlElement(Namespace = Namespaces.NamespaceTikLRRC)]
        public RegistroRespuestaConsultaFactuSistemaFacturacion[] RegistroRespuestaConsultaFactuSistemaFacturacion { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representacioón textual de la instancia.
        /// </summary>
        /// <returns>Representacioón textual de la instancia.</returns>
        public override string ToString()
        {

            return $"{Cabecera}, {ResultadoConsulta}";

        }

        #endregion

    }

}