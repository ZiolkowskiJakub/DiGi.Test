using DiGi.Geometry.Planar.Interfaces;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;
using DiGi.Solar.Classes;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that GeometricalShadingSolverResult correctly calculates its area and converts 2D faces to 3D representations even when the 2D face list contains null elements.
        /// </summary>
        [Fact]
        public void GeometricalShadingSolverResult()
        {
            DateTime dateTime = new(2026, 6, 26, 12, 0, 0);
            Plane plane = DiGi.Geometry.Spatial.Constants.Plane.WorldZ;

            List<IPolygonalFace2D> polygonalFace2Ds = [null!];

            GeometricalShadingSolverResult geometricalShadingSolverResult = new(dateTime, plane, polygonalFace2Ds);

            Assert.Equal(0.0, geometricalShadingSolverResult.Area);

            List<IPolygonalFace3D>? polygonalFace3Ds = geometricalShadingSolverResult.GetPolygonalFace3Ds();
            Assert.NotNull(polygonalFace3Ds);
            Assert.Empty(polygonalFace3Ds);
        }
    }
}