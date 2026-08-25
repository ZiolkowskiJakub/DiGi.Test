using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using System.Diagnostics;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a dense run of touching building outlines is cut out of a surface coarser than the buildings themselves.
        /// <para>This is the shape of the reported failure: a 100 m lattice puts whole terraces inside single triangles, so every remainder carries holes and reaches the general triangulator, and the terraces share edges and leave micro slivers between them. Both are what the conforming Delaunay constraint enforcement fails to converge on.</para>
        /// </summary>
        [Fact]
        public void Mesh3D_Difference_DenseCluster()
        {
            Mesh3D mesh3D = Mesh3D_Difference_Terrain(629000, 489000, 100, 4, 0.02, 0.01);
            Assert.Equal(32, mesh3D.TrianglesCount);

            List<PolygonalFace2D> polygonalFace2Ds = Mesh3D_Difference_Terrace(629020, 489020, 24, 0.29670597283903605);
            polygonalFace2Ds.AddRange(Mesh3D_Difference_Terrace(629020, 489060, 24, 0.29670597283903605));
            polygonalFace2Ds.AddRange(Mesh3D_Difference_Terrace(629140, 489030, 24, 1.0471975511965976));

            Assert.True(polygonalFace2Ds.Count > 60);

            Mesh3D? mesh3D_Result = mesh3D.Difference(polygonalFace2Ds);
            Assert.NotNull(mesh3D_Result);
            Assert.True(mesh3D_Result.GetArea() < mesh3D.GetArea(), "Nothing was cut out of the surface.");
        }

        /// <summary>
        /// Tests that a cluster of degenerate outlines in one corner of a surface does not stop the outlines in the opposite corner from being cut out.
        /// <para>The cluster carries slivers, a needle and pairs a fraction of the tolerance apart, which is what used to take the whole call down rather than the one triangle it sat on.</para>
        /// </summary>
        [Fact]
        public void Mesh3D_Difference_FaultIsolation()
        {
            Mesh3D mesh3D = Mesh3D_Difference_Terrain(629000, 489000, 100, 4, 0.02, 0.01);

            List<PolygonalFace2D> polygonalFace2Ds = [];

            // A degenerate cluster in the lower left cell: a needle, a sliver and two outlines a fraction
            // of the tolerance apart.
            polygonalFace2Ds.Add(Mesh3D_Difference_Face([new Point2D(629010, 489010), new Point2D(629060, 489010.0000002), new Point2D(629060, 489010.0000004), new Point2D(629010, 489010.0000006)]));
            polygonalFace2Ds.Add(Mesh3D_Difference_Face([new Point2D(629012, 489012), new Point2D(629062, 489012.0000001), new Point2D(629012, 489012.0000003)]));
            polygonalFace2Ds.Add(Mesh3D_Difference_Face([new Point2D(629020, 489020), new Point2D(629030, 489020), new Point2D(629030, 489030), new Point2D(629020, 489030)]));
            polygonalFace2Ds.Add(Mesh3D_Difference_Face([new Point2D(629030.0000003, 489020), new Point2D(629040, 489020), new Point2D(629040, 489030), new Point2D(629030.0000003, 489030)]));

            // A plain 20 m square in the opposite corner, which must still be cut out in full.
            PolygonalFace2D polygonalFace2D_Far = Mesh3D_Difference_Square(629350, 489350, 20);
            polygonalFace2Ds.Add(polygonalFace2D_Far);

            Mesh3D? mesh3D_Result = mesh3D.Difference(polygonalFace2Ds);
            Assert.NotNull(mesh3D_Result);

            Polygon2D polygon2D_Far = Mesh3D_Difference_Rectangle(629350, 489350, 20);
            foreach (Triangle3D triangle3D in mesh3D_Result.GetTriangles() ?? [])
            {
                Point3D? point3D_Centroid = triangle3D.GetCentroid();
                Assert.NotNull(point3D_Centroid);
                Assert.False(polygon2D_Far.Inside(new Point2D(point3D_Centroid.X, point3D_Centroid.Y), DiGi.Core.Constants.Tolerance.Distance), "The outline standing clear of the degenerate cluster was not cut out.");
            }
        }

        private static long Mesh3D_Difference_MeasureDense(List<string> lines)
        {
            // A 1 km square sampled at 100 m, which is the coarsest step the stored counties come at, and
            // the one that puts whole terraces inside single triangles.
            Mesh3D mesh3D = Mesh3D_Difference_Terrain(629000, 489000, 100, 10, 0.02, 0.01);

            System.Random random = new(20260825);

            List<PolygonalFace2D> polygonalFace2Ds = [];
            while (polygonalFace2Ds.Count < 1000)
            {
                double x = 629000 + (random.NextDouble() * 850);
                double y = 489000 + (random.NextDouble() * 850);

                polygonalFace2Ds.AddRange(Mesh3D_Difference_Terrace(x, y, 20, random.NextDouble() * System.Math.PI));
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            Mesh3D? mesh3D_Result = mesh3D.Difference(polygonalFace2Ds);
            stopwatch.Stop();

            Assert.NotNull(mesh3D_Result);
            Assert.True(mesh3D_Result.GetArea() < mesh3D.GetArea());

            lines.Add($"Dense terraces, lattice 100 m: triangles in {mesh3D.TrianglesCount}, outlines {polygonalFace2Ds.Count}, triangles out {mesh3D_Result.TrianglesCount}, elapsed {stopwatch.ElapsedMilliseconds} ms.");

            return stopwatch.ElapsedMilliseconds;
        }

        private static PolygonalFace2D Mesh3D_Difference_Face(List<Point2D> point2Ds)
        {
            PolygonalFace2D? polygonalFace2D = Planar.Create.PolygonalFace2D(new Polygon2D(point2Ds));
            Assert.NotNull(polygonalFace2D);

            return polygonalFace2D;
        }

        private static List<PolygonalFace2D> Mesh3D_Difference_Terrace(double x, double y, int count, double angle)
        {
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);

            double width = 6;
            double height = 10;

            List<PolygonalFace2D> polygonalFace2Ds = [];
            for (int i = 0; i < count; i++)
            {
                // Every third house is pushed a fraction of the tolerance away from its neighbour, so the
                // run carries both shared edges and micro gaps rather than one or the other.
                double offset = (i * width) + (i % 3 == 0 ? 3e-7 : 0);

                List<Point2D> point2Ds = [];
                foreach ((double u, double v) in new[] { (offset, 0.0), (offset + width, 0.0), (offset + width, height), (offset, height) })
                {
                    point2Ds.Add(new Point2D(x + (u * cos) - (v * sin), y + (u * sin) + (v * cos)));
                }

                polygonalFace2Ds.Add(Mesh3D_Difference_Face(point2Ds));
            }

            return polygonalFace2Ds;
        }
    }
}
