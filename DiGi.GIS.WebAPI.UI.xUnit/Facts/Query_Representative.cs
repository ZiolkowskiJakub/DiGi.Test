using DiGi.GIS.WebAPI.UI.Classes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the choice of a typology node's representative building (<see cref="Query.Representative"/>): the medoid over the definition's parameters.
        /// <para>The centre of a symmetric set is chosen, and an outlier never is. A parameter that does not vary within the node is dropped. A row missing a value cannot be chosen, however central. Ties go to the ordinal-first reference whatever the input order. A categorical parameter restricts the choice to its modal value.</para>
        /// <para>The robust scale falls back from the interquartile range to the median absolute deviation and then to the full range. With no varying parameter, the first reference stands for the node. Empty input answers null.</para>
        /// </summary>
        [Fact]
        public void Representative()
        {
            // The centre of a symmetric set.
            List<string?> references = [.. Enumerable.Range(1, 9).Select(i => (string?)("R" + i))];
            List<double?> values = [.. Enumerable.Range(1, 9).Select(i => (double?)i)];
            Representative? representative = Query.Representative(references, [values], null);
            Assert.NotNull(representative);
            Assert.Equal("R5", references[representative!.Index]);
            Assert.Equal(1, representative.ParameterCount);
            Assert.Equal(9, representative.CandidateCount);
            Assert.False(representative.Sampled);
            Assert.True(representative.MeanDistance > 0);
            Assert.True(representative.Percentile90Distance >= representative.MeanDistance);

            // An outlier never wins; the median does.
            List<string?> references_Outlier = ["A", "B", "C", "D", "E"];
            List<double?> values_Outlier = [10, 11, 12, 13, 1000];
            representative = Query.Representative(references_Outlier, [values_Outlier], null);
            Assert.Equal("C", references_Outlier[representative!.Index]);

            // A parameter constant within the node is dropped and changes nothing.
            List<double?> values_Constant = [7, 7, 7, 7, 7];
            representative = Query.Representative(references_Outlier, [values_Outlier, values_Constant], null);
            Assert.Equal("C", references_Outlier[representative!.Index]);
            Assert.Equal(1, representative.ParameterCount);

            // A missing value disqualifies the central row; it still counts as a neighbour.
            List<double?> values_Second = [1, 2, null, 4, 5];
            representative = Query.Representative(references_Outlier, [values_Outlier, values_Second], null);
            Assert.NotEqual("C", references_Outlier[representative!.Index]);
            Assert.Equal(4, representative.CandidateCount);

            // A tie goes to the ordinal-first reference, in either input order.
            representative = Query.Representative(["B", "A"], [new List<double?>() { 0, 1 }], null);
            Assert.Equal(1, representative!.Index);
            representative = Query.Representative(["A", "B"], [new List<double?>() { 1, 0 }], null);
            Assert.Equal(0, representative!.Index);

            // The modal category first: "C" is the numeric centre but carries the minority category.
            List<string?> categories = ["x", "x", "y", "x", "y"];
            representative = Query.Representative(references_Outlier, [values_Outlier], [categories]);
            Assert.Equal("x", categories[representative!.Index]);
            Assert.Equal(3, representative.CandidateCount);

            // A zero interquartile range falls back to the median absolute deviation, then to the range.
            List<double?> values_Spike = [5, 5, 5, 5, 9];
            representative = Query.Representative(references_Outlier, [values_Spike], null);
            Assert.Equal(1, representative!.ParameterCount);
            Assert.Equal("A", references_Outlier[representative.Index]);

            // Nothing varies: no parameter takes part and the first reference stands for the node.
            representative = Query.Representative(["Q", "P", "R"], [new List<double?>() { 3, 3, 3 }], null);
            Assert.Equal(0, representative!.ParameterCount);
            Assert.Equal(1, representative.Index);
            Assert.Equal(0, representative.MeanDistance);

            // Nothing to choose from.
            Assert.Null(Query.Representative(null, null, null));
            Assert.Null(Query.Representative([], null, null));

            // A single building stands for itself, at no distance from anything.
            representative = Query.Representative(["A"], [new List<double?>() { 1 }], null);
            Assert.Equal(0, representative!.Index);
            Assert.Equal(0, representative.MeanDistance);
        }

        /// <summary>
        /// Tests that the bounded path of <see cref="Query.Representative"/> - more candidates than the exact threshold, only those nearest the median profile evaluated - chooses the same building as the exact evaluation of every candidate, on 2 500 buildings over two parameters.
        /// </summary>
        [Fact]
        public void Representative_Sampled()
        {
            Random random = new(42);

            List<string?> references = [];
            List<double?> values_X = [];
            List<double?> values_Y = [];
            for (int i = 0; i < 2500; i++)
            {
                references.Add("R" + i.ToString("D4", System.Globalization.CultureInfo.InvariantCulture));
                values_X.Add(random.NextDouble() * 100);
                values_Y.Add(System.Math.Pow(random.NextDouble(), 2) * 500);
            }

            Representative? representative_Exact = Query.Representative(references, [values_X, values_Y], null, int.MaxValue);
            Representative? representative_Sampled = Query.Representative(references, [values_X, values_Y], null);

            Assert.NotNull(representative_Exact);
            Assert.NotNull(representative_Sampled);
            Assert.False(representative_Exact!.Sampled);
            Assert.True(representative_Sampled!.Sampled);
            Assert.Equal(2500, representative_Sampled.CandidateCount);
            Assert.Equal(representative_Exact.Index, representative_Sampled.Index);
            Assert.Equal(representative_Exact.MeanDistance, representative_Sampled.MeanDistance, 12);
        }

        /// <summary>
        /// Tests <see cref="Query.Quantile"/>: linear interpolation between ranks (so the 0.5 quantile is the median of an even set), non-finite values ignored, and NaN for no value or a probability outside [0, 1].
        /// </summary>
        [Fact]
        public void Quantile()
        {
            List<double> values = [4, 1, 3, 2];
            Assert.Equal(2.5, values.Quantile(0.5), 12);
            Assert.Equal(1.75, values.Quantile(0.25), 12);
            Assert.Equal(1, values.Quantile(0), 12);
            Assert.Equal(4, values.Quantile(1), 12);
            Assert.Equal(2.5, new List<double>() { 1, double.NaN, 2, 3, double.PositiveInfinity, 4 }.Quantile(0.5), 12);
            Assert.True(double.IsNaN(new List<double>().Quantile(0.5)));
            Assert.True(double.IsNaN(((List<double>?)null).Quantile(0.5)));
            Assert.True(double.IsNaN(values.Quantile(1.5)));
        }
    }
}
