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

using System;
using System.Xml.Serialization;

namespace VeriFactu.Xml.Factu.Consulta.Respuesta
{

    /// <summary>
    /// Bloque que contiene todos los campos de una factura.
    /// </summary>
    public class RegistroRespuestaConsultaFactuSistemaFacturacion : IComparable
    {

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Bloque que contiene los campos que identifican al registros de facturación.
        /// </summary>
        [XmlElement(Namespace = Namespaces.NamespaceTikLRRC)]
        public Factu.Respuesta.IDFactura IDFactura { get; set; }

        /// <summary>
        /// Bloque que contiene los campos del registros de facturación registrado.
        /// </summary>
        [XmlElement(Namespace = Namespaces.NamespaceTikLRRC)]
        public DatosRegistroFacturacion DatosRegistroFacturacion { get; set; }

        /// <summary>
        /// Bloque que contiene los campos con información de la presentación realizada:
        /// <para> NIFPresentador</para>
        /// <para> TimestampPresentacion</para>
        /// <para> IdPeticion</para>
        /// </summary>
        [XmlElement(Namespace = Namespaces.NamespaceTikLRRC)]
        public DatosPresentacion DatosPresentacion { get; set; }

        /// <summary>
        /// Bloque que contiene los campos del estado del registro de facturación registrado:
        /// <para> TimestampUltimaModificacion</para>
        /// <para> EstadoRegistro</para>
        /// <para> CodigoErrorRegistro</para>
        /// <para> DescripcionErrorRegistro</para>
        /// </summary>
        [XmlElement(Namespace = Namespaces.NamespaceTikLRRC)]
        public EstadoRegistro EstadoRegistro { get; set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representacioón textual de la instancia.
        /// </summary>
        /// <returns>Representacioón textual de la instancia.</returns>
        public override string ToString()
        {

            return $"{DatosRegistroFacturacion?.RefExterna}, {IDFactura}, {EstadoRegistro}, {DatosRegistroFacturacion?.FechaHoraHusoGenRegistro}";

        }

        /// <summary>
        /// Compara la instancia actual con otro objeto del mismo tipo y devuelve un entero
        /// que indica si la posición de la instancia actual es anterior, posterior o igual
        /// que la del otro objeto en el criterio de ordenación.
        /// </summary>
        /// <param name="obj">Objeto que se va a comparar con esta instancia.</param>
        /// <returns>
        /// Un valor que indica el orden relativo de los objetos que se están comparando.
        /// El valor devuelto tiene los siguientes significados: Valor Significado Menor
        /// que cero Esta instancia es anterior a obj en el criterio de ordenación. Cero
        /// Esta instancia se produce en la misma posición del criterio de ordenación que
        /// obj. Mayor que cero Esta instancia sigue a obj en el criterio de ordenación.
        /// </returns>
        /// <exception cref="ArgumentException">obj no es del mismo tipo que esta instancia.</exception>
        public int CompareTo(object obj)
        {

            var other = obj as RegistroRespuestaConsultaFactuSistemaFacturacion;

            if (other == null)
                throw new ArgumentException($"El objeto {obj} no es del tipo RegistroRespuestaConsultaFactuSistemaFacturacion.");

            if(ulong.TryParse(DatosRegistroFacturacion.RefExterna, out ulong id) && ulong.TryParse(other.DatosRegistroFacturacion.RefExterna, out ulong idOther))
                return id.CompareTo(idOther);

            var date = Convert.ToDateTime(DatosRegistroFacturacion.FechaHoraHusoGenRegistro);
            var dateOther = Convert.ToDateTime(other.DatosRegistroFacturacion.FechaHoraHusoGenRegistro);

            return date.CompareTo(dateOther);

        }

        #endregion

    }

}