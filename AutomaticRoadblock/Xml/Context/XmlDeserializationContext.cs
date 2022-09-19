using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using AutomaticRoadblocks.Xml.Parser;

namespace AutomaticRoadblocks.Xml.Context
{
    public class XmlDeserializationContext : XmlContext
    {
        #region Constructors

        public XmlDeserializationContext(
            XPathDocument document,
            XPathNavigator currentNode,
            Type deserializationType,
            List<IXmlDeserializer> deserializers) : base(document, currentNode)
        {
            DeserializationType = deserializationType;
            Deserializers = deserializers;
        }

        internal XmlDeserializationContext(XmlDeserializationContext parent, XPathNavigator currentNode, string value, Type deserializationType)
            : base(parent.Document, currentNode, value)
        {
            Deserializers = parent.Deserializers;
            DeserializationType = deserializationType;
        }

        private XmlDeserializationContext(XmlDeserializationContext parent, XPathNavigator currentNode, Type deserializationType)
            : base(parent.Document, currentNode)
        {
            Deserializers = parent.Deserializers;
            DeserializationType = deserializationType;
        }

        private XmlDeserializationContext(XmlDeserializationContext parent, XPathNodeIterator nodes, Type deserializationType)
            : base(parent.Document, nodes)
        {
            Deserializers = parent.Deserializers;
            DeserializationType = deserializationType;
        }

        #endregion

        public List<IXmlDeserializer> Deserializers { get; }

        public Type DeserializationType { get; }

        /// <summary>
        /// Deserialize the given node for the given type.
        /// </summary>
        /// <param name="parser">Set the parser that is being used.</param>
        /// <param name="node">Set the node that must be deserialized.</param>
        /// <param name="type">Set the type to which the node must be mapped.</param>
        /// <returns>Returns the deserialized node.</returns>
        /// <exception cref="XmlException">Is thrown when an error occurs during deserialization of the node.</exception>
        public object Deserialize(XmlParser parser, XPathNavigator node, Type type)
        {
            var deserializer = Deserializers.Find(e => e.CanHandle(type));

            if (deserializer == null)
                throw new XmlException("Could not find deserializer for type " + type);

            return deserializer.Deserialize(parser, new XmlDeserializationContext(this, node, type));
        }

        /// <summary>
        /// Deserialize the given nodes for the given type.
        /// </summary>
        /// <param name="parser">Set the parser that is being used.</param>
        /// <param name="nodes">Set the nodes to deserialize.</param>
        /// <param name="type">Set the type to which the node must be mapped.</param>
        /// <returns>Returns an enumerable of the deserialized nodes.</returns>
        /// <exception cref="XmlException">Is thrown when an error occurs during deserialization of the nodes.</exception>
        public IEnumerable Deserialize(XmlParser parser, XPathNodeIterator nodes, Type type)
        {
            var deserializer = Deserializers.Find(e => e.CanHandle(type));

            if (deserializer == null)
                throw new XmlException("Could not find deserializer for type " + type);

            return (IEnumerable) deserializer.Deserialize(parser, new XmlDeserializationContext(this, nodes, type));
        }

        /// <summary>
        /// Deserialize the given value (assuming it's an attribute value) for the current node.
        /// </summary>
        /// <param name="parser">Set the parser that is being used.</param>
        /// <param name="value">Set the value to deserialize.</param>
        /// <param name="type">Set the type to which the value must be mapped.</param>
        /// <returns>Returns the deserialized value.</returns>
        /// <exception cref="XmlException">Is thrown when an error occurs during deserialization of the value.</exception>
        public object Deserialize(XmlParser parser, string value, Type type)
        {
            var deserializer = Deserializers.Find(e => e.CanHandle(type));

            if (deserializer == null)
                throw new XmlException("Could not find deserializer for type " + type);

            return deserializer.Deserialize(parser, new XmlDeserializationContext(this, CurrentNode, value, type));
        }

        protected bool Equals(XmlDeserializationContext other)
        {
            return Equals(Deserializers, other.Deserializers) && DeserializationType == other.DeserializationType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((XmlDeserializationContext) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Deserializers != null ? Deserializers.GetHashCode() : 0) * 397) ^
                       (DeserializationType != null ? DeserializationType.GetHashCode() : 0);
            }
        }
    }
}