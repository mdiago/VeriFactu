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
using VeriFactu.Config;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Factu.Anulacion;
using VeriFactu.Xml.Soap;

namespace VeriFactu.DataStore
{

    /// <summary>
    /// Representa un bandeja de entrada o salida de documentos del sistema.
    /// </summary>
    public class PeriodBox
    {        

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="seller">Vendedor o emisor de facturas al
        /// que corresponde el periodo.</param>
        /// <param name="periodID">Identificador del periodo.</param>
        /// <param name="invoiceCount">Número de facturas del periodo.</param>
        public PeriodBox(Seller seller, string periodID, int invoiceCount)
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
            var envelopeFiles = Directory.GetFiles(EnvelopeDir);

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
        internal Dictionary<string, DocumentBox> GetDocuments(List<Envelope> envelopes)
        {

            var documents = new Dictionary<string, DocumentBox>();

            foreach (var envelope in envelopes)
            {

                var registros = GetInvoiceRecords(envelope);

                foreach (var registro in registros)
                {

                    var recordID = SetRegistro(registro);

                    if (documents.ContainsKey(recordID))
                        documents[recordID].AddRecord(registro);
                    else
                        documents.Add(recordID, new DocumentBox(this, registro));


                }

            }

            return documents;

        }

        /// <summary>
        /// Devuelve una lista de objetos relacionados cada uno con
        /// una factura.
        /// </summary>
        /// <param name="envelope">Sobre SOAP.</param>
        /// <returns>lista de objetos relacionados cada uno con
        /// una factura.</returns>
        internal virtual IList<object> GetInvoiceRecords(Envelope envelope) 
        {

            var result = new List<object>();

            var regFactuSistemaFacturacion = envelope.Body.Registro as RegFactuSistemaFacturacion;

            if (regFactuSistemaFacturacion == null)
                throw new InvalidOperationException($"Error: Encontrado un envío de mensaje SOAP con propiedad Body desconocida {regFactuSistemaFacturacion}");

            var registros = regFactuSistemaFacturacion?.RegistroFactura as IList<RegistroFactura>;

            if (registros == null)
                throw new InvalidOperationException($"Error: Encontrado un RegFactuSistemaFacturacion que no es una lista {regFactuSistemaFacturacion}");

            if (registros.Count == 0)
                throw new InvalidOperationException($"Error: Encontrado un RegFactuSistemaFacturacion sin elementos {registros}");

            foreach ( var registro in registros)
                result.Add(registro.Registro);

            return result;

        }

        /// <summary>
        /// Actualiza el campo de IDFactura en el registro que
        /// se pasa como parámetro, y devuelve un identificador
        /// basado en el mismo.
        /// </summary>
        /// <param name="record">Registro a actualizar.</param>
        /// <returns>Identificador basado en el IDFactura.</returns>
        internal virtual string SetRegistro(object record) 
        {

            var registro = record as Registro;

            if (registro == null)
                throw new InvalidOperationException($"Error: Encontrado un envío de mensaje SOAP con propiedad Body desconocida {registro}");

            var alta = registro as RegistroAlta;
            var baja = registro as RegistroAnulacion;


            if (alta != null)
                registro.IDFactura = alta.IDFacturaAlta;

            if (baja != null)
                registro.IDFactura = baja.IDFacturaAnulada;
        

            return $"{registro.IDFactura}";        


        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Ruta a la bandeja. Por defecto la bandeja de salida
        /// de registros.
        /// </summary>
        public virtual string EnvelopeDir => $"{Settings.Current.OutboxPath}{SellerID}{Path.DirectorySeparatorChar}{PeriodID}{Path.DirectorySeparatorChar}";

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
        /// Recupera un diccionario de documentos envíados del periodo
        /// cuya clave es el IDFactura.
        /// </summary>
        /// <returns>Diccionario de documentos del periodo.</returns>
        public Dictionary<string, DocumentBox> GetDocuments()
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
