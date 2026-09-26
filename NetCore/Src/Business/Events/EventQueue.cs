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
using System.Diagnostics;
using System.Linq;
using VeriFactu.Business.FlowControl;
using VeriFactu.Common;
using VeriFactu.Config;
using VeriFactu.DataStore;
using VeriFactu.Xml.Factu.Evento;

namespace VeriFactu.Business.Events
{

    /// <summary>
    /// Gestiona en segundo plano la generación y registro
    /// de eventos SIF.
    /// </summary>
    internal class EventQueue : IntervalWorker
    {

        #region Variables Privadas Estáticas

        /// <summary>
        /// Duración en horas del periodo de generación
        /// del resumen de eventos.
        /// </summary>
        const int SummaryPeriodHours = 6;

        #endregion

        #region Variables Privadas de Instancia

        /// <summary>
        /// Cola de eventos pendientes de procesamiento.
        /// </summary>
        readonly Queue<EventEntry> _PendingQueue;

        /// <summary>
        /// Bloqueo para acceso thread safe a la cola.
        /// </summary>
        readonly object _Locker = new object();

        /// <summary>
        /// Indica si la cola se está procesando.
        /// </summary>
        bool _IsWorking;

        /// <summary>
        /// Indica si la cola se está cerrando.
        /// </summary>
        bool _IsClosing;

        /// <summary>
        /// Momento de inicio del periodo actual de resumen
        /// de eventos.
        /// </summary>
        DateTime _SummaryPeriodStart;

        /// <summary>
        /// Indica que existe un ciclo periódico de resumen
        /// pendiente de procesamiento.
        /// </summary>
        bool _SummaryPending;

        #endregion

        #region Constructores Estáticos

        /// <summary>
        /// Constructor.
        /// </summary>
        static EventQueue()
        {

            ActiveEventQueue = new EventQueue();
            ActiveEventQueue.Start();

            AddStartupEvents();

        }

        #endregion

        #region Constructores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        private EventQueue()
        {

            _PendingQueue = new Queue<EventEntry>();
            _SummaryPeriodStart = DateTime.Now;

        }

        #endregion

        #region Propiedades Públicas Estáticas

        /// <summary>
        /// Indica que la cola de eventos ha sido inicializada.
        /// </summary>
        internal static bool Initialized => true;

        /// <summary>
        /// Cola de eventos activa en el sistema.
        /// </summary>
        internal static EventQueue ActiveEventQueue { get; private set; }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Número de eventos pendientes de procesamiento.
        /// </summary>
        internal int Count
        {
            get
            {

                lock (_Locker)
                    return _PendingQueue.Count;

            }
        }

        #endregion

        #region Métodos Privados Estáticos

        /// <summary>
        /// Solicita la finalización de la cola de eventos.
        /// El cierre se realizará una vez procesados todos
        /// los eventos pendientes.
        /// </summary>
        internal static void Exit()
        {

            var queue = ActiveEventQueue;

            if (queue == null)
                return;

            lock (queue._Locker)
            {

                if (queue._IsClosing)
                    return;

                var sellers =
                    EventAuditService
                        .GetInvoiceSellers()
                        .ToList();

                foreach (var sellerID in sellers)
                {

                    var sellerName =
                        EventAuditService.GetInvoiceSellerName(
                            sellerID);

                    // Evento 10: resumen del periodo.
                    queue.AddEventSummary(
                        sellerID,
                        sellerName);

                    // Evento 02: fin del funcionamiento NO VERI*FACTU.
                    queue.Add(
                        Xml.Factu.Evento.TipoEvento.FinNoVerifactu,
                        sellerID,
                        sellerName);

                }

                queue._IsClosing = true;

                if (queue._PendingQueue.Count == 0 &&
                    !queue._IsWorking)
                    DoExit();

            }

        }
        
        /// <summary>
        /// Añade los eventos correspondientes al inicio del sistema
        /// para todos los emisores existentes.
        /// </summary>
        private static void AddStartupEvents()
        {

            var sellers = EventAuditService.GetInvoiceSellers();

            foreach (var sellerID in sellers)
            {

                var sellerName = EventAuditService.GetInvoiceSellerName(sellerID);

                if (sellerName == null) 
                    throw new InvalidOperationException($"No se ha podido obtener el nombre del emisor {sellerID}.");

                ActiveEventQueue.Add(
                        Xml.Factu.Evento.TipoEvento.InicioNoVerifactu,
                        sellerID,
                        sellerName);

                ActiveEventQueue.AddInvoiceAnomalyDetection(
                    sellerID,
                    sellerName);

                ActiveEventQueue.AddEventAnomalyDetection(
                    sellerID,
                    sellerName);               

            }

        }

