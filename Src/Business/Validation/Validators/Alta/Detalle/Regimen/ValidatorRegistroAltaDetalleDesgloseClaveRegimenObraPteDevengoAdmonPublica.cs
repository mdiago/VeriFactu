﻿/*
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
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators.Alta.Detalle.Regimen
{

    /// <summary>
    /// Valida los datos de RegistroAlta DetalleDesglose ClaveRegimen 14. IVA pendiente AAPP.
    /// </summary>
    public class ValidatorRegistroAltaDetalleDesgloseClaveRegimenObraPteDevengoAdmonPublica : ValidatorRegistroAlta
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
        public ValidatorRegistroAltaDetalleDesgloseClaveRegimenObraPteDevengoAdmonPublica(Envelope envelope, RegistroAlta registroAlta,
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

            // Si Impuesto = “01” (IVA), “03” (IGIC) o no se cumplimenta (considerándose “01” - IVA):
            // Si ClaveRegimen = “14”:
            // FechaOperacion, campo de cumplimentación obligatoria y posterior a fecha de expedición.
            // Todos los destinatarios tienen que estar identificados mediante NIF y comenzar por “P”,”Q”,”S” o “V”.
            // TipoFactura: se validará que tipo de factura sea “F1”, “R1”, “R2”, “R3” o “R4”.
            if (_DetalleDesglose.Impuesto != Impuesto.IVA &&
                _DetalleDesglose.Impuesto != Impuesto.IGIC)
            {

                if (_FechaOperacion == null || (_FechaOperacion??_FechaExpedicion).CompareTo(_FechaExpedicion) < 0)
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                        $" Cuando ClaveRegimen sea igual a “14”" +
                        $" FechaOperacion, campo de cumplimentación obligatoria y posterior a fecha de expedición.");

                if(_IsSimplificada)
                    result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                        $" Cuando ClaveRegimen sea igual a “14”" +
                        $" TipoFactura: se validará que tipo de factura sea “F1”, “R1”, “R2”, “R3” o “R4”.");

                // Todos los destinatarios tienen que estar identificados mediante NIF y comenzar por “P”,”Q”,”S” o “V”.
                foreach (var destinatario in _RegistroAlta.Destinatarios)
                {

                    if (string.IsNullOrEmpty(destinatario.NIF))
                        result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                            $" Cuando ClaveRegimen sea igual a “14”" +
                            $" El destinatario {destinatario} tiene que estar identificado mediante NIF.");
                    else if (Array.IndexOf("PQSV".ToCharArray(), destinatario.NIF[0]) == -1)
                        result.Add($"Error en el bloque RegistroAlta ({_RegistroAlta}) en el detalle {_DetalleDesglose}:" +
                            $" Cuando ClaveRegimen sea igual a “14”" +
                            $" El destinatario {destinatario} tiene que estar identificado" +
                            $" mediante NIF y este debe comenzar por “P”,”Q”,”S” o “V”.");                    

                }



            }


            return result;

        }

        #endregion

    }

}