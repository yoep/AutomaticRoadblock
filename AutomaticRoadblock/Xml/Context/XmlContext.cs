using System.Xml.XPath;

namespace AutomaticRoadblocks.Xml.Context
{
    public abstract class XmlContext
    {
        private readonly string _xPath;

        protected XmlContext(XPathDocument document, XPathNavigator currentNode, string lookupName)
        {
            Document = document;
            CurrentNode = currentNode;
            _xPath = ToXPath(currentNode);
            LookupName = lookupName;
        }

        protected XmlContext(XPathDocument document, XPathNavigator currentNode, string lookupName, string value = null)
        {
            Document = document;
            CurrentNode = currentNode;
            _xPath = ToXPath(currentNode);
            LookupName = lookupName;
            Value = value;
        }

        /// <summary>
        /// The XML document that is being parsed.
        /// </summary>
        public XPathDocument Document { get; }

        public XPathNavigator CurrentNode { get; }

        /// <summary>
        /// The current xpath navigation of the context.
        /// </summary>
        public string XPath => _xPath + LookupName;

        /// <summary>
        /// The current lookup name for the content in the <see cref="CurrentNode"/>.
        /// </summary>
        public string LookupName { get; }

        /// <summary>
        /// The current value of the node context.
        /// </summary>
        public string Value { get; }

        private static string ToXPath(XPathNavigator currentNode)
        {
            var xPath = "";
            var nodes = currentNode.SelectAncestors(XPathNodeType.Element, true);

            if (nodes.Count == 1)
            {
                return "/";
            }

            foreach (XPathNavigator node in nodes)
            {
                xPath = $"{node.Name}/" + xPath;
            }

            return xPath;
        }
    }
}