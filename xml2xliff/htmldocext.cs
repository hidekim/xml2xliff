using System;
using HtmlAgilityPack;

namespace xml2xliff
{
    public class NodeVisitorHtml
    {
        public NodeVisitorHtml(HtmlNode i1, string i2)
        {
            Item1 = i1;
            Item2 = i2;
        }
        public HtmlNode Item1;
        public string Item2;
    }

    public static class HtmlDocumentExtensions
    {
        public static void IterateThroughAllNodes(
            this HtmlDocument doc,
            Action<NodeVisitorHtml> elementVisitor)
        {
            string xpath = string.Empty;
            if (doc != null && elementVisitor != null)
            {
                foreach (var node in doc.DocumentNode.ChildNodes)
                {
                    var nodeVisitor = new NodeVisitorHtml(node, xpath);
                    doIterateNode(
                        nodeVisitor,
                        elementVisitor);
                }
            }
        }

        private static void doIterateNode(
            NodeVisitorHtml nodeVisitor,
            Action<NodeVisitorHtml> elementVisitor)
        {
            elementVisitor(nodeVisitor);

            var node = nodeVisitor.Item1;
            var xpath = nodeVisitor.Item2;

            int i = 1;
            foreach (var childNode in node.ChildNodes)
            {
                var childNodeVisitor = new NodeVisitorHtml(childNode, xpath + $@"/{node.Name}[{i}]");
                doIterateNode(childNodeVisitor, elementVisitor);
                i++;
            }
        }
    }
}
