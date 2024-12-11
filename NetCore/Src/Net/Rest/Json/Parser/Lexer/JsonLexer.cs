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
using System.Reflection;
using VeriFactu.Net.Rest.Json.Parser.Lexer.Tokens;

namespace VeriFactu.Net.Rest.Json.Parser.Lexer
{

    /// <summary>
    /// Analizador léxico JSON.
    /// </summary>
    public class JsonLexer
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Mapa de tipos de JsonToken por carácter inicial de fragmento.
        /// </summary>
        Dictionary<char, Type> _TokenMap = new Dictionary<char, Type>()
        {

            { '{',      typeof(JsonLeftBrace) },
            { '}',      typeof(JsonRightBrace)},
            { '[',      typeof(JsonLeftBracket)},
            { ']',      typeof(JsonRightBracket)},
            { ':',      typeof(JsonColon)},
            { ',',      typeof(JsonComma)},
            { '-',      typeof(JsonNumber)},
            { '0',      typeof(JsonNumber)},
            { '1',      typeof(JsonNumber)},
            { '2',      typeof(JsonNumber)},
            { '3',      typeof(JsonNumber)},
            { '4',      typeof(JsonNumber)},
            { '5',      typeof(JsonNumber)},
            { '6',      typeof(JsonNumber)},
            { '7',      typeof(JsonNumber)},
            { '8',      typeof(JsonNumber)},
            { '9',      typeof(JsonNumber)},
            { '"',      typeof(JsonString)},
            { 't',      typeof(JsonTrue)},
            { 'f',      typeof(JsonFalse)},
            { 'n',      typeof(JsonNull)},

        };

        /// <summary>
        /// Fragmentos de texto obtenidos como resultado del 
        /// analisis.
        /// </summary>
        List<JsonToken> _Tokens = new List<JsonToken>();

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="jsonText">Texto JSON.</param>
        public JsonLexer(string jsonText)
        {

            JsonText = jsonText;
            Analize(JsonText);

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Realiza el analisis léxico de una
        /// cadena JSON.
        /// </summary>
        /// <param name="jsonText">Cadena JSON para analizar.</param>
        /// <exception cref="ArgumentException">Excepción por carácteres no esperados.</exception>
        internal void Analize(string jsonText)
        {

            int position = 0;

            while (position < jsonText.Length)
            {

                var ch = jsonText[position];
                var token = GetFromChar(ch, position);

                if (token.Length > 0)
                    _Tokens.Add(token);
                else
                    throw new ArgumentException($"Unknown character '{jsonText[position]}'.");

                position += token.Length;

            }

        }

        /// <summary>
        /// Devuelve los fragmentos de texto
        /// resultado del analisis.
        /// </summary>
        /// <returns>Fragmentos de texto
        /// resultado del analisis.</returns>
        internal List<JsonToken> GetTokens()
        {

            return _Tokens;

        }

        /// <summary>
        /// Obtiene una instancia de fragmento de
        /// analisis de texto según el carácter de 
        /// la posición inicial del fragmento pasada
        /// como argumento.
        /// </summary>
        /// <param name="c">Carácter inicial del fragmento.</param>
        /// <param name="position">Posición inicial del fragmento
        /// en respecto a la cadena JSON total.</param>
        /// <returns>Instancia con la clase derivada de JsonToken
        /// correspondiente a ese fragmento de texto.</returns>
        /// <exception cref="ArgumentException">Error si el carácter
        /// no se correponde con ningún tipo de fragmento de texto
        /// válido.</exception>
        private JsonToken GetFromChar(char c, int position)
        {

            if (!_TokenMap.ContainsKey(c))
                throw new ArgumentException($"Unknown character '{c}'.");
            
            return Activator.CreateInstance(_TokenMap[c], BindingFlags.Instance |
                BindingFlags.NonPublic | BindingFlags.CreateInstance,
                null, new object[2] { this, position }, null, null) as JsonToken;

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Texto JSON.
        /// </summary>
        public string JsonText { get; private set; }

        #endregion

    }

}