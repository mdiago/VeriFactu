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
    public class RegistroAlta : Registro
    { 

        #region Métodos Privados de Instancia

        /// <summary>
        /// Devuelve la cadena de entrada para el cálculo
        /// del hash previa conversión mediante UTF-8.
        /// </summary>
        /// <returns></returns>
        protected override string GetHashTextInput()
        {

            return $"IDEmisorFactura={IDFactura?.IDEmisorFactura}" +
                $"&NumSerieFactura={IDFactura?.NumSerieFactura}" +
                $"&FechaExpedicionFactura={IDFactura?.FechaExpedicionFactura}" +
                $"&TipoFactura={TipoFactura}" +
                $"&CuotaTotal={CuotaTotal}" +
                $"&ImporteTotal={ImporteTotal}" +
                $"&Huella={Encadenamiento?.RegistroAnterior?.Huella}" +
                $"&FechaHoraHusoGenRegistro={FechaHoraHusoGenRegistro}";

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// <para>Identificación de la versión.</para>
        /// <para>Alfanumérico(3)L15</para>
        /// </summary>
        [XmlElement("IDVersion")]
        public string IDVersion { get; set; }

        /// <summary>
        /// Datos de identificación de factura expedida para
        /// operaciones de baja y consulta.
        /// </summary>
        [XmlElement("IDFactura", Namespace = Namespaces.NamespaceSFLR)]
        public IDFactura IDFactura { get; set; }

        /// <summary>
        /// <para>Nombre-razón social del obligado a expedir la factura.</para>
        /// <para>Alfanumérico (120)</para>
        /// </summary>
        [XmlElement("NombreRazonEmisor", Namespace = Namespaces.NamespaceSFLR)]
        public string NombreRazonEmisor { get; set; }

        /// <summary>
        /// <para>Indicador que especifica que se trata de una subsanación
        /// de un registro de facturación de alta previamente generado,
        /// por lo que el contenido de este nuevo registro de facturación
        /// es el correcto y el que deberá tenerse en cuenta.
        /// Si no se informa este campo se entenderá que tiene valor "N" (Alta Normal-Inicial).
        /// Este campo forma parte del detalle de las circunstancias de generación de los
        /// registros de facturación.</para>
        /// <para>Alfanumérico (1) L4</para>
        /// </summary>
        public string Subsanacion { get; set; }

        /// <summary>
        /// <para>Indicador que especifica que se está generando -para volverlo a remitir-
        /// un nuevo registro de facturación de alta subsanado tras haber sido rechazado
        /// en su remisión inmediatamente anterior, es decir, en el último envío que contenía
        /// ese registro de facturación de alta rechazado. Si no se informa este campo se
        /// entenderá que tiene valor "N". Solo es necesario informarlo en caso de remisión
        /// voluntaria «VERI*FACTU». Este campo forma parte del detalle de las circunstancias
        /// de generación de los registros de facturación.</para>
        /// <para>Alfanumérico (1) L17</para>
        /// </summary>
        public RechazoPrevio RechazoPrevio { get; set; }

        /// <summary>
        ///  Con true se serializa el dato, con false no.
        /// </summary>
        [XmlIgnore]
        public bool RechazoPrevioSpecified { get; set; }

        /// <summary>
        /// <para>Clave del tipo de factura (L2).</para>
        /// </summary>
        [XmlElement("TipoFactura", Namespace = Namespaces.NamespaceSFLR)]
        public TipoFactura TipoFactura { get; set; }

        /// <summary>
        ///  Con true se serializa el dato, con false no.
        /// </summary>
        [XmlIgnore]
        public bool TipoFacturaSpecified { get; set; }

        /// <summary>
        ///  Identifica si el tipo de factura rectificativa
        ///  es por sustitución o por diferencia (L3).
        /// </summary>
        [XmlElement("TipoRectificativa", Namespace = Namespaces.NamespaceSFLR)]
        public TipoRectificativa TipoRectificativa { get; set; }

        /// <summary>
        ///  Con true se serializa el dato, con false no.
        /// </summary>
        [XmlIgnore]
        public bool TipoRectificativaSpecified { get; set; }

        /// <summary>
        /// El ID de las facturas rectificadas, únicamente se rellena
        /// en el caso de rectificación de facturas.
        /// </summary>
        [XmlElement("FacturasRectificadas", Namespace = Namespaces.NamespaceSFLR)]
        public List<IDFactura> FacturasRectificadas { get; set; }

        /// <summary>
        /// El ID de las facturas sustituidas, únicamente se rellena
        /// en el caso de facturas sustituidas.
        /// </summary>
        [XmlElement("FacturasSustituidas", Namespace = Namespaces.NamespaceSFLR)]
        public List<IDFactura> FacturasSustituidas { get; set; }

        /// <summary>
        /// Información importes rectificados.
        /// </summary>
        [XmlElement("ImporteRectificacion", Namespace = Namespaces.NamespaceSFLR)]
        public ImporteRectificacion ImporteRectificacion { get; set; }

        /// <summary>
        /// <para>Fecha en la que se ha realizado la operación siempre
        /// que sea diferente a la fecha de expedición.</para>
        /// <para>Fecha(dd-mm-yyyy)</para>
        /// </summary>
        [XmlElement("FechaOperacion", Namespace = Namespaces.NamespaceSFLR)]
        public string FechaOperacion { get; set; }

        /// <summary>
        /// <para>Descripción del objeto de la factura.</para>
        /// <para>Alfanumérico (500)</para>
        /// </summary>
        [XmlElement("DescripcionOperacion", Namespace = Namespaces.NamespaceSFLR)]
        public string DescripcionOperacion { get; set; }

        /// <summary>
        /// <para>Factura simplificada Articulo 7.2 Y 7.3 RD 1619/2012. 
        /// Si no se informa este campo se entenderá que tiene valor  “N".</para>
        /// <para>Alfunumérico (1) L4</para>
        /// </summary>
        public string FacturaSimplificadaArt7273 { get; set; }

        /// <summary>
        /// <para>Factura simplificada Articulo 7.2 Y 7.3 RD 1619/2012. 
        /// Si no se informa este campo se entenderá que tiene valor  “N".</para>
        /// <para>Alfunumérico (1) L4</para>
        /// </summary>
        public string FacturaSinIdentifDestinatarioArt61d { get; set; }

        /// <summary>
        /// <para>Identificador que especifica aquellas facturas con base o
        /// importe de la factura superior al umbral especificado.
        /// Si no se informa este campo se entenderá que tiene valor  “N”..</para>
        /// <para>Alfanumérico(1).</para>
        /// </summary>
        [XmlElement("Macrodato", Namespace = Namespaces.NamespaceSFLR)]
        public string Macrodato { get; set; }

        /// <summary>
        /// <para>Identificador que especifica si la factura ha
        /// sido emitida por un tercero o por el destinatario. </para>
        /// <para>Alfanumérico(1). L6.</para>
        /// </summary>
        [XmlElement("EmitidaPorTercerosODestinatario", Namespace = Namespaces.NamespaceSFLR)]
        public EmitidaPorTercerosODestinatario EmitidaPorTercerosODestinatario { get; set; }

        /// <summary>
        ///  Con true se serializa el dato, con false no.
        /// </summary>
        [XmlIgnore]
        public bool EmitidaPorTercerosODestinatarioSpecified { get; set; }

        /// <summary>
        /// Tercero que expide la factura.
        /// </summary>
        [XmlElement("Tercero", Namespace = Namespaces.NamespaceSFLR)]
        public Interlocutor Tercero { get; set; }

        /// <summary>
        /// Destinatarios de la factura.
        /// </summary>
        [XmlArray("Destinatarios", Namespace = Namespaces.NamespaceSFLR)]
        [XmlArrayItem("IDDestinatario", Namespace = Namespaces.NamespaceSFLR)]
        public List<Interlocutor> Destinatarios { get; set; }

        /// <summary>
        /// <para>Identificador que especifica si tiene minoración de la
        /// base imponible por la concesión de cupones, bonificaciones o
        /// descuentos cuando solo se expide el original de la factura.
        /// Este campo es necesario porque contribuye a completar el detalle
        /// de la tipología de la factura. Si no se informa este campo se
        /// entenderá que tiene valor  “N”.</para>
        /// <para>Alfanumérico (1) L11</para>
        /// </summary>
        [XmlElement("Cupon", Namespace = Namespaces.NamespaceSFLR)]
        public string Cupon { get; set; }

        /// <summary>
        /// Desglose de la factura.
        /// </summary>
        [XmlElement("Desglose", Namespace = Namespaces.NamespaceSFLR)]
        public Desglose Desglose { get; set; }

        /// <summary>
        /// <para>Importe total de la cuota (sumatorio de la Cuota Repercutida
        /// y Cuota de Recargo de Equivalencia).</para>
        /// <para>Decimal(12,2).</para>
        /// </summary>
        public string CuotaTotal { get; set; }

        /// <summary>
        /// <para>Importe total de la factura.</para>
        /// <para>Decimal(12,2).</para>
        /// </summary>
        [XmlElement("ImporteTotal", Namespace = Namespaces.NamespaceSFLR)]
        public string ImporteTotal { get; set; }

        /// <summary>
        /// Encadenamiento con la factura anterior..
        /// </summary>
        public Encadenamiento Encadenamiento { get; set; }

        /// <summary>
        ///  Información del sistema informático.
        /// </summary>
        [XmlElement("SistemaInformatico", Namespace = Namespaces.NamespaceSFLR)]
        public SistemaInformatico SistemaInformatico { get; set; }

        /// <summary>
        /// <para>Fecha, hora y huso horario de generación del registro de facturación.
        /// El huso horario es el que está usando el sistema informático de facturación
        /// en el momento de generación del registro de facturación.</para>
        /// <para>DateTime. Formato: YYYY-MM-DDThh:mm:ssTZD (ej: 2024-01-01T19:20:30+01:00) (ISO 8601)</para>
        /// </summary>
        public string FechaHoraHusoGenRegistro { get; set; }

        /// <summary>
        /// <para>Número de registro obtenido al enviar la autorización
        /// en materia de facturación o de libros registro</para>
        /// <para>Alfanumérico(15)</para>
        /// </summary>
        [XmlElement("NumRegistroAcuerdoFacturacion", Namespace = Namespaces.NamespaceSFLR)]
        public string NumRegistroAcuerdoFacturacion { get; set; }

        /// <summary>
        /// <para>Identificación del acuerdo (resolución) a que se refiere
        /// el artículo 5 del Reglamento. Este campo forma parte del detalle
        /// de las circunstancias de generación del registro de facturación.</para>
        /// <para>Alfanumérico (15)</para>
        /// </summary>
        [XmlElement("IdAcuerdoSistemaInformatico", Namespace = Namespaces.NamespaceSFLR)]
        public string IdAcuerdoSistemaInformatico { get; set; }

        /// <summary>
        ///  Tipo de algoritmo aplicado a cierto contenido del registro
        ///  de facturación para obtener la huella o «hash».
        /// <para>Alfanumérico (2) L12</para> 
        /// </summary>
        public TipoHuella TipoHuella { get; set; }

        /// <summary>
        ///  Con true se serializa el dato, con false no.
        /// </summary>
        [XmlIgnore]
        public bool TipoHuellaSpecified { get; set; }

        /// <summary>
        /// <para>Huella o «hash» de cierto contenido de este registro
        /// de facturación. Dicho contenido se detallará en la documentación
        /// correspondiente en la sede electrónica de la AEAT (documento de huella...).</para>
        /// <para>Alfanumérico (64)</para>
        /// </summary>
        public string Huella { get; set; }

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
