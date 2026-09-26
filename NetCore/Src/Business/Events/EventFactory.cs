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
using System.Linq;
using VeriFactu.Config;
using VeriFactu.DataStore;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Evento;

namespace VeriFactu.Business.Events
{

    /// <summary>
    /// Factoría para la creación de eventos SIF.
    /// </summary>
    internal static class EventFactory
    {

        #region Métodos Privados Estáticos

        /// <summary>
        /// Obtiene la identificación y huella de un registro
        /// de facturación.
        /// </summary>
        /// <param name="record">Registro de facturación.</param>
        /// <returns>
        /// Identificación y huella del registro de facturación.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Se produce cuando el registro no contiene los datos
        /// identificativos de la factura.
        /// </exception>
        private static IDFacturaExpedidaHuella GetInvoiceRecordReference(
            Registro record)
        {

            if (record?.IDFactura == null)
                throw new ArgumentException(
                    "El registro de facturación no contiene " +
                    "los datos identificativos de la factura.",
                    nameof(record));

            return new IDFacturaExpedidaHuella()
            {
                IDEmisorFactura =
                    record.IDFactura.IDEmisor,

                NumSerieFactura =
                    record.IDFactura.NumSerie,

                FechaExpedicionFactura =
                    record.IDFactura.FechaExpedicion,

                Huella =
                    record.Huella
            };

        }

        #endregion

        #region Métodos Internos Estáticos

        /// <summary>
        /// Crea un evento de lanzamiento del proceso de detección de anomalías
        /// en los registros de facturación.
        /// </summary>
        /// <param name="sellerID">Identificador del obligado a la emisión.</param>
        /// <param name="sellerName">Nombre o razón social del obligado a la emisión.</param>
        /// <param name="result">Resultado del proceso de detección de anomalías.</param>
        /// <returns>
        /// Evento de lanzamiento del proceso de detección de anomalías
        /// en los registros de facturación.
        /// </returns>
        internal static Evento CreateStartInvoiceAnomalyDetection(
            string sellerID,
            string sellerName,
            EventAuditResult result)
        {

            var datos =
                new LanzamientoProcesoDeteccionAnomaliasRegFacturacion()
                {
                    RealizadoProcesoSobreIntegridadHuellasRegFacturacion =
                        result.HashCheckPerformed ? "S" : "N",

                    NumeroDeRegistrosFacturacionProcesadosSobreIntegridadHuellas =
                        result.HashProcessed.ToString(),

                    RealizadoProcesoSobreIntegridadFirmasRegFacturacion =
                        result.SignatureCheckPerformed ? "S" : "N",

                    NumeroDeRegistrosFacturacionProcesadosSobreIntegridadFirmas =
                        result.SignatureProcessed.ToString(),

                    RealizadoProcesoSobreTrazabilidadCadenaRegFacturacion =
                        result.ChainCheckPerformed ? "S" : "N",

                    NumeroDeRegistrosFacturacionProcesadosSobreTrazabilidadCadena =
                        result.ChainProcessed.ToString(),

                    RealizadoProcesoSobreTrazabilidadFechasRegFacturacion =
                        result.DateCheckPerformed ? "S" : "N",

                    NumeroDeRegistrosFacturacionProcesadosSobreTrazabilidadFechas =
                        result.DateProcessed.ToString()
                };

            return Create(
                TipoEvento.LanzamientoDeteccionAnomaliasRegFacturacion,
                sellerID,
                sellerName,
                new DatosPropiosEvento()
                {
                    DatosEvento = datos
                });

        }

