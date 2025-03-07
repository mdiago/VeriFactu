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
using VeriFactu.Business.Operations;
using VeriFactu.Business.Validation.Validators;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation
{

    /// <summary>
    /// Implementa las validaciones establecidas en el documento
    /// de especificaciones de Validaciones.
    /// <para>Documento: Validaciones_Errores_Veri-Factu_BORRADOR.pdf</para>
    /// <para>Fecha: 2024-10-15</para>
    /// <para>Versión: 0.9.1</para>
    /// </summary>
    public class InvoiceValidation : IValidator
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Objeto Envelope a validar.
        /// </summary>
        internal Envelope _Envelope;

        /// <summary>
        /// Objeto RegFactuSistemaFacturacion contenido en el bloque
        /// Body del objeto Envelope a validar.
        /// </summary>
        protected RegFactuSistemaFacturacion _RegFactuSistemaFacturacion;

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="invoiceAction">Objeto InvoiceAction a validar.</param>
        public InvoiceValidation(InvoiceAction invoiceAction) : this(invoiceAction.Envelope)
        {

            _RegFactuSistemaFacturacion = _Envelope.Body.Registro as RegFactuSistemaFacturacion;

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="envelope">Objeto Envelope a validar.</param>
        public InvoiceValidation(Envelope envelope)
        {

            _Envelope = envelope;

            _RegFactuSistemaFacturacion = envelope.Body.Registro as RegFactuSistemaFacturacion;

            if (_RegFactuSistemaFacturacion == null)
                throw new Exception($"El registro Envelope ({_Envelope})" +
                    $" no contiene un valor del tipo 'RegFactuSistemaFacturacion'.");

        }

        #endregion

        #region Métodos Privados Estáticos

        /// <summary>
        /// Obtiene un DateTime de la cadena de representación
        /// de una fecha en un archivo xml.
        /// </summary>
        /// <param name="xmlDate">Cadena de fecha xml.</param>
        /// <returns>DateTime de la cadena de representación
        /// de una fecha en un archivo xml.</returns>
        protected static DateTime FromXmlDate(string xmlDate)
        {

            var year = Convert.ToInt32(xmlDate.Substring(6, 4));
            var month = Convert.ToInt32(xmlDate.Substring(3, 2));
            var day = Convert.ToInt32(xmlDate.Substring(0, 2));

            return new DateTime(year, month, day);

        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Ejecuta las validaciones del obejeto de negocio.
        /// </summary>
        /// <returns>Lista con las descripciones de los errores
        /// encontrados.</returns>
        public virtual List<string> GetErrors()
        {

            var result = new List<string>();

            var errorsCabecera = new ValidatorCabecera(_Envelope).GetErrors();
            var errorsRegistrosFactura = new ValidatorRegistroFactura(_Envelope).GetErrors();
            var errorsSistemaInrformatico = new ValidatorSistemaInformatico(_Envelope).GetErrors();

            result.AddRange(errorsCabecera);
            result.AddRange(errorsRegistrosFactura);
            result.AddRange(errorsSistemaInrformatico);

            return result;

        }

        #endregion

    }

}