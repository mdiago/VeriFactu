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
using VeriFactu.Business.Validation.Validators;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Anulacion;
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
    public class InvoiceValiation : IValidator
    {


        /// <summary>
        /// Objeto InvoiceAction en el caso de que la validación se
        /// haya asignado al mismo.
        /// </summary>
        InvoiceAction _InvoiceAction;

        /// <summary>
        /// Objeto Envelope a validar.
        /// </summary>
        internal Envelope _Envelope;

        /// <summary>
        /// Objeto RegFactuSistemaFacturacion contenido en el bloque
        /// Body del objeto Envelope a validar.
        /// </summary>
        protected RegFactuSistemaFacturacion _RegFactuSistemaFacturacion;



        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="invoiceAction">Objeto InvoiceAction a validar.</param>
        public InvoiceValiation(InvoiceAction invoiceAction) : this(invoiceAction.Envelope)
        {

            _InvoiceAction = invoiceAction;
            _RegFactuSistemaFacturacion = _Envelope.Body.Registro as RegFactuSistemaFacturacion;

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="envelope">Objeto Envelope a validar.</param>
        public InvoiceValiation(Envelope envelope)
        {

            _Envelope = envelope;

            _RegFactuSistemaFacturacion = envelope.Body.Registro as RegFactuSistemaFacturacion;

            if (_RegFactuSistemaFacturacion == null)
                throw new Exception($"El registro Envelope ({_Envelope})" +
                    $" no contiene un valor del tipo 'RegFactuSistemaFacturacion'.");

        }

        /// <summary>
        /// Ejecuta las validaciones del obejeto de negocio.
        /// </summary>
        public virtual List<string> GetErrors() 
        {

            var result = new List<string>();

            var errorsCabecera = new ValidatorCabecera(_Envelope).GetErrors();
            var errorsRegistrosFactura = new ValidatorRegistroFactura(_Envelope).GetErrors();
            

            result.AddRange(errorsCabecera);
            result.AddRange(errorsRegistrosFactura);

            return result;

        } 


        /// <summary>
        /// Validaciones del bloque de todos los items de RegistroFactura.
        /// </summary>
        /// <returns>Lista con los errores encontrados.</returns>
        public List<string> GetErrorsRegistroAnulacion(RegistroAnulacion registroAnulacion)
        {

            var result = new List<string>();


            return result;


        }


    }
}
