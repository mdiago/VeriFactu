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
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators.Alta
{

    /// <summary>
    /// Valida los datos de RegistroAlta FechaOperacion.
    /// </summary>
    public class ValidatorRegistroAltaFechaOperacion : ValidatorRegistroAlta
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="envelope"> Envelope de envío al
        /// servicio Verifactu de la AEAT.</param>
        /// <param name="registroAlta"> Registro de alta del bloque Body.</param>
        public ValidatorRegistroAltaFechaOperacion(Envelope envelope, RegistroAlta registroAlta) : base(envelope, registroAlta)
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

            // 7. FechaOperacion

            if (_FechaOperacion != null)
            {

                // La FechaOperacion no debe ser inferior a la fecha actual menos veinte años y no debe ser superior al año siguiente de la fecha actual.
                if (DateTime.Now.AddYears(-20).CompareTo(_FechaOperacion) > 0)
                    result.Add($"[3.1.3-7.0] Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" La FechaOperacion ({_FechaOperacion:yyyy-MM-dd}) no debe ser inferior a la fecha actual menos veinte años.");

                if ((_FechaOperacion ?? DateTime.Now).Year > DateTime.Now.Year)
                    result.Add($"[3.1.3-7.1] Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" La FechaOperacion ({_FechaOperacion:yyyy-MM-dd}) no debe ser superior al año siguiente de la fecha actual.");

                // Error 1146
                // Sólo se permite que la fecha de expedicion de la factura sea anterior a la fecha operación si los detalles del desglose son ClaveRegimen 14 o 15 e Impuesto 01, 03 o vacío.

                if (_FechaExpedicion.CompareTo(_FechaOperacion) < 0) 
                {

                    var detalles = _RegistroAlta.Desglose;

                    if (detalles != null)
                    {

                        foreach (var detalle in detalles)
                        {

                            var allowedPrev = (detalle.Impuesto == Xml.Factu.Impuesto.IVA || detalle.Impuesto == Xml.Factu.Impuesto.IGIC) 
                                && (detalle.ClaveRegimenSpecified&&(detalle.ClaveRegimen == ClaveRegimen.ObraPteDevengoAdmonPublica || 
                                detalle.ClaveRegimen == ClaveRegimen.TractoSucesivoPteDevengo));

                            if (!allowedPrev)
                                result.Add($"[3.1.3-7.2] Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                                    $" Sólo se permite que la fecha de expedicion de la factura sea anterior a la fecha operación si los" +
                                    $" detalles del desglose son ClaveRegimen 14 o 15 e Impuesto 01, 03 o vacío.");

                        }

                    }

                }

            }

            return result;

        }

        #endregion

    }

}