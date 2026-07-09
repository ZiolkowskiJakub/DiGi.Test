using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;
using System.Reflection;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests collinear segment-segment intersection producing a partial overlap segment.
        /// </summary>
        [Fact]
        public void Segment_Segment_Collinear_PartialOverlap()
        {
            Segment2D segment2D_1 = new(new Point2D(0, 0), new Point2D(4, 0));
            Segment2D segment2D_2 = new(new Point2D(2, 0), new Point2D(6, 0));

            IntersectionResult2D? intersectionResult2D = segment2D_1.IntersectionResult2D(segment2D_2);
            Assert.NotNull(intersectionResult2D);
            Assert.True(intersectionResult2D.Any());
            Assert.True(intersectionResult2D.Contains<Segment2D>());

            Segment2D? segment2D_Overlap = intersectionResult2D.GetGeometry2Ds<Segment2D>()?[0];
            Assert.NotNull(segment2D_Overlap);
            Assert.True(segment2D_Overlap.Similar(new Segment2D(new Point2D(2, 0), new Point2D(4, 0))));
        }

        /// <summary>
        /// Tests collinear segment-segment intersection where one segment fully contains the other.
        /// </summary>
        [Fact]
        public void Segment_Segment_Collinear_Contained()
        {
            Segment2D segment2D_Outer = new(new Point2D(0, 0), new Point2D(10, 0));
            Segment2D segment2D_Inner = new(new Point2D(3, 0), new Point2D(7, 0));

            IntersectionResult2D? intersectionResult2D = segment2D_Outer.IntersectionResult2D(segment2D_Inner);
            Assert.NotNull(intersectionResult2D);
            Assert.True(intersectionResult2D.Any());
            Assert.True(intersectionResult2D.Contains<Segment2D>());

            Segment2D? segment2D_Result = intersectionResult2D.GetGeometry2Ds<Segment2D>()?[0];
            Assert.NotNull(segment2D_Result);
            Assert.True(segment2D_Result.Similar(segment2D_Inner));
        }

        /// <summary>
        /// Tests collinear but non-overlapping (disjoint) segments, which should be rejected by the bounding box check.
        /// </summary>
        [Fact]
        public void Segment_Segment_Collinear_Disjoint()
        {
            Segment2D segment2D_1 = new(new Point2D(0, 0), new Point2D(1, 0));
            Segment2D segment2D_2 = new(new Point2D(3, 0), new Point2D(4, 0));

            IntersectionResult2D? intersectionResult2D = segment2D_1.IntersectionResult2D(segment2D_2);
            Assert.NotNull(intersectionResult2D);
            Assert.False(intersectionResult2D.Any());
        }

        /// <summary>
        /// Tests collinear segments that touch only at a shared endpoint, producing a single point.
        /// </summary>
        [Fact]
        public void Segment_Segment_Collinear_TouchingEndpoint()
        {
            Segment2D segment2D_1 = new(new Point2D(0, 0), new Point2D(2, 0));
            Segment2D segment2D_2 = new(new Point2D(2, 0), new Point2D(4, 0));

            IntersectionResult2D? intersectionResult2D = segment2D_1.IntersectionResult2D(segment2D_2);
            Assert.NotNull(intersectionResult2D);
            Assert.True(intersectionResult2D.Any());
            Assert.True(intersectionResult2D.Contains<Point2D>());

            Point2D? point2D = intersectionResult2D.GetGeometry2Ds<Point2D>()?[0];
            Assert.NotNull(point2D);
            Assert.Equal(2, point2D.X, 6);
            Assert.Equal(0, point2D.Y, 6);
        }

        /// <summary>
        /// Tests two segments that share a start vertex forming a V, producing that shared point.
        /// </summary>
        [Fact]
        public void Segment_Segment_SharedVertex()
        {
            Segment2D segment2D_1 = new(new Point2D(0, 0), new Point2D(2, 2));
            Segment2D segment2D_2 = new(new Point2D(0, 0), new Point2D(2, -2));

            IntersectionResult2D? intersectionResult2D = segment2D_1.IntersectionResult2D(segment2D_2);
            Assert.NotNull(intersectionResult2D);
            Assert.True(intersectionResult2D.Any());
            Assert.True(intersectionResult2D.Contains<Point2D>());

            Point2D? point2D = intersectionResult2D.GetGeometry2Ds<Point2D>()?[0];
            Assert.NotNull(point2D);
            Assert.Equal(0, point2D.X, 6);
            Assert.Equal(0, point2D.Y, 6);
        }

        /// <summary>
        /// Tests two identical segments, which should report the whole segment as the intersection.
        /// </summary>
        [Fact]
        public void Segment_Segment_Identical()
        {
            Segment2D segment2D_1 = new(new Point2D(0, 0), new Point2D(5, 5));
            Segment2D segment2D_2 = new(new Point2D(0, 0), new Point2D(5, 5));

            IntersectionResult2D? intersectionResult2D = segment2D_1.IntersectionResult2D(segment2D_2);
            Assert.NotNull(intersectionResult2D);
            Assert.True(intersectionResult2D.Any());
            Assert.True(intersectionResult2D.Contains<Segment2D>());

            Segment2D? segment2D_Result = intersectionResult2D.GetGeometry2Ds<Segment2D>()?[0];
            Assert.NotNull(segment2D_Result);
            Assert.True(segment2D_Result.Similar(segment2D_1));
        }

        /// <summary>
        /// Tests that segments whose bounding boxes are far apart are rejected and report no intersection.
        /// </summary>
        [Fact]
        public void Segment_Segment_BoundingBoxReject()
        {
            Segment2D segment2D_1 = new(new Point2D(0, 0), new Point2D(1, 1));
            Segment2D segment2D_2 = new(new Point2D(100, 100), new Point2D(101, 101));

            IntersectionResult2D? intersectionResult2D = segment2D_1.IntersectionResult2D(segment2D_2);
            Assert.NotNull(intersectionResult2D);
            Assert.False(intersectionResult2D.Any());
        }

        /// <summary>
        /// Tests that a single endpoint touching the interior of the other segment (with no other coincidence) hits the degenerate fall-through and returns null.
        /// <para>This documents and guards the long-standing behaviour preserved through the optimization refactor.</para>
        /// </summary>
        [Fact]
        public void Segment_Segment_EndpointTouchInterior_ReturnsNull()
        {
            Segment2D segment2D_Horizontal = new(new Point2D(0, 0), new Point2D(4, 0));
            Segment2D segment2D_Vertical = new(new Point2D(2, 0), new Point2D(2, 3));

            IntersectionResult2D? intersectionResult2D = segment2D_Horizontal.IntersectionResult2D(segment2D_Vertical);
            Assert.Null(intersectionResult2D);
        }

        /// <summary>
        /// Tests two properly crossing segments, producing the crossing point.
        /// </summary>
        [Fact]
        public void Segment_Segment_Crossing()
        {
            Segment2D segment2D_Horizontal = new(new Point2D(0, 0), new Point2D(4, 0));
            Segment2D segment2D_Vertical = new(new Point2D(2, -1), new Point2D(2, 3));

            IntersectionResult2D? intersectionResult2D = segment2D_Horizontal.IntersectionResult2D(segment2D_Vertical);
            Assert.NotNull(intersectionResult2D);
            Assert.True(intersectionResult2D.Any());
            Assert.True(intersectionResult2D.Contains<Point2D>());

            Point2D? point2D = intersectionResult2D.GetGeometry2Ds<Point2D>()?[0];
            Assert.NotNull(point2D);
            Assert.Equal(2, point2D.X, 6);
            Assert.Equal(0, point2D.Y, 6);
        }

        /// <summary>
        /// Tests segment-segment intersection at the distance tolerance boundary, asserting behaviour just inside and just outside the tolerance.
        /// </summary>
        [Fact]
        public void Segment_Segment_Tolerance_Boundary()
        {
            double tolerance = DiGi.Core.Constants.Tolerance.Distance;

            Segment2D segment2D_Base = new(new Point2D(0, 0), new Point2D(2, 0));

            // Collinear neighbour separated by half the tolerance -> treated as coincident -> intersects.
            Segment2D segment2D_Inside = new(new Point2D(2 + (tolerance * 0.5), 0), new Point2D(5, 0));
            IntersectionResult2D? intersectionResult2D_Inside = segment2D_Base.IntersectionResult2D(segment2D_Inside, tolerance);
            Assert.NotNull(intersectionResult2D_Inside);
            Assert.True(intersectionResult2D_Inside.Any());

            // Collinear neighbour separated by twice the tolerance -> out of range -> no intersection.
            Segment2D segment2D_Outside = new(new Point2D(2 + (tolerance * 2.0), 0), new Point2D(5, 0));
            IntersectionResult2D? intersectionResult2D_Outside = segment2D_Base.IntersectionResult2D(segment2D_Outside, tolerance);
            Assert.NotNull(intersectionResult2D_Outside);
            Assert.False(intersectionResult2D_Outside.Any());
        }

        /// <summary>
        /// Tests line-line intersection for crossing, parallel, and coincident configurations.
        /// </summary>
        [Fact]
        public void Line_Line_Configurations()
        {
            // Crossing
            Line2D line2D_1 = new(new Point2D(0, 0), new Vector2D(1, 0));
            Line2D line2D_2 = new(new Point2D(2, -3), new Vector2D(0, 1));
            IntersectionResult2D? intersectionResult2D_Cross = line2D_1.IntersectionResult2D(line2D_2);
            Assert.NotNull(intersectionResult2D_Cross);
            Assert.True(intersectionResult2D_Cross.Any());
            Point2D? point2D = intersectionResult2D_Cross.GetGeometry2Ds<Point2D>()?[0];
            Assert.NotNull(point2D);
            Assert.Equal(2, point2D.X, 6);
            Assert.Equal(0, point2D.Y, 6);

            // Parallel, distinct
            Line2D line2D_Parallel = new(new Point2D(0, 5), new Vector2D(1, 0));
            IntersectionResult2D? intersectionResult2D_Parallel = line2D_1.IntersectionResult2D(line2D_Parallel);
            Assert.NotNull(intersectionResult2D_Parallel);
            Assert.False(intersectionResult2D_Parallel.Any());

            // Coincident -> the shared line is reported
            Line2D line2D_Coincident = new(new Point2D(5, 0), new Vector2D(1, 0));
            IntersectionResult2D? intersectionResult2D_Coincident = line2D_1.IntersectionResult2D(line2D_Coincident);
            Assert.NotNull(intersectionResult2D_Coincident);
            Assert.True(intersectionResult2D_Coincident.Any());
            Assert.True(intersectionResult2D_Coincident.Contains<Line2D>());
        }

        /// <summary>
        /// Tests line-segment intersection where the segment lies on the line, crosses the line, and misses the line.
        /// </summary>
        [Fact]
        public void Line_Segment_Configurations()
        {
            Line2D line2D = new(new Point2D(0, 0), new Vector2D(1, 0));

            // Segment fully on the line -> segment result
            Segment2D segment2D_OnLine = new(new Point2D(1, 0), new Point2D(3, 0));
            IntersectionResult2D? intersectionResult2D_OnLine = line2D.IntersectionResult2D(segment2D_OnLine);
            Assert.NotNull(intersectionResult2D_OnLine);
            Assert.True(intersectionResult2D_OnLine.Any());
            Assert.True(intersectionResult2D_OnLine.Contains<Segment2D>());

            // Segment crossing the line -> point result
            Segment2D segment2D_Crossing = new(new Point2D(2, -1), new Point2D(2, 1));
            IntersectionResult2D? intersectionResult2D_Crossing = line2D.IntersectionResult2D(segment2D_Crossing);
            Assert.NotNull(intersectionResult2D_Crossing);
            Assert.True(intersectionResult2D_Crossing.Any());
            Point2D? point2D = intersectionResult2D_Crossing.GetGeometry2Ds<Point2D>()?[0];
            Assert.NotNull(point2D);
            Assert.Equal(2, point2D.X, 6);
            Assert.Equal(0, point2D.Y, 6);

            // Segment entirely above the line -> no intersection
            Segment2D segment2D_Miss = new(new Point2D(2, 1), new Point2D(2, 3));
            IntersectionResult2D? intersectionResult2D_Miss = line2D.IntersectionResult2D(segment2D_Miss);
            Assert.NotNull(intersectionResult2D_Miss);
            Assert.False(intersectionResult2D_Miss.Any());
        }

        /// <summary>
        /// Tests that a simple convex closed polygon reports no self-intersection, validating the adjacent and closing-edge exclusion logic.
        /// </summary>
        [Fact]
        public void SelfIntersection_ConvexPolygon_None()
        {
            Polygon2D polygon2D = new([new Point2D(0, 0), new Point2D(4, 0), new Point2D(4, 4), new Point2D(0, 4)]);

            IntersectionResult2D? intersectionResult2D = polygon2D.IntersectionResult2D();
            Assert.NotNull(intersectionResult2D);
            Assert.False(intersectionResult2D.Any());
        }

        /// <summary>
        /// Tests that a bowtie (self-crossing quadrilateral) reports exactly one self-intersection at its crossing point.
        /// </summary>
        [Fact]
        public void SelfIntersection_Bowtie_SinglePoint()
        {
            // Explicit closing point so the segment structure is deterministic (4 segments).
            Polyline2D polyline2D = new([new Point2D(0, 0), new Point2D(2, 2), new Point2D(2, 0), new Point2D(0, 2), new Point2D(0, 0)]);

            IntersectionResult2D? intersectionResult2D = polyline2D.IntersectionResult2D();
            Assert.NotNull(intersectionResult2D);
            Assert.True(intersectionResult2D.Any());
            Assert.Equal(1, intersectionResult2D.Count);

            Point2D? point2D = intersectionResult2D.GetGeometry2Ds<Point2D>()?[0];
            Assert.NotNull(point2D);
            Assert.Equal(1, point2D.X, 6);
            Assert.Equal(1, point2D.Y, 6);
        }

        /// <summary>
        /// Tests polygonal-face versus line intersection for a line crossing the face and a line entirely outside the face.
        /// </summary>
        [Fact]
        public void PolygonalFace_Line_ThroughAndOutside()
        {
            PolygonalFace2D? polygonalFace2D = Create.PolygonalFace2D(new Point2D(-2, -2), new Point2D(-2, 2), new Point2D(2, 2), new Point2D(2, -2));
            Assert.NotNull(polygonalFace2D);

            // Vertical line through the face -> segment from (0,-2) to (0,2)
            Line2D line2D_Through = new(new Point2D(0, -10), new Vector2D(0, 1));
            IntersectionResult2D? intersectionResult2D_Through = polygonalFace2D.IntersectionResult2D(line2D_Through);
            Assert.NotNull(intersectionResult2D_Through);
            Assert.True(intersectionResult2D_Through.Any());
            Assert.True(intersectionResult2D_Through.Contains<Segment2D>());

            // Horizontal line well above the face -> no intersection
            Line2D line2D_Outside = new(new Point2D(0, 10), new Vector2D(1, 0));
            IntersectionResult2D? intersectionResult2D_Outside = polygonalFace2D.IntersectionResult2D(line2D_Outside);
            Assert.NotNull(intersectionResult2D_Outside);
            Assert.False(intersectionResult2D_Outside.Any());
        }

        /// <summary>
        /// Tests polygonal-face versus polygonal-face intersection for overlapping and disjoint faces.
        /// </summary>
        [Fact]
        public void PolygonalFace_PolygonalFace_OverlapAndDisjoint()
        {
            PolygonalFace2D? polygonalFace2D_1 = Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(4, 0), new Point2D(4, 4), new Point2D(0, 4));
            PolygonalFace2D? polygonalFace2D_2 = Create.PolygonalFace2D(new Point2D(2, 2), new Point2D(6, 2), new Point2D(6, 6), new Point2D(2, 6));
            PolygonalFace2D? polygonalFace2D_Far = Create.PolygonalFace2D(new Point2D(10, 10), new Point2D(14, 10), new Point2D(14, 14), new Point2D(10, 14));
            Assert.NotNull(polygonalFace2D_1);
            Assert.NotNull(polygonalFace2D_2);
            Assert.NotNull(polygonalFace2D_Far);

            IntersectionResult2D? intersectionResult2D_Overlap = polygonalFace2D_1.IntersectionResult2D(polygonalFace2D_2);
            Assert.NotNull(intersectionResult2D_Overlap);
            Assert.True(intersectionResult2D_Overlap.Any());
            Assert.True(intersectionResult2D_Overlap.Contains<PolygonalFace2D>());

            IntersectionResult2D? intersectionResult2D_Disjoint = polygonalFace2D_1.IntersectionResult2D(polygonalFace2D_Far);
            Assert.NotNull(intersectionResult2D_Disjoint);
            Assert.False(intersectionResult2D_Disjoint.Any());
        }

        /// <summary>
        /// Tests that the IntersectionResult2D class round-trips through JSON serialization and cloning.
        /// </summary>
        [Fact]
        public void IntersectionResult2D_Serialization()
        {
            Segment2D segment2D_1 = new(new Point2D(-2, 0), new Point2D(2, 0));
            Segment2D segment2D_2 = new(new Point2D(0, -2), new Point2D(0, 2));

            IntersectionResult2D? intersectionResult2D = segment2D_1.IntersectionResult2D(segment2D_2);
            Assert.NotNull(intersectionResult2D);
            Assert.True(intersectionResult2D.Any());

            DiGi.Core.xUnit.Query.SerializationCheck(intersectionResult2D);
        }

        /// <summary>
        /// Tests the dense-star sample loaded from the shared fixture, validating the fixture round-trips and reproduces the expected self-intersection count within a coarse time budget.
        /// </summary>
        [Fact]
        public void SelfIntersection_DenseStar_Fixture()
        {
            string? filePath = DiGi.Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "Segment2Ds_DenseStar.json");
            Assert.False(string.IsNullOrWhiteSpace(filePath));

            List<Segment2D>? segment2Ds = DiGi.Core.Convert.ToDiGi<Segment2D>((DiGi.Core.Classes.Path)filePath!);
            Assert.NotNull(segment2Ds);
            Assert.Equal(399, segment2Ds.Count);

            // Rebuild the polyline from the loaded, ordered segments.
            List<Point2D> point2Ds = [];
            Point2D? point2D_Start = segment2Ds[0].Start;
            Assert.NotNull(point2D_Start);
            point2Ds.Add(point2D_Start);
            for (int i = 0; i < segment2Ds.Count; i++)
            {
                Point2D? point2D_End = segment2Ds[i].End;
                Assert.NotNull(point2D_End);
                point2Ds.Add(point2D_End);
            }

            Polyline2D polyline2D = new(point2Ds);

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            IntersectionResult2D? intersectionResult2D = polyline2D.IntersectionResult2D();
            stopwatch.Stop();

            Assert.NotNull(intersectionResult2D);
            Assert.True(intersectionResult2D.Any());
            Assert.Equal(68456, intersectionResult2D.Count);
            Assert.True(stopwatch.ElapsedMilliseconds < 10000, $"Dense-star self-intersection took {stopwatch.ElapsedMilliseconds} ms (possible O(k^2) dedup regression).");
        }

        /// <summary>
        /// Validates, over many random segment pairs, that any returned intersection point actually lies on both source segments.
        /// </summary>
        [Fact]
        public void Segment_Segment_Random_PointLiesOnBothSegments()
        {
            double tolerance = DiGi.Core.Constants.Tolerance.Distance;
            BoundingBox2D boundingBox2D = new(new Point2D(0, 0), new Point2D(20, 20));

            for (int seed = 0; seed < 300; seed++)
            {
                System.Random random = DiGi.Core.Create.Random(seed);
                Segment2D? segment2D_1 = DiGi.Geometry.Planar.Random.Create.Segment2D(boundingBox2D, random);
                Segment2D? segment2D_2 = DiGi.Geometry.Planar.Random.Create.Segment2D(boundingBox2D, random);
                if (segment2D_1 == null || segment2D_2 == null)
                {
                    continue;
                }

                IntersectionResult2D? intersectionResult2D = segment2D_1.IntersectionResult2D(segment2D_2, tolerance);
                if (intersectionResult2D == null || !intersectionResult2D.Any())
                {
                    continue;
                }

                List<Point2D>? point2Ds = intersectionResult2D.GetGeometry2Ds<Point2D>();
                if (point2Ds == null)
                {
                    continue;
                }

                for (int i = 0; i < point2Ds.Count; i++)
                {
                    Assert.True(segment2D_1.On(point2Ds[i], tolerance), $"Seed {seed}: intersection point not on the first segment.");
                    Assert.True(segment2D_2.On(point2Ds[i], tolerance), $"Seed {seed}: intersection point not on the second segment.");
                }
            }
        }

        /// <summary>
        /// Validates, over many random segment pairs, that the intersection is commutative (A vs B agrees with B vs A).
        /// </summary>
        [Fact]
        public void Segment_Segment_Random_Commutative()
        {
            double tolerance = DiGi.Core.Constants.Tolerance.Distance;
            BoundingBox2D boundingBox2D = new(new Point2D(0, 0), new Point2D(20, 20));

            for (int seed = 0; seed < 300; seed++)
            {
                System.Random random = DiGi.Core.Create.Random(seed);
                Segment2D? segment2D_1 = DiGi.Geometry.Planar.Random.Create.Segment2D(boundingBox2D, random);
                Segment2D? segment2D_2 = DiGi.Geometry.Planar.Random.Create.Segment2D(boundingBox2D, random);
                if (segment2D_1 == null || segment2D_2 == null)
                {
                    continue;
                }

                IntersectionResult2D? intersectionResult2D_Forward = segment2D_1.IntersectionResult2D(segment2D_2, tolerance);
                IntersectionResult2D? intersectionResult2D_Reverse = segment2D_2.IntersectionResult2D(segment2D_1, tolerance);

                Assert.Equal(intersectionResult2D_Forward == null, intersectionResult2D_Reverse == null);
                if (intersectionResult2D_Forward == null || intersectionResult2D_Reverse == null)
                {
                    continue;
                }

                Assert.Equal(intersectionResult2D_Forward.Any(), intersectionResult2D_Reverse.Any());
                if (!intersectionResult2D_Forward.Any())
                {
                    continue;
                }

                Point2D? point2D_Forward = intersectionResult2D_Forward.GetGeometry2Ds<Point2D>()?[0];
                Point2D? point2D_Reverse = intersectionResult2D_Reverse.GetGeometry2Ds<Point2D>()?[0];
                if (point2D_Forward != null || point2D_Reverse != null)
                {
                    Assert.NotNull(point2D_Forward);
                    Assert.NotNull(point2D_Reverse);
                    Assert.True(point2D_Forward.Similar(point2D_Reverse, tolerance), $"Seed {seed}: point results differ between orderings.");
                    continue;
                }

                Segment2D? segment2D_Forward = intersectionResult2D_Forward.GetGeometry2Ds<Segment2D>()?[0];
                Segment2D? segment2D_Reverse = intersectionResult2D_Reverse.GetGeometry2Ds<Segment2D>()?[0];
                Assert.NotNull(segment2D_Forward);
                Assert.NotNull(segment2D_Reverse);
                Assert.True(segment2D_Forward.Similar(segment2D_Reverse, tolerance), $"Seed {seed}: segment results differ between orderings.");
            }
        }

        /// <summary>
        /// Validates that the optimized collection-versus-collection intersection (spatial-hash deduplication) matches a naive pairwise reference over random data.
        /// </summary>
        [Fact]
        public void Collection_Collection_Random_MatchesNaiveReference()
        {
            double tolerance = DiGi.Core.Constants.Tolerance.Distance;
            BoundingBox2D boundingBox2D = new(new Point2D(0, 0), new Point2D(20, 20));

            for (int seed = 0; seed < 8; seed++)
            {
                System.Random random = DiGi.Core.Create.Random(seed);

                List<Segment2D> segment2Ds_1 = [];
                List<Segment2D> segment2Ds_2 = [];
                for (int i = 0; i < 25; i++)
                {
                    Segment2D? segment2D_1 = DiGi.Geometry.Planar.Random.Create.Segment2D(boundingBox2D, random);
                    if (segment2D_1 != null)
                    {
                        segment2Ds_1.Add(segment2D_1);
                    }

                    Segment2D? segment2D_2 = DiGi.Geometry.Planar.Random.Create.Segment2D(boundingBox2D, random);
                    if (segment2D_2 != null)
                    {
                        segment2Ds_2.Add(segment2D_2);
                    }
                }

                // Naive reference: pairwise intersection with a linear-scan deduplication.
                List<Point2D> point2Ds_Reference = [];
                List<Segment2D> segment2Ds_Reference = [];
                for (int i = 0; i < segment2Ds_1.Count; i++)
                {
                    for (int j = 0; j < segment2Ds_2.Count; j++)
                    {
                        IntersectionResult2D? intersectionResult2D_Pairwise = segment2Ds_1[i].IntersectionResult2D(segment2Ds_2[j], tolerance);
                        if (intersectionResult2D_Pairwise == null || !intersectionResult2D_Pairwise.Any())
                        {
                            continue;
                        }

                        Point2D? point2D = intersectionResult2D_Pairwise.GetGeometry2Ds<Point2D>()?[0];
                        if (point2D != null)
                        {
                            AddUniqueReferencePoint(point2Ds_Reference, point2D, tolerance);
                            continue;
                        }

                        Segment2D? segment2D = intersectionResult2D_Pairwise.GetGeometry2Ds<Segment2D>()?[0];
                        if (segment2D != null)
                        {
                            AddUniqueReferenceSegment(segment2Ds_Reference, segment2D, tolerance);
                        }
                    }
                }

                IntersectionResult2D? intersectionResult2D = segment2Ds_1.IntersectionResult2D(segment2Ds_2, tolerance);
                Assert.NotNull(intersectionResult2D);

                List<Point2D> point2Ds_Optimized = intersectionResult2D.GetGeometry2Ds<Point2D>() ?? [];
                List<Segment2D> segment2Ds_Optimized = intersectionResult2D.GetGeometry2Ds<Segment2D>() ?? [];

                Assert.Equal(point2Ds_Reference.Count, point2Ds_Optimized.Count);
                Assert.Equal(segment2Ds_Reference.Count, segment2Ds_Optimized.Count);

                for (int i = 0; i < point2Ds_Optimized.Count; i++)
                {
                    Point2D point2D_Optimized = point2Ds_Optimized[i];
                    Assert.Contains(point2Ds_Reference, x => x.Similar(point2D_Optimized, tolerance));
                }

                for (int i = 0; i < segment2Ds_Optimized.Count; i++)
                {
                    Segment2D segment2D_Optimized = segment2Ds_Optimized[i];
                    Assert.Contains(segment2Ds_Reference, x => x.Similar(segment2D_Optimized, tolerance));
                }
            }
        }

        private static void AddUniqueReferencePoint(List<Point2D> point2Ds, Point2D point2D, double tolerance)
        {
            for (int i = 0; i < point2Ds.Count; i++)
            {
                if (point2Ds[i].Similar(point2D, tolerance))
                {
                    return;
                }
            }

            point2Ds.Add(point2D);
        }

        private static void AddUniqueReferenceSegment(List<Segment2D> segment2Ds, Segment2D segment2D, double tolerance)
        {
            for (int i = 0; i < segment2Ds.Count; i++)
            {
                if (segment2Ds[i].Similar(segment2D, tolerance))
                {
                    return;
                }
            }

            segment2Ds.Add(segment2D);
        }
    }
}
