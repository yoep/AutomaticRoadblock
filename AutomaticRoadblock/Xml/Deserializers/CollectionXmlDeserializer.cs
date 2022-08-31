using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using RazerPoliceLights.Xml.Context;
using AutomaticRoadblocks.Xml;
using AutomaticRoadblocks.Xml.Parser;

namespace RazerPoliceLights.Xml.Deserializers
{
    public class CollectionXmlDeserializer : IXmlDeserializer
    {
        public object Deserialize(XmlParser parser, XmlDeserializationContext deserializationContext)
        {
            var type = deserializationContext.DeserializationType;
            var genericType = type.GetGenericArguments()[0]; //IEnumerable only has 1 generic argument type
            var values = (IList) Activator.CreateInstance(type);
            var deserializer = deserializationContext.Deserializers.Find(e => e.CanHandle(genericType));

            if (deserializer == null)
                throw new XmlException("Could not find deserializer for type " + genericType);

            foreach (XPathNavigator node in deserializationContext.Nodes)
            {
                values.Add(deserializationContext.Deserialize(parser, node, genericType));
            }

            return values;
        }

        public bool CanHandle(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }
    }
}