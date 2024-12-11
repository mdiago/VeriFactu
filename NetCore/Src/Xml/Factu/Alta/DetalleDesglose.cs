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
    /// Línea de desglose de la factura.
    /// </summary>
    public class DetalleDesglose
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        public DetalleDesglose()
        {

            ClaveRegimenSpecified = true;

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// <para>Impuesto de aplicación. Si no se informa este campo
        /// se entenderá que el impuesto de aplicación es el IVA.
        /// Este campo es necesario porque contribuye a completar el
        /// detalle de la tipología de la factura.</para>
        /// <para>Alfanumérico (1) L1:</para>
        /// <para>01: Impuesto sobre el Valor Añadido (IVA)</para>
        /// <para>02: Impuesto sobre la Producción, los Servicios y la Importación (IPSI) de Ceuta y Melilla</para>
        /// <para>03: Impuesto General Indirecto Canario (IGIC)</para>
        /// <para>05: Otros</para>
        /// </summary>
        public Impuesto Impuesto { get; set; }

        /// <summary>
        /// <para>Clave que identificará el tipo de régimen del
        /// impuesto o una operación con trascendencia tributaria.</para>
        /// <para>Alfanumérico (2) L8A/L8B.</para>
        /// <para>L8A y L8B DESCRIPCIÓN DE LA CLAVE DE RÉGIMEN PARA DESGLOSES DONDE EL IMPUESTO DE APLICACIÓN ES EL IVA / IGIC: </para>
        /// <para>01: Operación de régimen general.</para>
        /// <para>02: Exportación.</para>
        /// <para>03: Operaciones a las que se aplique el régimen especial de bienes usados, objetos de arte, antigüedades y objetos de colección.</para>
        /// <para>04: Régimen especial del oro de inversión.</para>
        /// <para>05: Régimen especial de las agencias de viajes.</para>
        /// <para>06: Régimen especial grupo de entidades en IVA / IGIC (Nivel Avanzado).</para>
        /// <para>07: Régimen especial del criterio de caja.</para>
        /// <para>08: Operaciones sujetas al IPSI  / IGIC (Impuesto sobre la Producción, los Servicios y la Importación  / Impuesto General Indirecto Canario).</para>
        /// <para>09: Facturación de las prestaciones de servicios de agencias de viaje que actúan como mediadoras en nombre y por cuenta ajena (D.A.4ª RD1619/2012).</para>
        /// <para>10: Cobros por cuenta de terceros de honorarios profesionales o de derechos derivados de la propiedad industrial, de autor u otros por cuenta de sus socios,
        /// asociados o colegiados efectuados por sociedades, asociaciones, colegios profesionales u otras entidades que realicen estas funciones de cobro.</para>
        /// <para>11: Operaciones de arrendamiento de local de negocio.</para>
        /// <para>14: Factura con IVA / IGIC pendiente de devengo en certificaciones de obra cuyo destinatario sea una Administración Pública.</para>
        /// <para>15: Factura con IVA / IGIC pendiente de devengo en operaciones de tracto sucesivo.</para>
        /// <para>17: L8A: Operación acogida a alguno de los regímenes previstos en el Capítulo XI del Título IX (OSS e IOSS).</para>
        /// <para>17: L8B: Régimen especial de comerciante minorista.</para>
        /// <para>18: L8A: Recargo de equivalencia.</para>
        /// <para>18: L8B: Régimen especial del pequeño empresario o profesional.</para>
        /// <para>19: L8A: Operaciones de actividades incluidas en el Régimen Especial de Agricultura, Ganadería y Pesca (REAGYP).</para>
        /// <para>19: L8B: Operaciones interiores exentas por aplicación artículo 25 Ley 19/1994.</para>
        /// <para>20: Régimen simplificado IVA. Régimen simplificado sólo para IVA.</para>
        /// </summary>
        public ClaveRegimen ClaveRegimen { get; set; }

        /// <summary>
        ///  Con true se serializa el dato, con false no.
        /// </summary>
        [XmlIgnore]
        public bool ClaveRegimenSpecified { get; set; }

        /// <summary>
        /// <para>Clave de la operación sujeta y
        /// no exenta o de la operación no sujeta.</para>
        /// <para>Alfanumérico(2). L9.</para>
        /// </summary>
        public CalificacionOperacion CalificacionOperacion { get; set; }

        /// <summary>
        /// <para>Campo que especifica la causa de exención.</para>
        /// <para>Alfanumérico(2). L10.</para>
        /// </summary>
        public CausaExencion OperacionExenta { get; set; }

        /// <summary>
        ///  Con true se serializa el dato, con false no.
        /// </summary>
        [XmlIgnore]
        public bool OperacionExentaSpecified { get; set; }

        /// <summary>
        /// <para>Porcentaje aplicado sobre la base
        /// imponible para calcular la cuota.</para>
        /// <para>Decimal(3,2).</para>
        /// </summary>
        public string TipoImpositivo { get; set; }

        /// <summary>
        /// <para>Magnitud dineraria sobre la que se
        /// aplica el tipo impositivo / Importe no sujeto.</para>
        /// <para>Decimal(12,2).</para>
        /// </summary>
        public string BaseImponibleOimporteNoSujeto { get; set; }

        /// <summary>
        /// <para>Magnitud dineraria sobre  la que se aplica
        /// el tipo impositivo en régimen especial de grupos nivel avanzado.</para>
        /// <para>Decimal(12,2).</para>
        /// </summary>
        public string BaseImponibleACoste { get; set; }

        /// <summary>
        /// <para>Cuota resultante de aplicar a la base
        /// imponible el tipo impositivo.</para>
        /// <para>Decimal(12,2).</para>
        /// </summary>
        public string CuotaRepercutida { get; set; }

        /// <summary>
        /// <para>Porcentaje asociado en función del impuesto y tipo impositivo.</para>
        /// <para>Decimal(3,2).</para>
        /// </summary>
        public string TipoRecargoEquivalencia { get; set; }

        /// <summary>
        /// <para>Cuota resultante de aplicar a la base
        /// imponible el tipo de recargo de equivalencia.</para>
        /// <para>Decimal(12,2).</para>
        /// </summary>
        public string CuotaRecargoEquivalencia { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representacioón textual de la instancia.
        /// </summary>
        /// <returns>Representacioón textual de la instancia.</returns>
        public override string ToString()
        {

            return $"[{ClaveRegimen}, {CalificacionOperacion}]" +
                $" {BaseImponibleOimporteNoSujeto} x {TipoImpositivo}" +
                $" = {CuotaRepercutida}";

        }

        #endregion

    }

}
