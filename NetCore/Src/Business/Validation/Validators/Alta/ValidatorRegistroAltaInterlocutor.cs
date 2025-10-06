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
using VeriFactu.Business.Validation.NIF;
using VeriFactu.Business.Validation.VIES;
using VeriFactu.Config;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators.Alta
{

    /// <summary>
    /// Valida los datos de RegistroAlta respecto a un intelocutor
    /// con un rol concreto (Tercero, Destinatario...).
    /// </summary>
    public class ValidatorRegistroAltaInterlocutor : ValidatorRegistroAlta
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Interlocutor a validar.
        /// </summary>
        readonly Interlocutor _Interlocutor;

        /// <summary>
        /// Rol del interlocutor en el registro
        /// de alta (Tercero, Destinatario...).
        /// </summary>
        readonly string _Rol;

        /// <summary>
        /// True indica que se admite el valor IDOtro.IDType = “07” (NO CENSADO)
        /// </summary>
        readonly bool _AllowNoCensado;

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="envelope"> Sobre SOAP envío.</param>
        /// <param name="registroAlta"> Registro alta factura.</param>
        /// <param name="interlocutor"> Interlocutor a validar. </param>
        /// <param name="rol"> Rol del interlocutor a validar (Destinatario, Tercero...)</param>
        /// <param name="allowNoCensado"> True indica que se admite el valor IDOtro.IDType = “07” (NO CENSADO).</param>
        public ValidatorRegistroAltaInterlocutor(Envelope envelope, RegistroAlta registroAlta, 
            Interlocutor interlocutor, string rol, bool allowNoCensado = false) : base(envelope, registroAlta)
        {

            _Interlocutor = interlocutor;
            _Rol = rol;
            _AllowNoCensado = allowNoCensado;

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

            // Si se identifica mediante NIF, el NIF debe estar identificado.
            if (!string.IsNullOrEmpty(_Interlocutor.NIF) && !Settings.Current.SkipNifAeatValidation)
                result.AddRange(new NifValidation(_Interlocutor.NIF, _Interlocutor.NombreRazon).GetErrors());

            // Si se cumplimenta NIF, no deberá existir la agrupación IDOtro y viceversa, pero es obligatorio que se cumplimente uno de los dos.
            if (_Interlocutor != null && !string.IsNullOrEmpty(_Interlocutor.NIF) && _Interlocutor.IDOtro != null)
                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                    $" {_Rol} si se cumplimenta NIF, no deberá existir la agrupación IDOtro y viceversa");

            if (_Interlocutor != null && string.IsNullOrEmpty(_Interlocutor.NIF) && _Interlocutor.IDOtro == null)
                result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                    $" {_Rol} es obligatorio que se cumplimente NIF o IDOtro.");

            if (_Interlocutor != null && _Interlocutor.IDOtro != null)
            {

                // Si el campo IDType = “02” (NIF-IVA), no será exigible el campo CodigoPais.
                if (_Interlocutor.IDOtro.IDType != IDType.NIF_IVA && !_Interlocutor.IDOtro.CodigoPaisSpecified)
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" {_Rol} es obligatorio que se cumplimente CodigoPais con IDOtro.IDType != “02”.");

                // Si el campo IDType = “02” (NIF-IVA), en caso de valor en CodigoPais.
                // El campo CodigoPais indicado no coincide con los dos primeros dígitos del identificador.
                if (_Interlocutor.IDOtro.IDType == IDType.NIF_IVA && _Interlocutor.IDOtro.CodigoPaisSpecified && 
                    !_Interlocutor.IDOtro.ID.StartsWith($"{_Interlocutor.IDOtro.CodigoPais}"))
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" {_Rol} El campo CodigoPais indicado en el interlocutor {_Interlocutor} es '{_Interlocutor.IDOtro.CodigoPais}'" +
                        $" y no coincide con los dos primeros dígitos del identificador '{_Interlocutor.IDOtro.ID.Substring(0,2)}'.");

                // Validación contra censo VIES
                var isValidViesVatNumber = Settings.Current.SkipViesVatNumberValidation ? true : ViesVatNumber.Validate(_Interlocutor.IDOtro.ID);

                // Cuando el tercero se identifique a través de la agrupación IDOtro e IDType sea “02”,
                // se validará que el campo identificador ID se ajuste a la estructura de NIF-IVA de
                // alguno de los Estados Miembros y debe estar identificado. Ver nota (1).
                if (_Interlocutor.IDOtro.IDType == IDType.NIF_IVA && !isValidViesVatNumber)
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" {_Rol} es obligatorio que IDOtro.ID = “{_Interlocutor.IDOtro.ID}” esté identificado.");

                // Validación estructura nota (1).
                isValidViesVatNumber = ViesVatNumber.ValidateStructure(_Interlocutor.IDOtro.ID);

                if (_Interlocutor.IDOtro.IDType == IDType.NIF_IVA && !isValidViesVatNumber)
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                        $" {_Rol} El valor del campo ID es incorrecto. IDOtro.ID = “{_Interlocutor.IDOtro.ID}”.");

                if (_AllowNoCensado) 
                {

                    // Si se identifica a través de la agrupación IDOtro y CodigoPais sea "ES", se validará que el campo IDType sea “03” o “07”..
                    if (_Interlocutor.IDOtro.CodigoPais == CodigoPais.ES && 
                        (_Interlocutor.IDOtro.IDType != IDType.PASAPORTE && _Interlocutor.IDOtro.IDType != IDType.NO_CENSADO))
                        result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                            $" {_Rol} es obligatorio que para IDOtro.CodigoPais = “{_Interlocutor.IDOtro.CodigoPais}”" +
                            $" IDOtro.IDType = “03” (PASAPORTE) o IDOtro.IDType = “07” (NO_CENSADO).");

                    // 1126 El valor del CodigoPais solo puede ser ES cuando el IDType sea Pasaporte (03) o No Censado (07). Si IDType es No Censado (07) el CodigoPais debe ser ES (España).
                    if ((_Interlocutor.IDOtro.IDType == IDType.PASAPORTE|| _Interlocutor.IDOtro.IDType == IDType.NO_CENSADO) && _Interlocutor.IDOtro.CodigoPais != CodigoPais.ES)
                        result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                           $" {_Rol} es obligatorio que para IDOtro.IDType = “{_Interlocutor.IDOtro.IDType}” el valor del CodigoPais sea ES.");

                }
                else 
                {

                    // Si se identifica a través de la agrupación IDOtro y CodigoPais sea "ES", se validará que el campo IDType sea “03”.
                    if (_Interlocutor.IDOtro.CodigoPais == CodigoPais.ES && _Interlocutor.IDOtro.IDType != IDType.PASAPORTE)
                        result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                            $" {_Rol} es obligatorio que para IDOtro.CodigoPais = “{_Interlocutor.IDOtro.CodigoPais}” IDOtro.IDType = “03” (PASAPORTE).");

                    // 1126 El valor del CodigoPais solo puede ser ES cuando el IDType sea Pasaporte (03) o No Censado (07). Si IDType es No Censado (07) el CodigoPais debe ser ES (España).
                    if (_Interlocutor.IDOtro.IDType == IDType.PASAPORTE && _Interlocutor.IDOtro.CodigoPais != CodigoPais.ES)
                        result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                           $" {_Rol} es obligatorio que para IDOtro.IDType = “{_Interlocutor.IDOtro.IDType}” el valor del CodigoPais sea ES.");

                    if (_Interlocutor.IDOtro.IDType == IDType.NO_CENSADO)
                        result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}):" +
                            $" {_Rol} no se admite IDOtro.IDType = “07” (NO CENSADO).");

                }

            }

            return result;

        }

        #endregion

    }

}