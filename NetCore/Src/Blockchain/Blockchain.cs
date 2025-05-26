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
    /// Representa una cadena de bloques.
    /// </summary>
    public class Blockchain : SingletonByKey<Blockchain>
    {

        #region Variables Privadas Estáticas

        /// <summary>
        /// Separador para los archivos csv.
        /// </summary>
        const char _CsvSeparator = ';';

        #endregion

        #region Variables Privadas de Instancia

        /// <summary>
        /// Bloqueo para thread safe.
        /// </summary>
        private readonly object _Locker = new object();

        #endregion

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

            BlockchainPath = GetBlockchainPath(Key);
            SellerID = Key;

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Devuelve la ruta de almacenamiento de la cadena
        /// de bloques.
        /// </summary>
        /// <param name="sellerID">Emisor al que pertenece la
        /// cadena de bloques a gestionar.</param>
        /// <returns>Ruta de almacenamiento de la cadena
        /// de bloques.</returns>
        private string GetBlockchainPath(string sellerID)
        {

            var dir = $"{Settings.Current.BlockchainPath}{sellerID}";

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return $"{dir}{Path.DirectorySeparatorChar}";

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
            PreviousID = CurrentID;
            Previous = Current;
            PreviousTimeStamp = CurrentTimeStamp;

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

            return GetControFilelLine();

        }

        /// <summary>
        /// Elimina el útlimo elemento añadido a la cadena.
        /// </summary>
        private void Remove() 
        {

            if (Previous == null && CurrentID > 1)
                throw new InvalidOperationException("No se puede eliminar el último" +
                    " elemento ya que no existe información del elemento previo.");

            // Restauro previo
            CurrentID = PreviousID;
            Current = Previous;
            CurrentTimeStamp = PreviousTimeStamp;

            // Vacío previo
            Previous = null;

        }

        /// <summary>
        /// Escribe los datos de la cadena en disco.
        /// </summary>
        /// <param name="csvLines">Líneas a escribir en el csv de control.</param>
        private void Write(List<string> csvLines = null)
        {

            WriteVar();
            WriteData(csvLines);

        }

        /// <summary>
        /// Almacena los datos del último elemento de la cadena
        /// en disco.
        /// </summary>
        private void WriteVar()
        {

            if (CurrentID == 0)
            {

                // No hay eslabones
                File.Delete(BlockchainVarFileName);

            }
            else 
            {

                // Escribo el valor de la variables actuales
                File.WriteAllText(BlockchainVarFileName, $"{CurrentID}{_CsvSeparator}" +    // 0
                    $"{(DateTime)CurrentTimeStamp:dd/MM/yyyy HH:mm:ss}{_CsvSeparator}" +                                  // 1
                    $"{Current.Huella}{_CsvSeparator}" +                                    // 2
                    $"{Current.IDFactura.FechaExpedicion}{_CsvSeparator}" +                 // 3
                    $"{Current.IDFactura.IDEmisor}{_CsvSeparator}" +                        // 4
                    $"{Current.IDFactura.NumSerie}");                                       // 5

            }


        }

        /// <summary>
        /// Devuelve un texto para el archivo csv de control
        /// representando la inserción en la cadena de bloques
        /// con los datos necesarios.
        /// </summary>
        /// <returns>Linea de archivo csv</returns>
        private string GetControFilelLine() 
        {

            return $"{CurrentID}{_CsvSeparator}" +                              // 0 Id de entrada en la cadena de bloques
                    $"{(DateTime)CurrentTimeStamp:dd/MM/yyyy HH:mm:ss}{_CsvSeparator}" +                      // 1 Marca de tiempo
                    $"{Current.Huella}{_CsvSeparator}" +                        // 2 Huella
                    $"{Current.IDFactura.FechaExpedicion}{_CsvSeparator}" +     // 3 Fecha expedición factura
                    $"{Current.IDFactura.IDEmisor}{_CsvSeparator}" +            // 4 Id emisor
                    $"{Current.IDFactura.NumSerie}{_CsvSeparator}" +            // 5 Número factura
                    $"[{Current.GetHashTextInput()}]";                          // 6 Cadena de entrada utilizada para el cálculo del hash


        }

        /// <summary>
        /// Añade los datos del último elemento al archivo de control
        /// de la cadena.
        /// </summary>
        /// <param name="csvLines">Líneas de control a incluir en el
        /// archivo csv de control.</param>
        private void WriteData(List<string> csvLines = null)
        {

            string line = GetControFilelLine();

            if (File.Exists(BlockchainDataPreviousFileName))
                File.Delete(BlockchainDataPreviousFileName);

            if (File.Exists(BlockchainDataFileName))
                File.Copy(BlockchainDataFileName, BlockchainDataPreviousFileName);

            if (csvLines == null)
                File.AppendAllText(BlockchainDataFileName, $"{line}\n");
            else
                File.AppendAllLines(BlockchainDataFileName, csvLines);

        }

        /// <summary>
        /// Recupera los datos del archivo de la cadena de bloques previo
        /// a la inserción del último elemento.
        /// </summary>
        /// <param name="blockchainDataFileName">Archivo de datos a restaurar.</param>
        /// <param name="blockchainDataPreviousFileName">Copia anterior utilizada para restaurar.</param>
        private void RestorePreviousData(string blockchainDataFileName, 
            string blockchainDataPreviousFileName)
        {            

            var isFirstLink = CurrentID == 0; // Se trataba del primer eslabón de la cadena
            var isFirstBlockPeriodLink = false; // Se trata del primer eslabón del periodo

            if (!File.Exists(blockchainDataPreviousFileName)) 
            {

                if (File.ReadAllLines(blockchainDataFileName).Length == 1)
                {

                    // Se trata del borrado del primer eslabón incluido en el periodo
                    // y por lo tanto no existe archivo previo que restaurar aún,
                    // por lo que únicamente borramos el archivo del periodo incializado
                    // con el registro a borrar
                    isFirstBlockPeriodLink = true;

                }
                else
                {

                    throw new InvalidOperationException("No se puede restaurar el archivo previo por que no existe.");

                }

            }

            var hasBlockchainDataPreviousFile = !(isFirstLink || isFirstBlockPeriodLink);

            File.Delete(blockchainDataFileName);

            if (hasBlockchainDataPreviousFile)
                File.Copy(blockchainDataPreviousFileName, blockchainDataFileName);

        }

        #endregion

        #region Propiedades Públicas Estáticas

        /// <summary>
        /// Indica si el sistema de cadena de bloques está inicializado.
        /// </summary>
        public static bool Initialized { get; private set; }

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
        /// Identificador del último eslabón de la cadena.
        /// </summary>
        public ulong CurrentID { get; private set; }

        /// <summary>
        /// Momento de generación del último eslabón de la cadena.
        /// </summary>
        public DateTime? CurrentTimeStamp { get; private set; }

        /// <summary>
        /// Último elemento de la cadena.
        /// </summary>
        public Registro Current { get; private set; }

        /// <summary>
        /// Identificador del penúltimo eslabón de la cadena.
        /// </summary>
        public ulong PreviousID { get; private set; }

        /// <summary>
        /// Momento de generación del penúltimo eslabón de la cadena.
        /// </summary>
        public DateTime? PreviousTimeStamp { get; private set; }

        /// <summary>
        /// Penúltimo elemento de la cadena.
        /// </summary>
        public Registro Previous { get; private set; }

        /// <summary>
        /// Path del directorio de archivado de los datos de la
        /// cadena.
        /// </summary>
        public string BlockchainPath { get; private set; }

        /// <summary>
        /// Archivo que almacena el valor de las variables en curso
        /// de la cadena.
        /// </summary>
        public string BlockchainVarFileName => $"{BlockchainPath}_{SellerID}.csv";

        /// <summary>
        /// Archivo copia de seguridad que almacena una porción del Blockchain correspondiente
        /// a los movimientos de un mes excepto el último movimiento. Es la copia del archivo
        /// con el nombre BlockchainDataFileName antes del registro del último movimiento.
        /// </summary>
        public string BlockchainDataPreviousFileName => $"{BlockchainPath}{CurrentTimeStamp:yyyyMM}.PREV.csv";

        /// <summary>
        /// Archivo que almacena una porción del Blockchain correspondiente
        /// a los movimientos de un mes.
        /// </summary>
        public string BlockchainDataFileName => $"{BlockchainPath}{CurrentTimeStamp:yyyyMM}.csv";

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
        public static void LoadBlockchainsFromDisk()
        {

            if (string.IsNullOrEmpty(Settings.Current.BlockchainPath) || !Directory.Exists(Settings.Current.BlockchainPath))
                throw new InvalidOperationException($"Revise el archivo de configuración {Settings.FileName}," +
                    $" el valor de BlockchainPath debe ser el de un directorio válido.");

            var dirs = Directory.GetDirectories(Settings.Current.BlockchainPath);

            foreach (var dir in dirs)
            {

                var sellerID = Path.GetFileName(dir);
                var blockchain = new Blockchain(sellerID);

                if (File.Exists(blockchain.BlockchainVarFileName))
                {

                    var lineVarData = File.ReadAllText(blockchain.BlockchainVarFileName);
                    var valuesVarData = lineVarData.Split(_CsvSeparator);

                    var currentID = valuesVarData[0];
                    var currentTimeStamp = valuesVarData[1];
                    var huella = valuesVarData[2];
                    var fechaExpedicionFactura = valuesVarData[3];
                    var idEmisorFactura = valuesVarData[4];
                    var numSerieFactura = valuesVarData[5];

                    blockchain.CurrentID = Convert.ToUInt64(currentID);
                    blockchain.CurrentTimeStamp = Convert.ToDateTime(currentTimeStamp);
                    blockchain.Current = new Registro()
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

            }

        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Añade un elemento a la cadena de bloques.
        /// </summary>
        /// <param name="registro">Registro a añadir.</param>
        public void Add(Registro registro)
        {

            Exception addException = null;

            lock (_Locker)
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
                throw new Exception($"Error añadiendo eslabón de la cadena.", addException);

        }

        /// <summary>
        /// Añade una lista de elementos a la cadena de bloques.
        /// </summary>
        /// <param name="registros">Registros a añadir.</param>
        public void Add(List<Registro> registros)
        {

            Exception addException = null;

            lock (_Locker)
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
                throw new Exception($"Error añadiendo eslabón de la cadena.", addException);

        }

        /// <summary>
        /// Elimina el último elememto añadido a la cadena.
        /// </summary>
        /// <param name="registro">Registro a eliminar.
        /// Sólo puede eliminarse el último elemento añadido.</param> 
        public void Delete(Registro registro) 
        {

            if (registro.Huella != Current.Huella)
                throw new InvalidOperationException($"Se ha intentado borrar el registro" +
                    $" {registro.Huella} que no coincide con el último {Current.Huella}");

            var blockchainDataFileName = BlockchainDataFileName;
            var blockchainDataPreviousFileName = BlockchainDataPreviousFileName;

            Remove();

            Exception restoreException = null;

            lock (_Locker)
            {
                
                try 
                {

                    WriteVar();
                    RestorePreviousData(blockchainDataFileName, blockchainDataPreviousFileName);

                }
                catch (Exception ex) 
                {

                    restoreException = ex;

                }

            }

            if (restoreException != null)
                throw new Exception($"Error al restaurar datos borrando último eslabón de la cadena.", restoreException);

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