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
using System.Collections.Generic;
using System.Diagnostics;
using VeriFactu.Business.Operations;
using VeriFactu.Common;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Respuesta;

namespace VeriFactu.Business.FlowControl
{

    /// <summary>
    /// Representa la cola de envío de documentos de un emisor
    /// determinado.
    /// </summary>
    public class SellerQueue : SingletonByKey<SellerQueue>
    {

        #region Variables Privadas Estáticas

        /// <summary>
        /// Máximo número de registros a incluir en un envío.
        /// </summary>
        internal static readonly int MaxRecordNumber = 1000;

        /// <summary>
        /// Bloqueo para thread safe.
        /// </summary>
        private readonly object _Locker = new object();

        #endregion

        #region Variables Privadas de Instancia

        /// <summary>
        /// Lista de procesamiento. En esta lista
        /// se incluyen todos los elmentos a procesar.
        /// </summary>
        Queue<InvoiceAction> _InvoiceActions = new Queue<InvoiceAction>();

        /// <summary>
        /// Almacena el momento de finalización del último
        /// envío.
        /// </summary>
        DateTime _LastProcessMoment;

        /// <summary>
        /// Almacena tiempo de espera comunicado por la AEAT
        /// en el últinmo envío.
        /// </summary>
        int _CurrentWaitSecods = 60;

        #endregion

        #region Propiedades Privadas de Instancia

        /// <summary>
        /// Identificador del vendedor.
        /// Debe utilizarse el identificador fiscal si existe (NIF, VAT Number...).
        /// En caso de no existir, se puede utilizar el número DUNS 
        /// o cualquier otro identificador acordado.
        /// </summary>        
        internal string SellerID { get; private set; }

        /// <summary>
        /// Momento a partir del cual ya se pueden realizar envíos.
        /// </summary>
        internal DateTime AllowedFrom => _LastProcessMoment.AddSeconds(_CurrentWaitSecods);

        /// <summary>
        /// Si es true indica que el envío ya está permitido
        /// por haberse superado el último tiempo de espera comunicado
        /// por la AEAT.
        /// </summary>
        internal bool IsAllowedFrom => AllowedFrom < DateTime.Now;

