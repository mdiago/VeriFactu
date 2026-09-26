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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using VeriFactu.Common;
using VeriFactu.Config;
using VeriFactu.NoVeriFactu.Signature;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Factu.Anulacion;
using VeriFactu.Xml.Factu.Evento;

namespace VeriFactu.Business.Events
{

    /// <summary>
    /// Proporciona los procesos de detección de anomalías
    /// sobre los registros almacenados.
    /// </summary>
    internal static class EventAuditService
    {

        #region Métodos Públicos Estáticos

        /// <summary>
        /// Realiza los procesos de detección de anomalías sobre los
        /// registros de facturación del obligado a la emisión.
        /// </summary>
        /// <param name="sellerID"> Identificador del obligado a la emisión.</param>
        /// <returns> Resultado del proceso de detección de anomalías.</returns>
        internal static EventAuditResult AuditInvoices(string sellerID)
        {

            var result = new EventAuditResult();

            AuditInvoiceHashes(sellerID, result);
            AuditInvoiceSignatures(sellerID, result);
            AuditInvoiceChain(sellerID, result);
            AuditInvoiceDates(sellerID, result);

            return result;

        }

        /// <summary>
        /// Realiza los procesos de detección de anomalías sobre los eventos.
        /// </summary>
        /// <param name="sellerID"> Identificador del obligado a la emisión.</param>
        /// <returns>Resultado del proceso de detección de anomalías.</returns>
        internal static EventRecordAuditResult AuditEvents(string sellerID)
        {

            var result = new EventRecordAuditResult();

            AuditEventHashes(sellerID, result);
            AuditEventSignatures(sellerID, result);
            AuditEventChain(sellerID, result);
            AuditEventDates(sellerID, result);

            return result;


        }

        #endregion

        #region Métodos Privados Estáticos

        /// <summary>
        /// Comprueba la integridad de las huellas de los registros
        /// de facturación del obligado a la emisión.
        /// </summary>
        /// <param name="sellerID"> Identificador del obligado a la emisión.</param>
        /// <param name="result"> Resultado del proceso de detección de anomalías.</param>
        private static void AuditInvoiceHashes(string sellerID, EventAuditResult result)
        {

            foreach (var filePath in GetInvoiceFiles(sellerID))
            {

                try
                {

                    var registro = Utils.GetRecord(filePath);

                    result.HashProcessed++;

                    if (!VerifyHash(registro))
                    {

                        result.Anomalies.Add(
                            new EventAuditAnomaly()
                            {
                                Type = "01",
                                Details =
                                    "La huella del registro no es válida.",
                                Record = registro
                            });

                    }

                }
                catch (Exception ex)
                {

                    result.Anomalies.Add(
                        new EventAuditAnomaly()
                        {
                            Type = "03",
                            Details =  $"No se ha podido comprobar la huella del archivo '{Path.GetFileName(filePath)}': {ex.Message}" 
                        });

                }

            }

            result.HashCheckPerformed = true;

        }

        /// <summary>
        /// Comprueba la trazabilidad de la cadena de registros
        /// de facturación del obligado a la emisión.
        /// </summary>
        /// <param name="sellerID"> Identificador del obligado a la emisión.</param>
        /// <param name="result"> Resultado del proceso de detección de anomalías.</param>
        private static void AuditInvoiceChain(string sellerID, EventAuditResult result)
        {

            result.ChainCheckPerformed = true;

            var records = GetInvoiceFiles(sellerID)
                .Select(Utils.GetRecord)
                .OrderBy(GetBlockchainLinkID)
                .ToList();

            for (int i = 0; i < records.Count; i++)
            {

                var current = records[i];

                result.ChainProcessed++;

                // Primer registro de la cadena.
                if (i == 0)
                {

                    if (current.Encadenamiento?.PrimerRegistro != "S")
                    {

                        result.Anomalies.Add(new EventAuditAnomaly
                        {
                            Type = "10",
                            Details = "El primer registro almacenado no está marcado como primer registro.",
                            Record = current
                        });

                    }

                    continue;

                }

                var previous = records[i - 1];

                if (current.Encadenamiento?.RegistroAnterior == null)
                {

                    result.Anomalies.Add(new EventAuditAnomaly
                    {
                        Type = "04",
                        Details =
                            "El registro de facturación no es el primero de la cadena " +
                            "pero no contiene la referencia al registro anterior.",
                        Record = current
                    });

                    continue;

                }

                var registroAnterior =
                    current.Encadenamiento.RegistroAnterior;

                // Comprobamos la huella del registro anterior.
                if (!string.Equals(
                    registroAnterior.Huella,
                    previous.Huella,
                    StringComparison.OrdinalIgnoreCase))
                {

                    result.Anomalies.Add(new EventAuditAnomaly
                    {
                        Type = "08",
                        Details = "La huella indicada del registro anterior no coincide con la almacenada.",
                        Record = current
                    });

                }

                // Comprobamos la identificación del registro anterior.
                if (!string.Equals(
                        registroAnterior.IDEmisorFactura,
                        previous.IDFactura.IDEmisorFactura,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        registroAnterior.NumSerieFactura,
                        previous.IDFactura.NumSerieFactura,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        registroAnterior.FechaExpedicionFactura,
                        previous.IDFactura.FechaExpedicionFactura,
                        StringComparison.Ordinal))
                {

                    result.Anomalies.Add(new EventAuditAnomaly
                    {
                        Type = "06",
                        Details =
                            "Los datos identificativos del registro anterior " +
                            "no coinciden con el registro anterior almacenado.",
                        Record = current
                    });

                }

            }

        }


