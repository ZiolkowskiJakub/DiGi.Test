using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests coordinate conversions between 3D space, a local 2D plane system, and back.
        /// </summary>
        [Fact]
        public void Conversion()
        {
            Plane plane = new(new Point3D(0, 0, 0), new Spatial.Classes.Vector3D(0, 0, 1));
            Line3D line3D = new(new Point3D(1, 2, 0), new Spatial.Classes.Vector3D(1, 1, 0));

            Line2D? line2D = plane.Convert(line3D);
            Assert.NotNull(line2D);

            Line3D? line3D_Temp = plane.Convert(line2D);
            Assert.NotNull(line3D_Temp);

            Assert.NotNull(line3D.Origin);
            Assert.NotNull(line3D_Temp.Origin);
            Assert.True(line3D.Origin.AlmostEquals(line3D_Temp.Origin, 1e-7));

            Assert.NotNull(line3D.Direction);
            Assert.NotNull(line3D_Temp.Direction);
            Assert.True(line3D.Direction.AlmostEquals(line3D_Temp.Direction, 1e-7));
        }
    }
}