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

namespace VeriFactu.Xml.Factu.Anulacion
{

    /// <summary>
    /// Datos correspondientes al registro de facturacion de anulación.
    /// </summary>
    public partial class RegistroAnulacion : Registro
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
        /// </summary>
        /// <returns>Cadena de entrada para el cálculo
        /// del hash</returns>
        internal protected override string GetHashTextInput()
        {

            IDFactura = IDFacturaAnulada;

            return $"IDEmisorFacturaAnulada={IDFacturaAnulada?.IDEmisorFacturaAnulada}" +
                $"&NumSerieFacturaAnulada={IDFacturaAnulada?.NumSerieFacturaAnulada}" +
                $"&FechaExpedicionFacturaAnulada={IDFacturaAnulada?.FechaExpedicionFacturaAnulada}" +
                $"&Huella={Encadenamiento?.RegistroAnterior?.Huella}" +
                $"&FechaHoraHusoGenRegistro={FechaHoraHusoGenRegistro}";

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
        public IDFactura IDFacturaAnulada { get; set; }

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
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {
            return $"{IDFacturaAnulada}";
        }

        #endregion

    }

}
