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
using System.Globalization;
using System.Text.RegularExpressions;

namespace VeriFactu.Net.Rest.Json.Parser.Lexer.Tokens
{

    /// <summary>
    /// Fragmento obtenido del análisis léxico de una cadena
    /// JSON que representa un valor de propiedad numérica.
    /// </summary>
    internal class JsonNumber : JsonToken
    {

        #region Propiedades Privadas de Instacia

        /// <summary>
        /// Longitud de la cadena de texto.
        /// </summary>
        internal override int Length
        {

            get
            {

                var curCharIndex = Start;
                var curChar = JsonLexer.JsonText[++curCharIndex];

                while (curCharIndex < JsonLexer.JsonText.Length && Regex.IsMatch($"{curChar}", @"[0-9.]"))
                    curChar = JsonLexer.JsonText[++curCharIndex];

                return curCharIndex - Start;

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
        /// <param name="jsonLexer">Analizador léxico al que
        /// pertenece el fragmento de texto.</param>
        /// <param name="start">Posición del inicio del
        /// fragmento de texto dentro de la cadena completa JSON.</param>
        internal JsonNumber(JsonLexer jsonLexer, int start) : base(jsonLexer, start) 
        { 
        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Convierte el valor del fragmento de texto
        /// en el tipo al que se interpreta que pertenece.
        /// </summary>
        /// <returns>Valor del fragmento de texto
        /// en el tipo al que se interpreta que pertenece.</returns>
        internal override object Covert()
        {

            return Convert.ToDecimal(Value, new NumberFormatInfo() { NumberDecimalSeparator = "." });

        }

        #endregion

    }

}
