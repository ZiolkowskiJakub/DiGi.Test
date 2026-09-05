using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that the count of subjects returned by a read of their own surroundings separates a partition that was not read from a building that fell out of it.
        /// <para>The building data update measures the radial ratios against whatever a bounding box read brings back, and that read used to be able to answer an empty set without an error. Zero subjects back has to be distinguishable from a few missing, because the first means the county partition is not being reached at all and the second means one stored bounding box does not match its own geometry - the first leaves a whole subdivision unwritten, the second understates a handful of ratios.</para>
        /// <para>The match is on county and reference together: a reference is unique only within a county, so a neighbouring county holding the same reference must not be read as the subject coming back.</para>
        /// </summary>
        [Fact]
        public void SubjectCount()
        {
            Building2D building2D_A = new() { CountyId = 8948, Reference = "reference_a" };
            Building2D building2D_B = new() { CountyId = 8948, Reference = "reference_b" };
            Building2D building2D_Neighbour = new() { CountyId = 8948, Reference = "reference_neighbour" };

            List<Building2D> building2Ds = [building2D_A, building2D_B];

            // The whole partition is missing: the surroundings hold buildings, but none of the subjects.
            Assert.Equal(0, Query.SubjectCount(building2Ds, [building2D_Neighbour], 8948));

            // One subject fell out of its own neighbourhood.
            Assert.Equal(1, Query.SubjectCount(building2Ds, [building2D_A, building2D_Neighbour], 8948));

            // Everything came back.
            Assert.Equal(2, Query.SubjectCount(building2Ds, [building2D_A, building2D_B, building2D_Neighbour], 8948));

            // The same reference under a sibling part is a different building and does not count.
            Building2D building2D_A_OtherCounty = new() { CountyId = 8957, Reference = "reference_a" };
            Assert.Equal(0, Query.SubjectCount(building2Ds, [building2D_A_OtherCounty], 8948));

            // A subject counted once however many times the surroundings hold it.
            Assert.Equal(1, Query.SubjectCount(building2Ds, [building2D_A, building2D_A], 8948));

            // Degenerate inputs answer zero rather than throwing.
            Assert.Equal(0, Query.SubjectCount(null, [building2D_A], 8948));
            Assert.Equal(0, Query.SubjectCount(building2Ds, null, 8948));
            Assert.Equal(0, Query.SubjectCount([], [building2D_A], 8948));
            Assert.Equal(0, Query.SubjectCount([new Building2D() { CountyId = 8948, Reference = "   " }], [building2D_A], 8948));
        }
    }
}
