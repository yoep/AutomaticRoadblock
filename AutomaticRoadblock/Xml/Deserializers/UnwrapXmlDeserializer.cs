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
            var nodes = parser.FetchNodesForMember(new XmlDeserializationContext(deserializationContext, clazz,
                GetLookupName(deserializationContext.DeserializationType)));

            if (nodes is { Count: >= 1 } && ChildrenAllHaveTheSameName(nodes))
            {
                ProcessElements(parser, deserializationContext, clazz, unwrapProperty, instance, GetLookupName(deserializationContext.DeserializationType));
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

        private static void ProcessElements(XmlParser parser, XmlDeserializationContext deserializationContext, Type clazz,
            PropertyInfo unwrapProperty, object instance, string lookupName)
        {
            var value = deserializationContext.Deserialize(parser, clazz, lookupName);
            unwrapProperty.SetValue(instance, value);
        }

        private static void ProcessElement(XmlParser parser, XmlDeserializationContext deserializationContext, PropertyInfo unwrapProperty, object instance)
        {
            var stringValue = GetValueForOptionalElementName(parser, deserializationContext) ?? deserializationContext.CurrentNode.Value;

            if (IsRequiredMember(unwrapProperty) && string.IsNullOrWhiteSpace(stringValue))
                throw new XmlException("Missing xml node for " + parser.GetXmlLookupName(unwrapProperty));

            var propertyType = unwrapProperty.PropertyType;
            var deserializer = deserializationContext.Deserializers.First(x => x.CanHandle(propertyType));
            var value = deserializer.Deserialize(parser, new XmlDeserializationContext(deserializationContext, deserializationContext.CurrentNode,
                GetLookupName(deserializationContext.DeserializationType), stringValue, propertyType));

            unwrapProperty.SetValue(instance, value);
        }

        private static string GetValueForOptionalElementName(XmlParser parser, XmlDeserializationContext deserializationContext)
        {
            var lookupName = GetLookupNameFromUnwrapAttribute(deserializationContext.DeserializationType);
            return string.IsNullOrWhiteSpace(lookupName)
                ? null
                : parser.FetchNodeForName(deserializationContext, lookupName)?.Value;
        }

        private static string GetLookupName(Type clazz)
        {
            var xmlRootAttribute = clazz.GetCustomAttribute<XmlRootAttribute>();

            return xmlRootAttribute != null
                ? xmlRootAttribute.ElementName
                : GetLookupNameFromUnwrapAttribute(clazz, clazz.Name);
        }

        private static string GetLookupNameFromUnwrapAttribute(Type clazz, string defaultName = null)
        {
            var unwrapProperty = clazz.GetProperties().First(IsUnwrapProperty);
            var unwrapAttribute = unwrapProperty.GetCustomAttribute<XmlUnwrapContentsAttribute>();

            return string.IsNullOrWhiteSpace(unwrapAttribute.OptionalElementName)
                ? defaultName
                : unwrapAttribute.OptionalElementName;
        }

        private static bool ContainsUnwrapAttribute(Type clazz)
        {
            return clazz.GetProperties().Any(IsUnwrapProperty);
        }

        private static bool IsUnwrapProperty(MemberInfo x)
        {
            return x.GetCustomAttribute<XmlUnwrapContentsAttribute>() != null;
        }

        private static bool IsRequiredMember(MemberInfo member)
        {
            var xmlProperty = member.GetCustomAttribute<XmlElementAttribute>();

            return xmlProperty is not { IsNullable: true };
        }

        private bool ChildrenAllHaveTheSameName(XPathNodeIterator nodes)
        {
            string nameToCheck = null;

            foreach (XPathNavigator node in nodes)
            {
                var children = node.SelectChildren(XPathNodeType.Element);

                // verify if the parent has any children
                // if not, we cannot match it for equal names
                if (children.Count == 0)
                    return false;

                foreach (XPathNavigator child in children)
                {
                    if (nameToCheck == null)
                    {
                        nameToCheck = child.Name;
                        continue;
                    }

                    if (!child.Name.Equals(nameToCheck))
                        return false;
                }
            }

            return true;
        }
    }
}