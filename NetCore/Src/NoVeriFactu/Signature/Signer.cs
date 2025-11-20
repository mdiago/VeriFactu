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
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using VeriFactu.NoVeriFactu.Signature.Xades;
using VeriFactu.NoVeriFactu.Signature.Xades.Props;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;

namespace VeriFactu.NoVeriFactu.Signature
{

    /// <summary>
    /// Clase responsable de la firma de ficheros de 
    /// registros en arreglo a las especificaciones
    /// de VERI*FACTU.
    /// </summary>
    public class Signer
    {

        #region Variables Privadas Estáticas

        /// <summary>
        /// Tipo de proveedor para RSA.
        /// </summary>
        internal readonly static int PROV_RSA_AES = 24;

        #endregion

        #region Propiedades Privadas de Instacia

        /// <summary>
        /// Información de la clave del certificado para la firma.
        /// </summary>
        private AsymmetricAlgorithm SigningKey { get; set; }

        /// <summary>
        /// Objeto KeyInfo de la firma.
        /// </summary>
        private KeyInfo KeyInfo { get; set; }

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="certificate">Certificado para realizar las firmas.</param>
        public Signer(X509Certificate2 certificate)
        {

            Certificate = certificate;
            SigningKey = GetCertificateKey(Certificate);
            KeyInfo = GetKeyInfo(Certificate);

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Devuelve una nueva instancia de VerifactuSignedXml
        /// basada en el documento xml de entrada.
        /// </summary>
        /// <param name="xmlDocument">Documento a firmar.</param>
        /// <returns>Nueva instancia de VerifactuSignedXml
        /// basada en el documento xml de entrada.</returns>
        private VerifactuSignedXml GetVerifactuSignedXml(XmlDocument xmlDocument)
        {

            var signatureId = GetSignatureId();

            var signedXml = new VerifactuSignedXml(xmlDocument, $"xmldsig-{signatureId}");
            signedXml.KeyInfo = KeyInfo;
            signedXml.SigningKey = SigningKey;

            var refInXml = GetInXmlReference(signatureId);
            signedXml.AddReference(refInXml);

            var refSignedProperties = GetSignedPropertiesReference(signatureId);
            signedXml.AddReference(refSignedProperties);

            return signedXml;

        }

        /// <summary>
        /// Obtiene un nuevo identificador para una firma.
        /// </summary>
        /// <returns></returns>
        private string GetSignatureId()
        {

            return $"{Guid.NewGuid()}";

        }

        /// <summary>
        /// Devuelve una clave válida para la firma con SA256.
        /// </summary>
        /// <param name="certificate">Certificado del que obtener la clave.</param>
        /// <returns>Clave válida para firma SA256.</returns>
        private AsymmetricAlgorithm GetCertificateKey(X509Certificate2 certificate)
        {

#if LE_461

            RSA src = RSACertificateExtensions.GetRSAPrivateKey(certificate);
            if (src is null) throw new CryptographicException("El certificado no contiene una clave RSA privada.");
            var p = src.ExportParameters(includePrivateParameters: true);
            var clone = new RSACng();
            clone.ImportParameters(p);

            return clone;

#else
            return certificate.GetRSAPrivateKey();
#endif
            
        }

        /// <summary>
        /// Devuelve el objeto KeyInfo de la firma adecuado
        /// para el certificado de entrada.
        /// </summary>
        /// <param name="certificate">Certificado.</param>
        /// <returns>KeyInfo de la firma.</returns>
        private KeyInfo GetKeyInfo(X509Certificate2 certificate)
        {

            KeyInfo keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(certificate, X509IncludeOption.WholeChain)); // Añade elemento KeyInfo.X509Data.RSAKeyValue.X509Certificate
            keyInfo.AddClause(new RSAKeyValue(certificate.GetRSAPublicKey()));  // Añade elemento KeyInfo.KeyValue.RSAKeyValue

            return keyInfo;

        }

        /// <summary>
        /// Devuelve la referencia correspondiente al documento
        /// xml sin firmar de entrada.
        /// </summary>
        /// <param name="signatureId">Identificador de la firma.</param>
        /// <returns>Referencia a incluir en la firma.</returns>
        private Reference GetInXmlReference(string signatureId)
        {

            Reference reference = new Reference();
            reference.Id = $"xmldsig-{signatureId}-ref0";
            reference.Uri = "";
            reference.DigestMethod = VerifactuSignedXml.XmlDsigSHA256Url;
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform()); // Enveloped           

