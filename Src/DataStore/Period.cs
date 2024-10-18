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
using System.IO;
using VeriFactu.Config;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Factu.Anulacion;
using VeriFactu.Xml.Soap;

namespace VeriFactu.DataStore
{

    /// <summary>
    /// Representa un periodo de facturación de un vendedor o emisor
    /// de facturas.
    /// </summary>
    public class Period
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="seller">Vendedor o emisor de facturas al
        /// que corresponde el periodo.</param>
        /// <param name="periodID">Identificador del periodo.</param>
        /// <param name="invoiceCount">Número de facturas del periodo.</param>
        public Period(Seller seller, string periodID, int invoiceCount)
        {

            Seller = seller;
            PeriodID = periodID;
            InvoiceCount = invoiceCount;

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Devuelve todos los envíos de un vendedor para un periodo
        /// determinado.
        /// </summary>
        /// <returns>Lista de documentos envíados.</returns>
        private List<Envelope> GetEnvelopes()
        {

            var envelopes = new List<Envelope>();
            var envelopeDir = $"{Settings.Current.OutboxPath}{SellerID}{Path.DirectorySeparatorChar}{PeriodID}{Path.DirectorySeparatorChar}";
            var envelopeFiles = Directory.GetFiles(envelopeDir);

            foreach (var envelopeFile in envelopeFiles)
                envelopes.Add(new Envelope(envelopeFile));

            return envelopes;

        }

        /// <summary>
        /// Diccionario de documentos.
        /// </summary>
        /// <param name="envelopes">Mensajes envíados al la AEAT.</param>
        /// <returns>Diccionario de mensajes enviados a Verifactu de la AEAT de un
        /// emisor y periodo determinados.</returns>
        private Dictionary<string, Document> GetDocuments(List<Envelope> envelopes)
        {

            var documents = new Dictionary<string, Document>();

            foreach (var envelope in envelopes)
            {

                var regFactuSistemaFacturacion = envelope.Body.Registro as RegFactuSistemaFacturacion;

                if (regFactuSistemaFacturacion == null)
                    throw new InvalidOperationException($"Error: Encontrado un envío de mensaje SOAP con propiedad Body desconocida {regFactuSistemaFacturacion}");

                var registros = regFactuSistemaFacturacion?.RegistroFactura as IList<object>;

                if (registros == null)
                    throw new InvalidOperationException($"Error: Encontrado un RegFactuSistemaFacturacion que no es una lista {regFactuSistemaFacturacion}");

                if (registros.Count == 0)
                    throw new InvalidOperationException($"Error: Encontrado un RegFactuSistemaFacturacion sin elementos {registros}");

                foreach (var registro in registros)
                {

                    var alta = registro as RegistroAlta;
                    var baja = registro as RegistroAnulacion;

                    var record = registro as Registro;

                    if (record == null)
                        throw new InvalidOperationException($"Error: Encontrado un envío de mensaje SOAP con propiedad Body desconocida {registro}");

                    if (alta != null)
                        record.IDFactura = alta.IDFacturaAlta;

                    if (baja != null)
                        record.IDFactura = baja.IDFacturaAnulada;

                    var recordID = $"{record.IDFactura}";

                    if (documents.ContainsKey(recordID))
                        documents[recordID].AddRecord(record);
                    else
                        documents.Add(recordID, new Document(this, record));


                }

            }

            return documents;

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Vendedor o emisor de facturas al que 
        /// corresponde el periodo.
        /// </summary>
        public Seller Seller { get; private set; }

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
                return Seller.SellerID; 
            }
        }

        /// <summary>
        /// Nombre del vendedor.
        /// </summary>        
        public string SellerName
        {
            get
            {
                return Seller.SellerName;
            }
        }

        /// <summary>
        /// Identificador del periodo.
        /// </summary>
        public string PeriodID { get; private set; }

        /// <summary>
        /// Número de facturas del periodo.
        /// </summary>
        public int InvoiceCount { get; private set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Recupera un diccionario de documentos del periodo
        /// cuya clave es el IDFactura.
        /// </summary>
        /// <returns>Diccionario de documentos del periodo.</returns>
        public Dictionary<string, Document> GetDocuments()
        {

            var envelopes = GetEnvelopes();
            return GetDocuments(envelopes);

        }

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns>Representación textual de la instancia.</returns>
        public override string ToString()
        {

            return $"{Seller} ({InvoiceCount:#,##0})";

        }

        #endregion

    }

}