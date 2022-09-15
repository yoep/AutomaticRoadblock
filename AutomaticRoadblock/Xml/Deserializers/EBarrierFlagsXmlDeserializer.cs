using System;
using System.Xml.XPath;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Xml.Context;
using AutomaticRoadblocks.Xml.Parser;

namespace AutomaticRoadblocks.Xml.Deserializers
{
    public class EBarrierFlagsXmlDeserializer : IXmlDeserializer
    {
        /// <inheritdoc />
        public bool CanHandle(Type type)
        {
            return type == typeof(EBarrierFlags);
        }

        /// <inheritdoc />
        public object Deserialize(XmlParser parser, XmlDeserializationContext deserializationContext)
        {
            var flags = EBarrierFlags.None;
            
            foreach (XPathNavigator flagNode in deserializationContext.CurrentNode.SelectChildren("Flag", ""))
            {
                if (Enum.TryParse<EBarrierFlags>(flagNode.Value, true, out var flag))
                {
                    flags |= flag;
                }
            }
            
            return flags;
        }
    }
}