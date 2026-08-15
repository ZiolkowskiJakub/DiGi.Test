using System;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the same run seed and county row identifier draw the same sample twice.
        /// <para>This is what makes a verification run before a change comparable with one after it.</para>
        /// </summary>
        [Fact]
        public void Sample_RepeatableForTheSameCounty()
        {
            List<string> references = References(1000);

            List<string>? references_A = references.Sample(200, new Random(Query.RandomSeed(20260811, 73485)));
            List<string>? references_B = references.Sample(200, new Random(Query.RandomSeed(20260811, 73485)));

            Assert.NotNull(references_A);
            Assert.NotNull(references_B);
            Assert.Equal(references_A, references_B);
        }

        /// <summary>
        /// Verifies that two counties sharing a run seed draw different samples.
        /// <para>Combining the seed with the county row identifier is what keeps a county's draw its own; a mix that ignored the identifier would sample the same positions in every county.</para>
        /// </summary>
        [Fact]
        public void Sample_DiffersBetweenCounties()
        {
            List<string> references = References(1000);

            List<string>? references_A = references.Sample(200, new Random(Query.RandomSeed(20260811, 73485)));
            List<string>? references_B = references.Sample(200, new Random(Query.RandomSeed(20260811, 73486)));

            Assert.NotNull(references_A);
            Assert.NotNull(references_B);
            Assert.NotEqual(references_A, references_B);
        }

        /// <summary>
        /// Verifies that a county's draw does not depend on what any other county holds.
        /// <para>The draw consumes one value per item returned, so a single generator advanced across counties made every county's sample depend on the population of the counties before it. The 2026-08-14 county part repair took three counties from tens of thousands of references to a handful, which shifted the sample of every county after them. Seeding per county is what removes that.</para>
        /// </summary>
        [Fact]
        public void Sample_IndependentOfPrecedingCounties()
        {
            List<string> references = References(1000);

            // One generator shared across counties: the neighbour's population decides where this county's
            // draw starts, so the same county samples differently depending on what came before it.
            Random random_Shared_AfterLargeNeighbour = new(20260811);
            _ = References(5000).Sample(200, random_Shared_AfterLargeNeighbour);

            Random random_Shared_AfterSmallNeighbour = new(20260811);
            _ = References(3).Sample(200, random_Shared_AfterSmallNeighbour);

            Assert.NotEqual(references.Sample(200, random_Shared_AfterLargeNeighbour), references.Sample(200, random_Shared_AfterSmallNeighbour));

            // Seeded per county, the neighbour cannot reach it either way.
            Assert.Equal(references.Sample(200, new Random(Query.RandomSeed(20260811, 73485))), references.Sample(200, new Random(Query.RandomSeed(20260811, 73485))));
        }

        /// <summary>
        /// Verifies that a sample size at or above the population, or below one, returns every item.
        /// </summary>
        [Fact]
        public void Sample_DegenerateSizes()
        {
            List<string> references = References(10);

            foreach (int sampleSize in new int[] { 10, 200, 0, -5 })
            {
                List<string>? references_Sample = references.Sample(sampleSize, new Random(1));

                Assert.NotNull(references_Sample);
                Assert.Equal(10, references_Sample.Count);
            }
        }

        /// <summary>
        /// Verifies that no item is drawn twice and that every drawn item came from the population.
        /// </summary>
        [Fact]
        public void Sample_DrawsWithoutRepetition()
        {
            List<string> references = References(1000);

            List<string>? references_Sample = references.Sample(200, new Random(Query.RandomSeed(20260811, 5)));

            Assert.NotNull(references_Sample);
            Assert.Equal(200, references_Sample.Count);
            Assert.Equal(200, new HashSet<string>(references_Sample).Count);
            Assert.True(new HashSet<string>(references).IsSupersetOf(references_Sample));
        }

        /// <summary>
        /// Verifies that a null population or a null random source returns nothing rather than throwing.
        /// </summary>
        [Fact]
        public void Sample_NullInputs()
        {
            Assert.Null(Query.Sample<string>(null, 200, new Random(1)));
            Assert.Null(Query.Sample(References(10), 200, null));
        }

        /// <summary>
        /// Verifies that the seed combination is stable, so a run today and a run in a later process agree.
        /// <para>Pinned as a literal on purpose: a mix built on <see cref="HashCode.Combine{T1, T2}(T1, T2)"/> passes every other fact here and fails this one, because it stirs in a seed randomized per process.</para>
        /// </summary>
        [Fact]
        public void RandomSeed_Stable()
        {
            Assert.Equal(unchecked((20260811 * 397) + 73485), Query.RandomSeed(20260811, 73485));
            Assert.Equal(Query.RandomSeed(20260811, 5), Query.RandomSeed(20260811, 5));
            Assert.NotEqual(Query.RandomSeed(20260811, 5), Query.RandomSeed(20260811, 6));
        }

        /// <summary>
        /// Builds a population of distinct references for the sampling facts.
        /// </summary>
        /// <param name="count">The number of references to build.</param>
        /// <returns>The references.</returns>
        private static List<string> References(int count)
        {
            List<string> result = new(count);
            for (int i = 0; i < count; i++)
            {
                result.Add($"reference_{i}");
            }

            return result;
        }
    }
}
