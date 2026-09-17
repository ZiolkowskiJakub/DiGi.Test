using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.OccupancySubdivisionId(int, IReadOnlyDictionary{int, List{int}}, IReadOnlyDictionary{int, uint?})"/> names the subdivision itself when it carries a figure, the smallest figured container otherwise - skipping containers without one - and nothing when no figure exists anywhere up the chain; an explicit zero counts as a figure.
        /// </summary>
        [Fact]
        public void OccupancySubdivisionId_NearestFiguredContainer()
        {
            // Warsaw's ids: Jelonki 55599 (no figure) inside Bemowo 55626 (116 180) inside Warszawa 55489 (1 622 594);
            // Chrzanów 55434 (no figure) inside an unfigured intermediate 1 inside Bemowo; Ugory 55499 (no figure) whose whole chain is unfigured.
            Dictionary<int, List<int>> containerIds_BySubdivisionId = new()
            {
                [55489] = [],
                [55626] = [55489],
                [55599] = [55626, 55489],
                [1] = [55626, 55489],
                [55434] = [1, 55626, 55489],
                [2] = [],
                [55499] = [2],
                [3] = [],
            };

            Dictionary<int, uint?> occupancies_BySubdivisionId = new()
            {
                [55489] = 1622594,
                [55626] = 116180,
                [55599] = null,
                [1] = null,
                [55434] = null,
                [2] = null,
                [55499] = null,
                [3] = 0,
            };

            Assert.Equal(55626, Query.OccupancySubdivisionId(55626, containerIds_BySubdivisionId, occupancies_BySubdivisionId));
            Assert.Equal(55626, Query.OccupancySubdivisionId(55599, containerIds_BySubdivisionId, occupancies_BySubdivisionId));
            Assert.Equal(55626, Query.OccupancySubdivisionId(55434, containerIds_BySubdivisionId, occupancies_BySubdivisionId));
            Assert.Equal(55489, Query.OccupancySubdivisionId(55489, containerIds_BySubdivisionId, occupancies_BySubdivisionId));
            Assert.Null(Query.OccupancySubdivisionId(55499, containerIds_BySubdivisionId, occupancies_BySubdivisionId));
            Assert.Equal(3, Query.OccupancySubdivisionId(3, containerIds_BySubdivisionId, occupancies_BySubdivisionId));

            // Unknown to the nesting: only its own figure can answer.
            Assert.Null(Query.OccupancySubdivisionId(999, containerIds_BySubdivisionId, occupancies_BySubdivisionId));
            Assert.Null(Query.OccupancySubdivisionId(55599, null, occupancies_BySubdivisionId));
            Assert.Equal(55626, Query.OccupancySubdivisionId(55626, null, occupancies_BySubdivisionId));
            Assert.Null(Query.OccupancySubdivisionId(55626, containerIds_BySubdivisionId, null));
        }
    }
}
