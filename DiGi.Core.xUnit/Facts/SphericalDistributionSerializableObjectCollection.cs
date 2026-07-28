using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the creation, bin indexing, point/range retrieval, wrap-around logic, deep cloning, and serialization of <see cref="Core.Classes.SphericalDistributionSerializableObjectCollection"/>.
        /// </summary>
        [Fact]
        public void SphericalDistributionSerializableObjectCollection()
        {
            double minAzimuth = 0;
            double maxAzimuth = 360;
            double azimuthInterval = 45;
            double minElevation = -90;
            double maxElevation = 90;
            double elevationInterval = 30;

            Core.Classes.SphericalDistributionSerializableObjectCollection collection_1 = new(
                minAzimuth,
                maxAzimuth,
                azimuthInterval,
                minElevation,
                maxElevation,
                elevationInterval);

            Assert.NotNull(collection_1.AzimuthRange);
            Assert.Equal(0, collection_1.AzimuthRange.Min);
            Assert.Equal(360, collection_1.AzimuthRange.Max);
            Assert.Equal(45, collection_1.AzimuthInterval);
            Assert.Equal(8, collection_1.AzimuthCount);

            Assert.NotNull(collection_1.ElevationRange);
            Assert.Equal(-90, collection_1.ElevationRange.Min);
            Assert.Equal(90, collection_1.ElevationRange.Max);
            Assert.Equal(30, collection_1.ElevationInterval);
            Assert.Equal(6, collection_1.ElevationCount);

            Assert.Equal(0, collection_1.Count);

            Core.Classes.Size size_1 = new(10, 20);
            Core.Classes.Size size_2 = new(30, 40);
            Core.Classes.Size size_3 = new(50, 60);

            collection_1.AddValue(0, 0, size_1);
            collection_1.AddValue(90, 45, size_2);
            collection_1.AddValues(355, -15, [size_3]);

            Assert.Equal(3, collection_1.Count);
            Assert.Equal(3, collection_1.Values.Count);

            // Test GetAzimuthRange starting from min (0, 45], (45, 90], etc.
            Core.Classes.Range<double>? azRange_Bin0 = collection_1.GetAzimuthRange(0);
            Assert.NotNull(azRange_Bin0);
            Assert.Equal(0, azRange_Bin0.Min);
            Assert.Equal(45, azRange_Bin0.Max);

            Core.Classes.Range<double>? azRange_Bin1 = collection_1.GetAzimuthRange(1);
            Assert.NotNull(azRange_Bin1);
            Assert.Equal(45, azRange_Bin1.Min);
            Assert.Equal(90, azRange_Bin1.Max);

            Core.Classes.Range<double>? azRange_Bin7 = collection_1.GetAzimuthRange(7);
            Assert.NotNull(azRange_Bin7);
            Assert.Equal(315, azRange_Bin7.Min);
            Assert.Equal(360, azRange_Bin7.Max);

            // Test GetElevationRange starting from min (-90, -60], (-60, -30], etc.
            Core.Classes.Range<double>? elRange_Bin0 = collection_1.GetElevationRange(0);
            Assert.NotNull(elRange_Bin0);
            Assert.Equal(-90, elRange_Bin0.Min);
            Assert.Equal(-60, elRange_Bin0.Max);

            List<Core.Classes.Range<double>>? allAzRanges = collection_1.GetAzimuthRanges();
            Assert.NotNull(allAzRanges);
            Assert.Equal(8, allAzRanges.Count);

            List<Core.Classes.Range<double>>? populatedAzRanges = collection_1.GetAzimuthRanges(populatedOnly: true);
            Assert.NotNull(populatedAzRanges);
            Assert.Equal(3, populatedAzRanges.Count);

            List<Core.Classes.Range<double>>? allElRanges = collection_1.GetElevationRanges();
            Assert.NotNull(allElRanges);
            Assert.Equal(6, allElRanges.Count);

            List<Core.Classes.Range<double>>? populatedElRanges = collection_1.GetElevationRanges(populatedOnly: true);
            Assert.NotNull(populatedElRanges);
            Assert.Equal(2, populatedElRanges.Count);

            // Coordinates (0, 0) fall into bin 0 (0, 45] and elevation bin 2 (-30, 0] containing size_1
            IReadOnlyList<Interfaces.ISerializableObject>? values_Point1 = collection_1.GetValues(0, 0);
            Assert.NotNull(values_Point1);
            Assert.Single(values_Point1);
            Core.Classes.Size? size_Point1 = values_Point1[0] as Core.Classes.Size;
            Assert.NotNull(size_Point1);
            Assert.Equal(10, size_Point1.Width);

            // Coordinates (90, 45) fall into bin 1 (45, 90] and elevation bin 4 (30, 60] containing size_2
            IReadOnlyList<Interfaces.ISerializableObject>? values_Point2 = collection_1.GetValues(90, 45);
            Assert.NotNull(values_Point2);
            Assert.Single(values_Point2);
            Core.Classes.Size? size_Point2 = values_Point2[0] as Core.Classes.Size;
            Assert.NotNull(size_Point2);
            Assert.Equal(30, size_Point2.Width);

            // Coordinates (355, -15) fall into bin 7 (315, 360] and elevation bin 2 (-30, 0] containing size_3
            IReadOnlyList<Interfaces.ISerializableObject>? values_Point3 = collection_1.GetValues(355, -15);
            Assert.NotNull(values_Point3);
            Assert.Single(values_Point3);
            Core.Classes.Size? size_Point3 = values_Point3[0] as Core.Classes.Size;
            Assert.NotNull(size_Point3);
            Assert.Equal(50, size_Point3.Width);

            IReadOnlyList<Interfaces.ISerializableObject>? values_WrapAround = collection_1.GetValues(350, 10, -30, 10);
            Assert.NotNull(values_WrapAround);
            Assert.Equal(2, values_WrapAround.Count);

            JsonObject? jsonObject_1 = collection_1.ToJsonObject();
            Assert.NotNull(jsonObject_1);

            Core.Classes.SphericalDistributionSerializableObjectCollection? collection_2 = Create.SerializableObject<Core.Classes.SphericalDistributionSerializableObjectCollection>(jsonObject_1);
            Assert.NotNull(collection_2);

            Assert.Equal(collection_1.Count, collection_2.Count);
            Assert.Equal(collection_1.AzimuthCount, collection_2.AzimuthCount);
            Assert.Equal(collection_1.ElevationCount, collection_2.ElevationCount);

            JsonObject? jsonObject_2 = collection_2.ToJsonObject();
            Assert.NotNull(jsonObject_2);
            Assert.Equal(jsonObject_1.ToJsonString(), jsonObject_2.ToJsonString());

            Core.Classes.SphericalDistributionSerializableObjectCollection collection_3 = new(collection_1);
            Assert.Equal(collection_1.Count, collection_3.Count);

            collection_3.Clear();
            Assert.Equal(0, collection_3.Count);

            Query.SerializationCheck(collection_1);
        }

        /// <summary>
        /// Tests generic <see cref="Core.Classes.SphericalDistributionSerializableObjectCollection{TSerializableObject}"/> with strongly typed elements.
        /// </summary>
        [Fact]
        public void SphericalDistributionSerializableObjectCollection_Generic()
        {
            Core.Classes.SphericalDistributionSerializableObjectCollection<Core.Classes.Size> collection_1 = new(0, 360, 90, -90, 90, 45);

            Core.Classes.Size size_1 = new(100, 200);
            collection_1.AddValue(45, 0, size_1);

            Assert.Equal(1, collection_1.Count);
            IReadOnlyList<Core.Classes.Size>? values_1 = collection_1.GetValues(45, 0);
            Assert.NotNull(values_1);
            Assert.Single(values_1);
            Assert.Equal(100, values_1[0].Width);

            Query.SerializationCheck(collection_1);
        }
    }
}
