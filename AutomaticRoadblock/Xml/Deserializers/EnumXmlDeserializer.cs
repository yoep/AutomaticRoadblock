using System;
using AutomaticRoadblocks.Xml.Context;
using AutomaticRoadblocks.Xml.Parser;

namespace AutomaticRoadblocks.Xml.Deserializers
{
    public class EnumXmlDeserializer : IXmlDeserializer
    {
        /// <inheritdoc />
        public bool CanHandle(Type type)
        {
            return type.IsEnum;
        }

        /// <inheritdoc />
        public object Deserialize(XmlParser parser, XmlDeserializationContext deserializationContext)
        {
            var value = !string.IsNullOrEmpty(deserializationContext.Value)
                ? deserializationContext.Value
                : deserializationContext.CurrentNode.Value;

            return Enum.Parse(deserializationContext.DeserializationType, UpperCaseFirstLetter(value));
        }

        private string UpperCaseFirstLetter(string value)
        {
            return char.ToUpper(value[0]) + value.Substring(1);
        }
    }
}