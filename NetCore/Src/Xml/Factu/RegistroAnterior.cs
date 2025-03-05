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

namespace VeriFactu.Xml.Factu
{

    /// <summary>
    /// Datos registro anterior. Artículo 7 de la Orden HAC/1177/2024 de 17 de octubre.
    /// <para>Datos del registro anterior: </para>
    /// NIF + NumSerieFactura + FechaExpedicionFactura + HuellaAnterior.Substring(0, 64)
    /// </summary>
    public class RegistroAnterior
    {

        #region Propiedades Públicas de Instancia   

        /// <summary>
        /// <para>NIF del obligado a expedir la factura a que se refiere el
        /// registro de facturación anterior (sea de alta o de anulación)
        /// generado en este sistema informático. Este campo forma parte del
        /// detalle de las circunstancias de generación de los registros de
        /// facturación, ya que es necesario para completar la identificación
        /// de la factura contenida en el registro de facturación anterior
        /// a encadenar en casos excepcionales y puntuales en los que no
        /// coincida con el actual, como al cambiar en un momento dado el
        /// NIF tras fusiones, absorciones, etc.</para>
        /// <para>FormatoNIF(9).</para>
        /// </summary>
        public string IDEmisorFactura { get; set; }

        /// <summary>
        /// <para>Nº Serie+Nº Factura que identifica a la factura a que
        /// se refiere el registro de facturación anterior
        /// (sea de alta o de anulación) generado en este sistema
        /// informático.</para>
        /// <para>Alfanumérico(60).</para>
        /// </summary>
        public string NumSerieFactura { get; set; }

        /// <summary>
        /// <para>Fecha de expedición de la factura a que se refiere
        /// el registro de facturación anterior (sea de alta o de anulación)
        /// generado en este sistema informático.</para>
        /// <para>Fecha(dd-mm-yyyy).</para>
        /// </summary>
        public string FechaExpedicionFactura { get; set; }

        /// <summary>
        /// <para>Primeros 64 caracteres de la huella o «hash» del registro
        /// de facturación anterior (sea de alta o de anulación) generado
        /// en este sistema informático.</para>
        /// <para>Alfanumérico(64).</para>
        /// </summary>
        public string Huella { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {
            return $"{NumSerieFactura} ({FechaExpedicionFactura})";
        }

        #endregion

    }

}
