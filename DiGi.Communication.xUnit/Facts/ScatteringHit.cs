using DiGi.Communication.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Communication.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the creation, properties, copy constructor, and serialization of <see cref="Classes.ScatteringHit"/>.
        /// </summary>
        [Fact]
        public void ScatteringHit()
        {
            string reference = "Building_101";
            Point3D point3D_Origin = new(10, 20, 30);
            Vector3D vector3D_Direction = new(1, 0, 0);
            Ray3D ray3D = new(point3D_Origin, vector3D_Direction);

            ScatteringHit scatteringHit_1 = new(reference, ray3D);

            Assert.Equal(reference, scatteringHit_1.Reference);
            Assert.NotNull(scatteringHit_1.Ray3D);
            if (scatteringHit_1.Ray3D is Ray3D ray3D_Actual)
            {
                Assert.Equal(10, ray3D_Actual.Origin.X);
                Assert.Equal(1, ray3D_Actual.Direction.X);
            }

            ScatteringHit scatteringHit_2 = new(scatteringHit_1);

            Assert.Equal(scatteringHit_1.Reference, scatteringHit_2.Reference);
            Assert.NotNull(scatteringHit_2.Ray3D);

            ScatteringObject scatteringObject = new(reference, null, Constants.ElectricalProperties.Concrete);
            ScatteringHit scatteringHit_3 = new(scatteringObject, ray3D);

            Assert.Equal(reference, scatteringHit_3.Reference);
            Assert.NotNull(scatteringHit_3.Ray3D);

            Core.xUnit.Query.SerializationCheck(scatteringHit_1);
        }
    }
}
