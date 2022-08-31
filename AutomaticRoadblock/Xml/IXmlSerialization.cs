using System;

namespace AutomaticRoadblocks.Xml
{
    public interface IXmlSerialization
    {
        /// <summary>
        /// Check if the deserializer can handle the given type.
        /// </summary>
        /// <param name="type">Set the type to check for compatibility.</param>
        /// <returns>Returns true if the serialization can handle the given type, else false.</returns>
        bool CanHandle(Type type);
    }
}