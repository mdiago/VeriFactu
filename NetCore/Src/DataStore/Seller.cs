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

using System.Collections.Generic;
using System.IO;
using VeriFactu.Config;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Soap;

namespace VeriFactu.DataStore
{

    /// <summary>
    /// Representa un vendedor o emisor de facturas
    /// en el sistema Verifactu.
    /// </summary>
    public class Seller
    {

        #region Propiedades Públicas de Instancia

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

        #endregion

        #region Métodos Públicos Estáticos

        /// <summary>
        /// Recupera los vendedores en el sistema.
        /// </summary>
        /// <returns>Vendedores en el sistema.</returns>
        public static Dictionary<string, List<PeriodOutbox>> GetSellers()
        {

            var sellersDic = new Dictionary<string, List<PeriodOutbox>>();

            foreach (var sellerDir in Directory.GetDirectories(Settings.Current.OutboxPath))
            {

                foreach (var periodDir in Directory.GetDirectories(sellerDir))
                {
                    var invoiceFiles = Directory.GetFiles(periodDir);

                    if (invoiceFiles.Length > 0)
                    {

                        var periodID = Path.GetFileName(periodDir);
                        var envelope = new Envelope(invoiceFiles[0]);
                        var registro = (envelope.Body.Registro as RegFactuSistemaFacturacion);

                        var seller = new Seller()
                        {
                            SellerID = $"{registro?.Cabecera?.ObligadoEmision?.IDOtro?.ID}{registro?.Cabecera?.ObligadoEmision?.NIF}",
                            SellerName = $"{registro?.Cabecera?.ObligadoEmision?.NombreRazon}"
                        };

                        var period = new PeriodOutbox(seller, periodID, invoiceFiles.Length);

                        if (sellersDic.ContainsKey(seller.SellerID))
                            sellersDic[seller.SellerID].Add(period);
                        else
                            sellersDic.Add(seller.SellerID, new List<PeriodOutbox>() { period });

                    }

                }

            }

            return sellersDic;

        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns>Representación textual de la instancia.</returns>
        public override string ToString()
        {
            return $"{SellerID} {SellerName}";
        }

        #endregion

    }

}
