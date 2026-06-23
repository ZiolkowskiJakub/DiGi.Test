namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the basic arithmetic operators, dot product, and length calculation of the Vector3D class.
        /// </summary>
        [Fact]
        public void Vector3D_Arithmetic()
        {
            DiGi.Geometry.Spatial.Classes.Vector3D vector3D_1 = new(1.0, 2.0, 3.0);
            DiGi.Geometry.Spatial.Classes.Vector3D vector3D_2 = new(-2.0, 0.0, 4.0);

            DiGi.Geometry.Spatial.Classes.Vector3D? vector3D_Sum = vector3D_1 + vector3D_2;
            DiGi.Geometry.Spatial.Classes.Vector3D? vector3D_Diff = vector3D_1 - vector3D_2;
            double double_Dot = vector3D_1 * vector3D_2;
            double double_Len = vector3D_1.Length;
            double double_SqLen = vector3D_1.SquaredLength;

            Assert.NotNull(vector3D_Sum);
            Assert.Equal(-1.0, vector3D_Sum.X);
            Assert.Equal(2.0, vector3D_Sum.Y);
            Assert.Equal(7.0, vector3D_Sum.Z);

            Assert.NotNull(vector3D_Diff);
            Assert.Equal(3.0, vector3D_Diff.X);
            Assert.Equal(2.0, vector3D_Diff.Y);
            Assert.Equal(-1.0, vector3D_Diff.Z);

            Assert.Equal(10.0, double_Dot);
            Assert.Equal(System.Math.Sqrt(14.0), double_Len, 5);
            Assert.Equal(14.0, double_SqLen);
        }

        /// <summary>
        /// Tests the cross product calculations of the Vector3D class.
        /// </summary>
        [Fact]
        public void Vector3D_CrossProduct()
        {
            DiGi.Geometry.Spatial.Classes.Vector3D vector3D_1 = new(1.0, 0.0, 0.0);
            DiGi.Geometry.Spatial.Classes.Vector3D vector3D_2 = new(0.0, 1.0, 0.0);

            DiGi.Geometry.Spatial.Classes.Vector3D? vector3D_Cross = vector3D_1.CrossProduct(vector3D_2);

            Assert.NotNull(vector3D_Cross);
            Assert.Equal(0.0, vector3D_Cross.X);
            Assert.Equal(0.0, vector3D_Cross.Y);
            Assert.Equal(1.0, vector3D_Cross.Z);
        }

        /// <summary>
        /// Tests normalization behavior of Vector3D, including unit vector conversion and handling of zero vectors.
        /// </summary>
        [Fact]
        public void Vector3D_Normalization()
        {
            DiGi.Geometry.Spatial.Classes.Vector3D vector3D_Main = new(0.0, 0.0, 5.0);
            DiGi.Geometry.Spatial.Classes.Vector3D? vector3D_Unit = vector3D_Main.Unit;

            Assert.NotNull(vector3D_Unit);
            Assert.Equal(0.0, vector3D_Unit.X);
            Assert.Equal(0.0, vector3D_Unit.Y);
            Assert.Equal(1.0, vector3D_Unit.Z);
            Assert.Equal(1.0, vector3D_Unit.Length);

            DiGi.Geometry.Spatial.Classes.Vector3D vector3D_Zero = new(0.0, 0.0, 0.0);
            DiGi.Geometry.Spatial.Classes.Vector3D? vector3D_ZeroUnit = vector3D_Zero.Unit;
            Assert.NotNull(vector3D_ZeroUnit);
            Assert.True(double.IsNaN(vector3D_ZeroUnit.X) || vector3D_ZeroUnit.X == 0.0);
        }

        /// <summary>
        /// Tests explicit and implicit conversion operators for Vector3D.
        /// </summary>
        [Fact]
        public void Vector3D_Conversions()
        {
            DiGi.Geometry.Spatial.Classes.Point3D point3D_Source = new(1.0, 2.0, 3.0);
            DiGi.Geometry.Spatial.Classes.Vector3D? vector3D_Converted = (DiGi.Geometry.Spatial.Classes.Vector3D?)point3D_Source;

            Assert.NotNull(vector3D_Converted);
            Assert.Equal(1.0, vector3D_Converted.X);
            Assert.Equal(2.0, vector3D_Converted.Y);
            Assert.Equal(3.0, vector3D_Converted.Z);

            DiGi.Geometry.Spatial.Classes.Vector3D? vector3D_FromTuple = (4.0, 5.0, 6.0);
            Assert.NotNull(vector3D_FromTuple);
            Assert.Equal(4.0, vector3D_FromTuple.X);
            Assert.Equal(5.0, vector3D_FromTuple.Y);
            Assert.Equal(6.0, vector3D_FromTuple.Z);
        }
    }
}
