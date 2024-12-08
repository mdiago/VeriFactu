/*
    This file is part of the Irene.Solutions.Facturae (R) project.
    Copyright (c) 2020-2021 Irene Solutions SL
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
    develop commercial activities involving the Irene.Solutions.Facturae software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving Irene.Solutions.Facturae services on the fly in a web application, 
    shipping Irene.Solutions.Facturae with a closed source product.
    
    For more information, please contact Irene Solutions SL. at this
    address: info@irenesolutions.com
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;

namespace VeriFactu.Net.Rest.Json.Serializer
{

    /// <summary>
    /// Serializador para propiedades primitivas.
    /// </summary>
    internal class JsonSerializer
    {

        #region Variables Privadas Estáticas

        /// <summary>
        /// Diccionario de serializadores para tipos primitivos.
        /// </summary>
        static readonly Dictionary<Type, IJsonSerializer> _SerializersByType = new Dictionary<Type, IJsonSerializer>()
        {
            {typeof(int),       new JsonIntSerializer() },
            {typeof(long),      new JsonIntSerializer() },
            {typeof(byte),      new JsonIntSerializer() },
            {typeof(decimal),   new JsonDecimalSerializer() },
            {typeof(double),    new JsonDecimalSerializer() },
            {typeof(string),    new JsonStringSerializer() },
            {typeof(DateTime),  new JsonDateTimeSerializer() },
            {typeof(DateTime?), new JsonDateTimeSerializer() },
            {typeof(byte[]),    new JsonByteArraySerializer() },
        };

        #endregion

        #region Variables Privadas de Instancia

        /// <summary>
        /// Serializador para los parámetros facilitados
        /// en el constructor.
        /// </summary>
        IJsonSerializer _Serializer;

        /// <summary>
        /// Info de a propiedad a serializar.
        /// </summary>
        PropertyInfo _PInf;

        /// <summary>
        /// Nombre de la propiedad a serializar
        /// </summary>
        string _PName;

        object _Value;


        #endregion

        #region Propiedades Privadas de Instacia

        /// <summary>
        /// Clave del valor para la serialización.
        /// </summary>
        internal string Key
        {
            
            get 
            {

                var attName = $"{Attribute?.Name}";

                if (!string.IsNullOrEmpty(attName))
                    return $"\"{attName}\"";

                return string.IsNullOrEmpty(_PName) ? $"\"{_PInf?.Name}\"" : $"\"{_PName}\"";


            }
        
        }

        /// <summary>
        /// Atributo de serialización JSON.
        /// </summary>
        internal JsonAttribute Attribute
        {
            
            get 
            {

                if (_PInf == null)
                    return null;

                return _PInf.GetCustomAttribute(typeof(JsonAttribute)) as JsonAttribute;


            }
        
        }

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Construye una nueva instancia de serializador de tipo.
        /// </summary>
        /// <param name="pInf">Info de la propiedad.</param>
        /// <param name="value">Valor a serializar.</param>
        internal JsonSerializer(PropertyInfo pInf, object value)
        {

            _PInf = pInf;
            _Value = value;

            if (pInf.PropertyType.IsEnum)
            {

                var enumItemField = _Value.GetType().GetField($"{value}");
                var xmlEnumAtt = enumItemField.GetCustomAttribute(typeof(XmlEnumAttribute)) as XmlEnumAttribute;

                string enumValue = null;

                if(xmlEnumAtt != null)
                    enumValue = xmlEnumAtt.Name;

                _Serializer = new JsonEnumSerializer(enumValue);

            }
            else 
            {

                if (_SerializersByType.ContainsKey(pInf.PropertyType))
                    _Serializer = _SerializersByType[pInf.PropertyType];
                else
                    _Serializer = new JsonDefaulSerializer();

            }

        }

        /// <summary>
        /// Construye una nueva instancia de serializador de tipo.
        /// </summary>
        /// <param name="pName">Nombre de la propiedad.</param>
        /// <param name="value">Valor a serializar.</param>
        internal JsonSerializer(string pName, object value)
        {

            _PName = pName;
            _Value = value;

            var pType = value.GetType();

            if (_SerializersByType.ContainsKey(pType))
                _Serializer = _SerializersByType[pType];
            else
                _Serializer = new JsonDefaulSerializer();

        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Devuelve la representación en JSON
        /// de la propiedad facilitada para la
        /// instancia facilitada.
        /// </summary>
        /// <returns>Representación JSON de la propiedad.</returns>
        public string ToJson()
        {

            if (_PInf == null && string.IsNullOrEmpty(_PName))
                return $"{_Serializer.ToJson(_Value)}";

            var type = _Value.GetType();

            var seralizeEnumDefault = type.IsEnum;

            if(Attribute != null)
                seralizeEnumDefault = seralizeEnumDefault && !Attribute.ExcludeOnDefault;

            if (type.IsValueType)
            {

                var defaultValue = Activator.CreateInstance(type);

                if (defaultValue.Equals(_Value) && !seralizeEnumDefault)
                    return null;
            }

            return $"{Key}:{_Serializer.ToJson(_Value)}";

        }

        #endregion

    }

}
