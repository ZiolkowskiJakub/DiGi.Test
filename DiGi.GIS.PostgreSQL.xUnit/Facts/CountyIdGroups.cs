using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a null county reference list yields an empty group list without throwing.
        /// </summary>
        [Fact]
        public void CountyIdGroups_NullInputIsEmpty()
        {
            List<List<int>> countyIdGroups = Query.CountyIdGroups(null);

            Assert.NotNull(countyIdGroups);
            Assert.Empty(countyIdGroups);
        }

        /// <summary>
        /// Verifies that parts sharing one code form one group, that each group is sorted, and that the groups are ordered by their lowest identifier.
        /// <para>2212 (słupski) is stored as two parts in the live data; 0418 stands alone here. The order of the references in must not change the order of the groups out.</para>
        /// </summary>
        [Fact]
        public void CountyIdGroups_GroupsByCodeAndOrdersByLowestIdentifier()
        {
            List<AdministrativeAreal2DReference> countyReferences =
            [
                new() { Id = 50000, Code = "0418" },
                new() { Id = 73485, Code = "2212" },
                new() { Id = 73482, Code = "2212" }
            ];

            List<List<int>> countyIdGroups = Query.CountyIdGroups(countyReferences);

            Assert.Equal(2, countyIdGroups.Count);

            Assert.Single(countyIdGroups[0]);
            Assert.Equal(50000, countyIdGroups[0][0]);

            Assert.Equal(2, countyIdGroups[1].Count);
            Assert.Equal(73482, countyIdGroups[1][0]);
            Assert.Equal(73485, countyIdGroups[1][1]);

            List<AdministrativeAreal2DReference> countyReferences_Reversed = [.. countyReferences];
            countyReferences_Reversed.Reverse();

            List<List<int>> countyIdGroups_Reversed = Query.CountyIdGroups(countyReferences_Reversed);

            Assert.Equal(countyIdGroups, countyIdGroups_Reversed);
        }

        /// <summary>
        /// Verifies that a code widens to parts that were never named: a run scoped to one part of a multi-part county must reach its siblings.
        /// <para>This is the widening the terrain tasks exist to use - without it, naming one part of 2212 would sample the whole county and file it under that part alone.</para>
        /// </summary>
        [Fact]
        public void CountyIdGroups_WidensCodeToPartsBeyondTheNamedOnes()
        {
            List<AdministrativeAreal2DReference> countyReferences = [new() { Id = 73485, Code = "2212" }];

            Dictionary<string, HashSet<int>> countyIds_ByCode = new()
            {
                ["2212"] = [73482, 73485]
            };

            List<List<int>> countyIdGroups = Query.CountyIdGroups(countyReferences, countyIds_ByCode);

            Assert.Single(countyIdGroups);
            Assert.Equal(2, countyIdGroups[0].Count);
            Assert.Equal(73482, countyIdGroups[0][0]);
            Assert.Equal(73485, countyIdGroups[0][1]);
        }

        /// <summary>
        /// Verifies that a code the dictionary does not name, or names with nothing, keeps the parts supplied with it.
        /// </summary>
        [Fact]
        public void CountyIdGroups_KeepsSuppliedPartsWhenWideningIsAbsentOrEmpty()
        {
            List<AdministrativeAreal2DReference> countyReferences =
            [
                new() { Id = 73482, Code = "2212" },
                new() { Id = 50000, Code = "0418" }
            ];

            Dictionary<string, HashSet<int>> countyIds_ByCode = new()
            {
                ["0418"] = []
            };

            List<List<int>> countyIdGroups = Query.CountyIdGroups(countyReferences, countyIds_ByCode);

            Assert.Equal(2, countyIdGroups.Count);

            Assert.Single(countyIdGroups[0]);
            Assert.Equal(50000, countyIdGroups[0][0]);

            Assert.Single(countyIdGroups[1]);
            Assert.Equal(73482, countyIdGroups[1][0]);
        }

        /// <summary>
        /// Verifies that a part with no usable code groups with itself.
        /// </summary>
        [Fact]
        public void CountyIdGroups_CodelessPartGroupsAlone()
        {
            List<AdministrativeAreal2DReference> countyReferences =
            [
                new() { Id = 90000, Code = null },
                new() { Id = 90001, Code = "  " }
            ];

            List<List<int>> countyIdGroups = Query.CountyIdGroups(countyReferences);

            Assert.Equal(2, countyIdGroups.Count);

            Assert.Single(countyIdGroups[0]);
            Assert.Equal(90000, countyIdGroups[0][0]);

            Assert.Single(countyIdGroups[1]);
            Assert.Equal(90001, countyIdGroups[1][0]);
        }

        /// <summary>
        /// Verifies that an identifier two widened codes share is kept by the first code only, so no part is walked twice.
        /// </summary>
        [Fact]
        public void CountyIdGroups_SharedIdentifierIsWalkedOnce()
        {
            List<AdministrativeAreal2DReference> countyReferences =
            [
                new() { Id = 73482, Code = "2212" },
                new() { Id = 50000, Code = "0418" }
            ];

            Dictionary<string, HashSet<int>> countyIds_ByCode = new()
            {
                ["2212"] = [73482, 73485],
                ["0418"] = [50000, 73485]
            };

            List<List<int>> countyIdGroups = Query.CountyIdGroups(countyReferences, countyIds_ByCode);

            Assert.Equal(2, countyIdGroups.Count);

            Assert.Single(countyIdGroups[0]);
            Assert.Equal(50000, countyIdGroups[0][0]);

            Assert.Equal(2, countyIdGroups[1].Count);
            Assert.Equal(73482, countyIdGroups[1][0]);
            Assert.Equal(73485, countyIdGroups[1][1]);

            Assert.Equal(3, countyIdGroups[0].Count + countyIdGroups[1].Count);
        }

        /// <summary>
        /// Verifies that null entries inside the reference list are skipped rather than faulting the run.
        /// </summary>
        [Fact]
        public void CountyIdGroups_SkipsNullEntries()
        {
            List<AdministrativeAreal2DReference> countyReferences = [null!, new() { Id = 73482, Code = "2212" }];

            List<List<int>> countyIdGroups = Query.CountyIdGroups(countyReferences);

            Assert.Single(countyIdGroups);
            Assert.Single(countyIdGroups[0]);
            Assert.Equal(73482, countyIdGroups[0][0]);
        }
    }
}
