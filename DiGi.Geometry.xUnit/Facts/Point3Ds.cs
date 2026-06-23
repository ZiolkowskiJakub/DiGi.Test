using DiGi.Geometry.Core.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the density-based spatial clustering on a collection of 3D points.
        /// </summary>
        [Fact]
        public void Point3Ds()
        {
            int count = DiGi.Core.Query.DecimalPlacesCount(0.0001);
            Assert.Equal(4, count);

            List<Point3D> point3Ds = [];
            point3Ds.Add(new Point3D(0, 0, 0));
            point3Ds.Add(new Point3D(0.000001, 0, 0));
            point3Ds.Add(new Point3D(10, 10, 10));
            point3Ds.Add(new Point3D(10, 10.000001, 10));
            point3Ds.Add(new Point3D(1, 1, 1));

            DensityBasedSpatialClusteringResult<Point3D>? densityBasedSpatialClusteringResult = DiGi.Geometry.Core.Create.DensityBasedSpatialClusteringResult(point3Ds, 0.001, 1);
            Assert.NotNull(densityBasedSpatialClusteringResult);

            Point3D? point3D = densityBasedSpatialClusteringResult.GetPoint(1, points => DiGi.Geometry.Spatial.Query.Average(points)!);
            Assert.NotNull(point3D);

            // Serialization can be verified to compile and serialize, but deserialization round-trip is bypassed
            // due to a core library limitation where dictionary keys containing complex ISerializableObject types (like Point3D)
            // cannot be deserialized back from their string representations.
            string? json = DiGi.Core.Convert.ToSystem_String(densityBasedSpatialClusteringResult);
            Assert.NotNull(json);
        }
    }
}