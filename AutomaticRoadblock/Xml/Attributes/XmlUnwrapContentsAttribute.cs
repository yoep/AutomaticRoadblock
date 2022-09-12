using System;

namespace AutomaticRoadblocks.Xml.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// Defines that the collection elements of the class should be stored in this field.
    /// Only one of these attributes can be applied within a class.
    /// All other properties will also be ignored.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class XmlUnwrapContentsAttribute : Attribute
    {
        /// <summary>
        /// Use the child element name for the value if present, else unwrap the contents from the node into the field.
        /// </summary>
        public string OptionalElementName { get; set; }
    }
}