        /// <summary>
        /// Si es true indica que el envío ya está permitido
        /// por haberse alcanzado el máximo de registros por lote
        /// establecido por la AEAT.
        /// </summary>
        internal bool IsAllowedMaxRecordNumber => _InvoiceActions.Count >= MaxRecordNumber;

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sellerID">Vendedor al que pertenece la cola de proceso.</param>
        public SellerQueue(string sellerID) : base(sellerID)
        {

            SellerID = Key;
            _LastProcessMoment = new DateTime(1, 1, 1);

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Contabiliza los elementos a envíar eliminándolos de la cola
        /// de envío y devolviendolos en una lista.
        /// </summary>
        /// <param name="invoiceRetrySends">Lista de reenvíos. Cuando en cola
        /// existen envíos normales y reenvíos no se puede realizar
        /// un único envío, ya que la cabecera difiere en los dos casos.
        /// En los reenvíos el bloque 'Cabecera' tiene en 'RemisionVoluntaria'
        /// el bloque 'Incidencia'='S'. En estos casos se procesan los
        /// envíos normales dándoles prioridad, que coincida en la cola
        /// un momento de envío en el que únicamente existan reenvíos; En
        /// este momento será cuando se envíen estos registros.</param>
        /// <returns>Lista de los elementos contabilizados.</returns>
        private List<InvoiceAction> Post(out List<InvoiceAction> invoiceRetrySends)
        {

            Utils.Log($"Ejecutando por cola ({SellerID}) tras tiempo espera en segundos:" +
                $" {_CurrentWaitSecods} desde {_LastProcessMoment} hasta {AllowedFrom}");

            Debug.Print($"Ejecutando por cola ({SellerID}) tras tiempo espera en segundos:" +
                $" {_CurrentWaitSecods} desde {_LastProcessMoment} hasta {AllowedFrom}");

            var recordCount = 0;
            var processInvoiceActions = new List<InvoiceAction>();

            // Preparo lote a procesar extrayendolo de la cola
            while (_InvoiceActions.Count > 0 && MaxRecordNumber > recordCount++) 
            {

                InvoiceAction processInvoiceAction = null;

                lock (_Locker)
                    processInvoiceAction = _InvoiceActions.Dequeue();

                processInvoiceActions.Add(processInvoiceAction);

            }

            invoiceRetrySends = new List<InvoiceAction>();
            
            var registros = new List<Registro>();
            var invoiceActions = new List<InvoiceAction>();


            // Flags para controlar si existen reenvíos y envíos normales
            bool hasRetrySends = false;
            bool hasInvoiceActions = false;

            foreach (var action in processInvoiceActions)
            {

                if (action.IsRetrySend)
                    hasRetrySends = true;
                else
                    hasInvoiceActions = true;

            }



            // Si sólo existen reenvíos, estos se procesarán
            var isOnlyRetrySends = hasRetrySends && !hasInvoiceActions;

            foreach (var invoiceAction in processInvoiceActions)
            {

                if (!invoiceAction.Posted)
                {

                    if (invoiceAction.IsRetrySend && !isOnlyRetrySends) // Si no son sólo reenvíos los almacenamos por devolver a la cola
                        invoiceRetrySends.Add(invoiceAction);
                    else
                        invoiceActions.Add(invoiceAction);

                    if(!invoiceAction.IsRetrySend) // En este caso no está contabilizada
                        registros.Add(invoiceAction.Registro); // Almaceno el registo para contabilizar

                }
                else
                {

                    Utils.Log($"Error en el proceso por lotes ({SellerID}) se ha intentado agregar al envío un registro ya contabilizado: {invoiceAction}");
                    Debug.Print($"Error en el proceso por lotes ({SellerID}) se ha intentado agregar al envío un registro ya contabilizado: {invoiceAction}");

                }

            }

            var blockchainManager = Blockchain.Blockchain.GetInstance(SellerID) as Blockchain.Blockchain;

            // Añado los registros a la cadena de bloques
            if(registros.Count > 0)
                blockchainManager.Add(registros);

            Utils.Log($"Actualizando datos de la cadena de bloques ({SellerID}) en {registros.Count} elementos {DateTime.Now}");
            Debug.Print($"Actualizando datos de la cadena de bloques ({SellerID}) en {registros.Count} elementos {DateTime.Now}");

            // Actualizo los cambios
            for (int i = 0; i < invoiceActions.Count; i++)
                invoiceActions[i].SaveBlockchainChanges();

            Utils.Log($"Finalizada actualización de datos de la cadena de bloques en {invoiceActions.Count} elementos {DateTime.Now}");
            Debug.Print($"Finalizada actualización de datos de la cadena de bloques en {invoiceActions.Count} elementos {DateTime.Now}");

            return invoiceActions;

        }

        /// <summary>
        /// Envío el lote de facturas a la AEAT.
        /// </summary>
        /// <param name="invoiceActions">Lista de acciones para registros de factura.</param>
        /// <returns>Devuelve La respuesta de la AEAT al envío.</returns>
        private RespuestaRegFactuSistemaFacturacion Send(List<InvoiceAction> invoiceActions)
        {

            Utils.Log($"Enviando datos a la AEAT {SellerID} de {invoiceActions.Count} elementos {DateTime.Now}");
            Debug.Print($"Enviando datos a la AEAT {SellerID} de {invoiceActions.Count} elementos {DateTime.Now}");

            var sender = new InvoiceBatch();
            var respuesta = sender.Send(invoiceActions);

            _LastProcessMoment = DateTime.Now;

            if (respuesta != null)
                _CurrentWaitSecods = respuesta.TiempoEsperaEnvio;

            Utils.Log($"Finalizado envío de datos {SellerID} a la AEAT de {invoiceActions.Count} elementos (quedan {_InvoiceActions.Count} registros) {DateTime.Now}");
            Utils.Log($"Establecido momento próxima ejecución {SellerID} (LastProcessMoment: {_LastProcessMoment} + " +
                $"CurrentWaitSecods: {_CurrentWaitSecods}) = {_LastProcessMoment.AddSeconds(_CurrentWaitSecods)}");

            return respuesta;

        }

        /// <summary>
        /// Procesa la respuesta de la AEAT.
        /// </summary>
        /// <param name="aeatResponse">Respuesta a procesar.</param>
        /// <param name="invoiceActions">Elementos a los que corresponde la respuesta.</param>
        private void ProcessReponse(RespuestaRegFactuSistemaFacturacion aeatResponse, List<InvoiceAction> invoiceActions) 
        {

            var invoiceBatch = new InvoiceBatch();
            invoiceBatch.ProcessReponse(aeatResponse, invoiceActions);           
        
        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Si es true indica que el envío ya está permitido
        /// por haberse alcanzado el máximo de registros por lote
        /// establecido por la AEAT.
        /// </summary>
        internal int Count => _InvoiceActions.Count;

        #endregion

        #region Métodos Públicos Estáticos

        /// <summary>
        /// Devuelve la instancia correspondiente a la cola de registros
        /// de un emisor de facturas.
        /// </summary>
        /// <param name="sellerID">Id. del emisor de factura.</param>
        /// <returns>Instancia correspondiente a la cola de registros
        /// de un emisor de facturas.</returns>
        public static SellerQueue Get(string sellerID)
        {

            return GetInstance(sellerID) as SellerQueue;

        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Añade un elemento a la cola de registros
        /// pendientes de envío.
        /// </summary>
        /// <param name="invoiceAction">Acción de registro a añadir.</param>
        internal void Add(InvoiceAction invoiceAction)
        {

            if (invoiceAction.Posted)
                throw new InvalidOperationException($"La operación {invoiceAction}" +
                    $" ya está contabilizada y por lo tanto no se puede agregar a la cola.");

            var busErrors = invoiceAction.GetBusErrors();

            if (busErrors.Count > 0)
                throw new Exception($"No se puede añadir un elemento con errores en validación: {string.Join("\n", busErrors)}");

            // Añado a la cola del emisor
            _InvoiceActions.Enqueue(invoiceAction);

        }

        /// <summary>
        /// Procesa toda la cola emisor a emisor.
        /// </summary>
        /// <returns>Lista de los elementos procesados.</returns>
        internal void Process()
        {

            if (!IsAllowedFrom && !IsAllowedMaxRecordNumber)
                return;

            if (_InvoiceActions.Count == 0)
                return;

            Utils.Log($"Ejecutando cola {this} con {_InvoiceActions.Count} pendientes (IsAllowedFrom={IsAllowedFrom} / IsAllowedMaxRecordNumber={IsAllowedMaxRecordNumber}).");
            Debug.Print($"Ejecutando cola {this} con {_InvoiceActions.Count} pendientes (IsAllowedFrom={IsAllowedFrom} / IsAllowedMaxRecordNumber={IsAllowedMaxRecordNumber}).");

            // Reenvíos a devolver a la cola
            List<InvoiceAction> invoiceRetrySends;

            var postedInvoiceActions = Post(out invoiceRetrySends);

            if (invoiceRetrySends.Count > 0)
            {               

                foreach (var invoiceRetrySend in invoiceRetrySends)
                    _InvoiceActions.Enqueue(invoiceRetrySend);

                Utils.Log($"Devueltos a la cola {this} {invoiceRetrySends.Count} pendientes.");
                Debug.Print($"Devueltos a la cola {this} {invoiceRetrySends.Count} pendientes.");

            }

            RespuestaRegFactuSistemaFacturacion aeatResponse = null;
            Exception sendException = null;

            try
            {

                aeatResponse = Send(postedInvoiceActions);

            }
            catch (Exception ex) 
            {

                sendException = ex;
                Utils.Log($"Error al obtener respuesta de la AEAT {ex}.");
                Debug.Print($"Error al obtener respuesta de la AEAT {ex}.");

            }

            if (sendException == null) 
            {

                ProcessReponse(aeatResponse, postedInvoiceActions);

                // Ejecuto manejador del evento SentFinished en su caso
                if (InvoiceQueue.SentFinished != null)
                    InvoiceQueue.SentFinished(postedInvoiceActions, aeatResponse);

            }
            else 
            {

                // Ejecuto manejador del evento SentError en su caso
                if (InvoiceQueue.SentError != null)
                    InvoiceQueue.SentError(postedInvoiceActions, sendException);


            }

        }

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns>Representación textual de la instancia.</returns>
        public override string ToString()
        {

            return $"{SellerID} ({_InvoiceActions.Count})";

        }

        #endregion

    }

}