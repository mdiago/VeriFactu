﻿/*
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
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators
{

    /// <summary>
    /// Valida los datos de RegistroAlta TipoRectificativa.
    /// </summary>
    public class ValidatorRegistroAltaTipoRectificativa : ValidatorRegistroAlta
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        public ValidatorRegistroAltaTipoRectificativa(Envelope envelope, RegistroAlta registroAlta) : base(envelope, registroAlta)
        {
        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Obtiene los errores de un bloque en concreto.
        /// </summary>
        /// <returns>Lista con los errores de un bloque en concreto.</returns>
        protected override List<string> GetBlockErrors()
        {

            var result = new List<string>();

            // 3. TipoRectificativa

            // Solo podrá incluirse este campo si el valor del campo TipoFactura es igual a “R1”, “R2”, “R3”,
            // “R4” o “R5”

            var isRectificativa = Array.IndexOf(new TipoFactura[]{ TipoFactura.R1, TipoFactura.R2,
                TipoFactura.R3, TipoFactura.R4, TipoFactura.R5 }, _RegistroAlta.TipoFactura) != -1;

            if (!isRectificativa && _RegistroAlta.TipoRectificativaSpecified)
                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                   $" Solo podrá incluirse este campo si el valor del campo TipoFactura es igual a “R1”, “R2”, “R3”," +
                   $" “R4” o “R5”.");

            // Campo obligatorio si TipoFactura es igual a “R1”, “R2”, “R3”, “R4” o “R5”.
            if (isRectificativa && !_RegistroAlta.TipoRectificativaSpecified)
                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                   $" Campo obligatorio si TipoFactura es igual a “R1”, “R2”, “R3”, “R4” o “R5”.");

            return result;

        }

        #endregion

    }

}