        /// <summary>
        /// Comprueba la integridad de las huellas de los registros
        /// </summary>
        /// <param name="sellerID"> Identificador del obligado a la emisión.</param>
        /// <param name="result"> Resultado del proceso de detección de anomalías.</param>
        private static void AuditEventHashes(string sellerID, EventRecordAuditResult result)
        {

            foreach (var filePath in GetEventFiles(sellerID))
            {

                try
                {

                    var evento = Utils.GetEventRecord(filePath);

                    result.HashProcessed++;

                    var calculatedHash = evento.GetHashOutput();

                    if (!string.Equals(evento.HuellaEvento, calculatedHash, StringComparison.OrdinalIgnoreCase))
                        result.Anomalies.Add(
                            new EventRecordAuditAnomaly
                            {
                                Type = "01",
                                Details =
                                    "La huella del registro de evento no es válida.",
                                Record = evento
                            });
                    
                }
                catch (Exception ex)
                {

                    result.Anomalies.Add(
                        new EventRecordAuditAnomaly
                        {
                            Type = "03",
                            Details =
                                $"No se ha podido comprobar la huella del archivo " +
                                $"'{Path.GetFileName(filePath)}': {ex.Message}"
                        });

                }

            }

            result.HashCheckPerformed = true;
        }

        /// <summary>
        /// Obtiene los identificadores de los obligados a la emisión.
        /// </summary>
        /// <returns> Identificadores de los obligados a la emisión.</returns>
        internal static IEnumerable<string> GetInvoiceSellers()
        {

            if (!Directory.Exists(Settings.Current.InvoicePath))
                return new string[0];

            return Directory.GetDirectories(Settings.Current.InvoicePath)
                .Select(Path.GetFileName);

        }

        /// <summary>
        /// Obtiene el nombre o razón social del obligado a la emisión.
        /// </summary>
        /// <param name="sellerID"> Identificador del obligado a la emisión.</param>
        /// <returns> Nombre o razón social del obligado a la emisión</returns>
        internal static string GetInvoiceSellerName(string sellerID)
        {

            foreach (var filePath in GetInvoiceFiles(sellerID))
            {
                var registro = Utils.GetRecord(filePath);

                var alta = registro as RegistroAlta;

                if (alta != null)
                    return alta.NombreRazonEmisor;

            }

            return null;

        }

        /// <summary>
        /// Obtiene los archivos de registros de facturación
        /// almacenados para un obligado a la emisión.
        /// </summary>
        /// <param name="sellerID"> Identificador del obligado a la emisión.</param>
        /// <returns>
        /// Archivos de registros de facturación.
        /// </returns>
        private static IEnumerable<string> GetInvoiceFiles(string sellerID)
        {

            var sellerPath = Path.Combine(
                Settings.Current.InvoicePath,
                sellerID);

            if (!Directory.Exists(sellerPath))
                return new string[0];

            return Directory.GetFiles(
                sellerPath,
                "*.xml",
                SearchOption.AllDirectories);

        }

