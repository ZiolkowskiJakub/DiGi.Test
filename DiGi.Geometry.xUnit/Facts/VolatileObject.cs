using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the serialization of polygonal faces and verifies that the generated system string representations are equal.
        /// </summary>
        [Fact]
        public void VolatileObject()
        {
            Polygon2D polygon2D = new(
            [
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(0, 10)
            ]);

            PolygonalFace2D? polygonalFace2D = Planar.Create.PolygonalFace2D(polygon2D);
            Assert.NotNull(polygonalFace2D);

            PolygonalFace3D polygonalFace3D = new(Spatial.Constants.Plane.WorldZ, polygonalFace2D);
            Assert.NotNull(polygonalFace3D);

            string? json = DiGi.Core.Convert.ToSystem_String(polygonalFace2D);
            Assert.NotNull(json);

            string? json_Volatile = DiGi.Core.Convert.ToSystem_String(polygonalFace2D);
            Assert.NotNull(json_Volatile);

            Assert.Equal(json, json_Volatile);
        }
    }
}