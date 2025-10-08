namespace DiGi.Core.xUnit
{
    public partial class Tests
    {
        [Fact]
        public void TryParseDouble()
        {
            double result;

            Assert.True("4.9999999999999996E-06".TryParseDouble(out result));
            Assert.Equal(4.9999999999999996E-06, result);

            Assert.True("4,0".TryParseDouble(out result));
            Assert.Equal(4.0, result);

            Assert.True("4.0".TryParseDouble(out result));
            Assert.Equal(4.0, result);

            Assert.True("4.1".TryParseDouble(out result));
            Assert.Equal(4.1, result);

            Assert.True("0,1".TryParseDouble(out result));
            Assert.Equal(0.1, result);

            Assert.True("1,234.56".TryParseDouble(out result));
            Assert.Equal(1234.56, result);

            Assert.True("1 234,56".TryParseDouble(out result));
            Assert.Equal(1234.56, result);
        }
    }
}