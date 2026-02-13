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
    /// Valida los datos de RegistroAlta DetalleDesglose CalificacionOperacion.
    /// </summary>
    public class ValidatorRegistroAltaDetalleDesgloseCalificacionOperacion : ValidatorRegistroAlta
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
        public ValidatorRegistroAltaDetalleDesgloseCalificacionOperacion(Envelope envelope, RegistroAlta registroAlta,
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

            // 3.1.3 Validaciones de negocio de la agrupación RegistroAlta en el bloque de RegistroFactura.
            //      15. Agrupación Desglose / DetalleDesglose. 

            var result = new List<string>();

            // Si CalificacionOperacion es “S2”, TipoFactura solo puede ser “F1”, “F3”, “R1”, “R2”, “R3” y “R4”.            

            if (_DetalleDesglose.CalificacionOperacion == CalificacionOperacion.S2 && _IsSimplificada)
                result.Add($"[3.1.3-15.4.0] Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                      $"Si CalificacionOperacion es “S2”, TipoFactura solo puede ser “F1”, “F3”, “R1”, “R2”, “R3” y “R4”.");

            var tipoImpositivo = XmlParser.ToDecimal(_DetalleDesglose.TipoImpositivo);
            var cuotaRepercutida = XmlParser.ToDecimal(_DetalleDesglose.CuotaRepercutida);
            var tipoImpositivoRE = XmlParser.ToDecimal(_DetalleDesglose.TipoRecargoEquivalencia);
            var cuotaRepercutidaRE = XmlParser.ToDecimal(_DetalleDesglose.CuotaRecargoEquivalencia);

            // Cuando CalificacionOperacion sea “S2” (https://github.com/mdiago/VeriFactu/issues/76):

            if (_DetalleDesglose.CalificacionOperacion == CalificacionOperacion.S2) 
            {

                // TipoImpositivo = 0 obligatoriamente. (No se admite que vaya vacío o que el campo no exista).
                if (string.IsNullOrEmpty(_DetalleDesglose.TipoImpositivo) || tipoImpositivo != 0)
                    result.Add($"[3.1.3-15.4.1] Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                            $"Cuando CalificacionOperacion sea “S2” TipoImpositivo = 0. (No se admite que vaya vacío o que el campo no exista).");

                // CuotaRepercutida = 0 obligatoriamente. (No se admite que vaya vacío o que el campo no exista).
                if (string.IsNullOrEmpty(_DetalleDesglose.CuotaRepercutida) || cuotaRepercutida != 0)
                    result.Add($"[3.1.3-15.4.2] Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                         $"Cuando CalificacionOperacion sea “S2” CuotaRepercutida = 0. (No se admite que vaya vacío o que el campo no exista).");

            }

            // Si CalificacionOperacion es = “N1/N2” e Impuesto = ”01” (IVA) o no se cumplimenta (considerándose “01” - IVA),
            // no se puede informar ninguno de estos campos:
            // TipoImpositivo, CuotaRepercutida.
            // TipoRecargoEquivalencia, CuotaRecargoEquivalencia.

            if ((_DetalleDesglose.CalificacionOperacion == CalificacionOperacion.N1 ||
                _DetalleDesglose.CalificacionOperacion == CalificacionOperacion.N2)&& 
                _DetalleDesglose.Impuesto == Impuesto.IVA) 
            {               

                if (tipoImpositivo != 0)
                    result.Add($"[3.1.3-15.4.3] Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                            $"Si CalificacionOperacion es = “N1/N2” e Impuesto = ”01” (IVA) o no se cumplimenta (considerándose “01” - IVA)" +
                            $" no se puede informar TipoImpositivo.");

                if (cuotaRepercutida != 0)
                    result.Add($"[3.1.3-15.4.4] Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                            $"Si CalificacionOperacion es = “N1/N2” e Impuesto = ”01” (IVA) o no se cumplimenta (considerándose “01” - IVA)" +
                            $" no se puede informar CuotaRepercutida.");                

                if (tipoImpositivoRE != 0)
                    result.Add($"[3.1.3-15.4.5] Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                         $"Si CalificacionOperacion es = “N1/N2” e Impuesto = ”01” (IVA) o no se cumplimenta (considerándose “01” - IVA)" +
                         $" no se puede informar TipoRecargoEquivalencia”.");

                if (cuotaRepercutidaRE != 0)
                    result.Add($"[3.1.3-15.4.6] Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                         $"Si CalificacionOperacion es = “N1/N2” e Impuesto = ”01” (IVA) o no se cumplimenta (considerándose “01” - IVA)" +
                         $" no se puede informar CuotaRecargoEquivalencia.");

            }

            return result;

        }

        #endregion

    }

}