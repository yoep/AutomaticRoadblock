using System;
using System.Collections;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using AutomaticRoadblocks.Xml.Attributes;
using AutomaticRoadblocks.Xml.Context;
using AutomaticRoadblocks.Xml.Parser;
using XmlAttribute = AutomaticRoadblocks.Xml.Attributes.XmlAttribute;
using XmlElement = AutomaticRoadblocks.Xml.Attributes.XmlElement;

namespace AutomaticRoadblocks.Xml.Deserializers
{
    public class ObjectXmlDeserializer : IXmlDeserializer
    {
        public object Deserialize(XmlParser parser, XmlDeserializationContext deserializationContext)
        {
            var instance = Activator.CreateInstance(deserializationContext.DeserializationType);

            foreach (var property in deserializationContext.DeserializationType.GetProperties())
            {
                if (IsIgnored(property) || IsUnwrapAttribute(property))
                    continue;

                if (GetElementAnnotation(property) != null && GetAttributeAnnotation(property) != null)
                    throw new XmlException("Property " + property.Name + " cannot be annotated with both " +
                                           typeof(XmlElement).FullName + " and " + typeof(XmlAttribute).FullName);

                var value = IsXmlElement(property)
                    ? ProcessElement(parser, deserializationContext, property)
                    : ProcessAttribute(parser, deserializationContext, property);

                property.SetValue(instance, value);
            }

            return instance;
        }

        private static object ProcessElement(
            XmlParser parser,
            XmlDeserializationContext deserializationContext,
            PropertyInfo property)
        {
            if (IsArrayOrCollection(property))
            {
                var nodes = parser.FetchNodesForMember(deserializationContext, property);

                if (nodes == null && IsRequiredMember(property))
                    throw new XmlException("Missing xml nodes for " + parser.GetXmlLookupName(property));

                return deserializationContext.Deserialize(parser, nodes, property.PropertyType);
            }

            XPathNavigator node;

            //verify if the node has children, else assume that the current non-attribute property is using the inner contents of the current node
            if (parser.HasNodeChildren(deserializationContext.CurrentNode))
            {
                node = parser.FetchNodeForMember(deserializationContext, property);

                switch (node)
                {
                    case null when IsRequiredMember(property):
                        throw new XmlException("Missing xml node for " + parser.GetXmlLookupName(property));
                    case null:
                        return null;
                }
            }
            else
            {
                node = deserializationContext.CurrentNode;
            }

            return deserializationContext.Deserialize(parser, node, property.PropertyType);
        }

        private static object ProcessAttribute(XmlParser parser, XmlDeserializationContext deserializationContext,
            PropertyInfo property)
        {
            var xmlAttribute = property.GetCustomAttribute<XmlAttribute>();
            var value = parser.FetchAttributeValue(deserializationContext, property);
            var type = property.PropertyType;

            if (string.IsNullOrEmpty(value) && !xmlAttribute.IsOptional)
                throw new XmlException("Missing xml attribute for " + XmlParser.GetXmlAttributeLookupNames(property));

            if (type.IsEnum)
                return ProcessEnum(value, type);

            if (type == typeof(Array))
                throw new DeserializationException("Attribute cannot be of type Array");

            return deserializationContext.Deserialize(parser, value, type);
        }

        private static object ProcessEnum(string value, Type type)
        {
            foreach (var enumValue in type.GetEnumValues())
            {
                if (enumValue.ToString().Equals(value, StringComparison.InvariantCultureIgnoreCase))
                    return enumValue;
            }

            throw new XmlException("Enumeration value " + value + " could not be found for " + type.Name);
        }

        public bool CanHandle(Type type)
        {
            return !IsNativeType(type);
        }

        private static bool IsRequiredMember(MemberInfo member)
        {
            var xmlProperty = member.GetCustomAttribute<XmlElement>();

            return xmlProperty == null || !xmlProperty.IsOptional;
        }

        private static bool IsIgnored(MemberInfo member)
        {
            return member.GetCustomAttribute<XmlIgnore>() != null;
        }
        
        private static bool IsUnwrapAttribute(MemberInfo member)
        {
            return member.GetCustomAttribute<XmlUnwrapContents>() != null;
        }

        private static bool IsNativeType(Type type)
        {
            var ns = type.Namespace;
            return !string.IsNullOrEmpty(ns) && ns.Equals("System");
        }

        private static bool IsXmlElement(MemberInfo member)
        {
            return GetAttributeAnnotation(member) == null;
        }

        private static bool IsArrayOrCollection(PropertyInfo property)
        {
            var type = property.PropertyType;
            return !IsNativeType(type)
                   && (type.IsArray || typeof(IEnumerable).IsAssignableFrom(type));
        }

        private static XmlAttribute GetAttributeAnnotation(MemberInfo member)
        {
            return member.GetCustomAttribute<XmlAttribute>();
        }

        private static XmlElement GetElementAnnotation(MemberInfo member)
        {
            return member.GetCustomAttribute<XmlElement>();
        }
    }
}