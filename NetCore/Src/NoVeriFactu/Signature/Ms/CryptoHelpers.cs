// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Modificado por Manuel Diago García 2025-04-18.

using System;
using System.Collections.Generic;
using System.Diagnostics;
#if NETCOREAPP
using System.Runtime.CompilerServices;
#endif
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;

namespace VeriFactu.NoVeriFactu.Signature.Ms
{
    internal static class CryptoHelpers
    {

        private static readonly char[] _invalidChars = new char[] { ',', '`', '[', '*', '&' };

        private static object CreateFromKnownName(string name)
        {

            var map = new Dictionary<string, object>()
            {
                { "http://www.w3.org/TR/2001/REC-xml-c14n-20010315" , new XmlDsigC14NTransform() },
                {"http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments" , new XmlDsigC14NWithCommentsTransform()},
                {"http://www.w3.org/2001/10/xml-exc-c14n#" , new XmlDsigExcC14NTransform()},
                {"http://www.w3.org/2001/10/xml-exc-c14n#WithComments" , new XmlDsigExcC14NWithCommentsTransform()},
                {"http://www.w3.org/2000/09/xmldsig#base64" , new XmlDsigBase64Transform()},
                {"http://www.w3.org/TR/1999/REC-xpath-19991116" , new XmlDsigXPathTransform()},
                {"http://www.w3.org/TR/1999/REC-xslt-19991116" , CreateXmlDsigXsltTransform()},
                {"http://www.w3.org/2000/09/xmldsig#enveloped-signature" , new XmlDsigEnvelopedSignatureTransform()},
                {"http://www.w3.org/2002/07/decrypt#XML" , new XmlDecryptionTransform()},
                {"urn:mpeg:mpeg21:2003:01-REL-R-NS:licenseTransform" , new XmlLicenseTransform()},
                {"http://www.w3.org/2000/09/xmldsig# X509Data" , new KeyInfoX509Data()},
                {"http://www.w3.org/2000/09/xmldsig# KeyName" , new KeyInfoName()},
#pragma warning disable CA1416 // This call site is reachable on all platforms. 'DSAKeyValue' is unsupported on: 'ios', 'maccatalyst', 'tvos'
                {"http://www.w3.org/2000/09/xmldsig# KeyValue/DSAKeyValue" , new DSAKeyValue()},
#pragma warning restore CA1416
                {"http://www.w3.org/2000/09/xmldsig# KeyValue/RSAKeyValue" , new RSAKeyValue()},
                {"http://www.w3.org/2000/09/xmldsig# RetrievalMethod" , new KeyInfoRetrievalMethod()},
                {"http://www.w3.org/2001/04/xmlenc# EncryptedKey" , new KeyInfoEncryptedKey()},
                {"http://www.w3.org/2000/09/xmldsig#dsa-sha1" , new DSASignatureDescription()},
                {"System.Security.Cryptography.DSASignatureDescription" , new DSASignatureDescription()},
                {"http://www.w3.org/2000/09/xmldsig#rsa-sha1" , new RSAPKCS1SHA1SignatureDescription()},
                {"System.Security.Cryptography.RSASignatureDescription" , new RSAPKCS1SHA1SignatureDescription()},
                {"http://www.w3.org/2001/04/xmldsig-more#rsa-sha256" , new RSAPKCS1SHA256SignatureDescription()},
                {"http://www.w3.org/2001/04/xmldsig-more#rsa-sha384" , new RSAPKCS1SHA384SignatureDescription()},
                {"http://www.w3.org/2001/04/xmldsig-more#rsa-sha512" , new RSAPKCS1SHA512SignatureDescription()}
            };

            if (map.ContainsKey(name))
                return map[name];
            
            return null;

        }
        
        private static XmlDsigXsltTransform CreateXmlDsigXsltTransform()
        {
#if NETCOREAPP
            if (!RuntimeFeature.IsDynamicCodeSupported)
            {
                // XSLTs are only supported when dynamic code is supported. See https://github.com/dotnet/runtime/issues/84389
                throw new NotSupportedException("SR.Cryptography_Xml_XsltRequiresDynamicCode");
            }
#endif

            return new XmlDsigXsltTransform();
        }

        public static T CreateFromName<T>(string name) where T : class
        {
            if (name == null || name.IndexOfAny(_invalidChars) >= 0)
            {
                return null;
            }
            try
            {
                return (CryptoConfig.CreateFromName(name) ?? CreateFromKnownName(name)) as T;
            }
#if NETCOREAPP
            catch (NotSupportedException)
            {
                if (name == "http://www.w3.org/TR/1999/REC-xslt-19991116")
                {
                    // allow XSLT NotSupportedException to be thrown
                    throw;
                }
                return null;
            }
#endif
            catch (Exception)
            {
                return null;
            }
        }

        public static T CreateNonTransformFromName<T>(string name) where T : class
        {
            Debug.Assert(!typeof(Transform).IsAssignableFrom(typeof(T)));

            return CreateFromName<T>(name);
        }
    }
}
