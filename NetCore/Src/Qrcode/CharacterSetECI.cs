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

namespace VeriFactu.Qrcode
{

    /// <summary>
    /// Encapsulates a Character Set ECI, according to "Extended Channel Interpretations" 5.3.1.1
    /// of ISO 18004.
    /// </summary>
    /// <author>Sean Owen</author>
    internal sealed class CharacterSetECI 
    {

        #region Variables Privadas Estáticas

        private static IDictionary<String, CharacterSetECI> NAME_TO_ECI;

        #endregion

        #region Variables Privadas de Instancia

        private readonly String encodingName;

        private readonly int value;

        #endregion

        #region Construtores de Instancia

        private CharacterSetECI(int value, String encodingName)
        {
            this.encodingName = encodingName;
            this.value = value;
        }

        #endregion

        #region Métodos Privados Estáticos

        private static void Initialize()
        {
            IDictionary<String, CharacterSetECI> n = new Dictionary<String, CharacterSetECI
                >(29);
            AddCharacterSet(0, "Cp437", n);
            AddCharacterSet(1, new String[] { "ISO8859_1", "ISO-8859-1" }, n);
            AddCharacterSet(2, "Cp437", n);
            AddCharacterSet(3, new String[] { "ISO8859_1", "ISO-8859-1" }, n);
            AddCharacterSet(4, new String[] { "ISO8859_2", "ISO-8859-2" }, n);
            AddCharacterSet(5, new String[] { "ISO8859_3", "ISO-8859-3" }, n);
            AddCharacterSet(6, new String[] { "ISO8859_4", "ISO-8859-4" }, n);
            AddCharacterSet(7, new String[] { "ISO8859_5", "ISO-8859-5" }, n);
            AddCharacterSet(8, new String[] { "ISO8859_6", "ISO-8859-6" }, n);
            AddCharacterSet(9, new String[] { "ISO8859_7", "ISO-8859-7" }, n);
            AddCharacterSet(10, new String[] { "ISO8859_8", "ISO-8859-8" }, n);
            AddCharacterSet(11, new String[] { "ISO8859_9", "ISO-8859-9" }, n);
            AddCharacterSet(12, new String[] { "ISO8859_10", "ISO-8859-10" }, n);
            AddCharacterSet(13, new String[] { "ISO8859_11", "ISO-8859-11" }, n);
            AddCharacterSet(15, new String[] { "ISO8859_13", "ISO-8859-13" }, n);
            AddCharacterSet(16, new String[] { "ISO8859_14", "ISO-8859-14" }, n);
            AddCharacterSet(17, new String[] { "ISO8859_15", "ISO-8859-15" }, n);
            AddCharacterSet(18, new String[] { "ISO8859_16", "ISO-8859-16" }, n);
            AddCharacterSet(20, new String[] { "SJIS", "Shift_JIS" }, n);
            NAME_TO_ECI = n;
        }
        private static void AddCharacterSet(int value, String encodingName, IDictionary<String, CharacterSetECI
    > n)
        {
            CharacterSetECI eci = new CharacterSetECI(value, encodingName);
            n.Put(encodingName, eci);
        }

        private static void AddCharacterSet(int value, String[] encodingNames, IDictionary<String, CharacterSetECI
            > n)
        {
            CharacterSetECI eci = new CharacterSetECI(value, encodingNames
                [0]);
            for (int i = 0; i < encodingNames.Length; i++)
            {
                n.Put(encodingNames[i], eci);
            }
        }

        #endregion

        #region Métodos Públicos Estáticos

        /// <param name="name">character set ECI encoding name</param>
        /// <returns>
        /// 
        /// <see cref="CharacterSetECI"/>
        /// representing ECI for character encoding, or null if it is legal
        /// but unsupported
        /// </returns>
        public static CharacterSetECI GetCharacterSetECIByName(String name)
        {
            if (NAME_TO_ECI == null)
            {
                Initialize();
            }
            return NAME_TO_ECI.Get(name);
        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <returns>name of the encoding.</returns>
        public String GetEncodingName()
        {
            return encodingName;
        }

        /// <returns>the value of the encoding.</returns>
        public int GetValue()
        {
            return value;
        }

        #endregion

    }

}