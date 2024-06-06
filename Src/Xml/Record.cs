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
    serving sii XML data on the fly in a web application, shipping VeriFactu
    with a closed source product.
    
    For more information, please contact Irene Solutions SL. at this
    address: info@irenesolutions.com
 */

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using VeriFactu.Xml.Factu;

namespace VeriFactu.Xml
{

    /// <summary>
    /// Representa un registro de Verifactu: Alta, baja, evento...
    /// </summary>
    public class Record
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
        /// Algoritmos de digest disponibles.
        /// </summary>
        static readonly Dictionary<string, Encoding> _Encodings = new Dictionary<string, Encoding>()
        {

            {"UTF-8", Encoding.UTF8 }

        };

        /// <summary>
        /// Algoritmo de hash.
        /// </summary>
        protected static HashAlgorithm HashAlgorithm { get; private set; }

        /// <summary>
        /// Encoding del texto de entrada para el hash de hash.
        /// </summary>
        protected static Encoding Encoding { get; private set; }

        #endregion

        #region Construtores Estáticos

        /// <summary>
        /// Constructor estático clase.
        /// </summary>
        static Record()
        {

            TipoHuella tipoHuella;
            var tipoHuellaOK = false;

            tipoHuellaOK = Enum.TryParse<TipoHuella>(Settings.Current.VeriFactuHashAlgorithm, out tipoHuella);

            if (!tipoHuellaOK)
                throw new ArgumentException($"El valor de la variable de configuración 'VeriFactuHashAlgorithm'" +
                    $" no puede ser '{Settings.Current.VeriFactuHashAlgorithm}'.");

            if (!_Encodings.ContainsKey(Settings.Current.VeriFactuHashInputEncoding))
                throw new ArgumentException($"El valor de la variable de configuración 'VeriFactuHashInputEncoding'" +
                    $" no puede ser '{Settings.Current.VeriFactuHashInputEncoding}'.");

            HashAlgorithm = _HashAlgorithms[tipoHuella];
            Encoding = _Encodings[Settings.Current.VeriFactuHashInputEncoding];

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Devuelve la cadena de entrada para el cálculo
        /// del hash previa conversión mediante UTF-8.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetHashTextInput()
        {

            throw new NotImplementedException("La clase base record no implmenta el método GetHashInput.\n" +
                "Este método debe implementarse en las clases derivadas.");

        }

        /// <summary>
        /// Devuelve el array de bytes sobre el que calcularemos el hash.
        /// </summary>
        /// <returns></returns>
        private byte[] GetHashInput()
        {

            var textInput = GetHashTextInput();
            var binInput = Encoding.GetBytes(textInput);

            return binInput;

        }

        /// <summary>
        /// Devuelve el hash para el registro.
        /// </summary>
        /// <returns>Hash para el registro.</returns>
        private byte[] GetHash()
        {

            var binInput = GetHashInput();
            var hash = HashAlgorithm.ComputeHash(binInput);

            return hash;

        }

        /// <summary>
        /// Devuelve la representación del hash en formato hexadecimal.
        /// </summary>
        /// <returns>Hash en formato hexadecimal</returns>
        protected string GetHashOutput()
        {

            var hash = GetHash();
            var output = BitConverter.ToString(hash);

            return output.Replace("-", "");

        }

        #endregion

    }

}
