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
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VeriFactu.Business;
using VeriFactu.Business.Operations;
using VeriFactu.Common;
using VeriFactu.Config;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Factu.Anulacion;
using VeriFactu.Xml.Soap;

namespace VeriFactu.DataStore
{

    /// <summary>
    /// Representa un archivo de exportación de facturas de un periodo.
    /// </summary>
    public class InvoicePeriodExportFile
    {

        #region Propiedades Privadas de Instacia

        /// <summary>
        /// Línea CSV que representa el archivo de exportación de facturas de un periodo.
        /// </summary>

        private string CsvLine { get; set; }

        /// <summary>
        /// Valores de la línea CSV que representan el archivo de exportación de facturas de un periodo.
        /// </summary>
        private string[] CsvLineValues { get; set; }

        #endregion

        #region Constructores de Instancia

        /// <summary>
        /// Crea una nueva instancia de la clase InvoicePeriodExportFile a partir de una línea CSV.
        /// </summary>
        /// <param name="csvLine"></param>
        public InvoicePeriodExportFile(string csvLine)
        {


            if (csvLine == null)
                throw new ArgumentNullException(nameof(csvLine));

            var csvLineValues = csvLine.Split(';');

            if (csvLineValues.Length != 7)
                throw new ArgumentException($"Número erróneo columnas en línea CSV" +
                    $" de entrada:\n{csvLine}.\nEncontradas {csvLineValues.Length} cuando deberían ser 7.");


            CsvLine = csvLine;
            CsvLineValues = csvLineValues;
            HashTextInput = csvLineValues[6].Substring(1, csvLineValues[6].Length - 2);

            IsInvoiceCancellation = Regex.Match(HashTextInput, @"(?<=^IDEmisorFacturaAnulada=)[^&]+").Success;

            var sufixExpr = IsInvoiceCancellation ? "Anulada" : "";

            SellerID = Regex.Match(HashTextInput, @"(?<=^IDEmisorFactura" + sufixExpr + @"=)[^&]+").Value;
            InvoiceID = Regex.Match(HashTextInput, @"(?<=&NumSerieFactura" + sufixExpr + @"=)[^&]+").Value;
            InvoiceDate = XmlParser.ToDate(Regex.Match(HashTextInput, @"(?<=&FechaExpedicionFactura" + sufixExpr + @"=)[^&]+").Value);
            Created = XmlParser.ToDate(Regex.Match(HashTextInput, @"(?<=&FechaHoraHusoGenRegistro=)[^&]+").Value);

            string encodedInvoiceID = null;

            if (IsInvoiceCancellation)
                encodedInvoiceID = Utils.GetEncodedToHex($"{InvoiceID}.DEL");
            else
                encodedInvoiceID = new InvoiceActionData(InvoiceID, InvoiceDate, SellerID).EncodedInvoiceID;

            InvoiceFilePath = GetInvoiceFilePath(encodedInvoiceID);

            if (!File.Exists(InvoiceFilePath))
                throw new InvalidDataException($"No existe el archivo de factura '{InvoiceFilePath}'" +
                    $" para la línea CSV:\n{csvLine}.");

            var record = Utils.GetRecord(InvoiceFilePath);

            if (record == null)
                throw new ArgumentNullException(nameof(record));

            var refExterna = IsInvoiceCancellation ? (record as RegistroAnulacion).RefExterna : (record as RegistroAlta).RefExterna;

            if (string.IsNullOrEmpty(refExterna))
                throw new InvalidDataException($"El archivo de factura '{InvoiceFilePath}' no contiene " +
                    $"el tipo de registro esperado.");

            if (Convert.ToUInt64(refExterna) != BlockchainLinkID)
                throw new InvalidDataException($"El ID de enlace '{BlockchainLinkID}' de la línea CSV:\n{csvLine}\n" +
                    $"no coincide con el ID de enlace '{refExterna}' del archivo de factura '{InvoiceFilePath}'.");

            Record = record;

        }

        #endregion

        #region Métodos Privados de Instancia


