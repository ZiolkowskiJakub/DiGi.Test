namespace DiGi.Core.xUnit
{
    public partial class Tests
    {
        [Fact]
        public void Serialization()
        {
            TestObject testObject = new();

            Query.SerializationCheck(testObject);


        }
    }
}