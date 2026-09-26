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
using VeriFactu.Common;
using VeriFactu.Config;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;

namespace VeriFactu.Blockchain
{

    /// <summary>
    /// Representa una cadena de registros de facturación.
    /// </summary>
    public class Blockchain : Chain<Blockchain, Registro>
    {

        #region Construtores Estáticos

        /// <summary>
        /// Constructor estático.
        /// </summary>
        static Blockchain()
        {

            LoadBlockchainsFromDisk();
            Initialized = true;

        }

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sellerID">Vendedor al que pertenece la cadena de bloques.</param>
        public Blockchain(string sellerID) : base(sellerID)
        {
        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Devuelve la ruta de almacenamiento de la cadena
        /// de registros de facturación.
        /// </summary>
        /// <param name="sellerID">Emisor al que pertenece la
        /// cadena de registros de facturación a gestionar.</param>
        /// <returns>Ruta de almacenamiento de la cadena
        /// de registros de facturación.</returns>
        protected override string GetChainDir(string sellerID)
        {

            return Path.Combine(Settings.Current.BlockchainPath, sellerID);

        }

        /// <summary>
        /// Devuelve un encadenamiento con el último elemento
        /// de la cadena.
        /// </summary>
        /// <returns>Encadenamiento con el último elemento
        /// de la cadena.</returns>
        private Encadenamiento GetEncadenamiento()
        {

            if (string.IsNullOrEmpty(Current?.Huella))
                return new Encadenamiento() { PrimerRegistro = "S" };

            return new Encadenamiento()
            {
                RegistroAnterior = new RegistroAnterior()
                {
                    Huella = Current.Huella,
                    FechaExpedicionFactura = Current.IDFactura.FechaExpedicion,
                    IDEmisorFactura = Current.IDFactura.IDEmisor,
                    NumSerieFactura = Current.IDFactura.NumSerie
                }
            };

        }

        /// <summary>
        /// Inserta un eslabón en la cadena.
        /// </summary>
        /// <param name="registro">Registro a encadenar.</param>
        private string Insert(Registro registro)
        {

            // Guardo previo
            SaveCurrent();

            // Actualizo los datos de encadenamiento con el registro anterior
            registro.Encadenamiento = GetEncadenamiento();

            // Establezco el momento de generación.
            CurrentTimeStamp = DateTime.Now;
            registro.FechaHoraHusoGenRegistro = XmlParser.GetXmlDateTimeIso8601(CurrentTimeStamp);            

            // Calculo la huella con los datos del encadenamiento ya actualizados
            registro.Huella = registro.GetHashOutput();

            // Establezco el elemento insertado como el último de la cadena
            Current = registro;
            CurrentID++;

            // Asigno el identificador del eslabón
            registro.BlockchainLinkID = CurrentID;
            registro.SetExternKey();

            return GetControlFileLine();

        }

        /// <summary>
        /// Elimina el útlimo elemento añadido a la cadena.
        /// </summary>
        /// <exception cref="InvalidOperationException">Se lanza si no se encuentra el último eslabón.</exception>
        private void Remove() 
        {

            if (Previous == null && CurrentID > 1)
                throw new InvalidOperationException("No se puede eliminar el último" +
                    " elemento ya que no existe información del elemento previo.");

            // Restauro previo
            RestorePrevious();

        }

        /// <summary>
        /// Devuelve un texto con los datos necesarios para restaurar.
        /// </summary>
        /// <returns> Texto con los datos necesarios para restaurar.</returns>
        protected override string GetVarFileLine()
        {

            return $"{CurrentID}{_CsvSeparator}" +                          // 0
                $"{CurrentTimeStamp}{_CsvSeparator}" +                      // 1
                $"{Current.Huella}{_CsvSeparator}" +                        // 2
                $"{Current.IDFactura.FechaExpedicion}{_CsvSeparator}" +     // 3
                $"{Current.IDFactura.IDEmisor}{_CsvSeparator}" +            // 4
                $"{Current.IDFactura.NumSerie}";                            // 5

        }

        /// <summary>
        /// Devuelve un texto para el archivo csv de control
        /// representando la inserción en la cadena de bloques
        /// con los datos necesarios.
        /// </summary>
        /// <returns>Linea de archivo csv</returns>
        protected override string GetControlFileLine() 
        {

            return $"{CurrentID}{_CsvSeparator}" +                              // 0 Id de entrada en la cadena de bloques
                    $"{CurrentTimeStamp}{_CsvSeparator}" +                      // 1 Marca de tiempo
                    $"{Current.Huella}{_CsvSeparator}" +                        // 2 Huella
                    $"{Current.IDFactura.FechaExpedicion}{_CsvSeparator}" +     // 3 Fecha expedición factura
                    $"{Current.IDFactura.IDEmisor}{_CsvSeparator}" +            // 4 Id emisor
                    $"{Current.IDFactura.NumSerie}{_CsvSeparator}" +            // 5 Número factura
                    $"[{Current.GetHashTextInput()}]";                          // 6 Cadena de entrada utilizada para el cálculo del hash

        }

        /// <summary>
        /// True si está desactivada la eliminación en la cadena.
        /// </summary>
        protected override bool GetDeleteDisabled()
        {

            return Settings.Current.DisableBlockchainDelete;

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
            var huella = values[2];
            var fechaExpedicionFactura = values[3];
            var idEmisorFactura = values[4];
            var numSerieFactura = values[5];

            CurrentID = Convert.ToUInt64(currentID);
            CurrentTimeStamp = Convert.ToDateTime(currentTimeStamp);

            Current = new Registro()
            {
                Huella = huella,
                IDFactura = new IDFactura()
                {
                    FechaExpedicion = fechaExpedicionFactura,
                    IDEmisor = idEmisorFactura,
                    NumSerie = numSerieFactura
                }
            };

        }

        #endregion

        #region Propiedades Públicas Estáticas

        /// <summary>
        /// Indica si el sistema de cadena de bloques está inicializado.
        /// </summary>
        public static bool Initialized { get; private set; }

        #endregion     

        #region Métodos Públicos Estáticos

        /// <summary>
        /// Devuelve la instancia correspondiente a la cadena de bloques
        /// de un emisor de facturas.
        /// </summary>
        /// <param name="sellerID">Id. del emisor de factura.</param>
        /// <returns>Instancia correspondiente a la cadena de bloques
        /// de un emisor de facturas.</returns>
        public static Blockchain Get(string sellerID)
        {

            return GetInstance(sellerID) as Blockchain;

        }

        /// <summary>
        /// Carga todas las cadenas de bloques.
        /// </summary>
        /// <exception cref="InvalidOperationException">Se lanza si BlockchainPath no es un directorio válido.</exception>
        public static void LoadBlockchainsFromDisk()
        {

            if (string.IsNullOrEmpty(Settings.Current.BlockchainPath) ||
                !Directory.Exists(Settings.Current.BlockchainPath))
                throw new InvalidOperationException(
                    $"Revise el archivo de configuración {Settings.FileName}," +
                    $" el valor de BlockchainPath debe ser el de un directorio válido.");

            var dirs = Directory.GetDirectories(Settings.Current.BlockchainPath);

            foreach (var dir in dirs)
            {

                var sellerID = Path.GetFileName(dir);
                var blockchain = new Blockchain(sellerID);

                blockchain.ReadVar();

            }

        }
        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Añade un elemento a la cadena de bloques.
        /// </summary>
        /// <param name="registro">Registro a añadir.</param>
        /// <exception cref="Exception">Si no se puede añadir el eslabón en la cadena.</exception>
        public void Add(Registro registro)
        {

            Exception addException = null;

            lock (Locker)
            {

                try 
                {

                    Insert(registro);
                    Write();

                }
                catch (Exception ex) 
                {

                    addException = ex;

                }

            }

            if (addException != null)
                throw new Exception($"Error añadiendo eslabón de la cadena: {addException.Message}.", addException);

        }

        /// <summary>
        /// Añade una lista de elementos a la cadena de bloques.
        /// </summary>
        /// <param name="registros">Registros a añadir.</param>
        /// <exception cref="Exception">Si no se puede añadir el eslabón en la cadena.</exception>
        public void Add(List<Registro> registros)
        {

            Exception addException = null;

            lock (Locker)
            {

                try
                {

                    var csvLines = new List<string>();

                    for(int r = 0; r < registros.Count; r++)
                        csvLines.Add(Insert(registros[r]));

                    Write(csvLines);

                }
                catch (Exception ex)
                {

                    addException = ex;

                }

            }

            if (addException != null)
                throw new Exception($"Error añadiendo eslabones de la cadena: {addException.Message}.", addException);

        }

        /// <summary>
        /// Elimina el último elememto añadido a la cadena.
        /// </summary>
        /// <param name="registro">Registro a eliminar.
        /// Sólo puede eliminarse el último elemento añadido.</param> 
        /// <exception cref="InvalidOperationException">
        /// Si se itenta eliminar un registro que no es el último.
        /// </exception>
        public void Delete(Registro registro) 
        {

            if (GetDeleteDisabled())
                throw new InvalidOperationException($"Se ha intentado borrar el registro" +
                   $" {registro.Huella} y en la configuración está establecido DisableClearPost = true.");

            Exception restoreException = null;            

            lock (Locker)
            {
                
                try 
                {

                    if (registro.Huella != Current?.Huella)
                        throw new InvalidOperationException($"Se ha intentado borrar el registro" +
                            $" {registro.Huella} que no coincide con el último {Current.Huella}");

                    var blockchainDataFileName = ChainDataFileName;
                    var blockchainDataPreviousFileName = ChainDataPreviousFileName;

                    Remove();


                    WriteVar();
                    RestorePreviousData(blockchainDataFileName, blockchainDataPreviousFileName);

                }
                catch (Exception ex) 
                {

                    restoreException = ex;

                }

            }

            if (restoreException != null)
                throw new Exception($"Error al restaurar datos borrando" +
                    $" último eslabón de la cadena: {restoreException.Message}.", restoreException);

        }

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns>Representación textual de la instancia.</returns>
        public override string ToString()
        {

            return $"{SellerID} ({CurrentID}, {Current?.IDFactura?.NumSerie}," +
                $" {Current?.IDFactura?.FechaExpedicion}, {Current?.Huella})";

        }

        #endregion

    }

}