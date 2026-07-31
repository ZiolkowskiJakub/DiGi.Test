using DiGi.Communication.Classes;
using DiGi.Communication.Enums;
using DiGi.Geometry.Spatial.Classes;
using System.Collections.Generic;

namespace DiGi.Communication.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the creation, properties, location indexing, node functions, copy constructor, and serialization of <see cref="Classes.ScatteringHits"/>.
        /// </summary>
        [Fact]
        public void ScatteringHits()
        {
            string reference = "Building_MultiHit";
            ElectricalProperties electricalProperties = Constants.ElectricalProperties.Vacuum;
            double frequency = 5.8e9;

            Point3D location_Tx = new(0, 0, 20);
            Point3D location_Rx = new(20, 0, 0);
            List<Point3D> locations =
            [
                new(0, 0, 0),
                new(1, 2, 3),
                new(4, 5, 6)
            ];

            ScatteringHits scatteringHits_1 = new(reference, electricalProperties, frequency, location_Tx, location_Rx, locations);

            Assert.Equal(reference, scatteringHits_1.Reference);
            Assert.Equal(frequency, scatteringHits_1.Frequency);
            Assert.NotNull(scatteringHits_1.ElectricalProperties);
            Assert.NotNull(scatteringHits_1.Location_Transmitter);
            Assert.NotNull(scatteringHits_1.Location_Receiver);
            Assert.NotNull(scatteringHits_1.Locations);
            Assert.Equal(3, scatteringHits_1.Count);

            // Indexer test
            Point3D? hit0 = scatteringHits_1[0];
            Assert.NotNull(hit0);
            Assert.Equal(0, hit0!.X);
            Assert.Equal(0, hit0.Y);
            Assert.Equal(0, hit0.Z);

            Point3D? hit1 = scatteringHits_1[1];
            Assert.NotNull(hit1);
            Assert.Equal(1, hit1!.X);
            Assert.Equal(2, hit1.Y);
            Assert.Equal(3, hit1.Z);

            // Node function vector & ray tests for specific hit index
            Vector3D? vector3D_Rx = scatteringHits_1.GetVector3D(Function.Receiver, 0);
            Assert.NotNull(vector3D_Rx);
            Assert.Equal(1, vector3D_Rx!.X, 6);
            Assert.Equal(0, vector3D_Rx.Y, 6);
            Assert.Equal(0, vector3D_Rx.Z, 6);

            Ray3D? ray3D_Tx = scatteringHits_1.GetRay3D(Function.Transmitter, 0);
            Assert.NotNull(ray3D_Tx);

            double azimuth = scatteringHits_1.GetAzimuth(Function.Receiver, 0);
            Assert.False(double.IsNaN(azimuth));

            double elevation = scatteringHits_1.GetElevation(Function.Receiver, 0);
            Assert.False(double.IsNaN(elevation));

            // Copy constructor test
            ScatteringHits scatteringHits_2 = new(scatteringHits_1);
            Assert.Equal(scatteringHits_1.Reference, scatteringHits_2.Reference);
            Assert.Equal(scatteringHits_1.Count, scatteringHits_2.Count);
            Assert.Equal(scatteringHits_1.Frequency, scatteringHits_2.Frequency);

            Core.xUnit.Query.SerializationCheck(scatteringHits_1);
        }
    }
}
