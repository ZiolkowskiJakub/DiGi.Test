using DiGi.Communication.Classes;
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

            Point3D point3D_Origin = new(0, 0, 0);
            Vector3D vector3D_Direction = new(1, 0, 0);
            Ray3D ray3D = new(point3D_Origin, vector3D_Direction);

            ScatteringHit scatteringHit = new("Ref_1", ray3D);

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
    }
}
