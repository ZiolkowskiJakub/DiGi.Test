using DiGi.Math.Classes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DiGi.Math.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that bulk polynomial evaluation over a large non-indexable sequence completes within a generous time budget, guarding against the previous O(n^2) ElementAt access inside the parallel branch.
        /// </summary>
        [Fact]
        public void PolynomialEquation_Evaluate_Performance()
        {
            List<double> coefficients = [1.0, -2.0, 0.5, 3.0, -1.5, 2.0, 0.25];
            PolynomialEquation polynomialEquation = new(coefficients);

            int count = 20000;

            // Warm-up to trigger JIT compilation of the evaluation paths.
            IEnumerable<double> values_WarmUp = Enumerable.Range(0, 1500).Select(i => i * 0.001);
            polynomialEquation.Evaluate(values_WarmUp);

            // A non-IList source so the method must materialize once instead of re-enumerating per element.
            IEnumerable<double> values_Enumerable = Enumerable.Range(0, count).Select(i => i * 0.001);

            Stopwatch stopwatch = Stopwatch.StartNew();
            List<double>? results = polynomialEquation.Evaluate(values_Enumerable);
            stopwatch.Stop();

            Assert.NotNull(results);
            Assert.Equal(count, results.Count);
            Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Bulk evaluation took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies that multiplication of two moderately large matrices completes within a generous time budget after removing the per-element indexer overhead from the inner loop.
        /// </summary>
        [Fact]
        public void Matrix_Multiply_Performance()
        {
            int size = 150;
            double[,] doubles_A = new double[size, size];
            double[,] doubles_B = new double[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    doubles_A[i, j] = ((i + j) % 7) - 3;
                    doubles_B[i, j] = ((i * j) % 5) - 2;
                }
            }

            Matrix matrix_A = new(doubles_A);
            Matrix matrix_B = new(doubles_B);

            // Warm-up.
            Matrix? matrix_WarmUp = matrix_A * matrix_B;
            Assert.NotNull(matrix_WarmUp);

            Stopwatch stopwatch = Stopwatch.StartNew();
            Matrix? matrix_Product = matrix_A * matrix_B;
            stopwatch.Stop();

            Assert.NotNull(matrix_Product);
            Assert.Equal(size, matrix_Product.RowCount());
            Assert.Equal(size, matrix_Product.ColumnCount());
            Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Matrix multiplication took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies that repeated range checks over a large interpolation set stay fast, guarding against the previous per-call list allocations incurred by separately evaluating MinX and MaxX.
        /// </summary>
        [Fact]
        public void LinearInterpolation_InRange_Performance()
        {
            LinearInterpolation linearInterpolation = new();
            int pointCount = 1000;
            for (int i = 0; i < pointCount; i++)
            {
                linearInterpolation.Add(i, i * 2.0);
            }

            // Warm-up.
            for (int i = 0; i < 1000; i++)
            {
                linearInterpolation.InRange(i % pointCount);
            }

            int iterations = 50000;
            int inRangeCount = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                if (linearInterpolation.InRange(i % pointCount))
                {
                    inRangeCount++;
                }
            }
            stopwatch.Stop();

            Assert.Equal(iterations, inRangeCount);
            Assert.True(stopwatch.ElapsedMilliseconds < 2000, $"Repeated InRange checks took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies that a large number of neighbour-index lookups over a large sorted array complete quickly with the binary-search implementation.
        /// </summary>
        [Fact]
        public void Query_NeigbourIndices_Performance()
        {
            int length = 100000;
            double[] doubles_Sorted = new double[length];
            for (int i = 0; i < length; i++)
            {
                doubles_Sorted[i] = i * 1.5;
            }

            // Warm-up.
            for (int i = 0; i < 1000; i++)
            {
                Query.NeigbourIndices(doubles_Sorted, i * 1.5, out _, out _);
            }

            int iterations = 1000000;
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                double value = ((i % length) * 1.5) + 0.5;
                Query.NeigbourIndices(doubles_Sorted, value, out _, out _);
            }
            stopwatch.Stop();

            Assert.True(stopwatch.ElapsedMilliseconds < 2000, $"Neighbour index lookups took {stopwatch.ElapsedMilliseconds} ms.");
        }
    }
}
