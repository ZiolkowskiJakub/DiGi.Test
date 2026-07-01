using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the VerticalPosition calculation for points relative to individual segments and complex segmentable 2D shapes (like polygons), verifying Above, Below, On, Inside, and Undefined relationships.
        /// </summary>
        [Fact]
        public void VerticalPosition()
        {
            // Null inputs
            Segment2D? segment2D_Null = null;
            Point2D? point2D_Null = null;
            Assert.Equal(Core.Enums.VerticalPosition.Undefined, DiGi.Geometry.Planar.Query.VerticalPosition(segment2D_Null, point2D_Null));

            // Setup a horizontal segment from (0, 2) to (10, 2)
            Segment2D segment2D_Horizontal = new(new Point2D(0.0, 2.0), new Point2D(10.0, 2.0));

            // Point above
            Point2D point2D_AboveSegment = new(5.0, 5.0);
            Assert.Equal(Core.Enums.VerticalPosition.Above, DiGi.Geometry.Planar.Query.VerticalPosition(segment2D_Horizontal, point2D_AboveSegment));

            // Point below
            Point2D point2D_BelowSegment = new(5.0, -1.0);
            Assert.Equal(Core.Enums.VerticalPosition.Below, DiGi.Geometry.Planar.Query.VerticalPosition(segment2D_Horizontal, point2D_BelowSegment));

            // Point on
            Point2D point2D_OnSegment = new(5.0, 2.0);
            Assert.Equal(Core.Enums.VerticalPosition.On, DiGi.Geometry.Planar.Query.VerticalPosition(segment2D_Horizontal, point2D_OnSegment));

            // Point outside X-bounds
            Point2D point2D_OutsideSegment = new(12.0, 5.0);
            Assert.Equal(Core.Enums.VerticalPosition.Undefined, DiGi.Geometry.Planar.Query.VerticalPosition(segment2D_Horizontal, point2D_OutsideSegment));

            // Setup a closed 2D shape using a PolygonalFace3D's 2D geometry
            PolygonalFace3D? polygonalFace3D = Spatial.Create.PolygonalFace3D(
                Spatial.Constants.Plane.WorldZ,
                new Point2D(0.0, 0.0),
                new Point2D(10.0, 0.0),
                new Point2D(10.0, 10.0),
                new Point2D(0.0, 10.0)
            );
            Assert.NotNull(polygonalFace3D);
            if (polygonalFace3D is null)
            {
                return;
            }

            IPolygonalFace2D? polygonalFace2D = polygonalFace3D.Geometry2D;
            Assert.NotNull(polygonalFace2D);
            if (polygonalFace2D is null)
            {
                return;
            }

            ISegmentable2D? segmentable2D_Polygon = polygonalFace2D.ExternalEdge;
            Assert.NotNull(segmentable2D_Polygon);
            if (segmentable2D_Polygon is null)
            {
                return;
            }

            // Point inside the polygon
            Point2D point2D_InsidePolygon = new(5.0, 5.0);
            Assert.Equal(Core.Enums.VerticalPosition.Inside, DiGi.Geometry.Planar.Query.VerticalPosition(segmentable2D_Polygon, point2D_InsidePolygon));

            // Point completely below the polygon
            Point2D point2D_BelowPolygon = new(5.0, -3.0);
            Assert.Equal(Core.Enums.VerticalPosition.Below, DiGi.Geometry.Planar.Query.VerticalPosition(segmentable2D_Polygon, point2D_BelowPolygon));

            // Point completely above the polygon
            Point2D point2D_AbovePolygon = new(5.0, 13.0);
            Assert.Equal(Core.Enums.VerticalPosition.Above, DiGi.Geometry.Planar.Query.VerticalPosition(segmentable2D_Polygon, point2D_AbovePolygon));

            // Point on the boundary of the polygon
            Point2D point2D_OnPolygon = new(5.0, 0.0);
            Assert.Equal(Core.Enums.VerticalPosition.On, DiGi.Geometry.Planar.Query.VerticalPosition(segmentable2D_Polygon, point2D_OnPolygon));
        }
    }
}