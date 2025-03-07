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
    /// Información del sistema informático.
    /// </summary>
    public class DatosControl
    {

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// <para>Huella de la factura (Realizada sobre el nodo RegistroFacturacion).</para>
        /// <para>Alfanumérico(64).</para>
        /// </summary>
        public string Huella { get; set; }

        /// <summary>
        /// <para>Tipo de hash aplicado para obtener la huella.</para>
        /// <para>Alfanumérico(2) L12.</para>
        /// <para>'01': SHA-256.</para>
        /// </summary>
        public TipoHuella TipoHuella { get; set; }

        /// <summary>
        /// <para>Identificador que especifica si la generación del registro de
        /// facturación se ha realizado durante algún tipo de incidencia
        /// (por ej. no hay electricidad o fallo del sistema informático de facturación).
        /// Si no se informa este campo se entenderá que tiene valor  “N”.</para>
        /// <para>Alfanumérico(1) L11.</para>
        /// <para>'S': Sí.</para>
        /// <para>'N': No.</para>
        /// </summary>
        public string Incidencia { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {
            return $"{TipoHuella} ({Huella})";
        }

        #endregion

    }

}