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
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

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
        /// 8 caracteres numéricos
        /// </summary>
        static readonly string Ptt8Num = @"^[0-9]{8}$";

        /// <summary>
        /// 9 caracteres numéricos
        /// </summary>
        static readonly string Ptt9Num = @"^[0-9]{9}$";

        /// <summary>
        /// 10 caracteres numéricos
        /// </summary>
        static readonly string Ptt10Num = @"^[0-9]{10}$";

        /// <summary>
        /// 11 caracteres numéricos
        /// </summary>
        static readonly string Ptt11Num = @"^[0-9]{11}$";

        /// <summary>
        /// 12 caracteres numéricos
        /// </summary>
        static readonly string Ptt12Num = @"^[0-9]{12}$";

        /// <summary>
        /// 8, 9 ó 10 caracteres numéricos
        /// </summary>
        static readonly string Ptt8To10Num = @"^[0-9]{8,10}$";

        /// <summary>
        /// de 2 a 10 caracteres numéricos
        /// </summary>
        static readonly string Ptt2To10Num = @"^[0-9]{2,10}$";

        /// <summary>
        /// 9 ó 12 caracteres numéricos
        /// </summary>
        static readonly string Ptt9o12Num = @"^(?:\d{9}|\d{12})$";

        /// <summary>
        ///  9 ó 10 caracteres numéricos
        /// </summary>
        static readonly string Ptt9o10Num = @"^(?:\d{9}|\d{10})$";

        /// <summary>
        /// 8 ó 9 caracteres alfanuméricos
        /// </summary>
        static readonly string Ptt8To9Alfanum = @"^[A-Za-z0-9]{8,9}$";

        /// <summary>
        /// 9 caracteres alfanuméricos
        /// </summary>
        static readonly string Ptt9Alfanum = @"^[A-Za-z0-9]{9}$";

        /// <summary>
        /// 11 caracteres alfanumérico
        /// </summary>
        static readonly string Ptt11Alfanum = @"^[A-Za-z0-9]{11}$";

        /// <summary>
        /// 12 caracteres alfanumérico
        /// </summary>
        static readonly string Ptt12Alfanum = @"^[A-Za-z0-9]{12}$";

        /// <summary>
        /// 5, 9 ó 12 caracteres alfanuméricos
        /// </summary>
        static readonly string Ptt5o9o12Alfanum = @"^(?:[A-Za-z0-9]{5}|[A-Za-z0-9]{9}|[A-Za-z0-9]{12})$";

        /// <summary>
        /// Patrones de expresiones regulares por pais.
        /// </summary>
        static readonly Dictionary<string, string> _PatternsByCountry = new Dictionary<string, string>() 
        {
            {"DE",  Ptt9Num},           // 9 caracteres numéricos
            {"AT",  Ptt9Alfanum},       // 9 caracteres alfanuméricos
            {"BE",  Ptt10Num},          // 10 caracteres numéricos
            {"CY",  Ptt9Alfanum},       // 9 caracteres alfanuméricos
            {"CZ",  Ptt8To10Num},       // 8, 9 ó 10 caracteres numéricos
            {"HR",  Ptt11Num},          // 11 caracteres numéricos
            {"DK",  Ptt8Num},           // 8 caracteres numéricos
            {"SK",  Ptt10Num},          // 10 caracteres numéricos
            {"SI",  Ptt8Num},           // 8 caracteres numéricos
            {"EE",  Ptt9Num},           // 9 caracteres numéricos
            {"FI",  Ptt8Num},           // 8 caracteres numéricos
            {"FR",  Ptt11Alfanum},      // 11 caracteres alfanumérico
            {"EL",  Ptt9Num},           // 9 caracteres numéricos
            {"GB",  Ptt5o9o12Alfanum},  // 5, 9 ó 12 caracteres alfanuméricos
            {"XI",  Ptt5o9o12Alfanum},  // 5, 9 ó 12 caracteres alfanuméricos
            {"NL",  Ptt12Alfanum},      // 12 caracteres alfanumérico
            {"HU",  Ptt8Num},           // 8 caracteres numéricos
            {"IT",  Ptt11Num},          // 11 caracteres numéricos
            {"IE",  Ptt8To9Alfanum},    // 8 ó 9 caracteres alfanuméricos
            {"LV",  Ptt9Num},           // 11 caracteres numéricos
            {"LT",  Ptt9o12Num},        // 9 ó 12 caracteres numéricos
            {"LU",  Ptt8Num},           // 8 caracteres numéricos
            {"MT",  Ptt8Num},           // 8 caracteres numéricos
            {"PL",  Ptt10Num},          // 10 caracteres numéricos
            {"PT",  Ptt9Num},           // 9 caracteres numéricos
            {"SE",  Ptt12Num},          // 12 caracteres numéricos
            {"BG",  Ptt9o10Num},        // 9 ó 10 caracteres numéricos
            {"RO",  Ptt2To10Num}        // de 2 a 10 caracteres numéricos sin ceros a la izquierda
        };


        /// <summary>
        /// Valida la estructura de un número de IVA intracomunitario.
        /// </summary>
        /// <param name="vatNumber">Número de IVA intracomunitario
        /// incluyendo el código de país.</param>
        /// <returns>True si es válido y false en caso contario.</returns>
        public static bool ValidateStructure(string vatNumber)
        {

            var country = vatNumber.Substring(0, 2);
            var number = vatNumber.Substring(2, vatNumber.Length - 2);

            if (!_PatternsByCountry.ContainsKey(country))
                return false;

            var pattern = _PatternsByCountry[country];

            return Regex.IsMatch(number, pattern);

        }

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