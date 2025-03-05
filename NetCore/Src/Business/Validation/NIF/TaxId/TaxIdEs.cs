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
using System.Text.RegularExpressions;

namespace VeriFactu.Business.Validation.NIF.TaxId
{
    /// <summary>
    /// Clase para almacenar, verificar y obtener información de números de identificación fiscal españoles
    /// de acuerdo con el Real Decreto 1065/2007, de 27 de julio.
    /// </summary>
    public class TaxIdEs
    {  

        /* Regular expresions related to TaxIdentificationNumber */

        #region Regular expresions related to TaxIdentificationNumber...

        private readonly Regex rgxTaxIdentification = new Regex(PatternTaxIdentification);
        private readonly Regex rgxTaxIdNieIndividual = new Regex(PatternTaxIdNieIndividual);
        private readonly Regex rgxTaxIdDniIndividual = new Regex(PatternTaxIdDniIndividual);
        private readonly Regex rgxTaxIdNifIndividual = new Regex(PatternTaxIdNifIndividual);
        private readonly Regex rgxTaxIdNifLegalEntity = new Regex(PatternTaxIdNifLegalEntity);
        // No residentes y corporaciones locales

        // Digito de control será una LETRA si la clave de entidad es P, Q, S o W. 
        // O también si los dos dígitos iniciales indican "No Residente"

        private readonly Regex rgxTaxIdNifLegalEntityNRyCL = new Regex(PatternTaxIdNifLegalEntityNRyCL);

        #endregion

        /* Dictionaries related to TaxIdentificationNumber */

        #region Dictionaries related to TaxIdentificationNumber...

        /// <summary>
        /// Diccionario de códigos de provincia definidos para el NIF de personas jurídicas.
        /// No significativo ya.
        /// </summary>
        static readonly Dictionary<string, string> TaxCCAA = new Dictionary<string, string>()
        {
             {"01","Álava"},
             {"02","Albacete"},
             {"03","Alicante"},
             {"53","Alicante"},
             {"54","Alicante"},
             {"04","Almería"},
             {"05","Ávila"},
             {"06","Badajoz"},
             {"07","Islas Baleares"},
             {"57","Islas Baleares"},
             {"08","Barcelona"},
             {"58","Barcelona"},
             {"59","Barcelona"},
             {"60","Barcelona"},
             {"61","Barcelona"},
             {"62","Barcelona"},
             {"63","Barcelona"},
             {"64","Barcelona"},
             {"65","Barcelona"},
             {"66","Barcelona"},
             {"68","Barcelona"},
             {"09","Burgos"},
             {"10","Cáceres"},
             {"11","Cádiz"},
             {"72","Cádiz"},
             {"12","Castellón"},
             {"13","Ciudad Real"},
             {"14","Córdoba"},
             {"56","Córdoba"},
             {"15","La Coruña"},
             {"70","La Coruña"},
             {"16","Cuenca"},
             {"17","Gerona"},
             {"55","Gerona"},
             {"18","Granada"},
             {"19","Guadalajara"},
             {"20","Guipúzcoa"},
             {"21","Huelva"},
             {"22","Huesca"},
             {"23","Jaén"},
             {"24","León"},
             {"25","Lérida"},
             {"26","La Rioja"},
             {"27","Lugo"},
             {"28","Madrid"},
             {"78","Madrid"},
             {"79","Madrid"},
             {"80","Madrid"},
             {"81","Madrid"},
             {"82","Madrid"},
             {"83","Madrid"},
             {"84","Madrid"},
             {"85","Madrid"},
             {"86","Madrid"},
             {"29","Málaga"},
             {"92","Málaga"},
             {"93","Málaga"},
             {"30","Murcia"},
             {"73","Murcia"},
             {"31","Navarra"},
             {"71","Navarra"},
             {"32","Orense"},
             {"33","Asturias"},
             {"74","Asturias"},
             {"34","Palencia"},
             {"35","Las Palmas"},
             {"76","Las Palmas"},
             {"36","Pontevedra"},
             {"94","Pontevedra"},
             {"37","Salamanca"},
             {"38","Santa Cruz de Tenerife"},
             {"75","Santa Cruz de Tenerife"},
             {"39","Cantabria"},
             {"40","Segovia"},
             {"41","Sevilla"},
             {"90","Sevilla"},
             {"91","Sevilla"},
             {"42","Soria"},
             {"43","Tarragona"},
             {"77","Tarragona"},
             {"44","Teruel"},
             {"45","Toledo"},
             {"46","Valencia"},
             {"96","Valencia"},
             {"97","Valencia"},
             {"98","Valencia"},
             {"47","Valladolid"},
             {"48","Vizcaya"},
             {"95","Vizcaya"},
             {"49","Zamora"},
             {"50","Zaragoza"},
             {"99","Zaragoza"},
             {"51","Ceuta"},
             {"52","Melilla"}
        };

