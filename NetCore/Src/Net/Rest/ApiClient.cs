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
using System.Dynamic;
using System.Net;
using System.Text;
using VeriFactu.Business;
using VeriFactu.Config;
using VeriFactu.Net.Core.Net.Rest.Json.Parser;

namespace VeriFactu.Net.Rest
{

    /// <summary>
    /// Representa un cliente API REST de Irene Solutions para Verifactu.
    /// </summary>
    public static class ApiClient
    {

        #region Variables Privadas Estáticas

        static Settings _Settings;

        #endregion

        #region Construtores Estáticos

        /// <summary>
        /// Constructor.
        /// </summary>
        static ApiClient() 
        {

            _Settings = Settings.Current;

        }

        #endregion

        #region Métodos Privados Estáticos

        /// <summary>
        /// Realiza una llamada al API y recupera el 
        /// resultado.
        /// </summary>
        /// <param name="invoice">Factura para realizar la llamada.</param>
        /// <param name="url">Endpoint de la llamada.</param>
        /// <returns>Resultado llamada API.</returns>
        public static ExpandoObject Post(Invoice invoice, string url)
        {

            byte[] buff = null;

            invoice.ServiceKey = Settings.Current.Api.ServiceKey;

            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                buff = wc.UploadData(url, Encoding.UTF8.GetBytes(invoice.ToJson()));
            }

            var json = Encoding.UTF8.GetString(buff);
            var jsonParser = new JsonParser(json);

            return jsonParser.GetResult();

        }

        #endregion

        #region Propiedades Públicas Estáticas

        /// <summary>
        /// Datos API cargados de configuración.
        /// </summary>
        public static Api Api 
        { 
            get 
            { 
                return _Settings.Api;
            } 
        }

        #endregion

        #region Métodos Públicos Estáticos

        /// <summary>
        /// Crea un registro de alta mediante el API.
        /// </summary>
        /// <param name="invoice">Factura a remitir de alta.</param>
        /// <returns>Resultado llamada API.</returns>
        public static ExpandoObject Create(Invoice invoice) 
        {

            return Post(invoice, Api.EndPointCreate);

        }

        /// <summary>
        /// Crea un registro de anulación mediante el API.
        /// </summary>
        /// <param name="invoice">Factura a anular.</param>
        /// <returns>Resultado llamada API.</returns>
        public static ExpandoObject Delete(Invoice invoice)
        {

            return Post(invoice, Api.EndPointCancel);

        }

        /// <summary>
        /// Crea un código QR mediante el API.
        /// </summary>
        /// <param name="invoice">Factura para el QR.</param>
        /// <returns>Resultado llamada API.</returns>
        public static ExpandoObject GetQr(Invoice invoice)
        {

            return Post(invoice, Api.EndPointGetQrCode);

        }

        /// <summary>
        /// Crea un código QR mediante el API.
        /// </summary>
        /// <returns>Resultado llamada API.</returns>
        public static ExpandoObject GetSellers()
        {

            var invoice = new Invoice("", new DateTime(1, 1, 1), "");
            return Post(invoice, Api.EndPointGetSellers);

        }

        #endregion

    }

}