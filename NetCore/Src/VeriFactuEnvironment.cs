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

namespace VeriFactu
{
    /// <summary>
    /// Proporciona la configuración global del entorno de VeriFactu.
    /// </summary>
    public static class VeriFactuEnvironment
    {

        /// <summary>
        /// Bloqueo para thread safe.
        /// </summary>
        private static readonly object _Locker = new object();

        /// <summary>
        /// Directorio de trabajo de la aplicación.
        /// </summary>
        private static string _Path;

        /// <summary>
        /// Para indicar si la configuración del entorno ha sido bloqueada y no puede modificarse.
        /// </summary>
        private static bool _Locked;

        /// <summary>
        /// Obtiene o establece la ruta base utilizada por VeriFactu.
        /// Esta propiedad únicamente puede establecerse una vez y antes
        /// de que el entorno de VeriFactu haya sido inicializado.
        /// </summary>
        /// <returns>Ruta de configuración de VeriFactu.</returns>
        public static string Path
        {

            get 
            {

                lock (_Locker)
                    return _Path; 

            }
            set
            {

                lock (_Locker)
                {

                    if (_Locked)
                        throw new InvalidOperationException(
                            "La ruta no puede modificarse después de que VeriFactu haya sido inicializado.");

                    if (_Path != null)
                        throw new InvalidOperationException(
                            "La ruta únicamente puede establecerse una vez.");

                    if (string.IsNullOrWhiteSpace(value))
                        throw new ArgumentException(
                            "La ruta no puede ser nula ni estar vacía.",
                            nameof(value));

                    string fullPath;

                    try
                    {

                        fullPath = System.IO.Path.GetFullPath(value);

                    }
                    catch (Exception ex) when (
                        ex is ArgumentException ||
                        ex is NotSupportedException ||
                        ex is System.Security.SecurityException)
                    {

                        throw new ArgumentException(
                            "La ruta especificada no es válida.",
                            nameof(value),
                            ex);

                    }

                    try
                    {

                        if (!System.IO.Directory.Exists(fullPath))
                            System.IO.Directory.CreateDirectory(fullPath);

                    }
                    catch (Exception ex)
                    {

                        throw new InvalidOperationException(
                            $"No se ha podido crear el directorio '{fullPath}'.",
                            ex);

                    }

                    _Path = fullPath;

                }

            }

        }

        /// <summary>
        /// Obtiene la ruta configurada y bloquea el entorno impidiendo
        /// posteriores modificaciones.
        /// </summary>
        /// <returns>Ruta configurada o null si no se ha establecido.</returns>
        internal static string GetPathAndLock()
        {

            lock (_Locker)
            {

                _Locked = true;
                return _Path;

            }

        }

    }

}