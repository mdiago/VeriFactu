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
using System.Runtime.InteropServices;
using VeriFactu.Business.Operations;

namespace Verifactu
{

    #region Interfaz COM

    /// <summary>
    /// Interfaz COM para la clase RectificationItem.
    /// </summary>
    [Guid("BF4B91AA-AF25-4C90-B8FB-BFEACE036703")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    [ComVisible(true)]
    public interface IVfInvoiceQuery
    {

        #region Propiedades Públicas de Instancia

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
        /// Año a consultar.
        /// </summary>
        string Year { get; set; }

        /// <summary>
        /// Mes a consultar.
        /// </summary>
        string Month { get; set; }

        /// <summary>
        /// Resultado última llamada.
        /// </summary>
        VfInvoice[] Invoices { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Devuelve las facturas emitidas por el NIF
        /// facilitado en la propiedad PartyID.
        /// </summary>
        /// <returns>Facturas emitidas registradas en la AEAT.</returns>
        void GetSales();

        /// <summary>
        /// Devuelve las facturas recibidas por el NIF
        /// facilitado en la propiedad PartyID.
        /// </summary>
        /// <returns>Facturas recibidas registradas en la AEAT.</returns>
        void GetPurchases();

        /// <summary>
        /// Devuelve el número de facturas.
        /// </summary>
        /// <returns>Número de facturas.</returns>
        int GetInvoicesCount();

        /// <summary>
        /// Devuelve una factura por su índice.
        /// </summary>
        /// <param name="index">Indice de la factura a devolver.</param>
        /// <returns>Factura con el indice indicado.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Si el indice está fuera de rango.</exception>
        VfInvoice GetInvoice(int index);

        #endregion

    }

    #endregion

    #region Clase COM

    /// <summary>
    /// Resultado de un envio de alta o anulación a la AEAT.
    /// </summary>
    [Guid("9279A76F-E672-4128-899F-A2B3CCB8E6C0")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    [ProgId("Verifactu.VfInvoiceQuery")]
    public class VfInvoiceQuery : IVfInvoiceQuery
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor. Para COM necesitamos un constructor
        /// sin parametros.
        /// </summary>
        public VfInvoiceQuery()
        {
        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Devuelve una lista de VfInvoices de una
        /// lista de Invoices.
        /// </summary>
        /// <param name="invoices">Lista de Invoices a convertir.</param>
        /// <returns> Lista de VfInvoices</returns>
        private VfInvoice[] GetFromInvoiceList(List<VeriFactu.Business.Invoice> invoices) 
        {

            VfInvoice[] result = new VfInvoice[invoices.Count];

            for (int i = 0; i < result.Length; i++)
            {

                result[i] = new VfInvoice
                {
                    InvoiceID = invoices[i].InvoiceID,
                    InvoiceType = $"{invoices[i].InvoiceType}",
                    InvoiceDate = invoices[i].InvoiceDate,
                    SellerID = invoices[i].SellerID,
                    SellerName = invoices[i].SellerName,
                    BuyerID = invoices[i].BuyerID,
                    BuyerName = invoices[i].BuyerName,
                    Text = invoices[i].Text
                };

                if (invoices[i].RectificationType != VeriFactu.Xml.Factu.Alta.TipoRectificativa.NA)
                    result[i].InvoiceType = $"{invoices[i].InvoiceType}";

                foreach (var taxItem in invoices[i].TaxItems)
                {

                    var txIt = new VfTaxItem()
                    {
                        TaxType = $"{taxItem.TaxType}",
                        TaxRate = (float)taxItem.TaxRate,
                        TaxBase = (float)taxItem.TaxBase,
                        TaxAmount = (float)taxItem.TaxAmount,
                        TaxRateSurcharge = (float)taxItem.TaxRateSurcharge,
                        TaxAmountSurcharge = (float)taxItem.TaxAmountSurcharge
                    };

                    result[i].InsertTaxItem(txIt);

                }

            }

            return result;

        }

        #endregion

        #region Propiedades Públicas de Instancia

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
        /// Año a consultar.
        /// </summary>
        public string Year { get; set; }

        /// <summary>
        /// Mes a consultar.
        /// </summary>
        public string Month { get; set; }

        /// <summary>
        /// Resultado última llamada.
        /// </summary>
        public VfInvoice[] Invoices { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Devuelve las facturas emitidas por el NIF
        /// facilitado en la propiedad PartyID.
        /// </summary>
        /// <returns>Facturas emitidas registradas en la AEAT.</returns>
        public void GetSales()
        {

            var invoiceQuery = new InvoiceQuery(SellerID, SellerName);

            // Consulta facturas emitidas
            var salesResponse = invoiceQuery.GetSales(Year, Month);

            // Lista de objetos Invoice a partir de la respuesta AEAT facturas emitidas
            var salesInvoices = InvoiceQuery.GetInvoices(salesResponse);

            Invoices = GetFromInvoiceList(salesInvoices);

        }

        /// <summary>
        /// Devuelve las facturas recibidas por el NIF
        /// facilitado en la propiedad PartyID.
        /// </summary>
        /// <returns>Facturas recibidas registradas en la AEAT.</returns>
        public void GetPurchases()
        {

            var invoiceQuery = new InvoiceQuery(SellerID, SellerName);

            // Consulta facturas emitidas
            var salesResponse = invoiceQuery.GetPurchases(Year, Month);

            // Lista de objetos Invoice a partir de la respuesta AEAT facturas recibidas
            var salesInvoices = InvoiceQuery.GetInvoices(salesResponse);

            Invoices = GetFromInvoiceList(salesInvoices);

        }

        /// <summary>
        /// Devuelve el número de facturas.
        /// </summary>
        /// <returns>Número de facturas.</returns>
        public int GetInvoicesCount()
        {
            return Invoices?.Length ?? 0;
        }

        /// <summary>
        /// Devuelve una factura por su índice.
        /// </summary>
        /// <param name="index">Indice de la factura a devolver.</param>
        /// <returns>Factura con el indice indicado.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Si el indice está fuera de rango.</exception>
        public VfInvoice GetInvoice(int index)
        {
            
            if (Invoices == null || index < 0 || index >= Invoices.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "Índice fuera de rango.");

            return Invoices[index];

        }

        #endregion

    }

    #endregion

}