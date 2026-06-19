using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.Typology.Classes;
using DiGi.Typology.Interfaces;
using System.Text.Json.Nodes;

namespace DiGi.Typology.xUnit
{
    public partial class Classes
    {
        public class FilterUniqueObjectTest : UniqueObject
        {
            public FilterUniqueObjectTest(string name)
                :base()
            {
                Name = name;
            }

            public string Name { get; set; }

            public override string? UniqueId => Name;
        }

        public class UniqueObjectTest : UniqueObject
        {
            private Dictionary<string, object?> dictionary = [];

            public UniqueObjectTest(string uniqueId)
                : base()
            {
                UniqueId = uniqueId;
            }

            public object? GetValue(string? name)
            {
                if(name is null)
                {
                    return null;
                }

                if(dictionary.TryGetValue(name, out object? result))
                {
                    return result;
                }

                return null;
            }

            public bool SetValue(string? name, object? value)
            {
                if (name is null)
                {
                    return false;
                }

                dictionary[name] = value;
                return true;
            }

            public override string? UniqueId { get; }

            public object? this[string name]
            {
                get
                {
                    return GetValue(name);
                }

                set
                {
                    SetValue(name, value);
                }
            }
        }




        /// <summary>
        /// A specialized, completely non-generic typology filter tailored specifically for ModelElement objects.
        /// </summary>
        public class TypologyFilterTest : TypologyFilter<TypologyFilterTest, string>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TypologyFilterTest"/> class.
            /// </summary>
            public TypologyFilterTest()
                : base()
            {
            }

            /// <summary>
            /// Initializes a new instance from a JsonObject for serialization/deserialization.
            /// </summary>
            /// <param name="jsonObject">The JSON object containing filter data.</param>
            public TypologyFilterTest(JsonObject jsonObject)
                : base(jsonObject)
            {
            }

            /// <summary>
            /// Copy constructor for deep cloning capabilities.
            /// </summary>
            /// <param name="typologyFilterTest">The source filter instance to copy from.</param>
            public TypologyFilterTest(TypologyFilterTest typologyFilterTest)
                : base(typologyFilterTest)
            {
            }
        }


        public class TypologyFilterSolverTest : TypologyFilterSolver<TypologyFilterTest, UniqueObjectTest>
        {
            protected override TypologyItem? GetTypologyItem(TypologyFilterTest? typologyFilter, ITypologyFilterRuleData? typologyFilterRuleData)
            {
                if (typologyFilter is null || typologyFilterRuleData is null)
                {
                    return null;
                }

                TypologyItem result = new()
                {
                    Name = $"{typologyFilter?.Value ?? string.Empty} {typologyFilterRuleData}",
                };

                return result;
            }

            protected override object? GetValue(TypologyFilterTest? typologyFilter, UniqueObjectTest? uniqueObject)
            {
                if(typologyFilter?.Value is not string name || uniqueObject is null)
                {
                    return null;
                }

                return uniqueObject.GetValue(name);
            }
        }


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

            TypologyFilterSolverTest typologyFilterSolverTest = new ()           
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

            for(int i = typologyPaths.Count - 1; i >= 0; i--)
            {
                Typology.Classes.Typology? typology_Temp = typology.GetTypology(typologyPaths[i]);
                Assert.NotNull(typology_Temp);

                if(typology_Temp.References is null || typology_Temp.References.Count < 2)
                {
                    continue;
                }

                Assert.True(typology_Temp.References.Contains("2") && typology_Temp.References.Contains("3"));
            }
        }
    }
}