        /// <summary>
        /// Obtiene la lista de archivos de registro de eventos de un contribuyente.
        /// </summary>
        /// <param name="sellerID"> Id. del emisor.</param>
        /// <returns> Lista de archivos de registro de eventos de un contribuyente.</returns>
        private static IEnumerable<string> GetEventFiles(string sellerID)
        {

            var sellerPath =
                Path.Combine(
                    Settings.Current.EventPath,
                    sellerID);

            if (!Directory.Exists(sellerPath))
                return new string[0];

            return Directory.GetFiles(
                sellerPath,
                "*.xml",
                SearchOption.AllDirectories);

        }

        /// <summary>
        /// Obtiene los registros de evento almacenados para un obligado a la emisión.
        /// </summary>
        /// <param name="sellerID"> Id. del emisor.</param>
        /// <returns> Los registros de evento almacenados para un obligado a la emisión.</returns>
        internal static IEnumerable<Evento> GetEventRecords(string sellerID)
        {

            return GetEventFiles(sellerID)
                .Select(Utils.GetEventRecord);

        }

        /// <summary>
        /// Obtiene los registros de facturación almacenados para un obligado a la emisión.
        /// </summary>
        /// <param name="sellerID"> Id. del emisor.</param>
        /// <returns>Registros de facturación almacenados para un obligado a la emisión.</returns>
        internal static IEnumerable<Registro> GetInvoiceRecords(string sellerID)
        {

            return GetInvoiceFiles(sellerID)
                .Select(Utils.GetRecord);

        }

        /// <summary>
        /// Comprueba la integridad de la huella de un registro
        /// de facturación.
        /// </summary>
        /// <param name="registro">
        /// Registro a comprobar.
        /// </param>
        /// <returns>
        /// True si la huella es válida; false en caso contrario.
        /// </returns>
        private static bool VerifyHash(Registro registro)
        {

            if (registro == null)
                return false;

            if (string.IsNullOrEmpty(registro.Huella))
                return false;

            var calculatedHash =
                registro.GetHashOutput();

            return string.Equals(
                registro.Huella,
                calculatedHash,
                StringComparison.OrdinalIgnoreCase);

        }

        /// <summary>
        /// Obtiene el identificador del eslabón de la cadena.
        /// </summary>
        /// <param name="registro">
        /// Registro de alta o anulación.
        /// </param>
        /// <returns>
        /// Identificador del eslabón de la cadena.
        /// </returns>
        internal static ulong GetBlockchainLinkID(Registro registro)
        {

            var alta =
                registro as RegistroAlta;

            if (alta != null)
                return Convert.ToUInt64(
                    alta.RefExterna);

            var anulacion =
                registro as RegistroAnulacion;

            if (anulacion != null)
                return Convert.ToUInt64(
                    anulacion.RefExterna);

            throw new InvalidOperationException(
                "No se ha podido obtener el identificador del eslabón.");

        }

        /// <summary>
        /// Comprueba la integridad de las firmas electrónicas de los
        /// registros de facturación del obligado a la emisión.
        /// </summary>
        /// <param name="sellerID">
        /// Identificador del obligado a la emisión.
        /// </param>
        /// <param name="result">
        /// Resultado del proceso de detección de anomalías.
        /// </param>
        private static void AuditInvoiceSignatures(string sellerID, EventAuditResult result)
        {

            result.SignatureCheckPerformed = true;

            foreach (var filePath in GetInvoiceFiles(sellerID))
            {

                var xml = File.ReadAllBytes(filePath);

                if (!SignatureVerifier.HasSignature(xml))
                    throw new InvalidOperationException(
                        "Se han encontrado registros de facturación sin firma " +
                        "electrónica en una instalación configurada como " +
                        "NO VERI*FACTU. No pueden coexistir registros generados " +
                        "en los modos VERI*FACTU y NO VERI*FACTU en una misma " +
                        "instalación. Debe realizar una copia de seguridad y " +
                        "eliminar los registros correspondientes al funcionamiento " +
                        "VERI*FACTU antes de continuar.");

                result.SignatureProcessed++;

                if (!SignatureVerifier.Verify(xml))
                {

                    result.Anomalies.Add(new EventAuditAnomaly
                    {
                        Type = "02",
                        Details =
                            "La firma electrónica del registro no es válida.",
                        Record = Utils.GetRecord(filePath)
                    });

                }

            }

        }

