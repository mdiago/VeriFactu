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
using System.IO;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Factu.Anulacion;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business
{

    /// <summary>
    /// Representa una acción de reenvío por fallo técnico
    /// en las comunicaciones u otra causa. La factura es
    /// correcta, ha sido contabilizada en la cadena de
    /// bloques pero no ha sido enviada a la AEAT.
    /// </summary>
    public class InvoiceRetrySend : InvoiceEntry
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="invoice">Instancia de factura de entrada en el sistema.</param>
        public InvoiceRetrySend(Invoice invoice) : base(invoice)
        {

            IsRetrySend = true;

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Establece el registro relativo a la entrada
        /// a contabilizar y enviar.
        /// </summary>
        internal override void SetRegistro()
        {

            // Path del archivo xml del registro de alta correspondiente a la factura
            var path = OriginalInvoiceFilePath;

            if(!File.Exists(OriginalInvoiceFilePath))
                throw new Exception($"No se ha encontrado el archivo del Registro correspondiente a {this}.");

            var envelope = new Envelope(OriginalInvoiceFilePath);

            // Establecemos el valor de RegistroAlta / RegistroAnulacion
            var RegFactuSistemaFacturacion = envelope.Body?.Registro as RegFactuSistemaFacturacion;

            if (RegFactuSistemaFacturacion == null)
                throw new Exception($"No se ha encontrado el RegFactuSistemaFacturacion correspondiente a la entrada {this}.");

            if (RegFactuSistemaFacturacion?.RegistroFactura == null|| RegFactuSistemaFacturacion?.RegistroFactura?.Count == 0)
                throw new Exception($"No se ha encontrado el RegistroFactura ningún RegistroAlta correspondiente a la entrada {this}.");

            Registro registro = RegFactuSistemaFacturacion.RegistroFactura[0]?.Registro as RegistroAlta;

            if(registro == null)
                registro = RegFactuSistemaFacturacion.RegistroFactura[0]?.Registro as RegistroAnulacion;

            if (registro == null)
                throw new Exception($"No se ha encontrado el RegistroAlta / RegistroAnulacion correspondiente a la entrada {this}.");

            var refExt = (registro as RegistroAlta).RefExterna?? (registro as RegistroAnulacion).RefExterna;

            if (string.IsNullOrEmpty(refExt))
                throw new Exception($"No se ha encontrado el RefExterna correspondiente a la entrada {this}.");

            registro.BlockchainLinkID = Convert.ToUInt64(refExt);

            var fechaHoraHusoGenRegistro = (registro as RegistroAlta).OrderedFechaHoraHusoGenRegistro ?? (registro as RegistroAnulacion).OrderedFechaHoraHusoGenRegistro;

            if (string.IsNullOrEmpty(fechaHoraHusoGenRegistro))
                throw new Exception($"No se ha encontrado el OrderedFechaHoraHusoGenRegistro correspondiente a la entrada {this}.");


            registro.FechaHoraHusoGenRegistro = fechaHoraHusoGenRegistro;

            // Establecemos el valor de Registro
            Registro = registro;

        }

        /// <summary>
        /// Genera el sobre SOAP.
        /// </summary>
        /// <returns>Sobre SOAP.</returns>
        internal override Envelope GetEnvelope()
        {

            var envelope = new Envelope(OriginalInvoiceFilePath); 

            // Establecemos Incidencia 
            (envelope.Body.Registro as RegFactuSistemaFacturacion).Cabecera.RemisionVoluntaria = new RemisionVoluntaria() { Incidencia = "S" };

            return envelope;

        }

        /// <summary>
        /// Actualiza los datos tras la incorporación del registro
        /// a la cadena de bloques.
        /// <para> 1. Recalcula Xml con la info Blockchain actualizada.</para>
        /// <para> 2. Guarda el registro en disco en el el directorio de registros emitidos.</para>
        /// <para> 3. Establece Posted = true.</para>
        /// </summary>
        internal override void SaveBlockchainChanges()
        {

            if (Registro.BlockchainLinkID == 0)
                throw new InvalidOperationException($"El registro {Registro}" +
                    $" no está incluido en la cadena de bloques.");

            if (Posted)
                throw new InvalidOperationException($"La operación {this}" +
                    $" ya está contabilizada.");


            // No hay que regener el Xml porqué es el de la factura original

            // Guardamos el xml
            File.WriteAllBytes(InvoiceFilePath, Xml);

            // Marcamos como contabilizado
            Posted = true;

        }


        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Path de la factura original en el directorio de facturas.
        /// </summary>
        public string OriginalInvoiceFilePath => $"{InvoicePostedPath}{EncodedInvoiceID}.xml";

        /// <summary>
        /// Path de la factura en el directorio de facturas.
        /// </summary>
        public override string InvoiceFilePath => $"{InvoicePostedPath}{EncodedInvoiceID}.RTS.{DateTime.Now:yyyy.MM.dd.HH.mm.ss.ffff}.xml";

        /// <summary>
        /// Path de la factura en el directorio de archivado de los datos de la
        /// cadena.
        /// </summary>
        public override string InvoiceEntryFilePath => $"{InvoiceEntryPath}{InvoiceEntryID}.RTS.{DateTime.Now:yyyy.MM.dd.HH.mm.ss.ffff}.xml";

        /// <summary>
        /// Path del directorio de archivado de los datos de la
        /// cadena.
        /// </summary>
        public override string ResponseFilePath => $"{ResponsesPath}{InvoiceEntryID}.RTS.{DateTime.Now:yyyy.MM.dd.HH.mm.ss.ffff}.xml";

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Devuelve una lista con los errores de la
        /// factura por el incumplimiento de reglas de negocio.
        /// </summary>
        /// <returns>Lista con los errores encontrados.</returns>
        public override List<string> GetBusErrors()
        {

            var errors = new List<string>();

            // Comprobamos que la factura existe
            if (!File.Exists(base.InvoiceFilePath))
                errors.Add($"No existe una entrada con SellerID: {Invoice.SellerID}" +
                    $" en el año {Invoice.InvoiceDate.Year} con el número {Invoice.InvoiceID}.");

            // Limite listas
            if (Invoice.RectificationItems?.Count > 1000)
                errors.Add($"Invoice.RectificationItems.Count no puede ser mayor de 1.000.");

            if (Invoice.TaxItems?.Count > 12)
                errors.Add($"Invoice.TaxItems.Count no puede ser mayor de 12.");

            errors.AddRange(GetTaxItemsValidationErrors());
            errors.AddRange(GetInvoiceValidationErrors());

            return errors;

        }

        #endregion

    }

}