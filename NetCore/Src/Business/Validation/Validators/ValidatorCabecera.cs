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
using VeriFactu.Business.Validation.NIF;
using VeriFactu.Config;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators
{

    /// <summary>
    /// Valida los datos de Cabecera.
    /// </summary>
    public class ValidatorCabecera : InvoiceValidation
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="envelope">Envelope de envío al
        /// servicio Verifactu de la AEAT.</param>
        public ValidatorCabecera(Envelope envelope) : base(envelope)
        {
        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Ejecuta las validaciones y devuelve una lista
        /// con los errores encontrados.
        /// </summary>
        /// <returns>Lista con las descripciones de los 
        /// errores encontrado.</returns>
        public override List<string> GetErrors()
        {

            // 3.1.1 Validaciones de negocio del bloque Cabecera.

            var result = new List<string>();

            var cabecera = _RegFactuSistemaFacturacion?.Cabecera;

            // 1. ObligadoEmision: El NIF del obligado a expedir (emitir) facturas asociado a la remisión debe estar identificado en la AEAT.
            //  4104 = Error en la cabecera: el valor del campo NIF del bloque ObligadoEmision no está identificado.

            if (cabecera?.ObligadoEmision?.NIF == null)
                result.Add("[3.1.1-1.0] Error en el bloque Cabecera: El NIF del bloque ObligadoEmision debe contener un valor");

            // El interlocutor en ObligadoEmision sólo puede contener datos en NIF y NombreRazon
            if (cabecera?.ObligadoEmision?.NombreRazonRepresentante != null ||
                cabecera?.ObligadoEmision?.NIFRepresentante != null ||
                cabecera?.ObligadoEmision?.IDOtro != null)
                result.Add("[3.1.1-1.1] El campo 'ObligadoEmision' no puede contener información en 'NombreRazonRepresentante' o 'NIFRepresentante'.");

            if (!Settings.Current.SkipNifAeatValidation) // 4107 = El NIF no está identificado en el censo de la AEAT.
            {

                var errors = new NifValidation(cabecera.ObligadoEmision.NIF, cabecera.ObligadoEmision.NombreRazon).GetErrors();

                foreach (var error in errors)
                    result.Add($"[3.1.1-1.2] {error}");

            }

            // 2. Representante: El NIF del representante/asesor del obligado a expedir (emitir) facturas asociado
            // a la remisión debe estar identificado en la AEAT.
            //  4105 = Error en la cabecera: el valor del campo NIF del bloque Representante no está identificado.
            if (cabecera.Representante != null)
            {

                if (cabecera?.Representante?.NIF == null)
                    result.Add("[3.1.1-2.0] Error en el bloque Cabecera: El valor del campo NIF del bloque Representante no está identificado");

                // 4107 = El NIF no está identificado en el censo de la AEAT.
                // 4123 = Error en la cabecera: el valor del campo NIF del bloque Representante no está identificado en el censo de la AEAT.
                // 4124 = Error en la cabecera: el valor del campo Nombre del bloque Representante no está identificado en el censo de la AEAT.
                if (!Settings.Current.SkipNifAeatValidation)
                {

                    var errors = new NifValidation(cabecera.Representante.NIF, cabecera.Representante.NombreRazon).GetErrors();

                    foreach (var error in errors)
                        result.Add($"[3.1.1-2.1] {error}");
                
                }

                // El interlocutor en Representante sólo puede contener datos en NIF y NombreRazon
                if (cabecera.Representante.NombreRazonRepresentante != null ||
                    cabecera.Representante.NIFRepresentante != null ||
                    cabecera?.Representante?.IDOtro != null)
                    result.Add($"[3.1.1-2.2] Error en el bloque Representante ({cabecera.Representante}): " +
                        "Los datos de interlocutor 'NombreRazonRepresentante' y 'NIFRepresentante'" +
                        " no pueden contener valor.");


            }

            // 3. FechaFinVeriFactu
            //  4120 = Error en la cabecera: el valor del campo FechaFinVeriFactu es incorrecto, debe ser 31-12-20XX, donde XX corresponde con el año actual o el anterior.
            if (cabecera.RemisionVoluntaria?.FechaFinVeriFactu != null)
            {

                // Sólo se permite contenido en sistemas que emite facturas verificables (Es el caso siempre)
                // La fecha debe tener el formato 31-12-20XX.
                if (!Regex.IsMatch(cabecera.RemisionVoluntaria.FechaFinVeriFactu, @"31-12-20\d{2}"))
                    result.Add("[3.1.1-3.0] Error en el bloque Cabecera: La fecha FechaFinVeriFactu debe tener el formato 31-12-20XX.");

                // El año de la fecha deberá ser igual al año de la fecha del sistema de la AEAT, o al año anterior(para admitir
                // casos excepcionales y puntuales que pudieran darse a finales de año y comienzo del siguiente).
                var fechaFinVeriFactuYear = Convert.ToInt32(cabecera.RemisionVoluntaria.FechaFinVeriFactu.Substring(6, 4));

                if (fechaFinVeriFactuYear > DateTime.Now.Year || fechaFinVeriFactuYear < DateTime.Now.Year - 1)
                    result.Add("[3.1.1-3.1] Error en el bloque Cabecera: El año de la fecha FechaFinVeriFactu deberá ser igual" +
                        " al año de la fecha del sistema de la AEAT, o al año anterior.");

            }

            // 4. Incidencia: Sólo se permite contenido en sistemas que emite facturas verificables. (Es el caso siempre)

            // 5. RefRequerimiento: Sólo se permite contenido en sistemas que emiten facturas no verificables. 
            if(cabecera.RemisionRequerimiento != null)
                result.Add("[3.1.1-5.0] Error en el bloque Cabecera: RefRequerimiento: Sólo se permite contenido en sistemas que emiten facturas no verificables," +
                        " la biblioteca Verifactu ha sido diseñada para funcionar únicamente como sistema verificable.");

            return result;

        }

        #endregion

    }

}