using DiGi.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.Core.xUnit
{
    public static partial class Query
    {
        /// <summary>
        /// Verifies that the provided serializable object can be correctly serialized to a JSON string and deserialized back into an object of the same type, ensuring data integrity by comparing the resulting serialized strings.
        /// </summary>
        /// <typeparam name="TSerializableObject">The type of the object being checked, which must implement <see cref="ISerializableObject"/>.</typeparam>
        /// <param name="serializableObject">The serializable object instance to be validated for serialization and deserialization consistency.</param>
        public static void SerializationCheck<TSerializableObject>(this TSerializableObject? serializableObject) where TSerializableObject : ISerializableObject
        {
            Assert.NotNull(serializableObject);

            string? json_Expected = Convert.ToSystem_String(serializableObject);

            Assert.NotNull(json_Expected);

            //Conversion

            List<TSerializableObject>? serializableObjects_Actual = Convert.ToDiGi<TSerializableObject>(json_Expected);

            Assert.NotNull(serializableObjects_Actual);

            Assert.NotEmpty(serializableObjects_Actual);

            TSerializableObject? serializableObject_Actual = serializableObjects_Actual.FirstOrDefault();

            Assert.NotNull(serializableObject_Actual);

            string? json_Actual = Convert.ToSystem_String(serializableObject_Actual);

            Assert.Equal(json_Expected, json_Actual);

            //Clone

            ISerializableObject? serializableObject_Clone;

            string? json_Clone;

            serializableObject_Clone = serializableObject.Clone();

            Assert.NotNull(serializableObject_Clone);

            json_Clone = Convert.ToSystem_String(serializableObject_Clone);

            Assert.Equal(json_Expected, json_Clone);

            serializableObject_Clone = Core.Query.Clone(serializableObject);

            json_Clone = Convert.ToSystem_String(serializableObject_Clone);

            Assert.Equal(json_Expected, json_Clone);
        }
    }
}
