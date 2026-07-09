using DiGi.Communication.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Communication.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the excess propagation delay of a mesh cell placed exactly on the propagation ellipsoid of the delay tau = 1 microsecond: the point (d / 2, 0, b_n) satisfies r_1 + r_2 = c * tau + d.
        /// </summary>
        [Fact]
        public void Delay()
        {
            double distance = 300;
            double delay = 1e-6;

            double semiMinorAxis = Query.SemiMinorAxis(delay, distance);

            MeshCell meshCell = new(new Point3D(distance / 2, 0, semiMinorAxis), new Vector3D(0, 0, 1), new MaterialProperties(15, 0.005));

            Assert.Equal(delay, meshCell.Delay(distance), 9);
            Assert.True(double.IsNaN(meshCell.Delay(0)));
        }
    }
}
