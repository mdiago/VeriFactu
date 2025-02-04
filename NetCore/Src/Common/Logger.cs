/*
    This file is part of the VeriFactu (R) project.
    Copyright (c) 2023-2024 Irene Solutions SL
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
using System.Text;
using VeriFactu.Config;

namespace VeriFactu.Common
{

    /// <summary>
    /// Se encarga del log.
    /// </summary>
    public class Logger
    {

        #region Variables Privadas Estáticas

        /// <summary>
        /// Almacena los mensajes.
        /// </summary>
        Dictionary<int, string> _Log;

        /// <summary>
        /// Almacena los mensajes.
        /// </summary>
        Dictionary<int, DateTime> _LogTime;

        /// <summary>
        /// Número de mensajes almacenados
        /// </summary>
        int _Count;

        /// <summary>
        /// Número máximo de registros a almacenar.
        /// </summary>
        int _MaxCount = 5000;

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor
        /// </summary>
        public Logger()
        {

            _Log = new Dictionary<int, string>();
            _LogTime = new Dictionary<int, DateTime>();

        }

        #endregion

        #region Finalizador de Instancia

        /// <summary>
        /// Finalizador.
        /// </summary>
        ~Logger()
        {

            Save();

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Obtiene un StringBuilder con los datos
        /// del log.
        /// </summary>
        /// <returns>StringBuilder con los datos
        /// del log.</returns>
        private StringBuilder GetStringBuilder()
        {

            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<int, string> kvp in _Log)
                sb.AppendLine($"[{kvp.Key.ToString().PadLeft(6, '0')}] {_LogTime[kvp.Key].ToString("yyyy-MM-dd HH:mm:ss")}: {kvp.Value}");

            return sb;

        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Almacena un mensaje en el log.
        /// </summary>
        /// <param name="msg">Mensaje.</param>
        public void Log(string msg)
        {

            _Log.Add(++_Count, msg);
            _LogTime.Add(_Count, DateTime.Now);

            if (_Count >= _MaxCount)
            {

                Save();
                Clear();

            }

        }

        /// <summary>
        /// Guarda el log en el archivo indicado.
        /// </summary>
        /// <param name="path">Ruta donde guardar el archivo.</param>
        public void Save(string path = null)
        {

            var text = $"{this}";

            if (string.IsNullOrEmpty(text))
                return;

            path = string.IsNullOrEmpty(path) ? $"{Settings.Current.LogPath}{DateTime.Now:yyyyMMddHHmmss}.txt" : path;
            File.WriteAllText(path, $"{this}");

        }

        /// <summary>
        /// Limpia el contenido del logger.
        /// </summary>
        public void Clear()
        {

            _Log = new Dictionary<int, string>();
            _LogTime = new Dictionary<int, DateTime>();
            _Count = 0;

        }

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns>Representación textual de la instancia.</returns>
        public override string ToString()
        {

            return GetStringBuilder().ToString();

        }

        #endregion

    }

}