        /// <summary>
        /// Crea un evento de detección de una anomalía
        /// en un registro de facturación.
        /// </summary>
        /// <param name="sellerID">Identificador del obligado a la emisión.</param>
        /// <param name="sellerName">Nombre o razón social del obligado a la emisión.</param>
        /// <param name="anomaly">Anomalía detectada.</param>
        /// <returns>Evento de detección de anomalía.</returns>
        internal static Evento CreateInvoiceAnomalyDetection(
            string sellerID,
            string sellerName,
            EventAuditAnomaly anomaly)
        {

            if (anomaly == null)
                throw new ArgumentNullException(
                    nameof(anomaly));

            if (string.IsNullOrEmpty(anomaly.Type))
                throw new ArgumentException(
                    "No se ha especificado el tipo de anomalía.",
                    nameof(anomaly));

            var datos =
                new DeteccionAnomaliasRegFacturacion()
                {
                    TipoAnomalia =
                        anomaly.Type,

                    OtrosDatosAnomalia =
                        anomaly.Details?.Length > 100
                            ? anomaly.Details.Substring(0, 100)
                            : anomaly.Details
                };

            if (anomaly.Record?.IDFactura != null)
            {
                datos.RegistroFacturacionAnomalo =
                    new IDFacturaExpedida()
                    {
                        IDEmisorFactura =
                            anomaly.Record.IDFactura.IDEmisor,

                        NumSerieFactura =
                            anomaly.Record.IDFactura.NumSerie,

                        FechaExpedicionFactura =
                            anomaly.Record.IDFactura.FechaExpedicion
                    };
            }

            return Create(
                TipoEvento.DeteccionAnomaliasRegFacturacion,
                sellerID,
                sellerName,
                new DatosPropiosEvento()
                {
                    DatosEvento = datos
                });

        }

        /// <summary>
        /// Crea un evento de lanzamiento del proceso de detección de anomalías
        /// en los registros de evento.
        /// </summary>
        /// <param name="sellerID">Identificador del obligado a la emisión.</param>
        /// <param name="sellerName">Nombre o razón social del obligado a la emisión.</param>
        /// <param name="result">Resultado del proceso de detección de anomalías.</param>
        /// <returns>
        /// Evento de lanzamiento del proceso de detección de anomalías
        /// en los registros de evento.
        /// </returns>
        internal static Evento CreateStartEventAnomalyDetection(
            string sellerID,
            string sellerName,
            EventRecordAuditResult result)
        {

            var datos =
                new LanzamientoProcesoDeteccionAnomaliasRegEvento()
                {
                    RealizadoProcesoSobreIntegridadHuellasRegEvento =
                        result.HashCheckPerformed ? "S" : "N",

                    NumeroDeRegistrosEventoProcesadosSobreIntegridadHuellas =
                        result.HashProcessed.ToString(),

                    RealizadoProcesoSobreIntegridadFirmasRegEvento =
                        result.SignatureCheckPerformed ? "S" : "N",

                    NumeroDeRegistrosEventoProcesadosSobreIntegridadFirmas =
                        result.SignatureProcessed.ToString(),

                    RealizadoProcesoSobreTrazabilidadCadenaRegEvento =
                        result.ChainCheckPerformed ? "S" : "N",

                    NumeroDeRegistrosEventoProcesadosSobreTrazabilidadCadena =
                        result.ChainProcessed.ToString(),

                    RealizadoProcesoSobreTrazabilidadFechasRegEvento =
                        result.DateCheckPerformed ? "S" : "N",

                    NumeroDeRegistrosEventoProcesadosSobreTrazabilidadFechas =
                        result.DateProcessed.ToString()
                };

            return Create(
                TipoEvento.LanzamientoDeteccionAnomaliasRegEvento,
                sellerID,
                sellerName,
                new DatosPropiosEvento()
                {
                    DatosEvento = datos
                });

        }

        /// <summary>
        /// Crea un evento de detección de una anomalía
        /// en un registro de evento.
        /// </summary>
        /// <param name="sellerID">Identificador del obligado a la emisión.</param>
        /// <param name="sellerName">Nombre o razón social del obligado a la emisión.</param>
        /// <param name="anomaly">Anomalía detectada.</param>
        /// <returns>Evento de detección de anomalía.</returns>
        internal static Evento CreateEventAnomalyDetection(
            string sellerID,
            string sellerName,
            EventRecordAuditAnomaly anomaly)
        {

            if (anomaly == null)
                throw new ArgumentNullException(
                    nameof(anomaly));

            if (string.IsNullOrEmpty(anomaly.Type))
                throw new ArgumentException(
                    "No se ha especificado el tipo de anomalía.",
                    nameof(anomaly));

            var datos =
                new DeteccionAnomaliasRegEvento()
                {
                    TipoAnomalia =
                        anomaly.Type,

                    OtrosDatosAnomalia =
                        anomaly.Details?.Length > 100
                            ? anomaly.Details.Substring(0, 100)
                            : anomaly.Details
                };

            if (anomaly.Record != null)
            {
                datos.RegistroEventoAnomalo =
                    new RegEvento()
                    {
                        TipoEvento =
                            anomaly.Record.TipoEvento,

                        FechaHoraHusoEvento =
                            anomaly.Record.FechaHoraHusoGenEvento,

                        HuellaEvento =
                            anomaly.Record.HuellaEvento
                    };
            }

            return Create(
                TipoEvento.DeteccionAnomaliasRegEvento,
                sellerID,
                sellerName,
                new DatosPropiosEvento()
                {
                    DatosEvento = datos
                });

        }

