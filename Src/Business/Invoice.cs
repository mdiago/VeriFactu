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
using VeriFactu.Business.TaxId;
using VeriFactu.Config;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Factu.Anulacion;

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

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="invoiceID">Identificador de la factura.</param>
        /// <param name="invoiceDate">Fecha emisión de documento.</param>
        /// <param name="sellerID">Identificador del vendedor.</param>        
        /// <exception cref="ArgumentNullException">Los argumentos invoiceID y sellerID no pueden ser nulos</exception>
        public Invoice(string invoiceID, DateTime invoiceDate, string sellerID) 
        {

            if (invoiceID == null || sellerID == null)
                throw new ArgumentNullException($"Los argumentos invoiceID y sellerID no pueden ser nulos.");

            InvoiceID = invoiceID;
            InvoiceDate = invoiceDate;
            SellerID = sellerID;

        }

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

        /// <summary>
        /// Obtiene la lista de destinatarios.
        /// </summary>
        /// <returns>Lista de destinatarios.</returns>
        private List<Interlocutor> GetDestinatarios() 
        {

            TaxIdEs taxId = null;
            var isTaxIdEs = false;

            try 
            {

                taxId = new TaxIdEs(BuyerID);
                isTaxIdEs = taxId.IsDCOK;

            } 
            catch (TaxIdEsException) // Id. fiscal no español
            {

                isTaxIdEs = false;

            } 
            catch (Exception ex) 
            {

                throw (ex);

            }

            if (isTaxIdEs && (BuyerCountryID == "ES" || string.IsNullOrEmpty(BuyerCountryID)))
                return new List<Interlocutor>()
                {
                    new Interlocutor
                    {
                        NombreRazon = BuyerName,
                        NIF = BuyerID
                    }
                };


            if(string.IsNullOrEmpty(BuyerCountryID))
                throw new Exception("Si BuyerID no es un identificador español válido" +
                    " (NIF, DNI, NIE...) es obligatorio que BuyerCountryID tenga un valor.");

            if (!Enum.TryParse<CodigoPais>(BuyerCountryID, out CodigoPais buyerCountryId))
                throw new Exception($"El código de pais consignado en BuyerCountryID='{BuyerCountryID}' no es válido.");

            return new List<Interlocutor>()
                {
                    new Interlocutor
                    {
                       IDOtro = new IDOtro()
                       { 
                            CodigoPais = buyerCountryId,
                            ID = BuyerID,
                            IDType = BuyerIDType
                       }
                    }
                };

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
        public string InvoiceID { get; private set; }

        /// <summary>
        /// Fecha emisión de documento.
        /// </summary>        
        public DateTime InvoiceDate { get; private set; }

        /// <summary>
        /// Identificador del vendedor.
        /// Debe utilizarse el identificador fiscal si existe (NIF, VAT Number...).
        /// En caso de no existir, se puede utilizar el número DUNS 
        /// o cualquier otro identificador acordado.
        /// </summary>        
        public string SellerID { get; private set; }

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
        /// Código del país del destinatario (a veces también denominado contraparte,
        /// es decir, el cliente) de la operación de la factura expedida.
        /// <para>Alfanumérico (2) (ISO 3166-1 alpha-2 codes) </para>
        /// </summary>        
        public string BuyerCountryID { get; set; }

        /// <summary>
        /// Clave para establecer el tipo de identificación
        /// en el pais de residencia. L7.
        /// </summary>        
        public IDType BuyerIDType { get; set; }

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
                IDFacturaAlta = new IDFactura()
                {
                    IDEmisorFactura = SellerID,
                    NumSerieFactura = InvoiceID,
                    FechaExpedicionFactura = XmlParser.GetXmlDate(InvoiceDate)                    
                }, 
                NombreRazonEmisor = SellerName,
                TipoFactura = InvoiceType,
                TipoFacturaSpecified = true,
                DescripcionOperacion = Text,
                Destinatarios = GetDestinatarios(),
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
        /// Obtiene el registro de alta para verifactu.
        /// </summary>
        /// <returns>Registro de alta para verifactu</returns>
        public RegistroAnulacion GetRegistroAnulacion()
        {           

            var registroAnulacion = new RegistroAnulacion()
            {
                IDVersion = Settings.Current.IDVersion,
                IDFacturaAnulada = new IDFactura()
                {
                    IDEmisorFacturaAnulada = SellerID,
                    NumSerieFacturaAnulada = InvoiceID,
                    FechaExpedicionFacturaAnulada = XmlParser.GetXmlDate(InvoiceDate)
                },
                SistemaInformatico = Settings.Current.SistemaInformatico,
                TipoHuella = TipoHuella.Sha256,
                TipoHuellaSpecified = true
            };

            return registroAnulacion;

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
