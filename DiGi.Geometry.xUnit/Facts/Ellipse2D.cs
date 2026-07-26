using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the functionality of the <see cref="Ellipse2D"/> class, verifying operations such as projection, bounding box calculation, intersection points, and point sampling.
        /// </summary>
        [Fact]
        public void Ellipse2D()
        {
            Ellipse2D ellipse2D = new(new Point2D(0, 0), 5, 10);

            Point2D? point2D_1 = ellipse2D.Project(new Point2D(11, 0));

            Point2D? point2D_2 = ellipse2D.Project(new Point2D(5, 5));

            Point2D? point2D_3 = ellipse2D.Project(new Point2D(0, 11));

            Point2D point2D_Input = new(10, 100);

            BoundingBox2D? boundingBox2D = new Ellipse2D(new Point2D(0, 0), 5, 10).GetBoundingBox();

            List<Point2D>? intersectionPoint2Ds = Planar.Query.IntersectionPoints(ellipse2D, new Segment2D(new Point2D(0, -20), new Point2D(0, 20)));
            Assert.NotNull(intersectionPoint2Ds);

            List<Point2D> point2Ds = [];
            point2Ds.Add(ellipse2D.GetPoint(new Vector2D(1, 0))!);
            point2Ds.Add(ellipse2D.GetPoint(new Vector2D(1, 1))!);
            point2Ds.Add(ellipse2D.GetPoint(new Vector2D(0, 1))!);
            point2Ds.Add(ellipse2D.GetPoint(new Vector2D(-1, 1))!);
            point2Ds.Add(ellipse2D.GetPoint(new Vector2D(-1, 0))!);
            point2Ds.Add(ellipse2D.GetPoint(new Vector2D(-1, -1))!);
            point2Ds.Add(ellipse2D.GetPoint(new Vector2D(0, -1))!);
            point2Ds.Add(ellipse2D.GetPoint(new Vector2D(1, -1))!);

            Assert.NotNull(point2D_1);
            Assert.Equal(5, point2D_1.X, 4);
            Assert.Equal(0, point2D_1.Y, 4);

            Assert.NotNull(point2D_2);

            Assert.NotNull(point2D_3);
            Assert.Equal(0, point2D_3.X, 4);
            Assert.Equal(10, point2D_3.Y, 4);

            Assert.NotNull(point2D_Input);

            Assert.NotNull(boundingBox2D);
            Assert.NotNull(boundingBox2D.Min);
            Assert.Equal(-5, boundingBox2D.Min.X, 4);
            Assert.Equal(-10, boundingBox2D.Min.Y, 4);
            Assert.NotNull(boundingBox2D.Max);
            Assert.Equal(5, boundingBox2D.Max.X, 4);
            Assert.Equal(10, boundingBox2D.Max.Y, 4);

            Assert.Equal(2, intersectionPoint2Ds!.Count);
            Assert.Contains(intersectionPoint2Ds, p => System.Math.Abs(p.X) < 1e-4 && System.Math.Abs(p.Y - 10) < 1e-4);
            Assert.Contains(intersectionPoint2Ds, p => System.Math.Abs(p.X) < 1e-4 && System.Math.Abs(p.Y + 10) < 1e-4);

            Assert.Equal(8, point2Ds.Count);
        }

        /// <summary>
        /// Tests transforming and translating an Ellipse2D object, verifying translation, scaling, rotation, and state-safety properties.
        /// </summary>
        [Fact]
        public void Ellipse2D_Transform()
        {
            Point2D point2D_Center = new(0.0, 0.0);
            Vector2D vector2D_DirA = new(1.0, 0.0);
            Ellipse2D ellipse2D_Target = new(point2D_Center, 5.0, 10.0, vector2D_DirA);

            // 1. Test Transform method with Translation
            Transform2D? transform2D_Trans = Planar.Create.Transform2D.Translation(10.0, -10.0);
            Assert.NotNull(transform2D_Trans);
            bool bool_TransResult = ellipse2D_Target.Transform(transform2D_Trans);
            Assert.True(bool_TransResult);
            Assert.NotNull(ellipse2D_Target.Center);
            Assert.Equal(10.0, ellipse2D_Target.Center.X, 9);
            Assert.Equal(-10.0, ellipse2D_Target.Center.Y, 9);
            Assert.Equal(5.0, ellipse2D_Target.A, 9);
            Assert.Equal(10.0, ellipse2D_Target.B, 9);
            Assert.NotNull(ellipse2D_Target.DirectionA);
            Assert.Equal(1.0, ellipse2D_Target.DirectionA.X, 9);
            Assert.Equal(0.0, ellipse2D_Target.DirectionA.Y, 9);

            // 2. Test Transform method with Uniform Scaling (radius should double)
            Transform2D? transform2D_Scale = Planar.Create.Transform2D.Scale(2.0);
            Assert.NotNull(transform2D_Scale);
            bool bool_ScaleResult = ellipse2D_Target.Transform(transform2D_Scale);
            Assert.True(bool_ScaleResult);
            Assert.NotNull(ellipse2D_Target.Center);
            Assert.Equal(20.0, ellipse2D_Target.Center.X, 9);
            Assert.Equal(-20.0, ellipse2D_Target.Center.Y, 9);
            Assert.Equal(10.0, ellipse2D_Target.A, 9);
            Assert.Equal(20.0, ellipse2D_Target.B, 9);

            // 3. Test Transform method with Rotation (around origin by 90 degrees)
            Transform2D? transform2D_Rot = Planar.Create.Transform2D.Rotation(System.Math.PI / 2.0);
            Assert.NotNull(transform2D_Rot);
            bool bool_RotResult = ellipse2D_Target.Transform(transform2D_Rot);
            Assert.True(bool_RotResult);
            Assert.NotNull(ellipse2D_Target.Center);
            Assert.Equal(20.0, ellipse2D_Target.Center.X, 9);
            Assert.Equal(20.0, ellipse2D_Target.Center.Y, 9);
            Assert.NotNull(ellipse2D_Target.DirectionA);
            Assert.Equal(0.0, ellipse2D_Target.DirectionA.X, 9);
            Assert.Equal(1.0, ellipse2D_Target.DirectionA.Y, 9);

            // 4. Test state safety on transformation failure
            Transform2D transform2D_Invalid = new((Math.Classes.Matrix3D?)null);
            bool bool_InvalidResult = ellipse2D_Target.Transform(transform2D_Invalid);
            Assert.False(bool_InvalidResult);
            Assert.NotNull(ellipse2D_Target.Center);
            Assert.Equal(20.0, ellipse2D_Target.Center.X, 9);
            Assert.Equal(20.0, ellipse2D_Target.Center.Y, 9);
            Assert.Equal(10.0, ellipse2D_Target.A, 9);
            Assert.Equal(20.0, ellipse2D_Target.B, 9);
            Assert.NotNull(ellipse2D_Target.DirectionA);
            Assert.Equal(0.0, ellipse2D_Target.DirectionA.X, 9);
            Assert.Equal(1.0, ellipse2D_Target.DirectionA.Y, 9);
        }

        /// <summary>
        /// Verifies that the tolerance overload of Project returns the true closest boundary point (satisfying the nearest-point optimality condition), that Distance reports that minimum distance rather than the radial approximation, and that GetFocalLength returns the inter-focal distance (2C).
        /// </summary>
        [Fact]
        public void Ellipse2D_ClosestPoint()
        {
            double double_A = 10.0;
            double double_B = 2.0;
            Ellipse2D ellipse2D = new(new Point2D(0.0, 0.0), double_A, double_B, new Vector2D(1.0, 0.0));

            // On-axis point: the closest boundary point coincides with the radial projection.
            Point2D point2D_OnAxis = new(20.0, 0.0);
            Point2D? point2D_OnAxisClosest = ellipse2D.Project(point2D_OnAxis, 1e-9);
            Assert.NotNull(point2D_OnAxisClosest);
            Assert.Equal(double_A, point2D_OnAxisClosest.X, 6);
            Assert.Equal(0.0, point2D_OnAxisClosest.Y, 6);

            // Off-axis external point: closest point differs from the radial projection.
            Point2D point2D_Target = new(7.0, 4.0);
            Point2D? point2D_Closest = ellipse2D.Project(point2D_Target, 1e-9);
            Point2D? point2D_Radial = ellipse2D.Project(point2D_Target);
            Assert.NotNull(point2D_Closest);
            Assert.NotNull(point2D_Radial);

            // The closest point lies on the boundary.
            double double_Equation = (point2D_Closest.X / double_A) * (point2D_Closest.X / double_A) + (point2D_Closest.Y / double_B) * (point2D_Closest.Y / double_B);
            Assert.Equal(1.0, double_Equation, 6);

            // Nearest-point optimality: (target - closest) is orthogonal to the boundary tangent at the closest point.
            double double_Cos = point2D_Closest.X / double_A;
            double double_Sin = point2D_Closest.Y / double_B;
            double double_TangentX = -double_A * double_Sin;
            double double_TangentY = double_B * double_Cos;
            double double_Dot = (point2D_Target.X - point2D_Closest.X) * double_TangentX + (point2D_Target.Y - point2D_Closest.Y) * double_TangentY;
            Assert.Equal(0.0, double_Dot, 4);

            // The closest point is at least as near as the radial approximation, and Distance reports that minimum.
            double double_ClosestDistance = point2D_Target.Distance(point2D_Closest);
            double double_RadialDistance = point2D_Target.Distance(point2D_Radial);
            Assert.True(double_ClosestDistance <= double_RadialDistance + 1e-9);
            Assert.True(double_RadialDistance - double_ClosestDistance > 1e-6);
            Assert.Equal(double_ClosestDistance, ellipse2D.Distance(point2D_Target), 6);

            // GetFocalLength is the distance between the two foci (2C).
            Assert.Equal(2.0 * System.Math.Sqrt(double_A * double_A - double_B * double_B), ellipse2D.GetFocalLength(), 9);
        }
    }
}