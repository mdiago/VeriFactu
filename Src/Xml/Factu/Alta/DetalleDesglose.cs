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

namespace VeriFactu.Xml.Factu.Alta
{

    /// <summary>
    /// Línea de desglose de la factura.
    /// </summary>
    public class DetalleDesglose
    {

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// <para>Clave que identificaráel tipo de régimen
        /// del IVA o una operación con trascendencia tributaria.</para>
        /// <para>Alfanumérico(2). L8.</para>
        /// </summary>
        public ClaveRegimen ClaveRegimen { get; set; }

        /// <summary>
        /// <para>Clave de la operación sujeta y
        /// no exenta o de la operación no sujeta.</para>
        /// <para>Alfanumérico(2). L9.</para>
        /// </summary>
        public CalificacionOperacion CalificacionOperacion { get; set; }


        /// <summary>
        /// <para>Campo que especifica la causa de exención.</para>
        /// <para>Alfanumérico(2). L10.</para>
        /// </summary>
        public CausaExencion OperacionExenta { get; set; }

        /// <summary>
        /// <para>Porcentaje aplicado sobre la base
        /// imponible para calcular la cuota.</para>
        /// <para>Decimal(3,2).</para>
        /// </summary>
        public string TipoImpositivo { get; set; }

        /// <summary>
        /// <para>Magnitud dineraria sobre la que se
        /// aplica el tipo impositivo / Importe no sujeto.</para>
        /// <para>Decimal(12,2).</para>
        /// </summary>
        public string BaseImponibleOimporteNoSujeto { get; set; }

        /// <summary>
        /// <para>Magnitud dineraria sobre  la que se aplica
        /// el tipo impositivo en régimen especial de grupos nivel avanzado.</para>
        /// <para>Decimal(12,2).</para>
        /// </summary>
        public string BaseImponibleACoste { get; set; }

        /// <summary>
        /// <para>Cuota resultante de aplicar a la base
        /// imponible el tipo impositivo.</para>
        /// <para>Decimal(12,2).</para>
        /// </summary>
        public string CuotaRepercutida { get; set; }

        /// <summary>
        /// <para>Pocentaje asociado en función
        /// del tipo de IVA .</para>
        /// <para>Decimal(3,2).</para>
        /// </summary>
        public string TipoRecargoEquivalencia { get; set; }

        /// <summary>
        /// <para>Cuota resultante de aplicar a la base
        /// imponible el tipo de recargo de equivalencia.</para>
        /// <para>Decimal(12,2).</para>
        /// </summary>
        public string CuotaRecargoEquivalencia { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representacioón textual de la instancia.
        /// </summary>
        /// <returns>Representacioón textual de la instancia.</returns>
        public override string ToString()
        {

            return $"[{ClaveRegimen}, {CalificacionOperacion}]" +
                $" {BaseImponibleOimporteNoSujeto} x {TipoImpositivo}" +
                $" = {CuotaRepercutida}";

        }

        #endregion

    }

}
