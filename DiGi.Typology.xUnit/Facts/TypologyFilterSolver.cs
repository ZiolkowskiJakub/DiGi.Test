using DiGi.Core.Classes;
using DiGi.Typology.Classes;
using DiGi.Typology.xUnit.Classes;

namespace DiGi.Typology.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Unit test verifying that the <see cref="TypologyFilterSolver{TTypologyFilter, TObject}"/> correctly solves typologies for a set of test objects.
        /// </summary>
        [Fact]
        public void TypologyFilterSolver()
        {
            List<UniqueObjectTest> uniqueObjectTests = [];

            UniqueObjectTest uniqueObjectTest;

            uniqueObjectTest = new UniqueObjectTest("1");
            uniqueObjectTest["AAA"] = 10;
            uniqueObjectTest["BBB"] = "Test 1";
            uniqueObjectTest["CCC"] = "Group 1";
            uniqueObjectTests.Add(uniqueObjectTest);

            uniqueObjectTest = new UniqueObjectTest("2");
            uniqueObjectTest["AAA"] = 100;
            uniqueObjectTest["BBB"] = "Test 2";
            uniqueObjectTest["CCC"] = "Group 1";
            uniqueObjectTests.Add(uniqueObjectTest);

            uniqueObjectTest = new UniqueObjectTest("3");
            uniqueObjectTest["AAA"] = 100;
            uniqueObjectTest["BBB"] = "Test 3";
            uniqueObjectTest["CCC"] = "Group 1";
            uniqueObjectTests.Add(uniqueObjectTest);

            uniqueObjectTest = new UniqueObjectTest("4");
            uniqueObjectTest["AAA"] = 99;
            uniqueObjectTest["BBB"] = "Test 4";
            uniqueObjectTest["CCC"] = "Group 2";
            uniqueObjectTests.Add(uniqueObjectTest);

            TypologyFilterTest typologyFilterTest;
            TypologyFilterTest typologyFilterTest_1;
            TypologyFilterTest typologyFilterTest_2;

            typologyFilterTest = new()
            {
                Value = "AAA",
                Rule = new DoubleRangeFilterRule([new Range<double>(0, 20), new Range<double>(20, 200)])
            };

            typologyFilterTest_1 = new()
            {
                Value = "CCC",
                Rule = new UniqueValueFilterRule()
            };

            typologyFilterTest.Filter = typologyFilterTest_1;

            typologyFilterTest_2 = new()
            {
                Value = "BBB",
                Rule = new UniqueValueFilterRule()
            };

            typologyFilterTest_1 = typologyFilterTest_2;

            TypologyFilterSolverTest typologyFilterSolverTest = new()
            {
                Input = typologyFilterTest,
                Objects = uniqueObjectTests
            };

            bool solved = typologyFilterSolverTest.Solve();

            Assert.True(solved);

            Typology.Classes.Typology? typology = typologyFilterSolverTest.Output;
            Assert.NotNull(typology);

            HashSet<string> references = typology.GetReferences(true);
            Assert.NotNull(references);
            Assert.Equal(4, references.Count);

            List<TypologyPath>? typologyPaths = typology.GetTypologyPaths(true);
            Assert.NotNull(typologyPaths);

            for (int i = typologyPaths.Count - 1; i >= 0; i--)
            {
                Typology.Classes.Typology? typology_Temp = typology.GetTypology(typologyPaths[i]);
                Assert.NotNull(typology_Temp);

                if (typology_Temp.References is null || typology_Temp.References.Count < 2)
                {
                    continue;
                }

                Assert.True(typology_Temp.References.Contains("2") && typology_Temp.References.Contains("3"));
            }
        }
    }
}