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
    /// Información del sistema informático.
    /// </summary>
    [XmlType(Namespace = Namespaces.NamespaceSf)]
    public class SistemaInformatico
    {

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Nombre y apellidos o razón social de la persona o entidad productora del sistema informático.
        /// Valor esperado:
        /// <para>Texto con una longitud máxima de 120 caracteres. </para>
        /// </summary>
        [XmlElement(Order = 0)]
        public string NombreRazon { get; set; }

        /// <summary>
        /// NIF de la persona o entidad productora del sistema informático.
        /// Valor esperado:
        /// <para>NIF con una longitud de 9 caracteres. </para>
        /// <para>Esta propiedad e IDOtro son excluyentes. </para>
        /// </summary>
        [XmlElement(Order = 1)]
        public string NIF { get; set; }

        /// <summary>
        /// Identificación de la persona o entidad productora del sistema informático mediante un identificador distinto del NIF.
        /// Valor esperado:
        /// <para>Identificador compuesto por código de país, tipo de identificador e identificador. </para>
        /// <para>Esta propiedad y NIF son excluyentes. </para>
        /// </summary>
        [XmlElement(Order = 2)]
        public IDOtro IDOtro { get; set; }

        /// <summary>
        /// Nombre del sistema informático.
        /// Valor esperado:
        /// <para>Texto con una longitud máxima de 30 caracteres. </para>
        /// <para>Propiedad opcional. </para>
        /// </summary>
        [XmlElement(Order = 3)]
        public string NombreSistemaInformatico { get; set; }

        /// <summary>
        /// Identificador del sistema informático.
        /// Valor esperado:
        /// <para>Texto con una longitud máxima de 2 caracteres. </para>
        /// </summary>
        [XmlElement(Order = 4)]
        public string IdSistemaInformatico { get; set; }

        /// <summary>
        /// Versión del sistema informático.
        /// Valor esperado:
        /// <para>Texto con una longitud máxima de 50 caracteres. </para>
        /// </summary>
        [XmlElement(Order = 5)]
        public string Version { get; set; }

        /// <summary>
        /// Número de instalación del sistema informático.
        /// Valor esperado:
        /// <para>Texto con una longitud máxima de 100 caracteres. </para>
        /// </summary>
        [XmlElement(Order = 6)]
        public string NumeroInstalacion { get; set; }

        /// <summary>
        /// Indica si el sistema informático puede funcionar exclusivamente como VERI*FACTU.
        /// Valores esperados:
        /// <para>"S": Sí. </para>
        /// <para>"N": No. </para>
        /// <para>Propiedad opcional. </para>
        /// </summary>        
        [XmlElement(Order = 7)]
        public string TipoUsoPosibleSoloVerifactu { get; set; }

        /// <summary>
        /// Indica si el sistema informático permite ser utilizado por más de un obligado tributario.
        /// Valores esperados:
        /// <para>"S": Sí. </para>
        /// <para>"N": No. </para>
        /// <para>Propiedad opcional. </para>
        /// </summary>
        [XmlElement(Order = 8)]
        public string TipoUsoPosibleMultiOT { get; set; }

        /// <summary>
        /// Indica si la instalación del sistema informático es utilizada por múltiples obligados tributarios.
        /// Valores esperados:
        /// <para>"S": Sí. </para>
        /// <para>"N": No. </para>
        /// <para>Propiedad opcional. </para>
        /// </summary>
        [XmlElement(Order = 9)]
        public string IndicadorMultiplesOT { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {
            return $"{NombreSistemaInformatico} {Version} ({IdSistemaInformatico}/{NumeroInstalacion}) - {NombreRazon}";
        }

        #endregion

    }

}