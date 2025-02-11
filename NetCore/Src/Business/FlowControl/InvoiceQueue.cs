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

using System;
using System.Collections.Generic;
using VeriFactu.Business.Operations;
using VeriFactu.Common;
using VeriFactu.Net;
using VeriFactu.Xml.Factu.Respuesta;

namespace VeriFactu.Business.FlowControl
{

    /// <summary>
    /// Gestiona el envío de registros a la AEAT con los intervalos de
    /// espera establecidos entre envíos.
    /// <para> Según lo establecido en la Orden HAC/1177/2024, de 17 de octubre
    ///  en su Artículo 16.</para>
    /// </summary>
    public class InvoiceQueue : IntervalWorker
    {

        #region Variables Privadas Estáticas

        /// <summary>
        /// Indica si la cola se está cerrando.
        /// </summary>
        static bool _IsClosing;

        #endregion

        #region Variables Privadas de Instancia

        /// <summary>
        /// Almacena los registro pendientes de envío.
        /// </summary>
        readonly Dictionary<string, SellerQueue> _SellerPendingQueue;

        /// <summary>
        /// Bloqueo para thread safe.
        /// </summary>
        private readonly object _Locker = new object();

        /// <summary>
        /// Indica si la cola se está procesando.
        /// </summary>
        bool _IsWorking;

        #endregion

        #region Construtores Estáticos

        /// <summary>
        /// Constructor.
        /// </summary>
        static InvoiceQueue() 
        {

            ActiveInvoiceQueue = GetInstance();
            ActiveInvoiceQueue.Start();

        }

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        InvoiceQueue()
        {

            if (ActiveInvoiceQueue != null)
                throw new InvalidOperationException("Ya existe una instancia de gestor de cola actualmente en el sistema." +
                    " Únicamente puede existir una instancia creada de esta clase.");

            _SellerPendingQueue = new Dictionary<string, SellerQueue>();

            ActiveInvoiceQueue = this;

        }

        #endregion

        #region Métodos Privados Estáticos

        /// <summary>
        /// Recupera la instancia que se encarga de gestionar
        /// la cola de facturas pendientes de envío.
        /// </summary>
        /// <returns> Instancia que se encarga de gestionar
        /// la cola de facturas pendientes de envío.</returns>
        private static InvoiceQueue GetInstance() 
        {

            if (ActiveInvoiceQueue == null)
                ActiveInvoiceQueue = new InvoiceQueue();

            return ActiveInvoiceQueue;

        }

        /// <summary>
        /// Finaliza la revisión de cola.
        /// </summary>
        public static void DoExit()
        {

            if (ActiveInvoiceQueue.Count > 0)
                throw new OperationCanceledException($"No se puede finalizar la cola de factura con" +
                    $" {ActiveInvoiceQueue.Count} documentos pendientes. Debe estar vacía.");

            ActiveInvoiceQueue.End();

            Utils.Log($"Finalizado cierre de cola {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");

            ActiveInvoiceQueue = null;

        }


        #endregion       

        #region Propiedades Públicas Estáticas

        /// <summary>
        /// Almacena el gestor de cola actualmente activo en
        /// el sitema.
        /// </summary>
        public static InvoiceQueue ActiveInvoiceQueue { get; private set; }

        /// <summary>
        /// Acción a ejecutar en la finalización del envío.
        /// </summary>
        public static Action<List<InvoiceAction>, RespuestaRegFactuSistemaFacturacion> SentFinished { get; set; }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Número total de elementos en cola.
        /// </summary>
        public int Count 
        { 
            get 
            {

                int count = 0;

                foreach (KeyValuePair<string, SellerQueue> kvp in _SellerPendingQueue) 
                    count = count + kvp.Value.Count;

                return count; 

            } 
        }

        #endregion

        #region Métodos Públicos Estáticos

        /// <summary>
        /// Finaliza la revisión de cola.
        /// </summary>
        public static void Exit() 
        {

            _IsClosing = true;

            Utils.Log($"Iniciado cierre de cola {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");

        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Añade un elemento a la cola de registros
        /// pendientes de envío.
        /// </summary>
        /// <param name="invoiceAction">Acción de registro a añadir.</param>
        public void Add(InvoiceAction invoiceAction) 
        {

            if (_IsClosing)
                throw new InvalidOperationException("No se pueden añadir" +
                    " elementos ya que se ha establecido el cierre.");

            if (invoiceAction.Posted)
                throw new InvalidOperationException($"La operación {invoiceAction}" +
                    $" ya está contabilizada y por lo tanto no se puede agregar a la cola.");

            var busErrors = invoiceAction.GetBusErrors();

            if (busErrors.Count > 0)
                throw new Exception($"No se puede añadir un elemento con errores en validación: {string.Join("\n", busErrors)}");

            var queue = SellerQueue.Get(invoiceAction.SellerID);

            queue.Add(invoiceAction);

            if (!_SellerPendingQueue.ContainsKey(invoiceAction.SellerID))
                _SellerPendingQueue.Add(invoiceAction.SellerID, queue);

        }

        /// <summary>
        /// Proceso a ejecutar periódicamente entre
        /// intervalos.
        /// </summary>
        public override void Execute()
        {

            try
            {

                Process();

            }
            catch (Exception ex)
            {

                Utils.Log($"InvoiceQueue error {ex}.");

            }

        }

        /// <summary>
        /// Procesa toda la cola emisor a emisor.
        /// </summary>
        public void Process()
        {

            if (_IsWorking)
                return;             

            // Compruebo el certificado
            var cert = Wsd.GetCheckedCertificate();

            if (cert == null)
                Utils.Log("Existe algún problema con el certificado.");

            if(_IsClosing)
                Utils.Log($"Ejecutando cola con cierre establecido {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");

            Exception processException = null;

            lock (_Locker)
                _IsWorking = true;

            foreach (KeyValuePair<string, SellerQueue> kvpInvoiceAction in _SellerPendingQueue)
            {                

                lock (_Locker)
                {

                    try
                    {

                        kvpInvoiceAction.Value.Process();

                    }
                    catch (Exception ex)
                    {

                        processException = ex;

                    }

                }

                if (processException != null)
                    Utils.Log($"Error procesando InvoiceQueue: {processException}");
                
                break;

            }            

            lock (_Locker)
                _IsWorking = false;

            if (_IsClosing && ActiveInvoiceQueue.Count == 0)
                DoExit();

        }

        #endregion

    }

}