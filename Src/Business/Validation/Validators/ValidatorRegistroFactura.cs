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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VeriFactu.Business.Validation.NIF;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Factu.Anulacion;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators
{

    /// <summary>
    /// Valida los datos de RegistroFactura.
    /// </summary>
    public class ValidatorRegistroFactura : InvoiceValiation
    {

        /// <summary>
        /// Constructor.
        /// </summary>
        public ValidatorRegistroFactura(Envelope envelope) : base(envelope) 
        {
        }

        /// <summary>
        /// Ejecuta las validaciones y devuelve una lista
        /// con los errores encontrados.
        /// </summary>
        /// <returns>Lista con las descripciones de los 
        /// errores encontrado.</returns>
        public override List<string> GetErrors()
        {

            var result = new List<string>();

            if (_RegFactuSistemaFacturacion.RegistroFactura.Count > 10000)
                result.Add($"La colección RegFactuSistemaFacturacion.RegistroFactura" +
                    $" contiene {_RegFactuSistemaFacturacion.RegistroFactura.Count}" +
                    $" elementos cuando sólo está permitido un máximo de 1000.");

            //1. Agrupaciones RegistroAlta y RegistroAnulacion: Dentro de cada una de las posibles repeticiones
            //u “ocurrencias” de RegistroFactura (de 1 a 1000) se pueden incluir registros de facturación de alta
            //(agrupación RegistroAlta) y de anulación(agrupación RegistroAnulacion) en un mismo mensaje remitido,
            //pero siempre que vayan en distintas ocurrencias de RegistroFactura(no pueden ir ambas agrupaciones a
            //la vez dentro de la misma ocurrencia). ?????????????????????

            foreach (var registroFactura in _RegFactuSistemaFacturacion.RegistroFactura)
                result.AddRange(GetErrorsRegistroFactura(registroFactura));

            return result;

        }

        /// <summary>
        /// Validaciones del bloque de todos los items de RegistroFactura.
        /// </summary>
        /// <returns>Lista con los errores encontrados.</returns>
        public List<string> GetErrorsRegistroFactura(object registro)
        {

            var result = new List<string>();

            var registroFactura = registro as RegistroFactura;

            if (registroFactura == null)
                result.Add($"Error en el bloque RegistroFactura: Se ha encontrado un" +
                    $" elemento de la clase {registro.GetType()} en la colección RegistroFactura" +
                    $". Esta colección sólo admite elementos del tipo RegistroFactura.");

            var registroAlta = registroFactura.Registro as RegistroAlta;
            var registroAnulacion = registroFactura.Registro as RegistroAnulacion;

            if (registroAlta == null && registroAnulacion == null)
                result.Add($"Error en el bloque RegistroFactura.Registro: Se ha encontrado un" +
                    $" elmento de la clase {registro.GetType()} en la colección RegistroFactura" +
                    $". Esta colección sólo admite elementos del tipo RegistroAlta o RegistroAnulacion.");

            //if (registroAlta != null)
            //    result.AddRange(GetErrorsRegistroAlta(registroAlta));

            if (registroAlta != null)
                new ValidatorRegistroAlta(_Envelope, registroAlta).GetErrors();


            if (registroAnulacion != null)
                result.AddRange(GetErrorsRegistroAnulacion(registroAnulacion));

            return result;

        }

    }

}
