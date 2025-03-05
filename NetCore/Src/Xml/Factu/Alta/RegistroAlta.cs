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
using System.Web;
using System.Xml.Serialization;

namespace VeriFactu.Xml.Factu.Alta
{

    /// <summary>
    /// Datos correspondientes al registro de facturacion de alta.
    /// </summary>
    public class RegistroAlta : Registro
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Hash del registro.
        /// </summary>
        string _Huella;

        /// <summary>
        /// Momento generación registro.
        /// </summary>
        string _FechaHoraHusoGenRegistro;

        /// <summary>
        /// Encadenamiento con el registro anterior.
        /// </summary>
        Encadenamiento _Encadenamiento;

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Devuelve la cadena de entrada para el cálculo
        /// del hash previa conversión mediante UTF-8.
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
        /// </summary>
        /// <returns>Cadena de entrada para el cálculo
        /// del hash</returns>
        internal protected override string GetHashTextInput()
        {

            IDFactura = IDFacturaAlta;
                                                                                        // Artículo 137.a de la Orden HAC/1177/2024 de 17 de octubre:
            return $"IDEmisorFactura={IDFacturaAlta?.IDEmisorFactura}" +                // 1.º NIF del emisor.
                $"&NumSerieFactura={IDFacturaAlta?.NumSerieFactura}" +                  // 2.º Numero de factura y serie.
                $"&FechaExpedicionFactura={IDFacturaAlta?.FechaExpedicionFactura}" +    // 3.º Fecha de expedición de la factura.
                $"&TipoFactura={TipoFactura}" +                                         // 4.º Tipo de factura.
                $"&CuotaTotal={CuotaTotal}" +                                           // 5.º Cuota total.
                $"&ImporteTotal={ImporteTotal}" +                                       // 6.º Importe total.
                $"&Huella={Encadenamiento?.RegistroAnterior?.Huella}" +                 // 7.º Huella del registro de facturación anterior.
                $"&FechaHoraHusoGenRegistro={FechaHoraHusoGenRegistro}";                // 8.º Fecha, hora y huso horario de generación del registro.

        }

