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

using System.Xml.Serialization;

namespace VeriFactu.Xml.Factu.Evento
{

    /// <summary>
    /// Datos identificativos del evento anterior.
    /// </summary>
    [XmlType(Namespace = Namespaces.NamespaceSf)]
    public class EventoAnterior
    {

        #region Propiedades Públicas de Instancia

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
        [XmlElement(Order = 0)]
        public TipoEvento TipoEvento { get; set; }

        /// <summary>
        /// Fecha, hora y huso horario de generación del evento anterior.
        /// Valor esperado:
        /// <para>Fecha y hora en formato ISO 8601: YYYY-MM-DDThh:mm:ssTZD. </para>
        /// <para>Ejemplo: 2024-01-01T19:20:30+01:00. </para>
        /// </summary>
        [XmlElement(Order = 1)]
        public string FechaHoraHusoGenEvento { get; set; }

        /// <summary>
        /// Huella del registro de evento anterior.
        /// Valor esperado:
        /// <para>Texto con una longitud máxima de 64 caracteres. </para>
        /// </summary>
        [XmlElement(Order = 2)]
        public string HuellaEvento { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {
            return $"{TipoEvento}: {FechaHoraHusoGenEvento} ({HuellaEvento})";
        }

        #endregion

    }

}