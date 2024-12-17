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
    serving VeriFactu XML data on the fly in a web application, shipping VeriFactu
    with a closed source product.
    
    For more information, please contact Irene Solutions SL. at this
    address: info@irenesolutions.com
 */

using System.Collections.Generic;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Anulacion;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators.Anulacion
{

    /// <summary>
    /// Valida los datos de RegistroAlta Generador.
    /// </summary>
    public class ValidatorRegistroAnulacionGenerador : ValidatorRegistroAnulacion
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="envelope"> Envelope de envío al
        /// servicio Verifactu de la AEAT.</param>
        /// <param name="registroAnulacion"> Registro de anulación del bloque Body.</param>
        public ValidatorRegistroAnulacionGenerador(Envelope envelope, RegistroAnulacion registroAnulacion) : base(envelope, registroAnulacion)
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

            // 2. Agrupación Generador

            // Si se informa esta agrupación, debe haberse informado el campo GeneradoPor.

            if (_RegistroAnulacion.Generador != null &&string.IsNullOrEmpty(_RegistroAnulacion.GeneradoPor))
                result.Add($"Error en el bloque RegistroAnulacion ({_RegistroAnulacion}):" +
                    $" {_RegistroAnulacion.GeneradoPor}: Si se informa la agrupación Generador, deberá informarse el campo GeneradoPor.");

            
            if (_RegistroAnulacion.Generador != null && !string.IsNullOrEmpty(_RegistroAnulacion.GeneradoPor))
            {

                // Validamos interlocutor
                result.AddRange(new ValidatorRegistroAnulacionInterlocutor(_Envelope, _RegistroAnulacion, _RegistroAnulacion.Generador, "Generador").GetErrors());

                // Si el valor de GeneradoPor es igual a “E”, debe estar relleno el campo NIF en el generador.
                if (_RegistroAnulacion.GeneradoPor == "E" && string.IsNullOrEmpty(_RegistroAnulacion.Generador.NIF))
                    result.Add($"Error en el bloque RegistroAnulacion ({_RegistroAnulacion}):" +
                        $" {_RegistroAnulacion.GeneradoPor}: Si el valor de GeneradoPor es igual a “E”, debe estar relleno el campo NIF en el generador.");

                // Si el valor de GeneradoPor es igual a “D”, cuando el Generador se identifique a través del
                // bloque IDOtro y CodigoPais sea "ES", se validará que el campo IDType sea “03” o “07”.
                if (_RegistroAnulacion.GeneradoPor == "D" &&
                    _RegistroAnulacion.Generador?.IDOtro?.CodigoPais == CodigoPais.ES &&
                    _RegistroAnulacion.Generador?.IDOtro?.IDType != IDType.PASAPORTE &&
                    _RegistroAnulacion.Generador?.IDOtro?.IDType != IDType.NO_CENSADO)
                    result.Add($"Error en el bloque RegistroAnulacion ({_RegistroAnulacion}):" +
                        $" {_RegistroAnulacion.GeneradoPor}: Si el valor de GeneradoPor es igual a “D”, cuando el Generador se identifique a través del" +
                        $" bloque IDOtro y CodigoPais sea 'ES', se validará que el campo IDType sea “03” o “07”.");

                // Si el valor del campo GeneradoPor es igual a “T”:
                if (_RegistroAnulacion.GeneradoPor == "T")
                {

                    // Si se identifica a través de la agrupación IDOtro y CodigoPais sea "ES", se validará que el campo IDType sea “03”.
                    if (_RegistroAnulacion.Generador?.IDOtro?.CodigoPais == CodigoPais.ES &&
                        _RegistroAnulacion.Generador?.IDOtro?.IDType != IDType.PASAPORTE)
                        result.Add($"Error en el bloque RegistroAnulacion ({_RegistroAnulacion}):" +
                        $" {_RegistroAnulacion.GeneradoPor}: Si se identifica a través de la agrupación IDOtro y CodigoPais sea 'ES', se validará que el campo IDType sea “03”.");

                    // No se admite el tipo de identificación IDType “07” (“No censado”).
                    if (_RegistroAnulacion.Generador?.IDOtro?.IDType != IDType.NO_CENSADO)
                        result.Add($"Error en el bloque RegistroAnulacion ({_RegistroAnulacion}):" +
                        $" {_RegistroAnulacion.GeneradoPor}: No se admite el tipo de identificación IDType “07” (“No censado”).");

                }

            }

            return result;

        }

        #endregion

    }

}
