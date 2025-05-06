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
using VeriFactu.Business.Validation.VIES;
using VeriFactu.Config;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Factu.Anulacion;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators
{

    /// <summary>
    /// Valida los datos de SistemaInformatico.
    /// </summary>
    public class ValidatorSistemaInformatico : InvoiceValidation
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="envelope">Envelope de envío al
        /// servicio Verifactu de la AEAT.</param>
        public ValidatorSistemaInformatico(Envelope envelope) : base(envelope)
        {
        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Validaciones del bloque de todos los items de RegistroFactura.
        /// </summary>
        /// <returns>Lista con los errores encontrados.</returns>
        private SistemaInformatico GetSistemaInformatico(object registro)
        {

            var result = new List<string>();

            var registroFactura = registro as RegistroFactura;

            if (registroFactura == null)
                result.Add($"Error en el bloque RegistroFactura: Se ha encontrado un" +
                    $" elemento de la clase {registro.GetType()} en la colección RegistroFactura" +
                    $". Esta colección sólo admite elementos del tipo RegistroFactura.");

            var registroAlta = registroFactura.Registro as RegistroAlta;
            var registroAnulacion = registroFactura.Registro as RegistroAnulacion;

            if (registroAlta == null && registroAnulacion == null)
                result.Add($"Error en el bloque RegistroFactura.Registro: Se ha encontrado un" +
                    $" elemento de la clase {registro.GetType()} en la colección RegistroFactura" +
                    $". Esta colección sólo admite elementos del tipo RegistroAlta o RegistroAnulacion.");


            return registroAlta?.SistemaInformatico ?? registroAnulacion?.SistemaInformatico;


        }

        /// <summary>
        /// Devuelve los errores de validación de una instancia
        /// de la clase SistemaInformatico.
        /// </summary>
        /// <param name="sistemaInformatico"> Instancia de la clase
        /// SistemaInformatico a validar.</param>
        /// <returns> Lista con las descripciones de
        /// los errores de validación de una instancia
        /// de la clase SistemaInformatico.</returns>
        private List<string> GetErrors(SistemaInformatico sistemaInformatico) 
        {

            var result = new List<string>();

            var interlocutor = sistemaInformatico;

            // Si se cumplimenta NIF, no deberá existir la agrupación IDOtro y viceversa, pero es obligatorio que se cumplimente uno de los dos.
            if (sistemaInformatico != null && !string.IsNullOrEmpty(sistemaInformatico.NIF) && sistemaInformatico.IDOtro != null)
                result.Add($"Error en el bloque SistemaInformatico ({sistemaInformatico}):" +
                    $" Si se cumplimenta NIF, no deberá existir la agrupación IDOtro y viceversa");

            if (sistemaInformatico != null && string.IsNullOrEmpty(sistemaInformatico.NIF) && sistemaInformatico.IDOtro == null)
                result.Add($"Error en el bloque SistemaInformatico ({sistemaInformatico}):" +
                    $" Es obligatorio que se cumplimente NIF o IDOtro.");

            // Falta informar campo obligatorio.: IdSistemaInformatico
            if (sistemaInformatico != null && string.IsNullOrEmpty(sistemaInformatico.IdSistemaInformatico))
                result.Add($"Error en el bloque SistemaInformatico ({sistemaInformatico}):" +
                    $" Falta informar campo obligatorio IdSistemaInformatico.");

            // IdSistemaInformatico tiene una longitud máxima de 2 caracteres.
            if (sistemaInformatico != null && !string.IsNullOrEmpty(sistemaInformatico.IdSistemaInformatico) && 
                sistemaInformatico.IdSistemaInformatico.Length > 2)
                result.Add($"Error en el bloque SistemaInformatico ({sistemaInformatico}):" +
                    $" El campo obligatorio IdSistemaInformatico tiene una longitud máxima de 2 caracteres.");

            if (sistemaInformatico != null && sistemaInformatico.IDOtro != null)
            {

                // Si el campo IDType = “02” (NIF-IVA), no será exigible el campo CodigoPais.
                if (sistemaInformatico.IDOtro.IDType != IDType.NIF_IVA)
                    result.Add($"Error en el bloque SistemaInformatico ({sistemaInformatico}):" +
                        $"Es obligatorio que se cumplimente CodigoPais con IDOtro.IDType != “02”.");

                var isValidViesVatNumber = Settings.Current.SkipViesVatNumberValidation ? true : ViesVatNumber.Validate(sistemaInformatico.IDOtro.ID);

                // Cuando el tercero se identifique a través de la agrupación IDOtro e IDType sea “02”,
                // se validará que el campo identificador ID se ajuste a la estructura de NIF-IVA de
                // alguno de los Estados Miembros y debe estar identificado. Ver nota (1).
                if (sistemaInformatico.IDOtro.IDType == IDType.NIF_IVA && !isValidViesVatNumber)
                    result.Add($"Error en el bloque SistemaInformatico ({sistemaInformatico}):" +
                        $" Es obligatorio que IDOtro.ID = “{sistemaInformatico.IDOtro.ID}” esté identificado.");
             

                // Si se identifica a través de la agrupación IDOtro y CodigoPais sea "ES", se validará que el campo IDType sea “03” o “07”..
                if (sistemaInformatico.IDOtro.CodigoPais == CodigoPais.ES &&
                    (sistemaInformatico.IDOtro.IDType != IDType.PASAPORTE || sistemaInformatico.IDOtro.IDType != IDType.NO_CENSADO))
                    result.Add($"Error en el bloque SistemaInformatico ({sistemaInformatico}):" +
                        $" Es obligatorio que para IDOtro.CodigoPais = “{sistemaInformatico.IDOtro.CodigoPais}”" +
                        $" IDOtro.IDType = “03” (PASAPORTE) o IDOtro.IDType = “07” (NO_CENSADO).");

            }

            return result;

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

            var result = new List<string>();

            if (_RegFactuSistemaFacturacion.RegistroFactura.Count > 10000)
                result.Add($"La colección RegFactuSistemaFacturacion.RegistroFactura" +
                    $" contiene {_RegFactuSistemaFacturacion.RegistroFactura.Count}" +
                    $" elementos cuando sólo está permitido un máximo de 1000.");

            //1. Agrupaciones RegistroAlta y RegistroAnulacion: Dentro de cada una de las posibles repeticiones
            //u “ocurrencias” de RegistroFactura (de 1 a 1000) se pueden incluir registros de facturación de alta
            //(agrupación RegistroAlta) y de anulación(agrupación RegistroAnulacion) en un mismo mensaje remitido,
            //pero siempre que vayan en distintas ocurrencias de RegistroFactura(no pueden ir ambas agrupaciones a
            //la vez dentro de la misma ocurrencia). ?????????????????????

            foreach (var registroFactura in _RegFactuSistemaFacturacion.RegistroFactura)
                result.AddRange(GetErrors(GetSistemaInformatico(registroFactura)));

            return result;

        }

        #endregion


    }

}