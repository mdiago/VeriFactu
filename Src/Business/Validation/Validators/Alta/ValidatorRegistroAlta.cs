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
using System.Text.RegularExpressions;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators.Alta
{

    /// <summary>
    /// Valida los datos de RegistroAlta.
    /// </summary>
    public class ValidatorRegistroAlta : InvoiceValiation
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Registro de alta a validar.
        /// </summary>
        protected RegistroAlta _RegistroAlta;

        /// <summary>
        /// Cabecera 
        /// </summary>
        protected Cabecera _Cabecera;

        /// <summary>
        /// Fecha operación.
        /// </summary>
        protected DateTime? _FechaOperacion = null;

        /// <summary>
        /// Fecha operación.
        /// </summary>
        protected DateTime _FechaExpedicion;

        /// <summary>
        /// Indicador de si la factura es rectificativa.
        /// </summary>
        protected bool _IsRectificativa = false;

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        public ValidatorRegistroAlta(Envelope envelope, RegistroAlta registroAlta) : base(envelope)
        {

            _RegistroAlta = registroAlta;
            _Cabecera = _RegFactuSistemaFacturacion?.Cabecera;

            var fechaOperacion = _RegistroAlta?.FechaOperacion;

            if (!string.IsNullOrEmpty(fechaOperacion))
                _FechaOperacion = FromXmlDate(fechaOperacion);

            var fechaExpedicion = _RegistroAlta?.IDFacturaAlta?.FechaExpedicion;

            if (string.IsNullOrEmpty(fechaExpedicion) && !Regex.IsMatch(fechaExpedicion, @"\d{2}-\d{2}-\d{4}"))
                throw new ArgumentException($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                $" La propiedad IDFactura.FechaExpedicion tiene que tener un valor con formato dd-mm-yyyy.");

            _FechaExpedicion = FromXmlDate(fechaExpedicion);

            _IsRectificativa = Array.IndexOf(new TipoFactura[]{ TipoFactura.R1, TipoFactura.R2,
                TipoFactura.R3, TipoFactura.R4, TipoFactura.R5 }, _RegistroAlta.TipoFactura) != -1;

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

            // 1. Agrupación IDFactura
            result.AddRange(new ValidatorRegistroAltaIDFactura(_Envelope, _RegistroAlta).GetErrors());
            // 2. RechazoPrevio
            result.AddRange(new ValidatorRegistroAltaRechazoPrevio(_Envelope, _RegistroAlta).GetErrors());
            // 3. TipoRectificativa
            result.AddRange(new ValidatorRegistroAltaTipoRectificativa(_Envelope, _RegistroAlta).GetErrors());
            // 4. Agrupación FacturasRectificadas
            result.AddRange(new ValidatorRegistroAltaFacturasRectificadas(_Envelope, _RegistroAlta).GetErrors());
            // 5. Agrupación FacturasSustituidas
            result.AddRange(new ValidatorRegistroAltaFacturasSustituidas(_Envelope, _RegistroAlta).GetErrors());
            // 6. Agrupación ImporteRectificacion
            result.AddRange(new ValidatorRegistroAltaImporteRectificacion(_Envelope, _RegistroAlta).GetErrors());
            // 7. FechaOperacion
            result.AddRange(new ValidatorRegistroAltaFechaOperacion(_Envelope, _RegistroAlta).GetErrors());
            // 8. FacturaSimplificadaArt7273
            result.AddRange(new ValidatorRegistroAltaFacturaSimplificadaArt7273(_Envelope, _RegistroAlta).GetErrors());
            // 9. FacturaSinIdentifDestinatarioArt61d
            result.AddRange(new ValidatorRegistroAltaFacturaSinIdentifDestinatarioArt61d(_Envelope, _RegistroAlta).GetErrors());
            // 10. Macrodato
            result.AddRange(new ValidatorRegistroAltaMacrodato(_Envelope, _RegistroAlta).GetErrors());
            // 11. EmitidaPorTerceroODestinatario
            result.AddRange(new ValidatorRegistroAltaEmitidaPorTerceroODestinatario(_Envelope, _RegistroAlta).GetErrors());
            // 12. Agrupación Tercero
            result.AddRange(new ValidatorRegistroAltaTercero(_Envelope, _RegistroAlta).GetErrors());
            // 13. Agrupación Destinatarios
            result.AddRange(new ValidatorRegistroAltaDestinatarios(_Envelope, _RegistroAlta).GetErrors());

            return result;

        }

        /// <summary>
        /// Obtiene un DateTime de la cadena de representación
        /// de una fecha en un archivo xml.
        /// </summary>
        /// <param name="xmlDate">Cadena de fecha xml.</param>
        /// <returns>DateTime de la cadena de representación
        /// de una fecha en un archivo xml.</returns>
        protected DateTime FromXmlDate(string xmlDate)
        {

            var year = Convert.ToInt32(xmlDate.Substring(6, 4));
            var month = Convert.ToInt32(xmlDate.Substring(3, 2));
            var day = Convert.ToInt32(xmlDate.Substring(0, 2));

            return new DateTime(year, month, day);

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
