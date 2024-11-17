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

namespace VeriFactu.Common
{

    /// <summary>
    /// Representa una clase en la que sólo puede existir
    /// una instancia por clave o identificador asignado.
    /// </summary>
    /// <typeparam name="T">Tipo de la clase a desplegar como
    /// de instancia única por clave (Blockchain o SellerQeue).</typeparam>
    public class SingletonByKey<T>
    {

        #region Variables Privadas Estáticas

        /// <summary>
        /// Diccionario dónde se registran todas las instancias
        /// de la clase generadas durante la ejecución de la biblioteca.
        /// Se registran todas en este diccionario estático para únicamente permitir
        /// la creación de una clase por clave.
        /// </summary>
        static readonly Dictionary<string, SingletonByKey<T>> _InstancesLoaded = new Dictionary<string, SingletonByKey<T>>();

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">Clave que indentifica unívocamente a la instancia.</param>
        protected SingletonByKey(string key)
        {

            Check(key);
            Register(key);
            Key = key;

        }

        #endregion

        #region Métodos Privados Estáticos

        /// <summary>
        /// Devuelve la instancia correspondiente a la cadena de bloques
        /// de un emisor de facturas.
        /// </summary>
        /// <param name="key">Clave asociada a esta instancia.</param>
        /// <returns>Instancia correspondiente a la clave
        /// facilitada en el parámetro key.</returns>
        protected static SingletonByKey<T> GetInstance(string key)
        {

            if (_InstancesLoaded.ContainsKey(key))
                return _InstancesLoaded[key];

            return Activator.CreateInstance(typeof(T), new object[] { key }) as SingletonByKey<T>;

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Si la clave facilitada como parámetro ya tiene una instancia
        /// en ejecución registrada lanza una excepción.
        /// </summary>
        /// <param name="key">Clave únivoca de instancia.</param>
        /// <exception cref="ArgumentException">Cuando ya hay registrada una instancia
        /// para la clave, o el valor de la misma es una cadena nula o vacía.</exception>
        private void Check(string key)
        {

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"El valor del parámetro key no puede" +
                    $" ser nulo o una cadena vacía.");


            if (_InstancesLoaded.ContainsKey(key))
                throw new ArgumentException($"Ya existe una instancia" +
                    $" de {typeof(T).Name} creada para la clave {key}.");

        }

        /// <summary>
        /// Registra la instancia para una
        /// clave determinada.
        /// </summary>
        /// <param name="key">Emisor al que pertenece la
        /// cadena de bloques a gestionar.</param>
        private void Register(string key)
        {

            _InstancesLoaded.Add(key, this);

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Identificador único de la instancia.
        /// </summary>        
        public string Key { get; private set; }

        #endregion

    }

}