        /// <summary>
        /// Tipos de organizaciones asociados a la primera letra del NIF de persona jurídica.
        /// </summary>
        static readonly Dictionary<string, string> LegalEntities = new Dictionary<string, string>()
        {
            {"A", "Sociedades anónimas"},
            {"B", "Sociedades de responsabilidad limitada"},
            {"C", "Sociedades colectivas"},
            {"D", "Sociedades comanditarias"},
            {"E", "Comunidades de bienes y herencias yacentes"},
            {"F", "Sociedades cooperativas"},
            {"G", "Asociaciones"},
            {"H", "Comunidades de propietarios en régimen de propiedad horizontal"},
            {"J", "Sociedades civiles, con o sin personalidad jurídica"},
            {"N", "Entidades extranjeras"},
            {"P", "Corporaciones Locales"},
            {"Q", "Organismos públicos"},
            {"R", "Congregaciones e instituciones religiosas"},
            {"S", "Órganos de la Administración del Estado y de las Comunidades Autónomas"},
            {"U", "Uniones Temporales de Empresas"},
            {"V", "Otros tipos no definidos en el resto de claves"},
            {"W", "Establecimientos permanentes de entidades no residentes en España"}
        };

        /// <summary>
        /// Tipos de personas físicas definidos por la letra del NIF o NIE.
        /// </summary>
        static readonly Dictionary<string, string> Individuals = new Dictionary<string, string>()
        {
            {"K", "Españoles menores de 14 años que carezcan de DNI. Anterior a la entrada en vigor de la Orden EHA/451/2008 el 1 de julio de 2008 también se incluían los extranjeros menores de 18 años que carecían de NIE (para los menores de la edad indicada no es obligatorio que tengan documentación de identidad, pero pueden solicitar un NIF si lo necesitan), esta Orden separa las claves de españoles y extranjeros, de forma que los extranjeros sin NIE transitoria o definitivamente pueden solicitar un NIF M."},
            {"L", "Españoles mayores de 14 años residentes en el extranjero y que no tengan DNI que se trasladan a España por un tiempo inferior a seis meses."},
            {"M", "Extranjeros sin NIE, de forma transitoria por estar obligados a tenerlo o bien de forma definitiva al no estar obligados a ello. Anterior a la entrada en vigor de la Orden EHA/451/2008 el 1 de julio de 2008 sólo se incluían los extranjeros sin NIE miembros de embajadas, consulados u organismos internacionales y que estuvieran acreditados en España (no están obligados a disponer de NIE)."},
            {"X", "Extranjeros residentes en España e identificados por la Policía con un número de identidad de extranjero (NIE), asignado hasta el 15 de julio de 2008. Los NIE, según la Orden de 7 de febrero de 1997, inicialmente constaban de X + 8 números + dígito de control, la Orden INT/2058/2008 redujo de 8 a 7 los números para que tuvieran la misma longitud que los NIF y CIF, y añadió las claves Y y Z antes que se asignaran 9999999 NIE X y desbordara la capacidad de los 7 dígitos, pero esta Orden mantiene la validez de los NIE X de 8 dígitos anteriores ya asignados."},
            {"Y", "Extranjeros residentes en España e identificados por la Policía con un NIE, asignado desde el 16 de julio de 2008 (Orden INT/2058/2008, BOE del 15 de julio )"},
            {"Z", "Letra reservada para Extranjeros identificados por la Policía, para cuando se agoten los 'NIE Y'"}
        };

