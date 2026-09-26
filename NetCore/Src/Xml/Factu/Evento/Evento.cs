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
using System.Xml;
using System.Xml.Serialization;
using VeriFactu.Common;
using VeriFactu.Xml.Factu.Alta;

namespace VeriFactu.Xml.Factu.Evento
{

    /// <summary>
    /// Datos de un evento.
    /// </summary>
    [XmlType(Namespace = Namespaces.NamespaceSf)]
    public class Evento : Registro
    {

        #region Métodos Privados de Instancia

        /// <summary>
        /// Obtiene la cadena de texto utilizada para el cálculo de la huella
        /// del registro de evento.
        /// </summary>
        /// <returns>Cadena de texto utilizada para el cálculo de la huella.</returns>
        internal protected override string GetHashTextInput()
        {

            var tipoEvento = Utils.GetXmlEnumValue(TipoEvento);

            return $"NIF={SistemaInformatico?.NIF?.Trim()}" +
                $"&ID={SistemaInformatico?.IDOtro?.ID?.Trim()}" +
                $"&IdSistemaInformatico={SistemaInformatico?.IdSistemaInformatico?.Trim()}" +
                $"&Version={SistemaInformatico?.Version?.Trim()}" +
                $"&NumeroInstalacion={SistemaInformatico?.NumeroInstalacion?.Trim()}" +
                $"&NIF={ObligadoEmision?.NIF?.Trim()}" +
                $"&TipoEvento={tipoEvento}" +
                $"&HuellaEvento={Encadenamiento?.EventoAnterior?.HuellaEvento?.Trim()}" +
                $"&FechaHoraHusoGenEvento={FechaHoraHusoGenEvento?.Trim()}";

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Datos identificativos del sistema informático que genera el registro de evento.
        /// Valor esperado:
        /// <para>Información de la persona o entidad productora, identificación del sistema, versión y número de instalación. </para>
        /// </summary>
        [XmlElement(Order = 0)]
        public SistemaInformatico SistemaInformatico { get; set; }

        /// <summary>
        /// Datos identificativos del obligado a expedir la factura.
        /// Valor esperado:
        /// <para>Nombre o razón social y NIF del obligado a la emisión. </para>
        /// </summary>
        [XmlElement(Order = 1)]
        public PersonaFisicaJuridicaES ObligadoEmision { get; set; }

        /// <summary>
        /// Indica si la factura ha sido expedida materialmente por el destinatario o por un tercero.
        /// Valores esperados:
        /// <para>"D": Destinatario. </para>
        /// <para>"T": Tercero. </para>
        /// <para>Propiedad opcional. </para>
        /// </summary>
        [XmlElement(Order = 2)]
        public EmitidaPorTerceroODestinatario EmitidaPorTerceroODestinatario { get; set; }

        /// <summary>
        /// Con true se serializa el dato, con false no.
        /// </summary>
        [XmlIgnore]
        public bool EmitidaPorTerceroODestinatarioSpecified { get; set; }

        /// <summary>
        /// Datos identificativos del tercero o destinatario que ha expedido materialmente la factura.
        /// Valor esperado:
        /// <para>Nombre o razón social y NIF o identificador alternativo del tercero o destinatario. </para>
        /// <para>Propiedad opcional. </para>
        /// </summary>
        [XmlElement(Order = 3)]
        public PersonaFisicaJuridica TerceroODestinatario { get; set; }

        /// <summary>
        /// Fecha, hora y huso horario de generación del registro de evento.
        /// Valor esperado:
        /// <para>Fecha y hora en formato ISO 8601: YYYY-MM-DDThh:mm:ssTZD. </para>
        /// <para>Ejemplo: 2024-01-01T19:20:30+01:00. </para>
        /// </summary>
        [XmlElement(Order = 4)]
        public string FechaHoraHusoGenEvento { get; set; }

        /// <summary>
        /// Tipo de evento.
        /// Valores esperados:
        /// <para>"01": Inicio del funcionamiento del sistema informático como «NO VERI*FACTU». </para>
        /// <para>"02": Fin del funcionamiento del sistema informático como «NO VERI*FACTU». </para>
        /// <para>"03": Lanzamiento del proceso de detección de anomalías en los registros de facturación. </para>
        /// <para>"04": Detección de anomalías en la integridad, inalterabilidad y trazabilidad de registros de facturación. </para>
        /// <para>"05": Lanzamiento del proceso de detección de anomalías en los registros de evento. </para>
        /// <para>"06": Detección de anomalías en la integridad, inalterabilidad y trazabilidad de registros de evento. </para>
        /// <para>"07": Restauración de copia de seguridad, cuando ésta se gestione desde el propio sistema informático de facturación. </para>
        /// <para>"08": Exportación de registros de facturación generados en un periodo. </para>
        /// <para>"09": Exportación de registros de evento generados en un periodo. </para>
        /// <para>"10": Registro resumen de eventos. </para>
        /// <para>"90": Otros tipos de eventos a registrar voluntariamente por la persona o entidad productora del sistema informático. </para>
        /// </summary>
        [XmlElement(Order = 5)]
        public TipoEvento TipoEvento { get; set; }

        /// <summary>
        /// Datos específicos asociados al tipo de evento.
        /// Valor esperado:
        /// <para>Datos propios correspondientes al tipo de evento generado. </para>
        /// <para>Propiedad opcional. </para>
        /// </summary>
        [XmlElement(Order = 6)]
        public DatosPropiosEvento DatosPropiosEvento { get; set; }

        /// <summary>
        /// Información adicional relativa al evento.
        /// Valor esperado:
        /// <para>Texto con una longitud máxima de 100 caracteres. </para>
        /// <para>Propiedad opcional. </para>
        /// </summary>
        [XmlElement(Order = 7)]
        public string OtrosDatosEvento { get; set; }

        /// <summary>
        /// Datos de encadenamiento del registro de evento.
        /// Valor esperado:
        /// <para>Indicador de primer evento o datos identificativos del evento anterior. </para>
        /// </summary>
        [XmlElement(Order = 8)]
        public Encadenamiento Encadenamiento { get; set; }

        /// <summary>
        /// Tipo de huella utilizada para el registro de evento.
        /// Valores esperados:
        /// <para>"01": SHA-256. </para>
        /// </summary>
        [XmlElement(Order = 9)]
        public TipoHuella TipoHuella { get; set; }

        /// <summary>
        /// Huella del registro de evento.
        /// Valor esperado:
        /// <para>Texto con una longitud máxima de 64 caracteres. </para>
        /// </summary>
        [XmlElement(Order = 10)]
        public string HuellaEvento { get; set; }

        /// <summary>
        /// Firma electrónica del registro de evento.
        /// Valor esperado:
        /// <para>Elemento Signature conforme al estándar XMLDSig. </para>
        /// </summary>
        [XmlAnyElement(Name = "Signature", Namespace = Namespaces.NamespaceDs, Order = 11)]
        public XmlElement Signature { get; set; }

        /// <summary>
        /// Id. del eslabón en la cadena de eventos.
        /// </summary>
        [XmlIgnore]
        public ulong EventChainLinkID { get; set; }

        /// <summary>
        /// Referencia externa.
        /// </summary>
        [XmlIgnore]
        public override string ExternKey => EventChainLinkID == 0 ?
            null : $"{EventChainLinkID}".PadLeft(20, '0');

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {
            return $"{TipoEvento}: {FechaHoraHusoGenEvento}";
        }

        #endregion

    }

}