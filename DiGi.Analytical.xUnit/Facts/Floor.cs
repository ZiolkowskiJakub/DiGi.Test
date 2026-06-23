using DiGi.Analytical.Building.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the creation of a <see cref="FaceFloor"/> instance and verifies that its geometry is correctly initialized.
        /// </summary>
        [Fact]
        public void Floor()
        {
            Plane? plane = Geometry.Spatial.Create.Plane(0.0);

            PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(plane,
            [
                new Geometry.Planar.Classes.Point2D(0, 0),
                new Geometry.Planar.Classes.Point2D(0, 10),
                new Geometry.Planar.Classes.Point2D(10, 0),
                new Geometry.Planar.Classes.Point2D(10, 10)
            ]);

            FaceFloor faceFloor = new(polygonalFace3D);

            Assert.NotNull(faceFloor?.Geometry);
        }
    }
}