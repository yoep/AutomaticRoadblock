using System;
using AutomaticRoadblocks.Xml.Context;
using AutomaticRoadblocks.Xml.Parser;

namespace AutomaticRoadblocks.Xml.Deserializers
{
    public class StringXmlDeserializer : IXmlDeserializer
    {
        public object Deserialize(XmlParser parser, XmlDeserializationContext deserializationContext)
        {
            return !string.IsNullOrEmpty(deserializationContext.Value)
                ? deserializationContext.Value
                : deserializationContext.CurrentNode.Value;
        }

        public bool CanHandle(Type type)
        {
            return type == typeof(string);
        }
    }
}