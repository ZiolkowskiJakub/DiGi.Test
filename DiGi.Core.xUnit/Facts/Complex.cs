namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        [Fact]
        public void Complex()
        {
            ComplexObject complexObject = new(new System.Numerics.Complex(1, 2));

            Query.SerializationCheck(complexObject);
        }
    }
}