        /// <summary>
        /// Comprueba la integridad de las fechas de los
        /// registros de facturación del obligado a la emisión.
        /// </summary>
        /// <param name="sellerID"> Identificador del obligado a la emisión.</param>
        /// <param name="result"> Resultado del proceso de detección de anomalías.</param>
        private static void AuditInvoiceDates(string sellerID, EventAuditResult result)
        {

            result.DateCheckPerformed = true;

            var records = GetInvoiceFiles(sellerID)
                .Select(Utils.GetRecord)
                .OrderBy(GetBlockchainLinkID)
                .ToList();

            var now = DateTimeOffset.Now;

            for (int i = 0; i < records.Count; i++)
            {

                var current = records[i];

                result.DateProcessed++;

                var currentDate =
                    DateTimeOffset.Parse(
                        current.FechaHoraHusoGenRegistro,
                        CultureInfo.InvariantCulture);

                // La fecha-hora del registro es anterior a la
                // fecha-hora del registro anterior.
                if (i > 0)
                {

                    var previous = records[i - 1];

                    var previousDate =
                        DateTimeOffset.Parse(
                            previous.FechaHoraHusoGenRegistro,
                            CultureInfo.InvariantCulture);

                    if (currentDate < previousDate)
                    {

                        result.Anomalies.Add(new EventAuditAnomaly
                        {
                            Type = "11",
                            Details =
                                "La fecha-hora de generación del registro es " +
                                "anterior a la del registro anterior.",
                            Record = current
                        });

                    }

                }

                // La fecha-hora del registro es posterior a la
                // fecha-hora del registro posterior.
                if (i < records.Count - 1)
                {

                    var next = records[i + 1];

                    var nextDate =
                        DateTimeOffset.Parse(
                            next.FechaHoraHusoGenRegistro,
                            CultureInfo.InvariantCulture);

                    if (currentDate > nextDate)
                    {

                        result.Anomalies.Add(new EventAuditAnomaly
                        {
                            Type = "12",
                            Details =
                                "La fecha-hora de generación del registro es " +
                                "posterior a la del registro posterior.",
                            Record = current
                        });

                    }

                }

                // Comprobamos también la coherencia con la hora actual
                // del sistema, teniendo en cuenta el minuto de tolerancia.
                if (currentDate > now.AddMinutes(1))
                {

                    result.Anomalies.Add(new EventAuditAnomaly
                    {
                        Type = "13",
                        Details =
                            "La fecha-hora de generación del registro es " +
                            "posterior a la fecha-hora actual del sistema.",
                        Record = current
                    });

                }

            }

        }

        /// <summary>
        /// Comprueba la integridad de las firmas electrónicas de los eventos.
        /// </summary>
        /// <param name="sellerID"> Identificador del obligado a la emisión.</param>
        /// <param name="result"> Resultado del proceso de detección de anomalías.</param>
        private static void AuditEventSignatures(string sellerID, EventRecordAuditResult result)
        {

            foreach (var filePath in GetEventFiles(sellerID))
            {

                try
                {

                    var xml = File.ReadAllBytes(filePath);

                    result.SignatureProcessed++;

                    if (!SignatureVerifier.Verify(xml))
                    {

                        result.Anomalies.Add(
                            new EventRecordAuditAnomaly
                            {
                                Type = "02",
                                Details =
                                    "La firma del registro de evento no es válida.",
                                Record = Utils.GetEventRecord(filePath)
                            });

                    }

                }
                catch (Exception ex)
                {

                    result.Anomalies.Add(
                        new EventRecordAuditAnomaly
                        {
                            Type = "03",
                            Details =
                                $"No se ha podido comprobar la firma del archivo " +
                                $"'{Path.GetFileName(filePath)}': {ex.Message}"
                        });

                }

            }

            result.SignatureCheckPerformed = true;
        }

