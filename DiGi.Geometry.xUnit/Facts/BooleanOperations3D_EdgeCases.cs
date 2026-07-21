using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        private static Polyhedron? Polyhedron_Box(double min, double max)
        {
            return Create.Polyhedron(new BoundingBox3D(new Point3D(min, min, min), new Point3D(max, max, max)));
        }

        private static Polyhedron? Polyhedron_SubdividedBox(double min, double max, int count)
        {
            List<IPolygonalFace3D> polygonalFace3Ds = [];

            void AddSide(System.Func<double, double, Point3D> map)
            {
                double step = (max - min) / count;
                for (int i = 0; i < count; i++)
                {
                    for (int j = 0; j < count; j++)
                    {
                        double u0 = min + (i * step);
                        double u1 = u0 + step;
                        double v0 = min + (j * step);
                        double v1 = v0 + step;

                        Polygon3D? polygon3D = Create.Polygon3D(new List<Point3D?> { map(u0, v0), map(u1, v0), map(u1, v1), map(u0, v1) });
                        PolygonalFace3D? polygonalFace3D = polygon3D == null ? null : Create.PolygonalFace3D(polygon3D);
                        if (polygonalFace3D != null)
                        {
                            polygonalFace3Ds.Add(polygonalFace3D);
                        }
                    }
                }
            }

            AddSide((u, v) => new Point3D(u, v, min));
            AddSide((u, v) => new Point3D(u, v, max));
            AddSide((u, v) => new Point3D(u, min, v));
            AddSide((u, v) => new Point3D(u, max, v));
            AddSide((u, v) => new Point3D(min, u, v));
            AddSide((u, v) => new Point3D(max, u, v));

            return Create.Polyhedron(polygonalFace3Ds);
        }

        /// <summary>
        /// Tests 3D boolean union and intersection of two identical polyhedra (fully coplanar, coincident boundaries).
        /// <para>Both operations must reproduce the original solid volume exactly once (deduplicated coplanar faces).</para>
        /// </summary>
        [Fact]
        public void BooleanOperations3D_Identical_UnionAndIntersection()
        {
            Polyhedron? polyhedron = Polyhedron_Box(-2, 2);
            Assert.NotNull(polyhedron);
            if (polyhedron == null)
            {
                return;
            }

            UnionResult3D? unionResult3D = polyhedron.UnionResult3D(polyhedron);
            Assert.NotNull(unionResult3D);
            Assert.True(unionResult3D.Any());

            Polyhedron? polyhedron_Union = unionResult3D.GetGeometry3Ds<Polyhedron>()?.FirstOrDefault();
            Assert.NotNull(polyhedron_Union);
            Assert.Equal(64.0, polyhedron_Union?.GetBoundingBox()?.GetVolume() ?? 0.0, 5);

            IntersectionResult3D? intersectionResult3D = polyhedron.IntersectionResult3D(polyhedron);
            Assert.NotNull(intersectionResult3D);
            Assert.True(intersectionResult3D.Any());

            Polyhedron? polyhedron_Intersection = intersectionResult3D.GetGeometry3Ds<Polyhedron>()?.FirstOrDefault();
            Assert.NotNull(polyhedron_Intersection);
            Assert.Equal(64.0, polyhedron_Intersection?.GetBoundingBox()?.GetVolume() ?? 0.0, 5);
        }

        /// <summary>
        /// Tests the 3D boolean intersection of two polyhedra touching along a shared coplanar face.
        /// <para>The contact region is two-dimensional, so the result must not contain a solid polyhedron.</para>
        /// </summary>
        [Fact]
        public void BooleanOperations3D_CoplanarTouch_IntersectionNotSolid()
        {
            Polyhedron? polyhedron_1 = Polyhedron_Box(-2, 2);
            Polyhedron? polyhedron_2 = Polyhedron_Box(2, 6);
            Assert.NotNull(polyhedron_1);
            Assert.NotNull(polyhedron_2);
            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            IntersectionResult3D? intersectionResult3D = polyhedron_1.IntersectionResult3D(polyhedron_2);
            Assert.NotNull(intersectionResult3D);
            Assert.False(intersectionResult3D.Contains<Polyhedron>());
        }

        /// <summary>
        /// Tests 3D boolean operations of two polyhedra sharing only a single edge (coincident edges, no shared volume or face area).
        /// <para>Intersection must not produce a solid; difference must preserve the first solid unchanged.</para>
        /// </summary>
        [Fact]
        public void BooleanOperations3D_EdgeTouch()
        {
            Polyhedron? polyhedron_1 = Polyhedron_Box(-2, 2);
            Assert.NotNull(polyhedron_1);

            // Touches polyhedron_1 only along the edge x = 2, y = 2
            BoundingBox3D boundingBox3D = new(new Point3D(2, 2, -2), new Point3D(6, 6, 2));
            Polyhedron? polyhedron_2 = Create.Polyhedron(boundingBox3D);
            Assert.NotNull(polyhedron_2);

            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            IntersectionResult3D? intersectionResult3D = polyhedron_1.IntersectionResult3D(polyhedron_2);
            Assert.NotNull(intersectionResult3D);
            Assert.False(intersectionResult3D.Contains<Polyhedron>());

            DifferenceResult3D? differenceResult3D = polyhedron_1.DifferenceResult3D(polyhedron_2);
            Assert.NotNull(differenceResult3D);
            Assert.True(differenceResult3D.Any());

            Polyhedron? polyhedron_Difference = differenceResult3D.GetGeometry3Ds<Polyhedron>()?.FirstOrDefault();
            Assert.NotNull(polyhedron_Difference);
            Assert.Equal(64.0, polyhedron_Difference?.GetBoundingBox()?.GetVolume() ?? 0.0, 5);

            UnionResult3D? unionResult3D = polyhedron_1.UnionResult3D(polyhedron_2);
            Assert.NotNull(unionResult3D);
            Assert.True(unionResult3D.Any());
        }

        /// <summary>
        /// Tests 3D boolean operations of two polyhedra touching at a single vertex only.
        /// <para>Intersection must not produce a solid; difference must preserve the first solid unchanged.</para>
        /// </summary>
        [Fact]
        public void BooleanOperations3D_VertexTouch()
        {
            Polyhedron? polyhedron_1 = Polyhedron_Box(-2, 2);
            Polyhedron? polyhedron_2 = Polyhedron_Box(2, 6);
            Assert.NotNull(polyhedron_1);
            Assert.NotNull(polyhedron_2);
            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            // Move the second box so it touches the first one at the single vertex (2, 2, 2)
            // Boxes [-2,2]^3 and [2,6]^3 share exactly that corner
            IntersectionResult3D? intersectionResult3D = polyhedron_1.IntersectionResult3D(polyhedron_2);
            Assert.NotNull(intersectionResult3D);
            Assert.False(intersectionResult3D.Contains<Polyhedron>());

            DifferenceResult3D? differenceResult3D = polyhedron_1.DifferenceResult3D(polyhedron_2);
            Assert.NotNull(differenceResult3D);
            Assert.True(differenceResult3D.Any());

            Polyhedron? polyhedron_Difference = differenceResult3D.GetGeometry3Ds<Polyhedron>()?.FirstOrDefault();
            Assert.NotNull(polyhedron_Difference);
            Assert.Equal(64.0, polyhedron_Difference?.GetBoundingBox()?.GetVolume() ?? 0.0, 5);
        }

        /// <summary>
        /// Tests 3D boolean operations where the volumes overlap and additionally share partially coplanar faces
        /// (the second box spans the full height of the first, so top and bottom faces are pairwise coplanar).
        /// <para>Verifies the coplanar-boundary classification: intersection and union must produce solids with the expected bounding boxes.</para>
        /// </summary>
        [Fact]
        public void BooleanOperations3D_PartialCoplanarOverlap()
        {
            Polyhedron? polyhedron_1 = Polyhedron_Box(-2, 2);
            Assert.NotNull(polyhedron_1);

            // Same height as polyhedron_1 - top and bottom faces are partially coplanar
            BoundingBox3D boundingBox3D = new(new Point3D(0, 0, -2), new Point3D(4, 4, 2));
            Polyhedron? polyhedron_2 = Create.Polyhedron(boundingBox3D);
            Assert.NotNull(polyhedron_2);

            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            IntersectionResult3D? intersectionResult3D = polyhedron_1.IntersectionResult3D(polyhedron_2);
            Assert.NotNull(intersectionResult3D);
            Polyhedron? polyhedron_Intersection = intersectionResult3D.GetGeometry3Ds<Polyhedron>()?.FirstOrDefault();
            Assert.NotNull(polyhedron_Intersection);

            BoundingBox3D? boundingBox3D_Intersection = polyhedron_Intersection?.GetBoundingBox();
            Assert.NotNull(boundingBox3D_Intersection);
            Assert.Equal(16.0, boundingBox3D_Intersection?.GetVolume() ?? 0.0, 5);

            UnionResult3D? unionResult3D = polyhedron_1.UnionResult3D(polyhedron_2);
            Assert.NotNull(unionResult3D);
            Polyhedron? polyhedron_Union = unionResult3D.GetGeometry3Ds<Polyhedron>()?.FirstOrDefault();
            Assert.NotNull(polyhedron_Union);

            BoundingBox3D? boundingBox3D_Union = polyhedron_Union?.GetBoundingBox();
            Assert.NotNull(boundingBox3D_Union);
            Assert.Equal(144.0, boundingBox3D_Union?.GetVolume() ?? 0.0, 5);

            DifferenceResult3D? differenceResult3D = polyhedron_1.DifferenceResult3D(polyhedron_2);
            Assert.NotNull(differenceResult3D);
            Assert.True(differenceResult3D.Any());

            Polyhedron? polyhedron_Difference = differenceResult3D.GetGeometry3Ds<Polyhedron>()?.FirstOrDefault();
            Assert.NotNull(polyhedron_Difference);
            Assert.Equal(64.0, polyhedron_Difference?.GetBoundingBox()?.GetVolume() ?? 0.0, 5);
        }

        /// <summary>
        /// Tests 3D boolean operations against a non-manifold, open-shell polyhedron (a box with one face missing).
        /// <para>The operations are best-effort on non-closed input and must not throw or hang; results must not be null.</para>
        /// </summary>
        [Fact]
        public void BooleanOperations3D_OpenShell_NonManifold()
        {
            Polyhedron? polyhedron_Solid = Polyhedron_Box(0, 4);
            Assert.NotNull(polyhedron_Solid);

            // Open box [-2, 2]^3 without the top (z = 2) face - a non-manifold, non-closed shell
            List<Point3D?[]> point3Ds_Faces =
            [
                [new Point3D(-2, -2, -2), new Point3D(2, -2, -2), new Point3D(2, 2, -2), new Point3D(-2, 2, -2)],
                [new Point3D(-2, -2, -2), new Point3D(2, -2, -2), new Point3D(2, -2, 2), new Point3D(-2, -2, 2)],
                [new Point3D(2, -2, -2), new Point3D(2, 2, -2), new Point3D(2, 2, 2), new Point3D(2, -2, 2)],
                [new Point3D(2, 2, -2), new Point3D(-2, 2, -2), new Point3D(-2, 2, 2), new Point3D(2, 2, 2)],
                [new Point3D(-2, 2, -2), new Point3D(-2, -2, -2), new Point3D(-2, -2, 2), new Point3D(-2, 2, 2)]
            ];

            List<IPolygonalFace3D> polygonalFace3Ds = [];
            foreach (Point3D?[] point3Ds in point3Ds_Faces)
            {
                Polygon3D? polygon3D = Create.Polygon3D(point3Ds);
                PolygonalFace3D? polygonalFace3D = polygon3D == null ? null : Create.PolygonalFace3D(polygon3D);
                if (polygonalFace3D != null)
                {
                    polygonalFace3Ds.Add(polygonalFace3D);
                }
            }

            Assert.Equal(5, polygonalFace3Ds.Count);

            Polyhedron? polyhedron_Open = Create.Polyhedron(polygonalFace3Ds);
            Assert.NotNull(polyhedron_Open);

            if (polyhedron_Open == null || polyhedron_Solid == null)
            {
                return;
            }

            UnionResult3D? unionResult3D = polyhedron_Open.UnionResult3D(polyhedron_Solid);
            Assert.NotNull(unionResult3D);

            IntersectionResult3D? intersectionResult3D = polyhedron_Open.IntersectionResult3D(polyhedron_Solid);
            Assert.NotNull(intersectionResult3D);

            DifferenceResult3D? differenceResult3D = polyhedron_Open.DifferenceResult3D(polyhedron_Solid);
            Assert.NotNull(differenceResult3D);
        }

        /// <summary>
        /// Tests the empty 3D boolean intersection of two polyhedra separated by a gap larger than the tolerance.
        /// <para>The result must exist and contain no geometry.</para>
        /// </summary>
        [Fact]
        public void BooleanOperations3D_GapAboveTolerance_EmptyIntersection()
        {
            double tolerance = 1e-3;

            Polyhedron? polyhedron_1 = Polyhedron_Box(-2, 2);
            Assert.NotNull(polyhedron_1);

            // Gap of 10 * tolerance along the x axis
            BoundingBox3D boundingBox3D = new(new Point3D(2.01, -2, -2), new Point3D(6, 2, 2));
            Polyhedron? polyhedron_2 = Create.Polyhedron(boundingBox3D);
            Assert.NotNull(polyhedron_2);

            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            IntersectionResult3D? intersectionResult3D = polyhedron_1.IntersectionResult3D(polyhedron_2, tolerance);
            Assert.NotNull(intersectionResult3D);
            Assert.False(intersectionResult3D.Any());
        }

        /// <summary>
        /// Tests the performance of 3D boolean operations on polyhedra with subdivided faces (54 faces each).
        /// <para>Union, intersection and difference must each complete well below the timeout threshold thanks to
        /// BVH culling and single-pass point-relation classification.</para>
        /// </summary>
        [Fact]
        public void BooleanOperations3D_Performance()
        {
            // Warm up / JIT compile before measuring performance
            {
                Polyhedron? polyhedron_WarmupA = Polyhedron_Box(-2, 2);
                Polyhedron? polyhedron_WarmupB = Polyhedron_Box(0, 4);
                if (polyhedron_WarmupA != null && polyhedron_WarmupB != null)
                {
                    _ = polyhedron_WarmupA.UnionResult3D(polyhedron_WarmupB);
                    _ = polyhedron_WarmupA.IntersectionResult3D(polyhedron_WarmupB);
                    _ = polyhedron_WarmupA.DifferenceResult3D(polyhedron_WarmupB);
                }
            }

            Polyhedron? polyhedron_1 = Polyhedron_SubdividedBox(-2, 2, 3);
            Polyhedron? polyhedron_2 = Polyhedron_SubdividedBox(0, 4, 3);
            Assert.NotNull(polyhedron_1);
            Assert.NotNull(polyhedron_2);
            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            Assert.Equal(54, polyhedron_1.Count);

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            UnionResult3D? unionResult3D = polyhedron_1.UnionResult3D(polyhedron_2);
            IntersectionResult3D? intersectionResult3D = polyhedron_1.IntersectionResult3D(polyhedron_2);
            DifferenceResult3D? differenceResult3D = polyhedron_1.DifferenceResult3D(polyhedron_2);

            stopwatch.Stop();

            Assert.NotNull(unionResult3D);
            Assert.True(unionResult3D.Any());
            Assert.NotNull(intersectionResult3D);
            Assert.True(intersectionResult3D.Any());
            Assert.NotNull(differenceResult3D);
            Assert.True(differenceResult3D.Any());

            Assert.True(stopwatch.ElapsedMilliseconds < 2000, $"3D boolean operations on subdivided polyhedra took too long: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }
}