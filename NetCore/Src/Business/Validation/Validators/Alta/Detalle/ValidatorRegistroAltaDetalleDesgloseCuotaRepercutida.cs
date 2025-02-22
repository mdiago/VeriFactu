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

using System;
using System.Collections.Generic;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators.Alta.Detalle
{

    /// <summary>
    /// Valida los datos de RegistroAlta DetalleDesglose CuotaRepercutida.
    /// </summary>
    public class ValidatorRegistroAltaDetalleDesgloseCuotaRepercutida : ValidatorRegistroAlta
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Interlocutor a validar.
        /// </summary>
        readonly DetalleDesglose _DetalleDesglose;

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="envelope"> Sobre SOAP envío.</param>
        /// <param name="registroAlta"> Registro alta factura.</param>
        /// <param name="detalleDesglose"> DetalleDesglose a validar. </param>
        public ValidatorRegistroAltaDetalleDesgloseCuotaRepercutida(Envelope envelope, RegistroAlta registroAlta,
            DetalleDesglose detalleDesglose) : base(envelope, registroAlta)
        {

            _DetalleDesglose = detalleDesglose;

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

            var cuotaRepercutida = XmlParser.ToDecimal(_DetalleDesglose.CuotaRepercutida);
            var baseImponibleOimporteNoSujeto = XmlParser.ToDecimal(_DetalleDesglose.BaseImponibleOimporteNoSujeto);
            var baseImponibleACoste = XmlParser.ToDecimal(_DetalleDesglose.BaseImponibleACoste);
            var tipoImpositivo = XmlParser.ToDecimal(_DetalleDesglose.TipoImpositivo);

            if (cuotaRepercutida != 0) 
            {

                if (_DetalleDesglose.CalificacionOperacion == CalificacionOperacion.S1)
                {                   

                    if (!((_RegistroAlta.TipoRectificativaSpecified && _RegistroAlta.TipoRectificativa == TipoRectificativa.I) ||
                        _RegistroAlta.TipoFactura == TipoFactura.R2 || _RegistroAlta.TipoFactura == TipoFactura.R3)) 
                    {

                        var taxBase = (baseImponibleACoste == 0) ? baseImponibleOimporteNoSujeto : baseImponibleACoste;
                        var texBase = (baseImponibleACoste == 0) ? "BaseImponibleOimporteNoSujeto" : "BaseImponibleACoste";

                        // CuotaRepercutida y BaseImponibleOimporteNoSujeto deben tener el mismo signo.
                        if ((cuotaRepercutida/Math.Abs(cuotaRepercutida)) != (taxBase / Math.Abs(taxBase)))
                            result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                                    $" CuotaRepercutida y {texBase} deben tener el mismo signo.");

                        // Si [BaseImponibleOimporteNoSujeto] ≤ 1.000,00:
                        // [CuotaRepercutida]=([BaseImponibleOimporteNoSujeto] * TipoImpositivo) +/- 1% de[BaseImponibleOimporteNoSujeto]
                        // (y en todo caso se admite una diferencia de +/- 10,00 euros). (CREO QUE ESTO ESTÁ MAL REDACTADO !!!)

                        var maxDiff = 10m;

                        if (Math.Abs(taxBase * tipoImpositivo / 100 - cuotaRepercutida) > maxDiff)
                            result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                                $" [CuotaRepercutida]=([{texBase}] * TipoImpositivo) +/- {maxDiff:#,##0.00}%.");

                    }

                }
                else 
                {

                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                           $" CuotaRepercutida solo podrá ser distinta de cero (positivo o negativo)" +
                           $" si CalificacionOperacion es “S1“.");

                }

            }          

            return result;

        }

        #endregion

    }

}
