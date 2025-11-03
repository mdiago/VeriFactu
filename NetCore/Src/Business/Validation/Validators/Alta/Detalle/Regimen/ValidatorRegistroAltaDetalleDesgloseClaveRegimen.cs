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

namespace VeriFactu.Business.Validation.Validators.Alta.Detalle.Regimen
{

    /// <summary>
    /// Valida los datos de RegistroAlta DetalleDesglose ClaveRegimen.
    /// </summary>
    public class ValidatorRegistroAltaDetalleDesgloseClaveRegimen : ValidatorRegistroAlta
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
        public ValidatorRegistroAltaDetalleDesgloseClaveRegimen(Envelope envelope, RegistroAlta registroAlta,
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

            // Solo podrá incluirse este campo si Impuesto = “01” (IVA), “03” (IGIC) o no se cumplimenta
            // (considerándose “01” - IVA) y será obligatorio.

            if (_DetalleDesglose.ClaveRegimenSpecified &&
                _DetalleDesglose.Impuesto != Impuesto.IVA &&
                _DetalleDesglose.Impuesto != Impuesto.IGIC) 
            {

                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                    $" Solo podrá incluirse ClaveRegimen si Impuesto = “01” (IVA), “03” (IGIC)" +
                    $" o no se cumplimenta (considerándose “01” - IVA).");

            }

            if (!_DetalleDesglose.ClaveRegimenSpecified &&
                (_DetalleDesglose.Impuesto == Impuesto.IVA ||
                _DetalleDesglose.Impuesto == Impuesto.IGIC))
            {

                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                    $" ClaveRegimen obligatorio si Impuesto = “01” (IVA), “03” (IGIC) o no se cumplimenta" +
                    $" (considerándose “01” - IVA).");

            }

            var _ValidatorByClaveRegimen = new Dictionary<ClaveRegimen, IValidator>() 
            {
                // 1199 =  Si Impuesto es '01' (IVA), '03' (IGIC) o no se cumplimenta y ClaveRegimen es 01 no pueden marcarse la OperacionExenta E2, E3.
                {ClaveRegimen.RegimenGeneral,               new ValidatorRegistroAltaDetalleDesgloseClaveRegimenRegimenGeneral(_Envelope, _RegistroAlta, _DetalleDesglose) },
                // 15.6.1 ClaveRegimen 03. REBU.
                {ClaveRegimen.Rebu,                         new ValidatorRegistroAltaDetalleDesgloseClaveRegimenRebu(_Envelope, _RegistroAlta, _DetalleDesglose) },
                // 15.6.2 ClaveRegimen 04. Operaciones con oro de inversión.
                {ClaveRegimen.OroInversion,                 new ValidatorRegistroAltaDetalleDesgloseClaveRegimenOroInversion(_Envelope, _RegistroAlta, _DetalleDesglose) },
                // 15.6.3 ClaveRegimen 06. Grupo de entidades nivel avanzado.
                {ClaveRegimen.GrupoEntidades,               new ValidatorRegistroAltaDetalleDesgloseClaveRegimenGrupoEntidades(_Envelope, _RegistroAlta, _DetalleDesglose) },
                // 15.6.4 ClaveRegimen 07. Criterio de caja.
                {ClaveRegimen.Recc,                         new ValidatorRegistroAltaDetalleDesgloseClaveRegimenRecc(_Envelope, _RegistroAlta, _DetalleDesglose) },
                // 15.6.5 ClaveRegimen 08.
                {ClaveRegimen.IpsiIgic,                     new ValidatorRegistroAltaDetalleDesgloseClaveRegimenIpsiIgic(_Envelope, _RegistroAlta, _DetalleDesglose) },
                // 15.6.6 ClaveRegimen 10. Cobro por cuenta de terceros.
                {ClaveRegimen.CobroTerceros,                new ValidatorRegistroAltaDetalleDesgloseClaveRegimenCobroTerceros(_Envelope, _RegistroAlta, _DetalleDesglose) },
                // 15.6.7 ClaveRegimen 11. Arrendamiento de local de negocio
                {ClaveRegimen.ArrendamientoLocalNecocio,    new ValidatorRegistroAltaDetalleDesgloseClaveRegimenArrendamientoLocalNecocio(_Envelope, _RegistroAlta, _DetalleDesglose) },
                // 15.6.8 ClaveRegimen 14. IVA pendiente AAPP.
                {ClaveRegimen.ObraPteDevengoAdmonPublica,   new ValidatorRegistroAltaDetalleDesgloseClaveRegimenObraPteDevengoAdmonPublica(_Envelope, _RegistroAlta, _DetalleDesglose) },
                // Añadido por error 1286: Si el impuesto es IVA(01), IGIC(03) o vacio, si ClaveRegimen es 02 solo se podrá informar OperacionExenta.
                {ClaveRegimen.Exportacion,                  new ValidatorRegistroAltaDetalleDesgloseClaveRegimenExportacion(_Envelope, _RegistroAlta, _DetalleDesglose) },
            };

            if(_DetalleDesglose.ClaveRegimenSpecified)
                if(_ValidatorByClaveRegimen.ContainsKey(_DetalleDesglose.ClaveRegimen))
                    result.AddRange(_ValidatorByClaveRegimen[_DetalleDesglose.ClaveRegimen].GetErrors());

            return result;

        }

        #endregion

    }

}