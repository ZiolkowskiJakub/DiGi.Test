using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that the origin anchored overload of <see cref="Create.Point2Ds(BoundingBox2D, Point2D, double, double, double)"/> places every node at a whole multiple of the grid size regardless of where the bounding box starts.
        /// </summary>
        [Fact]
        public void Point2Ds_OriginAnchored_ExactCoordinates()
        {
            Point2D origin = new(0, 0);
            BoundingBox2D boundingBox2D = new(new Point2D(140000.3, 140000.3), new Point2D(140093.7, 140093.7));

            List<Point2D>? point2Ds = boundingBox2D.Point2Ds(origin, 10, 10);
            Assert.NotNull(point2Ds);

            // 9 columns and 9 rows: indexes 14001 through 14009 on both axes.
            Assert.Equal(81, point2Ds.Count);

            Assert.Equal(140010d, point2Ds[0].X);
            Assert.Equal(140010d, point2Ds[0].Y);
            Assert.Equal(140090d, point2Ds[point2Ds.Count - 1].X);
            Assert.Equal(140090d, point2Ds[point2Ds.Count - 1].Y);

            foreach (Point2D point2D in point2Ds)
            {
                Assert.Equal(0d, point2D.X % 10);
                Assert.Equal(0d, point2D.Y % 10);
            }
        }

        /// <summary>
        /// Tests that two bounding boxes that are not aligned to each other still yield nodes of one shared lattice, so areas generated separately meet without a seam and without overlapping.
        /// </summary>
        [Fact]
        public void Point2Ds_OriginAnchored_AlignsAdjacentBoxes()
        {
            Point2D origin = new(0, 0);

            BoundingBox2D boundingBox2D_1 = new(new Point2D(1003.7, 0), new Point2D(1096.2, 40));
            BoundingBox2D boundingBox2D_2 = new(new Point2D(1096.2, 0), new Point2D(1188.4, 40));

            List<Point2D>? point2Ds_1 = boundingBox2D_1.Point2Ds(origin, 10, 10);
            List<Point2D>? point2Ds_2 = boundingBox2D_2.Point2Ds(origin, 10, 10);
            Assert.NotNull(point2Ds_1);
            Assert.NotNull(point2Ds_2);

            HashSet<double> xs_1 = [.. point2Ds_1.ConvertAll(x => x.X)];
            HashSet<double> xs_2 = [.. point2Ds_2.ConvertAll(x => x.X)];

            // 1100 falls in both boxes but only the second box may claim it, and no column may be claimed twice.
            Assert.DoesNotContain(1100d, xs_1);
            Assert.Contains(1100d, xs_2);
            Assert.Empty(xs_1.Intersect(xs_2));

            List<double> xs_All = [.. xs_1.Union(xs_2)];
            xs_All.Sort();

            for (int i = 1; i < xs_All.Count; i++)
            {
                Assert.Equal(10d, xs_All[i] - xs_All[i - 1]);
            }
        }

        /// <summary>
        /// Tests that generating the same bounding box twice yields the same coordinates bit for bit, which is what lets a run be repeated without shifting the nodes it already produced.
        /// </summary>
        [Fact]
        public void Point2Ds_OriginAnchored_IsIdempotent()
        {
            Point2D origin = new(0, 0);
            BoundingBox2D boundingBox2D = new(new Point2D(517333.17, 264881.09), new Point2D(517911.43, 265402.88));

            List<Point2D>? point2Ds_1 = boundingBox2D.Point2Ds(origin, 50, 50);
            List<Point2D>? point2Ds_2 = boundingBox2D.Point2Ds(origin, 50, 50);
            Assert.NotNull(point2Ds_1);
            Assert.NotNull(point2Ds_2);
            Assert.Equal(point2Ds_1.Count, point2Ds_2.Count);

            for (int i = 0; i < point2Ds_1.Count; i++)
            {
                Assert.Equal(point2Ds_1[i].X.GetHashCode(), point2Ds_2[i].X.GetHashCode());
                Assert.True(point2Ds_1[i].X == point2Ds_2[i].X);
                Assert.True(point2Ds_1[i].Y == point2Ds_2[i].Y);
            }
        }

        /// <summary>
        /// Tests that a coarse lattice is a strict subset of a finer one whose grid size divides it, which is what lets an area sampled coarsely be densified later without re-visiting the nodes it already holds.
        /// </summary>
        [Fact]
        public void Point2Ds_OriginAnchored_LatticesNest()
        {
            Point2D origin = new(0, 0);
            BoundingBox2D boundingBox2D = new(new Point2D(200000, 500000), new Point2D(200500, 500500));

            List<Point2D>? point2Ds_Coarse = boundingBox2D.Point2Ds(origin, 50, 50);
            List<Point2D>? point2Ds_Fine = boundingBox2D.Point2Ds(origin, 10, 10);
            Assert.NotNull(point2Ds_Coarse);
            Assert.NotNull(point2Ds_Fine);

            HashSet<(double X, double Y)> coordinates_Fine = [.. point2Ds_Fine.ConvertAll(x => (x.X, x.Y))];
            foreach (Point2D point2D in point2Ds_Coarse)
            {
                Assert.Contains((point2D.X, point2D.Y), coordinates_Fine);
            }
        }

        /// <summary>
        /// Tests that a bounding box holding no node of the lattice yields an empty list rather than null, so a caller tiling an area can tell an empty tile from invalid input.
        /// </summary>
        [Fact]
        public void Point2Ds_OriginAnchored_EmptyBoundingBox()
        {
            Point2D origin = new(0, 0);
            BoundingBox2D boundingBox2D = new(new Point2D(101, 101), new Point2D(108, 108));

            List<Point2D>? point2Ds = boundingBox2D.Point2Ds(origin, 10, 10);
            Assert.NotNull(point2Ds);
            Assert.Empty(point2Ds);
        }

        /// <summary>
        /// Tests that a null origin falls back to anchoring at the minimum corner of the bounding box, and that invalid grid sizes and a null bounding box give null.
        /// </summary>
        [Fact]
        public void Point2Ds_OriginAnchored_InvalidArguments()
        {
            BoundingBox2D boundingBox2D = new(new Point2D(0, 0), new Point2D(10, 10));

            List<Point2D>? point2Ds_NullOrigin = boundingBox2D.Point2Ds(null, 5, 5);
            Assert.NotNull(point2Ds_NullOrigin);
            Assert.Equal(9, point2Ds_NullOrigin.Count);
            Assert.Equal(0d, point2Ds_NullOrigin[0].X);

            Point2D origin = new(0, 0);
            Assert.Null(boundingBox2D.Point2Ds(origin, 0, 5));
            Assert.Null(boundingBox2D.Point2Ds(origin, 5, -1));
            Assert.Null(boundingBox2D.Point2Ds(origin, double.NaN, 5));
            Assert.Null(((BoundingBox2D?)null).Point2Ds(origin, 5, 5));
        }

        /// <summary>
        /// Tests the tolerance boundary on both sides: a node lying just outside the bounding box by less than the tolerance is included, and one lying outside by more than the tolerance is not.
        /// </summary>
        [Fact]
        public void Point2Ds_OriginAnchored_ToleranceBoundary()
        {
            Point2D origin = new(0, 0);
            double tolerance = 0.01;

            BoundingBox2D boundingBox2D_Inside = new(new Point2D(0, 0), new Point2D(20 - (tolerance / 2), 10));
            List<Point2D>? point2Ds_Inside = boundingBox2D_Inside.Point2Ds(origin, 10, 10, tolerance);
            Assert.NotNull(point2Ds_Inside);
            Assert.Contains(point2Ds_Inside, x => x.X == 20d);

            BoundingBox2D boundingBox2D_Outside = new(new Point2D(0, 0), new Point2D(20 - (tolerance * 2), 10));
            List<Point2D>? point2Ds_Outside = boundingBox2D_Outside.Point2Ds(origin, 10, 10, tolerance);
            Assert.NotNull(point2Ds_Outside);
            Assert.DoesNotContain(point2Ds_Outside, x => x.X == 20d);
        }
    }
}
