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
using VeriFactu.Config;

namespace VeriFactu.Common
{

    /// <summary>
    /// Clase base para la gestión de una cadena de elementos,
    /// manteniendo el elemento actual y el inmediatamente anterior.
    /// </summary>
    /// <typeparam name="TChain">Tipo concreto de la cadena.</typeparam>
    /// <typeparam name="TItem">Tipo de los elementos de la cadena.</typeparam>
    public abstract class Chain<TChain, TItem> : SingletonByKey<TChain>
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

        #region Propiedades Protegidas de Instancia

        /// <summary>
        /// Objeto utilizado para sincronizar las operaciones sobre la cadena.
        /// </summary>
        protected object Locker => _Locker;

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sellerID">Vendedor al que pertenece la cadena.</param>
        protected Chain(string sellerID) : base(sellerID)
        {

            ChainPath = GetChainPath(Key);
            SellerID = Key;

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Devuelve la ruta de almacenamiento de la cadena
        /// de bloques.
        /// </summary>
        /// <param name="sellerID">Emisor al que pertenece la
        /// cadena a gestionar.</param>
        /// <returns>Ruta de almacenamiento de la cadena
        /// de bloques.</returns>
        private string GetChainPath(string sellerID)
        {

            var dir = GetChainDir(sellerID);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return dir;

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Identificador del vendedor.
        /// Debe utilizarse el identificador fiscal si existe (NIF, VAT Number...).
        /// En caso de no existir, se puede utilizar el número DUNS 
        /// o cualquier otro identificador acordado.
        /// </summary>        
        public string SellerID { get; private set; }

        /// <summary>
        /// Path del directorio de archivado de los datos de la
        /// cadena.
        /// </summary>
        public string ChainPath { get; private set; }

        /// <summary>
        /// Archivo que almacena el valor de las variables en curso
        /// de la cadena.
        /// </summary>
        public string ChainVarFileName => Path.Combine(ChainPath, $"_{SellerID}.csv");

        /// <summary>
        /// Archivo copia de seguridad que almacena una porción del Blockchain correspondiente
        /// a los movimientos de un mes excepto el último movimiento. Es la copia del archivo
        /// con el nombre BlockchainDataFileName antes del registro del último movimiento.
        /// </summary>
        public string ChainDataPreviousFileName => Path.Combine(ChainPath, $"{CurrentTimeStamp:yyyyMM}.PREV.csv");

        /// <summary>
        /// Archivo que almacena una porción del Blockchain correspondiente
        /// a los movimientos de un mes.
        /// </summary>
        public string ChainDataFileName => Path.Combine(ChainPath, $"{CurrentTimeStamp:yyyyMM}.csv");

        /// <summary>
        /// Identificador del último eslabón de la cadena.
        /// </summary>
        public ulong CurrentID { get; protected set; }

        /// <summary>
        /// Momento de generación del último eslabón de la cadena.
        /// </summary>
        public DateTime? CurrentTimeStamp { get; protected set; }

        /// <summary>
        /// Último elemento de la cadena.
        /// </summary>
        public TItem Current { get; protected set; }

        /// <summary>
        /// Identificador del penúltimo eslabón de la cadena.
        /// </summary>
        public ulong PreviousID { get; protected set; }

        /// <summary>
        /// Momento de generación del penúltimo eslabón de la cadena.
        /// </summary>
        public DateTime? PreviousTimeStamp { get; protected set; }

        /// <summary>
        /// Penúltimo elemento de la cadena.
        /// </summary>
        public TItem Previous { get; protected set; }

        #endregion

        #region Métodos Protegidos de Instancia

        /// <summary>
        /// Guarda el estado actual de la cadena como estado anterior.
        /// </summary>
        protected void SaveCurrent()
        {

            PreviousID = CurrentID;
            Previous = Current;
            PreviousTimeStamp = CurrentTimeStamp;

        }

        /// <summary>
        /// Restaura el estado anterior de la cadena.
        /// </summary>
        protected void RestorePrevious()
        {

            CurrentID = PreviousID;
            Current = Previous;
            CurrentTimeStamp = PreviousTimeStamp;

            Previous = default(TItem);

        }

        /// <summary>
        /// Devuelve la ruta de almacenamiento de la cadena
        /// de bloques.
        /// </summary>
        /// <param name="sellerID">Emisor al que pertenece la
        /// cadena a gestionar.</param>
        /// <returns>Ruta de almacenamiento de la cadena
        /// de bloques.</returns>
        protected abstract string GetChainDir(string sellerID);

        /// <summary>
        /// Devuelve un texto para el archivo csv de control
        /// representando la inserción en la cadena
        /// con los datos necesarios.
        /// </summary>
        /// <returns>Linea de archivo csv</returns>
        protected abstract string GetControlFileLine();

        /// <summary>
        /// True si está desactivada la eliminación en la cadena.
        /// </summary>
        protected abstract bool GetDeleteDisabled();

        /// <summary>
        /// Devuelve un texto con los datos necesarios para restaurar
        /// el último elemento de la cadena.
        /// </summary>
        /// <returns>Linea del archivo de variables.</returns>
        protected abstract string GetVarFileLine();

        /// <summary>
        /// Restaura el último elemento de la cadena a partir de los
        /// datos almacenados en el archivo de variables.
        /// </summary>
        /// <param name="values">Valores almacenados.</param>
        protected abstract void RestoreCurrent(string[] values);

        /// <summary>
        /// Añade los datos del último elemento al archivo de control
        /// de la cadena.
        /// </summary>
        /// <param name="csvLines">Líneas de control a incluir en el
        /// archivo csv de control.</param>
        protected void WriteData(List<string> csvLines = null)
        {

            string line = GetControlFileLine();

            if (!GetDeleteDisabled() && File.Exists(ChainDataFileName))
                File.Copy(ChainDataFileName, ChainDataPreviousFileName, overwrite: true);

            if (csvLines == null)
                File.AppendAllText(ChainDataFileName, $"{line}\n");
            else
                File.AppendAllLines(ChainDataFileName, csvLines);

        }

        /// <summary>
        /// Almacena los datos del último elemento de la cadena
        /// en disco.
        /// </summary>
        protected void WriteVar()
        {

            if (CurrentID == 0)
            {
                File.Delete(ChainVarFileName);
            }
            else
            {
                File.WriteAllText(ChainVarFileName, GetVarFileLine());
            }

        }

        /// <summary>
        /// Escribe los datos de la cadena en disco.
        /// </summary>
        /// <param name="csvLines">Líneas a escribir en el csv de control.</param>
        protected void Write(List<string> csvLines = null)
        {
            WriteVar();
            WriteData(csvLines);
        }

        /// <summary>
        /// Recupera de disco el estado actual de la cadena.
        /// </summary>
        protected void ReadVar()
        {

            if (!File.Exists(ChainVarFileName))
                return;

            var line = File.ReadAllText(ChainVarFileName);

            if (string.IsNullOrWhiteSpace(line))
                return;

            var values = line.Split(_CsvSeparator);

            RestoreCurrent(values);

        }

        /// <summary>
        /// Recupera el archivo de datos de la cadena previo a la
        /// inserción del último elemento.
        /// </summary>
        /// <param name="dataFileName">Archivo de datos a restaurar.</param>
        /// <param name="dataPreviousFileName">Copia anterior utilizada para restaurar.</param>
        /// <exception cref="InvalidOperationException">
        /// Se lanza si no se encuentra el archivo previo a restaurar.
        /// </exception>
        protected void RestorePreviousData(string dataFileName, string dataPreviousFileName)
        {

            var isFirstLink = CurrentID == 0;
            var isFirstPeriodLink = false;

            if (!File.Exists(dataPreviousFileName))
            {

                if (File.ReadAllLines(dataFileName).Length == 1)
                {

                    // Se trata del borrado del primer eslabón incluido en el periodo
                    // y por lo tanto no existe archivo previo que restaurar aún,
                    // por lo que únicamente borramos el archivo del periodo incializado
                    // con el registro a borrar
                    isFirstPeriodLink = true;

                }
                else
                {

                    throw new InvalidOperationException(
                        "No se puede restaurar el archivo previo porque no existe.");

                }

            }

            var hasPreviousDataFile = !(isFirstLink || isFirstPeriodLink);

            if (hasPreviousDataFile)
                File.Copy(dataPreviousFileName, dataFileName, overwrite: true);

        }

        #endregion

    }

}