        /// <summary>
        /// Exporta los registros de facturación de un periodo y registra
        /// el evento de exportación correspondiente.
        /// </summary>
        /// <param name="sellerID">Emisor de las facturas.</param>
        /// <param name="sellerName">Nombre del emisor.</param>
        /// <param name="period">Periodo a exportar.</param>
        /// <param name="zipFilePath">Archivo ZIP de destino.</param>
        internal static void ExportInvoicePeriod(string sellerID, string sellerName, string period, string zipFilePath)
        {
            var queue = ActiveEventQueue;

            if (queue == null)
                throw new InvalidOperationException(
                    "La cola de eventos no está inicializada.");

            lock (queue._Locker)
            {
                if (queue._IsClosing)
                    throw new InvalidOperationException(
                        "La cola de eventos se está cerrando.");

                var export =
                    new InvoicePeriodExport(
                        sellerID,
                        period);

                export.ExportToZip(zipFilePath);

                var data = EventFactory.CreateInvoicePeriodExportData(export);

                queue.Add(
                    TipoEvento.ExportacionRegFacturacionPeriodo,
                    sellerID,
                    sellerName,
                    data);
            }
        }

        /// <summary>
        /// Exporta los registros de evento de un periodo y registra
        /// el evento de exportación correspondiente.
        /// </summary>
        /// <param name="sellerID">Identificador del emisor.</param>
        /// <param name="sellerName">Nombre del emisor.</param>
        /// <param name="period">Periodo a exportar.</param>
        /// <param name="zipFilePath">Ruta del archivo ZIP de destino.</param>
        internal static void ExportEventPeriod(string sellerID, string sellerName, string period, string zipFilePath)
        {

            var queue = ActiveEventQueue;

            if (queue == null)
                throw new InvalidOperationException(
                    "La cola de eventos no está inicializada.");

            lock (queue._Locker)
            {

                if (queue._IsClosing)
                    throw new InvalidOperationException(
                        "La cola de eventos se está cerrando.");

                var export = new EventPeriodExport(sellerID, period);

                export.ExportToZip(zipFilePath);

                var data = EventFactory.CreateEventPeriodExportData(export);

                queue.Add(
                    TipoEvento.ExportacionRegEventoPeriodo,
                    sellerID,
                    sellerName,
                    data);

            }

        }

        /// <summary>
        /// Registra un evento voluntario del sistema informático.
        /// </summary>
        /// <param name="sellerID">Identificador del emisor.</param>
        /// <param name="sellerName">Nombre del emisor.</param>
        /// <param name="otrosDatosEvento">Descripción del evento.</param>
        internal static void AddOtherEvent(string sellerID, string sellerName, string otrosDatosEvento)
        {

            if (string.IsNullOrWhiteSpace(otrosDatosEvento))
                throw new ArgumentException(
                    "La descripción del evento no puede estar vacía.",
                    nameof(otrosDatosEvento));

            if (otrosDatosEvento.Length > 100)
                throw new ArgumentException(
                    "La descripción del evento no puede superar los 100 caracteres.",
                    nameof(otrosDatosEvento));

            var queue = ActiveEventQueue;

            if (queue == null)
                throw new InvalidOperationException(
                    "La cola de eventos no está inicializada.");

            lock (queue._Locker)
            {

                if (queue._IsClosing)
                    throw new InvalidOperationException(
                        "La cola de eventos se está cerrando.");

                queue.Add(
                    TipoEvento.Otros,
                    sellerID,
                    sellerName,
                    otrosDatosEvento: otrosDatosEvento);

            }

        }