        /// <summary>
        /// Devuelve la cadena de parámetros para la url
        /// del servicio de validación con los valores
        /// de los parámetro urlencoded.
        /// </summary>
        /// <returns>Cadena de parámetros para la url
        /// del servicio de validación con los valores
        /// de los parámetro urlencoded.</returns>
        protected override string GetValidateUrlParams()
        {

            var nif = HttpUtility.UrlEncode($"{IDFacturaAlta?.IDEmisorFactura}");
            var numserie = HttpUtility.UrlEncode($"{IDFacturaAlta?.NumSerieFactura}");
            var fecha = HttpUtility.UrlEncode($"{IDFacturaAlta?.FechaExpedicionFactura}");
            var importe = HttpUtility.UrlEncode($"{ImporteTotal}");

            return $"nif={nif}&numserie={numserie}&fecha={fecha}&importe={importe}";

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// <para>Identificación de la versión actual del esquema o
        /// estructura de información utilizada para la generación y
        /// conservación / remisión de los registros de facturación.
        /// Este campo forma parte del detalle de las circunstancias
        /// de generación de los registros de facturación.</para>
        /// <para>Alfanumérico(3) L15:</para>
        /// <para>1.0: Versión actual (1.0) del esquema utilizado </para>
        /// </summary>
        [XmlElement("IDVersion", Namespace = Namespaces.NamespaceSF, Order = 1)]
        public override string IDVersion { get; set; }

        /// <summary>
        /// Datos de identificación de factura expedida para
        /// operaciones de baja y consulta. Contiene del NIF del obligado
        /// a expedir la factura, el número de factura y la fecha.
        /// </summary>
        [XmlElement("IDFactura", Namespace = Namespaces.NamespaceSF, Order = 2)]
        public IDFactura IDFacturaAlta { get; set; }

        /// <summary>
        /// <para>Dato adicional de contenido libre con el objetivo de que se pueda
        /// asociar opcionalmente información interna del sistema informático de facturación
        /// al registro de facturación. Este dato puede ayudar a completar la identificación
        /// o calificación de la factura y/o su registro de facturación.</para>
        /// <para>Alfanumérico (60)</para>
        /// </summary>
        [XmlElement("RefExterna", Namespace = Namespaces.NamespaceSF, Order = 3)]
        public string RefExterna { get; set; }

        /// <summary>
        /// <para>Nombre-razón social del obligado a expedir la factura.</para>
        /// <para>Alfanumérico (120)</para>
        /// </summary>
        [XmlElement("NombreRazonEmisor", Namespace = Namespaces.NamespaceSF, Order = 4)]
        public string NombreRazonEmisor { get; set; }

        /// <summary>
        /// <para>Indicador que especifica que se trata de una subsanación
        /// de un registro de facturación de alta previamente generado,
        /// por lo que el contenido de este nuevo registro de facturación
        /// es el correcto y el que deberá tenerse en cuenta.
        /// Si no se informa este campo se entenderá que tiene valor "N" (Alta Normal-Inicial).
        /// Este campo forma parte del detalle de las circunstancias de generación de los
        /// registros de facturación.</para>
        /// <para>Alfanumérico (1) L4:</para>
        /// <para>S: Sí</para>
        /// <para>N: No</para>
        /// </summary>
        [XmlElement(Order = 5)]
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
        [XmlElement(Order = 6)]
        public RechazoPrevio RechazoPrevio { get; set; }

        /// <summary>
        /// Con true se serializa el dato, con false no.
        /// </summary>
        [XmlIgnore]
        public bool RechazoPrevioSpecified { get; set; }

        /// <summary>
        /// <para>Clave del tipo de factura (L2).</para>
        /// </summary>
        [XmlElement("TipoFactura", Namespace = Namespaces.NamespaceSF, Order = 7)]
        public TipoFactura TipoFactura { get; set; }

        /// <summary>
        ///  Identifica si el tipo de factura rectificativa
        ///  es por sustitución o por diferencia (L3).
        /// </summary>
        [XmlElement("TipoRectificativa", Namespace = Namespaces.NamespaceSF, Order = 8)]
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
        [XmlArray("FacturasRectificadas", Namespace = Namespaces.NamespaceSF, Order = 9)]
        [XmlArrayItem(ElementName = "IDFacturaRectificada")]
        public IDFactura[] FacturasRectificadas { get; set; }

        /// <summary>
        /// El ID de las facturas sustituidas, únicamente se rellena
        /// en el caso de facturas sustituidas.
        /// </summary>
        [XmlArray("FacturasSustituidas", Namespace = Namespaces.NamespaceSF, Order = 10)]
        [XmlArrayItem(ElementName = "IDFacturaSustituida")]
        public IDFactura[] FacturasSustituidas { get; set; }

        /// <summary>
        /// Información importes rectificados.
        /// </summary>
        [XmlElement("ImporteRectificacion", Namespace = Namespaces.NamespaceSF, Order = 11)]
        public ImporteRectificacion ImporteRectificacion { get; set; }

        /// <summary>
        /// <para>Fecha en la que se ha realizado la operación siempre
        /// que sea diferente a la fecha de expedición.</para>
        /// <para>Fecha(dd-mm-yyyy)</para>
        /// </summary>
        [XmlElement("FechaOperacion", Namespace = Namespaces.NamespaceSF, Order = 12)]
        public string FechaOperacion { get; set; }

        /// <summary>
        /// <para>Descripción del objeto de la factura.</para>
        /// <para>Alfanumérico (500)</para>
        /// </summary>
        [XmlElement("DescripcionOperacion", Namespace = Namespaces.NamespaceSF, Order = 13)]
        public string DescripcionOperacion { get; set; }

        /// <summary>
        /// <para>Factura simplificada Articulo 7.2 Y 7.3 RD 1619/2012. 
        /// Si no se informa este campo se entenderá que tiene valor  “N".</para>
        /// <para>Alfanumérico (1) L4:</para>
        /// <para>S: Sí</para>
        /// <para>N: No</para>
        /// </summary>
        [XmlElement(Order = 14)]
        public string FacturaSimplificadaArt7273 { get; set; }

        /// <summary>
        /// <para>Factura simplificada Articulo 7.2 Y 7.3 RD 1619/2012. 
        /// Si no se informa este campo se entenderá que tiene valor  “N".</para>
        /// <para>Alfanumérico (1) L5:</para>
        /// <para>S: Sí</para>
        /// <para>N: No</para>
        /// </summary>
        [XmlElement(Order = 15)]
        public string FacturaSinIdentifDestinatarioArt61d { get; set; }

        /// <summary>
        /// <para>Identificador que especifica aquellas facturas con base
        /// o importe de la factura superior al umbral especificado.
        /// Este campo es necesario porque contribuye a completar el
        /// detalle de la tipología de la factura.
        /// Si no se informa este campo se entenderá que tiene valor “N”.</para>
        /// <para>Alfanumérico (1) L4:</para>
        /// <para>S: Sí</para>
        /// <para>N: No</para>
        /// </summary>
        [XmlElement("Macrodato", Namespace = Namespaces.NamespaceSF, Order = 16)]
        public string Macrodato { get; set; }

        /// <summary>
        /// <para>Identificador que especifica si la factura ha
        /// sido emitida por un tercero o por el destinatario. </para>
        /// <para>Alfanumérico(1). L6.</para>
        /// </summary>
        [XmlElement("EmitidaPorTercerosODestinatario", Namespace = Namespaces.NamespaceSF, Order = 17)]
        public EmitidaPorTercerosODestinatario EmitidaPorTercerosODestinatario { get; set; }

        /// <summary>
        ///  Con true se serializa el dato, con false no.
        /// </summary>
        [XmlIgnore]
        public bool EmitidaPorTercerosODestinatarioSpecified { get; set; }

        /// <summary>
        /// Tercero que expide la factura.
        /// </summary>
        [XmlElement("Tercero", Namespace = Namespaces.NamespaceSF, Order = 18)]
        public Interlocutor Tercero { get; set; }

        /// <summary>
        /// Destinatarios de la factura.
        /// </summary>
        [XmlArray("Destinatarios", Namespace = Namespaces.NamespaceSF, Order = 19)]
        [XmlArrayItem("IDDestinatario", Namespace = Namespaces.NamespaceSF)]
        public List<Interlocutor> Destinatarios { get; set; }

        /// <summary>
        /// <para>Identificador que especifica si tiene minoración de la
        /// base imponible por la concesión de cupones, bonificaciones o
        /// descuentos cuando solo se expide el original de la factura.
        /// Este campo es necesario porque contribuye a completar el detalle
        /// de la tipología de la factura. Si no se informa este campo se
        /// entenderá que tiene valor  “N”.</para>
        /// <para>Alfanumérico (1) L4:</para>
        /// <para>S: Sí</para>
        /// <para>N: No</para>
        /// </summary>
        [XmlElement("Cupon", Namespace = Namespaces.NamespaceSF, Order = 20)]
        public string Cupon { get; set; }

        /// <summary>
        /// Líneas de desglose de factura.
        /// </summary>
        [XmlArray("Desglose", Namespace = Namespaces.NamespaceSF, Order = 21)]
        [XmlArrayItem("DetalleDesglose", Namespace = Namespaces.NamespaceSF)]
        public List<DetalleDesglose> Desglose { get; set; }

        /// <summary>
        /// <para>Importe total de la cuota (sumatorio de la Cuota Repercutida
        /// y Cuota de Recargo de Equivalencia).</para>
        /// <para>Decimal(12,2).</para>
        /// </summary>
        [XmlElement("CuotaTotal", Namespace = Namespaces.NamespaceSF, Order = 22)]
        public string CuotaTotal { get; set; }

        /// <summary>
        /// <para>Importe total de la factura.</para>
        /// <para>Decimal(12,2).</para>
        /// </summary>
        [XmlElement("ImporteTotal", Namespace = Namespaces.NamespaceSF, Order = 23)]
        public string ImporteTotal { get; set; }

        /// <summary>
        /// Encadenamiento con la factura anterior..
        /// </summary>
        [XmlIgnore]
        public override Encadenamiento Encadenamiento
        {
            get
            {
                return _Encadenamiento;
            }
            set
            {
                _Encadenamiento = value;
                OrderedEncadenamiento = _Encadenamiento;
            }
        }

        /// <summary>
        /// Encadenamiento con la factura anterior..
        /// </summary>
        [XmlElement("Encadenamiento", Namespace = Namespaces.NamespaceSF, Order = 24)]
        public Encadenamiento OrderedEncadenamiento { get; set; }

        /// <summary>
        ///  Información del sistema informático.
        /// </summary>
        [XmlElement("SistemaInformatico", Namespace = Namespaces.NamespaceSF, Order = 25)]
        public SistemaInformatico SistemaInformatico { get; set; }

        /// <summary>
        /// <para>Fecha, hora y huso horario de generación del registro de facturación.
        /// El huso horario es el que está usando el sistema informático de facturación
        /// en el momento de generación del registro de facturación.</para>
        /// <para>DateTime. Formato: YYYY-MM-DDThh:mm:ssTZD (ej: 2024-01-01T19:20:30+01:00) (ISO 8601)</para>
        /// </summary>
        [XmlIgnore]
        public override string FechaHoraHusoGenRegistro
        {
            get
            {
                return _FechaHoraHusoGenRegistro;
            }
            set
            {
                _FechaHoraHusoGenRegistro = value;
                OrderedFechaHoraHusoGenRegistro = _FechaHoraHusoGenRegistro;
            }
        }

        /// <summary>
        /// <para>Fecha, hora y huso horario de generación del registro de facturación.
        /// El huso horario es el que está usando el sistema informático de facturación
        /// en el momento de generación del registro de facturación.</para>
        /// <para>DateTime. Formato: YYYY-MM-DDThh:mm:ssTZD (ej: 2024-01-01T19:20:30+01:00) (ISO 8601)</para>
        /// </summary>
        [XmlElement("FechaHoraHusoGenRegistro", Namespace = Namespaces.NamespaceSF, Order = 26)]
        public string OrderedFechaHoraHusoGenRegistro { get; set; }

        /// <summary>
        /// <para>Número de registro obtenido al enviar la autorización
        /// en materia de facturación o de libros registro</para>
        /// <para>Alfanumérico(15)</para>
        /// </summary>
        [XmlElement("NumRegistroAcuerdoFacturacion", Namespace = Namespaces.NamespaceSFLR, Order = 27)]
        public string NumRegistroAcuerdoFacturacion { get; set; }

        /// <summary>
        /// <para>Identificación del acuerdo (resolución) a que se refiere
        /// el artículo 5 del Reglamento. Este campo forma parte del detalle
        /// de las circunstancias de generación del registro de facturación.</para>
        /// <para>Alfanumérico (15)</para>
        /// </summary>
        [XmlElement("IdAcuerdoSistemaInformatico", Namespace = Namespaces.NamespaceSFLR, Order = 28)]
        public string IdAcuerdoSistemaInformatico { get; set; }

        /// <summary>
        ///  Tipo de algoritmo aplicado a cierto contenido del registro
        ///  de facturación para obtener la huella o «hash».
        /// <para>Alfanumérico (2) L12</para> 
        /// </summary>
        [XmlElement(Namespace = Namespaces.NamespaceSF, Order = 29)]
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
        [XmlIgnore]
        public override string Huella 
        {
            get 
            { 
                return _Huella; 
            } 
            set 
            { 
                _Huella = value;
                OrderedHuella = _Huella;
            } 
        }

        /// <summary>
        /// <para>Huella o «hash» de cierto contenido de este registro
        /// de facturación. Dicho contenido se detallará en la documentación
        /// correspondiente en la sede electrónica de la AEAT (documento de huella...).</para>
        /// <para>Alfanumérico (64)</para>
        /// </summary>
        [XmlElement("Huella", Namespace = Namespaces.NamespaceSF, Order = 30)]
        public string OrderedHuella { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Asigna la clave externa que vincula la factura con
        /// la cadena de bloques.
        /// </summary>
        public override void SetExternKey()
        {

            RefExterna = ExternKey;

        }

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {
            return $"{TipoFactura}-{IDFacturaAlta}";
        }

        #endregion

    }

}
