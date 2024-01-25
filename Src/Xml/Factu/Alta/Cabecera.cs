/*
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

using System;
using System.Xml.Serialization;

namespace VeriFactu.Xml.Factu.Alta
{

    /// <summary>
    /// Información del envío con la versión y 
    /// los datos del obligado.
    /// Datos de contexto de un suministro.
    /// </summary>
    [Serializable]
    [XmlRoot("Cabecera", Namespace = Namespaces.NamespaceSF)]
    public class Cabecera
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Versiones VeriFactu.
        /// </summary>
        string[] _IDVersions = new string[1]
        {
            // Versión 2023.01
            "0.1"
        };

        /// <summary>
        /// Versión VER*FACTU.
        /// </summary>
        string _Version;

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// <para>Identificación de la versión.</para>
        /// <para>Alfanumérico(3)L15</para>
        /// </summary>
        [XmlElement("IDVersion")]
        public string IDVersion
        {
            get
            {
                return _Version;
            }
            set
            {

                if (Array.IndexOf(_IDVersions, value) == -1)
                    throw new ArgumentException($"Versión {value} no reconocida." +
                        $"La versiones aceptada son {string.Join(", ", _IDVersions)}");

                _Version = value;
            }
        }

        /// <summary>
        /// Obligado que suministra la información.
        /// </summary>
        public Interlocutor ObligadoEmision { get; set; }

        /// <summary>
        /// <para>Tipo de registro (alta inicial, alta sustitutiva). 
        /// Contiene la operación a realizar en el sistema de la AEAT, 
        /// lo que forma parte del detalle de las circunstancias de 
        /// generación del registro.</para>
        /// <para>Alfanumérico (2) L16</para>
        /// </summary>
        [XmlElement("TipoRegistroAEAT")]
        public TipoRegistroAEAT TipoRegistroAEAT { get; set; }

        /// <summary>
        /// <para>Última fecha en la que el sistema informático actuará 
        /// como VERIFACTU. Después de la misma, el sistema dejará de 
        /// funcionar como VERI*FACTU. Este campo forma parte del detalle 
        /// de las circunstancias de generación de los registros de 
        /// facturación actuales y futuros.</para>
        /// <para>Fecha (dd-mm-yyyy)</para>
        /// </summary>
        [XmlElement("FechaFinVeriFactu")]
        public string FechaFinVeriFactu { get; set; }


        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representacioón textual de la instancia.
        /// </summary>
        /// <returns>Representacioón textual de la instancia.</returns>
        public override string ToString()
        {

            return $"[{IDVersion}] {ObligadoEmision}";

        }

        #endregion

    }

}
