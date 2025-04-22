// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Xml;

namespace VeriFactu.NoVeriFactu.Signature.Ms
{
    internal sealed class MyXmlDocument : XmlDocument
    {
        protected override XmlAttribute CreateDefaultAttribute(string prefix, string localName, string namespaceURI)
        {
            return CreateAttribute(prefix, localName, namespaceURI);
        }
    }
}
