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

namespace VeriFactu.Business.Validation.NIF.TaxId
{

    /// <summary>
    /// Equivalencias de letras con números para el cálculo del número de control en el NIE.
    /// </summary>
    public enum TaxIdEsDCDniNie
    {
        /// <summary>
        /// La letra T equivale a 0.
        /// </summary>
        T = 0,
        /// <summary>
        /// La letra R equivale a 1.
        /// </summary>
        R = 1,
        /// <summary>
        /// La letra W equivale a 2.
        /// </summary>
        W = 2,
        /// <summary>
        /// La letra A equivale a 3.
        /// </summary>
        A = 3,
        /// <summary>
        /// La letra G equivale a 4.
        /// </summary>
        G = 4,
        /// <summary>
        /// La letra M equivale a 5.
        /// </summary>
        M = 5,
        /// <summary>
        /// La letra Y equivale a 6.
        /// </summary>
        Y = 6,
        /// <summary>
        /// La letra F equivale a 7.
        /// </summary>
        F = 7,
        /// <summary>
        /// La letra P equivale a 8.
        /// </summary>
        P = 8,
        /// <summary>
        /// La letra D equivale a 9.
        /// </summary>
        D = 9,
        /// <summary>
        /// La letra X equivale a 10.
        /// </summary>
        X = 10,
        /// <summary>
        /// La letra B equivale a 11.
        /// </summary>
        B = 11,
        /// <summary>
        /// La letra N equivale a 12.
        /// </summary>
        N = 12,
        /// <summary>
        /// La letra J equivale a 13.
        /// </summary>
        J = 13,
        /// <summary>
        /// La letra T equivale a 14.
        /// </summary>
        Z = 14,
        /// <summary>
        /// La letra Z equivale a 15.
        /// </summary>
        S = 15,
        /// <summary>
        /// La letra S equivale a 16.
        /// </summary>
        Q = 16,
        /// <summary>
        /// La letra Q equivale a 17.
        /// </summary>
        V = 17,
        /// <summary>
        /// La letra V equivale a 18.
        /// </summary>
        H = 18,
        /// <summary>
        /// La letra H equivale a 19.
        /// </summary>
        L = 19,
        /// <summary>
        /// La letra L equivale a 20.
        /// </summary>
        C = 20,
        /// <summary>
        /// La letra C equivale a 21.
        /// </summary>
        K = 21,
        /// <summary>
        /// La letra K equivale a 22.
        /// </summary>
        E = 22
    }

}