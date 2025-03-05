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
using System.Security.Cryptography;
using System.Text;
using VeriFactu.Config;
using VeriFactu.Xml.Factu;

namespace VeriFactu.Common
{

    /// <summary>
    /// Alunas utilidades generales.
    /// </summary>
    public static class Utils
    {

        #region Variables Privadas Estáticas

        /// <summary>
        /// Algoritmos de dgiest disponibles.
        /// </summary>
        static readonly Dictionary<TipoHuella, HashAlgorithm> _HashAlgorithms = new Dictionary<TipoHuella, HashAlgorithm>()
        {

            {TipoHuella.Sha256, new SHA256Managed() }

        };

        /// <summary>
        /// Codificaciones de texto a binario disponibles.
        /// </summary>
        static readonly Dictionary<string, Encoding> _Encodings = new Dictionary<string, Encoding>()
        {

            {"UTF-8", Encoding.UTF8 }

        };

        /// <summary>
        /// Algoritmo de hash.
        /// </summary>
        internal static HashAlgorithm HashAlgorithm { get; private set; }

        /// <summary>
        /// Encoding del texto de entrada para el hash de hash.
        /// </summary>
        internal static Encoding Encoding { get; private set; }

        /// <summary>
        /// Gestor de log.
        /// </summary>
        public static Logger Logger { get; private set; }

        #endregion

        #region Construtores Estáticos

        /// <summary>
        /// Constructor estático clase.
        /// </summary>
        static Utils()
        {

            if (!_HashAlgorithms.ContainsKey(Settings.Current.VeriFactuHashAlgorithm))
                throw new ArgumentException($"El valor de la variable de configuración 'VeriFactuHashAlgorithm'" +
                    $" no puede ser '{Settings.Current.VeriFactuHashAlgorithm}'.");

            if (!_Encodings.ContainsKey(Settings.Current.VeriFactuHashInputEncoding))
                throw new ArgumentException($"El valor de la variable de configuración 'VeriFactuHashInputEncoding'" +
                    $" no puede ser '{Settings.Current.VeriFactuHashInputEncoding}'.");

            HashAlgorithm = _HashAlgorithms[Settings.Current.VeriFactuHashAlgorithm];
            Encoding = _Encodings[Settings.Current.VeriFactuHashInputEncoding];

            Logger = new Logger();

        }

        #endregion

        #region Métodos Privados Estáticos

        /// <summary>
        /// Codifica un texto de entrada en una cadena de bytes
        /// utilizando UTF8, y luego devuelve la cadena de bytes
        /// en hexadecimal.
        /// </summary>
        /// <param name="text">Texto a codificar.</param>
        /// <returns>Texto que contiene la codificación hexadecimal.</returns>
        internal static string GetEncodedToHex(string text)
        {

            return BitConverter.ToString(Utils.Encoding.GetBytes(text)).Replace("-", "");

        }

        /// <summary>
        /// Devuelve una cadena que ha sido anteriormente pasada a 
        /// binario con el Encoding establecido en la configración
        /// y posteriomente se ha convertido en un texto hesadecimal.
        /// </summary>
        /// <param name="text">Texto codificado a decodificar.</param>
        /// <returns>Texto decodificado.</returns>
        public static string GetFromEncodedToHex(string text)
        {

            if (string.IsNullOrEmpty(text) || text.Length % 2 != 0)
                throw new ArgumentException($"La cadena de entrada no es válida.");

            var buff = new List<byte>();

            for (int b = 0; b < text.Length; b = b + 2)
                buff.Add(Convert.ToByte($"{text[b]}{text[b + 1]}", 16));

            return Utils.Encoding.GetString(buff.ToArray());

        }

        /// <summary>
        /// Almacena un mensaje en el log.
        /// </summary>
        /// <param name="msg">Mensaje.</param>
        public static void Log(string msg)
        {

            if (Settings.Current.LoggingEnabled)
                Logger.Log(msg);

        }

        #endregion

    }

}
