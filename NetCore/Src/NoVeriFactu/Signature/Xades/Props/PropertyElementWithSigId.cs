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

using System.Xml;

namespace VeriFactu.NoVeriFactu.Signature.Xades.Props
{

    /// <summary>
    /// Representa un elemento xml que contiene
    /// información del id de firma.
    /// </summary>
    internal class PropertyElementWithSigId : PropXmlElement
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Id de firma.
        /// </summary>
        string _SignatureId;

        #endregion

        #region Propiedades Privadas de Instacia

        /// <summary>
        /// Id de la firma.
        /// </summary>
        internal string SignatureId
        {
            get
            {
                return _SignatureId;
            }
            set
            {
                _SignatureId = value;
                SetSignatureId(_SignatureId);
            }
        }

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parent">Nodo padre.</param>
        /// <param name="name">Nombre elemento.</param>
        /// <param name="prefix">Prefijo espacio de nombres.</param>
        /// <param name="nms">Espacio de nombres.</param>
        internal PropertyElementWithSigId(XmlNode parent, string name, string prefix = "xades",
            string nms = VerifactuSignedXml.XadesNamespaceUrl) : base(parent, name, prefix, nms)
        {
        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Añade información del id de firmna.
        /// </summary>
        /// <param name="signatureId">Id de firma.</param>
        protected virtual void SetSignatureId(string signatureId)
        {
            XmlElement.SetAttribute("Target", signatureId);
        }

        #endregion

    }

}