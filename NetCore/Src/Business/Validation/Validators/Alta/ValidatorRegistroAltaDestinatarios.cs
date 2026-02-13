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
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators.Alta
{

    /// <summary>
    /// Valida los datos de RegistroAlta Tercero.
    /// </summary>
    public class ValidatorRegistroAltaDestinatarios : ValidatorRegistroAlta
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="envelope"> Envelope de envío al
        /// servicio Verifactu de la AEAT.</param>
        /// <param name="registroAlta"> Registro de alta del bloque Body.</param>
        public ValidatorRegistroAltaDestinatarios(Envelope envelope, RegistroAlta registroAlta) : base(envelope, registroAlta)
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

            // 3.1.3 Validaciones de negocio de la agrupación RegistroAlta en el bloque de RegistroFactura.

            var result = new List<string>();

            // 13. Agrupación Destinatarios

            var destinatarios = _RegistroAlta.Destinatarios;

            // Si TipoFactura es “F1”, “F3”, “R1”, “R2”, “R3” o “R4”, la agrupación Destinatarios tiene que estar cumplimentada,
            // con al menos un destinatario.

            if ((destinatarios == null || destinatarios.Count == 0) && !_IsSimplificada)
                result.Add($"[3.1.3-13.0] Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                    $" Si TipoFactura es “F1”, “F3”, “R1”, “R2”, “R3” o “R4”, la agrupación" +
                    $" Destinatarios tiene que estar cumplimentada, con al menos un destinatario.");

            // Si TipoFactura es “F2” o “R5”, la agrupación Destinatarios no puede estar cumplimentada.

            if ((destinatarios != null && destinatarios.Count > 0) && _IsSimplificada)
                result.Add($"[3.1.3-13.1] Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                    $" Si TipoFactura es “F2” o “R5”, la agrupación Destinatarios no puede estar cumplimentada.");

            if (destinatarios != null) 
            {

                foreach (var destinatario in destinatarios) 
                {

                    // Validaciones de ID
                    result.AddRange(new ValidatorRegistroAltaInterlocutor(_Envelope, _RegistroAlta, destinatario, "Destinatario", true).GetErrors());

                    // Cuando se identifique a través del bloque “IDOtro” y IDType sea “02”, se validará que TipoFactura sea “F1”, “F3”, “R1”, “R2”, “R3” ó “R4”.

                    if (destinatario.IDOtro != null && destinatario.IDOtro.IDType == IDType.NIF_IVA && _IsSimplificada)
                        result.Add($"[3.1.3-13.2] Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                            $" El destinatario {destinatario} tiene un error en el TipoFactura. " +
                            $"Cuando se identifique a través del bloque “IDOtro” y IDType sea “02”," +
                            $" el TipoFactura debe ser “F1”, “F3”, “R1”, “R2”, “R3” ó “R4”.");

                    // Validaciones de textos

                    var errors = new ValidatorText("NombreRazon", destinatario.NombreRazon, 120).GetErrors();

                    foreach (var error in errors)
                        result.Add($"[3.1.3-13.3] {error}");

                    errors = new ValidatorText("NombreRazonRepresentante", destinatario.NombreRazonRepresentante, 120).GetErrors();

                    foreach (var error in errors)
                        result.Add($"[3.1.3-13.4] {error}");

                    if (destinatario.IDOtro != null)
                    {

                        errors = new ValidatorText("ID", destinatario.IDOtro.ID, 20).GetErrors();

                        foreach (var error in errors)
                            result.Add($"[3.1.3-13.5] {error}");                        
                    
                    }

                }

            }       

            return result;

        }

        #endregion

    }

}