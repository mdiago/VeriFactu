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

namespace VeriFactu.Xml.Factu.Alta
{

    /// <summary>
    /// Clave que identifica el tipo de régimen del IVA
    /// o una operación con trascendencia tributaria. L8.
    /// </summary>
    public enum ClaveRegimen
    {

        /// <summary>
        /// Operación de régimen general.
        /// </summary>
        [XmlEnum("01")]
        RegimenGeneral,

        /// <summary>
        /// Exportación.
        /// </summary>
        [XmlEnum("02")]
        Exportacion,

        /// <summary>
        /// Operaciones a las que se aplique el régimen especial de bienes usados, 
        /// objetos de arte, antigüedades y objetos de colección.
        /// </summary>
        [XmlEnum("03")]
        Rebu,

        /// <summary>
        /// Régimen especial del oro de inversión.
        /// </summary>
        [XmlEnum("04")]
        OroInversion,

        /// <summary>
        /// Régimen especial de las agencias de viajes.
        /// </summary>
        [XmlEnum("05")]
        AgenciasViajes,

        /// <summary>
        /// Régimen especial grupo de entidades en IVA (Nivel Avanzado).
        /// </summary>
        [XmlEnum("06")]
        GrupoEntidades,

        /// <summary>
        /// Régimen especial del criterio de caja..
        /// </summary>
        [XmlEnum("07")]
        Recc,

        /// <summary>
        /// Operaciones sujetas al IPSI  / IGIC 
        /// (Impuesto sobre la Producción, los Servicios y la Importación
        /// / Impuesto General Indirecto Canario).
        /// </summary>
        [XmlEnum("08")]
        IpsiIgic,

        /// <summary>
        /// Facturación de las prestaciones de servicios
        /// de agencias de viaje que actúan como mediadoras
        /// en nombre y por cuenta ajena (D.A.4ª RD1619/2012).
        /// </summary>
        [XmlEnum("09")]
        MediadoresAgenciasViaje,

        /// <summary>
        /// Cobros por cuenta de terceros de honorarios profesionales
        /// o de derechos derivados de la propiedad industrial,
        /// de autor u otros por cuenta de sus socios,
        /// asociados o colegiados efectuados por sociedades,
        /// asociaciones, colegios profesionales u otras entidades
        /// que realicen estas funciones de cobro.
        /// </summary>
        [XmlEnum("10")]
        CobroTerceros,

        /// <summary>
        /// Operaciones de arrendamiento de local de negocio.
        /// </summary>
        [XmlEnum("11")]
        ArrendamientoLocalNecocio,

        /// <summary>
        /// Factura con IVA pendiente de devengo en certificaciones
        /// de obra cuyo destinatario sea una Administración Pública.
        /// </summary>
        [XmlEnum("12")]
        ObraPteDevengoAdmonPublica,

        /// <summary>
        /// Factura con IVA pendiente de devengo en operaciones de tracto sucesivo.
        /// </summary>
        [XmlEnum("13")]
        TractoSucesivoPteDevengo,

        /// <summary>
        /// Régimen simplificado.
        /// </summary>
        [XmlEnum("14")]
        RegimenSimplificado,

        /// <summary>
        /// Recargo de equivalencia.
        /// </summary>
        [XmlEnum("15")]
        RecargoEquivalencia,

        /// <summary>
        /// Régimen especial de la agricultura.
        /// </summary>
        [XmlEnum("16")]
        RegimenEspecialAgricultura,


    }

}
