using System;
using System.Xml.XPath;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Xml.Context;
using AutomaticRoadblocks.Xml.Parser;

namespace AutomaticRoadblocks.Xml.Deserializers
{
    public class ELightSourceFlagsXmlDeserializer : IXmlDeserializer
    {
        /// <inheritdoc />
        public bool CanHandle(Type type)
        {
            return type == typeof(ELightSourceFlags);
        }

        /// <inheritdoc />
        public object Deserialize(XmlParser parser, XmlDeserializationContext deserializationContext)
        {
            var flags = ELightSourceFlags.None;

            foreach (XPathNavigator flagNode in deserializationContext.CurrentNode.SelectChildren("Flag", ""))
            {
                if (Enum.TryParse<ELightSourceFlags>(flagNode.Value, true, out var flag))
                {
                    flags |= flag;
                }
            }

            return flags;
        }
    }
}