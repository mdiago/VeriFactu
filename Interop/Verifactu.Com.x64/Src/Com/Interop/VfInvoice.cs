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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using VeriFactu.Business.Validation.NIF;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu.Alta;

namespace Verifactu
{

    #region Interfaz COM

    /// <summary>
    /// Interfaz COM para la clase VfInvoice.
    /// </summary>
    [Guid("8404DFF1-C973-44BA-9DFC-E17A02CDA5E3")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    [ComVisible(true)]
    public interface IVfInvoice
    {

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// <para>Clave del tipo de factura (L2).</para>
        /// </summary>
        string InvoiceType { get; set; }

        /// <summary>
        ///  Identifica si el tipo de factura rectificativa
        ///  es por sustitución o por diferencia (L3).
        /// </summary>
        string RectificationType { get; set; }

        /// <summary>
        /// Identificador de la factura.
        /// </summary>
        string InvoiceID { get; set; }

        /// <summary>
        /// Fecha emisión de documento.
        /// </summary>        
        DateTime InvoiceDate { get; set; }

        /// <summary>
        /// Identificador del vendedor.
        /// Debe utilizarse el identificador fiscal si existe (NIF, VAT Number...).
        /// En caso de no existir, se puede utilizar el número DUNS 
        /// o cualquier otro identificador acordado.
        /// </summary>        
        string SellerID { get; set; }

        /// <summary>
        /// Nombre del vendedor.
        /// </summary>        
        string SellerName { get; set; }

        /// <summary>
        /// Identidicador del comprador.
        /// Debe utilizarse el identificador fiscal si existe (NIF, VAT Number...).
        /// En caso de no existir, se puede utilizar el número DUNS 
        /// o cualquier otro identificador acordado.
        /// </summary>        
        string BuyerID { get; set; }

        /// <summary>
        /// Nombre del comprador.
        /// </summary>        
        string BuyerName { get; set; }

        /// <summary>
        /// Código del país del destinatario (a veces también denominado contraparte,
        /// es decir, el cliente) de la operación de la factura expedida.
        /// <para>Alfanumérico (2) (ISO 3166-1 alpha-2 codes) </para>
        /// </summary>        
        string BuyerCountryID { get; set; }

        /// <summary>
        /// Clave para establecer el tipo de identificación
        /// en el pais de residencia. L7.
        /// </summary>        
        int BuyerIDType { get; set; }

        /// <summary>
        /// Importe total: Total neto + impuestos soportado
        /// - impuestos retenidos.
        /// </summary>        
        decimal TotalAmount { get; }

        /// <summary>
        /// Total impuestos soportados.
        /// </summary>        
        decimal TotalTaxOutput { get; }

        /// <summary>
        /// Total impuestos soportados.
        /// </summary>        
        decimal TotalTaxOutputSurcharge { get; }

        /// <summary>
        /// Importe total impuestos retenidos.
        /// </summary>        
        decimal TotalTaxWithheld { get; }

        /// <summary>
        /// Texto del documento.
        /// </summary>
        string Text { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Establece la fecha operación en caso
        /// de que se necesaria.
        /// </summary>
        /// <param name="operationDate">Fecha operación.</param>
        [DispId(1)]
        void SeOperationDate(DateTime operationDate);

        /// <summary>
        /// Obtiene el registro de alta para verifactu.
        /// </summary>
        /// <returns>Registro de alta para verifactu</returns>
        [DispId(2)]
        string GetRegistroAlta();

        /// <summary>
        /// Añade datos en rectificativa por sustitución de la
        /// factura sustituida.
        /// </summary>
        /// <param name="taxItem">Datos factura sustituida.</param>
        [DispId(3)]
        void SetSubstitution(IVfTaxItem taxItem);

        /// <summary>
        /// Obtiene el registro de alta para verifactu.
        /// </summary>
        /// <returns>Registro de alta para verifactu</returns>
        string GetRegistroAnulacion();

        /// <summary>
        /// Obtiene un bitmap con la url de validación
        /// codificada en un código QR.
        /// </summary>
        /// <param name="path">Ruta donde se guardará el archivo de mapa de bits.</param>
        /// <returns>Bitmap con la url de validación
        /// codificada en un código QR.</returns>
        void GetValidateQr(string path);

        /// <summary>
        /// Obtiene un bitmap con la url de validación
        /// codificada en un código QR.
        /// </summary>
        /// <returns>Bitmap con la url de validación
        /// codificada en un código QR.</returns>
        byte[] GetValidateQrBytes();

        /// <summary>
        /// Devuelve la url para la validación del documento.
        /// </summary>
        /// <returns>Url para la validación del documento.</returns>
        string GetUrlValidate();

        /// <summary>
        /// Añade una línea de impuestos a la factura.
        /// </summary>
        /// <param name="taxItem">Línea de impuestos a añadir.</param>
        void InsertTaxItem(IVfTaxItem taxItem);

        /// <summary>
        /// Elimina una línea de impuestos de la factura.
        /// </summary>
        /// <param name="index">Número de la línea a eliminar.</param>
        void DeleteTaxItemAt(int index);

        /// <summary>
        /// Añade una línea de factura rectificada.
        /// </summary>
        /// <param name="rectificationItem">Línea de factura rectificada a añadir.</param>
        void InsertRectificationItem(IVfRectificationItem rectificationItem);

        /// <summary>
        /// Elimina una línea de factura rectificada de la factura.
        /// </summary>
        /// <param name="index">Número de la línea a eliminar.</param>
        void DeleteRectificationItemAt(int index);

        /// <summary>
        /// Envía la factura a Verifactu de la AEAT.
        /// </summary>
        /// <returns>Resultado de la operación.</returns>
        IVfInvoiceResult Send();

        /// <summary>
        /// Envía la anulación de la factura a Verifactu de la AEAT.
        /// </summary>
        /// <returns>Resultado de la operación.</returns>
        IVfInvoiceResult Delete();

        /// <summary>
        /// Válida los datos de un NIF mediante el servicio
        /// de validación de la AEAT.
        /// </summary>
        /// <param name="nif">NIF a validar.</param>
        /// <param name="name">Nombre asociado al NIF a validar.</param>
        /// <returns>Cadena con la descripción de los errores o null
        /// si todo es correcto.</returns>
        string GetNifErrors(string nif, string name);

        #endregion

    }

    #endregion

    #region Clase COM

    /// <summary>
    /// Representa una factura.
    /// </summary>
    [Guid("BBADB0CE-A83F-455C-8BCE-1D5E200BF5BC")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    [ProgId("Verifactu.VfInvoice")]
    public class VfInvoice : IVfInvoice
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Fecha operación.
        /// </summary>        
        DateTime? _OperationDate;

        /// <summary>
        /// Objeto factura generado con los datos.
        /// </summary>
        VeriFactu.Business.Invoice _Invoice;

        /// <summary>
        /// Lista de líneas de impuestos.
        /// </summary>
        List<VeriFactu.Business.TaxItem> _TaxItems;

        /// <summary>
        /// Facturas rectificadas.
        /// </summary>
        List<VeriFactu.Business.RectificationItem> _RectificationItems { get; set; }

        /// <summary>
        /// Información de rectificación sustitutiva.
        /// </summary>
        VeriFactu.Business.TaxItem _TaxItemSubstitution;

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor. Para COM necesitamos un constructor
        /// sin parametros.
        /// </summary>
        public VfInvoice()
        {

            _TaxItems = new List<VeriFactu.Business.TaxItem>();
            _RectificationItems = new List<VeriFactu.Business.RectificationItem>();

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Devuelve una instancia de clase Invoice creada a partir
        /// de los datos de esta instancia.
        /// </summary>
        /// <returns>Instancia de clase Invoice.</returns>
        private VeriFactu.Business.Invoice GetInvoice()
        {

            var result = new VeriFactu.Business.Invoice(InvoiceID, InvoiceDate, SellerID)
            {
                SellerName = SellerName,
                BuyerID = BuyerID,
                BuyerName = BuyerName,
                Text = Text
            };

            if (_OperationDate != null)
                result.OperationDate = _OperationDate;

            var invoiceType = string.IsNullOrEmpty(InvoiceType) ? "F1" : InvoiceType;

            if (!Enum.TryParse(invoiceType, out VeriFactu.Xml.Factu.Alta.TipoFactura tipoFactura))
                throw new ArgumentException($"El valor de InvoiceType '{invoiceType}' no es válido." +
                    $" Consulte en las especificaciones de la AEAT la lista: Clave del tipo de factura (L2)");

            result.InvoiceType = tipoFactura;

            var rectificationType = string.IsNullOrEmpty(RectificationType) ? "NA" : RectificationType;

            if (!Enum.TryParse(rectificationType, out VeriFactu.Xml.Factu.Alta.TipoRectificativa tipoRectificativa))
                throw new ArgumentException($"El valor de RectificationType '{rectificationType}' no es válido." +
                    $" Consulte en las especificaciones de la AEAT la lista: Identifica si el tipo de factura rectificativa" +
                    $" es por sustitución o por diferencia (L3).");

            result.RectificationType = tipoRectificativa;

            var buyerIDType = BuyerIDType == 0 ? 2 : BuyerIDType;

            var buyerIDTypes = new int[] { 2, 3, 4, 5, 6, 7 };

            if (Array.IndexOf(buyerIDTypes, buyerIDType) == -1)
                throw new ArgumentException($"El valor de InvoiceType '{BuyerIDType}' no es válido." +
                    $" Debe ser 2,3,4,5,6 o 7.");

            result.BuyerIDType = (VeriFactu.Xml.Factu.IDType)buyerIDType;

            if(!string.IsNullOrEmpty(BuyerCountryID))
                result.BuyerCountryID = BuyerCountryID;

            result.TaxItems = _TaxItems;
            result.CalculateTotals();
            result.RectificationItems = _RectificationItems;

            if (_TaxItemSubstitution != null)
                if (string.IsNullOrEmpty(InvoiceType) || InvoiceType[0] != 'R')
                    throw new ArgumentException("Para poder establecer la sustitución de una factura, " +
                        "el tipo de factura debe ser rectificativa.");

            return result;

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// <para>Clave del tipo de factura (L2).</para>
        /// </summary>
        public string InvoiceType { get; set; }

        /// <summary>
        ///  Identifica si el tipo de factura rectificativa
        ///  es por sustitución o por diferencia (L3).
        /// </summary>
        public string RectificationType { get; set; }

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
        /// Código del país del destinatario (a veces también denominado contraparte,
        /// es decir, el cliente) de la operación de la factura expedida.
        /// <para>Alfanumérico (2) (ISO 3166-1 alpha-2 codes) </para>
        /// </summary>        
        public string BuyerCountryID { get; set; }

        /// <summary>
        /// Clave para establecer el tipo de identificación
        /// en el pais de residencia. L7.
        /// </summary>        
        public int BuyerIDType { get; set; }

        /// <summary>
        /// Importe total: Total neto + impuestos soportado
        /// - impuestos retenidos.
        /// </summary>        
        public decimal TotalAmount
        {

            get
            {

                _Invoice = GetInvoice();
                return _Invoice.TotalAmount;

            }

        }

        /// <summary>
        /// Total impuestos soportados.
        /// </summary>        
        public decimal TotalTaxOutput
        {

            get
            {

                _Invoice = GetInvoice();
                return _Invoice.TotalTaxOutput;

            }

        }

        /// <summary>
        /// Total impuestos soportados.
        /// </summary>        
        public decimal TotalTaxOutputSurcharge
        {

            get
            {

                _Invoice = GetInvoice();
                return _Invoice.TotalTaxOutputSurcharge;

            }

        }

        /// <summary>
        /// Importe total impuestos retenidos.
        /// </summary>        
        public decimal TotalTaxWithheld
        {

            get
            {

                _Invoice = GetInvoice();
                return _Invoice.TotalTaxWithheld;

            }

        }

        /// <summary>
        /// Texto del documento.
        /// </summary>
        public string Text { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Establece la fecha operación en caso
        /// de que se necesaria.
        /// </summary>
        /// <param name="operationDate">Fecha operación.</param>
        public void SeOperationDate(DateTime operationDate)
        {

            _OperationDate = operationDate;

        }

        /// <summary>
        /// Obtiene el registro de alta para verifactu.
        /// </summary>
        /// <returns>Registro de alta para verifactu</returns>
        public string GetRegistroAlta()
        {

            _Invoice = GetInvoice();
            return Encoding.UTF8.GetString(new VeriFactu.Business.InvoiceEntry(_Invoice).GetXml());

        }

        /// <summary>
        /// Obtiene el registro de alta para verifactu.
        /// </summary>
        /// <returns>Registro de alta para verifactu</returns>
        public string GetRegistroAnulacion()
        {

            _Invoice = GetInvoice();
            return Encoding.UTF8.GetString(new VeriFactu.Business.InvoiceCancellation(_Invoice).GetXml());

        }

        /// <summary>
        /// Obtiene un bitmap con la url de validación
        /// codificada en un código QR.
        /// </summary>
        /// <param name="path">Ruta donde se guardará el archivo de mapa de bits.</param>
        /// <returns>Bitmap con la url de validación
        /// codificada en un código QR.</returns>
        public void GetValidateQr(string path)
        {

            var bmQr = GetValidateQrBytes();
            File.WriteAllBytes(path, bmQr);

        }

        /// <summary>
        /// Obtiene un bitmap con la url de validación
        /// codificada en un código QR.
        /// </summary>
        /// <returns>Bitmap con la url de validación
        /// codificada en un código QR.</returns>
        public byte[] GetValidateQrBytes()
        {

            _Invoice = GetInvoice();

            // Obtenemos una instancia de la clase RegistroAlta a partir de
            // la instancia del objeto de negocio Invoice
            var registro = _Invoice.GetRegistroAlta();

            // Obtenemos la imágen del QR
            var bmQr = registro.GetValidateQr();

            return bmQr;

        }


        /// <summary>
        /// Devuelve la url para la validación del documento.
        /// </summary>
        /// <returns>Url para la validación del documento.</returns>
        public string GetUrlValidate()
        {

            _Invoice = GetInvoice();
            var entry = new VeriFactu.Business.InvoiceEntry(_Invoice);
            return entry.Registro.GetUrlValidate();

        }

        /// <summary>
        /// Añade una línea de impuestos a la factura.
        /// </summary>
        /// <param name="taxItem">Línea de impuestos a añadir.</param>
        public void InsertTaxItem(IVfTaxItem taxItem)
        {

            var tax = string.IsNullOrEmpty(taxItem.Tax) ? "01" : taxItem.Tax;
            var taxScheme = string.IsNullOrEmpty(taxItem.TaxScheme) ? "01" : taxItem.TaxScheme;
            var taxType = string.IsNullOrEmpty(taxItem.TaxType) ? "S1" : taxItem.TaxType;
            var taxException = string.IsNullOrEmpty(taxItem.TaxException) ? "NA" : taxItem.TaxException;

            if (!Enum.TryParse(tax, out VeriFactu.Xml.Factu.Impuesto eTax))
                throw new ArgumentException($"El valor de Tax '{tax}' no es válido." +
                    $" Consulte en las especificaciones de la AEAT la lista: Impuesto (L1)");

            if (!Enum.TryParse(taxScheme, out VeriFactu.Xml.Factu.Alta.ClaveRegimen eTaxScheme))
                throw new ArgumentException($"El valor de TaxScheme '{taxScheme}' no es válido." +
                    $" Consulte en las especificaciones de la AEAT la lista: Regimen (L8A/L8B)");

            if (!Enum.TryParse(taxType, out VeriFactu.Xml.Factu.Alta.CalificacionOperacion eTaxType))
                throw new ArgumentException($"El valor de TaxType '{taxType}' no es válido." +
                    $" Consulte en las especificaciones de la AEAT la lista: Calificacion Operacion (L9)");

            if (!Enum.TryParse(taxException, out VeriFactu.Xml.Factu.Alta.CausaExencion eTaxException))
                throw new ArgumentException($"El valor de TaxException '{taxException}' no es válido." +
                    $" Consulte en las especificaciones de la AEAT la lista: CausaE xencion (L10)");


            _TaxItems.Add(new VeriFactu.Business.TaxItem()
            {
                Tax = eTax,
                TaxScheme = eTaxScheme,
                TaxType = eTaxType,
                TaxException = eTaxException,
                TaxBase = Math.Round(Convert.ToDecimal(taxItem.TaxBase),2),
                TaxRate = Math.Round(Convert.ToDecimal(taxItem.TaxRate), 2),
                TaxAmount = Math.Round(Convert.ToDecimal(taxItem.TaxAmount), 2),
                TaxRateSurcharge = Math.Round(Convert.ToDecimal(taxItem.TaxRateSurcharge), 2),
                TaxAmountSurcharge = Math.Round(Convert.ToDecimal(taxItem.TaxAmountSurcharge), 2)
            });


        }

        /// <summary>
        /// Elimina una línea de impuestos de la factura.
        /// </summary>
        /// <param name="index">Número de la línea a eliminar.</param>
        public void DeleteTaxItemAt(int index)
        {

            _TaxItems.RemoveAt(index);

        }

        /// <summary>
        /// Añade una línea de factura rectificada.
        /// </summary>
        /// <param name="rectificationItem">Línea de factura rectificada a añadir.</param>
        public void InsertRectificationItem(IVfRectificationItem rectificationItem)
        {

            _RectificationItems.Add(new VeriFactu.Business.RectificationItem()
            {
                InvoiceID = rectificationItem.InvoiceID,
                InvoiceDate = rectificationItem.InvoiceDate
            });

        }

        /// <summary>
        /// Elimina una línea de impuestos de la factura.
        /// </summary>
        /// <param name="index">Número de la línea a eliminar.</param>
        public void DeleteRectificationItemAt(int index)
        {

            _RectificationItems.RemoveAt(index);

        }

        /// <summary>
        /// Añade datos en rectificativa por sustitución de la
        /// factura sustituida.
        /// </summary>
        /// <param name="taxItem">Datos factura sustituida.</param>
        public void SetSubstitution(IVfTaxItem taxItem)
        {

            if (string.IsNullOrEmpty(InvoiceType) || InvoiceType[0] != 'R')
                throw new ArgumentException("Para poder establecer la sustitución de una factura, " +
                    "el tipo de factura debe ser rectificativa.");

            _TaxItemSubstitution = new VeriFactu.Business.TaxItem() 
            { 
                TaxBase = Math.Round(Convert.ToDecimal(taxItem.TaxBase), 2),
                TaxAmount = Math.Round(Convert.ToDecimal(taxItem.TaxAmount), 2),
                TaxAmountSurcharge = Math.Round(Convert.ToDecimal(taxItem.TaxAmountSurcharge), 2)
            };

        }


        /// <summary>
        /// Envía la factura a Verifactu de la AEAT.
        /// </summary>
        /// <returns>Resultado de la operación.</returns>
        public IVfInvoiceResult Send()
        {

            var result = new VfInvoiceResult()
            {
                ResultCode = "0"
            };

            _Invoice = GetInvoice();
            var entry = new VeriFactu.Business.InvoiceEntry(_Invoice);

            if (_TaxItemSubstitution != null) 
            {

                var registroAlta = entry.Registro as RegistroAlta;

                registroAlta.ImporteRectificacion = new ImporteRectificacion()
                {
                    BaseRectificada = XmlParser.GetXmlDecimal(_TaxItemSubstitution.TaxBase),
                    CuotaRectificada = XmlParser.GetXmlDecimal(_TaxItemSubstitution.TaxAmount),
                    CuotaRecargoRectificado = XmlParser.GetXmlDecimal(_TaxItemSubstitution.TaxAmountSurcharge)
                };

                registroAlta.TipoRectificativa = TipoRectificativa.S;

            }

            try
            {

                entry.Save();

            }
            catch (Exception ex)
            {
                result.ResultCode = "9001";
                result.ResultMessage = $"{ex}";

                return result;

            }

            if (entry.Status == "Correcto")
            {

                result.CSV = entry.CSV;
                result.Response = entry.Response;
                result.ResultMessage = "OK";

            }
            else if (!string.IsNullOrEmpty(entry.ErrorCode))
            {

                result.ResultCode = entry.ErrorCode;
                result.Response = entry.Response;
                result.ResultMessage = entry.ErrorDescription;

            }
            else if (entry.ErrorFault != null)
            {

                result.ResultCode = $"9002:{entry.ErrorFault.faultcode}";
                result.ResultMessage = entry.ErrorFault.faultstring;

            }
            else
            {

                result.ResultCode = "9009";
                result.ResultMessage = "Error desconocido";

            }

            return result;

        }

        /// <summary>
        /// Envía la anulación de la factura a Verifactu de la AEAT.
        /// </summary>
        /// <returns>Resultado de la operación.</returns>
        public IVfInvoiceResult Delete()
        {

            var result = new VfInvoiceResult()
            {
                ResultCode = "0"
            };

            _Invoice = GetInvoice();
            var cancellation = new VeriFactu.Business.InvoiceCancellation(_Invoice);

            try
            {

                cancellation.Save();

            }
            catch (Exception ex)
            {
                result.ResultCode = "9001";
                result.ResultMessage = $"{ex}";

                return result;

            }

            if (cancellation.Status == "Correcto")
            {

                result.CSV = cancellation.CSV;
                result.Response = cancellation.Response;
                result.ResultMessage = "OK";

            }
            else if (!string.IsNullOrEmpty(cancellation.ErrorCode))
            {

                result.ResultCode = cancellation.ErrorCode;
                result.Response = cancellation.Response;
                result.ResultMessage = cancellation.ErrorDescription;

            }
            else if (cancellation.ErrorFault != null)
            {

                result.ResultCode = $"9002:{cancellation.ErrorFault.faultcode}";
                result.ResultMessage = cancellation.ErrorFault.faultstring;

            }
            else
            {

                result.ResultCode = "9009";
                result.ResultMessage = "Error desconocido";

            }

            return result;

        }

        /// <summary>
        /// Válida los datos de un NIF mediante el servicio
        /// de validación de la AEAT.
        /// </summary>
        /// <param name="nif">NIF a validar.</param>
        /// <param name="name">Nombre asociado al NIF a validar.</param>
        /// <returns>Cadena con la descripción de los errores o null
        /// si todo es correcto.</returns>
        public string GetNifErrors(string nif, string name) 
        {

            var nifVal = new NifValidation(nif, name);
            var errors = nifVal.GetErrors();

            return errors.Count == 0 ? null : string.Join("\n", errors);

        }

        #endregion

    }

    #endregion

}