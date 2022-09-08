using System;
using System.Globalization;
using AutomaticRoadblocks.Xml.Context;
using AutomaticRoadblocks.Xml.Parser;

namespace AutomaticRoadblocks.Xml.Deserializers
{
    public class Int32Deserializer  : IXmlDeserializer
    {
        /// <inheritdoc />
        public bool CanHandle(Type type)
        {
            return type == typeof(int);
        }

        /// <inheritdoc />
        public object Deserialize(XmlParser parser, XmlDeserializationContext deserializationContext)
        {
            return !string.IsNullOrEmpty(deserializationContext.Value)
                ? int.Parse(deserializationContext.Value, CultureInfo.InvariantCulture)
                : deserializationContext.CurrentNode.ValueAsInt;
        }
    }
}