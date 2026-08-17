using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that every node produced by the origin anchored grid is recognised by <see cref="Query.TryGetGridIndex(Point2D, Point2D, double, double, out int, out int, double)"/> and that rebuilding the node from its indexes gives the coordinate back exactly.
        /// </summary>
        [Fact]
        public void TryGetGridIndex_RoundTrip()
        {
            Point2D origin = new(0, 0);
            double gridSize = 10;

            BoundingBox2D boundingBox2D = new(new Point2D(517333.17, 264881.09), new Point2D(517511.43, 265002.88));

            List<Point2D>? point2Ds = boundingBox2D.Point2Ds(origin, gridSize, gridSize);
            Assert.NotNull(point2Ds);
            Assert.NotEmpty(point2Ds);

            foreach (Point2D point2D in point2Ds)
            {
                Assert.True(point2D.TryGetGridIndex(origin, gridSize, gridSize, out int index_X, out int index_Y));
                Assert.True(point2D.X == origin.X + (index_X * gridSize));
                Assert.True(point2D.Y == origin.Y + (index_Y * gridSize));
            }
        }

        /// <summary>
        /// Tests that a point lying between nodes is rejected rather than snapped to the nearest one, which is what keeps points of another origin from being mistaken for nodes of this grid.
        /// </summary>
        [Fact]
        public void TryGetGridIndex_OffLattice()
        {
            Point2D origin = new(0, 0);
            double gridSize = 10;

            Assert.False(new Point2D(1005, 2000).TryGetGridIndex(origin, gridSize, gridSize, out int _, out int _));
            Assert.False(new Point2D(1000, 2005).TryGetGridIndex(origin, gridSize, gridSize, out int _, out int _));
            Assert.False(new Point2D(1003.27, 2008.91).TryGetGridIndex(origin, gridSize, gridSize, out int _, out int _));

            Assert.True(new Point2D(1000, 2000).TryGetGridIndex(origin, gridSize, gridSize, out int index_X, out int index_Y));
            Assert.Equal(100, index_X);
            Assert.Equal(200, index_Y);
        }

        /// <summary>
        /// Tests that a node of a coarser grid is recognised as a node of a finer grid whose size divides it, and that a node of a grid that does not divide it is not.
        /// </summary>
        [Fact]
        public void TryGetGridIndex_NestedGridSizes()
        {
            Point2D origin = new(0, 0);

            // A 50 metre node is also a 10 metre node.
            Assert.True(new Point2D(1050, 2100).TryGetGridIndex(origin, 10, 10, out int index_X, out int index_Y));
            Assert.Equal(105, index_X);
            Assert.Equal(210, index_Y);

            // A 30 metre node that is not a multiple of 100 is not a 100 metre node.
            Assert.False(new Point2D(1030, 2100).TryGetGridIndex(origin, 100, 100, out int _, out int _));
        }

        /// <summary>
        /// Tests the tolerance boundary on both sides: a point lying within the tolerance of a node is reported as that node, and one lying further away is not.
        /// </summary>
        [Fact]
        public void TryGetGridIndex_ToleranceBoundary()
        {
            Point2D origin = new(0, 0);
            double gridSize = 10;
            double tolerance = 0.01;

            Assert.True(new Point2D(1000 + (tolerance / 2), 2000).TryGetGridIndex(origin, gridSize, gridSize, out int index_X, out int index_Y, tolerance));
            Assert.Equal(100, index_X);
            Assert.Equal(200, index_Y);

            Assert.False(new Point2D(1000 + (tolerance * 2), 2000).TryGetGridIndex(origin, gridSize, gridSize, out int _, out int _, tolerance));
        }

        /// <summary>
        /// Tests that nodes lying below the origin are reported with negative indexes rather than rejected.
        /// </summary>
        [Fact]
        public void TryGetGridIndex_NegativeIndexes()
        {
            Point2D origin = new(0, 0);
            double gridSize = 10;

            Assert.True(new Point2D(-250, -1000).TryGetGridIndex(origin, gridSize, gridSize, out int index_X, out int index_Y));
            Assert.Equal(-25, index_X);
            Assert.Equal(-100, index_Y);
        }

        /// <summary>
        /// Tests that a null point, a null origin and an invalid grid size are all rejected with zeroed indexes.
        /// </summary>
        [Fact]
        public void TryGetGridIndex_InvalidArguments()
        {
            Point2D origin = new(0, 0);
            Point2D point2D = new(1000, 2000);

            Assert.False(((Point2D?)null).TryGetGridIndex(origin, 10, 10, out int index_X, out int index_Y));
            Assert.Equal(0, index_X);
            Assert.Equal(0, index_Y);

            Assert.False(point2D.TryGetGridIndex(null, 10, 10, out int _, out int _));
            Assert.False(point2D.TryGetGridIndex(origin, 0, 10, out int _, out int _));
            Assert.False(point2D.TryGetGridIndex(origin, 10, double.NaN, out int _, out int _));
        }
    }
}
