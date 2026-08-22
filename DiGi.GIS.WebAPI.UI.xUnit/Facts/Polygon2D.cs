using DiGi.Geometry.Planar.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Validates that <see cref="Create.Polygon2D(Circle2D?, int)"/> creates a valid regular polygon approximating the circle.
        /// </summary>
        [Fact]
        public void Polygon2D_FromCircle_Valid()
        {
            Circle2D circle2D = new(new Point2D(50.0, 100.0), 20.0);
            Polygon2D? polygon2D = circle2D.Polygon2D(64);

            Assert.NotNull(polygon2D);
            List<Point2D>? point2Ds = polygon2D.GetPoints();
            Assert.NotNull(point2Ds);
            Assert.Equal(64, point2Ds.Count);

            // Verify points are on radius distance from center
            foreach (Point2D point2D in point2Ds)
            {
                double distance = point2D.Distance(circle2D.Center);
                Assert.Equal(20.0, distance, 3);
            }

            // Approximate circle area = pi * r^2 ≈ 1256.637. 64-gon area ≈ 0.5 * 64 * r^2 * sin(2pi/64) ≈ 1251.48
            double area = polygon2D.GetArea();
            Assert.True(area > 1250.0 && area < 1257.0);
        }

        /// <summary>
        /// Validates that <see cref="Create.Polygon2D(Circle2D?, int)"/> returns null for invalid circle inputs.
        /// </summary>
        [Fact]
        public void Polygon2D_FromCircle_InvalidInputs_ReturnsNull()
        {
            Circle2D? circle2D_Null = null;
            Assert.Null(circle2D_Null.Polygon2D());

            Circle2D circle2D_ZeroRadius = new(new Point2D(0.0, 0.0), 0.0);
            Assert.Null(circle2D_ZeroRadius.Polygon2D());

            Circle2D circle2D_NegativeRadius = new(new Point2D(0.0, 0.0), -10.0);
            Assert.Null(circle2D_NegativeRadius.Polygon2D());

            Circle2D circle2D_TooFewSegments = new(new Point2D(0.0, 0.0), 10.0);
            Assert.Null(circle2D_TooFewSegments.Polygon2D(2));
        }

        /// <summary>
        /// Validates that <see cref="Create.Polygon2D(BoundingBox2D?)"/> creates a 4-corner rectangular polygon with matching area.
        /// </summary>
        [Fact]
        public void Polygon2D_FromBoundingBox_Valid()
        {
            BoundingBox2D boundingBox2D = new(new Point2D(10.0, 20.0), new Point2D(40.0, 60.0));
            Polygon2D? polygon2D = boundingBox2D.Polygon2D();

            Assert.NotNull(polygon2D);
            List<Point2D>? point2Ds = polygon2D.GetPoints();
            Assert.NotNull(point2Ds);
            Assert.Equal(4, point2Ds.Count);

            double area = polygon2D.GetArea();
            Assert.Equal(30.0 * 40.0, area, 3);
        }
    }
}
