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

using System.Collections.Generic;
using VeriFactu.Business.Validation.VIES;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators.Alta
{

    /// <summary>
    /// Valida los datos de RegistroAlta Tercero.
    /// </summary>
    public class ValidatorRegistroAltaTercero : ValidatorRegistroAlta
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        public ValidatorRegistroAltaTercero(Envelope envelope, RegistroAlta registroAlta) : base(envelope, registroAlta)
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

            // 12. Agrupación Tercero

            // Solo podrá cumplimentarse si EmitidaPorTerceroODestinatario es “T”.

            if (_RegistroAlta.Tercero != null && _RegistroAlta.EmitidaPorTercerosODestinatarioSpecified &&
                _RegistroAlta.EmitidaPorTercerosODestinatario != EmitidaPorTercerosODestinatario.T)
                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                    $" Tercero sólo podrá cumplimentarse si EmitidaPorTerceroODestinatario es “T”.");

            // Si se identifica mediante NIF, el NIF debe estar identificado y ser distinto del NIF del campo IDEmisorFactura de la agrupación IDFactura.
            var tercero = _RegistroAlta.Tercero;

            // Si se cumplimenta NIF, no deberá existir la agrupación IDOtro y viceversa, pero es obligatorio que se cumplimente uno de los dos.
            if (tercero != null && !string.IsNullOrEmpty(tercero.NIF) && tercero.IDOtro != null)
                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                    $" Tercero si se cumplimenta NIF, no deberá existir la agrupación IDOtro y viceversa");

            if (tercero != null && string.IsNullOrEmpty(tercero.NIF) && tercero.IDOtro == null)
                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                    $" Tercero es obligatorio que se cumplimente NIF o IDOtro.");

            if (tercero != null && tercero.IDOtro != null)
            {

                // Si el campo IDType = “02” (NIF-IVA), no será exigible el campo CodigoPais.
                if (tercero.IDOtro.IDType != IDType.NIF_IVA)
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" Tercero es obligatorio que se cumplimente CodigoPais con IDOtro.IDType != “02”.");

                // Cuando el tercero se identifique a través de la agrupación IDOtro e IDType sea “02”,
                // se validará que el campo identificador ID se ajuste a la estructura de NIF-IVA de
                // alguno de los Estados Miembros y debe estar identificado. Ver nota (1).
                if (tercero.IDOtro.IDType == IDType.NIF_IVA && !ViesVatNumber.Validate(tercero.IDOtro.ID))
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" Tercero es obligatorio que IDOtro.ID = “{tercero.IDOtro.ID}” esté identificado.");

                // Si se identifica a través de la agrupación IDOtro y CodigoPais sea "ES", se validará que el campo IDType sea “03”.
                if (tercero.IDOtro.CodigoPais == CodigoPais.ES && tercero.IDOtro.IDType == IDType.PASAPORTE)
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" Tercero es obligatorio que para IDOtro.CodigoPais = “{tercero.IDOtro.CodigoPais}” IDOtro.IDType = “03” (PASAPORTE).");

                if (tercero.IDOtro.IDType == IDType.NO_CENSADO)
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" Tercero no se admite IDOtro.IDType = “07” (NO CENSADO).");


            }

            return result;

        }

        #endregion

    }

}