        /// <summary>
        /// Comprueba la cadena de eventos 
        /// </summary>
        /// <param name="sellerID"> Identificador del obligado a la emisión.</param>
        /// <param name="result"> Resultado del proceso de detección de anomalías.</param>
        private static void AuditEventChain(string sellerID, EventRecordAuditResult result)
        {

            result.ChainCheckPerformed = true;

            var records = GetEventFiles(sellerID)
                .Select(Utils.GetEventRecord)
                .OrderBy(e => e.EventChainLinkID)
                .ToList();

            for (int i = 0; i < records.Count; i++)
            {
                var current = records[i];

                result.ChainProcessed++;

                // Primer registro de la cadena
                if (i == 0)
                {

                    if (current.Encadenamiento?.PrimerEvento != "S" || current.Encadenamiento?.EventoAnterior != null)
                        result.Anomalies.Add(
                            new EventRecordAuditAnomaly
                            {
                                Type = "10",
                                Details =
                                    "El primer registro almacenado no está marcado como primer evento.",
                                Record = current
                            });

                    continue;

                }

                var previous = records[i - 1];

                // Todo evento posterior al primero debe referenciar
                // al evento inmediatamente anterior.
                if (current.Encadenamiento?.EventoAnterior == null)
                {

                    result.Anomalies.Add(
                        new EventRecordAuditAnomaly
                        {
                            Type = "04",
                            Details =
                                "El registro no es el primero pero no contiene referencia al evento anterior.",
                            Record = current
                        });

                    continue;

                }

                var eventoAnterior = current.Encadenamiento.EventoAnterior;

                // Huella del evento anterior
                if (!string.Equals(eventoAnterior.HuellaEvento, previous.HuellaEvento, StringComparison.OrdinalIgnoreCase))
                    result.Anomalies.Add(
                        new EventRecordAuditAnomaly
                        {
                            Type = "08",
                            Details =
                                "La huella indicada del evento anterior no coincide con la almacenada.",
                            Record = current
                        });

                // Identificación del evento anterior
                if (eventoAnterior.TipoEvento != previous.TipoEvento ||
                    !string.Equals(
                        eventoAnterior.FechaHoraHusoGenEvento,
                        previous.FechaHoraHusoGenEvento,
                        StringComparison.Ordinal))
                {
                    result.Anomalies.Add(
                        new EventRecordAuditAnomaly
                        {
                            Type = "06",
                            Details =
                                "Los datos identificativos del evento anterior no coinciden con el almacenado.",
                            Record = current
                        });
                }

            }

        }

        /// <summary>
        /// Comprueba la integridad de las fechas de los
        /// registros de evento del obligado a la emisión.
        /// </summary>
        /// <param name="sellerID">Identificador del obligado a la emisión.</param>
        /// <param name="result">Resultado del proceso de detección de anomalías.</param>
        private static void AuditEventDates(string sellerID, EventRecordAuditResult result)
        {

            result.DateCheckPerformed = true;

            var records = GetEventFiles(sellerID)
                .Select(Utils.GetEventRecord)
                .OrderBy(e => e.EventChainLinkID)
                .ToList();

            var now = DateTimeOffset.Now;

            for (int i = 0; i < records.Count; i++)
            {

                var current = records[i];

                result.DateProcessed++;

                var currentDate =
                    DateTimeOffset.Parse(
                        current.FechaHoraHusoGenEvento,
                        CultureInfo.InvariantCulture);

                // La fecha-hora del evento es anterior a la
                // fecha-hora del evento anterior.
                if (i > 0)
                {

                    var previous = records[i - 1];

                    var previousDate =
                        DateTimeOffset.Parse(
                            previous.FechaHoraHusoGenEvento,
                            CultureInfo.InvariantCulture);

                    if (currentDate < previousDate)
                    {

                        result.Anomalies.Add(
                            new EventRecordAuditAnomaly
                            {
                                Type = "11",
                                Details =
                                    "La fecha-hora de generación del evento es " +
                                    "anterior a la del evento anterior.",
                                Record = current
                            });

                    }

                }

                // La fecha-hora del evento es posterior a la
                // fecha-hora del evento posterior.
                if (i < records.Count - 1)
                {

                    var next = records[i + 1];

                    var nextDate =
                        DateTimeOffset.Parse(
                            next.FechaHoraHusoGenEvento,
                            CultureInfo.InvariantCulture);

                    if (currentDate > nextDate)
                    {

                        result.Anomalies.Add(
                            new EventRecordAuditAnomaly
                            {
                                Type = "12",
                                Details =
                                    "La fecha-hora de generación del evento es " +
                                    "posterior a la del evento posterior.",
                                Record = current
                            });

                    }

                }

                // Comprobamos también la coherencia con la hora actual
                // del sistema, teniendo en cuenta el minuto de tolerancia.
                if (currentDate > now.AddMinutes(1))
                {

                    result.Anomalies.Add(
                        new EventRecordAuditAnomaly
                        {
                            Type = "13",
                            Details =
                                "La fecha-hora de generación del evento es " +
                                "posterior a la fecha-hora actual del sistema.",
                            Record = current
                        });

                }

            }

        }

        #endregion

    }

}