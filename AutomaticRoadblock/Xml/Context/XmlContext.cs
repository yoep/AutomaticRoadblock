using System.Xml.XPath;

namespace RazerPoliceLights.Xml.Context
{
    public abstract class XmlContext
    {
        protected XmlContext(XPathDocument document, XPathNavigator currentNode)
        {
            Document = document;
            CurrentNode = currentNode;
            Nodes = null;
        }
        
        protected XmlContext(XPathDocument document, XPathNavigator currentNode, string value)
        {
            Document = document;
            CurrentNode = currentNode;
            Nodes = null;
            Value = value;
        }

        protected XmlContext(XPathDocument document, XPathNodeIterator nodes)
        {
            Document = document;
            CurrentNode = null;
            Nodes = nodes;
        }

        public XPathDocument Document { get; }

        public XPathNavigator CurrentNode { get; }
        
        public XPathNodeIterator Nodes { get; }
        
        public string Value { get; }
    }
}