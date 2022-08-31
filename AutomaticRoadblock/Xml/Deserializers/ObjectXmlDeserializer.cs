using System;
using System.Collections;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using RazerPoliceLights.Xml.Attributes;
using RazerPoliceLights.Xml.Context;
using AutomaticRoadblocks.Xml;
using AutomaticRoadblocks.Xml.Parser;

namespace RazerPoliceLights.Xml.Deserializers
{
    public class ObjectXmlDeserializer : IXmlDeserializer
    {
        public object Deserialize(XmlParser parser, XmlDeserializationContext deserializationContext)
        {
            var instance = Activator.CreateInstance(deserializationContext.DeserializationType);

            foreach (var property in deserializationContext.DeserializationType.GetProperties())
            {
                if (!IsNotIgnored(property))
                    continue;

                if (GetElementAnnotation(property) != null && GetAttributeAnnotation(property) != null)
                    throw new XmlException("Property " + property.Name + " cannot be annotated with both " +
                                           typeof(Attributes.XmlElement).FullName + " and " + typeof(Attributes.XmlAttribute).FullName);

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

                if (node == null && IsRequiredMember(property))
                    throw new XmlException("Missing xml node for " + parser.GetXmlLookupName(property));
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
            var xmlAttribute = property.GetCustomAttribute<Attributes.XmlAttribute>();
            var value = parser.FetchAttributeValue(deserializationContext, property);
            var type = property.PropertyType;

            if (string.IsNullOrEmpty(value) && !xmlAttribute.IsOptional)
                throw new XmlException("Missing xml attribute for " + parser.GetXmlAttributeLookupName(property));

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
            var xmlProperty = member.GetCustomAttribute<Attributes.XmlElement>();

            return xmlProperty == null || !xmlProperty.IsOptional;
        }

        private static bool IsNotIgnored(MemberInfo member)
        {
            var xmlIgnore = member.GetCustomAttribute<XmlIgnore>();

            return xmlIgnore == null;
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

        private static Attributes.XmlAttribute GetAttributeAnnotation(MemberInfo member)
        {
            return member.GetCustomAttribute<Attributes.XmlAttribute>();
        }

        private static Attributes.XmlElement GetElementAnnotation(MemberInfo member)
        {
            return member.GetCustomAttribute<Attributes.XmlElement>();
        }
    }
}