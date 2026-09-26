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

namespace VeriFactu.EventChain
{

    /// <summary>
    /// Representa una cadena de registros de eventos.
    /// </summary>
    public class EventChain: Chain<EventChain, Evento>
    {

        #region Constructores Estáticos

        /// <summary>
        /// Constructor estático.
        /// </summary>
        static EventChain()
        {

            LoadEventChainsFromDisk();

            Initialized = true;

        }

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sellerID"> Vendedor al que pertenece la cadena de eventos.</param>
        public EventChain(string sellerID) : base(sellerID)
        {
        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Devuelve la ruta de almacenamiento de la cadena
        /// de eventos.
        /// </summary>
        /// <param name="sellerID">Emisor al que pertenece la
        /// cadena de eventos a gestionar.</param>
        /// <returns>Ruta de almacenamiento de la cadena
        /// de eventos.</returns>
        protected override string GetChainDir(string sellerID)
        {

            return Path.Combine(Settings.Current.EventChainPath, sellerID);

        }

        /// <summary>
        /// True si está desactivada la eliminación en la cadena.
        /// </summary>
        protected override bool GetDeleteDisabled()
        {

            return Settings.Current.DisableEventChainDelete;

        }

        /// <summary>
        /// Devuelve un texto para el archivo csv de control
        /// representando la inserción en la cadena de eventos
        /// con los datos necesarios.
        /// </summary>
        /// <returns>Linea de archivo csv</returns>
        protected override string GetControlFileLine()
        {

            return $"{CurrentID}{_CsvSeparator}" +                                      // 0 Id de entrada en la cadena de eventos
                    $"{CurrentTimeStamp}{_CsvSeparator}" +                              // 1 Marca de tiempo
                    $"{Current.HuellaEvento}{_CsvSeparator}" +                          // 2 Huella
                    $"{Utils.GetXmlEnumValue(Current.TipoEvento)}{_CsvSeparator}" +     // 3 Tipo de evento
                    $"[{Current.GetHashTextInput()}]";                                  // 4 Cadena utilizada para el cálculo del hash
        }

        /// <summary>
        /// Devuelve un texto con los datos necesarios para restaurar.
        /// </summary>
        /// <returns> Texto con los datos necesarios para restaurar.</returns>
        protected override string GetVarFileLine()
        {

            return $"{CurrentID}{_CsvSeparator}" +
                $"{CurrentTimeStamp}{_CsvSeparator}" +
                $"{Current.HuellaEvento}{_CsvSeparator}" +
                $"{Utils.GetXmlEnumValue(Current.TipoEvento)}{_CsvSeparator}" +
                $"{Current.FechaHoraHusoGenEvento}";

        }

        /// <summary>
        /// Restaura el último registro de la cadena a partir de los
        /// datos almacenados en el archivo de variables.
        /// </summary>
        /// <param name="values">Valores almacenados.</param>
        protected override void RestoreCurrent(string[] values)
        {

            var currentID = values[0];
            var currentTimeStamp = values[1];
            var huellaEvento = values[2];
            var tipoEvento = values[3];
            var fechaHoraHusoGenEvento = values[4];

            CurrentID = Convert.ToUInt64(currentID);
            CurrentTimeStamp = Convert.ToDateTime(currentTimeStamp);

            Current = new Evento()
            {
                HuellaEvento = huellaEvento,
                TipoEvento = (TipoEvento)Utils.GetEnumFromXmlValue(typeof(TipoEvento), tipoEvento),
                FechaHoraHusoGenEvento = fechaHoraHusoGenEvento
            };

        }

        /// <summary>
        /// Devuelve un encadenamiento con el último elemento
        /// de la cadena.
        /// </summary>
        /// <returns>Encadenamiento con el último elemento
        /// de la cadena.</returns>
        private Encadenamiento GetEncadenamiento()
        {
            if (Current == null)
                return new Encadenamiento()
                {
                    PrimerEvento = "S"
                };

            return new Encadenamiento()
            {
                EventoAnterior = new EventoAnterior()
                {
                    TipoEvento = Current.TipoEvento,
                    FechaHoraHusoGenEvento = Current.FechaHoraHusoGenEvento,
                    HuellaEvento = Current.HuellaEvento
                }
            };
        }

        /// <summary>
        /// Inserta un eslabón en la cadena.
        /// </summary>
        /// <param name="evento"> Evento a encadenar.</param>
        private void Insert(Evento evento)
        {

            SaveCurrent();

            evento.Encadenamiento = GetEncadenamiento();

            CurrentTimeStamp = DateTime.Now;

            evento.FechaHoraHusoGenEvento =
                XmlParser.GetXmlDateTimeIso8601(CurrentTimeStamp);

            evento.HuellaEvento = evento.GetHashOutput();

            Current = evento;
            CurrentID++;

            // Asigno el identificador del eslabón
            evento.EventChainLinkID = CurrentID;

        }

        /// <summary>
        /// Elimina el último elemento de la cadena restaurando el anterior.
        /// </summary>
        private void Remove()
        {

            if (Previous == null && CurrentID > 1)
                throw new InvalidOperationException("No se puede eliminar el último" +
                    " elemento ya que no existe información del elemento previo.");

            RestorePrevious();

        }

        #endregion

        #region Propiedades Públicas Estáticas

        /// <summary>
        /// Indica si el sistema de cadena de eventos está inicializado.
        /// </summary>
        public static bool Initialized { get; private set; }

        #endregion

        #region Métodos Públicos Estáticos

        /// <summary>
        /// Carga todas las cadenas de eventos.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Se lanza si EventChainPath no es un directorio válido.
        /// </exception>
        public static void LoadEventChainsFromDisk()
        {

            if (string.IsNullOrEmpty(Settings.Current.EventChainPath) ||
                !Directory.Exists(Settings.Current.EventChainPath))
                throw new InvalidOperationException(
                    $"Revise el archivo de configuración {Settings.FileName}," +
                    $" el valor de EventChainPath debe ser el de un directorio válido.");

            var dirs = Directory.GetDirectories(Settings.Current.EventChainPath);

            foreach (var dir in dirs)
            {

                var sellerID = Path.GetFileName(dir);
                var eventChain = new EventChain(sellerID);

                eventChain.ReadVar();

            }

        }

        /// <summary>
        /// Devuelve la instancia correspondiente a la cadena de eventos
        /// de un emisor.
        /// </summary>
        /// <param name="sellerID">Id. del emisor.</param>
        /// <returns>Cadena de eventos del emisor.</returns>
        public static EventChain Get(string sellerID)
        {

            return GetInstance(sellerID) as EventChain;

        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Añade un evento a la cadena.
        /// </summary>
        /// <param name="evento">Evento a añadir.</param>
        /// <exception cref="Exception">
        /// Si no se puede añadir el evento a la cadena.
        /// </exception>
        public void Add(Evento evento)
        {

            Exception addException = null;

            lock (Locker)
            {

                try
                {

                    Insert(evento);
                    Write();

                }
                catch (Exception ex)
                {

                    addException = ex;

                }

            }

            if (addException != null)
                throw new Exception(
                    $"Error añadiendo evento a la cadena: {addException.Message}.",
                    addException);

        }

        /// <summary>
        /// Elimina el último evento añadido a la cadena.
        /// </summary>
        /// <param name="evento">Evento a eliminar.
        /// Sólo puede eliminarse el último elemento añadido.</param>
        /// <exception cref="InvalidOperationException">
        /// Si se intenta eliminar un evento que no es el último.
        /// </exception>
        public void Delete(Evento evento)
        {

            if (GetDeleteDisabled())
                throw new InvalidOperationException($"Se ha intentado borrar el evento" +
                    $" {evento.HuellaEvento} y en la configuración está establecido" +
                    $" DisableEventChainDelete = true.");

            Exception restoreException = null;

            lock (Locker)
            {

                try
                {

                    if (evento.HuellaEvento != Current?.HuellaEvento)
                        throw new InvalidOperationException($"Se ha intentado borrar el evento" +
                            $" {evento.HuellaEvento} que no coincide con el último" +
                            $" {Current?.HuellaEvento}");

                    var eventChainDataFileName = ChainDataFileName;
                    var eventChainDataPreviousFileName = ChainDataPreviousFileName;

                    Remove();

                    WriteVar();

                    RestorePreviousData(
                        eventChainDataFileName,
                        eventChainDataPreviousFileName);

                }
                catch (Exception ex)
                {

                    restoreException = ex;

                }

            }

            if (restoreException != null)
                throw new Exception($"Error al restaurar datos borrando" +
                    $" último eslabón de la cadena de eventos:" +
                    $" {restoreException.Message}.", restoreException);

        }

        #endregion

    }

}