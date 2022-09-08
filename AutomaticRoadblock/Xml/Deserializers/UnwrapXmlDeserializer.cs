using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using AutomaticRoadblocks.Xml.Attributes;
using AutomaticRoadblocks.Xml.Context;
using AutomaticRoadblocks.Xml.Parser;
using XmlElement = AutomaticRoadblocks.Xml.Attributes.XmlElement;

namespace AutomaticRoadblocks.Xml.Deserializers
{
    public class UnwrapXmlDeserializer : IXmlDeserializer
    {
        /// <inheritdoc />
        public bool CanHandle(Type type)
        {
            return ContainsUnwrapAttribute(type);
        }

        /// <inheritdoc />
        public object Deserialize(XmlParser parser, XmlDeserializationContext deserializationContext)
        {
            var instance = Activator.CreateInstance(deserializationContext.DeserializationType);
            var unwrapProperty = deserializationContext.DeserializationType.GetProperties().First(HasUnwrapAttribute);
            var clazz = unwrapProperty.PropertyType;
            var nodes = parser.FetchNodesForMember(deserializationContext, unwrapProperty, GetLookupName(deserializationContext.DeserializationType));

            if (nodes != null)
            {
                ProcessElements(parser, deserializationContext, nodes, clazz, unwrapProperty, instance);
            }
            else
            {
                ProcessElement(parser, deserializationContext, unwrapProperty, instance);
            }

            return instance;
        }

        private static void ProcessElements(XmlParser parser, XmlDeserializationContext deserializationContext, XPathNodeIterator nodes, Type clazz,
            PropertyInfo unwrapProperty, object instance)
        {
            var value = deserializationContext.Deserialize(parser, nodes, clazz);
            unwrapProperty.SetValue(instance, value);
        }

        private static void ProcessElement(XmlParser parser, XmlDeserializationContext deserializationContext, PropertyInfo unwrapProperty, object instance)
        {
            if (IsRequiredMember(unwrapProperty) && !HasValue(deserializationContext))
                throw new XmlException("Missing xml node for " + parser.GetXmlLookupName(unwrapProperty));
            
            unwrapProperty.SetValue(instance, deserializationContext.CurrentNode.Value);
        }

        private static string GetLookupName(Type clazz)
        {
            var xmlRootAttribute = clazz.GetCustomAttribute<XmlRootName>();

            return xmlRootAttribute != null
                ? xmlRootAttribute.Name
                : clazz.Name;
        }

        private static bool ContainsUnwrapAttribute(Type clazz)
        {
            return clazz.GetProperties().Any(HasUnwrapAttribute);
        }

        private static bool HasUnwrapAttribute(MemberInfo x)
        {
            return x.GetCustomAttribute<XmlUnwrapContents>() != null;
        }

        private static bool HasValue(XmlContext xmlDeserializationContext)
        {
            return !string.IsNullOrWhiteSpace(xmlDeserializationContext.CurrentNode.Value);
        }

        private static bool IsRequiredMember(MemberInfo member)
        {
            var xmlProperty = member.GetCustomAttribute<XmlElement>();

            return xmlProperty is not { IsOptional: true };
        }
    }
}