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

using System.Collections.Generic;
using System.Xml.Serialization;

namespace VeriFactu.Xml.Factu.Evento
{

    /// <summary>
    /// Datos correspondientes al registro resumen de eventos.
    /// </summary>
    [XmlType(Namespace = Namespaces.NamespaceSf)]
    public class ResumenEventos
    {

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Resumen del número de eventos generados, agrupados por tipo.
        /// Valor esperado:
        /// <para>Uno o más tipos de evento con el número de eventos generados de cada tipo. </para>
        /// <para>Se admiten entre 1 y 20 elementos. </para>
        /// </summary>        
        [XmlElement("TipoEvento", Order = 0)]
        public List<TipoEventoAgr> TipoEvento { get; set; }

        /// <summary>
        /// Identificación y huella del primer registro de facturación del periodo.
        /// Valor esperado:
        /// <para>Identificación de la factura expedida y huella del registro de facturación. </para>
        /// <para>Propiedad opcional. </para>
        /// </summary>
        [XmlElement(Order = 1)]
        public IDFacturaExpedidaHuella RegistroFacturacionInicialPeriodo { get; set; }

        /// <summary>
        /// Identificación y huella del último registro de facturación del periodo.
        /// Valor esperado:
        /// <para>Identificación de la factura expedida y huella del registro de facturación. </para>
        /// <para>Propiedad opcional. </para>
        /// </summary>
        [XmlElement(Order = 2)]
        public IDFacturaExpedidaHuella RegistroFacturacionFinalPeriodo { get; set; }

        /// <summary>
        /// Número de registros de facturación de alta generados durante el periodo.
        /// Valor esperado:
        /// <para>Cadena numérica de 1 a 6 dígitos. </para>
        /// </summary>
        [XmlElement(Order = 3)]
        public string NumeroDeRegistrosFacturacionAltaGenerados { get; set; }

        /// <summary>
        /// Suma de las cuotas totales de los registros de facturación
        /// de alta generados durante el periodo.
        /// Valor esperado:
        /// <para>Número con signo opcional, de 1 a 12 dígitos enteros y hasta 2 decimales. </para>
        /// <para>El separador decimal esperado es ".". </para>
        /// </summary>
        [XmlElement(Order = 4)]
        public string SumaCuotaTotalAlta { get; set; }

        /// <summary>
        /// Suma de los importes totales de los registros de facturación
        /// de alta generados durante el periodo.
        /// Valor esperado:
        /// <para>Número con signo opcional, de 1 a 12 dígitos enteros y hasta 2 decimales. </para>
        /// <para>El separador decimal esperado es ".". </para>
        /// </summary>
        [XmlElement(Order = 5)]
        public string SumaImporteTotalAlta { get; set; }

        /// <summary>
        /// Número de registros de facturación de anulación generados durante el periodo.
        /// Valor esperado:
        /// <para>Cadena numérica de 1 a 6 dígitos. </para>
        /// </summary>
        [XmlElement(Order = 6)]
        public string NumeroDeRegistrosFacturacionAnulacionGenerados { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {
            return $"{TipoEvento?.Count ?? 0} tipos de evento, {NumeroDeRegistrosFacturacionAltaGenerados} altas, {NumeroDeRegistrosFacturacionAnulacionGenerados} anulaciones";
        }

        #endregion

    }

}