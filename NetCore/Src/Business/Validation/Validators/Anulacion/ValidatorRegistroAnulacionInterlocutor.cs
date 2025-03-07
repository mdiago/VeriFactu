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
using VeriFactu.Business.Validation.NIF;
using VeriFactu.Business.Validation.VIES;
using VeriFactu.Config;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Anulacion;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators.Anulacion
{

    /// <summary>
    /// Valida los datos de RegistroAlta respecto a un intelocutor
    /// con un rol concreto (Tercero, Destinatario...).
    /// </summary>
    public class ValidatorRegistroAnulacionInterlocutor : ValidatorRegistroAnulacion
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
        /// <param name="registroAnulaciona"> Registro alta factura.</param>
        /// <param name="interlocutor"> Interlocutor a validar. </param>
        /// <param name="rol"> Rol del interlocutor a validar (Destinatario, Tercero...)</param>
        /// <param name="allowNoCensado"> True indica que se admite el valor IDOtro.IDType = “07” (NO CENSADO).</param>
        public ValidatorRegistroAnulacionInterlocutor(Envelope envelope, RegistroAnulacion registroAnulaciona, 
            Interlocutor interlocutor, string rol, bool allowNoCensado = false) : base(envelope, registroAnulaciona)
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

            // Si se identifica mediante NIF, el NIF debe estar identificado y ser distinto del campo NIF de la
            // agrupación ObligadoEmisión del bloque Cabecera.
            if (!string.IsNullOrEmpty(_Interlocutor.NIF))
            {

                if (!Settings.Current.SkipNifAeatValidation)
                    result.AddRange(new NifValidation(_Interlocutor.NIF, _Interlocutor.NombreRazon).GetErrors());

                if(_Interlocutor.NIF == _Cabecera.ObligadoEmision.NIF)
                    result.Add($"Error en el bloque RegistroAnulacion: El NIF del {_Rol} {_Interlocutor.NIF}" +
                        $" con el nombre {_Interlocutor.NombreRazon} debe ser distinto del NIF del campo" +
                        $" Cabecera.ObligadoEmision.NIF de la agrupación IDFactura.");

            }

            // Si se cumplimenta NIF, no deberá existir la agrupación IDOtro y viceversa, pero es obligatorio que se cumplimente uno de los dos.
            if (_Interlocutor != null && !string.IsNullOrEmpty(_Interlocutor.NIF) && _Interlocutor.IDOtro != null)
                result.Add($"Error en el bloque RegistroAnulacion ({_RegistroAnulacion}):" +
                    $" {_Rol} si se cumplimenta NIF, no deberá existir la agrupación IDOtro y viceversa");

            if (_Interlocutor != null && string.IsNullOrEmpty(_Interlocutor.NIF) && _Interlocutor.IDOtro == null)
                result.Add($"Error en el bloque RegistroAnulacion ({_RegistroAnulacion}):" +
                    $" {_Rol} es obligatorio que se cumplimente NIF o IDOtro.");

            if (_Interlocutor != null && _Interlocutor.IDOtro != null)
            {

                // Si el campo IDType = “02” (NIF-IVA), no será exigible el campo CodigoPais.
                if (_Interlocutor.IDOtro.IDType != IDType.NIF_IVA)
                    result.Add($"Error en el bloque RegistroAnulacion ({_RegistroAnulacion}):" +
                        $" {_Rol} es obligatorio que se cumplimente CodigoPais con IDOtro.IDType != “02”.");

                var isValidViesVatNumber = Settings.Current.SkipViesVatNumberValidation ? true : ViesVatNumber.Validate(_Interlocutor.IDOtro.ID);

                // Cuando el tercero se identifique a través de la agrupación IDOtro e IDType sea “02”,
                // se validará que el campo identificador ID se ajuste a la estructura de NIF-IVA de
                // alguno de los Estados Miembros y debe estar identificado. Ver nota (1).
                if (_Interlocutor.IDOtro.IDType == IDType.NIF_IVA && !isValidViesVatNumber)
                    result.Add($"Error en el bloque RegistroAnulacion ({_RegistroAnulacion}):" +
                        $" {_Rol} es obligatorio que IDOtro.ID = “{_Interlocutor.IDOtro.ID}” esté identificado.");


                if (_AllowNoCensado) 
                {

                    // Si se identifica a través de la agrupación IDOtro y CodigoPais sea "ES", se validará que el campo IDType sea “03” o “07”..
                    if (_Interlocutor.IDOtro.CodigoPais == CodigoPais.ES && 
                        (_Interlocutor.IDOtro.IDType != IDType.PASAPORTE || _Interlocutor.IDOtro.IDType != IDType.NO_CENSADO))
                        result.Add($"Error en el bloque RegistroAnulacion ({_RegistroAnulacion}):" +
                            $" {_Rol} es obligatorio que para IDOtro.CodigoPais = “{_Interlocutor.IDOtro.CodigoPais}”" +
                            $" IDOtro.IDType = “03” (PASAPORTE) o IDOtro.IDType = “07” (NO_CENSADO).");

                }
                else 
                {

                    // Si se identifica a través de la agrupación IDOtro y CodigoPais sea "ES", se validará que el campo IDType sea “03”.
                    if (_Interlocutor.IDOtro.CodigoPais == CodigoPais.ES && _Interlocutor.IDOtro.IDType != IDType.PASAPORTE)
                        result.Add($"Error en el bloque RegistroAnulacion ({_RegistroAnulacion}):" +
                            $" {_Rol} es obligatorio que para IDOtro.CodigoPais = “{_Interlocutor.IDOtro.CodigoPais}” IDOtro.IDType = “03” (PASAPORTE).");


                    if (_Interlocutor.IDOtro.IDType == IDType.NO_CENSADO)
                        result.Add($"Error en el bloque RegistroAnulacion ({_RegistroAnulacion}):" +
                            $" {_Rol} no se admite IDOtro.IDType = “07” (NO CENSADO).");

                }



            }

            return result;

        }

        #endregion

    }

}