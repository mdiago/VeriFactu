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

using System.Xml.Serialization;

namespace VeriFactu.Xml.Factu.Alta
{

    /// <summary>
    /// Clave que identifica el tipo de régimen del IVA o IGIC
    /// o una operación con trascendencia tributaria. L8A/L8B.
    /// </summary>
    public enum ClaveRegimen
    {

        /// <summary>
        /// Operación de régimen general ('01').
        /// </summary>
        [XmlEnum("01")]
        RegimenGeneral = 1,

        /// <summary>
        /// Exportación ('02').
        /// </summary>
        [XmlEnum("02")]
        Exportacion = 2,

        /// <summary>
        /// Operaciones a las que se aplique el régimen especial de bienes usados, 
        /// objetos de arte, antigüedades y objetos de colección ('03').
        /// </summary>
        [XmlEnum("03")]
        Rebu = 3,

        /// <summary>
        /// Régimen especial del oro de inversión ('04').
        /// </summary>
        [XmlEnum("04")]
        OroInversion = 4,

        /// <summary>
        /// Régimen especial de las agencias de viajes ('05').
        /// </summary>
        [XmlEnum("05")]
        AgenciasViajes = 5,

        /// <summary>
        /// Régimen especial grupo de entidades en IVA/IGIC (Nivel Avanzado) ('06').
        /// </summary>
        [XmlEnum("06")]
        GrupoEntidades = 6,

        /// <summary>
        /// Régimen especial del criterio de caja ('07').
        /// </summary>
        [XmlEnum("07")]
        Recc = 7,

        /// <summary>
        /// <para> L8A: Operaciones sujetas al IPSI  / IGIC (Impuesto sobre la Producción, los Servicios y la Importación  / Impuesto General Indirecto Canario)</para>
        /// <para> L8B: Operaciones sujetas al IPSI / IVA (Impuesto sobre la Producción, los Servicios y la Importación / Impuesto sobre el Valor Añadido).</para>
        /// ('08').
        /// </summary>
        [XmlEnum("08")]
        IpsiIgic = 8,

        /// <summary>
        /// Facturación de las prestaciones de servicios
        /// de agencias de viaje que actúan como mediadoras
        /// en nombre y por cuenta ajena (D.A.4ª RD1619/2012) ('09').
        /// </summary>
        [XmlEnum("09")]
        MediadoresAgenciasViaje = 9,

        /// <summary>
        /// Cobros por cuenta de terceros de honorarios profesionales
        /// o de derechos derivados de la propiedad industrial,
        /// de autor u otros por cuenta de sus socios,
        /// asociados o colegiados efectuados por sociedades,
        /// asociaciones, colegios profesionales u otras entidades
        /// que realicen estas funciones de cobro ('10').
        /// </summary>
        [XmlEnum("10")]
        CobroTerceros = 10,

        /// <summary>
        /// Operaciones de arrendamiento de local de negocio ('11').
        /// </summary>
        [XmlEnum("11")]
        ArrendamientoLocalNecocio = 11,

        /// <summary>
        /// <para> L8A: Factura con IVA pendiente de devengo en certificaciones de obra cuyo destinatario sea una Administración Pública.</para>
        /// <para> L8B: Factura con IGIC pendiente de devengo en certificaciones de obra cuyo destinatario sea una Administración Pública.</para>
        /// <para> ('14')</para>
        /// </summary>
        [XmlEnum("14")]
        ObraPteDevengoAdmonPublica = 14,

        /// <summary>
        /// <para> L8A: Factura con IVA pendiente de devengo en operaciones de tracto sucesivo.</para>
        /// <para> L8B: Factura con IGIC pendiente de devengo en operaciones de tracto sucesivo.</para>
        /// <para> ('15')</para>
        /// </summary>
        [XmlEnum("15")]
        TractoSucesivoPteDevengo = 15,

        /// <summary>
        /// <para> L8A: OSS e IOSS IVA.</para>
        /// <para> L8B: Régimen especial de comerciante minorista.</para>
        /// <para> ('17')</para>
        /// </summary>
        [XmlEnum("17")]
        IossRegEspMin = 17,

        /// <summary>
        /// <para> L8A: Recargo de equivalencia.</para>
        /// <para> L8B: Régimen especial del pequeño empresario o profesional.</para>
        /// <para> ('18')</para>
        /// </summary>
        [XmlEnum("18")]
        RecEquivPeqEmp = 18,

        /// <summary>
        /// <para> L8A: Operaciones de actividades incluidas en el Régimen Especial de Agricultura, Ganadería y Pesca (REAGYP).</para>
        /// <para> L8B: Operaciones interiores exentas por aplicación artículo 25 Ley 19/1994.</para>
        /// <para> ('19')</para>
        /// </summary>
        [XmlEnum("19")]
        RegimenEspecialAgriculturaArt25Ley19_1994 = 19,

        /// <summary>
        /// Régimen simplificado sólo para IVA ('20').
        /// </summary>
        [XmlEnum("20")]
        RegimenSimplificado = 20

    }

}