            return reference;

        }

        /// <summary>
        /// Devuelve la referencia correspondiente a las 
        /// propiedades adicionales de la firma.
        /// </summary>
        /// <param name="signatureId">Identificador de la firma.</param>
        /// <returns>Referencia a incluir en la firma con la propiedades
        /// adicionales al documento de entrada a firmar.</returns>
        private Reference GetSignedPropertiesReference(string signatureId)
        {

            Reference reference = new Reference();
            reference.Type = "http://uri.etsi.org/01903#SignedProperties";
            reference.Uri = $"#xmldsig-{signatureId}-signedprops";
            reference.DigestMethod = VerifactuSignedXml.XmlDsigSHA256Url;
            reference.AddTransform(new XmlDsigC14NTransform());

            return reference;

        }

        /// <summary>
        /// Devuelve los datos binarios de una cadena con el documentos xml de
        /// entrada firmado.
        /// </summary>
        /// <param name="xmlDocument">Documento xml a firmar.</param>
        /// <param name="name">Nombre elemento.</param>
        /// <returns>Datos binarios xml firmado.</returns>
        private byte[] Sign(XmlDocument xmlDocument, string name = "RegistroAlta")
        {

            xmlDocument.PreserveWhitespace = true;

            var signedXml = GetVerifactuSignedXml(xmlDocument);

            var rootTmp = new RootTmp(xmlDocument, name);

            rootTmp.SignatureTmp.Object.QualifyingProperties.SignatureId = signedXml.SignatureId;
            rootTmp.SignatureTmp.Object.QualifyingProperties.SignedProperties.SignatureId = signedXml.SignatureId;
            rootTmp.SignatureTmp.Object.QualifyingProperties.SignedProperties.SignedDataObjectProperties.DataObjectFormat.SignatureId = signedXml.SignatureId;

            rootTmp.SignatureTmp.Object.QualifyingProperties.SignedProperties.SignedSignatureProperties.SigningCertificate.Cert.Certificate = Certificate;

            // Añado el objeto
            signedXml.AddObject(rootTmp.GetDataObject());

            //Calcula firma digital XML
            signedXml.ComputeSignature("ds");

            // Get the XML representation of the signature and save
            // it to an XmlElement object.
            XmlElement xmlDigitalSignature = signedXml.GetXml("ds");

            // Actualizo prefix en Object
            foreach (XmlElement elem in xmlDigitalSignature.ChildNodes)
                if (elem.Name == "Object")
                    elem.Prefix = "ds";

            // Append the element to the XML document.
            xmlDocument.DocumentElement.AppendChild(xmlDocument.ImportNode(xmlDigitalSignature, true));

            if (xmlDocument.FirstChild is XmlDeclaration)
                xmlDocument.RemoveChild(xmlDocument.FirstChild);

            byte[] xmlSigned = null;

            using (var msXmlSigned = new MemoryStream())
            {

                XmlTextWriter xmltw = new XmlTextWriter(msXmlSigned, new UTF8Encoding(false));
                xmlDocument.WriteTo(xmltw);
                xmltw.Close();

                xmlSigned = msXmlSigned.ToArray();

            }

            return xmlSigned;

        }

#endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Certificado para la firma.
        /// </summary>
        public X509Certificate2 Certificate { get; private set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Devuelve los datos binarios de una cadena con el documentos xml de
        /// entrada firmado.
        /// </summary>
        /// <param name="registro">Registro a firmar.</param>
        /// <returns>XML firmado.</returns>
        public byte[] Sign(Registro registro)
        {

#if !LE_461 && !LE_472 && !LE_480

            throw new NotImplementedException("La funcionalidad de firma sólo" +
                " está disponible para proyectos .NET Framework 4.6.1 o 4.7.2 o 4.8");

#endif

            var name = registro.GetType().Name;

            var nms = new Dictionary<string, string>()
            {
                { "sum1",       Namespaces.NamespaceSF },
            };

            // Firmamos el registro de alta
            var xml = new XmlParser().GetBytes(registro, nms);

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(System.Text.Encoding.UTF8.GetString(xml));            

            return Sign(xmlDocument, name);

        }

        #endregion

    }

}