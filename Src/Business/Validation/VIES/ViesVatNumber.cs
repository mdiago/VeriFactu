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

using System.Net;
using System.Text.RegularExpressions;

namespace VeriFactu.Business.Validation.VIES
{

    /// <summary>
    /// Validador de NIF comunitarios mediante API REST.
    /// https://ec.europa.eu/taxation_customs/vies/#/technical-information
    /// </summary>
    public class ViesVatNumber
    {

        /// <summary>
        /// Url del endpoint de validación.
        /// </summary>
        static readonly string UrlValidate = "https://ec.europa.eu/taxation_customs/vies/rest-api//check-vat-number";

        /// <summary>
        /// Valida un número de IVA intracomunitario.
        /// </summary>
        /// <param name="vatNumber">Número de IVA intracomunitario
        /// incluyendo el código de país.</param>
        /// <returns>True si es válido y false en caso contario.</returns>
        public static bool Validate(string vatNumber) 
        {

            var country = vatNumber.Substring(0, 2);
            var number = vatNumber.Substring(2, vatNumber.Length - 2);

            var json = "{\"countryCode\": \""+ country + "\",\"vatNumber\": \""+ number + "\"}";
            string valid = null;

            using (WebClient webClient = new WebClient()) 
            {

                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                string response = webClient.UploadString(UrlValidate, json);
                valid = Regex.Match(response, @"(?<=\Wvalid\W\s*:\s*)\w+").Value;

            }

            return (valid == "true");

        }
    }
}
