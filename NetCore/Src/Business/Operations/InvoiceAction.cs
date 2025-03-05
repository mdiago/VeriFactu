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
using VeriFactu.Business.Validation;

namespace VeriFactu.Business.Operations
{

    /// <summary>
    /// Representa una acción de alta o anulación de registro
    /// en todo lo referente a la factura, su envío a la AEAT
    /// y su gestión contable en la 
    /// cadena de bloques.
    /// </summary>
    public class InvoiceAction : InvoiceActionPost
    {    

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="invoice">Instancia de factura de entrada en el sistema.</param>
        public InvoiceAction(Invoice invoice) : base(invoice)
        {

            // Validamos
            var errors = GetBusErrors();

            if (errors.Count > 0)
                throw new InvalidOperationException(string.Join("\n", errors));  

        }

        #endregion

        #region Métodos Privados de Instancia


        /// <summary>
        /// Devuelve errores de las validaciones de negocio según las
        /// especificaciones.
        /// </summary>
        /// <returns>Lista de errores de validación según las especificaciones.</returns>
        internal virtual List<string> GetInvoiceValidationErrors() 
        {

            var validation = new InvoiceValidation(this);
            return validation.GetErrors();

        }

        /// <summary>
        /// Devuelve una lista con los errores de la
        /// línea de impuesto por el incumplimiento de reglas de negocio.
        /// </summary>
        /// <returns>Lista con los errores encontrados.</returns>
        private List<string> GetTaxItemValidationErrors(TaxItem taxItem) 
        {

            var errors = new List<string>();

            // Validaciones en líneas exentas
            if (taxItem.TaxException != VeriFactu.Xml.Factu.Alta.CausaExencion.NA) 
            { 
            
                if(taxItem.TaxRate + taxItem.TaxAmount + taxItem.TaxRateSurcharge + taxItem.TaxAmountSurcharge != 0)
                    errors.Add($"Taxitem [{taxItem}] con TaxException asignada '{taxItem.TaxException}' no puede tener un valor distinto de 0 en las propiedades" +
                        $" TaxRate, TaxAmount, TaxRateSurcharge y TaxAmountSurcharge.");

            }

            return errors;

        }

        /// <summary>
        /// Devuelve una lista con los errores de las
        /// líneas de impuesto por el incumplimiento de reglas de negocio.
        /// </summary>
        /// <returns>Lista con los errores encontrados.</returns>
        private List<string> GetTaxItemsValidationErrors()
        {

            var errors = new List<string>();

            if(Invoice.TaxItems != null)
                foreach (var taxItem in Invoice.TaxItems)
                    errors.AddRange(GetTaxItemValidationErrors(taxItem));

            return errors;

        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Devuelve una lista con los errores de la
        /// factura por el incumplimiento de reglas de negocio.
        /// </summary>
        /// <returns>Lista con los errores encontrados.</returns>
        public List<string> GetBusErrors()
        {

            var errors = new List<string>();

            if (File.Exists(InvoiceFilePath))
                errors.Add($"Ya existe una entrada con SellerID: {Invoice.SellerID}" +
                    $" en el año {Invoice.InvoiceDate.Year} con el número {Invoice.InvoiceID}.");

            if (string.IsNullOrEmpty(Invoice.SellerName))
                errors.Add($"Es necesario que la propiedad Invoice.SellerName tenga un valor.");

            // Limite listas
            if(Invoice.RectificationItems?.Count > 1000)
                errors.Add($"Invoice.RectificationItems.Count no puede ser mayor de 1.000.");

            if (Invoice.TaxItems?.Count > 12)
                errors.Add($"Invoice.TaxItems.Count no puede ser mayor de 12.");

            errors.AddRange(GetTaxItemsValidationErrors());
            errors.AddRange(GetInvoiceValidationErrors());

            return errors;

        }

        #endregion

    }

}