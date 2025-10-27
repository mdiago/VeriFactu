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
using VeriFactu.Business.Validation.NIF.TaxId;
using VeriFactu.Common;
using VeriFactu.Config;
using VeriFactu.Net.Rest.Json;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Factu.Anulacion;

namespace VeriFactu.Business
{

    /// <summary>
    /// Plain data object con los datos de una factura.
    /// </summary>
    public class InvoiceData : JsonSerializable
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        public InvoiceData() 
        { 
        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// <para>Clave del tipo de factura (L2).</para>
        /// </summary>
        public TipoFactura InvoiceType { get; set; }

        /// <summary>
        ///  Identifica si el tipo de factura rectificativa
        ///  es por sustitución o por diferencia (L3).
        /// </summary>
        public TipoRectificativa RectificationType { get; set; }

        /// <summary>
        /// Identificador de la factura.
        /// </summary>
        public string InvoiceID { get; set; }

        /// <summary>
        /// Fecha emisión de documento.
        /// </summary>        
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// Fecha operación.
        /// </summary>        
        public DateTime? OperationDate { get; set; }

        /// <summary>
        /// Identificador del vendedor.
        /// Debe utilizarse el identificador fiscal si existe (NIF, VAT Number...).
        /// En caso de no existir, se puede utilizar el número DUNS 
        /// o cualquier otro identificador acordado.
        /// </summary>        
        public string SellerID { get; set; }

        /// <summary>
        /// Nombre del vendedor.
        /// </summary>        
        [Json(Name = "CompanyName")]
        public string SellerName { get; set; }

        /// <summary>
        /// Identidicador del comprador.
        /// Debe utilizarse el identificador fiscal si existe (NIF, VAT Number...).
        /// En caso de no existir, se puede utilizar el número DUNS 
        /// o cualquier otro identificador acordado.
        /// </summary>        
        [Json(Name = "RelatedPartyID")]
        public string BuyerID { get; set; }

        /// <summary>
        /// Nombre del comprador.
        /// </summary>        
        [Json(Name = "RelatedPartyName")]
        public string BuyerName { get; set; }

        /// <summary>
        /// Código del país del destinatario (a veces también denominado contraparte,
        /// es decir, el cliente) de la operación de la factura expedida.
        /// <para>Alfanumérico (2) (ISO 3166-1 alpha-2 codes) </para>
        /// </summary>        
        [Json(Name = "CountryID")]
        public string BuyerCountryID { get; set; }

        /// <summary>
        /// Clave para establecer el tipo de identificación
        /// en el pais de residencia. L7.
        /// </summary>        
        [Json(ExcludeOnDefault = true, Name = "RelatedPartyIDType")]
        public IDType BuyerIDType { get; set; }

        /// <summary>
        /// Importe total: Total neto + impuestos soportado
        /// - impuestos retenidos.
        /// </summary>        
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Total impuestos soportados.
        /// </summary>        
        public decimal TotalTaxOutput { get; set; }

        /// <summary>
        /// Total impuestos soportados.
        /// </summary>        
        public decimal TotalTaxOutputSurcharge { get; set; }

        /// <summary>
        /// Importe total impuestos retenidos.
        /// </summary>        
        public decimal TotalTaxWithheld { get; set; }

        /// <summary>
        /// Texto del documento.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Líneas de impuestos.
        /// </summary>
        public List<TaxItem> TaxItems { get; set; }

        /// <summary>
        /// Esta colección almacena la información de las facturas modificados
        /// por la presenta factura. Estas modificaciones pueden provenir de
        /// dos situaciónes distintas:
        /// <para>1. La factura es del tipo F3 y se trata de la conversión de una
        /// factura simplificada a factura normal.</para>
        /// <para>2. Se trata de una factura rectificativa: R1, R2, R3, R4, R5.</para>
        /// Según la situación la información irá en el RegistroAlta en el
        /// bloque de FacturasRectificadas o en el de FacturasSustituidas.
        /// </summary>
        public List<RectificationItem> RectificationItems { get; set; }

        /// <summary>
        /// BaseRectificada para rectificativas por sustitución 'S'.
        /// </summary>        
        public decimal RectificationTaxBase { get; set; }

        /// <summary>
        /// CuotaRectificada para rectificativas por sustitución 'S'.
        /// </summary>        
        public decimal RectificationTaxAmount { get; set; }

        /// <summary>
        /// CuotaRecargoRectificado para rectificativas por sustitución 'S'.
        /// </summary>
        public decimal RectificationTaxAmountSurcharge { get; set; }

        #endregion     

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {
            
            return $"{SellerID}-{InvoiceID}-{XmlParser.GetXmlDate(InvoiceDate)}";

        }

        #endregion

    }

}
