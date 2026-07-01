using DiGi.Math.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.Math.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the Horner-based PolynomialEquation.Evaluate(double) produces correct results, including roots, exact integer checkpoints, fractional values against a factored reference, and the empty-coefficient edge case.
        /// </summary>
        [Fact]
        public void PolynomialEquation_HornerEvaluation()
        {
            // p(x) = (x-1)(x-2)(x-3)(x-4)(x-5) = x^5 - 15x^4 + 85x^3 - 225x^2 + 274x - 120
            List<double> coefficients = [-120.0, 274.0, -225.0, 85.0, -15.0, 1.0];
            PolynomialEquation polynomialEquation = new(coefficients);

            Assert.Equal(5, polynomialEquation.Degree);

            // Each factored root evaluates to (near) zero.
            for (int root = 1; root <= 5; root++)
            {
                Assert.True(System.Math.Abs(polynomialEquation.Evaluate(root)) < 1e-9);
            }

            // Exact integer checkpoints.
            Assert.Equal(-120.0, polynomialEquation.Evaluate(0.0));
            Assert.Equal(120.0, polynomialEquation.Evaluate(6.0));

            // Fractional values compared against the factored reference within tolerance.
            double[] doubles_Samples = [-2.5, 0.3, 1.7, 4.25, 7.9];
            foreach (double sample in doubles_Samples)
            {
                double expected = (sample - 1.0) * (sample - 2.0) * (sample - 3.0) * (sample - 4.0) * (sample - 5.0);
                double actual = polynomialEquation.Evaluate(sample);
                Assert.True(System.Math.Abs(actual - expected) <= 1e-6 * (1.0 + System.Math.Abs(expected)));
            }

            // Empty coefficients now evaluate to NaN (previously threw IndexOutOfRangeException).
            List<double> coefficients_Empty = [];
            PolynomialEquation polynomialEquation_Empty = new(coefficients_Empty);
            Assert.True(double.IsNaN(polynomialEquation_Empty.Evaluate(2.0)));
        }

        /// <summary>
        /// Verifies that bulk PolynomialEquation.Evaluate over a large dataset (parallel branch) returns the same results as single-value evaluation for both a non-indexable enumerable source and an indexable list source.
        /// </summary>
        [Fact]
        public void PolynomialEquation_Evaluate_ParallelPath()
        {
            // Degree 6 (>= 5 coefficients) combined with >= 1000 values triggers the parallel branch.
            List<double> coefficients = [1.0, -2.0, 0.5, 3.0, -1.5, 2.0, 0.25];
            PolynomialEquation polynomialEquation = new(coefficients);

            int count = 1500;

            // A non-IList source forces the method to materialize once instead of re-enumerating per element.
            IEnumerable<double> values_Enumerable = Enumerable.Range(0, count).Select(i => i * 0.01);
            List<double>? results_Parallel = polynomialEquation.Evaluate(values_Enumerable);
            Assert.NotNull(results_Parallel);
            Assert.Equal(count, results_Parallel.Count);

            // Reference: each value evaluated individually via single-value Horner.
            for (int i = 0; i < count; i++)
            {
                double value = i * 0.01;
                double expected = polynomialEquation.Evaluate(value);
                Assert.Equal(expected, results_Parallel[i]);
            }

            // An IList source uses the same path without re-materializing and must match exactly.
            List<double> values_List = [.. Enumerable.Range(0, count).Select(i => i * 0.01)];
            List<double>? results_List = polynomialEquation.Evaluate(values_List);
            Assert.NotNull(results_List);
            Assert.Equal(results_Parallel, results_List);
        }

        /// <summary>
        /// Verifies that Query.Median sorts a private copy of the input, returning the correct order statistic for unsorted odd and even collections without mutating the source, and handles single, empty, and null inputs.
        /// </summary>
        [Fact]
        public void Query_Median_Unsorted()
        {
            // Odd count, unsorted: median is the middle value of the sorted sequence (30.0).
            List<double> values_Odd = [50.0, 10.0, 30.0, 20.0, 40.0];
            Assert.Equal(30.0, values_Odd.Median());

            // The source collection must not be reordered by the median calculation.
            List<double> values_OddExpected = [50.0, 10.0, 30.0, 20.0, 40.0];
            Assert.Equal(values_OddExpected, values_Odd);

            // Even count, unsorted: average of the two central sorted values ((30 + 40) / 2 = 35.0).
            List<double> values_Even = [40.0, 10.0, 50.0, 20.0, 60.0, 30.0];
            Assert.Equal(35.0, values_Even.Median());

            // Single value, empty, and null edge cases.
            List<double> values_Single = [42.0];
            Assert.Equal(42.0, values_Single.Median());

            List<double> values_Empty = [];
            Assert.True(double.IsNaN(values_Empty.Median()));

            IEnumerable<double>? values_Null = null;
            Assert.True(double.IsNaN(values_Null.Median()));
        }

        /// <summary>
        /// Verifies that the binary-search Query.NeigbourIndices reproduces the original bracketing semantics (including for repeated values, exact matches, and out-of-range queries) by comparing against a linear reference implementation.
        /// </summary>
        [Fact]
        public void Query_NeigbourIndices_BinarySearchEquivalence()
        {
            // Repeated values lock in the bracketing semantics across the optimization.
            double[] doubles_Sorted = [10.0, 20.0, 20.0, 35.0, 35.0, 35.0, 50.0];

            double[] doubles_Queries = [5.0, 10.0, 15.0, 20.0, 27.5, 35.0, 42.0, 50.0, 60.0];
            foreach (double query in doubles_Queries)
            {
                Query.NeigbourIndices(doubles_Sorted, query, out int lowerIndex, out int upperIndex);
                ReferenceNeigbourIndices(doubles_Sorted, query, out int lowerIndex_Reference, out int upperIndex_Reference);

                Assert.Equal(lowerIndex_Reference, lowerIndex);
                Assert.Equal(upperIndex_Reference, upperIndex);
            }

            // NaN and empty inputs yield the (-1, -1) sentinel.
            Query.NeigbourIndices(doubles_Sorted, double.NaN, out int lowerIndex_NaN, out int upperIndex_NaN);
            Assert.Equal(-1, lowerIndex_NaN);
            Assert.Equal(-1, upperIndex_NaN);

            double[] doubles_Empty = [];
            Query.NeigbourIndices(doubles_Empty, 1.0, out int lowerIndex_EmptyArray, out int upperIndex_EmptyArray);
            Assert.Equal(-1, lowerIndex_EmptyArray);
            Assert.Equal(-1, upperIndex_EmptyArray);
        }

        /// <summary>
        /// Verifies that the allocation-free MinX, MaxX, and single-pass InRange of LinearInterpolation remain correct for out-of-order insertion, boundary and NaN queries, and empty sets, while interpolation still produces the expected value.
        /// </summary>
        [Fact]
        public void LinearInterpolation_BoundsAndRange()
        {
            // Points added out of order; the extremes must still be reported correctly.
            LinearInterpolation linearInterpolation = new();
            linearInterpolation.Add(30.0, 300.0);
            linearInterpolation.Add(10.0, 100.0);
            linearInterpolation.Add(20.0, 200.0);

            Assert.Equal(10.0, linearInterpolation.MinX);
            Assert.Equal(30.0, linearInterpolation.MaxX);

            // InRange boundary behaviour and NaN handling.
            Assert.True(linearInterpolation.InRange(10.0));
            Assert.True(linearInterpolation.InRange(30.0));
            Assert.True(linearInterpolation.InRange(15.0));
            Assert.False(linearInterpolation.InRange(9.999));
            Assert.False(linearInterpolation.InRange(30.001));
            Assert.False(linearInterpolation.InRange(double.NaN));

            // Interpolation still correct (15.0 -> 150.0 on the 10..20 segment).
            Assert.Equal(150.0, linearInterpolation.CalculateY(15.0));

            // Empty interpolation: MinX/MaxX are NaN and nothing is in range.
            LinearInterpolation linearInterpolation_Empty = new();
            Assert.True(double.IsNaN(linearInterpolation_Empty.MinX));
            Assert.True(double.IsNaN(linearInterpolation_Empty.MaxX));
            Assert.False(linearInterpolation_Empty.InRange(5.0));
        }

        /// <summary>
        /// Verifies that the backing-array Matrix multiplication produces correct results for a rectangular (non-square) product and returns null on a dimension mismatch.
        /// </summary>
        [Fact]
        public void Matrix_Multiply_RectangularCorrectness()
        {
            int rowCount_A = 5;
            int sharedCount = 4;
            int columnCount_B = 6;

            double[,] doubles_A = new double[rowCount_A, sharedCount];
            for (int i = 0; i < rowCount_A; i++)
            {
                for (int j = 0; j < sharedCount; j++)
                {
                    doubles_A[i, j] = (i * 7) - (j * 3) + 1;
                }
            }

            double[,] doubles_B = new double[sharedCount, columnCount_B];
            for (int i = 0; i < sharedCount; i++)
            {
                for (int j = 0; j < columnCount_B; j++)
                {
                    doubles_B[i, j] = (i * 2) + (j * 5) - 4;
                }
            }

            Matrix matrix_A = new(doubles_A);
            Matrix matrix_B = new(doubles_B);

            Matrix? matrix_Product = matrix_A * matrix_B;
            Assert.NotNull(matrix_Product);
            Assert.Equal(rowCount_A, matrix_Product.RowCount());
            Assert.Equal(columnCount_B, matrix_Product.ColumnCount());

            for (int i = 0; i < rowCount_A; i++)
            {
                for (int j = 0; j < columnCount_B; j++)
                {
                    double expected = 0.0;
                    for (int k = 0; k < sharedCount; k++)
                    {
                        expected += doubles_A[i, k] * doubles_B[k, j];
                    }

                    Assert.Equal(expected, matrix_Product[i, j]);
                }
            }

            // Incompatible dimensions return null.
            Matrix matrix_Mismatch = new(new double[3, 3]);
            Assert.Null(matrix_A * matrix_Mismatch);
        }

        /// <summary>
        /// Verifies that Create.Matrix.Identity produces a correct identity matrix and that Create.Matrix.Scale builds the expected diagonal scale matrix from a non-indexable enumerable source.
        /// </summary>
        [Fact]
        public void Create_Matrix_IdentityAndScale()
        {
            Matrix matrix_Identity = Create.Matrix.Identity(4);
            Assert.Equal(4, matrix_Identity.RowCount());
            Assert.Equal(4, matrix_Identity.ColumnCount());
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Assert.Equal(i == j ? 1.0 : 0.0, matrix_Identity[i, j]);
                }
            }

            // A non-IList source exercises the materialization path of Scale.
            IEnumerable<double> values_Scale = Enumerable.Range(1, 3).Select(i => i * 10.0);
            Matrix? matrix_Scale = Create.Matrix.Scale(values_Scale);
            Assert.NotNull(matrix_Scale);
            Assert.Equal(4, matrix_Scale.RowCount()); // count + 1
            Assert.Equal(4, matrix_Scale.ColumnCount());
            Assert.Equal(10.0, matrix_Scale[0, 0]);
            Assert.Equal(20.0, matrix_Scale[1, 1]);
            Assert.Equal(30.0, matrix_Scale[2, 2]);
            Assert.Equal(1.0, matrix_Scale[3, 3]); // trailing identity element preserved
            Assert.Equal(0.0, matrix_Scale[0, 1]);
        }

        /// <summary>
        /// Verifies that Create.PolynomialEquation fits a polynomial that reproduces the source data and that its guard clauses (null inputs, mismatched lengths, single point) return null.
        /// </summary>
        [Fact]
        public void Create_PolynomialEquation_Fit()
        {
            // Fit y = x^2 (order 2).
            double[] doubles_X = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0];
            double[] doubles_Y = new double[doubles_X.Length];
            for (int i = 0; i < doubles_X.Length; i++)
            {
                doubles_Y[i] = doubles_X[i] * doubles_X[i];
            }

            PolynomialEquation? polynomialEquation = Create.PolynomialEquation(doubles_X, doubles_Y, 2);
            Assert.NotNull(polynomialEquation);
            Assert.Equal(2, polynomialEquation.Degree);

            foreach (double x in doubles_X)
            {
                double expected = x * x;
                double actual = polynomialEquation.Evaluate(x);
                Assert.True(System.Math.Abs(actual - expected) < 1e-6);
            }

            // Guard clauses.
            Assert.Null(Create.PolynomialEquation(null, doubles_Y, 2));
            Assert.Null(Create.PolynomialEquation(doubles_X, [1.0, 2.0], 2));
            Assert.Null(Create.PolynomialEquation([1.0], [1.0], -1));
        }

        /// <summary>
        /// Verifies that Matrix.GetInversed returns the correct inverse for a known 2x2 matrix, leaves the source matrix unchanged, and returns null for a non-square matrix.
        /// </summary>
        [Fact]
        public void Matrix_GetInversed()
        {
            // [[4, 7], [2, 6]] has determinant 10 and inverse [[0.6, -0.7], [-0.2, 0.4]].
            double[,] doubles = new double[2, 2] { { 4.0, 7.0 }, { 2.0, 6.0 } };
            Matrix matrix = new(doubles);

            Matrix? matrix_Inversed = matrix.GetInversed();
            Assert.NotNull(matrix_Inversed);
            Assert.True(System.Math.Abs(matrix_Inversed[0, 0] - 0.6) < 1e-9);
            Assert.True(System.Math.Abs(matrix_Inversed[0, 1] - (-0.7)) < 1e-9);
            Assert.True(System.Math.Abs(matrix_Inversed[1, 0] - (-0.2)) < 1e-9);
            Assert.True(System.Math.Abs(matrix_Inversed[1, 1] - 0.4) < 1e-9);

            // The source matrix must be left unchanged (GetInversed returns a new matrix).
            Assert.Equal(4.0, matrix[0, 0]);
            Assert.Equal(7.0, matrix[0, 1]);
            Assert.Equal(2.0, matrix[1, 0]);
            Assert.Equal(6.0, matrix[1, 1]);

            // Non-square input returns null.
            Matrix matrix_NonSquare = new(new double[2, 3]);
            Assert.Null(matrix_NonSquare.GetInversed());
        }

        /// <summary>
        /// Linear reference implementation of the neighbour-index bracketing used to validate the optimized binary-search version.
        /// </summary>
        /// <param name="values">The sorted ascending array.</param>
        /// <param name="value">The value to bracket.</param>
        /// <param name="lowerIndex">The lower neighbour index, or -1 when no result exists.</param>
        /// <param name="upperIndex">The upper neighbour index, or -1 when no result exists.</param>
        private static void ReferenceNeigbourIndices(double[] values, double value, out int lowerIndex, out int upperIndex)
        {
            lowerIndex = -1;
            upperIndex = -1;

            if (values.Length == 0 || double.IsNaN(value))
            {
                return;
            }

            if (value <= values[0])
            {
                lowerIndex = 0;
                upperIndex = 0;
                return;
            }

            int lastIndex = values.Length - 1;
            if (value >= values[lastIndex])
            {
                lowerIndex = lastIndex;
                upperIndex = lastIndex;
                return;
            }

            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] > value)
                {
                    lowerIndex = i - 1;
                    upperIndex = i;
                    return;
                }
            }
        }
    }
}
