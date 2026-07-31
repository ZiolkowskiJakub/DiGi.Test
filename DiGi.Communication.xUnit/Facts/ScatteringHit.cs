using DiGi.Communication.Classes;
using DiGi.Communication.Enums;
using DiGi.Geometry.Spatial.Classes;
using System;

namespace DiGi.Communication.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the creation, properties, copy constructor, radioscience angle calculations, electrical properties, and serialization of <see cref="Classes.ScatteringHit"/>.
        /// </summary>
        [Fact]
        public void ScatteringHit()
        {
            string reference = "Building_101";
            ElectricalProperties electricalProperties = Constants.ElectricalProperties.Concrete;
            double frequency = 2.4e9;

            Point3D point3D_Tx = new(0, 0, 10);
            Point3D point3D_Hit = new(0, 0, 0);
            Point3D point3D_Rx = new(10, 0, 0);

            ScatteringHit scatteringHit_1 = new(reference, electricalProperties, frequency, point3D_Tx, point3D_Rx, point3D_Hit);

            Assert.Equal(reference, scatteringHit_1.Reference);
            Assert.Equal(frequency, scatteringHit_1.Frequency);
            Assert.NotNull(scatteringHit_1.ElectricalProperties);
            Assert.NotNull(scatteringHit_1.Location);
            Assert.NotNull(scatteringHit_1.Location_Transmitter);
            Assert.NotNull(scatteringHit_1.Location_Receiver);

            // Copy constructor check
            ScatteringHit scatteringHit_2 = new(scatteringHit_1);
            Assert.Equal(scatteringHit_1.Reference, scatteringHit_2.Reference);
            Assert.Equal(scatteringHit_1.Frequency, scatteringHit_2.Frequency);

            // Node functions: Receiver and Transmitter rays and vectors
            Vector3D? vector3D_Rx = scatteringHit_1.GetVector3D(Function.Receiver);
            Assert.NotNull(vector3D_Rx);
            Assert.Equal(1, vector3D_Rx!.X, 6);
            Assert.Equal(0, vector3D_Rx.Y, 6);
            Assert.Equal(0, vector3D_Rx.Z, 6);

            Vector3D? vector3D_Tx = scatteringHit_1.GetVector3D(Function.Transmitter);
            Assert.NotNull(vector3D_Tx);
            Assert.Equal(0, vector3D_Tx!.X, 6);
            Assert.Equal(0, vector3D_Tx.Y, 6);
            Assert.Equal(-1, vector3D_Tx.Z, 6);

            Ray3D? ray3D_Rx = scatteringHit_1.GetRay3D(Function.Receiver);
            Assert.NotNull(ray3D_Rx);

            Ray3D? ray3D_Tx = scatteringHit_1.GetRay3D(Function.Transmitter);
            Assert.NotNull(ray3D_Tx);

            // Electrical properties checks
            double conductivity = scatteringHit_1.GetConductivity();
            Assert.False(double.IsNaN(conductivity));
            double permittivity = scatteringHit_1.GetRelativePermittivity();
            Assert.False(double.IsNaN(permittivity));

            // Surface normal: bisector of (0,0,1) and (1,0,0) -> (1/sqrt(2), 0, 1/sqrt(2))
            Vector3D? normal = scatteringHit_1.GetNormal();
            Assert.NotNull(normal);
            Assert.Equal(1 / Math.Sqrt(2), normal!.X, 6);
            Assert.Equal(0, normal.Y, 6);
            Assert.Equal(1 / Math.Sqrt(2), normal.Z, 6);

            // Radioscience standards check: Oblique incidence at 45 deg (pi/4 rad)
            double reflectionAngle = scatteringHit_1.GetReflectionAngle();
            Assert.Equal(Math.PI / 4, reflectionAngle, 6);

            double grazingAngle = scatteringHit_1.GetGrazingAngle();
            Assert.Equal(Math.PI / 4, grazingAngle, 6);

            // Normal incidence test (0 deg reflection angle from surface normal, 90 deg grazing angle)
            Point3D point3D_TxNormal = new(0, 0, 10);
            Point3D point3D_RxNormal = new(0, 0, 10);
            ScatteringHit scatteringHit_NormalIncidence = new(reference, electricalProperties, frequency, point3D_TxNormal, point3D_RxNormal, point3D_Hit);

            Assert.Equal(0.0, scatteringHit_NormalIncidence.GetReflectionAngle(), 6);
            Assert.Equal(Math.PI / 2, scatteringHit_NormalIncidence.GetGrazingAngle(), 6);

            Core.xUnit.Query.SerializationCheck(scatteringHit_1);
        }
    }
}
