using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.Xml.XPath;
using AutomaticRoadblocks.Xml.Context;

namespace AutomaticRoadblocks.Xml.Parser
{
    public class XmlParser
    {
        public XPathNavigator FetchNodeForMember(XmlContext context, MemberInfo member)
        {
            Assert.NotNull(context, "context cannot be null");
            Assert.NotNull(member, "member cannot be null");

            return context.CurrentNode.SelectSingleNode(GetXmlLookupName(member));
        }

        public XPathNodeIterator FetchNodesForMember(XmlContext context, MemberInfo member)
        {
            Assert.NotNull(context, "context cannot be null");
            Assert.NotNull(member, "member cannot be null");
            return FetchNodesForMember(context, member, GetXmlLookupName(member));
        }

        public XPathNodeIterator FetchNodesForMember(XmlContext context, MemberInfo member, string lookupName)
        {
            Assert.NotNull(context, "context cannot be null");
            Assert.NotNull(member, "member cannot be null");
            Assert.NotNull(lookupName, "lookupName cannot be null");
            var currentNode = context.CurrentNode;
            var filteredChildNodes = currentNode.Name.Equals(lookupName)
                ? currentNode.SelectChildren(XPathNodeType.Element)
                : currentNode.Select(lookupName);

            return filteredChildNodes.Count > 1 ? filteredChildNodes : currentNode.SelectSingleNode(lookupName)?.SelectChildren(XPathNodeType.Element);
        }

        public string FetchAttributeValue(XmlContext context, MemberInfo member)
        {
            Assert.NotNull(context, "context cannot be null");
            Assert.NotNull(member, "member cannot be null");
            var attributeValue = GetXmlAttributeLookupNames(member)
                .Select(x => GetAttributeValue(context, x))
                .FirstOrDefault(x => !string.IsNullOrEmpty(x));

            return attributeValue;
        }

        public string GetAttributeValue(XmlContext context, string lookupName)
        {
            return context.CurrentNode.GetAttribute(lookupName, "");
        }

        public bool HasNodeChildren(XPathNavigator node)
        {
            return node.SelectChildren(XPathNodeType.Element).Count > 0;
        }

        public string GetXmlLookupName(MemberInfo member)
        {
            return member.GetType().IsInstanceOfType(typeof(Type))
                ? LookupTypeName(member)
                : LookupPropertyName(member);
        }

        public static ICollection<string> GetXmlAttributeLookupNames(MemberInfo member)
        {
            var xmlAttribute = member.GetCustomAttribute<XmlAttributeAttribute>();

            if (xmlAttribute != null && !string.IsNullOrEmpty(xmlAttribute.AttributeName))
            {
                return new List<string> { xmlAttribute.AttributeName, LowercaseFirstLetter(xmlAttribute.AttributeName) };
            }

            return new List<string> { member.Name, LowercaseFirstLetter(member.Name) };
        }

        private static string LowercaseFirstLetter(string value)
        {
            if (value == null || !char.IsUpper(value[0]))
                return value;

            return char.ToLower(value[0]) + value.Substring(1);
        }

        private static string LookupTypeName(MemberInfo member)
        {
            var rootName = member.GetCustomAttribute<XmlRootAttribute>();

            return rootName != null && !string.IsNullOrEmpty(rootName.ElementName) ? rootName.ElementName : member.Name;
        }

        private static string LookupPropertyName(MemberInfo member)
        {
            var xmlElement = member.GetCustomAttribute<XmlElementAttribute>();

            if (xmlElement != null && !string.IsNullOrEmpty(xmlElement.ElementName))
                return xmlElement.ElementName;

            return member.Name;
        }
    }
}