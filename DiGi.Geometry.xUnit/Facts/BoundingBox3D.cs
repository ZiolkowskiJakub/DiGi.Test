using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the InRange, On, and Inside checks on a <see cref="BoundingBox3D"/> instance.
        /// </summary>
        [Fact]
        public void BoundingBox3D()
        {
            BoundingBox3D boundingBox3D = new(new Point3D(0, 0, 0), new Point3D(10, 10, 10));

            Point3D point3D = new(5, 5, 5);

            bool inRange = boundingBox3D.InRange(point3D);
            bool on = boundingBox3D.On(point3D);
            bool inside = boundingBox3D.Inside(point3D);

            Assert.True(inRange);
            Assert.False(on);
            Assert.True(inside);
        }

        /// <summary>
        /// Tests that the allocation-free scalar bound accessors report the same values as the <see cref="BoundingBox3D.Min"/> and <see cref="BoundingBox3D.Max"/> points, and return <see cref="double.NaN"/> for an empty box.
        /// </summary>
        [Fact]
        public void BoundingBox3D_ScalarAccessors()
        {
            BoundingBox3D boundingBox3D = new(new Point3D(1, 2, 3), new Point3D(4, 5, 6));

            Assert.Equal(1.0, boundingBox3D.MinX);
            Assert.Equal(2.0, boundingBox3D.MinY);
            Assert.Equal(3.0, boundingBox3D.MinZ);
            Assert.Equal(4.0, boundingBox3D.MaxX);
            Assert.Equal(5.0, boundingBox3D.MaxY);
            Assert.Equal(6.0, boundingBox3D.MaxZ);

            Assert.Equal(boundingBox3D.Min.X, boundingBox3D.MinX);
            Assert.Equal(boundingBox3D.Max.Z, boundingBox3D.MaxZ);

            BoundingBox3D boundingBox3D_Empty = new((IEnumerable<Point3D?>?)null);

            Assert.True(double.IsNaN(boundingBox3D_Empty.MinX));
            Assert.True(double.IsNaN(boundingBox3D_Empty.MaxZ));
        }

        /// <summary>
        /// Tests that <see cref="BoundingBox3D.GetPoints"/> returns the eight corners of the box, all of which must lie within the box itself.
        /// <para>Regression for a bug where the four top-face corners were offset by the box width and depth, placing them outside the box.</para>
        /// </summary>
        [Fact]
        public void BoundingBox3D_GetPoints()
        {
            BoundingBox3D boundingBox3D = new(new Point3D(0, 0, 0), new Point3D(10, 20, 30));

            List<Point3D>? point3Ds = boundingBox3D.GetPoints();

            Assert.NotNull(point3Ds);
            Assert.Equal(8, point3Ds.Count);

            foreach (Point3D point3D in point3Ds)
            {
                Assert.True(boundingBox3D.InRange(point3D), $"Corner ({point3D.X}, {point3D.Y}, {point3D.Z}) lies outside the box.");
            }

            Assert.Contains(point3Ds, point3D => point3D.X == 0 && point3D.Y == 0 && point3D.Z == 0);
            Assert.Contains(point3Ds, point3D => point3D.X == 10 && point3D.Y == 20 && point3D.Z == 30);
            Assert.Contains(point3Ds, point3D => point3D.X == 0 && point3D.Y == 0 && point3D.Z == 30);
            Assert.Contains(point3Ds, point3D => point3D.X == 10 && point3D.Y == 20 && point3D.Z == 0);
        }

        /// <summary>
        /// Tests the expand-on-set behavior of the <see cref="BoundingBox3D.Min"/> and <see cref="BoundingBox3D.Max"/> setters, including the axis where the assigned value exceeds the opposite corner.
        /// <para>Regression for an order-of-operations bug in the Min setter that read the freshly reassigned Max, collapsing the minimum onto the assigned value.</para>
        /// </summary>
        [Fact]
        public void BoundingBox3D_MinMaxSetters()
        {
            BoundingBox3D boundingBox3D_Min = new(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
            boundingBox3D_Min.Min = new Point3D(-5, 5, 20);

            Assert.Equal(-5.0, boundingBox3D_Min.MinX);
            Assert.Equal(5.0, boundingBox3D_Min.MinY);
            Assert.Equal(10.0, boundingBox3D_Min.MinZ);
            Assert.Equal(10.0, boundingBox3D_Min.MaxX);
            Assert.Equal(10.0, boundingBox3D_Min.MaxY);
            Assert.Equal(20.0, boundingBox3D_Min.MaxZ);

            BoundingBox3D boundingBox3D_Max = new(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
            boundingBox3D_Max.Max = new Point3D(20, -5, 5);

            Assert.Equal(0.0, boundingBox3D_Max.MinX);
            Assert.Equal(-5.0, boundingBox3D_Max.MinY);
            Assert.Equal(0.0, boundingBox3D_Max.MinZ);
            Assert.Equal(20.0, boundingBox3D_Max.MaxX);
            Assert.Equal(0.0, boundingBox3D_Max.MaxY);
            Assert.Equal(5.0, boundingBox3D_Max.MaxZ);
        }

        /// <summary>
        /// Tests <see cref="BoundingBox3D.InRange(Triangle3D, double)"/> across the geometric cases: a triangle fully inside, an edge crossing a face, a large face spanning the box while all three vertices lie outside, a coplanar face, a degenerate collinear triangle, and a fully disjoint triangle.
        /// </summary>
        [Fact]
        public void BoundingBox3D_InRangeTriangle()
        {
            BoundingBox3D boundingBox3D = new(new Point3D(0, 0, 0), new Point3D(10, 10, 10));

            Triangle3D triangle3D_Inside = new(new Point3D(2, 2, 2), new Point3D(8, 2, 2), new Point3D(5, 8, 2));
            Assert.True(boundingBox3D.InRange(triangle3D_Inside));

            Triangle3D triangle3D_EdgeCrossing = new(new Point3D(-5, 5, 5), new Point3D(15, 5, 5), new Point3D(5, 20, 5));
            Assert.True(boundingBox3D.InRange(triangle3D_EdgeCrossing));

            Triangle3D triangle3D_Spanning = new(new Point3D(-100, -100, 5), new Point3D(200, -100, 5), new Point3D(50, 200, 5));
            Assert.True(boundingBox3D.InRange(triangle3D_Spanning));

            Triangle3D triangle3D_Coplanar = new(new Point3D(0, 0, 0), new Point3D(10, 0, 0), new Point3D(0, 10, 0));
            Assert.True(boundingBox3D.InRange(triangle3D_Coplanar));

            Triangle3D triangle3D_Collinear_Inside = new(new Point3D(0, 5, 5), new Point3D(5, 5, 5), new Point3D(10, 5, 5));
            Assert.True(boundingBox3D.InRange(triangle3D_Collinear_Inside));

            Triangle3D triangle3D_Disjoint = new(new Point3D(100, 100, 100), new Point3D(110, 100, 100), new Point3D(100, 110, 100));
            Assert.False(boundingBox3D.InRange(triangle3D_Disjoint));

            Triangle3D triangle3D_Collinear_Outside = new(new Point3D(100, 5, 5), new Point3D(105, 5, 5), new Point3D(110, 5, 5));
            Assert.False(boundingBox3D.InRange(triangle3D_Collinear_Outside));
        }

        /// <summary>
        /// Tests <see cref="BoundingBox3D.InRange(Triangle3D, double)"/> exactly inside and exactly outside the distance tolerance on a single axis.
        /// </summary>
        [Fact]
        public void BoundingBox3D_InRangeTriangle_ToleranceBoundaries()
        {
            BoundingBox3D boundingBox3D = new(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
            double tolerance = 0.1;

            Triangle3D triangle3D_Inside = new(new Point3D(0, 0, 10.0 + tolerance - 1e-9), new Point3D(10, 0, 10.0 + tolerance - 1e-9), new Point3D(0, 10, 10.0 + tolerance - 1e-9));
            Assert.True(boundingBox3D.InRange(triangle3D_Inside, tolerance));

            Triangle3D triangle3D_Outside = new(new Point3D(0, 0, 10.0 + tolerance + 1e-9), new Point3D(10, 0, 10.0 + tolerance + 1e-9), new Point3D(0, 10, 10.0 + tolerance + 1e-9));
            Assert.False(boundingBox3D.InRange(triangle3D_Outside, tolerance));
        }

        /// <summary>
        /// Tests <see cref="BoundingBox3D.InRange(Mesh3D, double)"/> for an overlapping mesh, a fully disjoint mesh, a null mesh, an empty mesh, and a box strictly enclosed by a hollow mesh whose surface it never touches.
        /// </summary>
        [Fact]
        public void BoundingBox3D_InRangeMesh3D()
        {
            BoundingBox3D boundingBox3D = new(new Point3D(0, 0, 0), new Point3D(10, 10, 10));

            List<Point3D> point3Ds_Overlapping = [new Point3D(-5, -5, 5), new Point3D(15, -5, 5), new Point3D(15, 15, 5), new Point3D(-5, 15, 5)];
            List<int[]> indexes_Overlapping = [[0, 1, 2], [0, 2, 3]];
            Mesh3D mesh3D_Overlapping = new(point3Ds_Overlapping, indexes_Overlapping);
            Assert.True(boundingBox3D.InRange(mesh3D_Overlapping));

            List<Point3D> point3Ds_Disjoint = [new Point3D(100, 100, 100), new Point3D(110, 100, 100), new Point3D(110, 110, 100), new Point3D(100, 110, 100)];
            List<int[]> indexes_Disjoint = [[0, 1, 2], [0, 2, 3]];
            Mesh3D mesh3D_Disjoint = new(point3Ds_Disjoint, indexes_Disjoint);
            Assert.False(boundingBox3D.InRange(mesh3D_Disjoint));

            Mesh3D? mesh3D_Null = null;
            Assert.False(boundingBox3D.InRange(mesh3D_Null));

            List<Point3D> point3Ds_Empty = [];
            List<int[]> indexes_Empty = [];
            Mesh3D mesh3D_Empty = new(point3Ds_Empty, indexes_Empty);
            Assert.False(boundingBox3D.InRange(mesh3D_Empty));

            BoundingBox3D boundingBox3D_Inner = new(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));
            Mesh3D mesh3D_Shell = CreateCubeShellMesh(100);
            Assert.True(mesh3D_Shell.GetBoundingBox() is BoundingBox3D boundingBox3D_Shell && boundingBox3D_Shell.InRange(boundingBox3D_Inner));
            Assert.False(boundingBox3D_Inner.InRange(mesh3D_Shell));
        }

        /// <summary>
        /// Tests <see cref="BoundingBox3D.InRange(Mesh3D, double)"/> exactly inside and exactly outside the distance tolerance on a single axis.
        /// </summary>
        [Fact]
        public void BoundingBox3D_InRangeMesh3D_ToleranceBoundaries()
        {
            BoundingBox3D boundingBox3D = new(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
            double tolerance = 0.1;

            List<int[]> indexes = [[0, 1, 2], [0, 2, 3]];

            List<Point3D> point3Ds_Inside =
            [
                new Point3D(0, 0, 10.0 + tolerance - 1e-9),
                new Point3D(10, 0, 10.0 + tolerance - 1e-9),
                new Point3D(10, 10, 10.0 + tolerance - 1e-9),
                new Point3D(0, 10, 10.0 + tolerance - 1e-9),
            ];
            Mesh3D mesh3D_Inside = new(point3Ds_Inside, indexes);
            Assert.True(boundingBox3D.InRange(mesh3D_Inside, tolerance));

            List<Point3D> point3Ds_Outside =
            [
                new Point3D(0, 0, 10.0 + tolerance + 1e-9),
                new Point3D(10, 0, 10.0 + tolerance + 1e-9),
                new Point3D(10, 10, 10.0 + tolerance + 1e-9),
                new Point3D(0, 10, 10.0 + tolerance + 1e-9),
            ];
            Mesh3D mesh3D_Outside = new(point3Ds_Outside, indexes);
            Assert.False(boundingBox3D.InRange(mesh3D_Outside, tolerance));
        }

        /// <summary>
        /// Tests that the bounding-box early-out in <see cref="BoundingBox3D.InRange(Mesh3D, double)"/> rejects a large mesh that is fully disjoint from the box without iterating its triangles, well under the stated time threshold.
        /// </summary>
        [Fact]
        public void BoundingBox3D_InRangeMesh3D_Performance()
        {
            // Warm up / JIT compile before measuring performance.
            {
                List<Point3D> point3Ds_Warmup = [new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0)];
                List<int[]> indexes_Warmup = [[0, 1, 2]];
                Mesh3D mesh3D_Warmup = new(point3Ds_Warmup, indexes_Warmup);
                BoundingBox3D boundingBox3D_Warmup = new(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));
                _ = boundingBox3D_Warmup.InRange(mesh3D_Warmup);
            }

            int count = 150;

            List<Point3D> point3Ds = new(count * count);
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    point3Ds.Add(new Point3D(i, j, 0));
                }
            }

            List<int[]> indexes = new((count - 1) * (count - 1) * 2);
            for (int i = 0; i < count - 1; i++)
            {
                for (int j = 0; j < count - 1; j++)
                {
                    int index_00 = i * count + j;
                    int index_10 = (i + 1) * count + j;
                    int index_01 = i * count + j + 1;
                    int index_11 = (i + 1) * count + j + 1;

                    indexes.Add([index_00, index_10, index_11]);
                    indexes.Add([index_00, index_11, index_01]);
                }
            }

            Mesh3D mesh3D = new(point3Ds, indexes);

            BoundingBox3D boundingBox3D = new(new Point3D(1000, 1000, 1000), new Point3D(1001, 1001, 1001));

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            bool inRange = boundingBox3D.InRange(mesh3D);
            stopwatch.Stop();

            Assert.False(inRange);
            Assert.True(stopwatch.ElapsedMilliseconds < 25, $"Mesh3D early-out performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Builds a hollow axis-aligned cube shell mesh of the given half-size, centered at the origin, as twelve triangles over eight corner vertices.
        /// </summary>
        /// <param name="halfSize">The <see cref="double"/> half-size of the cube along each axis.</param>
        /// <returns>A <see cref="Mesh3D"/> representing the cube shell.</returns>
        private static Mesh3D CreateCubeShellMesh(double halfSize)
        {
            List<Point3D> point3Ds =
            [
                new Point3D(-halfSize, -halfSize, -halfSize),
                new Point3D(halfSize, -halfSize, -halfSize),
                new Point3D(halfSize, halfSize, -halfSize),
                new Point3D(-halfSize, halfSize, -halfSize),
                new Point3D(-halfSize, -halfSize, halfSize),
                new Point3D(halfSize, -halfSize, halfSize),
                new Point3D(halfSize, halfSize, halfSize),
                new Point3D(-halfSize, halfSize, halfSize),
            ];

            List<int[]> indexes =
            [
                [0, 1, 2], [0, 2, 3],
                [4, 5, 6], [4, 6, 7],
                [0, 1, 5], [0, 5, 4],
                [3, 2, 6], [3, 6, 7],
                [0, 3, 7], [0, 7, 4],
                [1, 2, 6], [1, 6, 5],
            ];

            return new Mesh3D(point3Ds, indexes);
        }
    }
}
