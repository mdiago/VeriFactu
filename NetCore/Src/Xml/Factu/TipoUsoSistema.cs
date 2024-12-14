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

using System.Xml.Serialization;

namespace VeriFactu.Xml.Factu
{

    /// <summary>
    /// Clave que identifica el tipo de uso del
    /// sistema informático de facturación. Alfanumérico(2) L14.
    /// </summary>
    public enum TipoUsoSistema
    {

        /// <summary>
        /// Solo funciona en modo VERIFACTU.
        /// </summary>
        [XmlEnum("01")]
        Verifactu,

        /// <summary>
        /// Solo funciona en modo no-VERIFACTU (cumpliendo Reglamento).
        /// </summary>
        [XmlEnum("02")]
        NoVerifactu,

        /// <summary>
        /// Funciona tanto en modo VERIFACTU como no-VERIFACTU (cumpliendo Reglamento).
        /// </summary>
        [XmlEnum("03")]
        Ambos,

        /// <summary>
        /// Otros.
        /// </summary>
        [XmlEnum("04")]
        Otros  

    }

}
