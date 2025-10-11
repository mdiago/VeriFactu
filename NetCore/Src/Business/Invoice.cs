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
    /// Representa un factura en el sistema VeriFactu.
    /// </summary>
    public class Invoice : JsonSerializable
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Suma de las bases imponibles.
        /// </summary>   
        decimal _NetAmount;

        /// <summary>
        /// Registro de alta a partir del cual se ha
        /// generado la factura
        /// </summary>
        RegistroAlta _RegistroAltaSource;

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

            InvoiceID = invoiceID.Trim(); // La AEAT calcula el Hash sin espacios
            InvoiceDate = invoiceDate;

            var tSellerID = sellerID.Trim(); // La AEAT calcula el Hash sin espacios
            SellerID = tSellerID.ToUpper(); // https://github.com/mdiago/VeriFactu/issues/65

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="registroAlta">Registro de alta a partir del que se crea la factura.</param>
        public Invoice(RegistroAlta registroAlta) : this(registroAlta.IDFacturaAlta.NumSerieFactura,
                XmlParser.ToDate(registroAlta.IDFacturaAlta.FechaExpedicionFactura), $"{registroAlta.IDFacturaAlta.IDEmisorFactura}")
        {

            _RegistroAltaSource = registroAlta;

            InvoiceType = registroAlta.TipoFactura;
            SellerName = registroAlta.NombreRazonEmisor;
            Text = registroAlta.DescripcionOperacion;

            if (registroAlta.TipoRectificativaSpecified)
                RectificationType = registroAlta.TipoRectificativa;

            if (registroAlta.Destinatarios != null && registroAlta.Destinatarios.Count > 1)
                throw new NotImplementedException("El método estático Invoice.FromRegistroAlta" +
                    " no implementa la conversión de RegistrosAlta con más de un destinatario.");

            if (registroAlta.Destinatarios != null && registroAlta.Destinatarios.Count == 1)
            {

                var destinatario = registroAlta.Destinatarios[0];
                BuyerID = $"{destinatario.NIF}{destinatario?.IDOtro?.ID}";
                BuyerName = $"{destinatario.NombreRazon}";

                if (!string.IsNullOrEmpty($"{destinatario?.IDOtro?.CodigoPais}"))
                    BuyerCountryID = $"{destinatario?.IDOtro?.CodigoPais}";

                if (!string.IsNullOrEmpty($"{destinatario?.IDOtro?.IDType}"))
                    BuyerIDType = destinatario?.IDOtro?.IDType ?? IDType.NIF_IVA;

            }

            TaxItems = FromDesglose(registroAlta.Desglose);
            CalculateTotals();

        }

        #endregion

        #region Métodos Privados Estáticos

        /// <summary>
        /// Devuelve una lista de TaxItem a partir
        /// de una lista de objetos DetalleDesglose.
        /// </summary>
        /// <param name="Desglose"> Lista de objetos DetalleDesglose.</param>
        /// <returns> Lista de TaxItems</returns>
        internal static List<TaxItem> FromDesglose(List<DetalleDesglose> Desglose)
        {

            var taxitems = new List<TaxItem>();

            foreach (var desglose in Desglose)
            {
                var taxItem = new TaxItem()
                {
                    TaxBase = XmlParser.ToDecimal(desglose.BaseImponibleOimporteNoSujeto),
                    TaxRate = XmlParser.ToDecimal(desglose.TipoImpositivo),
                    TaxAmount = XmlParser.ToDecimal(desglose.CuotaRepercutida),
                    TaxRateSurcharge = XmlParser.ToDecimal(desglose.TipoRecargoEquivalencia),
                    TaxAmountSurcharge = XmlParser.ToDecimal(desglose.CuotaRecargoEquivalencia)
                };

                taxItem.Tax = desglose.Impuesto;

                if(desglose.ClaveRegimenSpecified)
                    taxItem.TaxScheme = desglose.ClaveRegimen;

                if (desglose.CalificacionOperacionSpecified)
                    taxItem.TaxType = desglose.CalificacionOperacion;

                taxitems.Add(taxItem);

            }

            return taxitems;

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Calcula los totales de la factura.
        /// </summary>
        internal void CalculateTotals() 
        {

            TotalAmount = TotalTaxOutput = TotalTaxWithheld = TotalTaxOutputSurcharge = _NetAmount = 0;

            if (TaxItems == null || TaxItems.Count == 0)
                return;

            foreach (var taxitem in TaxItems) 
            {

                if (taxitem.TaxClass == TaxClass.TaxOutput)
                {
                    _NetAmount += taxitem.TaxBase;
                    TotalTaxOutput += taxitem.TaxAmount;
                    TotalTaxOutputSurcharge += taxitem.TaxAmountSurcharge;
                }
                else 
                {
                    TotalTaxWithheld += taxitem.TaxAmount;
                }

            }

            TotalAmount = _NetAmount + TotalTaxOutput + TotalTaxOutputSurcharge - TotalTaxWithheld;

        }

        /// <summary>
        /// Obtiene el desglose de la factura.
        /// </summary>
        /// <returns>Desglose de la factura.</returns>
        private List<DetalleDesglose> GetDesglose() 
        {

            if (TaxItems == null || TaxItems?.Count == 0) 
                throw new InvalidOperationException("No se puede obtener el bloque obligatorio" +
                    " 'DetalleDesglose' ya que la lista de TaxItems no contiene elementos.");

            var desglose = new List<DetalleDesglose>(); 

            foreach (var taxitem in TaxItems)
            {

                // Valores por defecto
                var tax = Enum.IsDefined(typeof(Impuesto), taxitem.Tax) ? taxitem.Tax : Impuesto.IVA;
                var taxScheme = Enum.IsDefined(typeof(ClaveRegimen), taxitem.TaxScheme) ? taxitem.TaxScheme : ClaveRegimen.RegimenGeneral;

                // Máximo dos decimales
                var taxRate = Math.Round(taxitem.TaxRate, 2, MidpointRounding.AwayFromZero);
                var taxBase = Math.Round(taxitem.TaxBase, 2, MidpointRounding.AwayFromZero);
                var taxAmount = Math.Round(taxitem.TaxAmount, 2, MidpointRounding.AwayFromZero);
                var taxRateSurcharge = Math.Round(taxitem.TaxRateSurcharge, 2, MidpointRounding.AwayFromZero);
                var taxAmountSurcharge = Math.Round(taxitem.TaxAmountSurcharge, 2, MidpointRounding.AwayFromZero);

                var detalleDesglose = new DetalleDesglose()
                {
                    Impuesto = tax,
                    ClaveRegimen = taxScheme,
                    CalificacionOperacion = taxitem.TaxType,
                    CalificacionOperacionSpecified = taxitem.TaxException == CausaExencion.NA,
                    TipoImpositivo = XmlParser.GetXmlDecimal(taxRate),
                    BaseImponibleOimporteNoSujeto = XmlParser.GetXmlDecimal(taxBase),
                    CuotaRepercutida = XmlParser.GetXmlDecimal(taxAmount),
                };

                // Evita error de la AEAT 1237: El valor del campo CalificacionOperacion está informado como N1 o N2 y el impuesto es IVA. 
                // No se puede informar de los campos TipoImpositivo, CuotaRepercutida, TipoRecargoEquivalencia y CuotaRecargoEquivalencia.                
                var isN = detalleDesglose?.CalificacionOperacion == CalificacionOperacion.N1 || 
                    detalleDesglose?.CalificacionOperacion == CalificacionOperacion.N2; // No sujeta

                if (isN &detalleDesglose?.Impuesto == Impuesto.IVA) 
                    if (taxRate + taxAmount == 0)
                        detalleDesglose.TipoImpositivo = detalleDesglose.CuotaRepercutida = null;

                // Establece la serialización de la causa exención en su caso
                if (taxitem.TaxException != CausaExencion.NA) 
                {
                    detalleDesglose.OperacionExentaSpecified = true;
                    detalleDesglose.OperacionExenta = taxitem.TaxException;
                    detalleDesglose.CuotaRepercutida = detalleDesglose.TipoImpositivo = null;
                }

                if (taxitem.TaxAmountSurcharge != 0) 
                {

                    detalleDesglose.TipoRecargoEquivalencia = XmlParser.GetXmlDecimal(taxRateSurcharge);
                    detalleDesglose.CuotaRecargoEquivalencia = XmlParser.GetXmlDecimal(taxAmountSurcharge);

                }

                detalleDesglose.ClaveRegimenSpecified = (detalleDesglose.Impuesto == Impuesto.IVA || detalleDesglose.Impuesto == Impuesto.IGIC);

                desglose.Add(detalleDesglose);

            }

            return desglose;

        }

        /// <summary>
        /// Obtiene la lista de destinatarios.
        /// </summary>
        /// <returns>Lista de destinatarios.</returns>
        private List<Interlocutor> GetDestinatarios() 
        {

            // Factura simplificada sin contraparte
            if (string.IsNullOrEmpty(BuyerID) && (InvoiceType == TipoFactura.F2 || InvoiceType == TipoFactura.R5))
                return null;

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
                Utils.Log($"{ex}");
                throw;

            }

            if (isTaxIdEs && BuyerIDType != IDType.NO_CENSADO && (BuyerCountryID == "ES" || string.IsNullOrEmpty(BuyerCountryID)))
                return new List<Interlocutor>()
                {
                    new Interlocutor
                    {
                        NombreRazon = BuyerName,
                        NIF = BuyerID
                    }
                };

            if(string.IsNullOrEmpty(BuyerCountryID))
                throw new Exception($"Error en factura ({this}): Si BuyerID no es un identificador español válido" +
                    " (NIF, DNI, NIE...) es obligatorio que BuyerCountryID tenga un valor.");

            bool countryIdValid = Enum.TryParse(BuyerCountryID, out CodigoPais buyerCountryId);

            if (!countryIdValid)
                throw new Exception($"Error en factura ({this}): El código de pais consignado en BuyerCountryID='{BuyerCountryID}' no es válido.");

            if (BuyerIDType == 0)
                throw new Exception($"Error en factura ({this}): BuyerIDType debe tener un valor válido.");

            return new List<Interlocutor>()
                {
                    new Interlocutor
                    {
                        NombreRazon = BuyerName,
                        IDOtro = new IDOtro()
                        { 
                            CodigoPais = buyerCountryId,
                            CodigoPaisSpecified = countryIdValid,
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
        ///  Identifica si el tipo de factura rectificativa
        ///  es por sustitución o por diferencia (L3).
        /// </summary>
        public TipoRectificativa RectificationType { get; set; }

        /// <summary>
        /// Identificador de la factura.
        /// </summary>
        public string InvoiceID { get; private set; }

        /// <summary>
        /// Fecha emisión de documento.
        /// </summary>        
        public DateTime InvoiceDate { get; private set; }

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
        public string SellerID { get; private set; }

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
        public decimal TotalAmount { get; private set; }

        /// <summary>
        /// Total impuestos soportados.
        /// </summary>        
        public decimal TotalTaxOutput { get; private set; }

        /// <summary>
        /// Total impuestos soportados.
        /// </summary>        
        public decimal TotalTaxOutputSurcharge { get; private set; }

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
        /// Facturas rectificadas.
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

        /// <summary>
        /// RegistroAlta a partir del cual se ha creado la factura, en el
        /// caso de que la instancia se haya creado a partir de un registro
        /// de alta.
        /// </summary>
        [Json(JsonIgnore = true)]
        public RegistroAlta RegistroAltaSource 
        {

            get 
            {

                return _RegistroAltaSource;

            }

        }

        #endregion     

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Obtiene el registro de alta para verifactu.
        /// </summary>
        /// <returns>Registro de alta para verifactu</returns>
        public RegistroAlta GetRegistroAlta()
        {

            if (_RegistroAltaSource != null)
                return _RegistroAltaSource;

            CalculateTotals();

            // Máximo dos decimales
            var totalTaxAmount = Math.Round(TotalTaxOutput + TotalTaxOutputSurcharge, 2);
            var totalAmount = Math.Round(TotalAmount, 2);

            var registroAlta = new RegistroAlta()
            {
                IDVersion = Settings.Current.IDVersion,
                IDFacturaAlta = new IDFactura()
                {
                    IDEmisorFactura = SellerID.Trim(),
                    NumSerieFactura = InvoiceID.Trim(),
                    FechaExpedicionFactura = XmlParser.GetXmlDate(InvoiceDate)                    
                }, 
                NombreRazonEmisor = SellerName,
                TipoFactura = InvoiceType,
                DescripcionOperacion = Text,
                Destinatarios = GetDestinatarios(),
                Desglose = GetDesglose(),
                CuotaTotal = XmlParser.GetXmlDecimal(totalTaxAmount),
                ImporteTotal = XmlParser.GetXmlDecimal(totalAmount),
                SistemaInformatico = Settings.Current.SistemaInformatico,
                TipoHuella = TipoHuella.Sha256,
                TipoHuellaSpecified = true
            };

            if (OperationDate != null)
                registroAlta.FechaOperacion = XmlParser.GetXmlDate(OperationDate);

            var isRectification = Array.IndexOf(new TipoFactura[]{ TipoFactura.R1, TipoFactura.R2,
                TipoFactura.R3, TipoFactura.R4, TipoFactura.R5 }, InvoiceType) != -1;

            if (isRectification) 
            {

                // Verificamos si es por sustitución
                if (RectificationType == TipoRectificativa.S && RectificationTaxBase != 0) 
                {

                    registroAlta.ImporteRectificacion = new ImporteRectificacion()
                    {
                        BaseRectificada = XmlParser.GetXmlDecimal(RectificationTaxBase),
                        CuotaRectificada = XmlParser.GetXmlDecimal(RectificationTaxAmount),
                        CuotaRecargoRectificado = XmlParser.GetXmlDecimal(RectificationTaxAmountSurcharge)
                    };

                }

                // Establecemos el tipo de rectificativa (Por diferencias es el valor por defecto)
                if (RectificationType == TipoRectificativa.NA)
                    registroAlta.TipoRectificativa = TipoRectificativa.I; // Por defecto

                registroAlta.TipoRectificativaSpecified = true;              

            }

            if (RectificationItems?.Count > 0) 
            {

                if (!isRectification)
                    throw new InvalidOperationException("No se pueden incluir elementos en la lista 'RectificationItems'" +
                        " si InvoiceType no es rectificativa (R1, R2, R3, R4, R5).");

                // Añadimos las factura rectificadas
                registroAlta.FacturasRectificadas = new IDFactura[RectificationItems.Count];

                for(int rectificationIndex = 0; rectificationIndex< RectificationItems.Count; rectificationIndex++)
                    registroAlta.FacturasRectificadas[rectificationIndex] =new IDFactura()
                    {
                        IDEmisorFactura = SellerID,
                        NumSerieFactura = RectificationItems[rectificationIndex].InvoiceID,
                        FechaExpedicionFactura = XmlParser.GetXmlDate(RectificationItems[rectificationIndex].InvoiceDate)
                    };

            }

            if (RectificationType != TipoRectificativa.NA)
            {

                if (!isRectification)
                    throw new InvalidOperationException("RectificationType no puede estar asignado (tiene que ser TipoRectificativa.NA)" +
                        " si InvoiceType no es rectificativa (R1, R2, R3, R4, R5).");

                registroAlta.TipoRectificativa = RectificationType;
                registroAlta.TipoRectificativaSpecified = true;

            }

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
                    NumSerieFacturaAnulada = InvoiceID.Trim(), // La AEAT calcula el Hash sin espacios
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
