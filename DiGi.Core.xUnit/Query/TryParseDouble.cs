namespace DiGi.Core.xUnit
{
    public partial class Query
    {
        [Fact]
        public void TryParseDouble()
        {
            double result;

            Assert.True(Core.Query.TryParseDouble("4.9999999999999996E-06", out result));
            Assert.Equal(4.9999999999999996E-06, result);

            Assert.True(Core.Query.TryParseDouble("4,0", out result));
            Assert.Equal(4.0, result);

            Assert.True(Core.Query.TryParseDouble("4.0", out result));
            Assert.Equal(4.0, result);

            Assert.True(Core.Query.TryParseDouble("4.1", out result));
            Assert.Equal(4.1, result);

            Assert.True(Core.Query.TryParseDouble("0,1", out result));
            Assert.Equal(0.1, result);

            Assert.True(Core.Query.TryParseDouble("1,234.56", out result));
            Assert.Equal(1234.56, result);

            Assert.True(Core.Query.TryParseDouble("1 234,56", out result));
            Assert.Equal(1234.56, result);


        }
    }
}