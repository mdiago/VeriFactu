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

using System.Xml.Serialization;

namespace VeriFactu.Xml.Factu.Evento
{

    /// <summary>
    /// Datos de una anomalía detectada en los registros de evento.
    /// </summary>
    [XmlType(Namespace = Namespaces.NamespaceSf)]
    public class DeteccionAnomaliasRegEvento
    {

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Tipo de anomalía detectada.
        /// Valores esperados:
        /// <para>"01": Integridad-huella. </para>
        /// <para>"02": Integridad-firma. </para>
        /// <para>"03": Integridad - Otros. </para>
        /// <para>"04": Trazabilidad-cadena-registro - Reg. no primero pero con reg. anterior no anotado o inexistente. </para>
        /// <para>"05": Trazabilidad-cadena-registro - Reg. no último pero con reg. posterior no anotado o inexistente. </para>
        /// <para>"06": Trazabilidad-cadena-registro - Otros. </para>
        /// <para>"07": Trazabilidad-cadena-huella - Huella del reg. no se corresponde con la 'huella del reg. anterior' almacenada en el registro posterior. </para>
        /// <para>"08": Trazabilidad-cadena-huella - Campo 'huella del reg. anterior' no se corresponde con la huella del reg. anterior. </para>
        /// <para>"09": Trazabilidad-cadena-huella - Otros. </para>
        /// <para>"10": Trazabilidad-cadena - Otros. </para>
        /// <para>"11": Trazabilidad-fechas - Fecha-hora anterior a la fecha del reg. anterior. </para>
        /// <para>"12": Trazabilidad-fechas - Fecha-hora posterior a la fecha del reg. posterior. </para>
        /// <para>"13": Trazabilidad-fechas - Reg. con fecha-hora de generación posterior a la fecha-hora actual del sistema. </para>
        /// <para>"14": Trazabilidad-fechas - Otros. </para>
        /// <para>"15": Trazabilidad - Otros. </para>
        /// <para>"90": Otros. </para>
        /// </summary>
        [XmlElement(Order = 0)]
        public string TipoAnomalia { get; set; }

        /// <summary>
        /// Información adicional sobre la anomalía detectada.
        /// Valor esperado:
        /// <para>Texto con una longitud máxima de 100 caracteres. </para>
        /// <para>Propiedad opcional. </para>
        /// </summary>        
        [XmlElement(Order = 1)]
        public string OtrosDatosAnomalia { get; set; }

        /// <summary>
        /// Identificación del registro de evento en el que se ha detectado la anomalía.
        /// Valor esperado:
        /// <para>Datos identificativos del registro de evento anómalo. </para>
        /// <para>Propiedad opcional. </para>
        /// </summary>        
        [XmlElement(Order = 2)]
        public RegEvento RegistroEventoAnomalo { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {
            return $"{TipoAnomalia}: {RegistroEventoAnomalo}";
        }

        #endregion

    }

}