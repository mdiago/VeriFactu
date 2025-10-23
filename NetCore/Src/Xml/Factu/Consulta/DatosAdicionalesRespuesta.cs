﻿/*
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

namespace VeriFactu.Xml.Factu.Consulta
{

    /// <summary>
    /// Datos adicionales para la respuesta.
    /// </summary>
    public class DatosAdicionalesRespuesta
    {

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// <para> Indicador que especifica si se quiere obtener en la respuesta
        /// el campo NombreRazonEmisor en la información del registro se facturacion.
        /// Si el Valor es S aumenta el tiempo de respuesta en la cosulta por detinatario.
        /// Si no se informa este campo se entenderá que tiene valor “N”.</para>
        /// <para> Alfanumérico(1) Valores posibles: “S” o “N”</para>
        /// </summary>

        public string MostrarNombreRazonEmisor { get; set; }

        /// <summary>
        /// <para> Indicador que especifica si se quiere obtener en la respuesta el bloque
        /// SistemaInformatico en la información del registro se facturacion.
        /// Si el Valor es S aumenta el tiempo de respuesta en la cosulta.
        /// Si se consulta por Destinatario el valor del campo MostrarSistemaInformatico
        /// debe ser 'N' o no estar cumplimentado.</para>
        /// <para> Alfanumérico(1) Valores posibles: “S” o “N”</para>
        /// </summary>
        public string MostrarSistemaInformatico { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representacioón textual de la instancia.
        /// </summary>
        /// <returns>Representacioón textual de la instancia.</returns>
        public override string ToString()
        {

            return $"MostrarNombreRazonEmisor: {MostrarNombreRazonEmisor}, MostrarSistemaInformatico: {MostrarSistemaInformatico}";

        }

        #endregion

    }

}