        #endregion

        /* Public fields of class TaxIdEs */

        #region Public fields of class TaxIdEs...

        /// <summary>
        /// Patrón de expresión regular que identifica códigos
        /// de identificación fiscal españoles.
        /// </summary>
        public static string PatternTaxIdentification = @"(\b[A-HJ-NP-SU-Z0-9]{1}[0-9]{7}[A-NP-TV-Z0-9]{1}\b)";

        /// <summary>
        /// Patrón de expresión regular que identifica códigos
        /// de identificación fiscal españoles para extranjeros NIE.
        /// </summary>
        public static string PatternTaxIdNieIndividual =@"(\b[XYZ]{1}[0-9]{7}[A-HJ-NP-Z]{1}\b)";


        /// <summary>
        /// Patrón de expresión regular que identifica códigos
        /// de identificación fiscal españoles para DNI.
        /// </summary>
        public static string PatternTaxIdDniIndividual = @"(\b[0-9]{1}[0-9]{7}[A-HJ-NP-Z]{1}\b)";


        /// <summary>
        /// Patrón de expresión regular que identifica códigos
        /// de identificación fiscal españoles presona física NIF.
        /// </summary>
        public static string PatternTaxIdNifIndividual = @"(\b[KLM]{1}[0-9]{7}[A-HJ-NP-Z]{1}\b)";


        /// <summary>
        /// Patrón de expresión regular que identifica códigos
        /// de identificación fiscal españoles persona jurídica NIF empresa.
        /// </summary>
        public static string PatternTaxIdNifLegalEntity = @"(\b[A-HJ-NP-SUVW]{1}[0-9]{8}\b)";
        // No residentes y corporaciones locales

        // Digito de control será una LETRA si la clave de entidad es P, Q, S o W. 
        // O también si los dos dígitos iniciales indican "No Residente"


        /// <summary>
        /// Patrón de expresión regular que identifica códigos
        /// de identificación fiscal españoles para no resiedentes
        /// y corporaciones locales.
        /// </summary>
        public static string PatternTaxIdNifLegalEntityNRyCL = @"(\b[BSNPQRW]{1}[0-9]{7}[A-NP-Z]{1}\b)";

        /// <summary>
        /// Tipo de persona. Física o jurídica.
        /// </summary>
        public TaxIdEsPersonTypeCode PersonType;

        /// <summary>
        /// Denominación del tipo de persona.
        /// </summary>
        public string PersonTypeDen = "";

        /// <summary>
        /// Tipo de número de identificación fiscal: DNI, NIF o NIE.
        /// </summary>
        public TaxIdEsTypes TaxIdentificationType;

        /// <summary>
        /// Último error en la validación del número de identificación fiscal.
        /// </summary>
        public TaxIdEsError LastError;

        /// <summary>
        /// Almacena el dígito de control si se trata de una letra.
        /// </summary>
        public string DCStr = "";

        /// <summary>
        /// Almacena el dígito de control si se trata de un número.
        /// </summary>
        public int DCNum = 0;

        /// <summary>
        /// Flag que indica si el dígito de control es un número. Si es así tiene el valor true.
        /// </summary>
        public bool IsDCNum;

        /// <summary>
        /// Si la validación del dígito de control ha sido correcta contiene el valor true.
        /// </summary>
        public bool IsDCOK = false;

        #endregion

        /* Private fields of class TaxIdEs */

        #region Private fields of class TaxIdEs...

        private string taxCode;
        private string strFirst = "";

        #endregion

        /* Public properties of class TaxIdEs */

        #region Public properties of class TaxIdEs...

