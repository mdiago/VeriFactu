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
using VeriFactu.Xml.Factu.Respuesta;

namespace VeriFactu.DataStore
{

    /// <summary>
    /// Representa un conjunto de documentos de factura generado en el sistema. Incluye todos los registros
    /// de alta y anulación envíados, así como todas las respuestas de la AEAT relacionadas con
    /// las facturas de un emisor y periodo determinados.
    /// </summary>
    public class DocumentSet
    {

        #region Variables Privadas Estáticas

        /// <summary>
        /// Información de vendedores y periodos.
        /// </summary>
        static Dictionary<string, List<PeriodOutbox>> _Sellers = Seller.GetSellers();

        #endregion

        #region Propiedades Privadas de Instacia

        /// <summary>
        /// Representa un periodo de facturación de un vendedor o emisor
        /// de facturas en cuanto a los documentos de salida del sistema
        /// en forma de registros de alta o anulación.
        /// </summary>
        internal PeriodOutbox PeriodOutbox { get; private set; }

        /// <summary>
        /// Representa un periodo de facturación de un vendedor o emisor
        /// de facturas en lo que se refiere a documentos de entrada o
        /// respuesta por parte de la AEAT a los envíos.
        /// </summary>
        internal PeriodInbox PeriodInbox { get; private set; }

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        public DocumentSet(string sellerID, string periodID)
        {

            PeriodID = periodID;
            PeriodOutbox = GetPeriodOutbox(sellerID, periodID);
            Seller = PeriodOutbox.Seller;
            PeriodInbox = new PeriodInbox(PeriodOutbox.Seller, periodID, -1);

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Crea un gestor de los documentos de salida
        /// de un periodo y emisor concretos.
        /// </summary>
        /// <param name="sellerID">Identificador fiscal del vencedor (NIF).</param>
        /// <param name="periodID">Periodo de referencia en formato de año de 4 dígitos (Ejemplo '2025').</param>
        /// <returns> Instancia de PeriodOutbox como gestor de los documentos de salida de un periodo.</returns>
        private PeriodOutbox GetPeriodOutbox(string sellerID, string periodID)
        {

            PeriodOutbox periodOutbox = null;

            var periodOutboxes = _Sellers[sellerID];

            foreach (var pOutbox in periodOutboxes)
                if (pOutbox.PeriodID == periodID)
                    periodOutbox = pOutbox;

            return periodOutbox;

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Periodo del documento.
        /// </summary>
        public string PeriodID { get; private set; }

        /// <summary>
        /// Vendedor del documento.
        /// </summary>
        public Seller Seller { get; private set; }

        /// <summary>
        /// Documentos de salida.
        /// </summary>
        public Dictionary<string, DocumentBox> DocumentsOut { get; private set; }

        /// <summary>
        /// Documentos de entrada.
        /// </summary>
        public Dictionary<string, DocumentBox> DocumentsIn { get; private set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Carga todos los documentos de salida y entrada.
        /// </summary>
        public void LoadDocuments()
        {

            DocumentsOut = PeriodOutbox.GetDocuments();
            DocumentsIn = PeriodInbox.GetDocuments();

        }

        /// <summary>
        /// <para> Devuelve los documentos:</para>
        /// <para> Que no tengan una respuesta con estado 'Correcto'</para>
        /// </summary>
        /// <returns>Documentos con registro de salida que no tengan una
        /// respuesta con estado 'Correcto'.</returns>
        public Dictionary<string, DocumentBox> GetErrors()
        {

            if (PeriodOutbox == null)
                throw new InvalidOperationException("El conjunto de datos no tiene documentos cargados.");

            var errors = new Dictionary<string, DocumentBox>();

            foreach (KeyValuePair<string, DocumentBox> outDoc in DocumentsOut)
            {

                if (!DocumentsIn.ContainsKey(outDoc.Key))
                {

                    // No contiene respuesta de la AEAT
                    errors.Add(outDoc.Key, outDoc.Value);

                }
                else
                {

                    var inDoc = DocumentsIn[outDoc.Key];
                    var isError = true;

                    foreach (var record in inDoc.Records)
                    {

                        var respuesta = record as RespuestaLinea;

                        if (respuesta == null)
                            throw new InvalidOperationException($"Error en documento de entrada {record}:" +
                                $" No se trata de una instancia de RespuestaLinea.");

                        if (respuesta.EstadoRegistro == "Correcto")
                        {

                            isError = false;
                            break;

                        }

                    }

                    if (isError) // No contiene respuesta de la AEAT como 'Correcto'
                        errors.Add(outDoc.Key, outDoc.Value);

                }

            }

            return errors;

        }

        /// <summary>
        /// Devuelve las respuestas correspondientes al envío
        /// de un documento.
        /// </summary>
        /// <param name="docKey">Clave del documento de factura.</param>
        /// <returns> Respuestas correspondientes al envío
        /// de un documento.</returns>
        public DocumentBox GetDocumentInbox(string docKey) 
        { 
        
            if(!DocumentsIn.ContainsKey (docKey))
                return null;

            return DocumentsIn[docKey];
        
        }

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns>Representación textual de la instancia.</returns>
        public override string ToString()
        {

            return $"{Seller} ({PeriodID})";

        }

        #endregion

    }

}
