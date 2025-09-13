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

using System.Collections.Generic;
using System.Xml.Serialization;
using VeriFactu.Xml.Factu.Alta;

namespace VeriFactu.Xml.Factu.Consulta.Respuesta
{

    /// <summary>
    /// Datos de un registro de facturación en la AEAT.
    /// </summary>
    public class DatosRegistroFacturacion
    {

        #region Propiedades Públicas de Instancia  

        /// <summary>
        /// <para>Dato adicional de contenido libre con el objetivo de que se pueda
        /// asociar opcionalmente información interna del sistema informático de facturación
        /// al registro de facturación. Este dato puede ayudar a completar la identificación
        /// o calificación de la factura y/o su registro de facturación.</para>
        /// <para>Alfanumérico (60)</para>
        /// </summary>
        [XmlElement(Namespace = Namespaces.NamespaceTikLRRC)]
        public string RefExterna { get; set; }

        /// <summary>
        /// Indica si se trata de un registro de subsanación
        /// cuando su valor es 'S'.
        /// </summary>
        [XmlElement(Namespace = Namespaces.NamespaceTikLRRC)]
        public string Subsanacion { get; set; }

        /// <summary>
        /// Indica si existe un rechazo previo del registro
        /// por parte de la AEAT.
        /// <para>'N': No ha habido rechazo previo por la AEAT.</para>
        /// <para>'S': Ha habido rechazo previo por la AEAT.</para>
        /// <para>'X': Independientemente de si ha habido o no algún
        /// rechazo previo por la AEAT, el registro de facturación no
        /// existe en la AEAT (registro existente en ese sistema informático
        /// o en algún sistema informático del obligado tributario y que no
        /// se remitió a la AEAT, por ejemplo, al acogerse a la modalidad
        /// «VERI*FACTU» desde la modalidad «NO VERI*FACTU»).</para>
        /// </summary>
        [XmlElement(Namespace = Namespaces.NamespaceTikLRRC)]
        public string RechazoPrevio { get; set; }

        /// <summary>
        /// <para>Clave del tipo de factura (L2).</para>
        /// </summary>
        [XmlElement(Namespace = Namespaces.NamespaceTikLRRC)]
        public TipoFactura TipoFactura { get; set; }

        /// <summary>
        ///  Identifica si el tipo de factura rectificativa
        ///  es por sustitución o por diferencia (L3).
        /// </summary>
        [XmlElement("TipoRectificativa", Namespace = Namespaces.NamespaceTikLRRC)]
        public TipoRectificativa TipoRectificativa { get; set; }

        /// <summary>
        ///  Con true se serializa el dato, con false no.
        /// </summary>
        [XmlIgnore]
        public bool TipoRectificativaSpecified { get; set; }

        /// <summary>
        /// <para>Descripción del objeto de la factura.</para>
        /// <para>Alfanumérico (500)</para>
        /// </summary>
        [XmlElement(Namespace = Namespaces.NamespaceTikLRRC)]
        public string DescripcionOperacion { get; set; }

        /// <summary>
        /// Destinatarios de la factura.
        /// </summary>
        [XmlArray("Destinatarios", Namespace = Namespaces.NamespaceTikLRRC)]
        [XmlArrayItem("IDDestinatario", Namespace = Namespaces.NamespaceTikLRRC)]
        public List<Destinatario> Destinatarios { get; set; }

        /// <summary>
        /// Líneas de desglose de factura.
        /// </summary>
        [XmlArray("Desglose", Namespace = Namespaces.NamespaceTikLRRC)]
        [XmlArrayItem("DetalleDesglose", Namespace = Namespaces.NamespaceSF)]
        public List<DetalleDesglose> Desglose { get; set; }

        /// <summary>
        /// <para>Importe total de la cuota (sumatorio de la Cuota Repercutida
        /// y Cuota de Recargo de Equivalencia).</para>
        /// <para>Decimal(12,2).</para>
        /// </summary>
        [XmlElement("CuotaTotal", Namespace = Namespaces.NamespaceTikLRRC)]
        public string CuotaTotal { get; set; }

        /// <summary>
        /// <para>Importe total de la factura.</para>
        /// <para>Decimal(12,2).</para>
        /// </summary>
        [XmlElement("ImporteTotal", Namespace = Namespaces.NamespaceTikLRRC)]
        public string ImporteTotal { get; set; }

        /// <summary>
        /// Encadenamiento con la factura anterior..
        /// </summary>
        [XmlElement("Encadenamiento", Namespace = Namespaces.NamespaceTikLRRC)]
        public Encadenamiento Encadenamiento { get; set; }

        /// <summary>
        /// <para>Fecha, hora y huso horario de generación del registro de facturación.
        /// El huso horario es el que está usando el sistema informático de facturación
        /// en el momento de generación del registro de facturación.</para>
        /// <para>DateTime. Formato: YYYY-MM-DDThh:mm:ssTZD (ej: 2024-01-01T19:20:30+01:00) (ISO 8601)</para>
        /// </summary>
        [XmlElement(Namespace = Namespaces.NamespaceTikLRRC)]
        public string FechaHoraHusoGenRegistro { get; set; }

        /// <summary>
        ///  Tipo de algoritmo aplicado a cierto contenido del registro
        ///  de facturación para obtener la huella o «hash».
        /// <para>Alfanumérico (2) L12</para> 
        /// </summary>
        [XmlElement(Namespace = Namespaces.NamespaceTikLRRC)]
        public string TipoHuella { get; set; }

        /// <summary>
        /// <para>Huella o «hash» de cierto contenido de este registro
        /// de facturación. Dicho contenido se detallará en la documentación
        /// correspondiente en la sede electrónica de la AEAT (documento de huella...).</para>
        /// <para> El Artículo 137 de la Orden HAC/1177/2024 de 17 de octubre establece el contenido:</para>
        /// <para> a) Para el registro de facturación de alta:</para>
        /// <para> 1.º NIF del emisor.</para>
        /// <para> 2.º Numero de factura y serie.</para>
        /// <para> 3.º Fecha de expedición de la factura.</para>
        /// <para> 4.º Tipo de factura.</para>
        /// <para> 5.º Cuota total.</para>
        /// <para> 6.º Importe total.</para>
        /// <para> 7.º Huella del registro de facturación anterior.</para>
        /// <para> 8.º Fecha, hora y huso horario de generación del registro.</para>
        /// <para>Alfanumérico (64)</para>
        /// </summary>
        [XmlElement(Namespace = Namespaces.NamespaceTikLRRC)]
        public string Huella { get; set; }

        /// <summary>
        /// Indica si se trata de un registro envíado tras
        /// una incidencia técnica cuando su valor es 'S'.
        /// </summary>
        [XmlElement(Namespace = Namespaces.NamespaceTikLRRC)]
        public string Incidencia { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {
            return $"{TipoFactura}-{RefExterna}";
        }

        #endregion

    }

}