using DiGi.Core;
using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests <see cref="Planar.Create.Geometry2Ds"/> handling of null inputs, empty collections, and collections containing null elements.
        /// </summary>
        [Fact]
        public void Geometry2Ds_NullAndEmptyInputs()
        {
            List<IGeometry2D>? list_Null = null;
            List<IGeometry2D>? result_Null = Planar.Create.Geometry2Ds(list_Null);
            Assert.Null(result_Null);

            List<IGeometry2D> list_Empty = [];
            List<IGeometry2D>? result_Empty = Planar.Create.Geometry2Ds(list_Empty);
            Assert.NotNull(result_Empty);
            Assert.Empty(result_Empty);

            List<IGeometry2D?> list_WithNulls = [new Point2D(0, 0), null, new Point2D(10, 10)];
            List<IGeometry2D>? result_WithNulls = Planar.Create.Geometry2Ds(list_WithNulls!);
            Assert.NotNull(result_WithNulls);
            Assert.Equal(2, result_WithNulls.Count);
            Assert.DoesNotContain(null, result_WithNulls);
        }

        /// <summary>
        /// Tests that points lying on the edges or vertices of constructed polygons are properly deduplicated and filtered out.
        /// </summary>
        [Fact]
        public void Geometry2Ds_PointsOnPolygons()
        {
            // 4 segments forming a 10x10 square polygon
            Segment2D segment2D_1 = new(new Point2D(0, 0), new Point2D(10, 0));
            Segment2D segment2D_2 = new(new Point2D(10, 0), new Point2D(10, 10));
            Segment2D segment2D_3 = new(new Point2D(10, 10), new Point2D(0, 10));
            Segment2D segment2D_4 = new(new Point2D(0, 10), new Point2D(0, 0));

            // Point on corner vertex of polygon
            Point2D point2D_Vertex = new(0, 0);
            // Point on edge of polygon
            Point2D point2D_Edge = new(5, 0);
            // Point outside polygon
            Point2D point2D_Outside = new(20, 20);

            List<IGeometry2D> geometry2Ds = [segment2D_1, segment2D_2, segment2D_3, segment2D_4, point2D_Vertex, point2D_Edge, point2D_Outside];

            List<IGeometry2D>? result = Planar.Create.Geometry2Ds(geometry2Ds);

            Assert.NotNull(result);
            // Should contain 1 Polygon2D and 1 Point2D (point2D_Outside). The vertex and edge points must be filtered out.
            Assert.Equal(2, result.Count);
            Assert.Single(result.OfType<Polygon2D>());
            Assert.Single(result.OfType<Point2D>());
            Assert.Equal(point2D_Outside, result.OfType<Point2D>().First());
        }

        /// <summary>
        /// Tests that standalone segments and uncombined lines are preserved as segment/polyline geometries without being lost.
        /// </summary>
        [Fact]
        public void Geometry2Ds_DisconnectedSegments()
        {
            // Square (will form a polygon)
            Segment2D segment2D_1 = new(new Point2D(0, 0), new Point2D(10, 0));
            Segment2D segment2D_2 = new(new Point2D(10, 0), new Point2D(10, 10));
            Segment2D segment2D_3 = new(new Point2D(10, 10), new Point2D(0, 10));
            Segment2D segment2D_4 = new(new Point2D(0, 10), new Point2D(0, 0));

            // Disconnected standalone segment
            Segment2D segment2D_Standalone = new(new Point2D(50, 50), new Point2D(60, 60));

            List<IGeometry2D> geometry2Ds = [segment2D_1, segment2D_2, segment2D_3, segment2D_4, segment2D_Standalone];

            List<IGeometry2D>? result = Planar.Create.Geometry2Ds(geometry2Ds);

            Assert.NotNull(result);
            // Expected: 1 Polygon2D and 1 Segment2D (or Polyline2D)
            Assert.Equal(2, result.Count);
            Assert.Single(result.OfType<Polygon2D>());
            Assert.True(result.OfType<Segment2D>().Any() || result.OfType<Polyline2D>().Any());
        }

        /// <summary>
        /// Tests distance tolerance boundaries for point and segment deduplication.
        /// </summary>
        [Fact]
        public void Geometry2Ds_ToleranceBoundaries()
        {
            double tolerance = 1e-3;

            // Point inside tolerance boundary (distance < 1e-3)
            Point2D point2D_Base = new(0, 0);
            Point2D point2D_Inside = new(1e-4, 0);
            // Point outside tolerance boundary (distance > 1e-3)
            Point2D point2D_Outside = new(2e-3, 0);

            List<IGeometry2D> geometry2Ds = [point2D_Base, point2D_Inside, point2D_Outside];

            List<IGeometry2D>? result = Planar.Create.Geometry2Ds(geometry2Ds, tolerance);

            Assert.NotNull(result);
            // point2D_Base and point2D_Inside should be merged into 1 point; point2D_Outside kept separate
            Assert.Equal(2, result.Count);
        }

        /// <summary>
        /// Tests handling of overlapping, identical, and reversed collinear segments.
        /// </summary>
        [Fact]
        public void Geometry2Ds_OverlappingAndReversedGeometries()
        {
            Segment2D segment2D_Forward = new(new Point2D(0, 0), new Point2D(10, 0));
            Segment2D segment2D_Reversed = new(new Point2D(10, 0), new Point2D(0, 0));
            Segment2D segment2D_Identical = new(new Point2D(0, 0), new Point2D(10, 0));

            List<IGeometry2D> geometry2Ds = [segment2D_Forward, segment2D_Reversed, segment2D_Identical];

            List<IGeometry2D>? result = Planar.Create.Geometry2Ds(geometry2Ds);

            Assert.NotNull(result);
            // Duplicate similar segments should be deduplicated to a single segment or polyline
            Assert.Single(result);
        }

        /// <summary>
        /// Tests T-junction line intersections where a segment meets the interior of another segment.
        /// </summary>
        [Fact]
        public void Geometry2Ds_TJunctionsAndComplexIntersections()
        {
            // Horizontal segment from (0,0) to (20,0)
            Segment2D segment2D_H = new(new Point2D(0, 0), new Point2D(20, 0));
            // Vertical segment from (10,0) to (10,10) creating a T-junction at (10,0)
            Segment2D segment2D_V = new(new Point2D(10, 0), new Point2D(10, 10));

            List<IGeometry2D> geometry2Ds = [segment2D_H, segment2D_V];

            List<IGeometry2D>? result = Planar.Create.Geometry2Ds(geometry2Ds);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        /// <summary>
        /// Performance benchmark for <see cref="Planar.Create.Geometry2Ds"/> processing a large dataset.
        /// </summary>
        [Fact]
        public void Geometry2Ds_Performance()
        {
            // Warmup / JIT compile
            {
                Segment2D s1 = new(new Point2D(0, 0), new Point2D(1, 0));
                Segment2D s2 = new(new Point2D(1, 0), new Point2D(1, 1));
                Segment2D s3 = new(new Point2D(1, 1), new Point2D(0, 0));
                _ = Planar.Create.Geometry2Ds([s1, s2, s3]);
            }

            List<IGeometry2D> geometry2Ds = [];

            // 100 closed square polygons (400 segments)
            for (int i = 0; i < 100; i++)
            {
                double offset = i * 25.0;
                geometry2Ds.Add(new Segment2D(new Point2D(offset, 0), new Point2D(offset + 10, 0)));
                geometry2Ds.Add(new Segment2D(new Point2D(offset + 10, 0), new Point2D(offset + 10, 10)));
                geometry2Ds.Add(new Segment2D(new Point2D(offset + 10, 10), new Point2D(offset, 10)));
                geometry2Ds.Add(new Segment2D(new Point2D(offset, 10), new Point2D(offset, 0)));
            }

            // 500 points (some on vertices/edges of polygons, some standalone)
            for (int i = 0; i < 500; i++)
            {
                double x = (i % 100) * 25.0 + (i % 2 == 0 ? 0.0 : 5.0);
                double y = i % 2 == 0 ? 0.0 : 50.0;
                geometry2Ds.Add(new Point2D(x, y));
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            List<IGeometry2D>? result = Planar.Create.Geometry2Ds(geometry2Ds);
            stopwatch.Stop();

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Geometry2Ds performance benchmark completed in {stopwatch.ElapsedMilliseconds} ms.");
        }
    }
}
