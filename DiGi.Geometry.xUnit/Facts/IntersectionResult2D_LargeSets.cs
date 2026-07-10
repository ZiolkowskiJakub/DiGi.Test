using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the collection-versus-collection intersection over a large grid (200 horizontal x 200 vertical segments) plus 200 far-away segments, asserting the exact unique intersection count and a coarse time budget.
        /// </summary>
        [Fact]
        public void Collection_Collection_LargeGrid_Performance()
        {
            List<Segment2D> segment2Ds_Horizontal = [];
            for (int i = 0; i < 200; i++)
            {
                segment2Ds_Horizontal.Add(new Segment2D(new Point2D(-1, i), new Point2D(200, i)));
            }

            // Far-away horizontal segments exercising the bounding-box rejection path.
            for (int i = 0; i < 200; i++)
            {
                segment2Ds_Horizontal.Add(new Segment2D(new Point2D(-1, 1000 + i), new Point2D(200, 1000 + i)));
            }

            List<Segment2D> segment2Ds_Vertical = [];
            for (int i = 0; i < 200; i++)
            {
                segment2Ds_Vertical.Add(new Segment2D(new Point2D(i, -1), new Point2D(i, 200)));
            }

            // Warm up / JIT compile before measuring performance.
            _ = segment2Ds_Horizontal.IntersectionResult2D([segment2Ds_Vertical[0]]);

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            IntersectionResult2D? intersectionResult2D = segment2Ds_Horizontal.IntersectionResult2D(segment2Ds_Vertical);
            stopwatch.Stop();

            Assert.NotNull(intersectionResult2D);
            Assert.Equal(40000, intersectionResult2D.Count);
            Assert.True(stopwatch.ElapsedMilliseconds < 10000, $"Large-grid collection intersection took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Tests a single segment against a large collection (10000 crossing + 10000 far-away segments), asserting the exact unique intersection count and a coarse time budget.
        /// </summary>
        [Fact]
        public void Segment_LargeCollection_Performance()
        {
            Segment2D segment2D = new(new Point2D(0, 0), new Point2D(10000, 0));

            List<Segment2D> segment2Ds = [];
            for (int i = 0; i < 10000; i++)
            {
                double x = i + 0.5;
                segment2Ds.Add(new Segment2D(new Point2D(x, -1), new Point2D(x, 1)));
                segment2Ds.Add(new Segment2D(new Point2D(x, 100), new Point2D(x, 102)));
            }

            // Warm up / JIT compile before measuring performance.
            _ = segment2D.IntersectionResult2D([segment2Ds[0]]);

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            IntersectionResult2D? intersectionResult2D = segment2D.IntersectionResult2D(segment2Ds);
            stopwatch.Stop();

            Assert.NotNull(intersectionResult2D);
            Assert.Equal(10000, intersectionResult2D.Count);
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Segment versus large collection took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Tests a line against a large collection (10000 crossing + 10000 far-away segments), asserting the exact unique intersection count and a coarse time budget.
        /// </summary>
        [Fact]
        public void Line_LargeCollection_Performance()
        {
            Line2D line2D = new(new Point2D(0, 0), new Vector2D(1, 0));

            List<Segment2D> segment2Ds = [];
            for (int i = 0; i < 10000; i++)
            {
                double x = i + 0.5;
                segment2Ds.Add(new Segment2D(new Point2D(x, -1), new Point2D(x, 1)));
                segment2Ds.Add(new Segment2D(new Point2D(x, 100), new Point2D(x, 102)));
            }

            // Warm up / JIT compile before measuring performance.
            _ = line2D.IntersectionResult2D([segment2Ds[0]]);

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            IntersectionResult2D? intersectionResult2D = line2D.IntersectionResult2D(segment2Ds);
            stopwatch.Stop();

            Assert.NotNull(intersectionResult2D);
            Assert.Equal(10000, intersectionResult2D.Count);
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Line versus large collection took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Tests deduplication at scale: two collections of spokes all passing through the origin (62500 candidate pairs) must collapse to a single intersection point.
        /// </summary>
        [Fact]
        public void Collection_Collection_CoincidentSpokes_Dedup()
        {
            List<Segment2D> segment2Ds_1 = [];
            List<Segment2D> segment2Ds_2 = [];
            for (int i = 0; i < 250; i++)
            {
                double angle_1 = System.Math.PI * i / 500.0;
                segment2Ds_1.Add(new Segment2D(new Point2D(-10 * System.Math.Cos(angle_1), -10 * System.Math.Sin(angle_1)), new Point2D(10 * System.Math.Cos(angle_1), 10 * System.Math.Sin(angle_1))));

                double angle_2 = System.Math.PI * (i + 250) / 500.0;
                segment2Ds_2.Add(new Segment2D(new Point2D(-10 * System.Math.Cos(angle_2), -10 * System.Math.Sin(angle_2)), new Point2D(10 * System.Math.Cos(angle_2), 10 * System.Math.Sin(angle_2))));
            }

            IntersectionResult2D? intersectionResult2D = segment2Ds_1.IntersectionResult2D(segment2Ds_2);

            Assert.NotNull(intersectionResult2D);
            Assert.Equal(1, intersectionResult2D.Count);

            Point2D? point2D = intersectionResult2D.GetGeometry2Ds<Point2D>()?[0];
            Assert.NotNull(point2D);
            Assert.Equal(0, point2D.X, 6);
            Assert.Equal(0, point2D.Y, 6);
        }

        /// <summary>
        /// Tests a polygonal face with a large vertex count (2000-gon) against a line through its center, asserting a single chord segment of the expected length and a coarse time budget.
        /// </summary>
        [Fact]
        public void PolygonalFace_Line_LargeVertexCount_Performance()
        {
            Point2D[] point2Ds = new Point2D[2000];
            for (int i = 0; i < 2000; i++)
            {
                double angle = 2.0 * System.Math.PI * i / 2000.0;
                point2Ds[i] = new Point2D(100 * System.Math.Cos(angle), 100 * System.Math.Sin(angle));
            }

            PolygonalFace2D? polygonalFace2D = Create.PolygonalFace2D(point2Ds);
            Assert.NotNull(polygonalFace2D);

            Line2D line2D = new(new Point2D(0, 0), new Vector2D(1, 0));

            // Warm up / JIT compile before measuring performance.
            _ = polygonalFace2D.IntersectionResult2D(new Line2D(new Point2D(0, 1000), new Vector2D(1, 0)));

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            IntersectionResult2D? intersectionResult2D = polygonalFace2D.IntersectionResult2D(line2D);
            stopwatch.Stop();

            Assert.NotNull(intersectionResult2D);
            Assert.Equal(1, intersectionResult2D.Count);

            Segment2D? segment2D = intersectionResult2D.GetGeometry2Ds<Segment2D>()?[0];
            Assert.NotNull(segment2D);
            Assert.Equal(200, segment2D.Length, 2);
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Polygonal face (2000-gon) versus line took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Tests two overlapping polygonal faces with large vertex counts (1000-gons), asserting an intersection face is produced.
        /// </summary>
        [Fact]
        public void PolygonalFace_PolygonalFace_LargeVertexCount()
        {
            Point2D[] point2Ds_1 = new Point2D[1000];
            Point2D[] point2Ds_2 = new Point2D[1000];
            for (int i = 0; i < 1000; i++)
            {
                double angle = 2.0 * System.Math.PI * i / 1000.0;
                point2Ds_1[i] = new Point2D(100 * System.Math.Cos(angle), 100 * System.Math.Sin(angle));
                point2Ds_2[i] = new Point2D(100 + 100 * System.Math.Cos(angle), 100 * System.Math.Sin(angle));
            }

            PolygonalFace2D? polygonalFace2D_1 = Create.PolygonalFace2D(point2Ds_1);
            PolygonalFace2D? polygonalFace2D_2 = Create.PolygonalFace2D(point2Ds_2);
            Assert.NotNull(polygonalFace2D_1);
            Assert.NotNull(polygonalFace2D_2);

            IntersectionResult2D? intersectionResult2D = polygonalFace2D_1.IntersectionResult2D(polygonalFace2D_2);

            Assert.NotNull(intersectionResult2D);
            Assert.True(intersectionResult2D.Any());
            Assert.True(intersectionResult2D.Contains<PolygonalFace2D>());
        }
    }
}
