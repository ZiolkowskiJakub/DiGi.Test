using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using System.Diagnostics;
using System.Reflection;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that cutting a shape lying wholly inside one triangle of a mesh opens a hole of exactly that shape and puts the corners of what remains back onto the plane of the mesh.
        /// <para>The mesh is a tilted lattice, so an elevation that was interpolated wrongly shows up as a corner off the plane rather than as a plausible looking surface.</para>
        /// </summary>
        [Fact]
        public void Mesh3D_Difference_Hole()
        {
            double slopeX = 0.1;
            double slopeY = 0.05;
            double factor = System.Math.Sqrt(1 + (slopeX * slopeX) + (slopeY * slopeY));

            Mesh3D mesh3D = Mesh3D_Difference_Terrain(0, 0, 10, 2, slopeX, slopeY);
            Assert.Equal(8, mesh3D.TrianglesCount);

            // A 2 x 2 square sitting well inside the lower triangle of the first cell, clear of every edge.
            PolygonalFace2D polygonalFace2D = Mesh3D_Difference_Square(6, 2, 2);

            Mesh3D? mesh3D_Result = mesh3D.Difference([polygonalFace2D]);
            Assert.NotNull(mesh3D_Result);

            double area_Expected = (400 - 4) * factor;
            Assert.True(System.Math.Abs(mesh3D_Result.GetArea() - area_Expected) <= area_Expected * 1e-4, $"Cut mesh area {mesh3D_Result.GetArea()} does not match the expected {area_Expected}.");

            List<Triangle3D>? triangle3Ds = mesh3D_Result.GetTriangles();
            Assert.NotNull(triangle3Ds);
            Assert.NotEmpty(triangle3Ds);

            Polygon2D polygon2D = Mesh3D_Difference_Rectangle(6, 2, 2);
            foreach (Triangle3D triangle3D in triangle3Ds)
            {
                Point3D? point3D_Centroid = triangle3D.GetCentroid();
                Assert.NotNull(point3D_Centroid);
                Assert.False(polygon2D.Inside(new Point2D(point3D_Centroid.X, point3D_Centroid.Y), DiGi.Core.Constants.Tolerance.Distance), "A triangle was left inside the shape that was cut out.");
            }

            List<Point3D>? point3Ds = mesh3D_Result.GetPoints();
            Assert.NotNull(point3Ds);

            foreach (Point3D point3D in point3Ds)
            {
                double z = (slopeX * point3D.X) + (slopeY * point3D.Y);
                Assert.True(System.Math.Abs(point3D.Z - z) <= DiGi.Core.Constants.Tolerance.MacroDistance, $"Corner ({point3D.X}, {point3D.Y}, {point3D.Z}) was not put back onto the plane of the mesh (expected Z {z}).");
            }

            DiGi.Core.xUnit.Query.SerializationCheck(mesh3D_Result);
        }

        /// <summary>
        /// Tests that triangles covered by the shapes being cut out are dropped, and that a shape covering the whole mesh leaves nothing of it.
        /// </summary>
        [Fact]
        public void Mesh3D_Difference_FullyCovered()
        {
            double factor = System.Math.Sqrt(1 + 0.01);

            Mesh3D mesh3D = Mesh3D_Difference_Terrain(0, 0, 10, 2, 0.1, 0);
            Assert.Equal(8, mesh3D.TrianglesCount);

            Assert.Null(mesh3D.Difference([Mesh3D_Difference_Square(-5, -5, 30)]));

            // The second cell along X, both of its triangles, taken exactly.
            Mesh3D? mesh3D_Result = mesh3D.Difference([Mesh3D_Difference_Square(10, 0, 10)]);
            Assert.NotNull(mesh3D_Result);
            Assert.Equal(6, mesh3D_Result.TrianglesCount);

            double area_Expected = (400 - 100) * factor;
            Assert.True(System.Math.Abs(mesh3D_Result.GetArea() - area_Expected) <= area_Expected * 1e-4, $"Cut mesh area {mesh3D_Result.GetArea()} does not match the expected {area_Expected}.");
        }

        /// <summary>
        /// Tests that a shape standing clear of the mesh takes nothing away from it, and that the mesh handed back is a copy rather than the one that was passed in.
        /// </summary>
        [Fact]
        public void Mesh3D_Difference_Disjoint()
        {
            Mesh3D mesh3D = Mesh3D_Difference_Terrain(0, 0, 10, 2, 0.1, 0.05);

            Mesh3D? mesh3D_Result = mesh3D.Difference([Mesh3D_Difference_Square(100, 100, 10)]);
            Assert.NotNull(mesh3D_Result);
            Assert.NotSame(mesh3D, mesh3D_Result);
            Assert.Equal(mesh3D.TrianglesCount, mesh3D_Result.TrianglesCount);
            Assert.Equal(mesh3D.PointsCount, mesh3D_Result.PointsCount);
            Assert.Equal(mesh3D.GetArea(), mesh3D_Result.GetArea(), 9);
        }

        /// <summary>
        /// Tests the answers given for null and empty input.
        /// <para>A missing mesh gives nothing back, while a mesh with nothing to cut out of it gives back a copy of itself rather than the instance that was passed in.</para>
        /// </summary>
        [Fact]
        public void Mesh3D_Difference_Null()
        {
            Mesh3D mesh3D = Mesh3D_Difference_Terrain(0, 0, 10, 2, 0.1, 0.05);

            Assert.Null(((Mesh3D?)null).Difference((IEnumerable<PolygonalFace2D>?)null));
            Assert.Null(((Mesh3D?)null).Difference([Mesh3D_Difference_Square(0, 0, 5)]));

            Mesh3D? mesh3D_Null = mesh3D.Difference((IEnumerable<PolygonalFace2D>?)null);
            Assert.NotNull(mesh3D_Null);
            Assert.NotSame(mesh3D, mesh3D_Null);
            Assert.Equal(mesh3D.TrianglesCount, mesh3D_Null.TrianglesCount);

            Mesh3D? mesh3D_Empty = mesh3D.Difference(new List<PolygonalFace2D>());
            Assert.NotNull(mesh3D_Empty);
            Assert.NotSame(mesh3D, mesh3D_Empty);
            Assert.Equal(mesh3D.TrianglesCount, mesh3D_Empty.TrianglesCount);
            Assert.Equal(mesh3D.GetArea(), mesh3D_Empty.GetArea(), 9);
        }

        /// <summary>
        /// Tests that the courtyard of a real building keeps its ground when the outline of that building is cut out of a surface.
        /// <para>The fixture is a stored footprint in PL-1992 coordinates carrying an internal edge, so this also exercises the subtraction and the triangulation at the coordinate magnitudes the terrain surfaces actually use. The area taken away must be the area of the face, which already excludes the courtyard, and not the area enclosed by its external edge.</para>
        /// </summary>
        [Fact]
        public void Mesh3D_Difference_Courtyard()
        {
            string? path = DiGi.Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "PolygonalFace2D_CourtyardBuilding.json");
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }

            PolygonalFace2D? polygonalFace2D = DiGi.Core.Convert.ToDiGi<PolygonalFace2D>((DiGi.Core.Classes.Path)path)?.FirstOrDefault();
            Assert.NotNull(polygonalFace2D);

            List<IPolygonal2D>? internalEdges = polygonalFace2D.InternalEdges;
            Assert.NotNull(internalEdges);
            Assert.NotEmpty(internalEdges);

            BoundingBox2D? boundingBox2D = polygonalFace2D.GetBoundingBox();
            Assert.NotNull(boundingBox2D);

            double slopeX = 0.02;
            double slopeY = 0.01;
            double factor = System.Math.Sqrt(1 + (slopeX * slopeX) + (slopeY * slopeY));

            // A 10 m lattice reaching 20 m past the footprint on every side, which is the sampling step and
            // the scale the stored terrain surfaces come at.
            double size = 10;
            int count = (int)System.Math.Ceiling((System.Math.Max(boundingBox2D.Width, boundingBox2D.Height) + 40) / size);
            Mesh3D mesh3D = Mesh3D_Difference_Terrain(boundingBox2D.Min.X - 20, boundingBox2D.Min.Y - 20, size, count, slopeX, slopeY);

            Mesh3D? mesh3D_Result = mesh3D.Difference([polygonalFace2D]);
            Assert.NotNull(mesh3D_Result);

            double area_Removed = mesh3D.GetArea() - mesh3D_Result.GetArea();
            double area_Expected = polygonalFace2D.GetArea() * factor;

            Assert.True(System.Math.Abs(area_Removed - area_Expected) <= area_Expected * 5e-3, $"Area taken away {area_Removed} does not match the area of the face {area_Expected}; the courtyard was cut out as well or part of the building was missed.");

            Point2D? point2D_Courtyard = internalEdges[0]?.GetInternalPoint();
            Assert.NotNull(point2D_Courtyard);

            List<Triangle3D>? triangle3Ds = mesh3D_Result.GetTriangles();
            Assert.NotNull(triangle3Ds);

            bool covered = false;
            foreach (Triangle3D triangle3D in triangle3Ds)
            {
                Triangle2D triangle2D = new(new Point2D(triangle3D[0]!.X, triangle3D[0]!.Y), new Point2D(triangle3D[1]!.X, triangle3D[1]!.Y), new Point2D(triangle3D[2]!.X, triangle3D[2]!.Y));
                if (triangle2D.Inside(point2D_Courtyard, DiGi.Core.Constants.Tolerance.Distance))
                {
                    covered = true;
                    break;
                }
            }

            Assert.True(covered, "The courtyard of the building lost its ground.");
        }

        /// <summary>
        /// Tests the tolerance boundary of the area a shape must cover to be cut out at all, and that a triangle standing on its edge is kept rather than dropped.
        /// <para>A shape covering less than the tolerance is ignored, one covering more than it is not. A vertical triangle covers no ground, so nothing can be taken away from it and no elevation can be interpolated across it; dropping it would leave a gap for no reason.</para>
        /// </summary>
        [Fact]
        public void Mesh3D_Difference_ToleranceBoundary()
        {
            Mesh3D mesh3D = Mesh3D_Difference_Terrain(0, 0, 10, 2, 0.1, 0.05);

            // Just inside the boundary: 0.9 mm square covers 8.1e-7, below the 1e-6 tolerance.
            Mesh3D? mesh3D_Inside = mesh3D.Difference([Mesh3D_Difference_Square(6, 2, 0.9e-3)]);
            Assert.NotNull(mesh3D_Inside);
            Assert.Equal(mesh3D.TrianglesCount, mesh3D_Inside.TrianglesCount);
            Assert.Equal(mesh3D.GetArea(), mesh3D_Inside.GetArea(), 9);

            // Just outside it: 1.1 mm square covers 1.21e-6, above the tolerance.
            Mesh3D? mesh3D_Outside = mesh3D.Difference([Mesh3D_Difference_Square(6, 2, 1.1e-3)]);
            Assert.NotNull(mesh3D_Outside);
            Assert.True(mesh3D_Outside.TrianglesCount > mesh3D.TrianglesCount, "A shape covering more than the tolerance was not cut out.");

            // One triangle standing on its edge (in the XZ plane) beside one lying flat, with a shape
            // covering both of them from above.
            List<Point3D> point3Ds =
            [
                new Point3D(0, 0, 0),
                new Point3D(10, 0, 0),
                new Point3D(10, 0, 10),
                new Point3D(10, 10, 0)
            ];

            List<int[]> indexes = [[0, 1, 2], [0, 1, 3]];

            Mesh3D mesh3D_Vertical = new(point3Ds, indexes);
            Assert.Equal(2, mesh3D_Vertical.TrianglesCount);

            Mesh3D? mesh3D_VerticalResult = mesh3D_Vertical.Difference([Mesh3D_Difference_Square(-5, -5, 30)]);
            Assert.NotNull(mesh3D_VerticalResult);
            Assert.Equal(1, mesh3D_VerticalResult.TrianglesCount);
            Assert.Equal(50, mesh3D_VerticalResult.GetArea(), 6);
        }

        /// <summary>
        /// Tests that a not-a-number corner never reaches the result, from either side of the operation.
        /// <para>A triangle carrying such a corner cannot be drawn and would spoil the bounds of the whole mesh, so it is dropped; a shape carrying one is ignored and leaves the mesh as it was.</para>
        /// </summary>
        [Fact]
        public void Mesh3D_Difference_NaN()
        {
            Mesh3D mesh3D = Mesh3D_Difference_Terrain(0, 0, 10, 2, 0.1, 0.05);

            List<Point3D>? point3Ds = mesh3D.GetPoints();
            Assert.NotNull(point3Ds);

            List<int[]>? indexes = mesh3D.GetIndexes();
            Assert.NotNull(indexes);

            point3Ds.Add(new Point3D(double.NaN, 5, 5));
            point3Ds.Add(new Point3D(50, 50, 0));
            point3Ds.Add(new Point3D(60, 50, 0));
            indexes.Add([point3Ds.Count - 3, point3Ds.Count - 2, point3Ds.Count - 1]);

            Mesh3D mesh3D_NaN = new(point3Ds, indexes);
            Assert.Equal(9, mesh3D_NaN.TrianglesCount);

            Mesh3D? mesh3D_Result = mesh3D_NaN.Difference([Mesh3D_Difference_Square(100, 100, 10)]);
            Assert.NotNull(mesh3D_Result);
            Assert.Equal(8, mesh3D_Result.TrianglesCount);

            List<Point3D>? point3Ds_Result = mesh3D_Result.GetPoints();
            Assert.NotNull(point3Ds_Result);
            Assert.True(Spatial.Query.IsValid(point3Ds_Result), "A not-a-number corner reached the cut mesh.");

            // A shape carrying such a corner cannot describe an area and is ignored.
            Polygon2D polygon2D = new([new Point2D(1, 1), new Point2D(double.NaN, 1), new Point2D(5, 5), new Point2D(1, 5)]);

            Mesh3D? mesh3D_NaNShape = mesh3D.Difference([polygon2D]);
            Assert.NotNull(mesh3D_NaNShape);
            Assert.Equal(mesh3D.TrianglesCount, mesh3D_NaNShape.TrianglesCount);
            Assert.Equal(mesh3D.GetArea(), mesh3D_NaNShape.GetArea(), 9);
        }

        /// <summary>
        /// Tests that a self-intersecting shape is repaired rather than rejected, and that both of its lobes are cut out.
        /// </summary>
        [Fact]
        public void Mesh3D_Difference_SelfIntersecting()
        {
            double factor = System.Math.Sqrt(1 + 0.01);

            Mesh3D mesh3D = Mesh3D_Difference_Terrain(0, 0, 10, 2, 0.1, 0);

            // A bow tie over the first cell: the two lobes are the triangles (0,0)-(5,5)-(10,0) and
            // (0,10)-(5,5)-(10,10), covering 25 each.
            Polygon2D polygon2D = new([new Point2D(0, 0), new Point2D(10, 10), new Point2D(10, 0), new Point2D(0, 10)]);

            Mesh3D? mesh3D_Result = mesh3D.Difference([polygon2D]);
            Assert.NotNull(mesh3D_Result);

            double area_Expected = (400 - 50) * factor;
            Assert.True(System.Math.Abs(mesh3D_Result.GetArea() - area_Expected) <= area_Expected * 1e-3, $"Cut mesh area {mesh3D_Result.GetArea()} does not match the expected {area_Expected}; a lobe of the repaired shape was missed.");
        }

        /// <summary>
        /// Tests that several shapes cut out at once each open their own hole and that the areas add up.
        /// </summary>
        [Fact]
        public void Mesh3D_Difference_MultipleFootprints()
        {
            double slopeX = 0.1;
            double slopeY = 0.05;
            double factor = System.Math.Sqrt(1 + (slopeX * slopeX) + (slopeY * slopeY));

            Mesh3D mesh3D = Mesh3D_Difference_Terrain(0, 0, 10, 4, slopeX, slopeY);

            List<PolygonalFace2D> polygonalFace2Ds =
            [
                Mesh3D_Difference_Square(6, 2, 2),
                Mesh3D_Difference_Square(24, 12, 3),
                Mesh3D_Difference_Square(12, 26, 4)
            ];

            Mesh3D? mesh3D_Result = mesh3D.Difference(polygonalFace2Ds);
            Assert.NotNull(mesh3D_Result);

            double area_Expected = (1600 - 4 - 9 - 16) * factor;
            Assert.True(System.Math.Abs(mesh3D_Result.GetArea() - area_Expected) <= area_Expected * 1e-4, $"Cut mesh area {mesh3D_Result.GetArea()} does not match the expected {area_Expected}.");

            List<Point3D>? point3Ds = mesh3D_Result.GetPoints();
            Assert.NotNull(point3Ds);

            foreach (Point3D point3D in point3Ds)
            {
                double z = (slopeX * point3D.X) + (slopeY * point3D.Y);
                Assert.True(System.Math.Abs(point3D.Z - z) <= DiGi.Core.Constants.Tolerance.MacroDistance, $"Corner ({point3D.X}, {point3D.Y}, {point3D.Z}) was not put back onto the plane of the mesh (expected Z {z}).");
            }
        }

        /// <summary>
        /// Measures cutting building sized outlines out of a surface at the size, resolution and building density the terrain service and the building store answer with for the largest area the 3D views ask for.
        /// <para>Two surfaces are measured over the same 1 km square in PL-1992 coordinates, because the cost is driven by the resolution rather than by the triangle count. A 10 m lattice is finer than the buildings, so a cut leaves simple remainders. A 50 m lattice is coarser than them, so whole buildings fall inside single triangles and every remainder carries holes - far fewer triangles, and the slower of the two. The stored counties are sampled at 10 m to 100 m, so both occur.</para>
        /// </summary>
        [Fact]
        public void Mesh3D_Difference_Performance()
        {
            Mesh3D mesh3D_WarmUp = Mesh3D_Difference_Terrain(629000, 489000, 10, 4, 0.02, 0.01);
            _ = mesh3D_WarmUp.Difference([Mesh3D_Difference_Square(629015, 489015, 6)]);

            List<string> lines = [];

            // A thousand outlines on a lattice finer than they are: many triangles, simple remainders.
            long elapsed_Fine = Mesh3D_Difference_Measure(10, 100, 1000, lines);
            Assert.True(elapsed_Fine < 2500, $"Cutting a 10 m lattice failed the threshold! Elapsed: {elapsed_Fine} ms.");

            // The density the building store actually answers with, on a lattice coarser than the buildings:
            // few triangles, every remainder holed, and the slower of the two.
            long elapsed_Coarse = Mesh3D_Difference_Measure(50, 20, 250, lines);
            Assert.True(elapsed_Coarse < 500, $"Cutting a 50 m lattice failed the threshold! Elapsed: {elapsed_Coarse} ms.");

            // The reported case: a thousand outlines in touching terraced runs on a lattice far coarser
            // than they are, so whole runs sit inside single triangles and every remainder carries holes.
            long elapsed_Dense = Mesh3D_Difference_MeasureDense(lines);
            Assert.True(elapsed_Dense < 1000, $"Cutting dense terraced runs failed the threshold! Elapsed: {elapsed_Dense} ms.");

            string? path_Reports = DiGi.Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly());
            if (!string.IsNullOrWhiteSpace(path_Reports))
            {
                File.WriteAllLines(Path.Combine(path_Reports, "Mesh3D_Difference_Performance.txt"), lines);
            }
        }

        private static long Mesh3D_Difference_Measure(double size, int count, int buildings, List<string> lines)
        {
            Mesh3D mesh3D = Mesh3D_Difference_Terrain(629000, 489000, size, count, 0.02, 0.01);
            Assert.Equal(2 * count * count, mesh3D.TrianglesCount);

            System.Random random = new(20260822);

            List<PolygonalFace2D> polygonalFace2Ds = [];
            while (polygonalFace2Ds.Count < buildings)
            {
                double x = 629000 + (random.NextDouble() * ((count * size) - 20));
                double y = 489000 + (random.NextDouble() * ((count * size) - 20));
                double angle = random.NextDouble() * System.Math.PI;

                PolygonalFace2D? polygonalFace2D = Mesh3D_Difference_Building(x, y, 12, 8, angle);
                if (polygonalFace2D == null)
                {
                    continue;
                }

                polygonalFace2Ds.Add(polygonalFace2D);
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            Mesh3D? mesh3D_Result = mesh3D.Difference(polygonalFace2Ds);
            stopwatch.Stop();

            Assert.NotNull(mesh3D_Result);
            Assert.True(mesh3D_Result.GetArea() < mesh3D.GetArea());

            lines.Add($"Lattice {size} m: triangles in {mesh3D.TrianglesCount}, outlines {polygonalFace2Ds.Count}, triangles out {mesh3D_Result.TrianglesCount}, elapsed {stopwatch.ElapsedMilliseconds} ms.");

            return stopwatch.ElapsedMilliseconds;
        }

        private static Mesh3D Mesh3D_Difference_Terrain(double originX, double originY, double size, int count, double slopeX, double slopeY)
        {
            List<Point3D> point3Ds = [];
            for (int i = 0; i <= count; i++)
            {
                for (int j = 0; j <= count; j++)
                {
                    double x = originX + (i * size);
                    double y = originY + (j * size);

                    point3Ds.Add(new Point3D(x, y, (slopeX * (x - originX)) + (slopeY * (y - originY))));
                }
            }

            List<int[]> indexes = [];
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    int index = (i * (count + 1)) + j;
                    int index_Next = ((i + 1) * (count + 1)) + j;

                    indexes.Add([index, index_Next, index_Next + 1]);
                    indexes.Add([index, index_Next + 1, index + 1]);
                }
            }

            return new Mesh3D(point3Ds, indexes);
        }

        private static Polygon2D Mesh3D_Difference_Rectangle(double x, double y, double size)
        {
            return new Polygon2D([new Point2D(x, y), new Point2D(x + size, y), new Point2D(x + size, y + size), new Point2D(x, y + size)]);
        }

        private static PolygonalFace2D Mesh3D_Difference_Square(double x, double y, double size)
        {
            PolygonalFace2D? polygonalFace2D = Planar.Create.PolygonalFace2D(Mesh3D_Difference_Rectangle(x, y, size));
            Assert.NotNull(polygonalFace2D);

            return polygonalFace2D;
        }

        private static PolygonalFace2D? Mesh3D_Difference_Building(double x, double y, double width, double height, double angle)
        {
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);

            List<Point2D> point2Ds = [];
            foreach ((double u, double v) in new[] { (0.0, 0.0), (width, 0.0), (width, height), (0.0, height) })
            {
                point2Ds.Add(new Point2D(x + (u * cos) - (v * sin), y + (u * sin) + (v * cos)));
            }

            return Planar.Create.PolygonalFace2D(new Polygon2D(point2Ds));
        }
    }
}
