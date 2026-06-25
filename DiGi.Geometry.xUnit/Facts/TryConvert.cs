using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using System.Collections.Generic;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the TryConvert extension method, verifying conversions between various polygonal geometries such as Polygon2D, Rectangle2D, Triangle2D, and their respective interface types, ensuring correctness of self-conversions, rectangular shapes detection, and triangle decomposition.
        /// </summary>
        [Fact]
        public void TryConvert()
        {
            List<Point2D> point2Ds_Square = [new(0, 0), new(4, 0), new(4, 4), new(0, 4)];
            List<Point2D> point2Ds_Trapezoid = [new(0, 0), new(5, 0), new(4, 3), new(1, 3)];
            List<Point2D> point2Ds_Triangle = [new(0, 0), new(3, 0), new(0, 4)];

            Polygon2D polygon2D_Square = new(point2Ds_Square);
            Polygon2D polygon2D_Trapezoid = new(point2Ds_Trapezoid);
            Triangle2D triangle2D_Base = new(point2Ds_Triangle);
            Rectangle2D rectangle2D_Base = new(new Point2D(0, 0), 4.0, 4.0);

            // 1. Self and subclass/interface compatibility conversions
            bool success_PolygonToPolygon = polygon2D_Square.TryConvert(out List<Polygon2D>? polygon2Ds_PolygonToPolygon);
            Assert.True(success_PolygonToPolygon);
            Assert.NotNull(polygon2Ds_PolygonToPolygon);
            Assert.Single(polygon2Ds_PolygonToPolygon);

            bool success_PolygonToIPolygonal = polygon2D_Square.TryConvert(out List<IPolygonal2D>? polygonal2Ds_PolygonToIPolygonal);
            Assert.True(success_PolygonToIPolygonal);
            Assert.NotNull(polygonal2Ds_PolygonToIPolygonal);
            Assert.Single(polygonal2Ds_PolygonToIPolygonal);

            bool success_TriangleToPolygon = triangle2D_Base.TryConvert(out List<Polygon2D>? polygon2Ds_TriangleToPolygon);
            Assert.True(success_TriangleToPolygon);
            Assert.NotNull(polygon2Ds_TriangleToPolygon);
            Assert.Single(polygon2Ds_TriangleToPolygon);

            // 2. Rectangle conversions (detecting rectangularity)
            bool success_PolygonToRectangle = polygon2D_Square.TryConvert(out List<Rectangle2D>? rectangle2Ds_PolygonToRectangle);
            Assert.True(success_PolygonToRectangle);
            Assert.NotNull(rectangle2Ds_PolygonToRectangle);
            Assert.Single(rectangle2Ds_PolygonToRectangle);

            bool success_TrapezoidToRectangle = polygon2D_Trapezoid.TryConvert(out List<Rectangle2D>? rectangle2Ds_TrapezoidToRectangle);
            Assert.False(success_TrapezoidToRectangle);
            Assert.Null(rectangle2Ds_TrapezoidToRectangle);

            bool success_TriangleToRectangle = triangle2D_Base.TryConvert(out List<Rectangle2D>? rectangle2Ds_TriangleToRectangle);
            Assert.False(success_TriangleToRectangle);
            Assert.Null(rectangle2Ds_TriangleToRectangle);

            // 3. Triangle conversions (triangulating shapes)
            bool success_PolygonToTriangle = polygon2D_Square.TryConvert(out List<Triangle2D>? triangle2Ds_PolygonToTriangle);
            Assert.True(success_PolygonToTriangle);
            Assert.NotNull(triangle2Ds_PolygonToTriangle);
            Assert.Equal(2, triangle2Ds_PolygonToTriangle.Count);

            bool success_RectangleToTriangle = rectangle2D_Base.TryConvert(out List<Triangle2D>? triangle2Ds_RectangleToTriangle);
            Assert.True(success_RectangleToTriangle);
            Assert.NotNull(triangle2Ds_RectangleToTriangle);
            Assert.Equal(2, triangle2Ds_RectangleToTriangle.Count);

            bool success_TriangleToTriangle = triangle2D_Base.TryConvert(out List<Triangle2D>? triangle2Ds_TriangleToTriangle);
            Assert.True(success_TriangleToTriangle);
            Assert.NotNull(triangle2Ds_TriangleToTriangle);
            Assert.Single(triangle2Ds_TriangleToTriangle);

            // 4. Null and invalid source conversions
            IPolygonal2D? polygonal2D_Null = null;
            bool success_NullSource = polygonal2D_Null.TryConvert(out List<Polygon2D>? polygon2Ds_NullSource);
            Assert.False(success_NullSource);
            Assert.Null(polygon2Ds_NullSource);
        }
    }
}
