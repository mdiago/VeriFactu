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
    serving VeriFactu XML data on the fly in a web application, shipping VeriFactu
    with a closed source product.
    
    For more information, please contact Irene Solutions SL. at this
    address: info@irenesolutions.com
 */

using System;
using System.Collections.Generic;
using System.IO;
using VeriFactu.Config;
using VeriFactu.Xml.Factu.Fault;
using VeriFactu.Xml.Factu.Respuesta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.DataStore
{

    /// <summary>
    /// Representa un periodo de facturación de un vendedor o emisor
    /// de facturas en lo que se refiere a documentos de entrada o
    /// respuesta por parte de la AEAT a los envíos.
    /// </summary>
    public class PeriodInbox : PeriodBox
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="seller">Vendedor o emisor de facturas al
        /// que corresponde el periodo.</param>
        /// <param name="periodID">Identificador del periodo.</param>
        /// <param name="invoiceCount">Número de facturas del periodo.</param>
        public PeriodInbox(Seller seller, string periodID, int invoiceCount) : base(seller, periodID, invoiceCount)
        {
        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Devuelve una lista de objetos relacionados cada uno con
        /// una factura.
        /// </summary>
        /// <param name="envelope">Sobre SOAP.</param>
        /// <returns>lista de objetos relacionados cada uno con
        /// una factura.</returns>
        internal override IList<object> GetInvoiceRecords(Envelope envelope)
        {

            var result = new List<object>();

            var respuestaRegFactuSistemaFacturacion = envelope.Body.Registro as RespuestaRegFactuSistemaFacturacion;
            var fault = envelope.Body.Registro as Fault;

            if (respuestaRegFactuSistemaFacturacion == null && fault == null)
                throw new InvalidOperationException($"Error: Encontrado un envío de mensaje SOAP con propiedad Registro desconocida {envelope.Body.Registro}");

            if (fault != null) // Se trata de un error no controlado por la AEAT (no se recibión el envío)
                return result;

            var registros = respuestaRegFactuSistemaFacturacion?.RespuestaLinea as IList<RespuestaLinea>;

            if (registros == null)
                throw new InvalidOperationException($"Error: Encontrado un RespuestaLinea que no es una lista {respuestaRegFactuSistemaFacturacion?.RespuestaLinea}");

            if (registros.Count == 0)
                throw new InvalidOperationException($"Error: Encontrado un RespuestaLinea sin elementos {registros}");

            foreach (var registro in registros)
                result.Add(registro);

            return result;

        }


        /// <summary>
        /// Actualiza el campo de IDFactura en el registro que
        /// se pasa como parámetro, y devuelve un identificador
        /// basado en el mismo.
        /// </summary>
        /// <param name="record">Registro a actualizar.</param>
        /// <returns>Identificador basado en el IDFactura.</returns>
        internal override string SetRegistro(object record)
        {

            var registro = record as RespuestaLinea;

            if (registro == null)
                throw new InvalidOperationException($"Error: Encontrado un envío de mensaje SOAP con propiedad RespuestaLinea desconocida {record}"); 

            return $"{registro.IDFactura}";


        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Ruta a la bandeja. Por defecto la bandeja de salida
        /// de registros.
        /// </summary>
        public override string EnvelopeDir => $"{Settings.Current.InboxPath}{SellerID}{Path.DirectorySeparatorChar}{PeriodID}{Path.DirectorySeparatorChar}"; 

        #endregion

    }

}