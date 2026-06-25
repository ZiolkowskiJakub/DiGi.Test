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
            DiGi.Geometry.Planar.Classes.Point2D point2D_Center = new(0.0, 0.0);
            DiGi.Geometry.Planar.Classes.Vector2D vector2D_DirA = new(1.0, 0.0);
            DiGi.Geometry.Planar.Classes.Ellipse2D ellipse2D_Target = new(point2D_Center, 5.0, 10.0, vector2D_DirA);

            // 1. Test Transform method with Translation
            DiGi.Geometry.Planar.Classes.Transform2D? transform2D_Trans = DiGi.Geometry.Planar.Create.Transform2D.Translation(10.0, -10.0);
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
            DiGi.Geometry.Planar.Classes.Transform2D? transform2D_Scale = DiGi.Geometry.Planar.Create.Transform2D.Scale(2.0);
            Assert.NotNull(transform2D_Scale);
            bool bool_ScaleResult = ellipse2D_Target.Transform(transform2D_Scale);
            Assert.True(bool_ScaleResult);
            Assert.NotNull(ellipse2D_Target.Center);
            Assert.Equal(20.0, ellipse2D_Target.Center.X, 9);
            Assert.Equal(-20.0, ellipse2D_Target.Center.Y, 9);
            Assert.Equal(10.0, ellipse2D_Target.A, 9);
            Assert.Equal(20.0, ellipse2D_Target.B, 9);

            // 3. Test Transform method with Rotation (around origin by 90 degrees)
            DiGi.Geometry.Planar.Classes.Transform2D? transform2D_Rot = DiGi.Geometry.Planar.Create.Transform2D.Rotation(System.Math.PI / 2.0);
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
            DiGi.Geometry.Planar.Classes.Transform2D transform2D_Invalid = new((DiGi.Math.Classes.Matrix3D?)null);
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
    }
}