﻿/*
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

namespace VeriFactu.Xml.Factu
{

    /// <summary>
    /// Información del sistema informático.
    /// </summary>
    public class SistemaInformatico : Interlocutor
    {

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// <para>Nombre dado por el productor al sistema
        /// informático de facturación utilizado.</para>
        /// <para>Alfanumérico (30).</para>
        /// </summary>
        public string NombreSistemaInformatico { get; set; }

        /// <summary>
        /// <para>Código ID del sistema informático de facturación utilizado.</para>
        /// <para>Alfanumérico(2).</para>
        /// </summary>
        public string IdSistemaInformatico { get; set; }

        /// <summary>
        /// <para>Identificación de la Versión del sistema de facturación utilizado.</para>
        /// <para>Alfanumérico(50).</para>
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// <para>Número de instalación del sistema informático de facturación utilizado.</para>
        /// <para>Alfanumérico(30).</para>
        /// </summary>
        public string NumeroInstalacion { get; set; }

        /// <summary>
        /// <para>Especifica si para cumplir el Reglamento el sistema informático
        /// de facturación solo puede funcionar exclusivamente como Veri*Factu.</para>
        /// <para> Alfanumérico (1) L11.</para>
        /// </summary>
        public string TipoUsoPosibleSoloVerifactu { get; set; }

        /// <summary>
        /// <para> Especifica si el sistema informático de facturación permite ser
        /// configurado para facturar sin cumplir con el Reglamento ni con el SII
        /// ni con los sistemas de facturación admitidos por las administraciones
        /// tributarias forales.</para>
        /// <para> Alfanumérico (1) L11.</para>
        /// </summary>
        public string TipoUsoPosibleOtros { get; set; }

        /// <summary>
        /// <para> Especifica si el sistema informático de facturación permite
        /// llevar independientemente la facturación de varios obligados tributarios.</para>
        /// <para> Alfanumérico (1) L11.</para>
        /// </summary>
        public string TipoUsoPosibleMultiOT { get; set; }

        /// <summary>
        /// <para> Número de obligados tributarios dados de alta en el sistema
        /// informático sobre los que llevar independientemente su facturación.</para>
        /// <para> Alfanumérico (1) L11.</para>
        /// </summary>
        public string NumeroOTAlta { get; set; }    

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {
            return $"{IdSistemaInformatico} ({Version})";
        }

        #endregion

    }
}
