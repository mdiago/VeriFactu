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

            var result = new List<string>();

            // 13. Agrupación Destinatarios

            var destinatarios = _RegistroAlta.Destinatarios;

            // Si TipoFactura es “F1”, “F3”, “R1”, “R2”, “R3” o “R4”, la agrupación Destinatarios tiene que estar cumplimentada, con al menos un destinatario.

            var isRequired  = Array.IndexOf(new TipoFactura[]{ TipoFactura.F1, TipoFactura.F3,
                TipoFactura.R1, TipoFactura.R2, TipoFactura.R3, TipoFactura.R4 }, _RegistroAlta.TipoFactura) != -1;

            if ((destinatarios == null || destinatarios.Count == 0) && isRequired)
                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                    $" Si TipoFactura es “F1”, “F3”, “R1”, “R2”, “R3” o “R4”, la agrupación" +
                    $" Destinatarios tiene que estar cumplimentada, con al menos un destinatario.");

            // Si TipoFactura es “F2” o “R5”, la agrupación Destinatarios no puede estar cumplimentada.

            var isForbidden = Array.IndexOf(new TipoFactura[]{ TipoFactura.F1, TipoFactura.F3,
                TipoFactura.R1, TipoFactura.R2, TipoFactura.R3, TipoFactura.R4 }, _RegistroAlta.TipoFactura) != -1;

            if ((destinatarios != null && destinatarios.Count > 0) && isForbidden)
                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                    $" SSi TipoFactura es “F2” o “R5”, la agrupación Destinatarios no puede estar cumplimentada.");

            // Si se identifica mediante NIF, el NIF debe estar identificado y ser distinto del NIF del campo IDEmisorFactura de la agrupación IDFactura.

            // Si se cumplimenta NIF, no deberá existir la agrupación IDOtro y viceversa, pero es obligatorio que se cumplimente uno de los dos.

            // Cuando uno o varios destinatarios se identifiquen a través del NIF, los NIF deben estar identificados y ser distintos del NIF del campo IDEmisorFactura de la agrupación IDFactura.

            // Si el campo IDType = “02” (NIF-IVA), no será exigible el campo CodigoPais.

            // Si el campo IDType = “07” (No censado), el campo CodigoPais debe ser “ES”.

            // Cuando uno o varios destinatarios se identifiquen a través de la agrupación IDOtro e IDType sea “02”, se validará que el campo identificador se ajuste a la estructura de NIF-IVA de alguno de los Estados Miembros y debe estar identificado. Ver nota (1).

            // Cuando uno o varios destinatarios se identifiquen a través de la agrupación IDOtro y CodigoPais sea "ES", se validará que el campo IDType sea “03” o “07”.

            // Cuando se identifique a través del bloque “IDOtro” y IDType sea “02”, se validará que TipoFactura sea “F1”, “F3”, “R1”, “R2”, “R3” ó “R4”.

            return result;

        }

        #endregion

    }

}
