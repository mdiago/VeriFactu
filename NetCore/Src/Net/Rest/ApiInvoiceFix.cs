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
using VeriFactu.Business;
using VeriFactu.Net.Rest.Json;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;

namespace VeriFactu.Net.Rest
{

    /// <summary>
    /// Extiende la clase Invoice para poder incluir la propiedad 
    /// </summary>
    internal class ApiInvoiceFix : JsonSerializable
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Factura de subsanación.
        /// </summary>
        protected Invoice _Invoice;

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="invoice">Factura de subsanación</param>
        public ApiInvoiceFix(Invoice invoice) 
        {

            _Invoice = invoice;

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// <para>Clave del tipo de factura (L2).</para>
        /// </summary>
        public TipoFactura InvoiceType => _Invoice.InvoiceType;

        /// <summary>
        ///  Identifica si el tipo de factura rectificativa
        ///  es por sustitución o por diferencia (L3).
        /// </summary>
        public TipoRectificativa RectificationType => _Invoice.RectificationType;

        /// <summary>
        /// Identificador de la factura.
        /// </summary>
        public string InvoiceID => _Invoice.InvoiceID;

        /// <summary>
        /// Fecha emisión de documento.
        /// </summary>        
        public DateTime InvoiceDate => _Invoice.InvoiceDate;

        /// <summary>
        /// Fecha operación.
        /// </summary>        
        public DateTime? OperationDate => _Invoice.OperationDate;

        /// <summary>
        /// Identificador del vendedor.
        /// Debe utilizarse el identificador fiscal si existe (NIF, VAT Number...).
        /// En caso de no existir, se puede utilizar el número DUNS 
        /// o cualquier otro identificador acordado.
        /// </summary>        
        public string SellerID => _Invoice.SellerID;

        /// <summary>
        /// Nombre del vendedor.
        /// </summary>        
        [Json(Name = "CompanyName")]
        public string SellerName => _Invoice.SellerName;

        /// <summary>
        /// Identidicador del comprador.
        /// Debe utilizarse el identificador fiscal si existe (NIF, VAT Number...).
        /// En caso de no existir, se puede utilizar el número DUNS 
        /// o cualquier otro identificador acordado.
        /// </summary>        
        [Json(Name = "RelatedPartyID")]
        public string BuyerID => _Invoice.BuyerID;

        /// <summary>
        /// Nombre del comprador.
        /// </summary>        
        [Json(Name = "RelatedPartyName")]
        public string BuyerName => _Invoice.BuyerName;

        /// <summary>
        /// Código del país del destinatario (a veces también denominado contraparte,
        /// es decir, el cliente) de la operación de la factura expedida.
        /// <para>Alfanumérico (2) (ISO 3166-1 alpha-2 codes) </para>
        /// </summary>        
        public string BuyerCountryID => _Invoice.BuyerCountryID;

        /// <summary>
        /// Clave para establecer el tipo de identificación
        /// en el pais de residencia. L7.
        /// </summary>        
        [Json(ExcludeOnDefault = true)]
        public IDType BuyerIDType => _Invoice.BuyerIDType;

        /// <summary>
        /// Importe total: Total neto + impuestos soportado
        /// - impuestos retenidos.
        /// </summary>        
        public decimal TotalAmount => _Invoice.TotalAmount;

        /// <summary>
        /// Total impuestos soportados.
        /// </summary>        
        public decimal TotalTaxOutput => _Invoice.TotalTaxOutput;

        /// <summary>
        /// Total impuestos soportados.
        /// </summary>        
        public decimal TotalTaxOutputSurcharge => _Invoice.TotalTaxOutputSurcharge;

        /// <summary>
        /// Importe total impuestos retenidos.
        /// </summary>        
        public decimal TotalTaxWithheld => _Invoice.TotalTaxWithheld;

        /// <summary>
        /// Texto del documento.
        /// </summary>
        public string Text => _Invoice.Text;

        /// <summary>
        /// Líneas de impuestos.
        /// </summary>
        public List<TaxItem> TaxItems => _Invoice.TaxItems;

        /// <summary>
        /// Facturas rectificadas.
        /// </summary>
        public List<RectificationItem> RectificationItems => _Invoice.RectificationItems;

        /// <summary>
        /// RegistroAlta a partir del cual se ha creado la factura, en el
        /// caso de que la instancia se haya creado a partir de un registro
        /// de alta.
        /// </summary>
        public RegistroAlta RegistroAltaSource => _Invoice.RegistroAltaSource;

        /// <summary>
        /// Indica si se trata de una subsanación de una factura.
        /// </summary>
        public bool IsInvoiceFix => true;

        #endregion     

    }

}