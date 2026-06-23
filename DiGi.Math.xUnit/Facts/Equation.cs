using DiGi.Math.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace DiGi.Math.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the construction, evaluation, and serialization of the LinearEquation class.
        /// </summary>
        [Fact]
        public void LinearEquation()
        {
            // Constructor with coefficients A and B
            double coefficientA = 2.5;
            double coefficientB = -1.0;
            LinearEquation linearEquation_1 = new(coefficientA, coefficientB);

            Assert.Equal(coefficientA, linearEquation_1.A);
            Assert.Equal(coefficientB, linearEquation_1.B);

            List<double> coefficients = linearEquation_1.Coefficients;
            Assert.Equal(2, coefficients.Count);
            Assert.Equal(coefficientB, coefficients[0]);
            Assert.Equal(coefficientA, coefficients[1]);

            // Copy constructor
            LinearEquation linearEquation_Copy = new(linearEquation_1);
            Assert.Equal(linearEquation_1.A, linearEquation_Copy.A);
            Assert.Equal(linearEquation_1.B, linearEquation_Copy.B);

            // Single evaluation
            double xVal = 4.0;
            double expectedResult = coefficientB + coefficientA * xVal; // -1 + 2.5 * 4 = 9.0
            double actualResult = linearEquation_1.Evaluate(xVal);
            Assert.Equal(expectedResult, actualResult);

            // Evaluation with NaN
            Assert.True(double.IsNaN(linearEquation_1.Evaluate(double.NaN)));

            // Bulk evaluation
            List<double> xVals = [0.0, 1.0, 2.0];
            List<double>? yVals = linearEquation_1.Evaluate(xVals);
            Assert.NotNull(yVals);
            Assert.Equal(xVals.Count, yVals.Count);
            for (int i = 0; i < xVals.Count; i++)
            {
                Assert.Equal(coefficientB + coefficientA * xVals[i], yVals[i]);
            }

            // Bulk evaluation with null input
            Assert.Null(linearEquation_1.Evaluate(null));

            // Serialization Check
            DiGi.Core.xUnit.Query.SerializationCheck(linearEquation_1);

            // JSON constructor
            JsonObject jsonObject = new()
            {
                ["A"] = coefficientA,
                ["B"] = coefficientB
            };
            LinearEquation linearEquation_Json = new(jsonObject);
            Assert.Equal(coefficientA, linearEquation_Json.A);
            Assert.Equal(coefficientB, linearEquation_Json.B);
        }

        /// <summary>
        /// Tests the construction, evaluation, explicit conversion, and serialization of the PolynomialEquation class.
        /// </summary>
        [Fact]
        public void PolynomialEquation()
        {
            // Constructor with coefficients array [a0, a1, a2] representing a0 + a1*x + a2*x^2
            List<double> coefficientsInput = [1.0, 2.0, 3.0];
            PolynomialEquation polynomialEquation_1 = new(coefficientsInput);

            Assert.Equal(2, polynomialEquation_1.Degree);
            List<double>? coefficients = polynomialEquation_1.Coefficients;
            Assert.NotNull(coefficients);
            Assert.Equal(coefficientsInput.Count, coefficients.Count);
            for (int i = 0; i < coefficientsInput.Count; i++)
            {
                Assert.Equal(coefficientsInput[i], coefficients[i]);
            }

            // Copy constructor
            PolynomialEquation polynomialEquation_Copy = new(polynomialEquation_1);
            Assert.Equal(polynomialEquation_1.Degree, polynomialEquation_Copy.Degree);
            List<double>? coefficientsCopy = polynomialEquation_Copy.Coefficients;
            Assert.NotNull(coefficientsCopy);
            Assert.Equal(coefficients.Count, coefficientsCopy.Count);

            // Single evaluation: 1 + 2*x + 3*x^2 at x = 2 is 1 + 4 + 12 = 17
            double xVal = 2.0;
            double expectedResult = 17.0;
            double actualResult = polynomialEquation_1.Evaluate(xVal);
            Assert.Equal(expectedResult, actualResult);

            // Bulk evaluation for small dataset (< 1000 items)
            List<double> xVals_Small = [0.0, 1.0, 2.0];
            List<double>? yVals_Small = polynomialEquation_1.Evaluate(xVals_Small);
            Assert.NotNull(yVals_Small);
            Assert.Equal(3, yVals_Small.Count);
            Assert.Equal(1.0, yVals_Small[0]); // x=0 -> 1
            Assert.Equal(6.0, yVals_Small[1]); // x=1 -> 1+2+3=6
            Assert.Equal(17.0, yVals_Small[2]); // x=2 -> 17

            // Bulk evaluation for large dataset (>= 1000 items) to cover parallel path
            List<double> xVals_Large = Enumerable.Range(0, 1050).Select(x => (double)x).ToList();
            List<double>? yVals_Large = polynomialEquation_1.Evaluate(xVals_Large);
            Assert.NotNull(yVals_Large);
            Assert.Equal(1050, yVals_Large.Count);
            for (int i = 0; i < 1050; i++)
            {
                double expectedVal = 1.0 + 2.0 * i + 3.0 * i * i;
                Assert.Equal(expectedVal, yVals_Large[i]);
            }

            // Explicit operator from LinearEquation
            LinearEquation linearEquation = new(3.0, 5.0); // 3x + 5
            PolynomialEquation? polynomialEquation_Cast = (PolynomialEquation?)linearEquation;
            Assert.NotNull(polynomialEquation_Cast);
            Assert.Equal(1, polynomialEquation_Cast.Degree);
            List<double>? castCoefficients = polynomialEquation_Cast.Coefficients;
            Assert.NotNull(castCoefficients);
            Assert.Equal(2, castCoefficients.Count);
            Assert.Equal(5.0, castCoefficients[0]); // constant b
            Assert.Equal(3.0, castCoefficients[1]); // slope a

            // Serialization Check
            DiGi.Core.xUnit.Query.SerializationCheck(polynomialEquation_1);
        }
    }
}
