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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using VeriFactu.Config;
using System.IO;
using System.Diagnostics;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Factu.Anulacion;
using VeriFactu.Xml;

namespace VeriFactu.DataStore
{

    /// <summary>
    /// Exporta las facturas de un periodo.
    /// </summary>
    public class InvoicePeriodExport
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Bloqueo para thread safe.
        /// </summary>
        private readonly object _Locker = new object();

        /// <summary>
        /// Separador para los archivos csv.
        /// </summary>
        protected const char _CsvSeparator = ';';

        #endregion

        #region Propiedades Privadas de Instacia

        /// <summary>
        /// Directorio donde se almacenan los bloques de la
        /// cadena del emisor determinado.
        /// </summary>
        private string CsvDir { get; set; }

        /// <summary>
        /// Archivo csv de la cadena de bloques del emisor
        /// para un periodo determinado.
        /// </summary>
        private string CsvFile { get; set; }

        /// <summary>
        /// Líneas del archivo CSV.
        /// </summary>
        private string[] CsvLines { get; set; }

        #endregion

        #region Constructores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sellerID"> Emisor facturas.</param>
        /// <param name="period"> Periodo emisión.</param>
        public InvoicePeriodExport(string sellerID, string period) 
        {

            if (sellerID == null)
                throw new ArgumentNullException(nameof(sellerID));

            if (period == null)
                throw new ArgumentNullException(nameof(period));

            var csvDir = Path.Combine(Settings.Current.BlockchainPath, sellerID);
            var csvFile = Path.Combine(csvDir, $"{period}.csv");

            if (!File.Exists(csvFile))
                throw new InvalidDataException($"No existe ninguna factura" +
                    $" para el emisor '{sellerID}' y periodo '{period}'.");

            var csvLines = GetCsvLines(csvFile);

            SellerID = sellerID;
            Period = period;
            CsvDir = csvDir;
            CsvFile = csvFile;
            CsvLines = csvLines;

            InvoicePeriodExportFiles = new List<InvoicePeriodExportFile>();

            ulong? previousBlockchainLinkID = null;

            foreach (var csvLine in csvLines) 
            {

                var file = new InvoicePeriodExportFile(csvLine);

                if(previousBlockchainLinkID.HasValue && file.BlockchainLinkID != previousBlockchainLinkID.Value + 1)
                    throw new InvalidDataException($"El archivo '{file.InvoiceFilePath}'" +
                        $" no tiene un BlockchainLinkID consecutivo al anterior." +
                        $" Se esperaba {previousBlockchainLinkID.Value + 1} y se ha encontrado {file.BlockchainLinkID}.");

                InvoicePeriodExportFiles.Add(file);

                previousBlockchainLinkID = file.BlockchainLinkID;

            }
               

            SetSumProps();

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Devuelve las líneas de un archivo csv.
        /// </summary>
        /// <param name="csvFile">Ruta al archivo csv.</param>
        /// <returns>Líneas del archivo csv.</returns>
        private string[] GetCsvLines(string csvFile) 
        {

            string[] csvLines;

            lock (_Locker)
                csvLines = File.ReadAllLines(csvFile);


            if (csvLines.Length == 0)
                throw new InvalidDataException($"No existen líneas" +
                    $" el el archivo '{csvFile}'.");

            return csvLines;

        }

        /// <summary>
        /// Establece el valor de TotalTaxAmount y TotalAmount.
        /// </summary>
        private void SetSumProps() 
        {

            foreach (var file in InvoicePeriodExportFiles)
            {
                if (file.Record is RegistroAlta alta)
                {
                    Registrations++;

                    TotalTaxAmount += XmlParser.ToDecimal(alta.CuotaTotal);

                    TotalAmount += XmlParser.ToDecimal(alta.ImporteTotal);

                }
                else if (file.Record is RegistroAnulacion)
                {
                    Cancellations++;
                }
            }

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Emisor facturas.
        /// </summary>
        public string SellerID { get; private set; }

        /// <summary>
        /// Periodo.
        /// </summary>
        public string Period { get; private set; }

        /// <summary>
        /// Obtiene la lista de archivos de facturas del periodo.
        /// </summary>
        public List<InvoicePeriodExportFile> InvoicePeriodExportFiles { get; private set; }

        /// <summary>
        /// Fecha creación primera factura del periodo.
        /// </summary>
        public DateTime Start => InvoicePeriodExportFiles.First().Created;

        /// <summary>
        /// Fecha creación última factura del periodo.
        /// </summary>
        public DateTime End => InvoicePeriodExportFiles.Last().Created;

        /// <summary>
        /// Primer archivo de facturas del periodo.
        /// </summary>
        public InvoicePeriodExportFile First => InvoicePeriodExportFiles.First();

        /// <summary>
        ///´Último archivo de facturas del periodo.
        /// </summary>
        public InvoicePeriodExportFile Last => InvoicePeriodExportFiles.Last();

        /// <summary>
        /// Número de registros de alta.
        /// </summary>
        public int Registrations { get; private set; }

        /// <summary>
        /// Número de registros de anulación.
        /// </summary>
        public int Cancellations { get; private set; }

        /// <summary>
        /// Total de la cuota soportada del conjunto de registros de alta
        /// del periodo sin tener en cuenta los registros de anulación.
        /// </summary>
        public decimal TotalTaxAmount { get; private set; }

        /// <summary>
        /// Total de la importe total del conjunto de registros de alta
        /// del periodo sin tener en cuenta los registros de anulación.
        /// </summary>
        public decimal TotalAmount { get; private set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Exporta las facturas del periodo a un archivo ZIP.
        /// </summary>
        /// <param name="zipFilePath">Ruta del archivo comprimido a guardar.</param>
        public void ExportToZip(string zipFilePath)
        {

            // Abrimos o creamos el archivo ZIP en modo Create
            using (ZipArchive archivoZip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {

                foreach (var invoicePeriodExportFile in InvoicePeriodExportFiles)
                {

                    // Obtenemos solo el nombre del archivo para la entrada del ZIP
                    string nombreEntrada = Path.GetFileName(invoicePeriodExportFile.InvoiceFilePath);

                    // Añadimos el archivo al contenedor ZIP
                    archivoZip.CreateEntryFromFile(invoicePeriodExportFile.InvoiceFilePath, nombreEntrada);

                }

            }

        }

        /// <summary>
        /// Representación en cadena de la instancia.
        /// </summary>
        /// <returns>Representación en cadena de la instancia.</returns>
        public override string ToString()
        {

            return $"{SellerID}, {Period}, {InvoicePeriodExportFiles.Count}";

        }

        #endregion

    }

}
