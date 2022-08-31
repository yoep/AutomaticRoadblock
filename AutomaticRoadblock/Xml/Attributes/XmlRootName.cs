using System;

namespace RazerPoliceLights.Xml.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// Defines serialization and deserialization information for the xml root element.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class XmlRootName : Attribute
    {
        public XmlRootName(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Get the name of the xml root element.
        /// </summary>
        public string Name { get; }
    }
}