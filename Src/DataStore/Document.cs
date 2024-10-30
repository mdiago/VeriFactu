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

using System.Collections.Generic;
using System;
using VeriFactu.Xml.Factu;

namespace VeriFactu.DataStore
{

    /// <summary>
    /// Representa un documento de facturación.
    /// </summary>
    public class Document
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Registros del documento (alta / anulación en su caso).
        /// </summary>
        List<Registro> _Records;

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="period">Periodo al que pertenece el documento.</param>
        /// <param name="record">Registro que pertenece el documento.</param>
        public Document(Period period, Registro record = null)
        {

            _Records = new List<Registro>();

            Period = period;
            AddRecord(record);

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Periodo al que pertenece el documento.
        /// </summary>
        public Period Period { get; private set; }

        /// <summary>
        /// Identificador del periodo.
        /// </summary>        
        public string PeriodID
        {
            get
            {
                return Period.PeriodID;
            }
        }

        /// <summary>
        /// Identificador del vendedor.
        /// Debe utilizarse el identificador fiscal si existe (NIF, VAT Number...).
        /// En caso de no existir, se puede utilizar el número DUNS 
        /// o cualquier otro identificador acordado.
        /// </summary>        
        public string SellerID
        {
            get
            {
                return Period.SellerID;
            }
        }

        /// <summary>
        /// Nombre del vendedor.
        /// </summary>        
        public string SellerName
        {
            get
            {
                return Period.SellerName;
            }
        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Añade un nuevo registro aldocumento.
        /// </summary>
        /// <param name="record">Registro a añadir.</param>
        public void AddRecord(Registro record) 
        {

            if(!record.IDFactura.FechaExpedicion.EndsWith(PeriodID))
                throw new InvalidOperationException($"No se puede añadir un registro con año" +
                    $" de emisión factura distinto de {PeriodID}.");

            if (record.IDFactura.IDEmisor != SellerID)
                throw new InvalidOperationException($"No se puede añadir un registro con emisor" +
                    $" de factura distinto de {SellerID}.");

            if(_Records.Count > 0)
                if($"{_Records[0].IDFactura}" != $"{record.IDFactura}")
                    throw new InvalidOperationException($"No se puede añadir un registro con IDFactura" +
                        $" {record.IDFactura} ya que ya existe un documento con IDFactura {_Records[0].IDFactura}.");

            _Records.Add(record);

        }

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns>Representación textual de la instancia.</returns>
        public override string ToString()
        {
            return $"({SellerID} {SellerName})";
        }

        #endregion

    }

}
