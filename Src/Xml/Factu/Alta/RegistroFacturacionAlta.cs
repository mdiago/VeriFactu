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
    public class RegistroFacturacionAlta
    {

        #region Propiedades Públicas de Instancia

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
        /// <para>Tipo de registro (alta inicial, alta sustitutiva). 
        /// Contiene la operación realizada en el sistema informático
        /// de facturación utilizado, lo que forma parte del detalle de
        /// las circunstancias de generación del registro de facturación.</para>
        ///  <para>Alfanumérico (2)  L17.</para>
        /// </summary>
        [XmlElement("TipoRegistroSIF", Namespace = Namespaces.NamespaceSFLR)]
        public TipoRegistroSIF TipoRegistroSIF { get; set; }

        /// <summary>
        /// <para>Clave del tipo de factura (L2).</para>
        /// </summary>
        [XmlElement("TipoFactura", Namespace = Namespaces.NamespaceSFLR)]
        public TipoFactura TipoFactura { get; set; }

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
        [XmlElement("FacturaSimplificadaArticulos7.2_7.3", Namespace = Namespaces.NamespaceSFLR)]
        public string FacturaSimplificadaArticulos7_2_7_3 { get; set; }

        /// <summary>
        /// <para>Factura simplificada Articulo 7.2 Y 7.3 RD 1619/2012. 
        /// Si no se informa este campo se entenderá que tiene valor  “N".</para>
        /// <para>Alfunumérico (1) L4</para>
        /// </summary>
        [XmlElement("FacturaSinIdentifDestinatarioArticulo6.1.d", Namespace = Namespaces.NamespaceSFLR)]
        public string FacturaSinIdentifDestinatarioArticulo6_1_d { get; set; }

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
        /// <para>Importe total de la factura.</para>
        /// <para>Decimal(12,2).</para>
        /// </summary>
        [XmlElement("ImporteTotal", Namespace = Namespaces.NamespaceSFLR)]
        public string ImporteTotal { get; set; }

        /// <summary>
        /// Encadenamiento con la factura anterior..
        /// </summary>
        [XmlElement("EncadenamientoRegistroAnterior", Namespace = Namespaces.NamespaceSFLR)]
        public EncadenamientoRegistroAnterior EncadenamientoRegistroAnterior { get; set; }

        /// <summary>
        ///  Información del sistema informático.
        /// </summary>
        [XmlElement("SistemaInformatico", Namespace = Namespaces.NamespaceSFLR)]
        public SistemaInformatico SistemaInformatico { get; set; }

        /// <summary>
        /// <para>Número de registro obtenido al enviar la autorización
        /// en materia de facturación o de libros registro</para>
        /// <para>Alfanumérico(15)</para>
        /// </summary>
        [XmlElement("FechaGenRegistro", Namespace = Namespaces.NamespaceSFLR)]
        public string FechaGenRegistro { get; set; }

        /// <summary>
        /// <para>Número de registro obtenido al enviar la autorización
        /// en materia de facturación o de libros registro</para>
        /// <para>Alfanumérico(15)</para>
        /// </summary>
        [XmlElement("NumRegistroAcuerdoFacturacion", Namespace = Namespaces.NamespaceSFLR)]
        public string NumRegistroAcuerdoFacturacion { get; set; }

        /// <summary>
        /// <para>Número de registro obtenido al enviar la autorización
        /// en materia de facturación o de libros registro</para>
        /// <para>Alfanumérico(15)</para>
        /// </summary>
        [XmlElement("HoraGenRegistro", Namespace = Namespaces.NamespaceSFLR)]
        public string HoraGenRegistro { get; set; }

        /// <summary>
        /// <para>Huso horario que está usando el sistema informático de
        /// facturación en el momento de generación del registro de facturación.</para>
        /// <para>Alfanumérico (2) L13</para>
        /// </summary>
        [XmlElement("HusoHorarioGenRegistro", Namespace = Namespaces.NamespaceSFLR)]
        public HusoHorario HusoHorarioGenRegistro { get; set; }

        /// <summary>
        /// <para>Número de registro obtenido al enviar la autorización en materia
        /// de facturación o de libros registro a que se refiere la disposición
        /// adicional primera del Real Decreto que aprueba el Reglamento.
        /// Este campo forma parte del detalle de las circunstancias de
        /// generación del registro de facturación.</para>
        /// <para>Alfanumérico(15).</para>
        /// </summary>
        [XmlElement("NumRegistroAcuerdoSistemaInformatico", Namespace = Namespaces.NamespaceSFLR)]
        public string NumRegistroAcuerdoSistemaInformatico { get; set; }

        /// <summary>
        /// <para>Identificación del acuerdo (resolución) a que se refiere
        /// el artículo 5 del Reglamento. Este campo forma parte del detalle
        /// de las circunstancias de generación del registro de facturación.</para>
        /// <para>Alfanumérico (15)</para>
        /// </summary>
        [XmlElement("IdAcuerdoSistemaInformatico", Namespace = Namespaces.NamespaceSFLR)]
        public string IdAcuerdoSistemaInformatico { get; set; }

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
