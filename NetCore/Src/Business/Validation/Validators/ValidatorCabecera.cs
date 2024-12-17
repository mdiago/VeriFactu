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

            var result = new List<string>();

            var cabecera = _RegFactuSistemaFacturacion?.Cabecera;

            // 1. ObligadoEmision: El NIF del obligado a expedir (emitir) facturas asociado a la remisión debe estar identificado en la AEAT.

            if (cabecera?.ObligadoEmision?.NIF == null)
                result.Add("Error en el bloque Cabecera: El NIF del bloque ObligadoEmision debe contener un valor");

            if(!Settings.Current.SkipNifAeatValidation)
                result.AddRange(new NifValidation(cabecera.ObligadoEmision.NIF, cabecera.ObligadoEmision.NombreRazon).GetErrors());

            // 2. Representante: El NIF del representante/asesor del obligado a expedir (emitir) facturas asociado
            // a la remisión debe estar identificado en la AEAT.
            if (cabecera.Representante != null)
            {

                if (!Settings.Current.SkipNifAeatValidation)
                    result.AddRange(new NifValidation(cabecera.Representante.NIF, cabecera.Representante.NombreRazon).GetErrors());

            }

            // 3. FechaFinVeriFactu
            if (cabecera.FechaFinVeriFactu != null)
            {

                // Sólo se permite contenido en sistemas que emite facturas verificables (Es el caso siempre)
                // La fecha debe tener el formato 31-12-20XX.
                if (!Regex.IsMatch(cabecera.FechaFinVeriFactu, @"31-12-20\d{2}"))
                    result.Add("Error en el bloque Cabecera: La fecha FechaFinVeriFactu debe tener el formato 31-12-20XX.");

                // El año de la fecha deberá ser igual al año de la fecha del sistema de la AEAT, o al año anterior(para admitir
                // casos excepcionales y puntuales que pudieran darse a finales de año y comienzo del siguiente).
                var fechaFinVeriFactuYear = Convert.ToInt32(cabecera.FechaFinVeriFactu.Substring(6, 4));

                if (fechaFinVeriFactuYear > DateTime.Now.Year && fechaFinVeriFactuYear < DateTime.Now.Year - 1)
                    result.Add("Error en el bloque Cabecera: El año de la fecha FechaFinVeriFactu deberá ser igual" +
                        " al año de la fecha del sistema de la AEAT, o al año anterior.");

            }

            // 4. Incidencia: Sólo se permite contenido en sistemas que emite facturas verificables. (Es el caso siempre)

            // 5. RefRequerimiento: Sólo se permite contenido en sistemas que emiten facturas no verificables. (En nuestro caso no existe el bloque)

            return result;

        }

        #endregion

    }

}
