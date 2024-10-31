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

namespace VeriFactu.Business.Validation.NIF.TaxId
{

    /// <summary>
    /// Equivalencias de letras con números para el calculo del número de control en el NIF.
    /// </summary>
    public enum TaxIdEsDCNif
    {
        /// <summary>
        /// La letra J equivale a 0.
        /// </summary>
        J = 0,
        /// <summary>
        /// La letra A equivale a 1.
        /// </summary>
        A = 1,
        /// <summary>
        /// La letra B equivale a 2.
        /// </summary>
        B = 2,
        /// <summary>
        /// La letra C equivale a 3.
        /// </summary>
        C = 3,
        /// <summary>
        /// La letra D equivale a 4.
        /// </summary>
        D = 4,
        /// <summary>
        /// La letra E equivale a 5.
        /// </summary>
        E = 5,
        /// <summary>
        /// La letra F equivale a 6.
        /// </summary>
        F = 6,
        /// <summary>
        /// La letra G equivale a 7.
        /// </summary>
        G = 7,
        /// <summary>
        /// La letra H equivale a 8.
        /// </summary>
        H = 8,
        /// <summary>
        /// La letra I equivale a 9.
        /// </summary>
        I = 9
    }

}
