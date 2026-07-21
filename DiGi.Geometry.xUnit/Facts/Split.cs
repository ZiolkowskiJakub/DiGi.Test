using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;
using System.Diagnostics;

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

            Assert.True(Query.TrySplit(new Plane(Create.Plane(1.0)), polyhedron, out List<Polyhedron>? polyhedrons));

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

        /// <summary>
        /// Tests splitting an ellipsoid mesh by a horizontal plane through its center.
        /// <para>Verifies that both above and below meshes are produced, each side is non-empty,
        /// and the total number of triangles increases due to splitting of crossing triangles.</para>
        /// </summary>
        [Fact]
        public void Split_Mesh3D_Plane_Basic()
        {
            Ellipsoid ellipsoid = new(new Point3D(0, 0, 0), 2, 2, 2);

            Mesh3D? mesh3D = Create.Mesh3D(ellipsoid, 8, 12);
            Assert.NotNull(mesh3D);

            Plane plane = Spatial.Constants.Plane.WorldZ;

            Assert.True(Query.TrySplit(plane, mesh3D, out List<Mesh3D>? mesh3Ds_Above, out List<Mesh3D>? mesh3Ds_Below));

            Assert.NotNull(mesh3Ds_Above);
            Assert.NotEmpty(mesh3Ds_Above);
            Assert.NotNull(mesh3Ds_Below);
            Assert.NotEmpty(mesh3Ds_Below);

            int totalTriangles = 0;
            foreach (Mesh3D mesh in mesh3Ds_Above)
            {
                Assert.True(mesh.TrianglesCount > 0);
                totalTriangles += mesh.TrianglesCount;
            }
            foreach (Mesh3D mesh in mesh3Ds_Below)
            {
                Assert.True(mesh.TrianglesCount > 0);
                totalTriangles += mesh.TrianglesCount;
            }

            Assert.True(totalTriangles >= mesh3D.TrianglesCount);

            // Test convenience overload
            Assert.True(Query.TrySplit(plane, mesh3D, out List<Mesh3D>? result));
            Assert.NotNull(result);
            Assert.Equal(mesh3Ds_Above.Count + mesh3Ds_Below.Count, result.Count);
        }

        /// <summary>
        /// Tests splitting a mesh that is entirely above the plane.
        /// <para>All triangles go to the above list; below is empty.</para>
        /// </summary>
        [Fact]
        public void Split_Mesh3D_Plane_FullyAbove()
        {
            Ellipsoid ellipsoid = new(new Point3D(0, 0, 10), 2, 2, 2);

            Mesh3D? mesh3D = Create.Mesh3D(ellipsoid, 8, 12);
            Assert.NotNull(mesh3D);

            Plane plane = Spatial.Constants.Plane.WorldZ;

            Assert.True(Query.TrySplit(plane, mesh3D, out List<Mesh3D>? mesh3Ds_Above, out List<Mesh3D>? mesh3Ds_Below));

            Assert.NotNull(mesh3Ds_Above);
            Assert.NotEmpty(mesh3Ds_Above);
            Assert.NotNull(mesh3Ds_Below);
            Assert.Empty(mesh3Ds_Below);
        }

        /// <summary>
        /// Tests splitting a mesh that is entirely below the plane.
        /// <para>All triangles go to the below list; above is empty.</para>
        /// </summary>
        [Fact]
        public void Split_Mesh3D_Plane_FullyBelow()
        {
            Ellipsoid ellipsoid = new(new Point3D(0, 0, -10), 2, 2, 2);

            Mesh3D? mesh3D = Create.Mesh3D(ellipsoid, 8, 12);
            Assert.NotNull(mesh3D);

            Plane plane = Spatial.Constants.Plane.WorldZ;

            Assert.True(Query.TrySplit(plane, mesh3D, out List<Mesh3D>? mesh3Ds_Above, out List<Mesh3D>? mesh3Ds_Below));

            Assert.NotNull(mesh3Ds_Above);
            Assert.Empty(mesh3Ds_Above);
            Assert.NotNull(mesh3Ds_Below);
            Assert.NotEmpty(mesh3Ds_Below);
        }

        /// <summary>
        /// Tests splitting with null and invalid inputs.
        /// </summary>
        [Fact]
        public void Split_Mesh3D_Plane_NullInputs()
        {
            Ellipsoid ellipsoid = new(new Point3D(0, 0, 0), 2, 2, 2);

            Mesh3D? mesh3D = Create.Mesh3D(ellipsoid, 8, 12);
            Assert.NotNull(mesh3D);

            Plane? plane = Spatial.Constants.Plane.WorldZ;

            Assert.False(Query.TrySplit(null as Plane, mesh3D, out List<Mesh3D>? mesh3Ds_Above_Null, out List<Mesh3D>? mesh3Ds_Below_Null));
            Assert.Null(mesh3Ds_Above_Null);
            Assert.Null(mesh3Ds_Below_Null);

            Assert.False(Query.TrySplit(plane, null as Mesh3D, out List<Mesh3D>? mesh3Ds_Above_Null2, out List<Mesh3D>? mesh3Ds_Below_Null2));
            Assert.Null(mesh3Ds_Above_Null2);
            Assert.Null(mesh3Ds_Below_Null2);

            Assert.False(Query.TrySplit(null as Plane, null as Mesh3D, out List<Mesh3D>? mesh3Ds_Above_Null3, out List<Mesh3D>? mesh3Ds_Below_Null3));
            Assert.Null(mesh3Ds_Above_Null3);
            Assert.Null(mesh3Ds_Below_Null3);
        }

        /// <summary>
        /// Tests splitting a coplanar mesh where all triangles lie on the plane.
        /// <para>The split returns false since no triangles are on either side.</para>
        /// </summary>
        [Fact]
        public void Split_Mesh3D_Plane_Coplanar()
        {
            Point3D point3D_1 = new(0, 0, 0);
            Point3D point3D_2 = new(10, 0, 0);
            Point3D point3D_3 = new(0, 10, 0);

            Mesh3D mesh3D = new(new List<Point3D> { point3D_1, point3D_2, point3D_3 }, new List<int[]> { new int[] { 0, 1, 2 } });

            Plane plane = Spatial.Constants.Plane.WorldZ;

            Assert.False(Query.TrySplit(plane, mesh3D, out List<Mesh3D>? mesh3Ds_Above, out List<Mesh3D>? mesh3Ds_Below));
            Assert.Null(mesh3Ds_Above);
            Assert.Null(mesh3Ds_Below);
        }

        /// <summary>
        /// Tests that vertices in the above mesh are never below the plane beyond tolerance.
        /// <para>After splitting a box mesh by a planar plane, iterates all vertices in the above
        /// and below meshes to verify correct classification.</para>
        /// </summary>
        [Fact]
        public void Split_Mesh3D_Plane_VertexClassification()
        {
            BoundingBox3D boundingBox3D = new(new Point3D(0, 0, 0), new Point3D(6, 6, 6));
            Polyhedron? polyhedron = Create.Polyhedron(boundingBox3D);
            Assert.NotNull(polyhedron);

            Mesh3D? mesh3D = Create.Mesh3D(polyhedron);
            Assert.NotNull(mesh3D);

            double tolerance = DiGi.Core.Constants.Tolerance.Distance;
            Plane plane = new(new Point3D(0, 0, 3), Spatial.Constants.Vector3D.WorldZ);

            Assert.True(Query.TrySplit(plane, mesh3D, out List<Mesh3D>? mesh3Ds_Above, out List<Mesh3D>? mesh3Ds_Below, tolerance));

            Assert.NotNull(mesh3Ds_Above);
            Assert.NotEmpty(mesh3Ds_Above);

            Spatial.Classes.Vector3D? normal = plane.Normal;
            Spatial.Classes.Point3D? origin = plane.Origin;
            Assert.NotNull(normal);
            Assert.NotNull(origin);

            double SignedDistance(Point3D point3D)
            {
                return normal.X * (point3D.X - origin.X) + normal.Y * (point3D.Y - origin.Y) + normal.Z * (point3D.Z - origin.Z);
            }

            foreach (Mesh3D mesh in mesh3Ds_Above)
            {
                List<Point3D>? point3Ds = mesh.GetPoints();
                Assert.NotNull(point3Ds);
                foreach (Point3D point3D in point3Ds)
                {
                    Assert.True(SignedDistance(point3D) >= -tolerance);
                }
            }

            Assert.NotNull(mesh3Ds_Below);
            Assert.NotEmpty(mesh3Ds_Below);

            foreach (Mesh3D mesh in mesh3Ds_Below)
            {
                List<Point3D>? point3Ds = mesh.GetPoints();
                Assert.NotNull(point3Ds);
                foreach (Point3D point3D in point3Ds)
                {
                    Assert.True(SignedDistance(point3D) <= tolerance);
                }
            }
        }

        /// <summary>
        /// Tests splitting a mesh that yields multiple disconnected components per side.
        /// <para>Two separated boxes straddle the plane at different XY locations, producing
        /// at least two connected components on each side.</para>
        /// </summary>
        [Fact]
        public void Split_Mesh3D_Plane_DisconnectedComponents()
        {
            BoundingBox3D box_1 = new(new Point3D(-10, -10, -10), new Point3D(-2, -2, 10));
            BoundingBox3D box_2 = new(new Point3D(2, 2, -10), new Point3D(10, 10, 10));

            Polyhedron? polyhedron_1 = Create.Polyhedron(box_1);
            Assert.NotNull(polyhedron_1);
            Polyhedron? polyhedron_2 = Create.Polyhedron(box_2);
            Assert.NotNull(polyhedron_2);

            Mesh3D? mesh3D_1 = Create.Mesh3D(polyhedron_1);
            Assert.NotNull(mesh3D_1);
            Mesh3D? mesh3D_2 = Create.Mesh3D(polyhedron_2);
            Assert.NotNull(mesh3D_2);

            List<Triangle3D>? triangle3Ds_1 = mesh3D_1.GetTriangles();
            List<Triangle3D>? triangle3Ds_2 = mesh3D_2.GetTriangles();
            Assert.NotNull(triangle3Ds_1);
            Assert.NotNull(triangle3Ds_2);

            List<Triangle3D> allTriangles = [.. triangle3Ds_1, .. triangle3Ds_2];
            Mesh3D? combinedMesh = Create.Mesh3D(allTriangles);
            Assert.NotNull(combinedMesh);

            Plane plane = Spatial.Constants.Plane.WorldZ;

            Assert.True(Query.TrySplit(plane, combinedMesh, out List<Mesh3D>? mesh3Ds_Above, out List<Mesh3D>? mesh3Ds_Below));

            Assert.NotNull(mesh3Ds_Above);
            Assert.True(mesh3Ds_Above.Count >= 2);
            Assert.NotNull(mesh3Ds_Below);
            Assert.True(mesh3Ds_Below.Count >= 2);
        }

        /// <summary>
        /// Tests that splitting a single triangle crossing the plane produces the correct number of sub-triangles.
        /// <para>A triangle with one vertex above, one below, and one on the plane should produce one triangle above and one below.</para>
        /// </summary>
        [Fact]
        public void Split_Mesh3D_Plane_SingleTriangle()
        {
            Point3D point3D_Above = new(0, 0, 5);
            Point3D point3D_Below = new(0, 0, -5);
            Point3D point3D_On = new(10, 0, 0);

            Mesh3D mesh3D = new(new List<Point3D> { point3D_Above, point3D_Below, point3D_On }, new List<int[]> { new int[] { 0, 1, 2 } });

            Plane plane = Spatial.Constants.Plane.WorldZ;

            Assert.True(Query.TrySplit(plane, mesh3D, out List<Mesh3D>? mesh3Ds_Above, out List<Mesh3D>? mesh3Ds_Below));

            Assert.NotNull(mesh3Ds_Above);
            Assert.Single(mesh3Ds_Above);
            Assert.True(mesh3Ds_Above[0].TrianglesCount >= 1);
            Assert.NotNull(mesh3Ds_Below);
            Assert.Single(mesh3Ds_Below);
            Assert.True(mesh3Ds_Below[0].TrianglesCount >= 1);
        }

        /// <summary>
        /// Tests the decomposition of a mesh into its connected components.
        /// <para>Verifies that a connected sphere yields one component, a mesh with two disjoint triangles yields two,
        /// and null/empty inputs return null.</para>
        /// </summary>
        [Fact]
        public void Mesh3Ds_ConnectedComponents()
        {
            Ellipsoid ellipsoid = new(new Point3D(0, 0, 0), 2, 2, 2);

            Mesh3D? mesh3D_Sphere = Create.Mesh3D(ellipsoid, 8, 12);
            Assert.NotNull(mesh3D_Sphere);

            List<Mesh3D>? components_Sphere = Create.Mesh3Ds(mesh3D_Sphere);
            Assert.NotNull(components_Sphere);
            Assert.Single(components_Sphere);

            Point3D point3D_A1 = new(0, 0, 0);
            Point3D point3D_A2 = new(1, 0, 0);
            Point3D point3D_A3 = new(0, 1, 0);
            Point3D point3D_B1 = new(10, 10, 0);
            Point3D point3D_B2 = new(11, 10, 0);
            Point3D point3D_B3 = new(10, 11, 0);

            Mesh3D mesh3D_Disjoint = new(new List<Point3D> { point3D_A1, point3D_A2, point3D_A3, point3D_B1, point3D_B2, point3D_B3 }, new List<int[]> { new int[] { 0, 1, 2 }, new int[] { 3, 4, 5 } });

            List<Mesh3D>? components_Disjoint = Create.Mesh3Ds(mesh3D_Disjoint);
            Assert.NotNull(components_Disjoint);
            Assert.Equal(2, components_Disjoint.Count);

            Assert.Null(Create.Mesh3Ds(null as Mesh3D));

            Mesh3D mesh3D_Empty = new(new List<Point3D>(), new List<int[]>());
            Assert.Null(Create.Mesh3Ds(mesh3D_Empty));
        }

        /// <summary>
        /// Tests the performance of splitting a large mesh by a plane.
        /// <para>A sphere mesh with approximately 40 thousand triangles is split and the operation must complete within the stated threshold.</para>
        /// </summary>
        [Fact]
        public void Split_Mesh3D_Plane_Performance()
        {
            Ellipsoid ellipsoid = new(new Point3D(1, 2, 3), 3, 2, 1);
            Plane plane = new(new Point3D(0, 0, 3), Spatial.Constants.Vector3D.WorldX);

            // Warm-up
            Mesh3D? mesh3D_WarmUp = Create.Mesh3D(ellipsoid, 8, 12);
            Assert.NotNull(mesh3D_WarmUp);
            Assert.True(Query.TrySplit(plane, mesh3D_WarmUp, out List<Mesh3D>? _, out List<Mesh3D>? _));

            Mesh3D? mesh3D = Create.Mesh3D(ellipsoid, 100, 200);
            Assert.NotNull(mesh3D);

            Stopwatch stopwatch = Stopwatch.StartNew();
            bool result = Query.TrySplit(plane, mesh3D, out List<Mesh3D>? mesh3Ds_Above, out List<Mesh3D>? mesh3Ds_Below);
            stopwatch.Stop();

            Assert.True(result);
            Assert.NotNull(mesh3Ds_Above);
            Assert.NotNull(mesh3Ds_Below);

            Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Mesh3D split took {stopwatch.ElapsedMilliseconds} ms, expected less than 1000 ms.");
        }
    }
}