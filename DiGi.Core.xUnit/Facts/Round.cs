using DiGi.Core.Enums;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that the rounding functionality correctly processes a zero value using the defined distance tolerance.
        /// </summary>
        [Fact]
        public void Round()
        {
            double double_Value = Core.Query.Round(0, Constants.Tolerance.Distance);

            Assert.Equal(0.0, double_Value);
        }

        /// <summary>
        /// Tests that rounding a large value with an extremely small tolerance succeeds without throwing an OverflowException.
        /// </summary>
        [Fact]
        public void Round_OverflowSafety()
        {
            double double_Value = 1e20;
            double double_Tolerance = 1e-20;
            double double_Result = Core.Query.Round(double_Value, double_Tolerance);

            Assert.Equal(double_Value, double_Result);
        }

        /// <summary>
        /// Verifies that double.Round(tolerance) and double.Round(tolerance, RoundingMethod.Nearest) return identical results across various values and tolerances.
        /// </summary>
        [Fact]
        public void Round_ComparisonWithNearest()
        {
            double[] values = [0.0, 2.3, 2.5, 2.7, -2.3, -2.5, -2.7, 100.499, -100.499, 1e15];
            double[] tolerances = [0.1, 0.5, 1.0, 5.0, 10.0, 1e-3, 1e-6];

            foreach (double value in values)
            {
                foreach (double tolerance in tolerances)
                {
                    double result_Default = value.Round(tolerance);
                    double result_Nearest = value.Round(tolerance, RoundingMethod.Nearest);

                    Assert.Equal(result_Default, result_Nearest);
                }
            }
        }

        /// <summary>
        /// Tests all RoundingMethod enum values (Ceiling, Floor, Truncate, Nearest, Undefined) for positive and negative numbers.
        /// </summary>
        [Fact]
        public void Round_RoundingMethods()
        {
            double tolerance = 0.5;

            // Positive value: 2.3
            double value_Positive = 2.3;
            Assert.Equal(2.5, value_Positive.Round(tolerance, RoundingMethod.Nearest));
            Assert.Equal(2.5, value_Positive.Round(tolerance, RoundingMethod.Ceiling));
            Assert.Equal(2.0, value_Positive.Round(tolerance, RoundingMethod.Floor));
            Assert.Equal(2.0, value_Positive.Round(tolerance, RoundingMethod.Truncate));
            Assert.Equal(2.3, value_Positive.Round(tolerance, RoundingMethod.Undefined));

            // Negative value: -2.3
            double value_Negative = -2.3;
            Assert.Equal(-2.5, value_Negative.Round(tolerance, RoundingMethod.Nearest));
            Assert.Equal(-2.0, value_Negative.Round(tolerance, RoundingMethod.Ceiling));
            Assert.Equal(-2.5, value_Negative.Round(tolerance, RoundingMethod.Floor));
            Assert.Equal(-2.0, value_Negative.Round(tolerance, RoundingMethod.Truncate));
            Assert.Equal(-2.3, value_Negative.Round(tolerance, RoundingMethod.Undefined));
        }

        /// <summary>
        /// Tests edge cases such as NaN, Infinities, zero tolerance, and overflow conditions for all RoundingMethod options.
        /// </summary>
        [Fact]
        public void Round_EdgeCases()
        {
            double tolerance = 0.1;
            RoundingMethod[] methods =
            [
                RoundingMethod.Undefined,
                RoundingMethod.Ceiling,
                RoundingMethod.Floor,
                RoundingMethod.Nearest,
                RoundingMethod.Truncate
            ];

            foreach (RoundingMethod method in methods)
            {
                // NaN value returns NaN / value
                Assert.True(double.IsNaN(double.NaN.Round(tolerance, method)));

                // Positive / Negative infinity returns the value unchanged
                Assert.True(double.IsPositiveInfinity(double.PositiveInfinity.Round(tolerance, method)));
                Assert.True(double.IsNegativeInfinity(double.NegativeInfinity.Round(tolerance, method)));

                // Zero tolerance returns original value
                Assert.Equal(5.5, 5.5.Round(0.0, method));

                // NaN tolerance returns NaN (except for Undefined which returns original value)
                if (method == RoundingMethod.Undefined)
                {
                    Assert.Equal(5.5, 5.5.Round(double.NaN, method));
                }
                else
                {
                    Assert.True(double.IsNaN(5.5.Round(double.NaN, method)));
                }

                // Overflow safety path (huge values with tiny tolerance)
                double value_Huge = 1e20;
                double tolerance_Tiny = 1e-20;
                double result_Huge = value_Huge.Round(tolerance_Tiny, method);
                Assert.Equal(value_Huge, result_Huge);
            }
        }
    }
}