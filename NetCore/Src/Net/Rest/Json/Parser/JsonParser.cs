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
using System.Collections;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using VeriFactu.Net.Rest.Json.Parser.Lexer;
using static System.Net.Mime.MediaTypeNames;

namespace VeriFactu.Net.Rest.Json.Parser
{

    /// <summary>
    /// Deserializador JSON.
    /// </summary>
    public class JsonParser 
    {

        #region Variables Privadas de Instacia

        /// <summary>
        /// Analizador léxico.
        /// </summary>
        JsonLexer _JsonLexer;

        /// <summary>
        /// Lector de fragmentos utilizado
        /// para compener el objeto resultado.
        /// </summary>
        JsonParserReader _JsonParserReader;

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="jsonText">Texto JSON.</param>
        public JsonParser(string jsonText)
        {

            _JsonLexer = new JsonLexer(jsonText);
            _JsonParserReader = new JsonParserReader(_JsonLexer);

            _JsonParserReader.Read();

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Devuelve una nueva instancia del tipo
        /// genérico facilitado a partir de un expando.
        /// </summary>
        /// <typeparam name="T">Tipo de la instancia
        /// a crear.</typeparam>
        /// <param name="dynObj">Objeto expando a partir
        /// del cual devolver la instancia.</param>
        /// <returns>Nueva instancia del tipo T creada
        /// a partir del objeto expando facilitado como parámetrol</returns>
        private T GetFromExpando<T>(dynamic dynObj)
        {

            var type = typeof(T);
            var obj = Activator.CreateInstance(type);
            var primitives = new Type[] { typeof(string), typeof(DateTime), typeof(int), typeof(decimal) };

            foreach (var kvp in dynObj)
            {

                PropertyInfo pInf = type.GetProperty(kvp.Key);

                // Si no existe, busco si hay alguna mapeada por JsonAttribute
                if (pInf == null)
                    pInf = GetByJsonAttName(type, kvp.Key);              

                if (pInf == null)
                    throw new NotSupportedException($"No se ha encontrado para serializar: {kvp}.");

                var att = pInf.GetCustomAttribute(typeof(JsonAttribute)) as JsonAttribute;

                if (att != null && att.JsonIgnore)
                    continue;

                var isPrimitive = Array.IndexOf(primitives, pInf.PropertyType) != -1;
                var iList = kvp.Value as IList;

                if (isPrimitive)
                {

                    pInf.SetValue(obj, kvp.Value);

                }
                else if (pInf.PropertyType.IsEnum)
                {

                    var names = Enum.GetNames(pInf.PropertyType);
                    var values = Enum.GetValues(pInf.PropertyType);
                    var valueIndex = Array.IndexOf(names, kvp.Value);

                    if (valueIndex > -1)
                    {

                        var enumValue = values.GetValue(valueIndex);
                        pInf.SetValue(obj, enumValue);

                    }

                }
                else if (iList != null && iList.Count > 0)
                {

                    var oList = Activator.CreateInstance(pInf.PropertyType) as IList;
                    var childType = pInf.PropertyType.GenericTypeArguments[0];

                    foreach (var i in iList)
                    {

                        if (Array.IndexOf(primitives, childType) != -1)
                        {

                            oList.Add(kvp.Value);

                        }
                        else
                        {

                            var getFromExpando = GetType().GetMethod("GetFromExpando", BindingFlags.NonPublic | BindingFlags.Instance);
                            var getFromExpandoT = getFromExpando.MakeGenericMethod(childType);
                            var child = getFromExpandoT.Invoke(this, new object[] { i });
                            oList.Add(child);

                        }

                    }

                }
                else
                {

                    throw new NotSupportedException($"No se ha podido serializar: {kvp}.");

                }

            }

            return (T)obj;

        }

        /// <summary>
        /// Devuelve PropertyInfo de la propiedad del
        /// tipo facilitado como parámetro type
        /// que tiene la propiedad Name de su JsonAttribute
        /// equivalente al valor del parámetro name.
        /// </summary>
        /// <param name="type">Tipo en el que buscar.</param>
        /// <param name="name">Nombre a buscar.</param>
        /// <returns>PropertyInfo de la propiedad del
        /// tipo facilitado como parámetro type
        /// que tiene la propiedad Name de su JsonAttribute
        /// equivalente al valor del parámetro name</returns>
        private PropertyInfo GetByJsonAttName(Type type, string name) 
        {

            foreach (var propertyInfo in type.GetProperties())
            {

                var jsonAttribute = propertyInfo.GetCustomAttribute(typeof(JsonAttribute)) as JsonAttribute;

                if (jsonAttribute != null && !string.IsNullOrEmpty(jsonAttribute.Name) && jsonAttribute.Name == name)
                    return propertyInfo;

            }

            return null;

        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Devuelve el resultado de la deserialización
        /// de la cadena JSON.
        /// </summary>
        /// <returns>resultado de la deserialización
        /// de la cadena JSON.</returns>
        public dynamic GetResult() 
        {

            return _JsonParserReader.GetResult();

        }

        /// <summary>
        /// Devuelve el resultado de la deserialización
        /// de la cadena JSON.
        /// </summary>
        /// <returns>resultado de la deserialización
        /// de la cadena JSON.</returns>
        public T GetResult<T>()
        {

            var dynObj = _JsonParserReader.GetResult();

            return GetFromExpando<T>(dynObj);            

        }

        #endregion



    }

}