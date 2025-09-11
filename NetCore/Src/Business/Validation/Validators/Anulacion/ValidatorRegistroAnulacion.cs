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
using VeriFactu.Xml.Factu.Anulacion;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators.Anulacion
{

    /// <summary>
    /// Valida los datos de RegistroAnulacion.
    /// </summary>
    public class ValidatorRegistroAnulacion : InvoiceValidation
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Registro de alta a validar.
        /// </summary>
        protected RegistroAnulacion _RegistroAnulacion;

        /// <summary>
        /// Cabecera 
        /// </summary>
        protected Cabecera _Cabecera;

        /// <summary>
        /// Fecha operación.
        /// </summary>
        protected DateTime _FechaExpedicion;

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="envelope"> Envelope de envío al
        /// servicio Verifactu de la AEAT.</param>
        /// <param name="registroAnulacion"> Registro de anulación del bloque Body.</param>
        public ValidatorRegistroAnulacion(Envelope envelope, RegistroAnulacion registroAnulacion) : base(envelope)
        {

            _RegistroAnulacion = registroAnulacion;
            _Cabecera = _RegFactuSistemaFacturacion?.Cabecera;

            var fechaExpedicion = _RegistroAnulacion?.IDFacturaAnulada?.FechaExpedicion;

            if (string.IsNullOrEmpty(fechaExpedicion) && !Regex.IsMatch(fechaExpedicion, @"\d{2}-\d{2}-\d{4}"))
                throw new ArgumentException($"Error en el bloque RegistroAlta ({_RegistroAnulacion}):" +
                $" La propiedad IDFactura.FechaExpedicion tiene que tener un valor con formato dd-mm-yyyy.");

            _FechaExpedicion = FromXmlDate(fechaExpedicion);

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Obtiene los errores de un bloque en concreto.
        /// </summary>
        /// <returns>Lista con los errores de un bloque en concreto.</returns>
        protected virtual List<string> GetBlockErrors()
        {

            var result = new List<string>();

            // 1. GeneradoPor
            result.AddRange(new ValidatorRegistroAnulacionGeneradoPor(_Envelope, _RegistroAnulacion).GetErrors());
            // 2. Agrupación Generador
            result.AddRange(new ValidatorRegistroAnulacionGenerador(_Envelope, _RegistroAnulacion).GetErrors());
            // 3. Huella (del registro anterior)
            result.AddRange(new ValidatorRegistroAnulacionHuella(_Envelope, _RegistroAnulacion).GetErrors());

            // Validaciones de textos
            result.AddRange(new ValidatorText("IDVersion", _RegistroAnulacion.IDVersion, 3).GetErrors());
            result.AddRange(new ValidatorText("RefExterna", _RegistroAnulacion.RefExterna, 60).GetErrors());
            result.AddRange(new ValidatorText("SinRegistroPrevio", _RegistroAnulacion.SinRegistroPrevio, 1, @"[SN]").GetErrors());
            result.AddRange(new ValidatorText("RechazoPrevio", _RegistroAnulacion.RechazoPrevio, 1, @"[SN]").GetErrors());
            result.AddRange(new ValidatorText("GeneradoPor", _RegistroAnulacion.GeneradoPor, 1, @"[EDT]").GetErrors());
            result.AddRange(new ValidatorText("OrderedHuella", _RegistroAnulacion.OrderedHuella, 64).GetErrors());

            return result;

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

            return GetBlockErrors();

        }

        #endregion

    }

}