using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlContentTranslator
{
    public static class XmlUtils
    {
        private static readonly StringBuilder Sb = new StringBuilder();

        public static bool IsTextNode(this XmlNode node)
        {
            return node != null && node.ChildNodes.Count == 1 && node.ChildNodes[0].NodeType == XmlNodeType.Text;
        }

        public static bool IsParentElement(this XmlNode node)
        {
            return node != null && node.ChildNodes.Count > 0 && !node.IsTextNode() && node.NodeType != XmlNodeType.Comment && node.NodeType != XmlNodeType.CDATA;
        }

        public static string BuildNodePath(XmlNode node)
        {
            Sb.Clear();
            Sb.Append(node.Name);
            if (node.NodeType == XmlNodeType.Attribute)
            {
                XmlNode old = node;
                node = (node as XmlAttribute).OwnerElement; // use OwnerElement for attributes as ParentNode is null
                Sb.Insert(0, node.Name + "@" + GetAttributeIndex(node, old));
                //  node = node.ParentNode;
            }
            while (node.ParentNode != null)
            {
                Sb.Insert(0, node.ParentNode.Name + GetNodeIndex(node) + "/");
                node = node.ParentNode;
            }
            return Sb.ToString();
        }

        public static string GetAttributeIndex(XmlNode node, XmlNode child)
        {
            if (node.Attributes == null)
                return string.Empty;

            int nameCount = 0;
            foreach (XmlAttribute x in node.Attributes)
            {
                if (x.Name == child.Name)
                    nameCount++;
            }

            int i = 0;
            foreach (XmlAttribute x in node.Attributes)
            {
                if (x == node)
                    break;
                i++;
            }
            if (i == 0 || nameCount < 2)
                return string.Empty;
            return string.Format("[{0}]", i);
        }

        public static string GetNodeIndex(XmlNode node)
        {
            int i = 0;
            if (node.NodeType == XmlNodeType.Comment || node.NodeType == XmlNodeType.CDATA)
                return string.Empty;

            if (!string.IsNullOrEmpty(node.NamespaceURI))
            {
                var man = new XmlNamespaceManager(node.OwnerDocument.NameTable);
                man.AddNamespace(node.Prefix, node.NamespaceURI);

                foreach (var x in node.ParentNode.SelectNodes(node.Name, man))
                {
                    if (x == node)
                        break;
                    i++;
                }

            }
            else
            {
                foreach (var x in node.ParentNode.SelectNodes(node.Name))
                {
                    if (x == node)
                        break;
                    i++;
                }
            }

            if (i == 0)
                return string.Empty;
            return string.Format("[{0}]", i);
        }

        public static void ConvertToSelfClosingTags(XmlElement element)
        {
            if (element != null && !element.IsEmpty)
            {
                bool noChildElements = true;
                if (element.HasChildNodes)
                {
                    for (XmlNode node = element.FirstChild; node != null; node = node.NextSibling)
                    {
                        if (node.NodeType == XmlNodeType.Element)
                        {
                            ConvertToSelfClosingTags(node as XmlElement);
                            noChildElements = false;
                        }
                    }
                }
                if (noChildElements && element.InnerText.Length == 0 && element.ParentNode?.ParentNode?.ParentNode != null)
                {
                    element.IsEmpty = true;
                }
            }
        }

    }
}
