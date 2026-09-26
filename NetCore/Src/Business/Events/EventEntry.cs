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

using VeriFactu.Xml.Factu.Evento;

namespace VeriFactu.Business.Events
{

    /// <summary>
    /// Representa una entrada pendiente de procesamiento
    /// en la cola de eventos SIF.
    /// </summary>
    internal class EventEntry
    {

        #region Propiedades Privadas de Instacia

        /// <summary>
        /// Indica que la entrada representa la ejecución del proceso
        /// de detección de anomalías sobre los registros de evento.
        /// </summary>
        internal bool IsEventAnomalyDetection { get; private set; }

        /// <summary>
        /// Indica que la entrada representa la generación
        /// del resumen de eventos.
        /// </summary>
        internal bool IsEventSummary { get; private set; }

        #endregion

        #region Constructores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tipoEvento">Tipo de evento.</param>
        /// <param name="sellerID">Identificador del emisor.</param>
        /// <param name="sellerName">Nombre o razón social del emisor.</param>
        /// <param name="datosPropiosEvento">Datos propios del evento.</param>
        /// <param name="otrosDatosEvento">Otros datos del evento.</param>
        internal EventEntry(TipoEvento tipoEvento, string sellerID, string sellerName,
            DatosPropiosEvento datosPropiosEvento = null, string otrosDatosEvento = null)
        {

            TipoEvento = tipoEvento;
            SellerID = sellerID;
            SellerName = sellerName;
            DatosPropiosEvento = datosPropiosEvento;
            OtrosDatosEvento = otrosDatosEvento;

        }

        /// <summary>
        /// Crea una entrada para ejecutar una operación interna
        /// sobre los registros de evento.
        /// </summary>
        /// <param name="sellerID">Identificador del emisor.</param>
        /// <param name="sellerName">Nombre o razón social del emisor.</param>
        /// <param name="isEventSummary">
        /// Indica si la operación corresponde a la generación
        /// del resumen de eventos.
        /// </param>
        internal EventEntry(
            string sellerID,
            string sellerName,
            bool isEventSummary)
        {

            SellerID = sellerID;
            SellerName = sellerName;
            IsEventSummary = isEventSummary;

        }

        /// <summary>
        /// Crea una entrada para ejecutar el proceso de detección
        /// de anomalías sobre los registros de evento.
        /// </summary>
        internal EventEntry(string sellerID, string sellerName)
        {

            SellerID = sellerID;
            SellerName = sellerName;
            IsEventAnomalyDetection = true;

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Tipo de evento.
        /// </summary>
        public TipoEvento TipoEvento { get; private set; }

        /// <summary>
        /// Identificador del emisor al que corresponde el evento.
        /// </summary>
        public string SellerID { get; private set; }

        /// <summary>
        /// Nombre o razón social del emisor al que corresponde el evento.
        /// </summary>
        public string SellerName { get; private set; }

        /// <summary>
        /// Datos propios del evento.
        /// </summary>
        public DatosPropiosEvento DatosPropiosEvento { get; private set; }

        /// <summary>
        /// Otros datos asociados al evento.
        /// </summary>
        public string OtrosDatosEvento { get; private set; }

        #endregion

    }

}