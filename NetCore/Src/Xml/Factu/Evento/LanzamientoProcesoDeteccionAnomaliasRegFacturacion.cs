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
    /// Datos del lanzamiento del proceso de detección de anomalías
    /// en los registros de facturación.
    /// </summary>
    [XmlType(Namespace = Namespaces.NamespaceSf)]
    public class LanzamientoProcesoDeteccionAnomaliasRegFacturacion
    {

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Indica si se ha realizado el proceso de comprobación de la integridad
        /// de las huellas de los registros de facturación.
        /// Valores esperados:
        /// <para>"S": Sí. </para>
        /// <para>"N": No. </para>
        /// </summary>
        [XmlElement(Order = 0)]
        public string RealizadoProcesoSobreIntegridadHuellasRegFacturacion { get; set; }

        /// <summary>
        /// Número de registros de facturación procesados en la comprobación
        /// de la integridad de las huellas.
        /// Valor esperado:
        /// <para>Cadena numérica de 1 a 7 dígitos. </para>
        /// <para>Propiedad opcional. </para>
        /// </summary>
        [XmlElement(Order = 1)]
        public string NumeroDeRegistrosFacturacionProcesadosSobreIntegridadHuellas { get; set; }

        /// <summary>
        /// Indica si se ha realizado el proceso de comprobación de la integridad
        /// de las firmas de los registros de facturación.
        /// Valores esperados:
        /// <para>"S": Sí. </para>
        /// <para>"N": No. </para>
        /// </summary>
        [XmlElement(Order = 2)]
        public string RealizadoProcesoSobreIntegridadFirmasRegFacturacion { get; set; }

        /// <summary>
        /// Número de registros de facturación procesados en la comprobación
        /// de la integridad de las firmas.
        /// Valor esperado:
        /// <para>Cadena numérica de 1 a 7 dígitos. </para>
        /// <para>Propiedad opcional. </para>
        /// </summary>
        [XmlElement(Order = 3)]
        public string NumeroDeRegistrosFacturacionProcesadosSobreIntegridadFirmas { get; set; }

        /// <summary>
        /// Indica si se ha realizado el proceso de comprobación de la trazabilidad
        /// de la cadena de los registros de facturación.
        /// Valores esperados:
        /// <para>"S": Sí. </para>
        /// <para>"N": No. </para>
        /// </summary>
        [XmlElement(Order = 4)]
        public string RealizadoProcesoSobreTrazabilidadCadenaRegFacturacion { get; set; }

        /// <summary>
        /// Número de registros de facturación procesados en la comprobación
        /// de la trazabilidad de la cadena.
        /// Valor esperado:
        /// <para>Cadena numérica de 1 a 7 dígitos. </para>
        /// <para>Propiedad opcional. </para>
        /// </summary>
        [XmlElement(Order = 5)]
        public string NumeroDeRegistrosFacturacionProcesadosSobreTrazabilidadCadena { get; set; }

        /// <summary>
        /// Indica si se ha realizado el proceso de comprobación de la trazabilidad
        /// de las fechas de los registros de facturación.
        /// Valores esperados:
        /// <para>"S": Sí. </para>
        /// <para>"N": No. </para>
        /// </summary>
        [XmlElement(Order = 6)]
        public string RealizadoProcesoSobreTrazabilidadFechasRegFacturacion { get; set; }

        /// <summary>
        /// Número de registros de facturación procesados en la comprobación
        /// de la trazabilidad de las fechas.
        /// Valor esperado:
        /// <para>Cadena numérica de 1 a 7 dígitos. </para>
        /// <para>Propiedad opcional. </para>
        /// </summary>
        [XmlElement(Order = 7)]
        public string NumeroDeRegistrosFacturacionProcesadosSobreTrazabilidadFechas { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {
            return $"Huellas: {RealizadoProcesoSobreIntegridadHuellasRegFacturacion} ({NumeroDeRegistrosFacturacionProcesadosSobreIntegridadHuellas}), Firmas: {RealizadoProcesoSobreIntegridadFirmasRegFacturacion} ({NumeroDeRegistrosFacturacionProcesadosSobreIntegridadFirmas}), Cadena: {RealizadoProcesoSobreTrazabilidadCadenaRegFacturacion} ({NumeroDeRegistrosFacturacionProcesadosSobreTrazabilidadCadena}), Fechas: {RealizadoProcesoSobreTrazabilidadFechasRegFacturacion} ({NumeroDeRegistrosFacturacionProcesadosSobreTrazabilidadFechas})";
        }

        #endregion

    }

}