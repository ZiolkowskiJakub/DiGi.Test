using DiGi.Communication.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Communication.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the grazing angle of incidence on the mesh cell plane: a ray towards (0, 1, 1) hits a horizontal plane at 45 degrees, a ray along the plane at 0 degrees, and a degenerate cell yields NaN.
        /// </summary>
        [Fact]
        public void IncidenceAngle()
        {
            MaterialProperties materialProperties = new(15, 0.005);

            MeshCell meshCell_Oblique = new(new Point3D(0, 1, 1), new Vector3D(0, 0, 1), materialProperties);
            Assert.Equal(Math.PI / 4, meshCell_Oblique.IncidenceAngle(), 6);

            MeshCell meshCell_Grazing = new(new Point3D(1, 0, 0), new Vector3D(0, 0, 1), materialProperties);
            Assert.Equal(0.0, meshCell_Grazing.IncidenceAngle(), 6);

            MeshCell meshCell_Degenerate = new(new Point3D(1, 0, 0), new Vector3D(0, 0, 0), materialProperties);
            Assert.True(double.IsNaN(meshCell_Degenerate.IncidenceAngle()));
        }
    }
}
