namespace DiGi.Core.xUnit
{
    public partial class Tests
    {
        /// <summary>
        /// Verifies that the serialization and deserialization process works correctly for a <see cref="TestObject"/>.
        /// </summary>
        [Fact]
        public void Serialization()
        {
            TestObject testObject = new();

            Query.SerializationCheck(testObject);
        }
    }
}
