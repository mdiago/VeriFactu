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
using System.Text.RegularExpressions;

namespace VeriFactu.Net.Rest.Json.Parser.Lexer.Tokens
{

    /// <summary>
    /// Fragmento obtenido del análisis léxico de una cadena
    /// JSON que representa un valor de propiedad string.
    /// </summary>
    internal class JsonString : JsonToken
    {

        #region Propiedades Privadas de Instacia

        /// <summary>
        /// Longitud de la cadena de texto.
        /// </summary>

        internal override int Length
        {

            get
            {

                return GetLength();

            }

        }

        /// <summary>
        /// Valor de la cadena de texto.
        /// </summary>
        internal override string Value
        {

            get
            {

                return JsonLexer.JsonText.Substring(Start, Length);

            }

        }

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="jsonLexer">Analizador léxico.</param>
        /// <param name="start">Posición del inicio del
        /// fragmento de texto dentro de la cadena completa JSON.</param>
        internal JsonString(JsonLexer jsonLexer, int start) : base(jsonLexer, start)
        {
        }

        #endregion

        #region Métodos Privados de Instancia

        internal int GetLength() 
        {

            var curCharIndex = Start;
            var prevChar = JsonLexer.JsonText[curCharIndex];
            var curChar = JsonLexer.JsonText[++curCharIndex];

            while (curCharIndex < JsonLexer.JsonText.Length && (curChar != '"' || prevChar == '\\'))
            {

                prevChar = JsonLexer.JsonText[curCharIndex];
                curChar = JsonLexer.JsonText[++curCharIndex];

            }

            return curCharIndex - Start + 1;


        }

        /// <summary>
        /// Convierte el valor del fragmento de texto
        /// en el tipo al que se interpreta que pertenece.
        /// </summary>
        /// <returns>Valor del fragmento de texto
        /// en el tipo al que se interpreta que pertenece.</returns>
        internal override object Covert()
        {

            if (Value.Length == 2)
                return "";

            var value = Value.Substring(1, Value.Length - 2);

            if (Regex.IsMatch(value, @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z"))
                return Convert.ToDateTime(value);

            return value;

        }

        #endregion

    }
}