        /// <summary>
        /// Finaliza definitivamente la cola de eventos.
        /// </summary>
        private static void DoExit()
        {

            if (ActiveEventQueue == null)
                return;

            if (ActiveEventQueue.Count > 0 ||
                ActiveEventQueue._IsWorking)
                throw new OperationCanceledException(
                    "No se puede finalizar la cola de eventos mientras" +
                    " existan eventos pendientes o se esté procesando.");

            ActiveEventQueue.End();
            ActiveEventQueue = null;

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Procesa un evento pendiente.
        /// </summary>
        /// <param name="entry">Entrada de evento a procesar.</param>
        private void Process(EventEntry entry)
        {

            if (entry.IsEventAnomalyDetection)
            {
                ProcessEventAnomalyDetection(entry);
                return;
            }

            if (entry.IsEventSummary)
            {
                ProcessEventSummary(entry);
                return;
            }

            ProcessEvent(entry);

        }

        /// <summary>
        /// Genera, encadena, firma y almacena un evento.
        /// </summary>
        /// <param name="entry">Entrada de evento a procesar.</param>
        private void ProcessEvent(EventEntry entry)
        {

            // Creo el evento
            var evento = EventFactory.Create(
                entry.TipoEvento,
                entry.SellerID,
                entry.SellerName,
                entry.DatosPropiosEvento,
                entry.OtrosDatosEvento);

            // Añado el evento a la cadena
            EventChain.EventChain.Get(entry.SellerID).Add(evento);

            // Creo el registro de evento
            var registroEvento = new Xml.Factu.Evento.RegistroEvento()
            {
                IDVersion = Settings.Current.IDVersion,
                Evento = evento
            };

            // Compruebo el certificado
            var cert = Net.Wsd.GetCheckedCertificate();

            if (cert == null)
                throw new Exception(
                    "Existe algún problema con el certificado.");

            // Firmo el registro de evento
            var signer =
                new NoVeriFactu.Signature.Signer(cert);

            var xml = signer.Sign(registroEvento);

            // Almaceno el XML firmado
            var eventData = new EventData(
                entry.SellerID,
                evento);

            System.IO.File.WriteAllBytes(
                eventData.EventFilePath,
                xml);

        }

        /// <summary>
        /// Ejecuta el proceso de detección de anomalías
        /// sobre los registros de evento.
        /// </summary>
        /// <param name="entry">Representa una entrada pendiente de
        /// procesamiento en la cola de eventos SIF.</param>
        private void ProcessEventAnomalyDetection(EventEntry entry)
        {

            var result =
                EventAuditService.AuditEvents(
                    entry.SellerID);

            // Evento 05: lanzamiento del proceso.
            var startEvent =
                EventFactory.CreateStartEventAnomalyDetection(
                    entry.SellerID,
                    entry.SellerName,
                    result);

            ProcessEvent(
                new EventEntry(
                    startEvent.TipoEvento,
                    entry.SellerID,
                    entry.SellerName,
                    startEvent.DatosPropiosEvento,
                    startEvent.OtrosDatosEvento));

            // Eventos 06: anomalías detectadas.
            foreach (var anomaly in result.Anomalies)
            {

                var anomalyEvent =
                    EventFactory.CreateEventAnomalyDetection(
                        entry.SellerID,
                        entry.SellerName,
                        anomaly);

                ProcessEvent(
                    new EventEntry(
                        anomalyEvent.TipoEvento,
                        entry.SellerID,
                        entry.SellerName,
                        anomalyEvent.DatosPropiosEvento,
                        anomalyEvent.OtrosDatosEvento));

            }

        }
        /// <summary>
        /// Genera el resumen de eventos correspondiente al periodo
        /// transcurrido desde el último resumen.
        /// </summary>
        /// <param name="entry">
        /// Entrada pendiente de procesamiento en la cola de eventos SIF.
        /// </param>
        private void ProcessEventSummary(EventEntry entry)
        {

            var result =
                EventSummaryService.GetSummary(
                    entry.SellerID);

            var summaryEvent =
                EventFactory.CreateEventSummary(
                    entry.SellerID,
                    entry.SellerName,
                    result);

            ProcessEvent(
                new EventEntry(
                    summaryEvent.TipoEvento,
                    entry.SellerID,
                    entry.SellerName,
                    summaryEvent.DatosPropiosEvento,
                    summaryEvent.OtrosDatosEvento));

        }

        /// <summary>
        /// Comprueba si ha transcurrido el periodo establecido
        /// para generar un resumen de eventos.
        /// </summary>
        private void CheckEventSummary()
        {

            var now = DateTime.Now;

            lock (_Locker)
            {

                if (_IsClosing || _SummaryPending)
                    return;

                if (now - _SummaryPeriodStart <
                    TimeSpan.FromHours(SummaryPeriodHours))
                    return;

                var sellers =
                     EventAuditService
                         .GetInvoiceSellers()
                         .ToList();

                if (sellers.Count == 0)
                {
                    _SummaryPeriodStart = now;
                    return;
                }

                foreach (var sellerID in sellers)
                {

                    var sellerName =
                        EventAuditService.GetInvoiceSellerName(
                            sellerID);

                    AddEventSummary(
                        sellerID,
                        sellerName);

                }

                _SummaryPending = true;
                _SummaryPeriodStart = now;

            }

        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Añade un evento a la cola de procesamiento.
        /// </summary>
        /// <param name="tipoEvento">Tipo de evento.</param>
        /// <param name="sellerID">Identificador del emisor.</param>
        /// <param name="sellerName">Nombre o razón social del emisor.</param>
        /// <param name="datosPropiosEvento">Datos propios del evento.</param>
        /// <param name="otrosDatosEvento">Otros datos del evento.</param>
        internal void Add(Xml.Factu.Evento.TipoEvento tipoEvento, string sellerID,
            string sellerName, Xml.Factu.Evento.DatosPropiosEvento datosPropiosEvento = null,
            string otrosDatosEvento = null)
        {

            var entry = new EventEntry(
                tipoEvento,
                sellerID,
                sellerName,
                datosPropiosEvento,
                otrosDatosEvento);

            lock (_Locker)
            {

                if (_IsClosing)
                    throw new InvalidOperationException("No se pueden añadir eventos" +
                        " ya que se ha establecido el cierre.");

                _PendingQueue.Enqueue(entry);

            }

        }

        /// <summary>
        /// Ejecuta el proceso de detección de anomalías sobre los registros
        /// de facturación y añade a la cola los eventos resultantes.
        /// </summary>
        /// <param name="sellerID">
        /// Identificador del obligado a la emisión.
        /// </param>
        /// <param name="sellerName">
        /// Nombre o razón social del obligado a la emisión.
        /// </param>
        internal void AddInvoiceAnomalyDetection(
            string sellerID,
            string sellerName)
        {

            // Realizo una única auditoría.
            var result = EventAuditService.AuditInvoices(sellerID);

            // Evento 03: lanzamiento del proceso.
            var startEvent =
                EventFactory.CreateStartInvoiceAnomalyDetection(
                    sellerID,
                    sellerName,
                    result);

            Add(
                startEvent.TipoEvento,
                sellerID,
                sellerName,
                startEvent.DatosPropiosEvento,
                startEvent.OtrosDatosEvento);

            // Eventos 04: anomalías detectadas.
            foreach (var anomaly in result.Anomalies)
            {

                var anomalyEvent =
                    EventFactory.CreateInvoiceAnomalyDetection(
                        sellerID,
                        sellerName,
                        anomaly);

                Add(
                    anomalyEvent.TipoEvento,
                    sellerID,
                    sellerName,
                    anomalyEvent.DatosPropiosEvento,
                    anomalyEvent.OtrosDatosEvento);

            }

        }

        /// <summary>
        /// Añade a la cola el proceso de detección de anomalías
        /// sobre los registros de evento.
        /// </summary>
        internal void AddEventAnomalyDetection(string sellerID, string sellerName)
        {

            var entry = new EventEntry(
                sellerID,
                sellerName);

            lock (_Locker)
            {
                if (_IsClosing)
                    throw new InvalidOperationException(
                        "No se pueden añadir eventos" +
                        " ya que se ha establecido el cierre.");

                _PendingQueue.Enqueue(entry);
            }

        }

        /// <summary>
        /// Añade a la cola la generación de un resumen de eventos.
        /// </summary>
        /// <param name="sellerID">
        /// Identificador del obligado a la emisión.
        /// </param>
        /// <param name="sellerName">
        /// Nombre o razón social del obligado a la emisión.
        /// </param>
        internal void AddEventSummary(
            string sellerID,
            string sellerName)
        {

            var entry = new EventEntry(
                sellerID,
                sellerName,
                true);

            lock (_Locker)
            {

                if (_IsClosing)
                    throw new InvalidOperationException(
                        "No se pueden añadir eventos" +
                        " ya que se ha establecido el cierre.");

                _PendingQueue.Enqueue(entry);

            }

        }

        /// <summary>
        /// Proceso a ejecutar periódicamente entre
        /// intervalos.
        /// </summary>
        public override void Execute()
        {

            lock (_Locker)
            {

                if (_IsWorking)
                    return;

                _IsWorking = true;

            }

            try
            {

                CheckEventSummary();

                while (true)
                {

                    EventEntry entry;

                    lock (_Locker)
                    {

                        if (_PendingQueue.Count == 0)
                            break;

                        entry = _PendingQueue.Dequeue();

                    }

                    try
                    {

                        Process(entry);

                    }
                    catch (Exception ex)
                    {

                        string entryDescription;

                        if (entry.IsEventAnomalyDetection)
                            entryDescription =
                                "detección de anomalías de eventos";
                        else if (entry.IsEventSummary)
                            entryDescription =
                                "resumen de eventos";
                        else
                            entryDescription =
                                $"evento {entry.TipoEvento}";

                        Utils.Log($"EventQueue error procesando {entryDescription}" +
                            $" del emisor {entry.SellerID}: {ex}.");

                        Debug.Print($"EventQueue error procesando {entryDescription}" +
                            $" del emisor {entry.SellerID}: {ex}.");

                    }

                }

            }
            finally
            {

                lock (_Locker)
                {

                    _IsWorking = false;

                    if (_PendingQueue.Count == 0)
                        _SummaryPending = false;

                }

            }

            if (_IsClosing && Count == 0)
                DoExit();

        }

        #endregion

    }

}