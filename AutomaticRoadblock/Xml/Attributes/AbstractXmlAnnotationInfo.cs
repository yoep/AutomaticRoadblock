using System;

namespace RazerPoliceLights.Xml.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// Defines standard annotation information for xml serialization and deserialization.
    /// </summary>
    public abstract class AbstractXmlAnnotationInfo : Attribute
    {
        /// <summary>
        /// Get or set the name of the xml property which must be used during serialization and deserialization.
        /// If no name is set, the member name will be used instead.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Get or set the default value for the member if the xml info is not available.
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Get or set of the member if the member is optional (will only be used during deserialization).
        /// </summary>
        public bool IsOptional { get; set; }
    }
}