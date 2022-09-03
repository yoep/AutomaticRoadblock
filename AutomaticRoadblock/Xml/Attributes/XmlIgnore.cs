using System;

namespace AutomaticRoadblocks.Xml.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// Defines that the serializer and deserializer should ignore the member that is being annotated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class XmlIgnore : Attribute
    {
    }
}