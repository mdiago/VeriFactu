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
using System.IO;
using VeriFactu.Net;
using VeriFactu.Xml.Factu;

namespace VeriFactu.Business.Operations
{

    /// <summary>
    /// Representa una acción de alta o anulación de registro
    /// en todo lo referente a su gestión contable en la 
    /// cadena de bloques.
    /// </summary>
    public class InvoiceActionPost : InvoiceActionMessage
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Bloqueo para thread safe.
        /// </summary>
        private readonly object _Locker = new object();

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="invoice">Instancia de factura de entrada en el sistema.</param>
        public InvoiceActionPost(Invoice invoice) : base(invoice)
        {

            BlockchainManager = Blockchain.Blockchain.Get(Invoice.SellerID);

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Contabiliza una entrada.
        /// <para> 1. Incluye el registro en la cadena de bloques.</para>
        /// <para> 2. Recalcula Xml con la info Blockchain actualizada.</para>
        /// <para> 3. Guarda el registro en disco en el el directorio de registros emitidos.</para>
        /// <para> 4. Establece Posted = true.</para>
        /// </summary>
        protected void Post()
        {

            // Añadimos el registro de alta (1)
            BlockchainManager.Add(Registro);

            // Actualizamos datos (2,3,4)
            SaveBlockchainChanges();

        }

        /// <summary>
        /// Actualiza los datos tras la incorporación del registro
        /// a la cadena de bloques.
        /// <para> 1. Recalcula Xml con la info Blockchain actualizada.</para>
        /// <para> 2. Guarda el registro en disco en el el directorio de registros emitidos.</para>
        /// <para> 3. Establece Posted = true.</para>
        /// </summary>
        internal void SaveBlockchainChanges() 
        {

            if (Registro.BlockchainLinkID == 0)
                throw new InvalidOperationException($"El registro {Registro}" +
                    $" no está incluido en la cadena de bloques.");

            if (Posted)
                throw new InvalidOperationException($"La operación {this}" +
                    $" ya está contabilizada.");


            // Regeneramos el Xml
            Xml = GetXml();

            // Guardamos el xml
            File.WriteAllBytes(InvoiceFilePath, Xml);

            // Marcamos como contabilizado
            Posted = true;

        }

        /// <summary>
        /// Deshace cambios de guardado de documente eliminando
        /// el elemento de la cadena de bloques y marcando los
        /// archivos relacionados como erróneos.
        /// </summary>
        protected void ClearPost()
        {

            Exception undoException = null;

            lock (_Locker)
            {

                try
                {

                    //Reevierto cambios
                    BlockchainManager.Delete(Registro);

                    if (File.Exists(InvoiceEntryFilePath))
                    {

                        File.Copy(InvoiceEntryFilePath, GeErrorInvoiceEntryFilePath());
                        File.Delete(InvoiceEntryFilePath);

                    }

                    Posted = false;

                }
                catch (Exception ex)
                {

                    undoException = ex;

                }

            }

            if (undoException != null)
                throw new Exception($"Se ha producido un error al intentar descontabilizar" +
                    $" el envío en la cadena de bloques.", undoException);

        }

        /// <summary>
        /// Ejecuta la contabilización del registro.
        /// </summary>
        /// <returns>Si todo funciona correctamente devuelve null.
        /// En caso contrario devuelve una excepción con el error.</returns>
        internal void ExecutePost()
        {

            // Compruebo el certificado
            var cert = Wsd.GetCheckedCertificate();

            if (cert == null)
                throw new Exception("Existe algún problema con el certificado.");

            Exception postException = null;

            lock (_Locker)
            {

                try
                {

                    Post();                   

                }
                catch (Exception ex)
                {

                    postException = ex;

                }

            }

            if (postException != null)
                throw new Exception($"Se ha producido un error al intentar contabilizar" +
                    $" el envío en la cadena de bloques.", postException);

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Gestor de cadena de bloques para el registro.
        /// </summary>
        public Blockchain.Blockchain BlockchainManager { get; private set; }   

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Contabiliza y envía a la AEAT el registro.
        /// </summary>
        public void Save()
        {

            if (IsSaved)
                throw new InvalidOperationException("El objeto InvoiceEntry sólo" +
                    " puede llamar al método Save() una vez.");

            Exception sentException = null;

            ExecutePost();

            try
            {
                ExecuteSend();
                ProcessResponse();
            }
            catch (Exception ex)
            {

                sentException = ex;
                ClearPost();

            }

            if (string.IsNullOrEmpty(CSV) || sentException != null)
                if(sentException == null)
                    ClearPost();

            if (sentException != null)
                throw new Exception($"Se ha producido un error al intentar realizar el envío" +
                    $" o procesar la respuesta.", sentException);

            IsSaved = true;

        }

        #endregion

    }
}