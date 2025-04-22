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
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using VeriFactu.NoVeriFactu.Signature.Ms;

namespace VeriFactu.NoVeriFactu.Signature.Xades
{

    /// <summary>
    /// Proporciona un contenedor en un objeto de firma XML base
    /// para facilitar la creación de firmas XML ampliando la
    /// funcionalidad de 'SignedXml' según las especificaciones
    /// de VERI*FACTU.
    /// </summary>
    internal class VerifactuSignedXml : SignedXml
    {

        #region Variables Privadas de Instancia

        /// <summary>
        /// Url para función hash SHA256
        /// </summary>
        internal const string XmlDsigSHA256Url = "http://www.w3.org/2001/04/xmlenc#sha256";

        /// <summary>
        /// Url para algoritmo de encriptado RSA con función hash SHA256
        /// </summary>
        internal const string XmlDsigRSASHA256Url = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

        /// <summary>
        /// Espacio nombres Xades.
        /// </summary>
        internal const string XadesNamespaceUrl = "http://uri.etsi.org/01903/v1.3.2#";

        #endregion

        #region Propiedades Privadas de Instacia

        /// <summary>
        /// Id de firma.
        /// </summary>
        internal string SignatureId => m_signature.Id;

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Representa un documento xml firmado derivado de la clase
        /// SignedXml para añadir la lógica de firma según las especificaciones
        /// de VERI*FACTU.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="idSignature"></param>
        internal VerifactuSignedXml(XmlDocument document, string idSignature) : base(document)
        {

            m_signature.Id = idSignature;

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Recupera el elemento xml con el atributo Id igual
        /// al valor pasado como argumento en 'idValue' dentro
        /// de la lista de nodos xml 'nodeList'.
        /// </summary>
        /// <param name="nodeList">Lista de nodos xml en la que buscar el elemento.</param>
        /// <param name="idValue">Valor del atributo Id.</param>
        /// <returns>Elemento encontrado con atributo Id igual
        /// a 'idValue' o null si no se encuentra.</returns>
        private XmlElement GetElementById(XmlNodeList nodeList, string idValue)
        {

            XmlElement elem = null;

            foreach (XmlNode node in nodeList)
            {

                if (node?.Attributes?["Id"]?.Value == idValue)
                    return node as XmlElement;

                elem = GetElementById(node.ChildNodes, idValue);

                if (elem != null)
                    return elem;

            }

            return elem;

        }

        /// <summary>
        /// Obtiene el valor de la propiedad privada 'm_context' de la 
        /// clase madre 'SignedXml' utilizando reflection.
        /// </summary>
        /// <returns>Valor de la propiedad privada 'm_context' de la 
        /// clase madre 'SignedXml'.</returns>
        private XmlElement GetContext()
        {

            Type t = typeof(SignedXml);
            FieldInfo m = t.GetField("m_context", BindingFlags.NonPublic | BindingFlags.Instance);
            return m.GetValue(this) as XmlElement;

        }

        /// <summary>
        /// Contruye los elemento xml de las referencias de la firma
        /// calculando las huellas correspondientes a cada una.
        /// Invocamos al método no público de la clase madre 'SignedXml'
        /// con reflection.
        /// </summary>
        private void BuildDigestedReferences()
        {

            Type t = typeof(SignedXml);
            MethodInfo m = t.GetMethod("BuildDigestedReferences", BindingFlags.NonPublic | BindingFlags.Instance);
            m.Invoke(this, new object[] { });

        }

        /// <summary>
        /// Obtiene la huella tras la canonicalización xml.
        /// </summary>
        /// <param name="hash">Huella.</param>
        /// <param name="prefix">Prefijo a incluir en el bloque 'Signature'.</param>
        /// <returns>Huella calculada.</returns>
        private byte[] GetC14NDigest(HashAlgorithm hash, string prefix)
        {


            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.AppendChild(doc.ImportNode(SignedInfo.GetXml(), true));

            var _context = GetContext();

            // Add non default namespaces in scope
            CanonicalXmlNodeList namespaces = (_context == null ? null : Utils.GetPropagatedAttributes(_context));
            Utils.AddNamespaces(doc.DocumentElement, namespaces);

            Transform c14nMethodTransform = SignedInfo.CanonicalizationMethodObject;

            SetPrefix(prefix, doc.DocumentElement);

            c14nMethodTransform.LoadInput(doc);

            return c14nMethodTransform.GetDigestedOutput(hash);

        }

        /// <summary>
        /// Establece la propiedad 'Prefix' entre los
        /// nodos hijos de un nodo y sobre sus nodos
        /// hijos de forma recursiva.
        /// Se realiza el proceso hasta alcanzar el
        /// nodo 'xades:QualifyingProperties'.
        /// </summary>
        /// <param name="prefix">Prefijo a establecer.</param>
        /// <param name="node">Nodo a tratar.</param>
        private void SetPrefix(string prefix, XmlNode node)
        {

            foreach (XmlNode n in node.ChildNodes)
            {

                if (n.Name == "xades:QualifyingProperties")
                    return;

                SetPrefix(prefix, n);

            }

            node.Prefix = prefix;

        }

        /// <summary>
        /// Calcula la firma.
        /// </summary>
        /// <param name="prefix">Prefijo a fijar antes
        /// del computo de la firma.</param>
        internal void ComputeSignature(string prefix)
        {

            BuildDigestedReferences();

            // Load the key
            AsymmetricAlgorithm key = SigningKey;

            if (key == null)
                throw new CryptographicException("SR.Cryptography_Xml_LoadKeyFailed");

            // Check the signature algorithm associated with the key so that we can accordingly set the signature method
            if (SignedInfo.SignatureMethod == null)
            {
                if (key is DSA)
                {
                    SignedInfo.SignatureMethod = XmlDsigDSAUrl;
                }
                else if (key is RSA)
                {
                    // Default to RSA-SHA256
                    SignedInfo.SignatureMethod = SignedInfo.SignatureMethod ?? XmlDsigRSASHA256Url;
                }
                else
                {
                    throw new CryptographicException("SR.Cryptography_Xml_CreatedKeyFailed");
                }
            }

            // See if there is a signature description class defined in the Config file
            SignatureDescription signatureDescription = CryptoHelpers.CreateNonTransformFromName<SignatureDescription>(SignedInfo.SignatureMethod);
            if (signatureDescription == null)
                throw new CryptographicException("SR.Cryptography_Xml_SignatureDescriptionNotCreated");
            HashAlgorithm hashAlg = signatureDescription.CreateDigest();
            if (hashAlg == null)
                throw new CryptographicException("SR.Cryptography_Xml_CreateHashAlgorithmFailed");

            // Updates the HashAlgorithm's state for signing with the signature formatter below.
            // The return value is not needed.
            GetC14NDigest(hashAlg, prefix);

            AsymmetricSignatureFormatter asymmetricSignatureFormatter = signatureDescription.CreateFormatter(key);
            m_signature.SignatureValue = asymmetricSignatureFormatter.CreateSignature(hashAlg);

        }

        /// <summary>
        /// Recupera un elemento xml a partir de la
        /// instancia actual.
        /// </summary>
        /// <param name="prefix">Prefijo a establecer.</param>
        /// <returns>Elemento xml a partir de la
        /// instancia actual.</returns>
        internal XmlElement GetXml(string prefix)
        {

            XmlElement elem = GetXml();
            SetPrefix(prefix, elem);

            return elem;

        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Recupera el elemento xml con el atributo Id igual
        /// al valor pasado como argumento en 'idValue' dentro
        /// del documento xml 'document'.
        /// </summary>
        /// <param name="document">Documento xml en el que buscar el elemento.</param>
        /// <param name="idValue">Valor del atributo Id.</param>
        /// <returns>Elemento encontrado con atributo Id igual
        /// a 'idValue' o null si no se encuentra.</returns>
        public override XmlElement GetIdElement(XmlDocument document, string idValue)
        {

            var elem = base.GetIdElement(document, idValue);

            if (elem != null)
                return elem;

            foreach (DataObject dsObj in Signature.ObjectList)
            {

                var nodeList = dsObj.Data;

                if (nodeList != null)
                    elem = GetElementById(nodeList, idValue);

                if (elem != null)
                    return elem;

            }

            return null;

        }

        #endregion

    }

}