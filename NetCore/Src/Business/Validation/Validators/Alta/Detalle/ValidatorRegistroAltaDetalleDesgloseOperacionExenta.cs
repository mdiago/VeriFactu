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

using System.Collections.Generic;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators.Alta.Detalle
{

    /// <summary>
    /// Valida los datos de RegistroAlta DetalleDesglose OperacionExenta.
    /// </summary>
    public class ValidatorRegistroAltaDetalleDesgloseOperacionExenta : ValidatorRegistroAlta
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
        public ValidatorRegistroAltaDetalleDesgloseOperacionExenta(Envelope envelope, RegistroAlta registroAlta,
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

            // Si Impuesto = “01” (IVA) o no se cumplimenta (considerándose “01” - IVA), el valor 
            // de OperacionExenta deberá estar contenido en lista L10 (No inlcuye 'E7' ni 'E8').
            if (_DetalleDesglose.Impuesto == Impuesto.IVA && _DetalleDesglose.OperacionExentaSpecified && 
                (_DetalleDesglose.OperacionExenta == CausaExencion.E7 || _DetalleDesglose.OperacionExenta == CausaExencion.E8))
            {

                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                     $" Si Impuesto = “01” (IVA) o no se cumplimenta (considerándose “01” - IVA), el valor" +
                     $" de OperacionExenta deberá estar contenido en lista L10 (No inlcuye 'E7' ni 'E8').");

            }

            //Si Impuesto = “03” (IGIC), el valor de OperacionExenta deberá estar contenido en 
            //lista L10 y adicionalmente podrá contener los valores “E7” y “E8”.


            // Si Impuesto = “01” (IVA), “03” (IGIC) o no se cumplimenta (considerándose “01” - IVA),
            // y ClaveRegimen es igual a “01”, no pueden marcarse los valores de OperacionExenta “E2” y “E3”.

            if ((_DetalleDesglose.Impuesto == Impuesto.IVA || _DetalleDesglose.Impuesto == Impuesto.IGIC) &&
                _DetalleDesglose.ClaveRegimen == ClaveRegimen.RegimenGeneral &&
                (_DetalleDesglose.OperacionExentaSpecified &&
                (_DetalleDesglose.OperacionExenta == CausaExencion.E2 ||
                _DetalleDesglose.OperacionExenta == CausaExencion.E3))) 
            {

                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                     $" Si Impuesto = “01” (IVA), “03” (IGIC) o no se cumplimenta (considerándose “01” - IVA)," +
                     $" y ClaveRegimen es igual a “01”, no pueden marcarse los valores de OperacionExenta “E2” y “E3”.");


            }

            var tipoImpositivo = XmlParser.ToDecimal(_DetalleDesglose.TipoImpositivo);
            var cuotaRepercutida = XmlParser.ToDecimal(_DetalleDesglose.CuotaRepercutida);
            var tipoImpositivoRE = XmlParser.ToDecimal(_DetalleDesglose.TipoRecargoEquivalencia);
            var cuotaRepercutidaRE = XmlParser.ToDecimal(_DetalleDesglose.CuotaRecargoEquivalencia);

            // Si el campo OperacionExenta está cumplimentado no se pueden
            // informar ninguno de estos campos:
            // TipoImpositivo, CuotaRepercutida,
            // TipoRecargoEquivalencia y CuotaRecargoEquivalencia.
            if (_DetalleDesglose.OperacionExentaSpecified) 
            { 
            
                if(tipoImpositivo != 0)
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                         $" Si el campo OperacionExenta está cumplimentado con cualquier valor de la lista L10 no se puede" +
                         $" informar el campo TipoImpositivo.");

                if (cuotaRepercutida != 0)
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                         $" Si el campo OperacionExenta está cumplimentado con cualquier valor de la lista L10 no se puede" +
                         $" informar el campo CuotaRepercutida.");


                if (tipoImpositivoRE != 0)
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                         $" Si el campo OperacionExenta está cumplimentado con cualquier valor de la lista L10 no se puede" +
                         $" informar el campo TipoRecargoEquivalencia.");


                if (cuotaRepercutidaRE != 0)
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                         $" Si el campo OperacionExenta está cumplimentado con cualquier valor de la lista L10 no se puede" +
                         $" informar el campo CuotaRecargoEquivalencia.");

            }

            return result;

        }

        #endregion

    }

}