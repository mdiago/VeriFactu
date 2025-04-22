// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Xml;

namespace VeriFactu.NoVeriFactu.Signature.Ms
{
    internal static class Utils
    {
        // The maximum number of characters in an XML document (0 means no limit).
        internal const int MaxCharactersInDocument = 0;

        // The entity expansion limit. This is used to prevent entity expansion denial of service attacks.
        internal const long MaxCharactersFromEntities = (long)1e7;

        private static bool HasNamespace(XmlElement element, string prefix, string value)
        {
            if (IsCommittedNamespace(element, prefix, value)) return true;
            if (element.Prefix == prefix && element.NamespaceURI == value) return true;
            return false;
        }

        // A helper function that determines if a namespace node is a committed attribute
        internal static bool IsCommittedNamespace(XmlElement element, string prefix, string value)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            string name = ((prefix.Length > 0) ? "xmlns:" + prefix : "xmlns");
            if (element.HasAttribute(name) && element.GetAttribute(name) == value) return true;
            return false;
        }

        internal static bool IsRedundantNamespace(XmlElement element, string prefix, string value)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            XmlNode ancestorNode = ((XmlNode)element).ParentNode;
            while (ancestorNode != null)
            {
                XmlElement ancestorElement = ancestorNode as XmlElement;
                if (ancestorElement != null)
                    if (HasNamespace(ancestorElement, prefix, value)) return true;
                ancestorNode = ancestorNode.ParentNode;
            }

            return false;
        }

        internal static bool VerifyAttributes(XmlElement element, string[] expectedAttrNames)
        {
            foreach (XmlAttribute attr in element.Attributes)
            {
                // There are a few Xml Special Attributes that are always allowed on any node. Make sure we allow those here.
                bool attrIsAllowed = attr.Name == "xmlns" || attr.Name.StartsWith("xmlns:") || attr.Name == "xml:space" || attr.Name == "xml:lang" || attr.Name == "xml:base";
                int expectedInd = 0;
                while (!attrIsAllowed && expectedAttrNames != null && expectedInd < expectedAttrNames.Length)
                {
                    attrIsAllowed = attr.Name == expectedAttrNames[expectedInd];
                    expectedInd++;
                }
                if (!attrIsAllowed)
                    return false;
            }
            return true;
        }


        internal static string DiscardWhiteSpaces(string inputBuffer, int inputOffset, int inputCount)
        {
            int i, iCount = 0;
            for (i = 0; i < inputCount; i++)
                if (char.IsWhiteSpace(inputBuffer[inputOffset + i])) iCount++;
            char[] rgbOut = new char[inputCount - iCount];
            iCount = 0;
            for (i = 0; i < inputCount; i++)
                if (!char.IsWhiteSpace(inputBuffer[inputOffset + i]))
                {
                    rgbOut[iCount++] = inputBuffer[inputOffset + i];
                }
            return new string(rgbOut);
        }

       

        internal static XmlReaderSettings GetSecureXmlReaderSettings(XmlResolver xmlResolver)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.XmlResolver = xmlResolver;
            settings.DtdProcessing = DtdProcessing.Parse;
            settings.MaxCharactersFromEntities = MaxCharactersFromEntities;
            settings.MaxCharactersInDocument = MaxCharactersInDocument;
            return settings;
        }

       

        // This removes all children of an element.
        internal static void RemoveAllChildren(XmlElement inputElement)
        {
            XmlNode child = inputElement.FirstChild;
            XmlNode sibling;

            while (child != null)
            {
                sibling = child.NextSibling;
                inputElement.RemoveChild(child);
                child = sibling;
            }
        }

 

        internal static void AddNamespaces(XmlElement elem, CanonicalXmlNodeList namespaces)
        {
            if (namespaces != null)
            {
                foreach (XmlNode attrib in namespaces)
                {
                    string name = ((attrib.Prefix.Length > 0) ? attrib.Prefix + ":" + attrib.LocalName : attrib.LocalName);
                    // Skip the attribute if one with the same qualified name already exists
                    if (elem.HasAttribute(name) || (name.Equals("xmlns") && elem.Prefix.Length == 0)) continue;
                    XmlAttribute nsattrib = (XmlAttribute)elem.OwnerDocument.CreateAttribute(name);
                    nsattrib.Value = attrib.Value;
                    elem.SetAttributeNode(nsattrib);
                }
            }
        }      

        // This method gets the attributes that should be propagated
        internal static CanonicalXmlNodeList GetPropagatedAttributes(XmlElement elem)
        {
            if (elem == null)
                return null;

            CanonicalXmlNodeList namespaces = new CanonicalXmlNodeList();
            XmlNode ancestorNode = elem;
            bool bDefNamespaceToAdd = true;

            while (ancestorNode != null)
            {
                XmlElement ancestorElement = ancestorNode as XmlElement;
                if (ancestorElement == null)
                {
                    ancestorNode = ancestorNode.ParentNode;
                    continue;
                }
                if (!Utils.IsCommittedNamespace(ancestorElement, ancestorElement.Prefix, ancestorElement.NamespaceURI))
                {
                    // Add the namespace attribute to the collection if needed
                    if (!Utils.IsRedundantNamespace(ancestorElement, ancestorElement.Prefix, ancestorElement.NamespaceURI))
                    {
                        string name = ((ancestorElement.Prefix.Length > 0) ? "xmlns:" + ancestorElement.Prefix : "xmlns");
                        XmlAttribute nsattrib = elem.OwnerDocument.CreateAttribute(name);
                        nsattrib.Value = ancestorElement.NamespaceURI;
                        namespaces.Add(nsattrib);
                    }
                }
                if (ancestorElement.HasAttributes)
                {
                    XmlAttributeCollection attribs = ancestorElement.Attributes;
                    foreach (XmlAttribute attrib in attribs)
                    {
                        // Add a default namespace if necessary
                        if (bDefNamespaceToAdd && attrib.LocalName == "xmlns")
                        {
                            XmlAttribute nsattrib = elem.OwnerDocument.CreateAttribute("xmlns");
                            nsattrib.Value = attrib.Value;
                            namespaces.Add(nsattrib);
                            bDefNamespaceToAdd = false;
                            continue;
                        }
                        // retain the declarations of type 'xml:*' as well
                        if (attrib.Prefix == "xmlns" || attrib.Prefix == "xml")
                        {
                            namespaces.Add(attrib);
                            continue;
                        }
                        if (attrib.NamespaceURI.Length > 0)
                        {
                            if (!Utils.IsCommittedNamespace(ancestorElement, attrib.Prefix, attrib.NamespaceURI))
                            {
                                // Add the namespace attribute to the collection if needed
                                if (!Utils.IsRedundantNamespace(ancestorElement, attrib.Prefix, attrib.NamespaceURI))
                                {
                                    string name = ((attrib.Prefix.Length > 0) ? "xmlns:" + attrib.Prefix : "xmlns");
                                    XmlAttribute nsattrib = elem.OwnerDocument.CreateAttribute(name);
                                    nsattrib.Value = attrib.NamespaceURI;
                                    namespaces.Add(nsattrib);
                                }
                            }
                        }
                    }
                }
                ancestorNode = ancestorNode.ParentNode;
            }

            return namespaces;
        }

 
    }

}