        /// <summary>
        /// Crea un evento resumen de eventos.
        /// </summary>
        /// <param name="sellerID"> Identificador del obligado a la emisión.</param>
        /// <param name="sellerName"> Nombre o razón social del obligado a la emisión.</param>
        /// <param name="result"> Resultado del cálculo del resumen de eventos.</param>
        /// <returns>Evento resumen de eventos.</returns>
        /// <exception cref="ArgumentNullException"> Se produce cuando no se ha especificado el resultado.</exception>
        /// <exception cref="InvalidOperationException"> Se produce cuando no existen eventos que incluir en el resumen.</exception>
        internal static Evento CreateEventSummary(string sellerID, string sellerName, EventSummaryResult result)
        {

            if (result == null)
                throw new ArgumentNullException(
                    nameof(result));

            if (result.Events.Count == 0)
                throw new InvalidOperationException(
                    "No existen eventos que incluir en el resumen.");

            var datos =
                new ResumenEventos()
                {
                    TipoEvento = result.Events
                        .OrderBy(e => e.Key)
                        .Select(e =>
                            new TipoEventoAgr()
                            {
                                TipoEvento =
                                    e.Key,

                                NumeroDeEventos =
                                    e.Value.ToString()
                            })
                        .ToList(),

                    NumeroDeRegistrosFacturacionAltaGenerados =
                        result.InvoiceRegistrations.ToString(),

                    SumaCuotaTotalAlta =
                        XmlParser.GetXmlDecimal(
                            result.TotalTaxAmount),

                    SumaImporteTotalAlta =
                        XmlParser.GetXmlDecimal(
                            result.TotalAmount),

                    NumeroDeRegistrosFacturacionAnulacionGenerados =
                        result.InvoiceCancellations.ToString()
                };

            if (result.FirstInvoiceRecord != null)
            {
                datos.RegistroFacturacionInicialPeriodo =
                    GetInvoiceRecordReference(
                        result.FirstInvoiceRecord);
            }

            if (result.LastInvoiceRecord != null)
            {
                datos.RegistroFacturacionFinalPeriodo =
                    GetInvoiceRecordReference(
                        result.LastInvoiceRecord);
            }

            return Create(
                TipoEvento.ResumenEventos,
                sellerID,
                sellerName,
                new DatosPropiosEvento()
                {
                    DatosEvento = datos
                });

        }

        /// <summary>
        /// Crea un evento de exportación de registros de facturación de un periodo.
        /// </summary>
        /// <param name="sellerID"> Identificador del obligado a la emisión.</param>
        /// <param name="sellerName"> Nombre o razón social del obligado a la emisión.</param>
        /// <param name="export">Exportación de registros de un emisor/periodo.</param>
        /// <returns>Evento exportación.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        internal static Evento CreateInvoicePeriodExport(string sellerID, string sellerName, InvoicePeriodExport export)
        {

            if (export == null)
                throw new ArgumentNullException(nameof(export));

            if (export.InvoicePeriodExportFiles.Count == 0)
                throw new InvalidOperationException(
                    "No existen registros de facturación que exportar.");

            var datos =
                new ExportacionRegFacturacionPeriodo()
                {
                    FechaHoraHusoInicioPeriodoExport =
                        XmlParser.GetXmlDateTimeIso8601(export.Start),

                    FechaHoraHusoFinPeriodoExport =
                        XmlParser.GetXmlDateTimeIso8601(export.End),

                    RegistroFacturacionInicialPeriodo =
                        GetInvoiceRecordReference(
                            export.First.Record),

                    RegistroFacturacionFinalPeriodo =
                        GetInvoiceRecordReference(
                            export.Last.Record),

                    NumeroDeRegistrosFacturacionAltaExportados =
                        export.Registrations.ToString(),

                    SumaCuotaTotalAlta =
                        XmlParser.GetXmlDecimal(
                            export.TotalTaxAmount),

                    SumaImporteTotalAlta =
                        XmlParser.GetXmlDecimal(
                            export.TotalAmount),

                    NumeroDeRegistrosFacturacionAnulacionExportados =
                        export.Cancellations.ToString(),

                    RegistrosFacturacionExportadosDejanDeConservarse =
                        "N"
                };

            return Create(
                TipoEvento.ExportacionRegFacturacionPeriodo,
                sellerID,
                sellerName,
                new DatosPropiosEvento()
                {
                    DatosEvento = datos
                });

        }

