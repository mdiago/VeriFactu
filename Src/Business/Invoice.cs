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
using System.Collections.Generic;
using VeriFactu.Config;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;

namespace VeriFactu.Business
{

    /// <summary>
    /// Representa un factura en el sistema VeriFactu.
    /// </summary>
    public class Invoice
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Suma de las bases imponibles.
        /// </summary>   
        decimal _NetAmount;

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Calcula los totales de la factura.
        /// </summary>
        private void CalculateTotals() 
        {

            TotalAmount = TotalTaxOutput = _NetAmount = 0;

            if (TaxItems == null || TaxItems.Count == 0)
                return;

            foreach (var taxitem in TaxItems) 
            {

                _NetAmount += taxitem.TaxBase;
                TotalTaxOutput += taxitem.TaxAmount;

            }

            TotalAmount = _NetAmount + TotalTaxOutput - TotalTaxWithheld;

        }

        /// <summary>
        /// Obtiene el desglose de la factura.
        /// </summary>
        /// <returns>Desglose de la factura.</returns>
        private List<DetalleDesglose> GetDesglose() 
        {

            var desglose = new List<DetalleDesglose>(); 

            foreach (var taxitem in TaxItems)
            {

                desglose.Add(new DetalleDesglose() 
                {
                    ClaveRegimen = taxitem.TaxScheme,
                    CalificacionOperacion = taxitem.TaxType,
                    TipoImpositivo = XmlParser.GetXmlDecimal(taxitem.TaxRate),
                    BaseImponibleOimporteNoSujeto = XmlParser.GetXmlDecimal(taxitem.TaxBase),
                    CuotaRepercutida = XmlParser.GetXmlDecimal(taxitem.TaxAmount)
                });

            }

            return desglose;

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// <para>Clave del tipo de factura (L2).</para>
        /// </summary>
        public TipoFactura InvoiceType { get; set; }

        /// <summary>
        /// Identificador de la factura.
        /// </summary>
        public string InvoiceID { get; set; }

        /// <summary>
        /// Fecha emisión de documento.
        /// </summary>        
        public DateTime InvoiceDate { get; set; }

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
        public string SellerName { get; set; }

        /// <summary>
        /// Identidicador del comprador.
        /// Debe utilizarse el identificador fiscal si existe (NIF, VAT Number...).
        /// En caso de no existir, se puede utilizar el número DUNS 
        /// o cualquier otro identificador acordado.
        /// </summary>        
        public string BuyerID { get; set; }

        /// <summary>
        /// Nombre del comprador.
        /// </summary>        
        public string BuyerName { get; set; }

        /// <summary>
        /// Importe total: Total neto + impuestos soportado
        /// - impuestos retenidos.
        /// </summary>        
        public decimal TotalAmount { get; private set; }

        /// <summary>
        /// Total impuestos soportados.
        /// </summary>        
        public decimal TotalTaxOutput { get; private set; }

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

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Obtiene el registro de alta para verifactu.
        /// </summary>
        /// <returns>Registro de alta para verifactu</returns>
        public RegistroAlta GetRegistroAlta()
        {

            CalculateTotals();            

            var registroAlta = new RegistroAlta()
            {
                IDVersion = Settings.Current.IDVersion,
                IDFactura = new IDFactura()
                {
                    IDEmisorFactura = SellerID,
                    NumSerieFactura = InvoiceID,
                    FechaExpedicionFactura = XmlParser.GetXmlDate(InvoiceDate)                    
                }, 
                NombreRazonEmisor = SellerName,
                TipoFactura = InvoiceType,
                TipoFacturaSpecified = true,
                DescripcionOperacion = Text,
                Destinatarios = new List<Interlocutor>() 
                { 
                    new Interlocutor
                    { 
                        NombreRazon = BuyerName,
                        NIF = BuyerID
                    }
                },
                Desglose = GetDesglose(),
                CuotaTotal = XmlParser.GetXmlDecimal(TotalTaxOutput),
                ImporteTotal = XmlParser.GetXmlDecimal(TotalAmount),
                SistemaInformatico = Settings.Current.SistemaInformatico,
                TipoHuella = TipoHuella.Sha256,
                TipoHuellaSpecified = true
            };

            return registroAlta;

        }

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