        /// <summary>
        /// Busca el archivo de factura en el directorio del emisor y año de la factura a partir del ID de factura codificado.
        /// </summary>
        /// <param name="encodedInvoiceID">ID de factura codificado.</param>
        /// <returns>Ruta archivo encontrado</returns>
        /// <exception cref="Exception"> No encontrado el archivo.</exception>
        /// <exception cref="ArgumentOutOfRangeException"> Más de un archivo encontrado.</exception>
        private string GetInvoiceFilePath(string encodedInvoiceID) 
        {

            var files = Directory.GetFiles(InvoiceSellerYearDir, $"{encodedInvoiceID}*.xml", SearchOption.TopDirectoryOnly);

            if (files.Length == 0)
                throw new InvalidDataException( $"No existe ningún archivo de factura para el identificador '{encodedInvoiceID}' " +
                    $"en el directorio '{InvoiceSellerYearDir}'.");

            if (files.Length > 1)
                return GetMatchInvoiceFilePath(files);

            return files[0];
        
        }

        /// <summary>
        /// Selecciona de entre una lista de archivos el que tiene la referencia
        /// que se corresponde con el id de la cadena de bloques.
        /// </summary>
        /// <param name="files"> Matriz de archivos.</param>
        /// <returns> Ruta del archivo que se corresponde.</returns>
        private string GetMatchInvoiceFilePath(string[] files) 
        {

            Dictionary<ulong, string> fileMap = new Dictionary<ulong, string>();

            foreach (var file in files) 
            {

                var linkID = GetBlockchainLinkID(file);

                if (fileMap.ContainsKey(linkID))
                    throw new ArgumentOutOfRangeException(nameof(file), $"Existen {fileMap.Count} archivos de factura con el mismo ID de enlace '{linkID}' en el directorio '{InvoiceSellerYearDir}'.");

                fileMap.Add(linkID, file);

            }

            if(!fileMap.ContainsKey(BlockchainLinkID))
                throw new Exception($"No existe ningún archivo de factura con el ID de enlace '{BlockchainLinkID}' en el directorio '{InvoiceSellerYearDir}'.");

            return fileMap[BlockchainLinkID];

        }

        /// <summary>
        /// Recupera el ID del enlace de la cadena de bloques a partir de un archivo XML.
        /// </summary>
        /// <param name="xmlPath">Ruta al archivo xml.</param>
        /// <returns> Id. del enlace.</returns>
        private ulong GetBlockchainLinkID(string xmlPath) 
        {

            var record = string.Equals(InvoiceFilePath, xmlPath, StringComparison.OrdinalIgnoreCase) ? Record : Utils.GetRecord(xmlPath);

            var alta = record as RegistroAlta;
            var anulacion = record as RegistroAnulacion;

            var refExterna =  $"{anulacion?.RefExterna}{alta?.RefExterna}";

            return Convert.ToUInt64(refExterna);        

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Obtiene el ID del enlace de la cadena de bloques a partir de la línea CSV.
        /// </summary>
        public ulong BlockchainLinkID => Convert.ToUInt64(CsvLineValues[0]);

        /// <summary>
        /// Directorio de almacenamiento de facturas del emisor.
        /// </summary>
        public string InvoiceSellerDir => Path.Combine(Settings.Current.InvoicePath, SellerID);

        /// <summary>
        /// Directorio de almacenamiento de facturas del emisor
        /// para el año de la factura.
        /// </summary>
        public string InvoiceSellerYearDir => Path.Combine(InvoiceSellerDir, $"{Created.Year}");

        /// <summary>
        /// Obtiene el ID del periodo de facturación a partir de la línea CSV.
        /// </summary>
        public string HashTextInput { get; private set; }

        /// <summary>
        /// Número factura.
        /// </summary>
        public string InvoiceID { get; private set; }

        /// <summary>
        /// Id. emisor factura.
        /// </summary>
        public string SellerID { get; private set; }

        /// <summary>
        /// Fecha factura.
        /// </summary>
        public DateTime InvoiceDate { get; private set; }

        /// <summary>
        /// Fecha creacion.
        /// </summary>
        public DateTime Created { get; private set; }

        /// <summary>
        /// Ruta al archivo de factura.
        /// </summary>
        public string InvoiceFilePath { get; private set; }

        /// <summary>
        /// Indicador de anulación.
        /// Es true si se trata de una anulación
        /// de factura.
        /// </summary>
        public bool IsInvoiceCancellation { get; private set; }

        /// <summary>
        /// Registro del archivo de factura.
        /// </summary>
        public Registro Record { get; private set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representacion textual de la instancia.
        /// </summary>
        /// <returns>Representacion textual de la instancia.</returns>
        public override string ToString()
        {

            return $"{BlockchainLinkID}, {InvoiceFilePath}";

        }

        #endregion

    }

}
