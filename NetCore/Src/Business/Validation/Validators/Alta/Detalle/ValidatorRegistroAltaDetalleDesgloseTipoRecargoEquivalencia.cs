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
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators.Alta.Detalle
{

    /// <summary>
    /// Valida los datos de RegistroAlta DetalleDesglose TipoRecargoEquivalencia.
    /// </summary>
    public class ValidatorRegistroAltaDetalleDesgloseTipoRecargoEquivalencia : ValidatorRegistroAlta
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
        public ValidatorRegistroAltaDetalleDesgloseTipoRecargoEquivalencia(Envelope envelope, RegistroAlta registroAlta,
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

            var tipoImpositivo = XmlParser.ToDecimal(_DetalleDesglose.TipoImpositivo);
            var tipoImpositivoRE = XmlParser.ToDecimal(_DetalleDesglose.TipoRecargoEquivalencia);

            if (tipoImpositivoRE == 0)
                return result;

            // Si Impuesto = “01” (IVA) o no se cumplimenta (considerándose “01” - IVA) y CalificacionOperacion = “S1”:
            if (_DetalleDesglose.Impuesto == Impuesto.IVA && _DetalleDesglose.CalificacionOperacion == CalificacionOperacion.S1) 
            {

                // Solo se permiten TipoRecargoEquivalencia = 0; 0,26; 0,5; 0,62; 1; 1,4; 1,75;5,2 (valores que indican el tanto por ciento).
                var allowedRates = new decimal[] { 0m, 0.26m, 0.5m, 0.62m, 1m, 1.4m, 1.75m, 5.2m };

                // Si FechaOperacion (FechaExpedicionFactura de la agrupación IDFactura si no se informa FechaOperacion)
                // ≥ 1 de julio de 2022 y ≤ 30 de septiembre de 2024 se admitirá TipoImpositivo = 5.
                var fechaOperacion = _FechaOperacion ?? _FechaExpedicion;
                var allowedFive = fechaOperacion.CompareTo(new DateTime(2022, 7, 1)) >= 0 && fechaOperacion.CompareTo(new DateTime(2024, 9, 30)) <= 0;

                var isTipoImpositivoOK = Array.IndexOf(allowedRates, tipoImpositivoRE) != -1;

                if(!isTipoImpositivoOK)
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                        $" Solo se permiten TipoRecargoEquivalencia = {string.Join(", ", allowedRates)}" +
                        $" en la fecha {fechaOperacion:yyyy-MM-dd} (valores que indican el tanto por ciento).");

                switch (tipoImpositivo)
                {

                    case 21m:

                        // Si TipoImpositivo es 21 sólo se admitirán TipoRecargoEquivalencia = 5,2 ó 1,75.
                        if (tipoImpositivoRE != 5.2m && tipoImpositivo != 1.75m)
                            result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                                $" Si TipoImpositivo es 21 sólo se admitirán TipoRecargoEquivalencia = 5,2 ó 1,75.");

                        break;

                    case 10m:

                        // Si TipoImpositivo es 10 sólo se admitirá TipoRecargoEquivalencia = 1,4.
                        if (tipoImpositivoRE != 1.4m)
                            result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                                $" Si TipoImpositivo es 10 sólo se admitirá TipoRecargoEquivalencia = 1,4.");

                        break;

                    case 7.5m:

                        // Si TipoImpositivo es 7,5 sólo se admitirá TipoRecargoEquivalencia = 1:

                        // Si FechaOperacion (FechaExpedicionFactura de la agrupación IDFactura si
                        // no se informa FechaOperacion) es mayor o igual que 1 de octubre de 2024
                        // y menor o igual que 31 de diciembre de 2024 se admitirá el TipoRecargoEquivalencia = 1.
                        if (fechaOperacion.CompareTo(new DateTime(2024, 10, 1)) >= 0 && fechaOperacion.CompareTo(new DateTime(2024, 12, 31)) <= 0)
                        {

                            if (tipoImpositivoRE != 1m)
                                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                                    $" Si TipoImpositivo es 7.5 sólo se admitirá TipoRecargoEquivalencia = 1.");
                        }
                        else
                        {

                            result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                                    $" Si TipoImpositivo es 7.5 sólo se admitirá TipoRecargoEquivalencia = 1" +
                                    $" en el periodo entre 01-10-2024 y 31-12-2024.");

                        }

                        break;

                    case 5m:

                        // Si tipo impositivo es 5:

                        // Si FechaOperacion (FechaExpedicionFactura de la agrupación IDFactura si no se informa FechaOperacion)
                        // es igual o inferior al 31 de diciembre de 2022, solo se admitirá TipoRecargoEquivalencia = 0,5.

                        //Si FechaOperacion (FechaExpedicionFactura de la agrupación IDFactura si no se informa FechaOperacion)
                        //es mayor o igual que 1 de enero de 2023 y menor o igual que 30 de septiembre de 2024, solo se admitirá
                        // TipoRecargoEquivalencia = 0,62.

                        if (fechaOperacion.CompareTo(new DateTime(2022, 12, 31)) <= 0 && tipoImpositivoRE != 0.5m)
                            result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                                    $" Si TipoImpositivo es 5 sólo se admitirá TipoRecargoEquivalencia = 0.5" +
                                    $" en el periodo igual o anterior a 31-12-2022.");
                        else if (fechaOperacion.CompareTo(new DateTime(2023, 1, 1)) >= 0 && fechaOperacion.CompareTo(new DateTime(2024, 09, 30)) <= 0)
                            result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                                    $" Si TipoImpositivo es 5 sólo se admitirá TipoRecargoEquivalencia = 0.62" +
                                    $" en el periodo del 01-01-2023 al 30-09-2024.");
                        else
                            result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                                    $" Si TipoImpositivo es 5 sólo no se admitirá TipoRecargoEquivalencia" +
                                    $" en el periodo posterior a 30-09-2024.");

                        break;

                    case 4m:

                        // Si TipoImpositivo es 4 sólo se admitirá TipoRecargoEquivalencia = 0,5.
                        if (tipoImpositivoRE != 0.5m)
                            result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                                $" Si TipoImpositivo es 4 sólo se admitirá TipoRecargoEquivalencia = 0,5.");

                        break;

                    case 2m:

                        // Si TipoImpositivo es 2 sólo se admitirá TipoRecargoEquivalencia = 0,26.

                        // Si FechaOperacion (FechaExpedicionFactura de la agrupación IDFactura si no se informa FechaOperacion)
                        // es mayor o igual que 1 de octubre de 2024 y menor o igual que 31 de diciembre de 2024
                        // se admitirá el TipoRecargoEquivalencia = 0,26.
                        if (fechaOperacion.CompareTo(new DateTime(2024, 10, 1)) >= 0 && fechaOperacion.CompareTo(new DateTime(2024, 12, 31)) <= 0)
                        {

                            if (tipoImpositivoRE != 0.26m)
                                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                                    $" Si TipoImpositivo es 2 sólo se admitirá TipoRecargoEquivalencia = 0.26" +
                                    $" en el periodo del 01-10-2024 al 31-12-2024.");

                        }
                        else 
                        {

                            result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                                $" Si TipoImpositivo es 2 sólo se admitirá TipoRecargoEquivalencia = 0.26" +
                                $" en el periodo del 01-10-2024 al 31-12-2024.");

                        }

                        break;

                    case 0m:

                        //Si tipo impositivo es 0:

                        // Si FechaOperacion (FechaExpedicionFactura de la agrupación IDFactura si no se informa FechaOperacion)
                        // es mayor o igual que 1 de enero de 2023 y menor o igual que 30 de septiembre de 2024,
                        // solo se admitirá TipoRecargoEquivalencia = 0.

                        if (fechaOperacion.CompareTo(new DateTime(2023, 1, 1)) >= 0 && fechaOperacion.CompareTo(new DateTime(2024, 9, 30)) <= 0)
                        {

                            if (tipoImpositivoRE != 0m)
                                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                                    $" Si TipoImpositivo es 0 sólo se admitirá TipoRecargoEquivalencia = 0" +
                                    $" en el periodo del 01-10-2024 al 30-09-2024.");

                        }


                        break;

                }

            }

            return result;

        }

        #endregion

    }

}
