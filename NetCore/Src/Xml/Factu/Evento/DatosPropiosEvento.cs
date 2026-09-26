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
    /// Datos específicos asociados al tipo de evento.
    /// </summary>
    [XmlType(Namespace = Namespaces.NamespaceSf)]
    public class DatosPropiosEvento
    {

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Datos específicos correspondientes al tipo de evento.
        /// Valores esperados:
        /// <para>Para el tipo de evento "03": <see cref="LanzamientoProcesoDeteccionAnomaliasRegFacturacion"/>. </para>
        /// <para>Para el tipo de evento "04": <see cref="DeteccionAnomaliasRegFacturacion"/>. </para>
        /// <para>Para el tipo de evento "05": <see cref="LanzamientoProcesoDeteccionAnomaliasRegEvento"/>. </para>
        /// <para>Para el tipo de evento "06": <see cref="DeteccionAnomaliasRegEvento"/>. </para>
        /// <para>Para el tipo de evento "08": <see cref="ExportacionRegFacturacionPeriodo"/>. </para>
        /// <para>Para el tipo de evento "09": <see cref="ExportacionRegEventoPeriodo"/>. </para>
        /// <para>Para el tipo de evento "10": <see cref="ResumenEventos"/>. </para>
        /// <para>Solo puede contener uno de los tipos indicados. </para>
        /// </summary>
        [XmlElement("LanzamientoProcesoDeteccionAnomaliasRegFacturacion", typeof(LanzamientoProcesoDeteccionAnomaliasRegFacturacion), Order = 0)]
        [XmlElement("DeteccionAnomaliasRegFacturacion", typeof(DeteccionAnomaliasRegFacturacion), Order = 0)]
        [XmlElement("LanzamientoProcesoDeteccionAnomaliasRegEvento", typeof(LanzamientoProcesoDeteccionAnomaliasRegEvento), Order = 0)]
        [XmlElement("DeteccionAnomaliasRegEvento", typeof(DeteccionAnomaliasRegEvento), Order = 0)]
        [XmlElement("ExportacionRegFacturacionPeriodo", typeof(ExportacionRegFacturacionPeriodo), Order = 0)]
        [XmlElement("ExportacionRegEventoPeriodo", typeof(ExportacionRegEventoPeriodo), Order = 0)]
        [XmlElement("ResumenEventos", typeof(ResumenEventos), Order = 0)]
        public object DatosEvento { get; set; }
     
        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {

            return DatosEvento?.ToString() ?? string.Empty;

        }

        #endregion

    }

}