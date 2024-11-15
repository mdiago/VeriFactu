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
using VeriFactu.Xml;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators.Alta
{

    /// <summary>
    /// Valida los datos de RegistroAlta DetalleDesglose ImporteTotal.
    /// </summary>
    public class ValidatorRegistroAltaImporteTotal : ValidatorRegistroAlta
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="envelope"> Sobre SOAP envío.</param>
        /// <param name="registroAlta"> Registro alta factura.</param>
        public ValidatorRegistroAltaImporteTotal(Envelope envelope, 
            RegistroAlta registroAlta) : base(envelope, registroAlta)
        {
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


            // Se validará que sea igual a Ʃ (CuotaRepercutida + CuotaRecargoEquivalencia) de todas
            // las líneas de detalle de desglose.En caso contrario se devolverá un aviso de error
            // (no generará rechazo), admitiéndose un margen de error de +/ -10,00 euros.

            if (string.IsNullOrEmpty(_RegistroAlta.NumRegistroAcuerdoFacturacion) && 
                _RegistroAlta.FacturaSinIdentifDestinatarioArt61d != "S") 
            {

                var total = 0m;

                foreach (var desglose in _RegistroAlta.Desglose) 
                {

                    var baseImponibleOimporteNoSujeto = XmlParser.ToDecimal(desglose.BaseImponibleOimporteNoSujeto);
                    var cuotaRepercutida = XmlParser.ToDecimal(desglose.CuotaRepercutida);
                    var cuotaRecargoEquivalencia = XmlParser.ToDecimal(desglose.CuotaRecargoEquivalencia);

                    total += cuotaRepercutida + cuotaRecargoEquivalencia + baseImponibleOimporteNoSujeto;

                    // Esta validación no se aplicará cuando ClaveRegimen sea “03”, “05”, “06” o “09”.
                    var clavesExcluidas = new ClaveRegimen[]{ ClaveRegimen.Rebu, ClaveRegimen.AgenciasViajes, ClaveRegimen.GrupoEntidades, ClaveRegimen.MediadoresAgenciasViaje};
                    
                    if (Array.IndexOf(clavesExcluidas, desglose.ClaveRegimen) != -1)
                        return result;


                }

                var importeTotal = XmlParser.ToDecimal(_RegistroAlta.ImporteTotal);

                if (Math.Abs(importeTotal - total) > 10)
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" Ʃ (BaseImponibleOimporteNoSujeto + CuotaRepercutida + CuotaRecargoEquivalencia)" +
                        $" debe ser igual a ImporteTotal +/ -10,00 euros.");

            }

            return result;

        }

        #endregion

    }

}
