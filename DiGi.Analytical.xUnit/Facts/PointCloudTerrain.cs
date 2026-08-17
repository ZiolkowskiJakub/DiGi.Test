using DiGi.Analytical.Building.Classes;
using DiGi.Geometry.PointCloud.Spatial.Classes;
using DiGi.Geometry.Spatial.Classes;
using System.Collections.Generic;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the construction, geometry assignment, copy constructor and serialization round trip of <see cref="PointCloudTerrain"/>.
        /// </summary>
        [Fact]
        public void PointCloudTerrain()
        {
            List<Point3D?> point3Ds =
            [
                new Point3D(0, 0, 10),
                new Point3D(10, 0, 11),
                new Point3D(10, 10, 12),
                new Point3D(0, 10, 13)
            ];

            PointCloud3D pointCloud3D = new(point3Ds);

            PointCloudTerrain pointCloudTerrain_1 = new(pointCloud3D);

            Assert.NotNull(pointCloudTerrain_1.Geometry);
            Assert.Equal(point3Ds.Count, pointCloudTerrain_1.Geometry!.Count);

            // Copy constructor check
            PointCloudTerrain pointCloudTerrain_2 = new(pointCloudTerrain_1);
            Assert.Equal(pointCloudTerrain_1.Guid, pointCloudTerrain_2.Guid);
            Assert.NotNull(pointCloudTerrain_2.Geometry);
            Assert.Equal(point3Ds.Count, pointCloudTerrain_2.Geometry!.Count);

            // Null geometry is allowed
            PointCloudTerrain pointCloudTerrain_3 = new((PointCloud3D?)null);
            Assert.Null(pointCloudTerrain_3.Geometry);

            // String round trip
            string? json = Core.Convert.ToSystem_String(pointCloudTerrain_1);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<PointCloudTerrain>? pointCloudTerrains = Core.Convert.ToDiGi<PointCloudTerrain>(json);
            Assert.NotNull(pointCloudTerrains);

            PointCloudTerrain? pointCloudTerrain_Temp = pointCloudTerrains!.Count == 0 ? null : pointCloudTerrains[0];
            Assert.NotNull(pointCloudTerrain_Temp);
            Assert.Equal(pointCloudTerrain_1.Guid, pointCloudTerrain_Temp!.Guid);
            Assert.NotNull(pointCloudTerrain_Temp.Geometry);
            Assert.Equal(point3Ds.Count, pointCloudTerrain_Temp.Geometry!.Count);

            Core.xUnit.Query.SerializationCheck(pointCloudTerrain_1);
        }
    }
}
