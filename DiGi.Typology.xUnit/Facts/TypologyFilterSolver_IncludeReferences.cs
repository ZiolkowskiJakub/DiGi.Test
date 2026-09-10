using DiGi.Core.Classes;
using DiGi.Typology.Classes;
using DiGi.Typology.xUnit.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that solving with IncludeReferences false produces the same tree structure and node metadata as solving with it true, while leaving every node's reference set empty.
        /// <para>Objects are grouped by their filter rule data rather than by their references, so switching references off must not move a single node.</para>
        /// </summary>
        [Fact]
        public void TypologyFilterSolver_IncludeReferences()
        {
            List<UniqueObjectTest> uniqueObjectTests = [];

            UniqueObjectTest uniqueObjectTest;

            uniqueObjectTest = new UniqueObjectTest("1");
            uniqueObjectTest["AAA"] = 10;
            uniqueObjectTest["CCC"] = "Group 1";
            uniqueObjectTests.Add(uniqueObjectTest);

            uniqueObjectTest = new UniqueObjectTest("2");
            uniqueObjectTest["AAA"] = 100;
            uniqueObjectTest["CCC"] = "Group 1";
            uniqueObjectTests.Add(uniqueObjectTest);

            uniqueObjectTest = new UniqueObjectTest("3");
            uniqueObjectTest["AAA"] = 99;
            uniqueObjectTest["CCC"] = "Group 2";
            uniqueObjectTests.Add(uniqueObjectTest);

            TypologyFilterTest typologyFilterTest = new()
            {
                Value = "AAA",
                Rule = new DoubleRangeFilterRule([new Range<double>(0, 20), new Range<double>(21, 200)]),
                Filter = new TypologyFilterTest()
                {
                    Value = "CCC",
                    Rule = new UniqueValueFilterRule()
                }
            };

            Typology.Classes.Typology typology_WithReferences = Solve(typologyFilterTest, uniqueObjectTests, true);
            Typology.Classes.Typology typology_WithoutReferences = Solve(typologyFilterTest, uniqueObjectTests, false);

            Assert.Equal(3, typology_WithReferences.ReferenceSet(true).Count);
            Assert.Empty(typology_WithoutReferences.ReferenceSet(true));

            List<TypologyPath> typologyPaths_WithReferences = typology_WithReferences.TypologyPaths(true);
            List<TypologyPath> typologyPaths_WithoutReferences = typology_WithoutReferences.TypologyPaths(true);

            Assert.NotEmpty(typologyPaths_WithReferences);
            Assert.Equal(typologyPaths_WithReferences.Count, typologyPaths_WithoutReferences.Count);

            foreach (TypologyPath typologyPath in typologyPaths_WithReferences)
            {
                Typology.Classes.Typology? typology_WithReference = typology_WithReferences.SubTypology(typologyPath);
                Typology.Classes.Typology? typology_WithoutReference = typology_WithoutReferences.SubTypology(typologyPath);

                Assert.NotNull(typology_WithReference);
                Assert.NotNull(typology_WithoutReference);
                Assert.Equal(typology_WithReference.Name, typology_WithoutReference.Name);
                Assert.NotEmpty(typology_WithReference.References);
                Assert.Empty(typology_WithoutReference.References);
            }

            static Typology.Classes.Typology Solve(TypologyFilterTest typologyFilterTest, List<UniqueObjectTest> uniqueObjectTests, bool includeReferences)
            {
                TypologyFilterSolverTest typologyFilterSolverTest = new()
                {
                    Input = typologyFilterTest,
                    Objects = uniqueObjectTests,
                    IncludeReferences = includeReferences
                };

                Assert.True(typologyFilterSolverTest.Solve());
                Assert.NotNull(typologyFilterSolverTest.Output);

                return typologyFilterSolverTest.Output;
            }
        }
    }
}
