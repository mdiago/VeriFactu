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

namespace VeriFactu.Qrcode.Exceptions 
{

    /// <summary>
    /// Clase para mensajes predeterminados de error
    /// mediante constantes.
    /// </summary>
    /// <author>bruno@lowagie.com (Bruno Lowagie, Paulo Soares, et al.) - creator</author>
    public sealed class BarcodeExceptionMessageConstant 
    {

        /// <summary>
        /// Codabar must have at least start and stop character.
        /// </summary>
        public const String CODABAR_MUST_HAVE_AT_LEAST_START_AND_STOP_CHARACTER = "Codabar must have at least start "
             + "and stop character.";

        /// <summary>
        /// Codabar must have one of 'ABCD'.
        /// </summary>
        public const String CODABAR_MUST_HAVE_ONE_ABCD_AS_START_STOP_CHARACTER = "Codabar must have one of 'ABCD' "
             + "as start/stop character.";

        /// <summary>
        /// Illegal character in Codabar Barcode.
        /// </summary>
        public const String ILLEGAL_CHARACTER_IN_CODABAR_BARCODE = "Illegal character in Codabar Barcode.";

        /// <summary>
        /// In Codabar, start/stop characters are only allowed at the extremes.
        /// </summary>
        public const String IN_CODABAR_START_STOP_CHARACTERS_ARE_ONLY_ALLOWED_AT_THE_EXTREMES = "In Codabar, " + "start/stop characters are only allowed at the extremes.";

        /// <summary>
        /// Invalid codeword size.
        /// </summary>
        public const String INVALID_CODEWORD_SIZE = "Invalid codeword size.";

        /// <summary>
        /// macroSegmentId must be greater than or equa 0
        /// </summary>
        public const String MACRO_SEGMENT_ID_MUST_BE_GT_OR_EQ_ZERO = "macroSegmentId must be >= 0";

        /// <summary>
        /// macroSegmentId must be greater than 0
        /// </summary>
        public const String MACRO_SEGMENT_ID_MUST_BE_GT_ZERO = "macroSegmentId must be > 0";

        /// <summary>
        /// macroSegmentId must be minor macroSemgentCount. 
        /// </summary>
        public const String MACRO_SEGMENT_ID_MUST_BE_LT_MACRO_SEGMENT_COUNT = "macroSegmentId " + "must be < macroSemgentCount";

        /// <summary>
        /// Text cannot be null.
        /// </summary>
        public const String TEXT_CANNOT_BE_NULL = "Text cannot be null.";

        /// <summary>
        /// 
        /// </summary>
        public const String TEXT_IS_TOO_BIG = "Text is too big.";

        /// <summary>
        /// The text length must be even.
        /// </summary>
        public const String TEXT_MUST_BE_EVEN = "The text length must be even.";

        /// <summary>
        /// The two barcodes must be composed externally.
        /// </summary>
        public const String TWO_BARCODE_MUST_BE_EXTERNALLY = "The two barcodes must be composed externally.";

        /// <summary>
        /// 
        /// </summary>
        public const String THERE_ARE_ILLEGAL_CHARACTERS_FOR_BARCODE_128 = "There are illegal characters for " + "barcode 128 in {0}.";

        /// <summary>
        /// Constructor.
        /// </summary>
        private BarcodeExceptionMessageConstant() 
        {
        }

    }

}