        /// <summary>
        /// Crea los datos propios del evento de exportación de registros
        /// de facturación de un periodo.
        /// </summary>
        /// <param name="export">Exportación realizada.</param>
        /// <returns>Datos propios del evento.</returns>
        internal static DatosPropiosEvento CreateInvoicePeriodExportData(InvoicePeriodExport export)
        {
            if (export == null)
                throw new ArgumentNullException(nameof(export));

            if (export.InvoicePeriodExportFiles.Count == 0)
                throw new InvalidOperationException(
                    "No existen registros de facturación exportados.");

            var datos =
                new ExportacionRegFacturacionPeriodo()
                {
                    FechaHoraHusoInicioPeriodoExport =
                        XmlParser.GetXmlDateTimeIso8601(export.Start),

                    FechaHoraHusoFinPeriodoExport =
                        XmlParser.GetXmlDateTimeIso8601(export.End),

                    RegistroFacturacionInicialPeriodo =
                        GetInvoiceRecordReference(export.First.Record),

                    RegistroFacturacionFinalPeriodo =
                        GetInvoiceRecordReference(export.Last.Record),

                    NumeroDeRegistrosFacturacionAltaExportados =
                        export.Registrations.ToString(),

                    SumaCuotaTotalAlta =
                        XmlParser.GetXmlDecimal(export.TotalTaxAmount),

                    SumaImporteTotalAlta =
                        XmlParser.GetXmlDecimal(export.TotalAmount),

                    NumeroDeRegistrosFacturacionAnulacionExportados =
                        export.Cancellations.ToString(),

                    RegistrosFacturacionExportadosDejanDeConservarse =
                        "N"
                };

            return new DatosPropiosEvento()
            {
                DatosEvento = datos
            };

        }

        /// <summary>
        /// Crea los datos propios del evento de exportación de registros
        /// de evento de un periodo.
        /// </summary>
        /// <param name="export">Exportación realizada.</param>
        /// <returns>Datos propios del evento.</returns>
        internal static DatosPropiosEvento CreateEventPeriodExportData(EventPeriodExport export)
        {

            if (export == null)
                throw new ArgumentNullException(nameof(export));

            if (export.EventPeriodExportFiles.Count == 0)
                throw new InvalidOperationException(
                    "No existen registros de evento exportados.");

            var datos =
                new ExportacionRegEventoPeriodo()
                {
                    FechaHoraHusoInicioPeriodoExport =
                        XmlParser.GetXmlDateTimeIso8601(export.Start),

                    FechaHoraHusoFinPeriodoExport =
                        XmlParser.GetXmlDateTimeIso8601(export.End),

                    RegistroEventoInicialPeriodo =
                        GetEventRecordReference(export.First.Event),

                    RegistroEventoFinalPeriodo =
                        GetEventRecordReference(export.Last.Event),

                    NumeroDeRegEventoExportados =
                        export.Events.ToString(),

                    RegEventoExportadosDejanDeConservarse =
                        "N"
                };

            return new DatosPropiosEvento()
            {
                DatosEvento = datos
            };

        }

