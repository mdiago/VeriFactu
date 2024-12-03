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
using System.Diagnostics;
using System.Dynamic;
using VeriFactu.Net.Rest.Json.Parser.Lexer;
using VeriFactu.Net.Rest.Json.Parser.Lexer.Tokens;

namespace VeriFactu.Net.Rest.Json.Parser
{

    /// <summary>
    /// Lector de datos de cadena en JSON
    /// a partir de un analizador léxico.
    /// </summary>
    internal class JsonParserReader
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Analizador léxico al que
        /// corresponde leer.
        /// </summary>
        JsonLexer _JsonLexer;

        /// <summary>
        /// Fragmentos obtenidos por
        /// el analizador léxico.
        /// </summary>

        List<JsonToken> _Tokens;

        /// <summary>
        /// Íncide del fragmento en curso.
        /// </summary>
        int _CurrentIndex = 0;

        /// <summary>
        /// Clave actual.
        /// </summary>
        string _Key = null;

        #endregion

        #region Propiedades Privadas de Instacia

        /// <summary>
        /// Resultado de la deserialización del
        /// texto JSON compuesto en un Expando.
        /// </summary>
        ExpandoObject Result = new ExpandoObject();

        internal JsonToken Current => _Tokens[0];

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="jsonLexer">JsonLexer.</param>
        public JsonParserReader(JsonLexer jsonLexer)
        {
            _JsonLexer = jsonLexer;
            _Tokens = _JsonLexer.GetTokens();
        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Carga un objeto.
        /// </summary>
        /// <returns>Objeto cargado.</returns>
        /// <exception cref="InvalidOperationException">Si se encuentra una secuencia inesperada.</exception>
        private ExpandoObject LoadObject()
        {

            var result = new ExpandoObject();

            while (_CurrentIndex < _Tokens.Count)
            {
                
                var keyToken = _Tokens[++_CurrentIndex];
                _Key = keyToken.Value;

                if (_Key == ",")
                {
                    keyToken = _Tokens[++_CurrentIndex];
                    _Key = keyToken.Value;
                }

                if (_Tokens[++_CurrentIndex].Value != ":")
                    throw new InvalidOperationException("Esperado ':'.");

                var nextToken = _Tokens[++_CurrentIndex];
                var nextTokenValue = nextToken.Value;
                var key = $"{keyToken.Covert()}";

                if (nextTokenValue == "{")
                {
                    ((IDictionary<string, object>)result).Add(key, LoadObject());
                }
                else if (nextTokenValue == "[")
                {
                    ((IDictionary<string, object>)result).Add(key, LoadArray());
                }
                else
                {

                    ((IDictionary<string, object>)result).Add(key, nextToken.Covert());

                    nextTokenValue = _Tokens[++_CurrentIndex].Value;

                    if (nextTokenValue == ",")
                        continue;

                    if (nextTokenValue == "}")
                        break;

                    throw new InvalidOperationException("Esperado ',' o '}'.");

                }
            }

            return result;

        }

        /// <summary>
        /// Carga un array.
        /// </summary>
        /// <returns>Array cargado.</returns>
        /// <exception cref="InvalidOperationException">Si se encuentra una secuencia inesperada.</exception>
        private List<dynamic> LoadArray()
        {

            var result = new List<dynamic>();

            while (_CurrentIndex < _Tokens.Count)
            {

                var nextToken = _Tokens[++_CurrentIndex];
                var nextTokenValue = nextToken.Value;

                if (nextTokenValue == "{")
                    result.Add(LoadObject());
                else
                    result.Add(nextToken.Covert());

                nextToken = _Tokens[++_CurrentIndex];
                nextTokenValue = nextToken.Value;

                if (nextTokenValue == ",")
                    continue;

                if (nextTokenValue == "]")
                    break;

                throw new InvalidOperationException("Esperado ',' o '}'.");

            }

            _CurrentIndex++;

            return result;

        }

        /// <summary>
        /// Devuelve el resultado de la deserialización
        /// de la cadena JSON.
        /// </summary>
        /// <returns>resultado de la deserialización
        /// de la cadena JSON.</returns>
        internal dynamic GetResult()
        {

            return Result;

        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Ejecuta la lectura de datos.
        /// </summary>
        /// <returns>Posición en la que se finaliza la lectura.</returns>
        public int Read()
        {

            Result = LoadObject();
            return _CurrentIndex;

        }

        #endregion

    }

}
