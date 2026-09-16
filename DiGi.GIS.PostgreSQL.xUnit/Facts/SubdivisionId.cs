using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the write-side attribution rule, <see cref="Query.SubdivisionId(IEnumerable{System.ValueTuple{int, double, double}}, double)"/>: a building wholly inside a neighbourhood, its district and its city - three candidates with the same overlap, the building's own area - is filed under the smallest of them, whatever their identifiers.
        /// <para>The candidates are given in the order Warsaw's ids produce them: the city (55489) below the district (55626) below the neighbourhood (55599 is lower than the district but higher than the city). The previous lowest-identifier tie-break answered 55489 here and filed 57 551 Warsaw buildings under the city row (<see href="https://github.com/ZiolkowskiJakub/DiGi.GIS.PostgreSQL/issues/77">DiGi.GIS.PostgreSQL#77</see>).</para>
        /// </summary>
        [Fact]
        public void SubdivisionId_NestedContainers_SmallestWins()
        {
            double area_Building = 250.0;

            List<(int Id, double Area_Intersection, double Area_Container)> tuples =
            [
                (55489, area_Building, 516_700_000),
                (55626, area_Building, 24_900_000),
                (55599, area_Building, 3_100_000),
            ];

            Assert.Equal(55599, Query.SubdivisionId(tuples));
        }

        /// <summary>
        /// Verifies that the overlap still decides where the building straddles a boundary: a building mostly inside a large area and only clipping a small neighbour is filed under the large one, because the largest overlap is the first criterion and the container size only breaks ties within the tolerance.
        /// <para>Two overlaps a tolerance apart count as one, and the identifier decides only between two containers of the same size to the tolerance.</para>
        /// </summary>
        [Fact]
        public void SubdivisionId_Straddling_LargestOverlapFirst()
        {
            double tolerance = Core.Constants.Tolerance.MacroDistance;

            List<(int Id, double Area_Intersection, double Area_Container)> tuples =
            [
                (1, 200.0, 1_000_000),
                (2, 50.0, 1_000),
            ];

            Assert.Equal(1, Query.SubdivisionId(tuples, tolerance));

            List<(int Id, double Area_Intersection, double Area_Container)> tuples_Band =
            [
                (7, 200.0, 5_000),
                (3, 200.0 - tolerance / 2, 4_000),
            ];

            Assert.Equal(3, Query.SubdivisionId(tuples_Band, tolerance));

            List<(int Id, double Area_Intersection, double Area_Container)> tuples_Equal =
            [
                (9, 200.0, 4_000),
                (4, 200.0, 4_000 + tolerance / 2),
            ];

            Assert.Equal(4, Query.SubdivisionId(tuples_Equal, tolerance));
        }

        /// <summary>
        /// Verifies the empty answers: no candidates, or none with a positive overlap, is <see langword="null"/> rather than an exception or an arbitrary identifier.
        /// </summary>
        [Fact]
        public void SubdivisionId_NoCandidate_ReturnsNull()
        {
            Assert.Null(Query.SubdivisionId(null));
            Assert.Null(Query.SubdivisionId([]));
            Assert.Null(Query.SubdivisionId([(1, 0.0, 100.0), (2, -1.0, 100.0)]));
        }
    }
}
