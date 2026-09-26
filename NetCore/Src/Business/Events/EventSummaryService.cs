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
using System.Linq;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Factu.Anulacion;
using VeriFactu.Xml.Factu.Evento;

namespace VeriFactu.Business.Events
{

    /// <summary>
    /// Servicio para obtener el resumen de eventos de un vendedor.
    /// </summary>
    internal static class EventSummaryService
    {

        #region Métodos Privados Estáticos

        /// <summary>
        /// Agrega el resumen de facturación al resultado del resumen de eventos.
        /// </summary>
        /// <param name="sellerID">Id. del vendedor.</param>
        /// <param name="events">Eventos.</param>
        /// <param name="result">Resultado del resumen de eventos.</param>
        private static void AddInvoiceSummary(string sellerID, IList<Evento> events, EventSummaryResult result)
        {

            var records = EventAuditService
                .GetInvoiceRecords(sellerID)
                .OrderBy(EventAuditService.GetBlockchainLinkID)
                .ToList();

            var lastLinkID = GetLastInvoiceLinkID(events, records);

            var periodRecords = records
                .Where(r =>
                    EventAuditService.GetBlockchainLinkID(r) >
                    lastLinkID)
                .ToList();

            if (periodRecords.Count == 0)
                return;

            result.FirstInvoiceRecord = periodRecords.First();

            result.LastInvoiceRecord = periodRecords.Last();

            foreach (var record in periodRecords)
            {

                var alta = record as RegistroAlta;

                if (alta != null)
                {

                    result.InvoiceRegistrations++;

                    result.TotalTaxAmount +=
                        XmlParser.ToDecimal(
                            alta.CuotaTotal);

                    result.TotalAmount +=
                        XmlParser.ToDecimal(
                            alta.ImporteTotal);

                    continue;

                }

                if (record is RegistroAnulacion)
                    result.InvoiceCancellations++;

            }

        }

        /// <summary>
        /// Obtiene el ID de enlace de blockchain del último registro
        /// de facturación incluido en un resumen anterior.
        /// </summary>
        /// <param name="events">Eventos.</param>
        /// <param name="records">Registros de facturación.</param>
        /// <returns> ID de enlace de blockchain del último registro de facturación
        /// incluido en un resumen anterior. Cero si no existe ninguno.</returns>
        /// <exception cref="InvalidOperationException">
        /// Se produce cuando un resumen anterior identifica un registro
        /// de facturación que no puede localizarse.
        /// </exception>
        private static ulong GetLastInvoiceLinkID(
            IList<Evento> events,
            IList<Registro> records)
        {

            var summaries = events
                .Where(e =>
                    e.TipoEvento == TipoEvento.ResumenEventos)
                .OrderByDescending(e =>
                    e.EventChainLinkID);

            foreach (var evento in summaries)
            {

                var summary = evento.DatosPropiosEvento?.DatosEvento as ResumenEventos;

                var last = summary?.RegistroFacturacionFinalPeriodo;

                /*
                 * Puede haber resúmenes posteriores sin registros
                 * de facturación. En ese caso se continúa buscando
                 * hacia atrás hasta encontrar el último resumen que
                 * contenga un registro final de facturación.
                 */
                if (last == null)
                    continue;

                var record = records
                    .FirstOrDefault(r =>
                        IsSameInvoiceRecord(
                            r,
                            last));

                if (record == null)
                    throw new InvalidOperationException("No se ha podido localizar el último registro " +
                        "de facturación incluido en un resumen anterior.");

                return EventAuditService.GetBlockchainLinkID(record);

            }

            return 0;

        }

        /// <summary>
        /// Determina si un registro de facturación es el mismo
        /// que un registro de referencia.
        /// </summary>
        /// <param name="record">Registro a comparar.</param>
        /// <param name="reference">Registro de referencia.</param>
        /// <returns> True si ambos identifican el mismo registro de facturación.</returns>
        private static bool IsSameInvoiceRecord(Registro record, IDFacturaExpedidaHuella reference)
        {

            if (record?.IDFactura == null || reference == null)
                return false;

            return
                string.Equals(record.IDFactura.IDEmisor, reference.IDEmisorFactura, StringComparison.Ordinal) &&

                string.Equals(record.IDFactura.NumSerie, reference.NumSerieFactura, StringComparison.Ordinal) &&

                string.Equals(record.IDFactura.FechaExpedicion, reference.FechaExpedicionFactura, StringComparison.Ordinal) &&

                string.Equals(record.Huella, reference.Huella, StringComparison.OrdinalIgnoreCase);

        }

        #endregion

        #region Métodos Internos Estáticos

        /// <summary>
        /// Obtiene el resumen de eventos de un vendedor.
        /// </summary>
        /// <param name="sellerID">Id. del vendedor.</param>
        /// <returns>Resumen de eventos del vendedor.</returns>
        internal static EventSummaryResult GetSummary(string sellerID)
        {

            var result = new EventSummaryResult();

            var events = EventAuditService
                .GetEventRecords(sellerID)
                .OrderBy(e =>
                    e.EventChainLinkID)
                .ToList();

            var lastSummary = events
                .LastOrDefault(e =>
                    e.TipoEvento ==
                    TipoEvento.ResumenEventos);

            /*
             * El periodo de eventos comienza con el último
             * registro resumen de eventos.
             *
             * Si no existe ningún resumen anterior, se incluyen
             * todos los eventos existentes.
             */
            var periodEvents =
                lastSummary == null
                    ? events
                    : events
                        .Where(e =>
                            e.EventChainLinkID >=
                            lastSummary.EventChainLinkID)
                        .ToList();

            foreach (var evento in periodEvents)
            {

                if (!result.Events.ContainsKey(evento.TipoEvento))
                    result.Events.Add(evento.TipoEvento, 0);

                result.Events[evento.TipoEvento]++;

            }

            AddInvoiceSummary(sellerID, events, result);

            return result;

        }

        #endregion

    }

}