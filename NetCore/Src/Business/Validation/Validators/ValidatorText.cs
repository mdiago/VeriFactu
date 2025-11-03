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

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VeriFactu.Business.Validation.Validators
{

    /// <summary>
    /// Validaciones de texto
    /// </summary>
    internal class ValidatorText : IValidator
    {

        /// <summary>
        /// Nombre del campo.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Valor del texto a validar.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Longitud máxima.
        /// </summary>
        public int MaxLength { get; private set; }

        /// <summary>
        /// Patrón regex a cumplir por el texto.
        /// </summary>
        public string Pattern { get; private set; }

        /// <summary>
        /// Construye una nueva clase de validación de texto.
        /// </summary>
        /// <param name="name">Nombre campo.</param>
        /// <param name="text">Texto a validar.</param>
        /// <param name="maxLength">Longitud máxima.</param>
        /// <param name="pattern">Patrón regex a cumplir por el texto.</param>
        public ValidatorText(string name, string text, int maxLength, string pattern = null) 
        { 
        
            Name = name;
            Text = text;
            MaxLength = maxLength;
            Pattern = pattern;

        }

        /// <summary>
        /// Ejecuta las validaciones y devuelve una lista
        /// con los errores encontrados.
        /// </summary>
        /// <returns>Lista con las descripciones de los 
        /// errores encontrado.</returns>
        public List<string> GetErrors()
        {

            var result = new List<string>();

            if (string.IsNullOrEmpty(Text))
                return result;

            if (Text.Length > MaxLength)
                result.Add($"Error en el bloque {Name}: La longitud del texto es" +
                $" {Text.Length} cuando la longitud máxima es {MaxLength}.\n('{Text}')");

            if(!string.IsNullOrEmpty(Pattern))
                if(!Regex.IsMatch(Text, Pattern))
                    result.Add($"Error en el bloque {Name}: El texto debe cumplir" +
                    $" con la siguiente expresión regular{Pattern}.\n('{Text}')");

            return result;

        }

    }

}