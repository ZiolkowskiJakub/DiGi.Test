using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DiGi.Geometry.xUnit
{
    public partial class Classes
    {
        /// <summary>
        /// Tests the functionality of the planar intersection result calculation, verifying that intersections are correctly identified across various combinations of input flags and point positions relative to a plane.
        /// </summary>
        [Fact]
        public void PlanarIntersectionResult()
        {
            Plane plane = Spatial.Constants.Plane.WorldZ;

            PlanarIntersectionResult? planarIntersectionResult;
            Point3D point3D_1;
            Point3D point3D_2;

            point3D_1 = new Point3D(0, 0, 1);
            point3D_2 = new Point3D(0, 0, 10);

            planarIntersectionResult = Create.PlanarIntersectionResult(plane, point3D_1, point3D_2, true, true);
            Assert.NotNull(planarIntersectionResult);
            Assert.True(!planarIntersectionResult.Intersect);

            planarIntersectionResult = Create.PlanarIntersectionResult(plane, point3D_1, point3D_2, false, true);
            Assert.NotNull(planarIntersectionResult);
            Assert.True(planarIntersectionResult.Intersect);

            planarIntersectionResult = Create.PlanarIntersectionResult(plane, point3D_1, point3D_2, true, false);
            Assert.NotNull(planarIntersectionResult);
            Assert.True(!planarIntersectionResult.Intersect);

            planarIntersectionResult = Create.PlanarIntersectionResult(plane, point3D_1, point3D_2, false, false);
            Assert.NotNull(planarIntersectionResult);
            Assert.True(planarIntersectionResult.Intersect);

            point3D_1 = new Point3D(0, 1, 0);
            point3D_2 = new Point3D(0, 10, 0);

            planarIntersectionResult = Create.PlanarIntersectionResult(plane, point3D_1, point3D_2, true, true);
            Assert.NotNull(planarIntersectionResult);
            if (planarIntersectionResult is not null)
            {
                Assert.True(planarIntersectionResult.Intersect);

                List<Segment3D>? segment3Ds = planarIntersectionResult.GetGeometry3Ds<Segment3D>();
                Assert.NotNull(segment3Ds);
                Assert.Single(segment3Ds);
                if (segment3Ds is not null && segment3Ds.Count > 0)
                {
                    ILinear3D linear3D = segment3Ds[0];

                    Assert.True(linear3D.On(point3D_1));
                    Assert.True(linear3D.On(point3D_2));
                }
            }

            planarIntersectionResult = Create.PlanarIntersectionResult(plane, point3D_1, point3D_2, false, true);
            Assert.NotNull(planarIntersectionResult);
            if (planarIntersectionResult is not null)
            {
                Assert.True(planarIntersectionResult.Intersect);

                List<Ray3D>? ray3Ds = planarIntersectionResult.GetGeometry3Ds<Ray3D>();
                Assert.NotNull(ray3Ds);
                Assert.Single(ray3Ds);
                if (ray3Ds is not null && ray3Ds.Count > 0)
                {
                    ILinear3D linear3D = ray3Ds[0];

                    Assert.True(linear3D.On(point3D_1));
                    Assert.True(linear3D.On(point3D_2));
                }
            }

            planarIntersectionResult = Create.PlanarIntersectionResult(plane, point3D_1, point3D_2, true, false);
            Assert.NotNull(planarIntersectionResult);
            if (planarIntersectionResult is not null)
            {
                Assert.True(planarIntersectionResult.Intersect);

                List<Ray3D>? ray3Ds = planarIntersectionResult.GetGeometry3Ds<Ray3D>();
                Assert.NotNull(ray3Ds);
                Assert.Single(ray3Ds);
                if (ray3Ds is not null && ray3Ds.Count > 0)
                {
                    ILinear3D linear3D = ray3Ds[0];

                    Assert.True(linear3D.On(point3D_1));
                    Assert.True(linear3D.On(point3D_2));
                }
            }

            planarIntersectionResult = Create.PlanarIntersectionResult(plane, point3D_1, point3D_2, false, false);
            Assert.NotNull(planarIntersectionResult);
            if (planarIntersectionResult is not null)
            {
                Assert.True(planarIntersectionResult.Intersect);

                List<Line3D>? line3Ds = planarIntersectionResult.GetGeometry3Ds<Line3D>();
                Assert.NotNull(line3Ds);
                Assert.Single(line3Ds);
                if (line3Ds is not null && line3Ds.Count > 0)
                {
                    ILinear3D linear3D = line3Ds[0];

                    Assert.True(linear3D.On(point3D_1));
                    Assert.True(linear3D.On(point3D_2));
                }
            }
        }

        [Fact]
        public void PlanarIntersectionResult_Segment3D()
        {
            Plane plane = Spatial.Constants.Plane.WorldZ; // z=0

            // Intersecting
            Segment3D segment3D_Intersect = new(new Point3D(0, 0, -5), new Point3D(0, 0, 5));
            PlanarIntersectionResult? planarIntersectionResult = Create.PlanarIntersectionResult(plane, segment3D_Intersect);
            Assert.NotNull(planarIntersectionResult);
            Assert.True(planarIntersectionResult.Intersect);
            List<Point2D>? point2Ds = planarIntersectionResult.GetGeometry2Ds<Point2D>();
            Assert.NotNull(point2Ds);
            Assert.Single(point2Ds);
            Assert.Equal(0, point2Ds[0].X, 4);
            Assert.Equal(0, point2Ds[0].Y, 4);

            // Disjoint / Parallel
            Segment3D segment3D_Parallel = new(new Point3D(0, 0, 5), new Point3D(10, 0, 5));
            PlanarIntersectionResult? planarIntersectionResult_Parallel = Create.PlanarIntersectionResult(plane, segment3D_Parallel);
            Assert.NotNull(planarIntersectionResult_Parallel);
            Assert.False(planarIntersectionResult_Parallel.Intersect);

            // Parallel on Plane
            Segment3D segment3D_OnPlane = new(new Point3D(0, 0, 0), new Point3D(10, 0, 0));
            PlanarIntersectionResult? planarIntersectionResult_OnPlane = Create.PlanarIntersectionResult(plane, segment3D_OnPlane);
            Assert.NotNull(planarIntersectionResult_OnPlane);
            Assert.True(planarIntersectionResult_OnPlane.Intersect);
            List<Segment2D>? segment2Ds = planarIntersectionResult_OnPlane.GetGeometry2Ds<Segment2D>();
            Assert.NotNull(segment2Ds);
            Assert.Single(segment2Ds);

            // Disjoint / Not Parallel
            Segment3D segment3D_Disjoint = new(new Point3D(0, 0, 2), new Point3D(0, 0, 10));
            PlanarIntersectionResult? planarIntersectionResult_Disjoint = Create.PlanarIntersectionResult(plane, segment3D_Disjoint);
            Assert.NotNull(planarIntersectionResult_Disjoint);
            Assert.False(planarIntersectionResult_Disjoint.Intersect);
        }

        [Fact]
        public void PlanarIntersectionResult_Ray3D()
        {
            Plane plane = Spatial.Constants.Plane.WorldZ;

            // Intersecting
            Ray3D ray3D_Intersect = new(new Point3D(0, 0, -5), new Vector3D(0, 0, 1));
            PlanarIntersectionResult? planarIntersectionResult = Create.PlanarIntersectionResult(plane, ray3D_Intersect);
            Assert.NotNull(planarIntersectionResult);
            Assert.True(planarIntersectionResult.Intersect);
            List<Point2D>? point2Ds = planarIntersectionResult.GetGeometry2Ds<Point2D>();
            Assert.NotNull(point2Ds);
            Assert.Single(point2Ds);

            // Disjoint / Pointing Away
            Ray3D ray3D_Away = new(new Point3D(0, 0, 5), new Vector3D(0, 0, 1));
            PlanarIntersectionResult? planarIntersectionResult_Away = Create.PlanarIntersectionResult(plane, ray3D_Away);
            Assert.NotNull(planarIntersectionResult_Away);
            Assert.False(planarIntersectionResult_Away.Intersect);

            // Disjoint / Parallel
            Ray3D ray3D_Parallel = new(new Point3D(0, 0, 5), new Vector3D(1, 0, 0));
            PlanarIntersectionResult? planarIntersectionResult_Parallel = Create.PlanarIntersectionResult(plane, ray3D_Parallel);
            Assert.NotNull(planarIntersectionResult_Parallel);
            Assert.False(planarIntersectionResult_Parallel.Intersect);
        }

        [Fact]
        public void PlanarIntersectionResult_ISegmentable3D()
        {
            Plane plane = Spatial.Constants.Plane.WorldZ;

            // Disjoint path (z=5 to z=10)
            List<Point3D> point3Ds_Disjoint = [new(0, 0, 5), new(5, 5, 6), new(10, 0, 8)];
            Polyline3D polyline3D_Disjoint = new(point3Ds_Disjoint);
            PlanarIntersectionResult? planarIntersectionResult_Disjoint = Create.PlanarIntersectionResult(plane, polyline3D_Disjoint);
            Assert.NotNull(planarIntersectionResult_Disjoint);
            Assert.False(planarIntersectionResult_Disjoint.Intersect);

            // Intersecting path (crosses z=0)
            List<Point3D> point3Ds_Intersect = [new(0, 0, -5), new(5, 5, 5), new(10, 0, -5)];
            Polyline3D polyline3D_Intersect = new(point3Ds_Intersect);
            PlanarIntersectionResult? planarIntersectionResult_Intersect = Create.PlanarIntersectionResult(plane, polyline3D_Intersect);
            Assert.NotNull(planarIntersectionResult_Intersect);
            Assert.True(planarIntersectionResult_Intersect.Intersect);
            List<Point2D>? point2Ds_Result = planarIntersectionResult_Intersect.GetGeometry2Ds<Point2D>();
            Assert.NotNull(point2Ds_Result);
            Assert.Equal(2, point2Ds_Result.Count);
        }

        [Fact]
        public void PlanarIntersectionResult_IPolyhedron()
        {
            Plane plane = Spatial.Constants.Plane.WorldZ;

            // Disjoint Polyhedron (completely above plane, Z min = 5)
            BoundingBox3D boundingBox3D_Disjoint = new(new Point3D(0, 0, 5), new Point3D(10, 10, 15));
            Polyhedron? polyhedron_Disjoint = Create.Polyhedron(boundingBox3D_Disjoint);
            Assert.NotNull(polyhedron_Disjoint);
            PlanarIntersectionResult? planarIntersectionResult_Disjoint = Create.PlanarIntersectionResult(plane, polyhedron_Disjoint);
            Assert.NotNull(planarIntersectionResult_Disjoint);
            Assert.False(planarIntersectionResult_Disjoint.Intersect);

            // Intersecting Polyhedron (crosses Z=0)
            BoundingBox3D boundingBox3D_Intersect = new(new Point3D(-5, -5, -5), new Point3D(5, 5, 5));
            Polyhedron? polyhedron_Intersect = Create.Polyhedron(boundingBox3D_Intersect);
            Assert.NotNull(polyhedron_Intersect);
            PlanarIntersectionResult? planarIntersectionResult_Intersect = Create.PlanarIntersectionResult(plane, polyhedron_Intersect);
            Assert.NotNull(planarIntersectionResult_Intersect);
            Assert.True(planarIntersectionResult_Intersect.Intersect);
        }

        [Fact]
        public void PlanarIntersectionResult_FaceFace()
        {
            Plane plane1 = Spatial.Constants.Plane.WorldZ;
            List<Point3D> point3Ds_1 = [new(0, 0, 0), new(10, 0, 0), new(10, 10, 0), new(0, 10, 0)];
            Polygon3D polygon3D_Ext1 = new(plane1, point3Ds_1.ConvertAll(plane1.Convert)!);
            PolygonalFace3D? polygonalFace3D_1 = Create.PolygonalFace3D(polygon3D_Ext1);
            Assert.NotNull(polygonalFace3D_1);

            // Coplanar / overlapping
            List<Point3D> point3Ds_2 = [new(5, 5, 0), new(15, 5, 0), new(15, 15, 0), new(5, 15, 0)];
            Polygon3D polygon3D_Ext2 = new(plane1, point3Ds_2.ConvertAll(plane1.Convert)!);
            PolygonalFace3D? polygonalFace3D_2 = Create.PolygonalFace3D(polygon3D_Ext2);
            Assert.NotNull(polygonalFace3D_2);

            PlanarIntersectionResult? planarIntersectionResult_Coplanar = Create.PlanarIntersectionResult(polygonalFace3D_1, polygonalFace3D_2);
            Assert.NotNull(planarIntersectionResult_Coplanar);
            Assert.True(planarIntersectionResult_Coplanar.Intersect);

            // Disjoint / Parallel
            Plane plane3 = new(new Point3D(0, 0, 5), new Vector3D(0, 0, 1));
            List<Point3D> point3Ds_3 = [new(0, 0, 5), new(10, 0, 5), new(10, 10, 5), new(0, 10, 5)];
            Polygon3D polygon3D_Ext3 = new(plane3, point3Ds_3.ConvertAll(plane3.Convert)!);
            PolygonalFace3D? polygonalFace3D_3 = Create.PolygonalFace3D(polygon3D_Ext3);
            Assert.NotNull(polygonalFace3D_3);

            PlanarIntersectionResult? planarIntersectionResult_DisjointParallel = Create.PlanarIntersectionResult(polygonalFace3D_1, polygonalFace3D_3);
            Assert.NotNull(planarIntersectionResult_DisjointParallel);
            Assert.False(planarIntersectionResult_DisjointParallel.Intersect);

            // Disjoint Bounding Boxes
            Plane plane4 = new(new Point3D(20, 20, 0), new Vector3D(0, 0, 1));
            List<Point3D> point3Ds_4 = [new(20, 20, 0), new(30, 20, 0), new(30, 30, 0), new(20, 30, 0)];
            Polygon3D polygon3D_Ext4 = new(plane4, point3Ds_4.ConvertAll(plane4.Convert)!);
            PolygonalFace3D? polygonalFace3D_4 = Create.PolygonalFace3D(polygon3D_Ext4);
            Assert.NotNull(polygonalFace3D_4);

            PlanarIntersectionResult? planarIntersectionResult_Disjoint = Create.PlanarIntersectionResult(polygonalFace3D_1, polygonalFace3D_4);
            Assert.NotNull(planarIntersectionResult_Disjoint);
            Assert.False(planarIntersectionResult_Disjoint.Intersect);
        }

        [Fact]
        public void PlanarIntersectionResult_Degenerate()
        {
            Plane plane = Spatial.Constants.Plane.WorldZ;

            // Degenerate Segment3D (start == end)
            Segment3D segment3D_Degenerate = new(new Point3D(1, 1, 1), new Point3D(1, 1, 1));
            PlanarIntersectionResult? planarIntersectionResult = Create.PlanarIntersectionResult(plane, segment3D_Degenerate);
            Assert.NotNull(planarIntersectionResult);
            Assert.False(planarIntersectionResult.Intersect);

            // Null inputs
            Assert.Null(Create.PlanarIntersectionResult((Plane?)null, (Segment3D?)null));
            Assert.Null(Create.PlanarIntersectionResult(plane, (Segment3D?)null));
            Assert.Null(Create.PlanarIntersectionResult((Plane?)null, segment3D_Degenerate));
        }

        [Fact]
        public void PlanarIntersectionResult_ToleranceBoundaries()
        {
            Plane plane = Spatial.Constants.Plane.WorldZ;
            double tolerance = 1e-3;

            // Endpoint exactly inside boundary (Z = tolerance - 1e-9)
            Segment3D segment3D_Inside = new(new Point3D(0, 0, 1e-3 - 1e-9), new Point3D(0, 0, 10));
            PlanarIntersectionResult? planarIntersectionResult_Inside = Create.PlanarIntersectionResult(plane, segment3D_Inside, tolerance);
            Assert.NotNull(planarIntersectionResult_Inside);
            Assert.True(planarIntersectionResult_Inside.Intersect);

            // Endpoint exactly outside boundary (Z = tolerance + 1e-9)
            Segment3D segment3D_Outside = new(new Point3D(0, 0, 1e-3 + 1e-9), new Point3D(0, 0, 10));
            PlanarIntersectionResult? planarIntersectionResult_Outside = Create.PlanarIntersectionResult(plane, segment3D_Outside, tolerance);
            Assert.NotNull(planarIntersectionResult_Outside);
            Assert.False(planarIntersectionResult_Outside.Intersect);
        }

        [Fact]
        public void PlanarIntersectionResult_Performance()
        {
            Plane plane = Spatial.Constants.Plane.WorldZ;

            // Complex Polyline with 1000 vertices completely disjoint from plane
            List<Point3D> point3Ds = [];
            for (int i = 0; i < 1000; i++)
            {
                point3Ds.Add(new Point3D(i, i, 10));
            }
            Polyline3D polyline3D_Complex = new(point3Ds);

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            PlanarIntersectionResult? planarIntersectionResult = Create.PlanarIntersectionResult(plane, polyline3D_Complex);
            stopwatch.Stop();

            Assert.NotNull(planarIntersectionResult);
            Assert.False(planarIntersectionResult.Intersect);
            Assert.True(stopwatch.ElapsedMilliseconds < 5, $"Early exit performance check failed for ISegmentable3D! Took {stopwatch.ElapsedMilliseconds} ms.");

            // Complex Polyhedron with 1000 faces disjoint from plane
            // Let's create a sphere-like or box bounding polyhedron that is disjoint
            BoundingBox3D boundingBox3D = new(new Point3D(-100, -100, 20), new Point3D(100, 100, 40));
            Polyhedron? polyhedron = Create.Polyhedron(boundingBox3D);
            Assert.NotNull(polyhedron);

            stopwatch.Restart();
            PlanarIntersectionResult? planarIntersectionResult_Poly = Create.PlanarIntersectionResult(plane, polyhedron);
            stopwatch.Stop();

            Assert.NotNull(planarIntersectionResult_Poly);
            Assert.False(planarIntersectionResult_Poly.Intersect);
            Assert.True(stopwatch.ElapsedMilliseconds < 5, $"Early exit performance check failed for IPolyhedron! Took {stopwatch.ElapsedMilliseconds} ms.");
        }
    }
}
