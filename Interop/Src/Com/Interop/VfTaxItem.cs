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

namespace VeriFactu.Com.Interop
{

    #region Interfaz COM

    /// <summary>
    /// Interfaz COM para la clase TaxItem.
    /// </summary>
    [Guid("12842BEE-5120-4E64-BCC1-71EB6C9333C6")]
    [ComVisible(true)]
    public interface IVfTaxItem
    {

        /// <summary>
        /// <para>Impuesto de aplicación. Si no se informa este campo
        /// se entenderá que el impuesto de aplicación es el IVA.
        /// Este campo es necesario porque contribuye a completar el
        /// detalle de la tipología de la factura.</para>
        /// <para>Alfanumérico (1) L1:</para>
        /// <para>01: Impuesto sobre el Valor Añadido (IVA)</para>
        /// <para>02: Impuesto sobre la Producción, los Servicios y la Importación (IPSI) de Ceuta y Melilla</para>
        /// <para>03: Impuesto General Indirecto Canario (IGIC)</para>
        /// <para>05: Otros</para>
        /// </summary>
        string Tax { get; set; }

        /// <summary>
        /// Esquema impositivo.
        /// </summary>        
        string TaxScheme { get; set; }

        /// <summary>
        /// Identificador la categoría de impuestos.
        /// </summary>        
        string TaxType { get; set; }

        /// <summary>
        /// <para>Campo que especifica la causa de exención.</para>
        /// <para>Alfanumérico(2). L10.</para>
        /// </summary>
        string TaxException { get; set; }

        /// <summary>
        /// Base imponible.
        /// </summary>
        decimal TaxBase { get; set; }

        /// <summary>
        /// Tipo impositivo.
        /// </summary>
        decimal TaxRate { get; set; }

        /// <summary>
        /// Importe cuota impuesto.
        /// </summary>
        decimal TaxAmount { get; set; }

        /// <summary>
        /// Tipo impositivo recargo.
        /// </summary>
        decimal TaxRateSurcharge { get; set; }

        /// <summary>
        /// Importe cuota recargo impuesto.
        /// </summary>
        decimal TaxAmountSurcharge { get; set; }

    }

    #endregion

    #region Clase COM

    /// <summary>
    /// Representa una línea de impuestos.
    /// </summary>
    [Guid("1BB49501-174C-4C20-BEFA-FE9E10507486")]
    [ComVisible(true)]
    [ProgId("VeriFactu.Com.Interop.TaxItem")]
    public class VfTaxItem : IVfTaxItem
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor. Para COM necesitamos un constructor
        /// sin parametros.
        /// </summary>
        public VfTaxItem()
        {
        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// <para>Impuesto de aplicación. Si no se informa este campo
        /// se entenderá que el impuesto de aplicación es el IVA.
        /// Este campo es necesario porque contribuye a completar el
        /// detalle de la tipología de la factura.</para>
        /// <para>Alfanumérico (1) L1:</para>
        /// <para>01: Impuesto sobre el Valor Añadido (IVA)</para>
        /// <para>02: Impuesto sobre la Producción, los Servicios y la Importación (IPSI) de Ceuta y Melilla</para>
        /// <para>03: Impuesto General Indirecto Canario (IGIC)</para>
        /// <para>05: Otros</para>
        /// </summary>
        public string Tax { get; set; }

        /// <summary>
        /// Esquema impositivo.
        /// </summary>        
        public string TaxScheme { get; set; }

        /// <summary>
        /// Identificador la categoría de impuestos.
        /// </summary>        
        public string TaxType { get; set; }

        /// <summary>
        /// <para>Campo que especifica la causa de exención.</para>
        /// <para>Alfanumérico(2). L10.</para>
        /// </summary>
        public string TaxException { get; set; }

        /// <summary>
        /// Base imponible.
        /// </summary>
        public decimal TaxBase { get; set; }

        /// <summary>
        /// Tipo impositivo.
        /// </summary>
        public decimal TaxRate { get; set; }

        /// <summary>
        /// Importe cuota impuesto.
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Tipo impositivo recargo.
        /// </summary>
        public decimal TaxRateSurcharge { get; set; }

        /// <summary>
        /// Importe cuota recargo impuesto.
        /// </summary>
        public decimal TaxAmountSurcharge { get; set; }

        #endregion

    }

    #endregion

}
