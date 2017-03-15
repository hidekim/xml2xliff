using System;
using System.Xml;

namespace xml2xliff
{
    public class NodeVisitorXml
    {
        public NodeVisitorXml(XmlNode i1, string i2)
        {
            Item1 = i1;
            Item2 = i2;
        }
        public XmlNode Item1;
        public string Item2;
    }

    public static class XmlDocumentExtensions
    {
        public static void IterateThroughAllNodes(
            this XmlDocument doc,
            Action<NodeVisitorXml> elementVisitor)
        {
            string xpath = string.Empty;
            if (doc != null && elementVisitor != null)
            {
                foreach (XmlNode node in doc.ChildNodes)
                {
                    var nodeVisitor = new NodeVisitorXml(node, xpath);
                    doIterateNode(
                        nodeVisitor,
                        elementVisitor);
                }
            }
        }

        private static void doIterateNode(
            NodeVisitorXml nodeVisitor,
            Action<NodeVisitorXml> elementVisitor)
        {
            elementVisitor(nodeVisitor);

            var node = nodeVisitor.Item1;
            var xpath = nodeVisitor.Item2;

            int i = 1;
            foreach (XmlNode childNode in node.ChildNodes)
            {
                var childNodeVisitor = new NodeVisitorXml(childNode, xpath + $@"/{node.Name}[{i}]");
                doIterateNode(childNodeVisitor, elementVisitor);
                i++;
            }
        }
    }
}
