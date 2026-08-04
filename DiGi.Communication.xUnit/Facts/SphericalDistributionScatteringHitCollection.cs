using DiGi.Communication.Classes;
using DiGi.Communication.Enums;
using DiGi.Communication.Interfaces;
using DiGi.Geometry.Spatial.Classes;
using System.Collections.Generic;

namespace DiGi.Communication.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the creation, element population, range retrieval, and serialization of <see cref="Classes.SphericalDistributionScatteringHitCollection"/>.
        /// </summary>
        [Fact]
        public void SphericalDistributionScatteringHitCollection()
        {
            SphericalDistributionScatteringHitCollection collection_1 = new();

            Assert.Equal(0, collection_1.Count);

            ElectricalProperties electricalProperties = Constants.ElectricalProperties.Concrete;
            double frequency = 2.4e9;
            Point3D point3D_Tx = new(0, 0, 10);
            Point3D point3D_Rx = new(10, 0, 0);
            Point3D point3D_Hit = new(0, 0, 0);

            ScatteringHit scatteringHit = new("Ref_1", electricalProperties, frequency, point3D_Tx, point3D_Rx, point3D_Hit);

            double azimuth = 0.5;
            double elevation = 1.0;

            collection_1.AddValue(azimuth, elevation, scatteringHit);

            Assert.Equal(1, collection_1.Count);

            IReadOnlyList<IScatteringHit>? values = collection_1.GetValues(azimuth, elevation);
            Assert.NotNull(values);
            Assert.Single(values);
            Assert.Equal("Ref_1", values[0].Reference);

            SphericalDistributionScatteringHitCollection collection_2 = new(collection_1);
            Assert.Equal(collection_1.Count, collection_2.Count);

            Core.xUnit.Query.SerializationCheck(collection_1);
        }

        /// <summary>
        /// Tests the query combining the scattering hits of every delay of an <see cref="AngularPowerDistributionProfile"/> into a single <see cref="Classes.SphericalDistributionScatteringHitCollection"/>.
        /// <para>Asserts that the combined collection holds the hits of all delays, that every hit sits in the bin its own geometry places it in, and that a profile without hits combines into nothing rather than into an empty collection.</para>
        /// </summary>
        [Fact]
        public void SphericalDistributionScatteringHitCollection_Query()
        {
            ElectricalProperties electricalProperties = Constants.ElectricalProperties.Concrete;
            double frequency = 2.4e9;
            Point3D point3D_Transmitter = new(0, 0, 10);
            Point3D point3D_Receiver = new(50, 0, 10);

            ScatteringHit scatteringHit_1 = new("Ref_1", electricalProperties, frequency, point3D_Transmitter, point3D_Receiver, new Point3D(25, 20, 0));
            ScatteringHit scatteringHit_2 = new("Ref_2", electricalProperties, frequency, point3D_Transmitter, point3D_Receiver, new Point3D(25, -20, 0));
            ScatteringHit scatteringHit_3 = new("Ref_3", electricalProperties, frequency, point3D_Transmitter, point3D_Receiver, new Point3D(30, 35, 5));

            SphericalDistributionScatteringHitCollection collection_1 = new();
            collection_1.AddValue(Function.Receiver, scatteringHit_1);
            collection_1.AddValue(Function.Receiver, scatteringHit_2);

            SphericalDistributionScatteringHitCollection collection_2 = new();
            collection_2.AddValue(Function.Receiver, scatteringHit_3);

            Assert.Equal(2, collection_1.Count);
            Assert.Equal(1, collection_2.Count);

            List<AngularPowerDistribution> angularPowerDistributions = [new(1e-6, collection_1), new(2e-6, collection_2)];

            AngularPowerDistributionProfile angularPowerDistributionProfile = new(point3D_Receiver, frequency, angularPowerDistributions);

            SphericalDistributionScatteringHitCollection? collection_Combined = angularPowerDistributionProfile.SphericalDistributionScatteringHitCollection(Function.Receiver);

            Assert.NotNull(collection_Combined);
            Assert.Equal(collection_1.Count + collection_2.Count, collection_Combined.Count);

            // Every hit is binned by its own geometry, so it is found at the azimuth and elevation it
            // reports towards the receiver - the same bin it occupies in the collection of its delay.
            ScatteringHit[] scatteringHits_All = [scatteringHit_1, scatteringHit_2, scatteringHit_3];
            foreach (ScatteringHit scatteringHit in scatteringHits_All)
            {
                IReadOnlyList<IScatteringHit>? scatteringHits = collection_Combined.GetValues(scatteringHit.GetAzimuth(Function.Receiver), scatteringHit.GetElevation(Function.Receiver));
                Assert.NotNull(scatteringHits);
                Assert.Contains(scatteringHits, x => x.Reference == scatteringHit.Reference);
            }

            // A profile without distributions and a profile whose distributions hold no hits both
            // combine into nothing rather than into an empty collection.
            AngularPowerDistributionProfile angularPowerDistributionProfile_Empty = new(point3D_Receiver, frequency, null);
            Assert.Null(angularPowerDistributionProfile_Empty.SphericalDistributionScatteringHitCollection(Function.Receiver));

            AngularPowerDistributionProfile angularPowerDistributionProfile_NoHits = new(point3D_Receiver, frequency, [new(1e-6, new SphericalDistributionScatteringHitCollection())]);
            Assert.Null(angularPowerDistributionProfile_NoHits.SphericalDistributionScatteringHitCollection(Function.Receiver));
        }
    }
}
