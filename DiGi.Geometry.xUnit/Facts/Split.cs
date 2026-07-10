using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the functionality of splitting a <see cref="Polyhedron"/> using a plane to ensure that the polyhedron is correctly divided into multiple parts.
        /// </summary>
        [Fact]
        public void Split()
        {
            PolygonalFace3D? polygonalFace3D = Create.PolygonalFace3D(Spatial.Constants.Plane.WorldZ, new Point2D(0, 0), new Point2D(0, 10), new Point2D(10, 10), new Point2D(10, 0));

            Assert.NotNull(polygonalFace3D);

            if (polygonalFace3D is null)
            {
                return;
            }

            PolygonalFaceExtrusion polygonalFaceExtrusion = new(polygonalFace3D, Spatial.Constants.Vector3D.WorldZ * 10);

            Assert.NotNull(polygonalFaceExtrusion);

            if (polygonalFaceExtrusion is null)
            {
                return;
            }

            Polyhedron? polyhedron = Create.Polyhedron(polygonalFaceExtrusion);
            Assert.NotNull(polyhedron);

            Assert.True(Spatial.Query.TrySplit(new Plane(Create.Plane(1.0)), polyhedron, out List<Polyhedron>? polyhedrons));

            Assert.NotNull(polyhedrons);
            if (polyhedrons is null)
            {
                return;
            }

            foreach (Polyhedron polyhedron_Split in polyhedrons)
            {
                Point3D? internalPoint = polyhedron_Split?.GetInternalPoint();
                Assert.NotNull(internalPoint);

                if (internalPoint is null)
                {
                    continue;
                }

                bool inside = polyhedron.Inside(internalPoint);
                Assert.True(inside);
            }
        }

        /// <summary>
        /// Tests splitting a 3D polygonal face with other polygonal faces.
        /// </summary>
        [Fact]
        public void SplitFace()
        {
            Polygon2D polygon2D_1 = new(new List<Point2D>()
            {
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(0, 10)
            });

            PolygonalFace2D? polygonalFace2D_1 = Planar.Create.PolygonalFace2D(polygon2D_1);
            Assert.NotNull(polygonalFace2D_1);

            PolygonalFace3D polygonalFace3D_1 = new(Spatial.Constants.Plane.WorldZ, polygonalFace2D_1);

            Polygon2D polygon2D_2 = new(new List<Point2D>()
            {
                new Point2D(5, 5),
                new Point2D(20, 5),
                new Point2D(20, 20),
                new Point2D(5, 20)
            });

            Polygon3D? polygon3D_2 = Create.Polygon3D(new List<Point3D>()
            {
                new Point3D(2, 5, -1),
                new Point3D(2, 20, -1),
                new Point3D(2, 20, 1),
                new Point3D(2, 5, 1)
            });
            Assert.NotNull(polygon3D_2);

            PolygonalFace3D? polygonalFace3D_2 = Create.PolygonalFace3D(polygon3D_2);
            Assert.NotNull(polygonalFace3D_2);

            Polygon3D? polygon3D_3 = Create.Polygon3D(new List<Point3D>()
            {
                new Point3D(2, 5, -1),
                new Point3D(2, 5, 1),
                new Point3D(10, 5, 1),
                new Point3D(10, 5, -1)
            });
            Assert.NotNull(polygon3D_3);

            PolygonalFace3D? polygonalFace3D_3 = Create.PolygonalFace3D(polygon3D_3);
            Assert.NotNull(polygonalFace3D_3);

            bool splitSuccess = polygonalFace3D_1.TrySplit(new IPolygonalFace3D[] { polygonalFace3D_2!, polygonalFace3D_3! }, out List<PolygonalFace3D>? polygonalFace3Ds);
            Assert.True(splitSuccess);
            Assert.NotNull(polygonalFace3Ds);
            Assert.NotEmpty(polygonalFace3Ds);
        }

        /// <summary>
        /// Tests the segment split functionality under various geometric conditions and edge cases.
        /// <para>Verifies correct behavior for overlapping, collinear, intersecting, and zero-length segments, as well as duplicate filtering and tolerance boundaries.</para>
        /// </summary>
        [Fact]
        public void Split_Segments()
        {
            double tolerance = 1e-5;

            // Edge Case 1: Null or empty input
            List<Segment2D>? segment2Ds_Null = null;
            Assert.Null(Planar.Query.Split(segment2Ds_Null, tolerance));

            List<Segment2D> segment2Ds_Empty = [];
            List<Segment2D>? segment2Ds_EmptyResult = Planar.Query.Split(segment2Ds_Empty, tolerance);
            Assert.NotNull(segment2Ds_EmptyResult);
            Assert.Empty(segment2Ds_EmptyResult);

            // Edge Case 2: Segment shorter than tolerance (should be ignored)
            List<Segment2D> segment2Ds_Short = [new Segment2D(new Point2D(0, 0), new Point2D(0, tolerance * 0.5))];
            List<Segment2D>? segment2Ds_ShortResult = Planar.Query.Split(segment2Ds_Short, tolerance);
            Assert.NotNull(segment2Ds_ShortResult);
            Assert.Empty(segment2Ds_ShortResult);

            // Edge Case 3: Duplicate segments (should be deduplicated)
            List<Segment2D> segment2Ds_Duplicates =
            [
                new Segment2D(new Point2D(0, 0), new Point2D(10, 0)),
                new Segment2D(new Point2D(0, 0), new Point2D(10, 0)),
                new Segment2D(new Point2D(10, 0), new Point2D(0, 0)) // Reversed duplicate
            ];
            List<Segment2D>? segment2Ds_DuplicatesResult = Planar.Query.Split(segment2Ds_Duplicates, tolerance);
            Assert.NotNull(segment2Ds_DuplicatesResult);
            Assert.Single(segment2Ds_DuplicatesResult);
            Assert.True(Planar.Query.Similar(segment2Ds_DuplicatesResult[0], new Segment2D(new Point2D(0, 0), new Point2D(10, 0)), tolerance));

            // Edge Case 4: Non-intersecting segments
            List<Segment2D> segment2Ds_NoIntersection =
            [
                new Segment2D(new Point2D(0, 0), new Point2D(10, 0)),
                new Segment2D(new Point2D(0, 5), new Point2D(10, 5))
            ];
            List<Segment2D>? segment2Ds_NoIntersectionResult = Planar.Query.Split(segment2Ds_NoIntersection, tolerance);
            Assert.NotNull(segment2Ds_NoIntersectionResult);
            Assert.Equal(2, segment2Ds_NoIntersectionResult.Count);

            // Edge Case 5: T-junction (endpoint on interior of another segment)
            List<Segment2D> segment2Ds_TJunction =
            [
                new Segment2D(new Point2D(0, 0), new Point2D(10, 0)),
                new Segment2D(new Point2D(5, 0), new Point2D(5, 5))
            ];
            List<Segment2D>? segment2Ds_TJunctionResult = Planar.Query.Split(segment2Ds_TJunction, tolerance);
            Assert.NotNull(segment2Ds_TJunctionResult);
            // The horizontal segment should be split into two: (0,0)->(5,0) and (5,0)->(10,0).
            // The vertical segment is untouched. Total: 3 segments.
            Assert.Equal(3, segment2Ds_TJunctionResult.Count);

            // Edge Case 6: Cross-intersection (interior intersection)
            List<Segment2D> segment2Ds_Cross =
            [
                new Segment2D(new Point2D(0, 5), new Point2D(10, 5)),
                new Segment2D(new Point2D(5, 0), new Point2D(5, 10))
            ];
            List<Segment2D>? segment2Ds_CrossResult = Planar.Query.Split(segment2Ds_Cross, tolerance);
            Assert.NotNull(segment2Ds_CrossResult);
            // Both segments split at (5,5) into 2 parts each. Total: 4 segments.
            Assert.Equal(4, segment2Ds_CrossResult.Count);

            // Edge Case 7: Collinear overlapping segments
            List<Segment2D> segment2Ds_CollinearOverlapping =
            [
                new Segment2D(new Point2D(0, 0), new Point2D(6, 0)),
                new Segment2D(new Point2D(4, 0), new Point2D(10, 0))
            ];
            List<Segment2D>? segment2Ds_CollinearOverlappingResult = Planar.Query.Split(segment2Ds_CollinearOverlapping, tolerance);
            Assert.NotNull(segment2Ds_CollinearOverlappingResult);
            // Expected pieces: (0,0)->(4,0), (4,0)->(6,0), and (6,0)->(10,0). Total: 3 segments.
            Assert.Equal(3, segment2Ds_CollinearOverlappingResult.Count);
        }
    }
}