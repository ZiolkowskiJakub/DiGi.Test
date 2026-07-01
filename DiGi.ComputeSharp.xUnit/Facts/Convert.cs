using DiGi.ComputeSharp.Spatial.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Tests for the DiGi.Geometry &lt;-&gt; ComputeSharp conversion adapters.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Coordinate3 -> Vector3D must map X, Y, Z one-to-one. Regression test for the bug where the Z
        /// component was filled from X.
        /// </summary>
        [Fact]
        public void Convert_Coordinate3_ToDiGi_Vector3D()
        {
            Coordinate3 coordinate3 = new(2, 3, 5);

            Vector3D? vector3D = DiGi.ComputeSharp.Geometry.Spatial.Convert.ToDiGi_Vector3D(coordinate3);

            Assert.NotNull(vector3D);
            Assert.Equal(2.0, vector3D!.X, 1e-9);
            Assert.Equal(3.0, vector3D.Y, 1e-9);
            Assert.Equal(5.0, vector3D.Z, 1e-9);
        }

        /// <summary>
        /// Coordinate3 -> Point3D must map X, Y, Z one-to-one (control for the converter pattern).
        /// </summary>
        [Fact]
        public void Convert_Coordinate3_ToDiGi_Point3D()
        {
            Coordinate3 coordinate3 = new(2, 3, 5);

            Point3D? point3D = DiGi.ComputeSharp.Geometry.Spatial.Convert.ToDiGi(coordinate3);

            Assert.NotNull(point3D);
            Assert.Equal(2.0, point3D!.X, 1e-9);
            Assert.Equal(3.0, point3D.Y, 1e-9);
            Assert.Equal(5.0, point3D.Z, 1e-9);
        }
    }
}