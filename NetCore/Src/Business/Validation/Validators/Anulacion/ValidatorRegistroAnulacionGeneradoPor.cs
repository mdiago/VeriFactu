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
using VeriFactu.Xml.Factu.Anulacion;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators.Anulacion
{

    /// <summary>
    /// Valida los datos de RegistroAlta GeneradoPor.
    /// </summary>
    public class ValidatorRegistroAnulacionGeneradoPor : ValidatorRegistroAnulacion
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="envelope"> Envelope de envío al
        /// servicio Verifactu de la AEAT.</param>
        /// <param name="registroAnulacion"> Registro de anulación del bloque Body.</param>
        public ValidatorRegistroAnulacionGeneradoPor(Envelope envelope, RegistroAnulacion registroAnulacion) : base(envelope, registroAnulacion)
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

            // 3.1.4 Validaciones de negocio de la agrupación RegistroAnulacion en el bloque RegistroFactura.

            var result = new List<string>();

            // 1. GeneradoPor

            // Si se informa este campo, deberá informarse la agrupación Generador.

            if(!string.IsNullOrEmpty(_RegistroAnulacion.GeneradoPor) && _RegistroAnulacion.Generador == null)
                result.Add($"[3.1.4-2.0] Error en el bloque RegistroAnulacion ({_RegistroAnulacion}):" +
                    $" Si se informa el campo GeneradoPor, deberá informarse la agrupación Generador.");      

            return result;

        }

        #endregion

    }

}