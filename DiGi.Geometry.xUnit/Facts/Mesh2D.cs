using DiGi.Geometry.Planar.Classes;
using System.Collections.Generic;
using System.Diagnostics;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the creation of a 2D mesh from a polygonal face.
        /// </summary>
        [Fact]
        public void Mesh2D()
        {
            Polygon2D polygon2D = new(
            [
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(11, 10),
                new Point2D(11, 0),
                new Point2D(11, 0),
                new Point2D(20, 0),
                new Point2D(20, 11),
                new Point2D(0, 11)
            ]);

            PolygonalFace2D? polygonalFace2D = Planar.Create.PolygonalFace2D(polygon2D);
            Assert.NotNull(polygonalFace2D);

            Mesh2D? mesh2D = Planar.Create.Mesh2D(polygonalFace2D);
            Assert.NotNull(mesh2D);
        }

        /// <summary>
        /// Tests the <see cref="Mesh2D"/> query methods on a unit square mesh made of two triangles.
        /// <para>Verifies <see cref="Mesh2D.GetArea"/>, the unique edge count from <see cref="Mesh2D.GetSegments"/>, <see cref="Mesh2D.GetTriangles"/>, the boundary polygon, the inherited <see cref="DiGi.Geometry.Core.Classes.Mesh{TPoint}.IsClosed"/> check, and the JSON serialization round-trip.</para>
        /// </summary>
        [Fact]
        public void Mesh2D_Queries()
        {
            Mesh2D mesh2D = new(
                [new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 1), new Point2D(0, 1)],
                [new int[] { 0, 1, 2 }, new int[] { 0, 2, 3 }]);

            Assert.Equal(4, mesh2D.PointsCount);
            Assert.Equal(2, mesh2D.TrianglesCount);
            Assert.Equal(1.0, mesh2D.GetArea(), 10);

            List<Segment2D>? segment2Ds = mesh2D.GetSegments();
            Assert.NotNull(segment2Ds);
            Assert.Equal(5, segment2Ds.Count);

            List<Triangle2D>? triangle2Ds = mesh2D.GetTriangles();
            Assert.NotNull(triangle2Ds);
            Assert.Equal(2, triangle2Ds.Count);

            List<Polygon2D>? polygon2Ds = mesh2D.GetBoundaryEdges();
            Assert.NotNull(polygon2Ds);
            Assert.Single(polygon2Ds);

            Assert.False(mesh2D.IsClosed());

            DiGi.Core.xUnit.Query.SerializationCheck(mesh2D);
        }

        /// <summary>
        /// Tests the creation of a <see cref="Mesh2D"/> from triangles with coincident points merged by tolerance.
        /// <para>Two triangles sharing an edge, one with vertices perturbed within the tolerance, must merge into four unique points and two triangles.</para>
        /// </summary>
        [Fact]
        public void Mesh2D_Merge()
        {
            double tolerance = DiGi.Core.Constants.Tolerance.Distance;
            double offset = tolerance / 10;

            List<Triangle2D> triangle2Ds =
            [
                new(new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 1)),
                new(new Point2D(0, 0), new Point2D(1 + offset, 1 - offset), new Point2D(0, 1))
            ];

            Mesh2D? mesh2D = Planar.Create.Mesh2D(triangle2Ds, tolerance);
            Assert.NotNull(mesh2D);
            Assert.Equal(4, mesh2D.PointsCount);
            Assert.Equal(2, mesh2D.TrianglesCount);
            Assert.Equal(1.0, mesh2D.GetArea(), 3);
        }

        /// <summary>
        /// Tests the performance of creating a <see cref="Mesh2D"/> from a large triangle collection and querying it.
        /// <para>After a warm-up call, merging 20000 triangles of a 100 by 100 grid plus running <see cref="Mesh2D.GetArea"/> and <see cref="Mesh2D.GetSegments"/> must complete within the stated threshold.</para>
        /// </summary>
        [Fact]
        public void Mesh2D_Performance()
        {
            static List<Triangle2D> gridTriangles(int rows, int columns)
            {
                List<Triangle2D> result = new(2 * rows * columns);
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        Point2D point2D_1 = new(j, i);
                        Point2D point2D_2 = new(j + 1, i);
                        Point2D point2D_3 = new(j + 1, i + 1);
                        Point2D point2D_4 = new(j, i + 1);

                        result.Add(new Triangle2D(point2D_1, point2D_2, point2D_3));
                        result.Add(new Triangle2D(point2D_1, point2D_3, point2D_4));
                    }
                }

                return result;
            }

            // Warm-up (JIT)
            Mesh2D? mesh2D_WarmUp = Planar.Create.Mesh2D(gridTriangles(5, 5));
            Assert.NotNull(mesh2D_WarmUp);
            Assert.True(mesh2D_WarmUp.GetArea() > 0);

            Stopwatch stopwatch = Stopwatch.StartNew();

            Mesh2D? mesh2D = Planar.Create.Mesh2D(gridTriangles(100, 100));
            Assert.NotNull(mesh2D);

            double area = mesh2D.GetArea();
            List<Segment2D>? segment2Ds = mesh2D.GetSegments();

            stopwatch.Stop();

            Assert.Equal(101 * 101, mesh2D.PointsCount);
            Assert.Equal(2 * 100 * 100, mesh2D.TrianglesCount);
            Assert.Equal(10000.0, area, 6);
            Assert.NotNull(segment2Ds);
            Assert.Equal((101 * 100 * 2) + (100 * 100), segment2Ds.Count);

            Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Large mesh merge and evaluation took {stopwatch.ElapsedMilliseconds} ms, expected less than 1000 ms.");
        }
    }
}