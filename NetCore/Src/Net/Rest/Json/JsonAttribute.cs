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

namespace VeriFactu.Net.Rest.Json
{

    /// <summary>
    /// Esta clase se define para asociar a los objetos y a sus propiedades los datos necesarios
    /// para la serialización de datos. La serialización de datos se encarga de convertir los 
    /// objetos definidos mediante clases en cadenas formato JSON.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
    public class JsonAttribute : Attribute
    {

        #region Private Member Variables

        /// <summary>
        /// Excluir si el valor del tipo
        /// es el valor por defecto.
        /// </summary>
        private bool _ExcludeOnDefault;

        /// <summary>
        /// Nombre a asignar a la propiedad en el 
        /// proceso de serialización.
        /// </summary>
        private string _Name;

        #endregion

        #region Constructors

        /// <summary>
        /// Construye una nueva instancia de la clase JsonAttribute.
        /// </summary>
        public JsonAttribute()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Indica que la serialización no se debe realizar si la propiedad
        /// tiene el valor por defecto.
        /// </summary>
        public virtual bool ExcludeOnDefault
        {
            get
            {
                return _ExcludeOnDefault;
            }
            set
            {
                _ExcludeOnDefault = value;
            }
        }

        /// <summary>
        /// Nombre a asignar a la propiedad en el 
        /// proceso de serialización.
        /// </summary>
        public virtual string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        #endregion

    }

}