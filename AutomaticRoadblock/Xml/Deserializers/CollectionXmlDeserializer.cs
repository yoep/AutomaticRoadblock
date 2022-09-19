using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using AutomaticRoadblocks.Xml.Context;
using AutomaticRoadblocks.Xml.Parser;

namespace AutomaticRoadblocks.Xml.Deserializers
{
    public class CollectionXmlDeserializer : IXmlDeserializer
    {
        public object Deserialize(XmlParser parser, XmlDeserializationContext deserializationContext)
        {
            var type = deserializationContext.DeserializationType;
            var genericType = type.GetGenericArguments()[0]; // IEnumerable only has 1 generic argument type
            var values = (IList)Activator.CreateInstance(type);
            var deserializer = deserializationContext.Deserializers.Find(e => e.CanHandle(genericType));

            if (deserializer == null)
                throw new XmlException("Could not find deserializer for type " + genericType);

            if (deserializationContext.Nodes == null)
                return values;

            foreach (XPathNavigator node in DetermineNodesToIterate(deserializationContext))
            {
                values.Add(deserializationContext.Deserialize(parser, node, genericType));
            }

            return values;
        }

        public bool CanHandle(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        private XPathNodeIterator DetermineNodesToIterate(XmlDeserializationContext deserializationContext)
        {
            var nodes = deserializationContext.Nodes;

            // verify if there is only one node with the same sub-nodes
            // if so, use the children rather than the parent wrapper of the list
            if (nodes.Count == 1 && ChildrenAllHaveTheSameName(nodes))
            {
                return nodes.Current.SelectChildren(XPathNodeType.Element);
            }

            return deserializationContext.Nodes;
        }

        private bool ChildrenAllHaveTheSameName(XPathNodeIterator parent)
        {
            string nameToCheck = null;
            parent.MoveNext();

            foreach (XPathNavigator child in parent.Current.SelectChildren(XPathNodeType.Element))
            {
                if (nameToCheck == null)
                {
                    nameToCheck = child.Name;
                    continue;
                }

                if (!child.Name.Equals(nameToCheck))
                    return false;
            }

            return true;
        }
    }
}