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
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators.Alta
{

    /// <summary>
    /// Valida los datos de RegistroAlta DetalleDesglose CuotaTotal.
    /// </summary>
    public class ValidatorRegistroAltaCampoObligatorio : ValidatorRegistroAlta
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="envelope"> Sobre SOAP envío.</param>
        /// <param name="registroAlta"> Registro alta factura.</param>
        public ValidatorRegistroAltaCampoObligatorio(Envelope envelope, 
            RegistroAlta registroAlta) : base(envelope, registroAlta)
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

            if (string.IsNullOrEmpty(_RegistroAlta.IDVersion))
                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" El campo IDVersion es obligatorio");

            if (string.IsNullOrEmpty(_RegistroAlta.IDFacturaAlta.IDEmisorFactura))
                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" El campo IDFactura.IDEmisorFactura es obligatorio");

            if (string.IsNullOrEmpty(_RegistroAlta.IDFacturaAlta.NumSerieFactura))
                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" El campo IDFactura.NumSerieFactura es obligatorio");

            if (string.IsNullOrEmpty(_RegistroAlta.IDFacturaAlta.FechaExpedicionFactura))
                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" El campo IDFactura.FechaExpedicionFactura es obligatorio");

            if (string.IsNullOrEmpty(_RegistroAlta.NombreRazonEmisor))
                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" El campo NombreRazonEmisor es obligatorio");

            if (!_RegistroAlta.TipoFacturaSpecified)
                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" El campo TipoFactura es obligatorio");

            if (_RegistroAlta.FacturasRectificadas!= null &&_RegistroAlta.FacturasRectificadas.Count > 0) 
            {

                foreach (var rectificada in _RegistroAlta.FacturasRectificadas) 
                {

                    if (string.IsNullOrEmpty(rectificada.IDEmisorFactura))
                        result.Add($"Error en el bloque RegistroAlta.FacturasRectificadas ({rectificada}):" +
                                $" El campo IDEmisorFactura es obligatorio");

                    if (string.IsNullOrEmpty(rectificada.NumSerieFactura))
                        result.Add($"Error en el bloque RegistroAlta.FacturasRectificadas ({rectificada}):" +
                                $" El campo NumSerieFactura es obligatorio");

                    if (string.IsNullOrEmpty(rectificada.FechaExpedicionFactura))
                        result.Add($"Error en el bloque RegistroAlta.FacturasRectificadas ({rectificada}):" +
                                $" El campo FechaExpedicionFactura es obligatorio");


                }

            }

            if (string.IsNullOrEmpty(_RegistroAlta.DescripcionOperacion))
                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" El campo DescripcionOperacion es obligatorio");

            // Tercero
            if (_RegistroAlta.Tercero != null) 
            {

                if (string.IsNullOrEmpty(_RegistroAlta.Tercero.NombreRazon))
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                            $" El campo Tercero.NombreRazon es obligatorio");

                if (_RegistroAlta.Tercero.IDOtro == null  && string.IsNullOrEmpty(_RegistroAlta.Tercero.NIF))
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                            $" El campo Tercero.NIF es obligatorio");

                if (_RegistroAlta.Tercero.IDOtro != null && string.IsNullOrEmpty(_RegistroAlta.Tercero.IDOtro.ID))
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                            $" El campo Tercero.IDOtro.ID es obligatorio");

            }


            // Destinatarios

            if (_RegistroAlta.Destinatarios?.Count > 0)
            {

                foreach (var destinatario in _RegistroAlta.Destinatarios)
                {

                    if (string.IsNullOrEmpty(destinatario.NombreRazon))
                        result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                                $" El campo Destinatarios.NombreRazon es obligatorio");

                    if (destinatario.IDOtro == null && string.IsNullOrEmpty(destinatario.NIF))
                        result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                                $" El campo Destinatarios.NIF es obligatorio");

                    if (destinatario.IDOtro != null && string.IsNullOrEmpty(destinatario.IDOtro.ID))
                        result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                                $" El campo Destinatarios.IDOtro.ID es obligatorio");


                }

            }


            if (string.IsNullOrEmpty(_RegistroAlta.CuotaTotal))
                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" El campo CuotaTotal es obligatorio");


            if (string.IsNullOrEmpty(_RegistroAlta.ImporteTotal))
                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" El campo ImporteTotal es obligatorio");

            return result;

        }

        #endregion

    }

}
