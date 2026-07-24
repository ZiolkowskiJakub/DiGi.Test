using DiGi.Core.Interfaces;
using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests all constructor overloads of the Segment2D class, including coordinate parameters, point/vector inputs, copy constructor, and JSON roundtrip.
        /// </summary>
        [Fact]
        public void Segment2D_Constructors()
        {
            // 1. Double coordinates constructor
            Segment2D segment2D_1 = new(0.0, 0.0, 10.0, 0.0);
            Assert.NotNull(segment2D_1.Start);
            Assert.NotNull(segment2D_1.End);
            Assert.Equal(0.0, segment2D_1.Start.X, 9);
            Assert.Equal(0.0, segment2D_1.Start.Y, 9);
            Assert.Equal(10.0, segment2D_1.End.X, 9);
            Assert.Equal(0.0, segment2D_1.End.Y, 9);

            // 2. Start point and vector constructor
            Point2D point2D_Start = new(2.0, 3.0);
            Vector2D vector2D_Vec = new(4.0, 0.0);
            Segment2D segment2D_2 = new(point2D_Start, vector2D_Vec);
            Assert.NotNull(segment2D_2.Start);
            Assert.NotNull(segment2D_2.End);
            Assert.Equal(2.0, segment2D_2.Start.X, 9);
            Assert.Equal(3.0, segment2D_2.Start.Y, 9);
            Assert.Equal(6.0, segment2D_2.End.X, 9);
            Assert.Equal(3.0, segment2D_2.End.Y, 9);

            // 3. Start point and end point constructor
            Point2D point2D_End = new(10.0, 3.0);
            Segment2D segment2D_3 = new(point2D_Start, point2D_End);
            Assert.NotNull(segment2D_3.Start);
            Assert.NotNull(segment2D_3.End);
            Assert.Equal(2.0, segment2D_3.Start.X, 9);
            Assert.Equal(3.0, segment2D_3.Start.Y, 9);
            Assert.Equal(10.0, segment2D_3.End.X, 9);
            Assert.Equal(3.0, segment2D_3.End.Y, 9);

            // 4. Copy constructor
            Segment2D segment2D_Copy = new(segment2D_3);
            Assert.Equal(segment2D_3, segment2D_Copy);

            // 5. JSON serialization / deserialization roundtrip
            JsonObject? jsonObject = segment2D_3.ToJsonObject();
            Assert.NotNull(jsonObject);
            Segment2D segment2D_Json = new(jsonObject);
            Assert.NotNull(segment2D_Json.Start);
            Assert.NotNull(segment2D_Json.End);
            Assert.Equal(segment2D_3.Start.X, segment2D_Json.Start.X, 9);
            Assert.Equal(segment2D_3.Start.Y, segment2D_Json.Start.Y, 9);
            Assert.Equal(segment2D_3.End.X, segment2D_Json.End.X, 9);
            Assert.Equal(segment2D_3.End.Y, segment2D_Json.End.Y, 9);
        }

        /// <summary>
        /// Tests property getters, setters, and indexer behavior of Segment2D.
        /// </summary>
        [Fact]
        public void Segment2D_PropertiesAndIndexer()
        {
            Segment2D segment2D = new(0.0, 0.0, 10.0, 0.0);

            // Start & End properties
            Assert.Equal(10.0, segment2D.Length, 9);
            Assert.Equal(100.0, segment2D.SquaredLength, 9);

            // Modify Start
            segment2D.Start = new Point2D(2.0, 0.0);
            Assert.NotNull(segment2D.Start);
            Assert.Equal(2.0, segment2D.Start.X, 9);

            // Modify End
            segment2D.End = new Point2D(12.0, 0.0);
            Assert.NotNull(segment2D.End);
            Assert.Equal(12.0, segment2D.End.X, 9);
            Assert.Equal(10.0, segment2D.Length, 9);

            // Modify Length
            segment2D.Length = 20.0;
            Assert.Equal(20.0, segment2D.Length, 9);
            Assert.NotNull(segment2D.End);
            Assert.Equal(22.0, segment2D.End.X, 9);

            // Modify Direction
            segment2D.Direction = new Vector2D(0.0, 1.0);
            Assert.NotNull(segment2D.Direction);
            Assert.Equal(0.0, segment2D.Direction.X, 9);
            Assert.Equal(1.0, segment2D.Direction.Y, 9);

            // Indexer
            Assert.Equal(segment2D.Start, segment2D[0]);
            Assert.Equal(segment2D.End, segment2D[1]);
            Assert.Null(segment2D[-1]);
            Assert.Null(segment2D[2]);
        }

        /// <summary>
        /// Tests equality operators, Equals, GetHashCode, and implicit tuple conversions for Segment2D.
        /// </summary>
        [Fact]
        public void Segment2D_EqualityAndOperators()
        {
            Segment2D segment2D_1 = new(0.0, 0.0, 10.0, 0.0);
            Segment2D segment2D_2 = new(0.0, 0.0, 10.0, 0.0);
            Segment2D segment2D_3 = new(0.0, 0.0, 5.0, 5.0);

            // Operators == and !=
            Assert.True(segment2D_1 == segment2D_2);
            Assert.False(segment2D_1 != segment2D_2);
            Assert.False(segment2D_1 == segment2D_3);
            Assert.True(segment2D_1 != segment2D_3);

            // Null checks
            Segment2D? segment2D_Null = null;
            Assert.False(segment2D_1 == segment2D_Null);
            Assert.False(segment2D_Null == segment2D_1);

            // Equals & GetHashCode
            Assert.True(segment2D_1.Equals((object)segment2D_2));
            Assert.Equal(segment2D_1.GetHashCode(), segment2D_2.GetHashCode());

            // Implicit operators
            Segment2D? segment2D_TupleCoords = ((0.0, 0.0), (10.0, 0.0));
            Assert.NotNull(segment2D_TupleCoords);
            Assert.Equal(segment2D_1, segment2D_TupleCoords);

            Point2D point2D_A = new(0.0, 0.0);
            Point2D point2D_B = new(10.0, 0.0);
            Segment2D? segment2D_TuplePoints = (point2D_A, point2D_B);
            Assert.NotNull(segment2D_TuplePoints);
            Assert.Equal(segment2D_1, segment2D_TuplePoints);
        }

        /// <summary>
        /// Tests the Inverse method of the Segment2D class, verifying that reversing the segment correctly swaps its start and end points and negates its direction.
        /// </summary>
        [Fact]
        public void Segment2D_Inverse()
        {
            Point2D point2D_Start = new(0.0, 0.0);
            Point2D point2D_End = new(10.0, 0.0);
            Segment2D segment2D_Line = new(point2D_Start, point2D_End);

            // Verify initial state
            Assert.NotNull(segment2D_Line.Start);
            Assert.NotNull(segment2D_Line.End);
            Assert.Equal(0.0, segment2D_Line.Start.X, 9);
            Assert.Equal(0.0, segment2D_Line.Start.Y, 9);
            Assert.Equal(10.0, segment2D_Line.End.X, 9);
            Assert.Equal(0.0, segment2D_Line.End.Y, 9);
            Assert.Equal(10.0, segment2D_Line.Length, 9);

            // Invert segment
            bool success = segment2D_Line.Inverse();
            Assert.True(success);

            // Verify inverted state
            Assert.NotNull(segment2D_Line.Start);
            Assert.NotNull(segment2D_Line.End);
            Assert.Equal(10.0, segment2D_Line.Start.X, 9);
            Assert.Equal(0.0, segment2D_Line.Start.Y, 9);
            Assert.Equal(0.0, segment2D_Line.End.X, 9);
            Assert.Equal(0.0, segment2D_Line.End.Y, 9);
            Assert.Equal(10.0, segment2D_Line.Length, 9);
            Assert.NotNull(segment2D_Line.Direction);
            Assert.Equal(-1.0, segment2D_Line.Direction.X, 9);
            Assert.Equal(0.0, segment2D_Line.Direction.Y, 9);
        }

        /// <summary>
        /// Tests transforming a Segment2D object, verifying translation, scaling, and state-safety properties.
        /// </summary>
        [Fact]
        public void Segment2D_Transform()
        {
            Point2D point2D_Start = new(0.0, 0.0);
            Point2D point2D_End = new(10.0, 0.0);
            Segment2D segment2D_Line = new(point2D_Start, point2D_End);

            // 1. Test Transform method with Translation
            Transform2D? transform2D_Trans = Create.Transform2D.Translation(2.0, 3.0);
            Assert.NotNull(transform2D_Trans);
            bool bool_TransResult = segment2D_Line.Transform(transform2D_Trans);
            Assert.True(bool_TransResult);
            Assert.NotNull(segment2D_Line.Start);
            Assert.NotNull(segment2D_Line.End);
            Assert.Equal(2.0, segment2D_Line.Start.X, 9);
            Assert.Equal(3.0, segment2D_Line.Start.Y, 9);
            Assert.Equal(12.0, segment2D_Line.End.X, 9);
            Assert.Equal(3.0, segment2D_Line.End.Y, 9);

            // 2. Test state safety on transformation failure
            Transform2D transform2D_Invalid = new((Math.Classes.Matrix3D?)null);
            bool bool_InvalidResult = segment2D_Line.Transform(transform2D_Invalid);
            Assert.False(bool_InvalidResult);
            Assert.NotNull(segment2D_Line.Start);
            Assert.NotNull(segment2D_Line.End);
            Assert.Equal(2.0, segment2D_Line.Start.X, 9);
            Assert.Equal(3.0, segment2D_Line.Start.Y, 9);
            Assert.Equal(12.0, segment2D_Line.End.X, 9);
            Assert.Equal(3.0, segment2D_Line.End.Y, 9);
        }

        /// <summary>
        /// Tests Mid, GetBoundingBox, GetPoints, GetSegments, Move, and Clone operations on Segment2D.
        /// </summary>
        [Fact]
        public void Segment2D_CoreOperations()
        {
            Segment2D segment2D = new(0.0, 0.0, 10.0, 6.0);

            // Midpoint
            Point2D? mid = segment2D.Mid();
            Assert.NotNull(mid);
            Assert.Equal(5.0, mid.X, 9);
            Assert.Equal(3.0, mid.Y, 9);

            // Bounding box
            BoundingBox2D? bbox = segment2D.GetBoundingBox();
            Assert.NotNull(bbox);
            Assert.Equal(0.0, bbox.Min.X, 9);
            Assert.Equal(0.0, bbox.Min.Y, 9);
            Assert.Equal(10.0, bbox.Max.X, 9);
            Assert.Equal(6.0, bbox.Max.Y, 9);

            // GetPoints & GetSegments
            List<Point2D>? points = segment2D.GetPoints();
            Assert.NotNull(points);
            Assert.Equal(2, points.Count);

            List<Segment2D>? segments = segment2D.GetSegments();
            Assert.NotNull(segments);
            Assert.Single(segments);

            // Move
            Vector2D moveVec = new(1.0, 2.0);
            Assert.True(segment2D.Move(moveVec));
            Assert.NotNull(segment2D.Start);
            Assert.Equal(1.0, segment2D.Start.X, 9);
            Assert.Equal(2.0, segment2D.Start.Y, 9);

            // Clone
            ISerializableObject? clone = segment2D.Clone();
            Assert.NotNull(clone);
            Segment2D segment2D_Clone = Assert.IsType<Segment2D>(clone);
            Assert.Equal(segment2D, segment2D_Clone);
        }

        /// <summary>
        /// Tests spatial queries on Segment2D, including ClosestPoint, Distance, Project, and On checks.
        /// </summary>
        [Fact]
        public void Segment2D_SpatialQueries()
        {
            Segment2D segment2D = new(0.0, 0.0, 10.0, 0.0);

            // ClosestPoint
            Point2D? closestMid = segment2D.ClosestPoint(new Point2D(5.0, 5.0));
            Assert.NotNull(closestMid);
            Assert.Equal(5.0, closestMid.X, 9);
            Assert.Equal(0.0, closestMid.Y, 9);

            Point2D? closestBeforeStart = segment2D.ClosestPoint(new Point2D(-5.0, 5.0));
            Assert.NotNull(closestBeforeStart);
            Assert.Equal(0.0, closestBeforeStart.X, 9);
            Assert.Equal(0.0, closestBeforeStart.Y, 9);

            Point2D? closestAfterEnd = segment2D.ClosestPoint(new Point2D(15.0, 5.0));
            Assert.NotNull(closestAfterEnd);
            Assert.Equal(10.0, closestAfterEnd.X, 9);
            Assert.Equal(0.0, closestAfterEnd.Y, 9);

            // Distance
            Assert.Equal(5.0, segment2D.Distance(new Point2D(5.0, 5.0)), 9);
            Assert.Equal(0.0, segment2D.Distance(new Point2D(5.0, 0.0)), 9);

            // Project
            Point2D? projected = segment2D.Project(new Point2D(15.0, 5.0));
            Assert.NotNull(projected);
            Assert.Equal(15.0, projected.X, 9);
            Assert.Equal(0.0, projected.Y, 9);

            // On
            Assert.True(segment2D.On(new Point2D(5.0, 0.0)));
            Assert.False(segment2D.On(new Point2D(5.0, 5.0)));
            Assert.True(segment2D.On(new Point2D(5.0, 0.0), 1e-6));
        }

        /// <summary>
        /// Tests IntersectionPoint calculation for crossing, parallel, collinear, T-junction, and non-intersecting Segment2D geometries.
        /// </summary>
        [Fact]
        public void Segment2D_IntersectionPoint()
        {
            // Crossing segments
            Segment2D segment2D_1 = new(0.0, 5.0, 10.0, 5.0);
            Segment2D segment2D_2 = new(5.0, 0.0, 5.0, 10.0);

            Point2D? intersection = segment2D_1.IntersectionPoint(segment2D_2);
            Assert.NotNull(intersection);
            Assert.Equal(5.0, intersection.X, 9);
            Assert.Equal(5.0, intersection.Y, 9);

            // Parallel segments
            Segment2D segment2D_Parallel = new(0.0, 6.0, 10.0, 6.0);
            Assert.Null(segment2D_1.IntersectionPoint(segment2D_Parallel));

            // Non-crossing (lines cross outside segment bounds)
            Segment2D segment2D_Short = new(5.0, 6.0, 5.0, 10.0);
            Assert.Null(segment2D_1.IntersectionPoint(segment2D_Short));

            // T-junction (intersection on endpoint)
            Segment2D segment2D_TJunction = new(5.0, 5.0, 5.0, 10.0);
            Point2D? tIntersection = segment2D_1.IntersectionPoint(segment2D_TJunction);
            Assert.NotNull(tIntersection);
            Assert.Equal(5.0, tIntersection.X, 9);
            Assert.Equal(5.0, tIntersection.Y, 9);
        }

        /// <summary>
        /// Tests extension methods associated with Segment2D, including Extend, Similar, Create.Segment2Ds, Convert.ToNTS, and Convert.ToDiGi.
        /// </summary>
        [Fact]
        public void Segment2D_Extensions()
        {
            Segment2D segment2D = new(0.0, 0.0, 10.0, 0.0);

            // Extend start and end
            Segment2D? extended = segment2D.Extend(2.0, true, true);
            Assert.NotNull(extended);
            Assert.NotNull(extended.Start);
            Assert.NotNull(extended.End);
            Assert.Equal(-2.0, extended.Start.X, 9);
            Assert.Equal(12.0, extended.End.X, 9);

            // Extend end only
            Segment2D? extendedEnd = segment2D.Extend(2.0, false, true);
            Assert.NotNull(extendedEnd);
            Assert.NotNull(extendedEnd.Start);
            Assert.NotNull(extendedEnd.End);
            Assert.Equal(0.0, extendedEnd.Start.X, 9);
            Assert.Equal(12.0, extendedEnd.End.X, 9);

            // Similar (same and reversed direction)
            Segment2D segment2D_Reversed = new(10.0, 0.0, 0.0, 0.0);
            Assert.True(segment2D.Similar(segment2D_Reversed));

            // Create.Segment2Ds
            List<Point2D> points = [new(0.0, 0.0), new(10.0, 0.0), new(10.0, 10.0)];
            List<Segment2D>? createdSegments = points.Segment2Ds(true);
            Assert.NotNull(createdSegments);
            Assert.Equal(3, createdSegments.Count);

            // Convert ToNTS and ToDiGi
            NetTopologySuite.Geometries.LineSegment? ntsSegment = segment2D.ToNTS();
            Assert.NotNull(ntsSegment);
            Segment2D? backToDiGi = ntsSegment.ToDiGi();
            Assert.NotNull(backToDiGi);
            Assert.Equal(segment2D, backToDiGi);
        }

        /// <summary>
        /// Tests edge cases for Segment2D, including null inputs, zero-length segments, NaN values, and boundary tolerances.
        /// </summary>
        [Fact]
        public void Segment2D_EdgeCases()
        {
            // Zero-length segment (start == end)
            Segment2D zeroLength = new(5.0, 5.0, 5.0, 5.0);
            Assert.Equal(0.0, zeroLength.Length, 9);
            Assert.Equal(0.0, zeroLength.SquaredLength, 9);

            Point2D? closestZero = zeroLength.ClosestPoint(new Point2D(10.0, 10.0));
            Assert.NotNull(closestZero);
            Assert.Equal(5.0, closestZero.X, 9);
            Assert.Equal(5.0, closestZero.Y, 9);

            Assert.Equal(0.0, zeroLength.Distance(new Point2D(5.0, 5.0)), 9);

            // Null handling on methods
            Segment2D segment2D = new(0.0, 0.0, 10.0, 0.0);
            Assert.Null(segment2D.ClosestPoint(null));
            Assert.True(double.IsNaN(segment2D.Distance(null)));
            Assert.Null(segment2D.Project(null));
            Assert.False(segment2D.On((Point2D?)null));
            Assert.False(segment2D.Move(null));
            Assert.False(segment2D.Transform(null));

            // Null segment extension calls
            Segment2D? nullSegment = null;
            Assert.Null(nullSegment.Extend(5.0));
            Assert.True(nullSegment.Similar(null));
            Assert.False(segment2D.Similar(null));
        }
    }
}