        /// <summary>
        /// String de caracteres alfanuméricos que representa un número de identificación fiscal.
        /// </summary>
        public string TaxCode
        {
            get
            {
                return taxCode;
            }
            set
            {
                try
                {
                    
                    if (value == null) 
                    {
                        taxCode = null;
                        LastError = TaxIdEsError.InvalidString;
                        return;
                    }

                    taxCode = value.ToUpper();

                    if (!rgxTaxIdentification.IsMatch(taxCode))
                    {
                        taxCode = null;
                        LastError = TaxIdEsError.InvalidString;
                        return;
                    }
                    int numF;
                    DCStr = taxCode.Substring(taxCode.Length - 1, 1);
                    IsDCNum = Int32.TryParse(DCStr, out DCNum);
                    if (IsDCNum) DCStr = "";
                    strFirst = taxCode.Substring(0, 1);
                    if (Int32.TryParse(strFirst, out numF))
                    {
                        PersonType = TaxIdEsPersonTypeCode.Individual;
                        PersonTypeDen = "Españoles con documento nacional de identidad (DNI) asignado por el Ministerio del Interior";
                        TaxIdentificationType = TaxIdEsTypes.DNI;
                    }
                    else
                    {
                        if (Individuals.TryGetValue(strFirst, out PersonTypeDen))
                        {
                            PersonType = TaxIdEsPersonTypeCode.Individual;
                            if (strFirst == "K" | strFirst == "L" | strFirst == "M")
                                TaxIdentificationType = TaxIdEsTypes.NIF;
                            else
                                TaxIdentificationType = TaxIdEsTypes.NIE;
                        }
                        else
                        {
                            if (LegalEntities.TryGetValue(strFirst, out PersonTypeDen))
                            {
                                PersonType = TaxIdEsPersonTypeCode.LegalEntity;
                                TaxIdentificationType = TaxIdEsTypes.NIF;
                            }
                            else
                            {
                                LastError = TaxIdEsError.InvalidFirstChar;
                            }
                        }
                    }
                    if (TaxCode == null)
                        return;
                    switch (this.TaxIdentificationType)
                    {
                        case TaxIdEsTypes.DNI:
                            if (!rgxTaxIdDniIndividual.IsMatch(TaxCode))
                                throw new TaxIdEsException("TaxIdEs.DNI don't match the pattern: " + 
                                    rgxTaxIdDniIndividual.ToString());
                            break;
                        case TaxIdEsTypes.NIE:
                            if (!rgxTaxIdNieIndividual.IsMatch(TaxCode))
                                throw new TaxIdEsException("TaxIdEs.DNI don't match the pattern: " + 
                                    rgxTaxIdNieIndividual.ToString());
                            break;
                        case TaxIdEsTypes.NIF:
                            switch (this.PersonType)
                            {
                                case TaxIdEsPersonTypeCode.Individual:
                                    if (!rgxTaxIdNifIndividual.IsMatch(TaxCode))
                                        throw new TaxIdEsException("TaxIdEs.DNI don't match the pattern: " + 
                                            rgxTaxIdNifIndividual.ToString());
                                    break;
                                case TaxIdEsPersonTypeCode.LegalEntity:
                                    if (!rgxTaxIdNifLegalEntity.IsMatch(TaxCode) && 
                                        !rgxTaxIdNifLegalEntityNRyCL.IsMatch(TaxCode))
                                        throw new TaxIdEsException("TaxIdEs.DNI don't match the pattern: " +
                                            rgxTaxIdNifLegalEntity.ToString());
                                    break;
                                default:
                                    throw new TaxIdEsException("Unknown TaxIdEsPersonTypeCode.");
                            }
                            break;
                        default:
                            throw new TaxIdEsException("Unknown TaxIdentificationType.");
                    }
                    if (this.TaxIdentificationType == TaxIdEsTypes.NIF)
                    {
                        IsDCOK = (IsDCNum) ? (GetDCNif() == (TaxIdEsDCNif)DCNum) : 
                            (GetDCNif() == (TaxIdEsDCNif)Enum.Parse(typeof(TaxIdEsDCNif), DCStr));
                    }
                    else
                    {
                        IsDCOK = (IsDCNum) ? (GetDCDniNie() == (TaxIdEsDCDniNie)DCNum) : 
                            (GetDCDniNie() == (TaxIdEsDCDniNie)Enum.Parse(typeof(TaxIdEsDCDniNie), DCStr));
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        #endregion

        /* Constructors of class TaxIdEs */

        #region Constructor of class TaxIdEs...

        /// <summary>
        /// Crea una instancia de la clase TaxIdEs utilizada para la validación de números de identificación fiscal
        /// y para la obtención de información del tipo de persona y ubicación de las personas jurídicas.
        /// </summary>
        /// <param name="taxcode">Número de identificación fiscal a partir del cual crear la instacia.</param>
        public TaxIdEs(string taxcode)
        {
            TaxCode = taxcode;
        }

        /// <summary>
        /// Crea una instancia de la clase TaxIdEs vacía. 
        /// </summary>
        public TaxIdEs()
        {
        }

        #endregion

        /* Private methods and functions of class TaxIdEs */

        #region Private methods and functions of class TaxIdEs...

        /// <summary>
        /// Calcula el dígito de control de un NIF.
        /// </summary>
        /// <returns>Devualve un tipo TaxIdEsDCNif que repersenta un 
        /// número del 1 al 9, o bien una letra.</returns>
        private TaxIdEsDCNif GetDCNif()
        {
            try
            {
                string numCif = TaxCode.Substring(1, 7);
                int nCif = Int32.Parse(numCif);
                /* Suma A: número de las posiciones pares */
                int sumA = 0;
                for (int pos = 1; pos < numCif.Length; pos += 2)
                    sumA += Int32.Parse(numCif.Substring(pos, 1));
                /* Suma B: número de las posiciones impares x 2 (suma de decenas y unidades) */
                int sumB = 0;
                for (int pos = 0; pos < numCif.Length; pos += 2)
                {
                    int digI = 2 * Int32.Parse(numCif.Substring(pos, 1));
                    int digIU = digI % 10; /* Unidades */
                    int digID = (digI - digIU) / 10; /* Decenas */
                    sumB += (digIU + digID);
                }
                /* Continuamos ...*/
                int sumC = sumA + sumB;
                int digE = sumC % 10;
                int digD = 0;
                if (digE != 0) digD = 10 - digE;
                return (TaxIdEsDCNif)digD;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Calcula el dígito de control de un DNI o NIE.
        /// </summary>
        /// <returns>Devualve un tipo TaxIdEsDCNif que repersenta 
        /// un número del 1 al 22, o bien una letra.</returns>
        private TaxIdEsDCDniNie GetDCDniNie()
        {
            try
            {
                int nCif = (TaxCode[0] > 57) ?  // Si es una letra entonces se trata de un NIE
                    ((int)TaxCode[0] - (int)'X') * (int)Math.Pow(10, 7) + Int32.Parse(TaxCode.Substring(1, 7)) : // X=0, Y=1, Z=2
                    Int32.Parse(TaxCode.Substring(0, 8));
                /* Resto de división entre 23 */
                return (TaxIdEsDCDniNie)(nCif % 23);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        /* Public methods and functions of class TaxIdEs */

        #region Public methods and functions of class TaxIdEs...

        /// <summary>
        /// Devuelve el nombre de la provincia representada por los dos primeros dígitos de un NIF de persona jurídica.
        /// </summary>
        /// <returns>String con el nombre de la provincia. Cadena vacía en caso de no conseguirlo.</returns>
        public string GetCCAA()
        {
            string ret = "";
            try
            {
                if (TaxCode == null)
                    return ret;
                TaxCCAA.TryGetValue(TaxCode.Substring(1, 2), out ret);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ret;
        }

        #endregion

    }

}
