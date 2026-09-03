using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a null county reference list yields an empty sibling map without throwing.
        /// </summary>
        [Fact]
        public void SiblingCountyGroups_NullInputIsEmpty()
        {
            Dictionary<int, HashSet<int>> siblingCountyGroups = Query.SiblingCountyGroups(null);

            Assert.NotNull(siblingCountyGroups);
            Assert.Empty(siblingCountyGroups);
        }

        /// <summary>
        /// Verifies that a part with a unique code and a part with no usable code each group with themselves.
        /// </summary>
        [Fact]
        public void SiblingCountyGroups_SinglePartAndCodelessPart()
        {
            List<AdministrativeAreal2DReference> countyReferences =
            [
                new() { Id = 73482, Code = "2212" },
                new() { Id = 90000, Code = null },
                new() { Id = 90001, Code = "  " }
            ];

            Dictionary<int, HashSet<int>> siblingCountyGroups = Query.SiblingCountyGroups(countyReferences);

            Assert.Single(siblingCountyGroups[73482]);
            Assert.Contains(73482, siblingCountyGroups[73482]);

            Assert.Single(siblingCountyGroups[90000]);
            Assert.Contains(90000, siblingCountyGroups[90000]);

            Assert.Single(siblingCountyGroups[90001]);
            Assert.Contains(90001, siblingCountyGroups[90001]);
        }

        /// <summary>
        /// Verifies that a multi-part county - several rows sharing one code - maps every part to the full set of parts, because a county code is not a key.
        /// <para>2212 (słupski) is stored as two parts in the live data, so both parts see each other as siblings; an unrelated code is not swept in.</para>
        /// </summary>
        [Fact]
        public void SiblingCountyGroups_MultiPartCountySharesGroup()
        {
            List<AdministrativeAreal2DReference> countyReferences =
            [
                new() { Id = 73482, Code = "2212" },
                new() { Id = 73485, Code = "2212" },
                new() { Id = 50000, Code = "0418" }
            ];

            Dictionary<int, HashSet<int>> siblingCountyGroups = Query.SiblingCountyGroups(countyReferences);

            Assert.Equal(2, siblingCountyGroups[73482].Count);
            Assert.Contains(73482, siblingCountyGroups[73482]);
            Assert.Contains(73485, siblingCountyGroups[73482]);

            Assert.Equal(2, siblingCountyGroups[73485].Count);
            Assert.Contains(73482, siblingCountyGroups[73485]);
            Assert.Contains(73485, siblingCountyGroups[73485]);

            Assert.Single(siblingCountyGroups[50000]);
            Assert.Contains(50000, siblingCountyGroups[50000]);
        }

        /// <summary>
        /// Verifies that a null entry inside the reference list is skipped rather than faulting the run.
        /// </summary>
        [Fact]
        public void SiblingCountyGroups_SkipsNullEntries()
        {
            List<AdministrativeAreal2DReference> countyReferences = [null!, new() { Id = 73482, Code = "2212" }];

            Dictionary<int, HashSet<int>> siblingCountyGroups = Query.SiblingCountyGroups(countyReferences);

            Assert.Single(siblingCountyGroups);
            Assert.Contains(73482, siblingCountyGroups[73482]);
        }

        /// <summary>
        /// Verifies the loop and the fallback share one definition of a sibling part: feeding the helper's output to <see cref="Query.InScopeSubdivisionIds"/> reproduces the in-scope map a hand-built group would produce.
        /// <para>This is the differential guard for the extraction - the same multi-part county must in-scope its subdivision under every part of the group, and under no other part.</para>
        /// </summary>
        [Fact]
        public void SiblingCountyGroups_FeedsInScopeSubdivisionIds()
        {
            // 77971 is an unrelated part that must NOT inherit the subdivision - the differential guard for the extraction.
            List<AdministrativeAreal2DReference> countyReferences =
            [
                new() { Id = 73482, Code = "2212" },
                new() { Id = 73485, Code = "2212" },
                new() { Id = 77971, Code = "2401" }
            ];

            List<AdministrativeAreal2DReference> subdivisions =
            [
                new() { Id = 50000, CountyId = 73482 }
            ];

            Dictionary<int, HashSet<int>> siblingCountyGroups = Query.SiblingCountyGroups(countyReferences);
            Dictionary<int, HashSet<int>> inScopeSubdivisionIds = Query.InScopeSubdivisionIds(subdivisions, siblingCountyGroups);

            Assert.Contains(50000, inScopeSubdivisionIds[73482]);
            Assert.Contains(50000, inScopeSubdivisionIds[73485]);

            // 77971 is a real part that must NOT inherit the subdivision: the loop would not reach a building filed under it.
            Assert.DoesNotContain(77971, inScopeSubdivisionIds);
        }
    }
}
