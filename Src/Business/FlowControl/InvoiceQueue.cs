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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        DateTime _LastProcessMoment;

        int _CurrentWaitSecods;

        /// <summary>
        /// Almacena los registro pendientes de envío.
        /// </summary>
        Dictionary<string, List<InvoiceAction>> _SellerPendingQueue;

        /// <summary>
        /// Almacena los registros envíados y que han resultado erróneos.
        /// </summary>
        Dictionary<string, List<InvoiceAction>> _SellerErrorQueue;

        /// <summary>
        /// Constructor.
        /// </summary>

        InvoiceQueue() 
        {

            _LastProcessMoment = new DateTime(1, 1, 1);
            _SellerPendingQueue = new Dictionary<string, List<InvoiceAction>>();
            _SellerErrorQueue = new Dictionary<string, List<InvoiceAction>>();            

        }

        /// <summary>
        /// Proceso a ejecutar periódicamente entre
        /// intervalos.
        /// </summary>
        public override void Execute()
        {

            try
            {

                if (_LastProcessMoment.AddSeconds(_CurrentWaitSecods) < DateTime.Now)
                    Process();

            }
            catch (Exception ex)
            {

                Debug.Print($"InvoiceQueue error {ex}.");

            }

        }

        /// <summary>
        /// Procesa toda la cola.
        /// </summary>
        public void Process() 
        { 
        
        }


    }
}