        /// <summary>
        /// Obtiene los datos identificativos de un registro de evento.
        /// </summary>
        /// <param name="evento">Registro de evento.</param>
        /// <returns>Datos identificativos del registro de evento.</returns>
        private static RegEvento GetEventRecordReference(Evento evento)
        {

            if (evento == null)
                throw new ArgumentNullException(nameof(evento));

            return new RegEvento()
            {
                TipoEvento = evento.TipoEvento,
                FechaHoraHusoEvento = evento.FechaHoraHusoGenEvento,
                HuellaEvento = evento.HuellaEvento
            };

        }

        #endregion

        #region Métodos Públicos Estáticos

        /// <summary>
        /// Crea un evento SIF.
        /// </summary>
        /// <param name="tipoEvento">Tipo de evento.</param>
        /// <param name="sellerID">NIF del obligado a la emisión.</param>
        /// <param name="sellerName">Nombre o razón social del obligado a la emisión.</param>
        /// <param name="datosPropiosEvento">Datos propios del evento.</param>
        /// <param name="otrosDatosEvento">Otros datos del evento.</param>
        /// <returns>Evento SIF.</returns>
        public static Evento Create(
            TipoEvento tipoEvento,
            string sellerID,
            string sellerName,
            DatosPropiosEvento datosPropiosEvento = null,
            string otrosDatosEvento = null)
        {

            if (string.IsNullOrWhiteSpace(sellerID))
                throw new ArgumentException(
                    "El NIF del obligado a la emisión no puede estar vacío.",
                    nameof(sellerID));

            if (string.IsNullOrWhiteSpace(sellerName))
                throw new ArgumentException(
                    $"El nombre del obligado a la emisión ({sellerID}) no puede estar vacío.",
                    nameof(sellerName));

            var sistemaInformatico =
                Settings.Current.SistemaInformatico;

            if (sistemaInformatico == null)
                throw new InvalidOperationException(
                    "No se ha configurado el sistema informático.");

            if (!string.IsNullOrEmpty(sistemaInformatico.NIF) &&
                sistemaInformatico.IDOtro != null)
            {
                throw new InvalidOperationException(
                    "El sistema informático no puede estar identificado" +
                    " simultáneamente mediante NIF e IDOtro.");
            }

            if (string.IsNullOrEmpty(sistemaInformatico.NIF) &&
                sistemaInformatico.IDOtro == null)
            {
                throw new InvalidOperationException(
                    "El sistema informático debe estar identificado" +
                    " mediante NIF o IDOtro.");
            }

            return new Evento()
            {
                SistemaInformatico =
                    new VeriFactu.Xml.Factu.Evento.SistemaInformatico()
                    {
                        NombreRazon =
                            sistemaInformatico.NombreRazon,

                        NIF =
                            sistemaInformatico.NIF,

                        IDOtro =
                            sistemaInformatico.IDOtro == null
                                ? null
                                : new Xml.Factu.IDOtro()
                                {
                                    CodigoPais =
                                        sistemaInformatico.IDOtro.CodigoPais,

                                    CodigoPaisSpecified =
                                        sistemaInformatico.IDOtro.CodigoPaisSpecified,

                                    IDType =
                                        sistemaInformatico.IDOtro.IDType,

                                    ID =
                                        sistemaInformatico.IDOtro.ID
                                },

                        NombreSistemaInformatico =
                            sistemaInformatico.NombreSistemaInformatico,

                        IdSistemaInformatico =
                            sistemaInformatico.IdSistemaInformatico,

                        Version =
                            sistemaInformatico.Version,

                        NumeroInstalacion =
                            sistemaInformatico.NumeroInstalacion,

                        TipoUsoPosibleSoloVerifactu =
                            sistemaInformatico.TipoUsoPosibleSoloVerifactu,

                        TipoUsoPosibleMultiOT =
                            sistemaInformatico.TipoUsoPosibleMultiOT,

                        IndicadorMultiplesOT =
                            sistemaInformatico.IndicadorMultiplesOT
                    },

                ObligadoEmision =
                    new PersonaFisicaJuridicaES()
                    {
                        NombreRazon =
                            sellerName,

                        NIF =
                            sellerID
                    },

                TipoEvento =
                    tipoEvento,

                DatosPropiosEvento =
                    datosPropiosEvento,

                OtrosDatosEvento =
                    otrosDatosEvento,

                TipoHuella =
                    Settings.Current.VeriFactuHashAlgorithm
            };

        }

        #endregion

    }

}