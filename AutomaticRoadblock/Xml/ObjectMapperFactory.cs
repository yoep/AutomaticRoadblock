using System.Collections.Generic;
using AutomaticRoadblocks.Xml.Deserializers;

namespace AutomaticRoadblocks.Xml
{
    public class ObjectMapperFactory
    {
        private readonly List<IXmlDeserializer> _deserializers = new()
        {
            new UnwrapXmlDeserializer(), //the unwrap deserializer
            new CollectionXmlDeserializer(), //register generic collection handler before object deserializer
            new ObjectXmlDeserializer() //object deserializer should always be registered as it can handle everything
        };

        public static ObjectMapper CreateInstance()
        {
            var objectMapperFactory = new ObjectMapperFactory();

            return objectMapperFactory
                .RegisterNativeTypes()
                .RegisterAutomaticRoadblockSerializers()
                .GetInstance();
        }

        /// <summary>
        /// Registers serializers for System native types.
        /// </summary>
        /// <returns>Returns this instance.</returns>
        public ObjectMapperFactory RegisterNativeTypes()
        {
            //insert the natives before the object serializer (because everything is an object)
            _deserializers.InsertRange(0, new List<IXmlDeserializer>
            {
                new StringXmlDeserializer(),
                new DoubleXmlDeserializer(),
                new BooleanXmlDeserializer()
            });

            return this;
        }

        public ObjectMapperFactory RegisterAutomaticRoadblockSerializers()
        {
            // insert the additions before the object serializer (because everything is an object)
            _deserializers.InsertRange(0, new List<IXmlDeserializer>
            {
            });

            return this;
        }

        /// <summary>
        /// Creates an ObjectMapper instance based on the factory configuration.
        /// </summary>
        /// <returns>Returns a new ObjectMapper instance.</returns>
        public ObjectMapper GetInstance()
        {
            return new ObjectMapper(_deserializers);
        }
    }
}