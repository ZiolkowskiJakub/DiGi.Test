using DiGi.Core.Classes;
using DiGi.Core.IO.Table.Classes;
using DiGi.Typology.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a value on the edge of a range bucket lands in the bucket that declares it, and that a value the ranges do not cover reaches no bucket at all.
        /// <para>Buckets are matched by name rather than by index, because a node is filed in the order its bucket was first encountered rather than in the order the ranges were declared.</para>
        /// </summary>
        [Fact]
        public void Typology_RangeBoundaries()
        {
            Table table = TypologyTable();

            ColumnTypologyFilter<Column> columnTypologyFilter = new()
            {
                Value = IO.Constants.Column.PredictedYearBuilt,
                Rule = new IntegerRangeFilterRule([new Range<int>(0, 2003), new Range<int>(2004, 2020), new Range<int>(2021, int.MaxValue)])
            };

            Typology.Classes.Typology? typology = Create.Typology(table, columnTypologyFilter, IO.Constants.Column.Reference);

            Assert.NotNull(typology);

            AssertBucket(typology, "Predicted year built (0,2003>", ["b1", "b2"]);
            AssertBucket(typology, "Predicted year built (2004,2020>", ["b3", "b4"]);
            AssertBucket(typology, "Predicted year built (2021,2147483647>", ["b5", "b6"]);

            Assert.DoesNotContain("b7", Typology.Query.ReferenceSet(typology, true));

            static void AssertBucket(Typology.Classes.Typology typology, string name, string[] references)
            {
                Assert.True(Typology.Query.TryGetTypologies(typology, name, out List<Typology.Classes.Typology>? typologies));
                Assert.NotNull(typologies);
                Assert.Single(typologies);

                List<string> references_Node = typologies[0].References;
                references_Node.Sort();

                Assert.Equal(references, references_Node);
            }
        }
    }
}
