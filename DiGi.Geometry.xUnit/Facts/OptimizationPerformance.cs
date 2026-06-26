using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;
using DiGi.Math.Classes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies both the correctness and the performance improvements of the optimized MaxDistance query on a large dataset of 2D points.
        /// </summary>
        [Fact]
        public void MaxDistance_PerformanceAndCorrectness()
        {
            // Create 1000 points in a line (Max distance should be between first and last)
            List<Point2D> point2Ds = new();
            for (int i = 0; i < 1000; i++)
            {
                point2Ds.Add(new Point2D(i, i));
            }

            // Warm up / JIT compile
            _ = Planar.Query.MaxDistance(point2Ds.Take(10));

            // Execute optimized query and measure performance
            Stopwatch stopwatch = Stopwatch.StartNew();
            double distance = Planar.Query.MaxDistance(point2Ds, out Point2D? point2D_1, out Point2D? point2D_2);
            stopwatch.Stop();

            // Assert Correctness
            Assert.NotNull(point2D_1);
            Assert.NotNull(point2D_2);
            Assert.Equal(0, point2D_1.X);
            Assert.Equal(0, point2D_1.Y);
            Assert.Equal(999, point2D_2.X);
            Assert.Equal(999, point2D_2.Y);
            Assert.Equal(System.Math.Sqrt(2) * 999, distance, 1e-6);

            // Assert Performance (optimized should be extremely fast, typically < 100ms)
            Assert.True(stopwatch.ElapsedMilliseconds < 100, $"MaxDistance performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance improvements of the optimized Planar ExtremePoints query on a large dataset of 2D points.
        /// </summary>
        [Fact]
        public void ExtremePoints_Planar_PerformanceAndCorrectness()
        {
            List<Point2D> point2Ds = new();
            for (int i = 0; i < 1000; i++)
            {
                point2Ds.Add(new Point2D(i, -i));
            }

            // Warm up / JIT compile
            Planar.Query.ExtremePoints(point2Ds.Take(10), out _, out _);

            // Execute optimized query and measure performance
            Stopwatch stopwatch = Stopwatch.StartNew();
            Planar.Query.ExtremePoints(point2Ds, out Point2D? point2D_1, out Point2D? point2D_2);
            stopwatch.Stop();

            // Assert Correctness
            Assert.NotNull(point2D_1);
            Assert.NotNull(point2D_2);
            Assert.Equal(0, point2D_1.X);
            Assert.Equal(0, point2D_1.Y);
            Assert.Equal(999, point2D_2.X);
            Assert.Equal(-999, point2D_2.Y);

            // Assert Performance (optimized should be extremely fast, typically < 100ms)
            Assert.True(stopwatch.ElapsedMilliseconds < 100, $"Planar ExtremePoints performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance improvements of the optimized Spatial ExtremePoints query on a large dataset of 3D points.
        /// </summary>
        [Fact]
        public void ExtremePoints_Spatial_PerformanceAndCorrectness()
        {
            List<Point3D> point3Ds = new();
            for (int i = 0; i < 1000; i++)
            {
                point3Ds.Add(new Point3D(i, i, i));
            }

            // Warm up / JIT compile
            Spatial.Query.ExtremePoints(point3Ds.Take(10), out _, out _, out _);

            // Execute optimized query and measure performance
            Stopwatch stopwatch = Stopwatch.StartNew();
            Spatial.Query.ExtremePoints(point3Ds, out Point3D? point3D_1, out Point3D? point3D_2, out double distance);
            stopwatch.Stop();

            // Assert Correctness
            Assert.NotNull(point3D_1);
            Assert.NotNull(point3D_2);
            Assert.Equal(0, point3D_1.X);
            Assert.Equal(0, point3D_1.Y);
            Assert.Equal(0, point3D_1.Z);
            Assert.Equal(999, point3D_2.X);
            Assert.Equal(999, point3D_2.Y);
            Assert.Equal(999, point3D_2.Z);
            Assert.Equal(System.Math.Sqrt(3) * 999, distance, 1e-6);

            // Assert Performance (optimized should be extremely fast, typically < 100ms)
            Assert.True(stopwatch.ElapsedMilliseconds < 100, $"Spatial ExtremePoints performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the Inside polygon containment query on a large set of vertices.
        /// </summary>
        [Fact]
        public void Inside_PerformanceAndCorrectness()
        {
            // Create a large polygon (a simple square with 1000 collinear edge vertices for testing performance)
            List<Point2D> point2Ds = new();
            for (int i = 0; i < 250; i++) point2Ds.Add(new Point2D(i, 0));
            for (int i = 0; i < 250; i++) point2Ds.Add(new Point2D(250, i));
            for (int i = 250; i > 0; i--) point2Ds.Add(new Point2D(i, 250));
            for (int i = 250; i > 0; i--) point2Ds.Add(new Point2D(0, i));

            Point2D point2D_Inside = new(100, 100);
            Point2D point2D_Outside = new(300, 300);

            // Warm up / JIT compile
            _ = Planar.Query.Inside(point2Ds.Take(10), point2D_Inside);

            // Execute optimized query and measure performance
            Stopwatch stopwatch = Stopwatch.StartNew();
            bool inside_Result = Planar.Query.Inside(point2Ds, point2D_Inside);
            bool outside_Result = Planar.Query.Inside(point2Ds, point2D_Outside);
            stopwatch.Stop();

            // Assert Correctness
            Assert.True(inside_Result);
            Assert.False(outside_Result);

            // Assert Performance (optimized should be extremely fast, typically < 1ms)
            Assert.True(stopwatch.ElapsedMilliseconds < 5, $"Inside polygon performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the ClosestPoint query on a large polyline.
        /// </summary>
        [Fact]
        public void ClosestPoint_Planar_PerformanceAndCorrectness()
        {
            List<Segment2D> segment2Ds = new();
            for (int i = 0; i < 1000; i++)
            {
                segment2Ds.Add(new Segment2D(new Point2D(i, 0), new Point2D(i + 1, 0)));
            }

            Point2D point2D_Target = new(500.5, 10.0);

            // Warm up / JIT compile
            _ = Planar.Query.ClosestPoint(point2D_Target, segment2Ds.Take(10), out _);

            // Execute optimized query and measure performance
            Stopwatch stopwatch = Stopwatch.StartNew();
            Point2D? point2D_Result = Planar.Query.ClosestPoint(point2D_Target, segment2Ds, out double distance);
            stopwatch.Stop();

            // Assert Correctness
            Assert.NotNull(point2D_Result);
            Assert.Equal(500.5, point2D_Result.X);
            Assert.Equal(0, point2D_Result.Y);
            Assert.Equal(10.0, distance, 1e-6);

            // Assert Performance (optimized should be extremely fast, typically < 5ms)
            Assert.True(stopwatch.ElapsedMilliseconds < 10, $"ClosestPoint performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized Average query for both 2D and 3D points on a large lazy sequence.
        /// </summary>
        [Fact]
        public void Average_PerformanceAndCorrectness()
        {
            // Create a lazy sequence of 100,000 points to amplify any multiple-enumeration overhead
            IEnumerable<Point2D> point2Ds_Lazy = Enumerable.Range(0, 100000).Select(i => new Point2D(i, i));
            IEnumerable<Point3D> point3Ds_Lazy = Enumerable.Range(0, 100000).Select(i => new Point3D(i, i, i));

            // Warm up / JIT compile
            _ = Planar.Query.Average(point2Ds_Lazy.Take(10));
            _ = Spatial.Query.Average(point3Ds_Lazy.Take(10));

            // Measure 2D Average performance
            Stopwatch stopwatch_2D = Stopwatch.StartNew();
            Point2D? point2D_Average = Planar.Query.Average(point2Ds_Lazy);
            stopwatch_2D.Stop();

            // Measure 3D Average performance
            Stopwatch stopwatch_3D = Stopwatch.StartNew();
            Point3D? point3D_Average = Spatial.Query.Average(point3Ds_Lazy);
            stopwatch_3D.Stop();

            // Assert Correctness
            Assert.NotNull(point2D_Average);
            Assert.Equal(49999.5, point2D_Average.X, 1e-6);
            Assert.Equal(49999.5, point2D_Average.Y, 1e-6);

            Assert.NotNull(point3D_Average);
            Assert.Equal(49999.5, point3D_Average.X, 1e-6);
            Assert.Equal(49999.5, point3D_Average.Y, 1e-6);
            Assert.Equal(49999.5, point3D_Average.Z, 1e-6);

            // Assert Performance (optimized single-pass should take less than 100ms)
            Assert.True(stopwatch_2D.ElapsedMilliseconds < 100, $"Planar Average performance check failed! Took {stopwatch_2D.ElapsedMilliseconds} ms.");
            Assert.True(stopwatch_3D.ElapsedMilliseconds < 100, $"Spatial Average performance check failed! Took {stopwatch_3D.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized ConvexHull query with order preservation on a lazy sequence.
        /// </summary>
        [Fact]
        public void ConvexHull_PerformanceAndCorrectness()
        {
            // Create a lazy sequence of points (a circle of 2000 points)
            IEnumerable<Point2D> point2Ds_Lazy = Enumerable.Range(0, 2000).Select(i =>
            {
                double angle = i * System.Math.PI / 1000.0;
                return new Point2D(System.Math.Cos(angle) * 100.0, System.Math.Sin(angle) * 100.0);
            });

            // Warm up / JIT compile
            _ = Planar.Query.ConvexHull(point2Ds_Lazy.Take(10), true);

            // Execute and measure
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Point2D>? point2Ds_Hull = Planar.Query.ConvexHull(point2Ds_Lazy, true);
            stopwatch.Stop();

            // Assert Correctness
            Assert.NotNull(point2Ds_Hull);
            Assert.True(point2Ds_Hull.Count >= 3);

            // Assert Performance (optimized should be extremely fast, typically < 100ms)
            Assert.True(stopwatch.ElapsedMilliseconds < 150, $"ConvexHull performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized Polygonal2D creation method on a lazy sequence of points.
        /// </summary>
        [Fact]
        public void Polygonal2D_PerformanceAndCorrectness()
        {
            // Create a lazy sequence representing a triangle
            IEnumerable<Point2D> point2Ds_Triangle = Enumerable.Range(0, 3).Select(i => new Point2D(i, i * i));

            // Warm up / JIT compile
            _ = Planar.Create.Polygonal2D(point2Ds_Triangle);

            // Execute and measure
            Stopwatch stopwatch = Stopwatch.StartNew();
            DiGi.Geometry.Planar.Interfaces.IPolygonal2D? polygonal2D = Planar.Create.Polygonal2D(point2Ds_Triangle);
            stopwatch.Stop();

            // Assert Correctness
            Assert.NotNull(polygonal2D);
            Assert.IsType<Triangle2D>(polygonal2D);

            // Assert Performance
            Assert.True(stopwatch.ElapsedMilliseconds < 50, $"Polygonal2D performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized Polygon3D creation methods on a lazy sequence of 3D points.
        /// </summary>
        [Fact]
        public void Polygon3D_PerformanceAndCorrectness()
        {
            // Create a lazy sequence representing a non-collinear 3D polygon (a unit square)
            IEnumerable<Point3D> point3Ds_Lazy = new List<Point3D>
            {
                new Point3D(0, 0, 0),
                new Point3D(1, 0, 0),
                new Point3D(1, 1, 0),
                new Point3D(0, 1, 0)
            }.Select(x => x);
            DiGi.Geometry.Spatial.Classes.Vector3D vector3D_Normal = Spatial.Constants.Vector3D.WorldZ;

            // Warm up / JIT compile
            _ = Spatial.Create.Polygon3D(point3Ds_Lazy);
            _ = Spatial.Create.Polygon3D(vector3D_Normal, point3Ds_Lazy);

            // Execute and measure first overload
            Stopwatch stopwatch_1 = Stopwatch.StartNew();
            Polygon3D? polygon3D_1 = Spatial.Create.Polygon3D(point3Ds_Lazy);
            stopwatch_1.Stop();

            // Execute and measure second overload
            Stopwatch stopwatch_2 = Stopwatch.StartNew();
            Polygon3D? polygon3D_2 = Spatial.Create.Polygon3D(vector3D_Normal, point3Ds_Lazy);
            stopwatch_2.Stop();

            // Assert Correctness
            Assert.NotNull(polygon3D_1);
            Assert.NotNull(polygon3D_2);

            // Assert Performance
            Assert.True(stopwatch_1.ElapsedMilliseconds < 50, $"Polygon3D first overload performance check failed! Took {stopwatch_1.ElapsedMilliseconds} ms.");
            Assert.True(stopwatch_2.ElapsedMilliseconds < 50, $"Polygon3D second overload performance check failed! Took {stopwatch_2.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized Min/Max coordinate bounds queries for both 2D and 3D points on a large lazy sequence.
        /// </summary>
        [Fact]
        public void MinMax_PerformanceAndCorrectness()
        {
            // Create a lazy sequence of 100,000 points to amplify any multiple-enumeration overhead
            IEnumerable<Point2D> point2Ds_Lazy = Enumerable.Range(0, 100000).Select(i => new Point2D(i, -i));
            IEnumerable<Point3D> point3Ds_Lazy = Enumerable.Range(0, 100000).Select(i => new Point3D(i, -i, i * 2));

            // Warm up / JIT compile
            _ = Planar.Query.Min(point2Ds_Lazy.Take(10), out _);
            _ = Spatial.Query.Min(point3Ds_Lazy.Take(10), out _);

            // Measure 2D Min/Max performance
            Stopwatch stopwatch_2D = Stopwatch.StartNew();
            Point2D? point2D_Min = Planar.Query.Min(point2Ds_Lazy, out Point2D? point2D_Max);
            stopwatch_2D.Stop();

            // Measure 3D Min/Max performance
            Stopwatch stopwatch_3D = Stopwatch.StartNew();
            Point3D? point3D_Min = Spatial.Query.Min(point3Ds_Lazy, out Point3D? point3D_Max);
            stopwatch_3D.Stop();

            // Assert Correctness 2D
            Assert.NotNull(point2D_Min);
            Assert.NotNull(point2D_Max);
            Assert.Equal(0.0, point2D_Min.X, 1e-6);
            Assert.Equal(-99999.0, point2D_Min.Y, 1e-6);
            Assert.Equal(99999.0, point2D_Max.X, 1e-6);
            Assert.Equal(0.0, point2D_Max.Y, 1e-6);

            // Assert Correctness 3D
            Assert.NotNull(point3D_Min);
            Assert.NotNull(point3D_Max);
            Assert.Equal(0.0, point3D_Min.X, 1e-6);
            Assert.Equal(-99999.0, point3D_Min.Y, 1e-6);
            Assert.Equal(0.0, point3D_Min.Z, 1e-6);
            Assert.Equal(99999.0, point3D_Max.X, 1e-6);
            Assert.Equal(0.0, point3D_Max.Y, 1e-6);
            Assert.Equal(199998.0, point3D_Max.Z, 1e-6);

            // Assert Performance (optimized single-pass should take less than 100ms)
            Assert.True(stopwatch_2D.ElapsedMilliseconds < 100, $"Planar Min/Max performance check failed! Took {stopwatch_2D.ElapsedMilliseconds} ms.");
            Assert.True(stopwatch_3D.ElapsedMilliseconds < 100, $"Spatial Min/Max performance check failed! Took {stopwatch_3D.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized Polygons, Polygon2Ds, PolygonalFace2Ds, and Polyline2Ds creators on lazy sequences of segments.
        /// </summary>
        [Fact]
        public void PolygonsAndPolylines_PerformanceAndCorrectness()
        {
            // Create a lazy sequence representing a closed snap-round square (4 segments)
            IEnumerable<Segment2D> segment2Ds_Lazy = new List<Segment2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(10, 0)),
                new Segment2D(new Point2D(10, 0), new Point2D(10, 10)),
                new Segment2D(new Point2D(10, 10), new Point2D(0, 10)),
                new Segment2D(new Point2D(0, 10), new Point2D(0, 0))
            }.Select(x => x);

            // Warm up / JIT compile
            _ = Planar.Create.Polygons(segment2Ds_Lazy.Take(2));
            _ = Planar.Create.Polygon2Ds(segment2Ds_Lazy.Take(2));
            _ = Planar.Create.PolygonalFace2Ds(segment2Ds_Lazy.Take(2));
            _ = Planar.Create.Polyline2Ds(segment2Ds_Lazy.Take(2));

            // Execute and measure
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<NetTopologySuite.Geometries.Polygon>? polygons_NTS = Planar.Create.Polygons(segment2Ds_Lazy);
            List<Polygon2D>? polygon2Ds = Planar.Create.Polygon2Ds(segment2Ds_Lazy);
            List<PolygonalFace2D>? polygonalFace2Ds = Planar.Create.PolygonalFace2Ds(segment2Ds_Lazy);
            List<Polyline2D>? polyline2Ds = Planar.Create.Polyline2Ds(segment2Ds_Lazy);
            stopwatch.Stop();

            // Assert Correctness
            Assert.NotNull(polygons_NTS);
            Assert.Single(polygons_NTS);
            Assert.NotNull(polygon2Ds);
            Assert.Single(polygon2Ds);
            Assert.NotNull(polygonalFace2Ds);
            Assert.Single(polygonalFace2Ds);
            Assert.NotNull(polyline2Ds);

            // Assert Performance (should be extremely fast, typically < 100ms)
            Assert.True(stopwatch.ElapsedMilliseconds < 150, $"Polygons & Polylines creator performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized Distance, ClosestPoint, and On queries on a lazy sequence of segmentable geometries.
        /// </summary>
        [Fact]
        public void DistanceAndClosestPoint_PerformanceAndCorrectness()
        {
            // Create a lazy sequence of segmentable geometries (100 polylines, each with 10 segments)
            IEnumerable<Polyline2D> segmentable2Ds_Lazy = Enumerable.Range(0, 100).Select(i =>
            {
                List<Point2D> point2Ds_Poly = new();
                for (int j = 0; j < 10; j++)
                {
                    point2Ds_Poly.Add(new Point2D(i * 10 + j, 0));
                }
                return new Polyline2D(point2Ds_Poly);
            });

            Point2D point2D_Target = new(500.5, 10.0);

            // Warm up / JIT compile
            _ = Planar.Query.Distance(point2D_Target, segmentable2Ds_Lazy.Take(2));
            _ = Planar.Query.ClosestPoint(point2D_Target, segmentable2Ds_Lazy.Take(2));
            _ = Planar.Query.On(segmentable2Ds_Lazy.Take(2), point2D_Target);

            // Execute and measure
            Stopwatch stopwatch = Stopwatch.StartNew();
            double distance = Planar.Query.Distance(point2D_Target, segmentable2Ds_Lazy, out Point2D? point2D_Closest);
            Point2D? point2D_ClosestPoint = Planar.Query.ClosestPoint(point2D_Target, segmentable2Ds_Lazy, out double distance_Check);
            bool isOn = Planar.Query.On(segmentable2Ds_Lazy, point2D_Target);
            stopwatch.Stop();

            // Assert Correctness
            Assert.Equal(10.0, distance, 1e-6);
            Assert.NotNull(point2D_Closest);
            Assert.Equal(500.5, point2D_Closest.X, 1e-6);
            Assert.Equal(0.0, point2D_Closest.Y, 1e-6);

            Assert.NotNull(point2D_ClosestPoint);
            Assert.Equal(500.5, point2D_ClosestPoint.X, 1e-6);
            Assert.Equal(0.0, point2D_ClosestPoint.Y, 1e-6);
            Assert.Equal(10.0, distance_Check, 1e-6);
            Assert.False(isOn);

            // Assert Performance (should be extremely fast, typically < 100ms)
            Assert.True(stopwatch.ElapsedMilliseconds < 100, $"Distance & ClosestPoint query performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized AdjacentSegments query on a lazy sequence of segmentable geometries.
        /// </summary>
        [Fact]
        public void AdjacentSegments_PerformanceAndCorrectness()
        {
            // Create a lazy sequence of 50 unit squares adjacent to each other
            IEnumerable<Polygon2D> segmentable2Ds_Lazy = Enumerable.Range(0, 50).Select(i =>
            {
                List<Point2D> point2Ds_Square = new()
                {
                    new Point2D(i * 10, 0),
                    new Point2D(i * 10 + 10, 0),
                    new Point2D(i * 10 + 10, 10),
                    new Point2D(i * 10, 10)
                };
                return new Polygon2D(point2Ds_Square);
            });

            // Warm up / JIT compile
            _ = Planar.Query.AdjacentSegments(segmentable2Ds_Lazy.Take(2));

            // Execute and measure
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Segment2D>? segment2Ds_Adjacent = Planar.Query.AdjacentSegments(segmentable2Ds_Lazy);
            stopwatch.Stop();

            // Assert Correctness
            Assert.NotNull(segment2Ds_Adjacent);
            // Since squares share edges at x = 10, 20, 30, etc.
            Assert.True(segment2Ds_Adjacent.Count > 0);

            // Assert Performance (should be extremely fast, typically < 100ms)
            Assert.True(stopwatch.ElapsedMilliseconds < 150, $"AdjacentSegments performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized Rectangle2D creation overloads on lazy sequences of points.
        /// </summary>
        [Fact]
        public void Rectangle2D_PerformanceAndCorrectness()
        {
            IEnumerable<Point2D> point2Ds_Lazy = Enumerable.Range(0, 100).Select(i => new Point2D(i, i * 2));
            Vector2D vector2D_Direction = new(1, 1);
            _ = Planar.Create.Rectangle2D(point2Ds_Lazy.Take(5), vector2D_Direction);
            _ = Planar.Create.Rectangle2D(point2Ds_Lazy.Take(5));
            Stopwatch stopwatch = Stopwatch.StartNew();
            Rectangle2D? rectangle2D_1 = Planar.Create.Rectangle2D(point2Ds_Lazy, vector2D_Direction);
            Rectangle2D? rectangle2D_2 = Planar.Create.Rectangle2D(point2Ds_Lazy);
            stopwatch.Stop();
            Assert.NotNull(rectangle2D_1);
            Assert.NotNull(rectangle2D_2);
            Assert.True(stopwatch.ElapsedMilliseconds < 100, $"Rectangle2D creation performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized PolynomialEquation creation method on lazy sequences of points.
        /// </summary>
        [Fact]
        public void PolynomialEquation_PerformanceAndCorrectness()
        {
            IEnumerable<Point2D> point2Ds_Lazy = Enumerable.Range(0, 500).Select(i => new Point2D(i, i * i));
            _ = Planar.Create.PolynomialEquation(point2Ds_Lazy.Take(5), 2);
            Stopwatch stopwatch = Stopwatch.StartNew();
            PolynomialEquation? polynomialEquation = Planar.Create.PolynomialEquation(point2Ds_Lazy, 2);
            stopwatch.Stop();
            Assert.NotNull(polynomialEquation);
            Assert.True(stopwatch.ElapsedMilliseconds < 150, $"PolynomialEquation creation performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized Directions query on lazy sequences of segmentable geometries.
        /// </summary>
        [Fact]
        public void Directions_PerformanceAndCorrectness()
        {
            IEnumerable<Polygon2D> polygon2Ds_Lazy = Enumerable.Range(0, 50).Select(i =>
            {
                List<Point2D> point2Ds_Square = new()
                {
                    new Point2D(i, 0),
                    new Point2D(i + 1, 0),
                    new Point2D(i + 1, 1),
                    new Point2D(i, 1)
                };
                return new Polygon2D(point2Ds_Square);
            });
            _ = Planar.Query.Directions(polygon2Ds_Lazy.Take(2));
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Vector2D>? vector2Ds_Directions = Planar.Query.Directions(polygon2Ds_Lazy);
            stopwatch.Stop();
            Assert.NotNull(vector2Ds_Directions);
            Assert.True(vector2Ds_Directions.Count > 0);
            Assert.True(stopwatch.ElapsedMilliseconds < 150, $"Directions query performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized Split query on lazy sequences of segments.
        /// </summary>
        [Fact]
        public void Split_PerformanceAndCorrectness()
        {
            List<Segment2D> segment2Ds_Grid = new();
            for (int i = 0; i < 15; i++)
            {
                segment2Ds_Grid.Add(new Segment2D(new Point2D(0, i), new Point2D(15, i)));
                segment2Ds_Grid.Add(new Segment2D(new Point2D(i, 0), new Point2D(i, 15)));
            }
            IEnumerable<Segment2D> segment2Ds_Lazy = segment2Ds_Grid.Select(x => x);
            _ = Planar.Query.Split(segment2Ds_Lazy.Take(2));
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Segment2D>? segment2Ds_Result = Planar.Query.Split(segment2Ds_Lazy);
            stopwatch.Stop();
            Assert.NotNull(segment2Ds_Result);
            Assert.True(segment2Ds_Result.Count > 0);
            Assert.True(stopwatch.ElapsedMilliseconds < 200, $"Split segments performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized Mesh3D creation on a lazy sequence of 3D triangles.
        /// </summary>
        [Fact]
        public void Mesh3D_PerformanceAndCorrectness()
        {
            IEnumerable<Triangle3D> triangle3Ds_Lazy = Enumerable.Range(0, 100).Select(i =>
            {
                Point3D point3D_1 = new(i, 0, 0);
                Point3D point3D_2 = new(i + 1, 1, 0);
                Point3D point3D_3 = new(i, 1, 1);
                return new Triangle3D(point3D_1, point3D_2, point3D_3);
            });
            _ = Spatial.Create.Mesh3D(triangle3Ds_Lazy.Take(2));
            Stopwatch stopwatch = Stopwatch.StartNew();
            Mesh3D? mesh3D = Spatial.Create.Mesh3D(triangle3Ds_Lazy);
            stopwatch.Stop();
            Assert.NotNull(mesh3D);
            Assert.True(stopwatch.ElapsedMilliseconds < 150, $"Mesh3D creation performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized Polyhedron creation on a lazy sequence of 3D polygonal faces.
        /// </summary>
        [Fact]
        public void Polyhedron_PerformanceAndCorrectness()
        {
            List<IPolygonalFace3D> polygonalFace3Ds_Cube = new();
            List<Point3D> point3Ds_F1 = new() { new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(1, 1, 0), new Point3D(0, 1, 0) };
            Polygon3D? polygon3D_F1 = Spatial.Create.Polygon3D(point3Ds_F1);
            Assert.NotNull(polygon3D_F1);
            polygonalFace3Ds_Cube.Add(new PolygonalFace3D(polygon3D_F1));
            List<Point3D> point3Ds_F2 = new() { new Point3D(0, 0, 1), new Point3D(1, 0, 1), new Point3D(1, 1, 1), new Point3D(0, 1, 1) };
            Polygon3D? polygon3D_F2 = Spatial.Create.Polygon3D(point3Ds_F2);
            Assert.NotNull(polygon3D_F2);
            polygonalFace3Ds_Cube.Add(new PolygonalFace3D(polygon3D_F2));
            List<Point3D> point3Ds_F3 = new() { new Point3D(0, 0, 0), new Point3D(0, 1, 0), new Point3D(0, 1, 1), new Point3D(0, 0, 1) };
            Polygon3D? polygon3D_F3 = Spatial.Create.Polygon3D(point3Ds_F3);
            Assert.NotNull(polygon3D_F3);
            polygonalFace3Ds_Cube.Add(new PolygonalFace3D(polygon3D_F3));
            List<Point3D> point3Ds_F4 = new() { new Point3D(1, 0, 0), new Point3D(1, 1, 0), new Point3D(1, 1, 1), new Point3D(1, 0, 1) };
            Polygon3D? polygon3D_F4 = Spatial.Create.Polygon3D(point3Ds_F4);
            Assert.NotNull(polygon3D_F4);
            polygonalFace3Ds_Cube.Add(new PolygonalFace3D(polygon3D_F4));
            List<Point3D> point3Ds_F5 = new() { new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(1, 0, 1), new Point3D(0, 0, 1) };
            Polygon3D? polygon3D_F5 = Spatial.Create.Polygon3D(point3Ds_F5);
            Assert.NotNull(polygon3D_F5);
            polygonalFace3Ds_Cube.Add(new PolygonalFace3D(polygon3D_F5));
            List<Point3D> point3Ds_F6 = new() { new Point3D(0, 1, 0), new Point3D(1, 1, 0), new Point3D(1, 1, 1), new Point3D(0, 1, 1) };
            Polygon3D? polygon3D_F6 = Spatial.Create.Polygon3D(point3Ds_F6);
            Assert.NotNull(polygon3D_F6);
            polygonalFace3Ds_Cube.Add(new PolygonalFace3D(polygon3D_F6));
            IEnumerable<IPolygonalFace3D> polygonalFace3Ds_Lazy = polygonalFace3Ds_Cube.Select(x => x);
            _ = Spatial.Create.Polyhedron(polygonalFace3Ds_Lazy.Take(4));
            Stopwatch stopwatch = Stopwatch.StartNew();
            Polyhedron? polyhedron = Spatial.Create.Polyhedron(polygonalFace3Ds_Lazy);
            stopwatch.Stop();
            Assert.NotNull(polyhedron);
            Assert.True(stopwatch.ElapsedMilliseconds < 150, $"Polyhedron creation performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the Mesh constructor optimizations.
        /// </summary>
        [Fact]
        public void Mesh_PerformanceAndCorrectness()
        {
            List<Point3D> point3Ds = new();
            for (int i = 0; i < 1000; i++)
            {
                point3Ds.Add(new Point3D(i, 0, 0));
            }
            List<int[]> indices = new();
            for (int i = 0; i < 998; i++)
            {
                indices.Add([i, i + 1, i + 2]);
            }
            Stopwatch stopwatch = Stopwatch.StartNew();
            Mesh3D mesh3D = new(point3Ds, indices);
            stopwatch.Stop();
            Assert.NotNull(mesh3D);
            Assert.True(stopwatch.ElapsedMilliseconds < 50, $"Mesh constructor performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized Core index queries on lazy sequences.
        /// </summary>
        [Fact]
        public void IndexQueries_PerformanceAndCorrectness()
        {
            IEnumerable<int[]> indexes_Lazy = Enumerable.Range(0, 1000).Select(i => new int[] { i, i + 1, i + 2 });
            _ = Core.Query.AdjacencyIndexes(indexes_Lazy.Take(5));
            _ = Core.Query.AuxiliaryIndexes(indexes_Lazy.Take(5));
            _ = Core.Query.BoundaryIndexes(indexes_Lazy.Take(5), out _);
            _ = Core.Query.IsNonManifold(indexes_Lazy.Take(5));
            Stopwatch stopwatch = Stopwatch.StartNew();
            Dictionary<int, List<int[]>>? adjacentEdges_ByVertex = Core.Query.AdjacencyIndexes(indexes_Lazy);
            List<int[]>? intArrays_Auxiliary = Core.Query.AuxiliaryIndexes(indexes_Lazy);
            List<int[]>? intArrays_Boundary = Core.Query.BoundaryIndexes(indexes_Lazy, out List<int[]>? intArrays_AuxiliaryOut);
            bool isNonManifold = Core.Query.IsNonManifold(indexes_Lazy);
            stopwatch.Stop();
            Assert.NotNull(adjacentEdges_ByVertex);
            Assert.NotNull(intArrays_Auxiliary);
            Assert.NotNull(intArrays_Boundary);
            Assert.NotNull(intArrays_AuxiliaryOut);
            Assert.False(isNonManifold);
            Assert.True(stopwatch.ElapsedMilliseconds < 150, $"IndexQueries performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized Mesh2D creation on a lazy sequence of 2D triangles.
        /// </summary>
        [Fact]
        public void Mesh2D_PerformanceAndCorrectness()
        {
            IEnumerable<Triangle2D> triangle2Ds_Lazy = Enumerable.Range(0, 100).Select(i =>
            {
                Point2D point2D_1 = new(i, 0);
                Point2D point2D_2 = new(i + 1, 1);
                Point2D point2D_3 = new(i, 1);
                return new Triangle2D(point2D_1, point2D_2, point2D_3);
            });
            _ = Planar.Create.Mesh2D(triangle2Ds_Lazy.Take(2));
            Stopwatch stopwatch = Stopwatch.StartNew();
            Mesh2D? mesh2D = Planar.Create.Mesh2D(triangle2Ds_Lazy);
            stopwatch.Stop();
            Assert.NotNull(mesh2D);
            Assert.True(stopwatch.ElapsedMilliseconds < 150, $"Mesh2D creation performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized Polyline2Ds creation utilizing the O(1) out-edge count optimizer.
        /// </summary>
        [Fact]
        public void Polyline2Ds_PerformanceAndCorrectness()
        {
            IEnumerable<Segment2D> segment2Ds_Lazy = Enumerable.Range(0, 50).Select(i => new Segment2D(new Point2D(i, 0), new Point2D(i + 1, 0)));
            _ = Planar.Create.Polyline2Ds(segment2Ds_Lazy.Take(2));
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Polyline2D>? polyline2Ds = Planar.Create.Polyline2Ds(segment2Ds_Lazy);
            stopwatch.Stop();
            Assert.NotNull(polyline2Ds);
            Assert.True(polyline2Ds.Count > 0);
            Assert.True(stopwatch.ElapsedMilliseconds < 150, $"Polyline2Ds creation performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized Determinants calculation.
        /// </summary>
        [Fact]
        public void Determinants_PerformanceAndCorrectness()
        {
            IEnumerable<Point2D> point2Ds_Lazy = Enumerable.Range(0, 1000).Select(i => new Point2D(i, i % 2 == 0 ? 0 : 1));
            _ = Planar.Query.Determinants(point2Ds_Lazy.Take(5));
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<double>? double_Determinants = Planar.Query.Determinants(point2Ds_Lazy);
            stopwatch.Stop();
            Assert.NotNull(double_Determinants);
            Assert.Equal(1000, double_Determinants.Count);
            Assert.True(stopwatch.ElapsedMilliseconds < 100, $"Determinants calculation performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized Spatial Orientation query on lazy sequences.
        /// </summary>
        [Fact]
        public void Orientation_Spatial_PerformanceAndCorrectness()
        {
            Plane plane = new(new Point3D(0, 0, 0), Spatial.Constants.Vector3D.WorldZ);
            IEnumerable<Point2D> point2Ds_Lazy = new List<Point2D>
            {
                new Point2D(0, 0),
                new Point2D(1, 0),
                new Point2D(0, 1)
            }.Select(x => x);
            _ = Spatial.Query.Orientation(plane, point2Ds_Lazy.Take(2));
            Stopwatch stopwatch = Stopwatch.StartNew();
            DiGi.Geometry.Core.Enums.Orientation orientation = Spatial.Query.Orientation(plane, point2Ds_Lazy);
            stopwatch.Stop();
            Assert.NotEqual(Core.Enums.Orientation.Undefined, orientation);
            Assert.True(stopwatch.ElapsedMilliseconds < 100, $"Spatial Orientation performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized Triangle3D GetArea method.
        /// </summary>
        [Fact]
        public void Triangle3D_GetArea_PerformanceAndCorrectness()
        {
            Point3D point3D_1 = new(0, 0, 0);
            Point3D point3D_2 = new(10, 0, 0);
            Point3D point3D_3 = new(0, 10, 0);
            Triangle3D triangle3D = new(point3D_1, point3D_2, point3D_3);

            // Warm up / JIT compile
            _ = triangle3D.GetArea();

            // Execute and measure
            Stopwatch stopwatch = Stopwatch.StartNew();
            double double_Area = 0;
            for (int int_I = 0; int_I < 100000; int_I++)
            {
                double_Area = triangle3D.GetArea();
            }
            stopwatch.Stop();

            // Assert Correctness
            Assert.Equal(50.0, double_Area, 1e-6);

            // Assert Performance (100,000 iterations should take less than 100ms)
            Assert.True(stopwatch.ElapsedMilliseconds < 100, $"Triangle3D GetArea performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Verifies both the correctness and the performance of the optimized Planar Area shoelace query.
        /// </summary>
        [Fact]
        public void Planar_Area_PerformanceAndCorrectness()
        {
            List<Point2D> point2Ds = new();
            for (int int_I = 0; int_I < 50000; int_I++)
            {
                double double_Angle = int_I * 2.0 * System.Math.PI / 50000.0;
                point2Ds.Add(new(System.Math.Cos(double_Angle) * 10.0, System.Math.Sin(double_Angle) * 10.0));
            }

            // Warm up / JIT compile
            _ = Planar.Query.Area(point2Ds.Take(10));

            // Execute and measure
            Stopwatch stopwatch = Stopwatch.StartNew();
            double double_Area = Planar.Query.Area(point2Ds);
            stopwatch.Stop();

            // Assert Correctness
            Assert.Equal(314.15926, double_Area, 1e-5);

            // Assert Performance (should be extremely fast, typically < 20ms)
            Assert.True(stopwatch.ElapsedMilliseconds < 100, $"Planar Area performance check failed! Took {stopwatch.ElapsedMilliseconds} ms.");
        }
    }
}
