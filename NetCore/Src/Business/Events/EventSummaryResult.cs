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
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Evento;

namespace VeriFactu.Business.Events
{

    /// <summary>
    /// Datos correspondientes al registro resumen de eventos.
    /// </summary>
    internal class EventSummaryResult
    {

        #region Propiedades Privadas de Instacia

        /// <summary>
        /// Resumen de los registros de facturación generados, incluyendo
        /// el primer y último registro, el número total de registros, el importe total de impuestos y el importe total.
        /// </summary>
        internal Dictionary<TipoEvento, int> Events { get; private set; }

        /// <summary>
        /// Resumen de los registros de facturación generados, incluyendo
        /// el primer y último registro, el número total de registros, el importe total de impuestos y el importe total.
        /// </summary>
        internal Registro FirstInvoiceRecord { get; set; }

        /// <summary>
        /// Resumen de los registros de facturación generados, incluyendo el último.
        /// </summary>
        internal Registro LastInvoiceRecord { get; set; }

        /// <summary>
        /// Resumen de los registros de facturación generados, incluyendo el número total de registros.
        /// </summary>
        internal int InvoiceRegistrations { get; set; }

        /// <summary>
        /// Resumen de los registros de facturación generados, incluyendo el importe total de impuestos.
        /// </summary>
        internal decimal TotalTaxAmount { get; set; }

        /// <summary>
        /// Resumen de los registros de facturación generados, incluyendo el importe total.
        /// </summary>
        internal decimal TotalAmount { get; set; }

        /// <summary>
        /// Resumen de los registros de facturación generados, incluyendo el número total de registros cancelados.
        /// </summary>
        internal int InvoiceCancellations { get; set; }

        #endregion

        #region Constructores de Instancia

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="EventSummaryResult"/>.
        /// </summary>
        internal EventSummaryResult()
        {

            Events = new Dictionary<TipoEvento, int>();

        }

        #endregion

    }

}