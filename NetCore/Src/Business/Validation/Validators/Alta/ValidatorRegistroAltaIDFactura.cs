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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators.Alta
{

    /// <summary>
    /// Valida los datos de RegistroAlta.
    /// </summary>
    public class ValidatorRegistroAltaIDFactura : ValidatorRegistroAlta
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="envelope"> Envelope de envío al
        /// servicio Verifactu de la AEAT.</param>
        /// <param name="registroAlta"> Registro de alta del bloque Body.</param>
        public ValidatorRegistroAltaIDFactura(Envelope envelope, RegistroAlta registroAlta) : base(envelope, registroAlta)
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

            // 1. Agrupación IDFactura

            var nifEmisorFactura = $"{_RegistroAlta?.IDFacturaAlta?.IDEmisorFactura}";

            if (string.IsNullOrEmpty(nifEmisorFactura))
            {

                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                    $" La propiedad IDFactura.IDEmisorFactura tiene que tener un valor.");

            }
            else
            {

                // El NIF del campo IDEmisorFactura debe ser el mismo que el del campo NIF
                // de la agrupación ObligadoEmision del bloque Cabecera.

                if (_Cabecera?.ObligadoEmision?.NIF != _RegistroAlta?.IDFacturaAlta?.IDEmisorFactura)
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}): El NIF del campo IDEmisorFactura debe ser el mismo que el del campo NIF de la agrupación ObligadoEmision del bloque Cabecera.");

                // La FechaExpedicionFactura no podrá ser superior a la fecha actual.
                var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                if (_FechaExpedicion.CompareTo(now) > 0)
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" La propiedad IDFactura.FechaExpedicion {_FechaExpedicion:yyyy-MM-dd}" +
                        $" no puede ser mayor que la fecha actual {now:yyyy-MM-dd}.");

                // La FechaExpedicionFactura no debe ser inferior a 01/07/2024
                if (_FechaExpedicion.CompareTo(new DateTime(2024, 7, 1)) < 0)
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" La propiedad IDFactura.FechaExpedicion {_FechaExpedicion:yyyy-MM-dd}" +
                        $" no puede ser inferior del 2024-07-01.");


                if (_FechaOperacion != null)
                {

                    // Si Impuesto = “01” (IVA), “03” (IGIC) o no se cumplimenta (considerándose “01” - IVA),
                    // la FechaExpedicionFactura solo puede ser anterior a la FechaOperacion, si ClaveRegimen = "14" o "15”.
                    foreach (var desglose in _RegistroAlta.Desglose)
                    {

                        if ((desglose.ClaveRegimen == ClaveRegimen.ObraPteDevengoAdmonPublica ||
                            desglose.ClaveRegimen == ClaveRegimen.TractoSucesivoPteDevengo) &&
                            (desglose.Impuesto == Impuesto.IVA || desglose.Impuesto == Impuesto.IGIC))
                        {

                            if (_FechaExpedicion.CompareTo(_FechaOperacion) > 0)
                                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                                    $" Para ClaveRegimen '14' 0 '15' en IVA/IGIC la propiedad IDFactura.FechaExpedicion {_FechaExpedicion:yyyy-MM-dd}" +
                                    $" no puede ser mayor que la fecha FechaOperacion {_FechaOperacion:yyyy-MM-dd}.");

                        }

                    }

                }

                // NumSerieFactura solo puede contener caracteres ASCII del 32 a 126 (caracteres imprimibles)
                var numSerie = _RegistroAlta?.IDFacturaAlta?.NumSerie;

                if (string.IsNullOrEmpty(numSerie))
                {

                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                    $" La propiedad IDFactura.IDEmisorFactura tiene que tener un valor.");

                }
                else
                {

                    var okNumSerie = Regex.Match(numSerie, @"[\x20-\x7E]+").Value;

                    if (numSerie != okNumSerie)
                        result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                            $" La propiedad IDFactura.IDEmisorFactura tiene que tener un valor.");

                }

            }

            return result;

        }

        #endregion

    }

}
