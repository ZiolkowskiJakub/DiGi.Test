using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that Query.UnknownCountyIds separates identifiers that are county rows from identifiers that are not, and recognises a four character county code passed where an identifier was wanted.
        /// <para>This is the guard that stops a mis-scoped run: an identifier in no county row matches no stored building, so every step downstream reports a legitimate zero and the run ends green having done nothing.</para>
        /// </summary>
        [Fact]
        public void UnknownCountyIds()
        {
            List<AdministrativeAreal2DReference> administrativeAreal2DReferences =
            [
                new() { Id = 73482, Code = "2212" },
                new() { Id = 73485, Code = "2212" },
                new() { Id = 5, Code = "0201" }
            ];

            Dictionary<int, List<int>> countyIds_Unknown = Query.UnknownCountyIds(administrativeAreal2DReferences, [73482, 73485, 5]);
            Assert.Empty(countyIds_Unknown);

            //2212 is the code of a county held as two polygon parts, not the identifier of either of them
            countyIds_Unknown = Query.UnknownCountyIds(administrativeAreal2DReferences, [2212]);
            Assert.Single(countyIds_Unknown);
            Assert.Equal([73482, 73485], countyIds_Unknown[2212]);

            //A stored code is zero padded to four characters, so the plain decimal form has to be tried as well
            countyIds_Unknown = Query.UnknownCountyIds(administrativeAreal2DReferences, [201]);
            Assert.Single(countyIds_Unknown);
            Assert.Equal([5], countyIds_Unknown[201]);

            //Neither an identifier nor a code - nothing to suggest, but still not a county
            countyIds_Unknown = Query.UnknownCountyIds(administrativeAreal2DReferences, [999999]);
            Assert.Single(countyIds_Unknown);
            Assert.Empty(countyIds_Unknown[999999]);

            //A known identifier alongside an unknown one leaves only the unknown one reported
            countyIds_Unknown = Query.UnknownCountyIds(administrativeAreal2DReferences, [73485, 2212]);
            Assert.Single(countyIds_Unknown);
            Assert.True(countyIds_Unknown.ContainsKey(2212));

            Assert.Empty(Query.UnknownCountyIds(null, [2212]));
            Assert.Empty(Query.UnknownCountyIds(administrativeAreal2DReferences, null));
        }

        /// <summary>
        /// Verifies that Query.SiblingCountyIds resolves each named county row to every polygon part of its county, so a write is filed under the part its reference belongs to rather than under whichever part happened to be named.
        /// </summary>
        [Fact]
        public void SiblingCountyIds()
        {
            List<AdministrativeAreal2DReference> administrativeAreal2DReferences =
            [
                new() { Id = 73485, Code = "2212" },
                new() { Id = 73482, Code = "2212" },
                new() { Id = 5, Code = "0201" }
            ];

            Dictionary<int, List<int>> countyIds_Siblings = Query.SiblingCountyIds(administrativeAreal2DReferences, [73485]);
            Assert.Single(countyIds_Siblings);

            //Naming one part answers with both, ordered ascending
            Assert.Equal([73482, 73485], countyIds_Siblings[73485]);

            countyIds_Siblings = Query.SiblingCountyIds(administrativeAreal2DReferences, [5]);
            Assert.Equal([5], countyIds_Siblings[5]);

            //An identifier that is in no county row is left out rather than guessed at
            countyIds_Siblings = Query.SiblingCountyIds(administrativeAreal2DReferences, [2212]);
            Assert.Empty(countyIds_Siblings);

            Assert.Empty(Query.SiblingCountyIds(null, [73485]));
            Assert.Empty(Query.SiblingCountyIds(administrativeAreal2DReferences, null));
        }
    }
}
