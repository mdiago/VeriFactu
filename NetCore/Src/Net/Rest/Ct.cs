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
    serving VeriFactu XML data on the fly in a web application, shipping VeriFactu
    with a closed source product.
    
    For more information, please contact Irene Solutions SL. at this
    address: info@irenesolutions.com
 */

using VeriFactu.Config;
using VeriFactu.DataStore;
using VeriFactu.Net.Rest.Json;

namespace VeriFactu.Net.Rest
{

    /// <summary>
    /// API Ct.
    /// </summary>
    internal class Ct : JsonSerializable
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor
        /// </summary>
        public Ct()
        {

            SystemName = Settings.Current.SistemaInformatico.NombreSistemaInformatico;
            SystemID = Settings.Current.SistemaInformatico.IdSistemaInformatico;
            VersionID = Settings.Current.SistemaInformatico.Version;
            InstallationNumber = Settings.Current.SistemaInformatico.NumeroInstalacion;
            UseType = Settings.Current.SistemaInformatico.TipoUsoPosibleSoloVerifactu;
            MultiOT = Settings.Current.SistemaInformatico.TipoUsoPosibleSoloVerifactu;
            HasMultiOT = Settings.Current.SistemaInformatico.IndicadorMultiplesOT;
            CountOT = Seller.GetSellers().Count;

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// <para>Nombre dado por el productor al sistema
        /// informático de facturación utilizado.</para>
        /// <para>Alfanumérico (30).</para>
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// <para>Código ID del sistema informático de facturación utilizado.</para>
        /// <para>Alfanumérico(2).</para>
        /// </summary>
        public string SystemID { get; set; }

        /// <summary>
        /// <para>Identificación de la Versión del sistema de facturación utilizado.</para>
        /// <para>Alfanumérico(50).</para>
        /// </summary>
        public string VersionID { get; set; }

        /// <summary>
        /// <para>Número de instalación del sistema informático de facturación (SIF) utilizado.
        /// Deberá distinguirlo de otros posibles SIF utilizados para realizar la facturación del
        /// obligado a expedir facturas, es decir, de otras posibles instalaciones de SIF pasadas,
        /// presentes o futuras utilizadas para realizar la facturación del obligado a expedir
        /// facturas, incluso aunque en dichas instalaciones se emplee el mismo SIF de un productor.</para>
        /// <para>Alfanumérico(100).</para>
        /// </summary>
        public string InstallationNumber { get; set; }

        /// <summary>
        /// <para>Especifica si para cumplir el Reglamento el sistema informático
        /// de facturación solo puede funcionar exclusivamente como Veri*Factu.</para>
        /// <para>Alfanumérico (1) L4:</para>
        /// <para>S: Sí</para>
        /// <para>N: No</para>
        /// </summary>
        public string UseType { get; set; }

        /// <summary>
        /// <para> Especifica si el sistema informático de facturación permite llevar independientemente
        /// la facturación de varios obligados tributarios (valor "S") o solo de uno (valor "N").
        /// Obligatorio en registros de facturación de alta y de anulación, y opcional en registros
        /// de evento.</para>
        /// <para>Alfanumérico (1) L4:</para>
        /// <para>S: Sí</para>
        /// <para>N: No</para>
        /// </summary>
        public string MultiOT { get; set; }

        /// <summary>
        /// <para> Indicador de que el sistema informático, en el momento de la generación de este registro,
        /// está soportando la facturación de más de un obligado tributario. Este valor deberá obtenerlo automáticamente
        /// el sistema informático a partir del número de obligados tributarios contenidos y/o gestionados en él en ese
        /// momento, independientemente de su estado operativo (alta, baja...), no pudiendo obtenerse a partir de otra
        /// información ni ser introducido directamente por el usuario del sistema informático ni cambiado por él.
        /// El valor "N" significará que el sistema informático solo contiene y/o gestiona un único obligado tributario
        /// (de alta o de baja o en cualquier otro estado), que se corresponderá con el obligado a expedir factura de
        /// este registro de facturación. En cualquier otro caso, se deberá informar este campo con el valor "S".
        /// Obligatorio en registros de facturación de alta y de anulación, y opcional en registros de evento.</para>
        /// <para>Alfanumérico (1) L4:</para>
        /// <para>S: Sí</para>
        /// <para>N: No</para>
        /// </summary>
        public string HasMultiOT { get; set; }

        /// <summary>
        /// Número de obligados tributarios gestionado por el sistema.
        /// </summary>
        public int CountOT { get; set; }

        #endregion

    }

}