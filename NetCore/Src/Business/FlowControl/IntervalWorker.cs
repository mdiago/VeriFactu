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
using System.Threading;
using VeriFactu.Common;

namespace VeriFactu.Business.FlowControl
{

    /// <summary>
    /// Clase para ejecutar en un hilo de fondo
    /// una tarea de manera periodica.
    /// </summary>
    public class IntervalWorker
    {

        #region Private Members Variables

        /// <summary>
        /// Utilizado para esperar el intervalo determinado,
        /// y para finalizar definitivamente el hilo de trabajo.
        /// </summary>
        ManualResetEvent _End;

        #endregion

        #region Private Methods

        /// <summary>
        /// Procedimiento en bucle en el que ejecuta periódicamente
        /// el método Execute.
        /// </summary>
        private void Process()
        {
            while (true)
            {
                if (_End.WaitOne(Interval))
                    break;

                Execute();
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Intervalo en milegundos entre cada ejecución. Por defecto
        /// 30 segundos.
        /// </summary>
        public int Interval = 5000;

        #endregion

        #region Public Methods

        /// <summary>
        /// Proceso a ejecutar periódicamente entre
        /// intervalos.
        /// </summary>
        public virtual void Execute()
        {
        }

        /// <summary>
        /// Comienza a ejecutar el proceso de manera
        /// continua y periodica hasta que se finaliza
        /// el trabajo con End.
        /// </summary>
        public void Start()
        {
            try
            {
                _End = new ManualResetEvent(false);
                new Thread(Process).Start();
            }
            catch (Exception ex)
            {
                Utils.Log($"Error IntervalWorker.Start:\n{ex.Message}");
            }
        }


        /// <summary>
        /// Finaliza el trabajo.
        /// </summary>
        public void End()
        {
            try
            {
                _End.Set();
            }
            catch (Exception ex)
            {
                Utils.Log($"Error IntervalWorker.End:\n{ex.Message}");
            }
        }

        #endregion

    }

}
