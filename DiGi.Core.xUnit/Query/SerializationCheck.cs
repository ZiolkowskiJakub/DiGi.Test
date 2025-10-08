using DiGi.Core.Interfaces;

namespace DiGi.Core.xUnit
{
    public static partial class Query
    {
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