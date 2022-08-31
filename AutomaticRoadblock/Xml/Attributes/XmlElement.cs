using System;

namespace RazerPoliceLights.Xml.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// Defines serialization and deserialization behavior for xml elements.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class XmlElement : AbstractXmlAnnotationInfo
    {
    }
}