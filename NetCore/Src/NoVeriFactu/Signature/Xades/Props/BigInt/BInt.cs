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

namespace VeriFactu.NoVeriFactu.Signature.Xades.Props.BigInt
{

    /// <summary>
    /// Clase utilizada para representar textualmente en notación
    /// decimal un número entero grande que no se puede almacenar
    /// en los 8 bytes de un ulong.
    /// </summary>
    internal class BInt
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Valor inicial.
        /// </summary>
        string _Value = "0";

        /// <summary>
        /// Conjunto de bytes que componen
        /// el número entero grande.
        /// </summary>
        byte[] _Bytes;

        /// <summary>
        /// Lista de instancias de BByte que componen
        /// el entero.
        /// </summary>
        List<BByte> _BBytes = new List<BByte>();

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bytes">Lista de bytes que componen
        /// el número entero.</param>
        public BInt(byte[] bytes)
        {

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            _Bytes = bytes;
            Load();

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Realiza la multipliplicación aplicando el método
        /// tradicional a dos cadenas de números enteros de entrada.
        /// </summary>
        /// <param name="a">Cadena con el primer factor.</param>
        /// <param name="b">Cadena con el segundo factor.</param>
        /// <returns>Cadena con el número resultante.</returns>
        internal static string Multiply(string a, string b)
        {

            string result = "0";
            int charOffset = 48;

            for (int i = b.Length - 1; i > -1; i--)
            {

                int carry = 0;

                var ib = b[i] - charOffset;
                var iResult = "";

                for (int j = a.Length - 1; j > -1; j--)
                {

                    var ia = a[j] - charOffset;
                    var s = $"{ia * ib + carry}";

                    if (s.Length > 1)
                    {

                        carry = s[0] - charOffset;
                        s = $"{s[s.Length - 1]}";

                    }
                    else
                    {

                        carry = 0;

                    }

                    iResult = $"{s}{iResult}";

                }

                if (carry > 0)
                    iResult = $"{carry}{iResult}";

                var sufix = "".PadLeft(b.Length - (i + 1), '0');

                iResult = $"{iResult}{sufix}";

                result = Sum(result, iResult);

            }

            return result;

        }

        /// <summary>
        /// Realiza la suma aplicando el método
        /// tradicional a dos cadenas de números enteros de entrada.
        /// </summary>
        /// <param name="a">Cadena con el primer sumando.</param>
        /// <param name="b">Cadena con el segundo sumando.</param>
        /// <returns>Cadena con el número resultante.</returns>
        internal static string Sum(string a, string b)
        {

            string result = "";
            int charOffset = 48;
            int carry = 0;

            int length = Math.Max(a.Length, b.Length);

            a = a.PadLeft(length, '0');
            b = b.PadLeft(length, '0');

            for (int i = length - 1; i > -1; i--)
            {

                var ia = (i < a.Length ? a[i] - charOffset : 0);
                var ib = (i < b.Length ? b[i] - charOffset : 0);
                var s = $"{ia + ib + carry}";

                if (s.Length > 1)
                {

                    carry = s[0] - charOffset;
                    s = $"{s[s.Length - 1]}";

                }
                else
                {

                    carry = 0;

                }

                result = $"{s}{result}";

            }

            if (carry > 0)
                result = $"{carry}{result}";

            return result;

        }

        /// <summary>
        /// Carga una lista de instancias de Bbyte que representan
        /// los bytes que componen el número entero.
        /// </summary>
        internal void Load()
        {

            for (int i = 0; i < _Bytes.Length; i++)
            {

                var bByte = new BByte(_Bytes[i], i);
                _BBytes.Add(bByte);
                _Value = BInt.Sum(_Value, bByte.Value);

            }

        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Represéntación textual de la entrada.
        /// </summary>
        /// <returns>Represéntación textual de la entrada.</returns>
        public override string ToString()
        {

            return _Value;

        }

        #endregion

    }

}