/*
    This file is part of the VeriFactu (R) project.
    Copyright (c) 2024-2026 Irene Solutions SL
    Author: Irene Solutions SL.

    NO VERI*FACTU implementation developed with the valuable contribution of:
    Javier Florit González
    GAMADI CONSULTING BALEARS SL (B57336786)
    javier.florit@gamadic.com

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
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using VeriFactu.Business.Operations;
using VeriFactu.Config;
using VeriFactu.DataStore;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business
{

    /// <summary>
    /// Representa un envío por requerimiento de facturas al sistema VERI*FACTU de la AEAT.
    /// </summary>
    public class InvoicePeriodReq
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Registros de facturación correspondientes al periodo requerido.
        /// </summary>
        InvoicePeriodExport _InvoicePeriod;

        #endregion

        #region Constructores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sellerID">Identificador del vendedor.</param>        
        /// <param name="period">Periodo requerido por la AEAT.</param>
        public InvoicePeriodReq(string sellerID, string period)
        {

            if (string.IsNullOrEmpty(Settings.Current.IsNotVerifactu))
                throw new InvalidOperationException("La remisión por requerimiento sólo está disponible" +
                    " para sistemas configurados como NO VERI*FACTU.");

            _InvoicePeriod = new InvoicePeriodExport(sellerID, period);

            SellerID = sellerID;
            SellerName = GetSellerName();
            Period = period;

            if (string.IsNullOrEmpty(SellerName))
                throw new InvalidOperationException($"No se ha podido obtener el nombre o razón social" +
                    $" del obligado a la emisión '{sellerID}'.");

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Genera el sobre correspondiente a un lote de registros
        /// de facturación.
        /// </summary>
        /// <param name="records">Registros de facturación a remitir.</param>
        /// <param name="refRequirement">Referencia del requerimiento de la AEAT.</param>
        /// <param name="isLast">Indica si se trata del último envío del requerimiento.</param>
        /// <returns>Sobre a remitir a la AEAT.</returns>
        private Envelope GetEnvelope(List<Registro> records, string refRequirement, bool isLast)
        {

            var registro = new RegFactuSistemaFacturacion()
            {
                Cabecera = new Cabecera()
                {
                    ObligadoEmision = new Interlocutor()
                    {
                        NombreRazon = SellerName,
                        NIF = SellerID
                    },
                    RemisionRequerimiento = new RemisionRequerimiento()
                    {
                        RefRequerimiento = refRequirement,
                        FinRequerimiento = isLast ? "S" : "N"
                    }
                },
                RegistroFactura = new List<RegistroFactura>()
            };

            foreach (var record in records)
                registro.RegistroFactura.Add(
                    new RegistroFactura()
                    {
                        Registro = record
                    });

            return new Envelope()
            {
                Body = new Body()
                {
                    Registro = registro
                }
            };

        }
        /// <summary>
        /// Obtiene el nombre o razón social del obligado a la emisión
        /// de los registros de facturación del periodo.
        /// </summary>
        /// <returns>Nombre o razón social del obligado a la emisión.</returns>
        private string GetSellerName()
        {

            foreach (var file in _InvoicePeriod.InvoicePeriodExportFiles)
            {

                var alta = file.Record as RegistroAlta;

                if (alta != null)
                    return alta.NombreRazonEmisor;
    

            }

            return null;

        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Remite a la AEAT los registros de facturación correspondientes
        /// al periodo requerido.
        /// </summary>
        /// <param name="refRequirement">Referencia del requerimiento de la AEAT.</param>
        /// <param name="certificate">Certificado para la petición.</param>
        /// <returns>Respuestas devueltas por la AEAT para cada uno de los envíos realizados.</returns>
        public List<string> Send(string refRequirement, X509Certificate2 certificate = null)
        {

            if (string.IsNullOrWhiteSpace(refRequirement))
                throw new ArgumentException(
                    "La referencia del requerimiento no puede estar vacía.",
                    nameof(refRequirement));

            var records = new List<Registro>();

            foreach (var file in _InvoicePeriod.InvoicePeriodExportFiles)
                records.Add(file.Record);

            var responses = new List<string>();

            const int batchSize = 1000;

            for (int i = 0; i < records.Count; i += batchSize)
            {

                var count = Math.Min(
                    batchSize,
                    records.Count - i);

                var batch = records.GetRange(
                    i,
                    count);

                var envelope = GetEnvelope(
                    batch,
                    refRequirement,
                    i + count == records.Count);

                var xml = new XmlParser().GetBytes(
                    envelope,
                    Namespaces.Items);

                var response = InvoiceActionMessage.SendXmlBytes(
                    xml,
                    certificate: certificate,
                    isRequirement: true);

                responses.Add(response);

            }

            return responses;

        }

        /// <summary>
        /// Obtiene el xml correspondiente a la remisión por requerimiento
        /// sin realizar el envío a la AEAT.
        /// </summary>
        /// <param name="refRequirement">Referencia del requerimiento de la AEAT.</param>
        /// <returns>Xml correspondiente a la remisión por requerimiento.</returns>
        public byte[] GetXml(string refRequirement)
        {

            if (string.IsNullOrWhiteSpace(refRequirement))
                throw new ArgumentException("La referencia del requerimiento no puede estar vacía.",
                    nameof(refRequirement));

            if (refRequirement.Length > 18)
                throw new ArgumentException("La referencia del requerimiento no puede superar los 18 caracteres.",
                    nameof(refRequirement));

            if (!refRequirement.All(char.IsLetterOrDigit))
                throw new ArgumentException("La referencia del requerimiento debe ser alfanumérica.",
                    nameof(refRequirement));

            var records = new List<Registro>();

            foreach (var file in _InvoicePeriod.InvoicePeriodExportFiles)
                records.Add(file.Record);

            var envelope = GetEnvelope(records, refRequirement, true);

            return new XmlParser().GetBytes(envelope, Namespaces.Items);

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Identificador del vendedor.
        /// </summary>
        public string SellerID { get; private set; }

        /// <summary>
        /// Periodo requerido por la AEAT.
        /// </summary>
        public string Period { get; private set; }

        /// <summary>
        /// Nombre o razón social del obligado a la emisión.
        /// </summary>
        public string SellerName { get; private set; }

        #endregion

    }

}