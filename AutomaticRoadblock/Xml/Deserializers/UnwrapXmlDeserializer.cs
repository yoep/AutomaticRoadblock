using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using AutomaticRoadblocks.Xml.Attributes;
using AutomaticRoadblocks.Xml.Context;
using AutomaticRoadblocks.Xml.Parser;

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
            var instance = CreateInstance(parser, deserializationContext);
            var properties = deserializationContext.DeserializationType.GetProperties();
            var unwrapProperty = properties.First(IsUnwrapProperty);
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

        private static object CreateInstance(XmlParser parser, XmlDeserializationContext deserializationContext)
        {
            // deserialize the node into a normal object without the unwrap property first
            // this allows us to reuse existing deserializers for types without having to reimplement everything in this deserializer
            return deserializationContext.Deserializers
                .Where(x => x.GetType() != typeof(UnwrapXmlDeserializer))
                .First(x => x.CanHandle(deserializationContext.DeserializationType))
                .Deserialize(parser, deserializationContext);
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
            var xmlRootAttribute = clazz.GetCustomAttribute<XmlRootAttribute>();

            return xmlRootAttribute != null
                ? xmlRootAttribute.ElementName
                : clazz.Name;
        }

        private static bool ContainsUnwrapAttribute(Type clazz)
        {
            return clazz.GetProperties().Any(IsUnwrapProperty);
        }

        private static bool IsUnwrapProperty(MemberInfo x)
        {
            return x.GetCustomAttribute<XmlUnwrapContents>() != null;
        }

        private static bool HasValue(XmlContext xmlDeserializationContext)
        {
            return !string.IsNullOrWhiteSpace(xmlDeserializationContext.CurrentNode.Value);
        }

        private static bool IsRequiredMember(MemberInfo member)
        {
            var xmlProperty = member.GetCustomAttribute<XmlElementAttribute>();

            return xmlProperty is not { IsNullable: true };
        }
    }
}