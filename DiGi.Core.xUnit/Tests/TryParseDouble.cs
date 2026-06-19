namespace DiGi.Core.xUnit
{
    public partial class Tests
    {
        /// <summary>
        /// Verifies that the <c>TryParseDouble</c> extension method correctly parses various string formats, including scientific notation and different culture-specific separators, into double-precision floating-point numbers.
        /// </summary>
        [Fact]
        public void TryParseDouble()
        {
            double result;

            // Original test cases
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

            // Signed numbers (Negative and Positive)
            Assert.True("-4.0".TryParseDouble(out result));
            Assert.Equal(-4.0, result);

            Assert.True("+4.0".TryParseDouble(out result));
            Assert.Equal(4.0, result);

            Assert.True("-1 234,56".TryParseDouble(out result));
            Assert.Equal(-1234.56, result);

            Assert.True("-4.9999999999999996E-06".TryParseDouble(out result));
            Assert.Equal(-4.9999999999999996E-06, result);

            Assert.True("+4.9999999999999996E-06".TryParseDouble(out result));
            Assert.Equal(4.9999999999999996E-06, result);

            // Surrounding and mixed whitespaces
            Assert.True("   -4.0   ".TryParseDouble(out result));
            Assert.Equal(-4.0, result);

            Assert.True("\t 1 234,56 \r\n".TryParseDouble(out result));
            Assert.Equal(1234.56, result);

            // Special values (Infinity, NaN)
            Assert.True("Infinity".TryParseDouble(out result));
            Assert.True(double.IsPositiveInfinity(result));

            Assert.True("-Infinity".TryParseDouble(out result));
            Assert.True(double.IsNegativeInfinity(result));

            Assert.True("NaN".TryParseDouble(out result));
            Assert.True(double.IsNaN(result));

            Assert.True("  -Infinity  ".TryParseDouble(out result));
            Assert.True(double.IsNegativeInfinity(result));

            // Null, empty, and whitespace only
            Assert.False(((string)null!).TryParseDouble(out result));
            Assert.False("".TryParseDouble(out result));
            Assert.False("   ".TryParseDouble(out result));

            // Invalid formats and misplaced signs
            Assert.False("abc".TryParseDouble(out result));
            Assert.False("--4.0".TryParseDouble(out result));
            Assert.False("-+4.0".TryParseDouble(out result));
            Assert.False("4.0-".TryParseDouble(out result));
            Assert.False("4-0".TryParseDouble(out result));
            Assert.False("12.34.56".TryParseDouble(out result));
            Assert.False("1,23,45".TryParseDouble(out result));
        }
    }
}
