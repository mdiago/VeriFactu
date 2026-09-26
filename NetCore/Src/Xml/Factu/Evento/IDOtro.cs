/*
    This file is part of the VeriFactu (R) project.
    Copyright (c) 2024-2026 Irene Solutions SL
    Author: Irene Solutions SL.

    NO VERI*FACTU implementation developed with the valuable contribution of:
    Javier Florit González
    GAMADI CONSULTING BALEARS SL (B57336786)
    javier.florit@gamadic.com

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

using System.Xml.Serialization;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;

namespace VeriFactu.Net.Core.Src.Xml.Factu.Evento
{

    /// <summary>
    /// Identificador de persona física o jurídica distinto del NIF.
    /// </summary>
    [XmlType(Namespace = Namespaces.NamespaceSf)]
    public class IDOtro
    {

        /// <summary>
        /// Código del país asociado al identificador.
        /// Valor esperado:
        /// <para>Código de país según ISO 3166-1 alfa-2. </para>
        /// <para>Propiedad opcional. </para>
        /// </summary>
        [XmlElement(Order = 0)]
        public CodigoPais CodigoPais { get; set; }

        /// <summary>
        /// Con true se serializa el dato, con false no.
        /// </summary>
        [XmlIgnore]
        public bool CodigoPaisSpecified { get; set; }

        /// <summary>
        /// Tipo de identificador de la persona física o jurídica.
        /// Valores esperados:
        /// <para>"02": NIF-IVA. </para>
        /// <para>"03": Pasaporte. </para>
        /// <para>"04": ID en el país de residencia. </para>
        /// <para>"05": Certificado de residencia. </para>
        /// <para>"06": Otro documento probatorio. </para>
        /// <para>"07": No censado. </para>
        /// </summary>
        [XmlElement(Order = 1)]
        public IDType IDType { get; set; }

        /// <summary>
        /// Identificador de la persona física o jurídica.
        /// Valor esperado:
        /// <para>Texto con una longitud máxima de 20 caracteres. </para>
        /// </summary>
        [XmlElement(Order = 2)]
        public string ID { get; set; }

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {
            return CodigoPaisSpecified ? $"{CodigoPais}-{IDType}: {ID}" : $"{IDType}: {ID}";
        }

        #endregion

    }

}