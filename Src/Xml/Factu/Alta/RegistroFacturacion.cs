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

using System.Collections.Generic;
using System.Xml.Serialization;

namespace VeriFactu.Xml.Factu.Alta
{

    /// <summary>
    /// Datos correspondientes al registro de facturacion de alta.
    /// </summary>
    public class RegistroFacturacion
    {

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Período al que corresponden los apuntes. 
        /// Todos los apuntes deben corresponder al mismo período impositivo.
        /// </summary>
        public PeriodoLiquidacion PeriodoLiquidacion { get; set; }

        /// <summary>
        /// Datos de identificación de factura expedida para
        /// operaciones de baja y consulta.
        /// </summary>
        public IDFactura IDFactura { get; set; }

        /// <summary>
        /// <para>Clave del tipo de factura (L2).</para>
        /// </summary>
        public TipoFactura TipoFactura { get; set; }

        /// <summary>
        ///  Identifica si el tipo de factura rectificativa
        ///  es por sustitución o por diferencia (L3).
        /// </summary>
        public TipoRectificativa TipoRectificativa { get; set; }

        /// <summary>
        /// El ID de las facturas rectificadas, únicamente se rellena
        /// en el caso de rectificación de facturas.
        /// </summary>
        public List<IDFactura> FacturasRectificadas { get; set; }

        /// <summary>
        /// El ID de las facturas sustituidas, únicamente se rellena
        /// en el caso de facturas sustituidas.
        /// </summary>
        public List<IDFactura> FacturasSustituidas { get; set; }

        /// <summary>
        /// Información importes rectificados.
        /// </summary>
        public ImporteRectificacion ImporteRectificacion { get; set; }

        /// <summary>
        /// <para>Fecha en la que se ha realizado la operación siempre
        /// que sea diferente a la fecha de expedición.</para>
        /// <para>Fecha(dd-mm-yyyy)</para>
        /// </summary>
        public string FechaOperacion { get; set; }

        /// <summary>
        /// <para>Número de registro obtenido al enviar la autorización
        /// en materia de facturación o de libros registro</para>
        /// <para>Alfanumérico(15)</para>
        /// </summary>
        public string NumRegistroAcuerdoFacturacion { get; set; }

        /// <summary>
        /// <para>Identificación de la autorización a que se
        /// refiere el artículo 5 del RD XX72022.</para>
        /// <para>Alfanumérico(15).</para>
        /// </summary>
        public string NumRegistroAcuerdoSistemaInformatico { get; set; }

        /// <summary>
        /// <para>Descripción del objeto de la factura.</para>
        /// <para>Alfanumérico(500).</para>
        /// </summary>
        public string DescripcionOperacion { get; set; }

        /// <summary>
        /// <para>Referencia Externa. Dato adicional de contenido libre.</para>
        /// <para>Alfanumérico(60)</para>
        /// </summary>
        public string RefExterna { get; set; }

        /// <summary>
        /// <para>Factura simplificada Articulo 7,2 Y 7,3 RD 1619/2012.
        /// Si no se informa este campo se entenderá que tiene valor 'N'.</para>
        /// <para>L4</para>
        /// </summary>
        [XmlElement(ElementName = "FacturaSimplificadaArticulos7.2_7.3")]
        public FacturaSimplificadaArticulos7_2_7_3 FacturaSimplificadaArticulos7_2_7_3 { get; set; }

        /// <summary>
        /// <para>Factura sin identificación destinatario artículo 6,1,d)
        /// RD 1616/2012. Si no se informa este campo se entenderá que
        /// tiene valor 'N'.</para>
        /// <para>L5</para>
        /// </summary>
        [XmlElement(ElementName = "FacturaSinIdentifDestinatarioArticulo6.1.d")]
        public FacturaSinIdentifDestinatarioArticulo6_1_d FacturaSinIdentifDestinatarioArticulo6_1_d { get; set; }

        /// <summary>
        /// <para>Identificador que especifica aquellas facturas con base
        /// o importe de la factura superior al umbral especificado.
        /// Si no se informa este campo se entenderá que tiene valor 'N'.</para>
        /// <para>Alfanumérico(1).</para>
        /// </summary>
        public string Macrodato { get; set; }

        /// <summary>
        /// <para>Identificador que especifica si la factura ha
        /// sido emitida por un tercero o por el destinatario. </para>
        /// <para>Alfanumérico(1). L6.</para>
        /// </summary>
        public EmitidaPorTercerosODestinatario EmitidaPorTercerosODestinatario { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {
            return $"{TipoFactura}-{IDFactura}";
        }

        #endregion

    }

}
