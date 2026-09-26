/*
    This file is part of the VeriFactu (R) project.
    Copyright (c) 2024-2026 Irene Solutions SL
    Author: Irene Solutions SL.

    NO VERI*FACTU implementation developed with the valuable contribution of:
    Javier Florit González
    GAMADI CONSULTING BALEARS SL (B57336786)
    javier.florit@gamadic.com

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
using System.IO.Compression;
using System.Linq;
using VeriFactu.Config;
using VeriFactu.Xml.Factu.Evento;

namespace VeriFactu.DataStore
{
    /// <summary>
    /// Exportación de los registros de eventos correspondientes
    /// a un periodo.
    /// </summary>
    public class EventPeriodExport
    {

        #region Constructores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sellerID">Emisor.</param>
        /// <param name="period">Periodo de generación.</param>
        public EventPeriodExport(string sellerID, string period)
        {

            if (sellerID == null)
                throw new ArgumentNullException(nameof(sellerID));

            if (period == null)
                throw new ArgumentNullException(nameof(period));

            var csvDir = Path.Combine(Settings.Current.EventChainPath, sellerID);

            var csvFile = Path.Combine(csvDir, $"{period}.csv");

            if (!File.Exists(csvFile))
                throw new InvalidDataException($"No existe ningún evento para el emisor " +
                    $"'{sellerID}' y periodo '{period}'.");

            var csvLines = File.ReadAllLines(csvFile).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

            SellerID = sellerID;
            Period = period;
            CsvDir = csvDir;
            CsvFile = csvFile;
            CsvLines = csvLines;

            EventPeriodExportFiles = new List<EventPeriodExportFile>();

            ulong? previousEventChainLinkID = null;

            foreach (var csvLine in csvLines)
            {
                var file = new EventPeriodExportFile(sellerID, csvLine);

                if (previousEventChainLinkID.HasValue &&
                    file.EventChainLinkID !=
                    previousEventChainLinkID.Value + 1)
                {
                    throw new InvalidDataException($"El archivo '{file.EventFilePath}' no tiene un EventChainLinkID consecutivo " +
                        $"al anterior. Se esperaba {previousEventChainLinkID.Value + 1} y se ha encontrado {file.EventChainLinkID}.");
                }

                EventPeriodExportFiles.Add(file);

                previousEventChainLinkID = file.EventChainLinkID;

            }

            if (EventPeriodExportFiles.Count == 0)
                throw new InvalidDataException($"No existe ningún evento para el emisor " +
                    $"'{sellerID}' y periodo '{period}'.");
        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Emisor.
        /// </summary>
        public string SellerID { get; private set; }

        /// <summary>
        /// Periodo exportado.
        /// </summary>
        public string Period { get; private set; }

        /// <summary>
        /// Directorio que contiene la cadena de eventos.
        /// </summary>
        public string CsvDir { get; private set; }

        /// <summary>
        /// Archivo CSV que contiene la cadena de eventos.
        /// </summary>
        public string CsvFile { get; private set; }

        /// <summary>
        /// Líneas de la cadena de eventos.
        /// </summary>
        public IList<string> CsvLines { get; private set; }

        /// <summary>
        /// Archivos de eventos incluidos en la exportación.
        /// </summary>
        public IList<EventPeriodExportFile> EventPeriodExportFiles { get; private set; }

        /// <summary>
        /// Primer evento incluido en la exportación.
        /// </summary>
        public EventPeriodExportFile First => EventPeriodExportFiles.First();

        /// <summary>
        /// Último evento incluido en la exportación.
        /// </summary>
        public EventPeriodExportFile Last => EventPeriodExportFiles.Last();

        /// <summary>
        /// Fecha y hora inicial del periodo exportado.
        /// </summary>
        public DateTime Start => First.Created;

        /// <summary>
        /// Fecha y hora final del periodo exportado.
        /// </summary>
        public DateTime End => Last.Created;

        /// <summary>
        /// Número de registros de eventos exportados.
        /// </summary>
        public int Events => EventPeriodExportFiles.Count;

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Exporta los registros de eventos a un archivo ZIP.
        /// </summary>
        /// <param name="zipFilePath"> Ruta del archivo ZIP de destino.</param>
        public void ExportToZip(string zipFilePath)
        {

            using (var zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {

                foreach (var file in EventPeriodExportFiles)
                {
                    var entryName = Path.GetFileName(file.EventFilePath);

                    zip.CreateEntryFromFile(file.EventFilePath, entryName);

                }

            }

        }

        #endregion

    }

}