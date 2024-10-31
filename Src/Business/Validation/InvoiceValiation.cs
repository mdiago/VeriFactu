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
using System.Text.RegularExpressions;
using VeriFactu.Business.Validation.NIF;
using VeriFactu.Business.Validation.Validators;
using VeriFactu.Business.Validation.VIES;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
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
        /// Validaciones del bloque de todos los items de RegistroAlta.
        /// </summary>
        /// <returns>Lista con los errores encontrados.</returns>
        public List<string> GetErrorsRegistroAlta(RegistroAlta registroAlta)
        {

            var cabecera = _RegFactuSistemaFacturacion?.Cabecera;

            DateTime? operacion = null;

            var result = new List<string>();

            // 1. Agrupación IDFactura

            var nifEmisorFactura = $"{registroAlta?.IDFacturaAlta?.IDEmisorFactura}";

            if (string.IsNullOrEmpty(nifEmisorFactura))
            {

                result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                    $" La propiedad IDFactura.IDEmisorFactura tiene que tener un valor.");

            }
            else 
            {

                // El NIF del campo IDEmisorFactura debe ser el mismo que el del campo NIF
                // de la agrupación ObligadoEmision del bloque Cabecera.

                if (cabecera?.ObligadoEmision?.NIF != registroAlta?.IDFacturaAlta?.IDEmisorFactura)
                    result.Add($"Error en el bloque RegistroAlta ({registroAlta}): El NIF del campo IDEmisorFactura debe ser el mismo que el del campo NIF de la agrupación ObligadoEmision del bloque Cabecera.");

                // La FechaExpedicionFactura no podrá ser superior a la fecha actual.

                var fechaExpedicion = registroAlta?.IDFacturaAlta?.FechaExpedicion;

                if (string.IsNullOrEmpty(fechaExpedicion))
                {

                    result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                    $" La propiedad IDFactura.FechaExpedicion tiene que tener un valor.");

                } 
                else 
                {

                    if (Regex.IsMatch(fechaExpedicion, @"\d{2}-\d{2}-\d{4}")) 
                    {

                        var year = Convert.ToInt32(fechaExpedicion.Substring(6,4));
                        var month = Convert.ToInt32(fechaExpedicion.Substring(3, 2));
                        var day = Convert.ToInt32(fechaExpedicion.Substring(0, 2));

                        var expedicion = new DateTime(year, month, day);
                        var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                        // La FechaExpedicionFactura no podrá ser superior a la fecha actual.

                        if (expedicion.CompareTo(now) > 0)
                            result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                                $" La propiedad IDFactura.FechaExpedicion {expedicion:yyyy-MM-dd}" +
                                $" no puede ser mayor que la fecha actual {now:yyyy-MM-dd}.");

                        // La FechaExpedicionFactura no debe ser inferior a 01/07/2024
                        if (expedicion.CompareTo(new DateTime(2024, 7, 1)) < 0)
                            result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                                $" La propiedad IDFactura.FechaExpedicion {expedicion:yyyy-MM-dd}" +
                                $" no puede ser inferior del 2024-07-01.");


                        var fechaOperacion = registroAlta?.FechaOperacion;

                        if (!string.IsNullOrEmpty(fechaOperacion))
                        {

                            // Si Impuesto = “01” (IVA), “03” (IGIC) o no se cumplimenta (considerándose “01” - IVA),
                            // la FechaExpedicionFactura solo puede ser anterior a la FechaOperacion, si ClaveRegimen = "14" o "15”.

                            year = Convert.ToInt32(fechaOperacion.Substring(6, 4));
                            month = Convert.ToInt32(fechaOperacion.Substring(3, 2));
                            day = Convert.ToInt32(fechaOperacion.Substring(0, 2));

                            operacion = new DateTime(year, month, day);

                            foreach (var desglose in registroAlta.Desglose)
                            {

                                if ((desglose.ClaveRegimen == ClaveRegimen.ObraPteDevengoAdmonPublica || 
                                    desglose.ClaveRegimen == ClaveRegimen.TractoSucesivoPteDevengo) &&
                                    (desglose.Impuesto == Impuesto.IVA || desglose.Impuesto == Impuesto.IGIC))
                                {

                                    if (expedicion.CompareTo(operacion) > 0)
                                        result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                                            $" Para ClaveRegimen '14' 0 '15' en IVA/IGIC la propiedad IDFactura.FechaExpedicion {expedicion:yyyy-MM-dd}" +
                                            $" no puede ser mayor que la fecha FechaOperacion {operacion:yyyy-MM-dd}.");

                                }

                            }

                        }

                    }
                    else 
                    {

                        result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                        $" La propiedad IDFactura.FechaExpedicion tiene que tener un formato dd-mm-yyyy.");

                    }
                    
                }

                // NumSerieFactura solo puede contener caracteres ASCII del 32 a 126 (caracteres imprimibles)
                var numSerie = registroAlta?.IDFacturaAlta?.NumSerie;

                if (string.IsNullOrEmpty(numSerie)) 
                {

                    result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                    $" La propiedad IDFactura.IDEmisorFactura tiene que tener un valor.");

                } 
                else 
                {

                    var okNumSerie = Regex.Match(numSerie, @"[\x20-\x7E]+").Value;

                    if(numSerie != okNumSerie)
                        result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                            $" La propiedad IDFactura.IDEmisorFactura tiene que tener un valor.");

                }

            }

            // 2. RechazoPrevio

            // Solo podrá incluirse el campo RechazoPrevio con valor “X” si se ha
            // informado el campo Subsanacion y tiene el valor “S”.
            if(registroAlta.RechazoPrevio == RechazoPrevio.X && registroAlta.Subsanacion != "S")
                result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                    $" Solo podrá incluirse el campo RechazoPrevio con valor “X” si se" +
                    $" ha informado el campo Subsanacion y tiene el valor “S”.");

            // No podrá informarse el campo RechazoPrevio con valor “S” si no se
            // informa el campo Subsanación o éste tiene el valor “N”
            if (registroAlta.RechazoPrevio == RechazoPrevio.S && 
                (string.IsNullOrEmpty(registroAlta.Subsanacion) || registroAlta.Subsanacion == "N"))
                result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                    $" No podrá informarse el campo RechazoPrevio con valor “S” si no se" +
                    $" informa el campo Subsanación o éste tiene el valor “N”.");

            // 3. TipoRectificativa

            // Solo podrá incluirse este campo si el valor del campo TipoFactura es igual a “R1”, “R2”, “R3”,
            // “R4” o “R5”
            
            var isRectificativa = Array.IndexOf(new TipoFactura[]{ TipoFactura.R1, TipoFactura.R2, 
                TipoFactura.R3, TipoFactura.R4, TipoFactura.R5 }, registroAlta.TipoFactura) != -1;

            if(!isRectificativa && registroAlta.TipoRectificativaSpecified)
                result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                   $" Solo podrá incluirse este campo si el valor del campo TipoFactura es igual a “R1”, “R2”, “R3”," +
                   $" “R4” o “R5”.");

            // Campo obligatorio si TipoFactura es igual a “R1”, “R2”, “R3”, “R4” o “R5”.
            if (isRectificativa && !registroAlta.TipoRectificativaSpecified)
                result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                   $" Campo obligatorio si TipoFactura es igual a “R1”, “R2”, “R3”, “R4” o “R5”.");

            // 4. Agrupación FacturasRectificadas
            
            var facturasRecticadas = registroAlta?.FacturasRectificadas;

            if (facturasRecticadas != null) 
            {

                if(facturasRecticadas.Count > 1000)
                    result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                       $" La colección FacturasRectificadas no puede" +
                       $" contener más de 1000 elementos y contiene {facturasRecticadas.Count}”.");


                if (!isRectificativa)
                {
                    // Sólo podrá incluirse esta agrupación (no es obligatoria) si TipoFactura es igual a “R1”, “R2”, “R3”, “R4” o “R5”.
                    result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                        $" La colección FacturasRectificadas sólo puede existir" +
                        $" si TipoFactura es igual a “R1”, “R2”, “R3”, “R4” o “R5”.");

                } 
                else 
                {

                    // El NIF del campo IDEmisorFactura debe estar identificado.
                    foreach (var facturaRectificada in facturasRecticadas) 
                    { 
                    
                        if(facturaRectificada?.IDEmisorFactura != registroAlta?.IDFacturaAlta?.IDEmisorFactura)
                            result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                               $" El NIF del campo IDEmisorFactura de FacturasRectificada ({facturaRectificada?.IDEmisorFactura}) debe estar" +
                               $" identificado y debe se el mismo que IDEmisorFactura ({registroAlta?.IDFacturaAlta?.IDEmisorFactura}).");

                    }

                }


            }

            // 5. Agrupación FacturasSustituidas

            var facturasSustituidas = registroAlta?.FacturasSustituidas;

            if (facturasSustituidas != null)
            {

                if (facturasSustituidas.Count > 1000)
                    result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                       $" La colección FacturasSustituidas no puede" +
                       $" contener más de 1000 elementos y contiene {facturasSustituidas.Count}”.");


                if (registroAlta.TipoFacturaSpecified && registroAlta.TipoFactura == TipoFactura.F3)
                {

                    // El NIF del campo IDEmisorFactura debe estar identificado.
                    foreach (var facturasSustituida in facturasSustituidas)
                    {

                        if (facturasSustituida?.IDEmisorFactura != registroAlta?.IDFacturaAlta?.IDEmisorFactura)
                            result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                               $" El NIF del campo IDEmisorFactura de FacturasSustituida ({facturasSustituida?.IDEmisorFactura}) debe estar" +
                               $" identificado y debe se el mismo que IDEmisorFactura ({registroAlta?.IDFacturaAlta?.IDEmisorFactura}).");

                    }

                }
                else
                {

                    // Sólo podrá incluirse esta agrupación (no es obligatoria) cuando el campo TipoFactura="F3"
                    result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                        $" La colección FacturasSustituidas sólo puede existir" +
                        $" si TipoFactura es igual a “F3”.");

                }

            }

            // 6. Agrupación ImporteRectificacion

            var importeRectificacion = registroAlta?.ImporteRectificacion;

            if (registroAlta.TipoRectificativaSpecified && registroAlta.TipoRectificativa == TipoRectificativa.S)
            {

                // Obligatorio si TipoRectificativa = “S”

                if (importeRectificacion == null)
                    result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                        $" Obligatorio informar el bloque ImporteRectificacion si TipoRectificativa = 'S'.");

            }
            else 
            {

                // Sólo deberá incluirse esta agrupación si el campo TipoRectificativa = "S".

                if (importeRectificacion != null)
                    result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                        $" Sólo deberá incluirse el bloque ImporteRectificacion si el campo TipoRectificativa = 'S'.");

            }

            // 7. FechaOperacion

            if (operacion != null) 
            {

                // La FechaOperacion no debe ser inferior a la fecha actual menos veinte años y no debe ser superior al año siguiente de la fecha actual.
                if (DateTime.Now.AddYears(-20).CompareTo(operacion) > 0)
                    result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                        $" La FechaOperacion ({operacion:yyyy-MM-dd}) no debe ser inferior a la fecha actual menos veinte años.");

                if((operacion??DateTime.Now).Year > DateTime.Now.Year)
                    result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                        $" La FechaOperacion ({operacion:yyyy-MM-dd}) no debe ser superior al año siguiente de la fecha actual.");

            }

            // 8.FacturaSimplificadaArt7273

            // Sólo se podrá rellenar con “S” si TipoFactura=“F1” o “F3” o “R1” o “R2” o “R3” o “R4”.
            var allowedFacturaSimplificadaArt7273 = Array.IndexOf(new TipoFactura[]{ TipoFactura.F1, TipoFactura.F3,
                TipoFactura.R1, TipoFactura.R2, TipoFactura.R3, TipoFactura.R4 }, registroAlta.TipoFactura) != -1;

            if(registroAlta.FacturaSimplificadaArt7273 == "S" && !allowedFacturaSimplificadaArt7273)
                result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                        $" La propiedad FacturaSimplificadaArt7273 sólo se puede rellenar" +
                        $" con “S” si TipoFactura=“F1” o “F3” o “R1” o “R2” o “R3” o “R4”.");

            // 9.FacturaSinIdentifDestinatarioArt61d

            // Sólo se podrá rellenar con “S” si TipoFactura=”F2” o “R5”.

            var allowedFacturaSinIdentifDestinatarioArt61d = Array.IndexOf(
                new TipoFactura[]{ TipoFactura.F2, TipoFactura.R5}, registroAlta.TipoFactura) != -1;

            if (registroAlta.FacturaSinIdentifDestinatarioArt61d == "S" && !allowedFacturaSinIdentifDestinatarioArt61d)
                result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                        $" La propiedad FacturaSinIdentifDestinatarioArt61d sólo se puede rellenar" +
                        $" con “S” si TipoFactura=”F2” o “R5”.");

            // 10. Macrodato

            // Campo obligatorio si ImporteTotal >= |100.000.000,00| (valor absoluto).

            if(XmlParser.ToDecimal(registroAlta.ImporteTotal) >100000000m && (registroAlta.Macrodato == null || registroAlta.Macrodato == "N"))
                result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                        $" El campo Macrodato debe contener el valor 'S' obligatoriamente" +
                        $" si ImporteTotal >= |100.000.000,00| (valor absoluto).");

            // 11. EmitidaPorTerceroODestinatario

            // Si es igual a “T”, el bloque Tercero será de cumplimentación obligatoria.

            if(registroAlta.Tercero == null && registroAlta.EmitidaPorTercerosODestinatarioSpecified && 
                registroAlta.EmitidaPorTercerosODestinatario == EmitidaPorTercerosODestinatario.T)
                result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                    $" Si EmitidaPorTerceroODestinatario es igual a “T”," +
                    $" el bloque Tercero será de cumplimentación obligatoria.");

            // Si es igual a “D”, el bloque Destinatarios será de cumplimentación obligatoria.

            if ((registroAlta.Destinatarios == null || registroAlta.Destinatarios.Count == 0) && 
                registroAlta.EmitidaPorTercerosODestinatarioSpecified && 
                registroAlta.EmitidaPorTercerosODestinatario == EmitidaPorTercerosODestinatario.D)
                result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                    $" Si EmitidaPorTerceroODestinatario es igual a “D”, el bloque" +
                    $" Destinatarios será de cumplimentación obligatoria,");

            // 12. Agrupación Tercero

            // Solo podrá cumplimentarse si EmitidaPorTerceroODestinatario es “T”.

            if (registroAlta.Tercero != null && registroAlta.EmitidaPorTercerosODestinatarioSpecified &&
                registroAlta.EmitidaPorTercerosODestinatario != EmitidaPorTercerosODestinatario.T)
                result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                    $" Tercero sólo podrá cumplimentarse si EmitidaPorTerceroODestinatario es “T”.");

            // Si se identifica mediante NIF, el NIF debe estar identificado y ser distinto del NIF del campo IDEmisorFactura de la agrupación IDFactura.
            var tercero = registroAlta.Tercero;

            // Si se cumplimenta NIF, no deberá existir la agrupación IDOtro y viceversa, pero es obligatorio que se cumplimente uno de los dos.
            if(tercero != null && !string.IsNullOrEmpty(tercero.NIF) && tercero.IDOtro != null)
                result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                    $" Tercero si se cumplimenta NIF, no deberá existir la agrupación IDOtro y viceversa");

            if (tercero != null && string.IsNullOrEmpty(tercero.NIF) && tercero.IDOtro == null)
                result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                    $" Tercero es obligatorio que se cumplimente NIF o IDOtro.");

            if (tercero != null && tercero.IDOtro != null) 
            {

                // Si el campo IDType = “02” (NIF-IVA), no será exigible el campo CodigoPais.
                if(tercero.IDOtro.IDType != IDType.NIF_IVA)
                    result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                        $" Tercero es obligatorio que se cumplimente CodigoPais con IDOtro.IDType != “02”.");

                // Cuando el tercero se identifique a través de la agrupación IDOtro e IDType sea “02”,
                // se validará que el campo identificador ID se ajuste a la estructura de NIF-IVA de
                // alguno de los Estados Miembros y debe estar identificado. Ver nota (1).
                if (tercero.IDOtro.IDType == IDType.NIF_IVA && !ViesVatNumber.Validate(tercero.IDOtro.ID))
                    result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                        $" Tercero es obligatorio que IDOtro.ID = “{tercero.IDOtro.ID}” esté identificado.");

                // Si se identifica a través de la agrupación IDOtro y CodigoPais sea "ES", se validará que el campo IDType sea “03”.
                if(tercero.IDOtro.CodigoPais == CodigoPais.ES && tercero.IDOtro.IDType == IDType.PASAPORTE)
                    result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                        $" Tercero es obligatorio que para IDOtro.CodigoPais = “{tercero.IDOtro.CodigoPais}” IDOtro.IDType = “03” (PASAPORTE).");

                if(tercero.IDOtro.IDType == IDType.NO_CENSADO)
                    result.Add($"Error en el bloque RegistroAlta ({registroAlta}):" +
                        $" Tercero no se admite IDOtro.IDType = “07” (NO CENSADO).");


            }


            // 13. Agrupación Destinatarios

            // Si TipoFactura es “F1”, “F3”, “R1”, “R2”, “R3” o “R4”, la agrupación Destinatarios tiene que estar cumplimentada, con al menos un destinatario.

            // Si TipoFactura es “F2” o “R5”, la agrupación Destinatarios no puede estar cumplimentada.

            // Si se identifica mediante NIF, el NIF debe estar identificado y ser distinto del NIF del campo IDEmisorFactura de la agrupación IDFactura.

            // Si se cumplimenta NIF, no deberá existir la agrupación IDOtro y viceversa, pero es obligatorio que se cumplimente uno de los dos.

            // Cuando uno o varios destinatarios se identifiquen a través del NIF, los NIF deben estar identificados y ser distintos del NIF del campo IDEmisorFactura de la agrupación IDFactura.

            // Si el campo IDType = “02” (NIF-IVA), no será exigible el campo CodigoPais.

            // Si el campo IDType = “07” (No censado), el campo CodigoPais debe ser “ES”.

            // Cuando uno o varios destinatarios se identifiquen a través de la agrupación IDOtro e IDType sea “02”, se validará que el campo identificador se ajuste a la estructura de NIF-IVA de alguno de los Estados Miembros y debe estar identificado. Ver nota (1).

            // Cuando uno o varios destinatarios se identifiquen a través de la agrupación IDOtro y CodigoPais sea "ES", se validará que el campo IDType sea “03” o “07”.

            // Cuando se identifique a través del bloque “IDOtro” y IDType sea “02”, se validará que TipoFactura sea “F1”, “F3”, “R1”, “R2”, “R3” ó “R4”.

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
