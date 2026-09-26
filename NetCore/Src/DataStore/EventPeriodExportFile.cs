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
using System.IO;
using VeriFactu.Common;
using VeriFactu.Config;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu.Evento;

namespace VeriFactu.DataStore
{

    /// <summary>
    /// Registro de evento incluido en una exportación de periodo.
    /// </summary>
    public class EventPeriodExportFile
    {

        #region Constructores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sellerID">Emisor.</param>
        /// <param name="csvLine"> Línea correspondiente de la cadena de eventos.</param>
        public EventPeriodExportFile(string sellerID, string csvLine)
        {

            if (sellerID == null)
                throw new ArgumentNullException(nameof(sellerID));

            if (csvLine == null)
                throw new ArgumentNullException(nameof(csvLine));

            var values =
                csvLine.Split(';');

            EventChainLinkID = Convert.ToUInt64(values[0]);

            Created = XmlParser.ToDate(values[1]);

            EventFilePath = Path.Combine(Settings.Current.EventPath, sellerID, $"{Created.Year}", $"{EventChainLinkID:00000000000000000000}.xml");

            if (!File.Exists(EventFilePath))
                throw new InvalidDataException($"No existe el archivo de evento '{EventFilePath}' correspondiente al " +
                    $"eslabón '{EventChainLinkID}'.");

            var evento = Utils.GetEventRecord(EventFilePath);

            if (evento == null)
                throw new ArgumentNullException(nameof(evento));

            if (evento.EventChainLinkID != EventChainLinkID)
                throw new InvalidDataException($"El ID de enlace '{EventChainLinkID}' de la línea CSV:\n{csvLine}\n" +
                    $"no coincide con el ID de enlace '{evento.EventChainLinkID}' del archivo " +
                    $"de evento '{EventFilePath}'.");

            Event = evento;

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Identificador del eslabón de la cadena de eventos.
        /// </summary>
        public ulong EventChainLinkID { get; private set; }

        /// <summary>
        /// Fecha y hora de generación del evento.
        /// </summary>
        public DateTime Created { get; private set; }

        /// <summary>
        /// Ruta del archivo XML del evento.
        /// </summary>
        public string EventFilePath { get; private set; }

        /// <summary>
        /// Evento almacenado.
        /// </summary>
        public Evento Event { get; private set; }

        #endregion

    }

}