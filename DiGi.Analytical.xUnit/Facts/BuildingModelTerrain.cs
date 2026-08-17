using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Geometry.PointCloud.Spatial.Classes;
using DiGi.Geometry.Spatial.Classes;
using System.Collections.Generic;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests <see cref="BuildingModel.Update(ITerrain)"/>, <see cref="BuildingModel.GetTerrains{TTerrain}"/> and <see cref="BuildingModel.Remove(ITerrain)"/>.
        /// <para>Covers storage under the terrain identifier, retrieval by the non-generic and by the concrete terrain type, the clone boundary of the model and removal.</para>
        /// </summary>
        [Fact]
        public void BuildingModelTerrain()
        {
            PlaneTerrain planeTerrain = new(new Plane(new Point3D(0, 0, 100), new Vector3D(0, 0, 1)));

            List<Point3D?> point3Ds =
            [
                new Point3D(0, 0, 100),
                new Point3D(10, 0, 101),
                new Point3D(10, 10, 102)
            ];

            PointCloudTerrain pointCloudTerrain = new(new PointCloud3D(point3Ds));

            BuildingModel buildingModel = new();

            Assert.False(buildingModel.Update((ITerrain?)null));
            Assert.True(buildingModel.Update(planeTerrain));
            Assert.True(buildingModel.Update(pointCloudTerrain));

            // Both terrains are reachable through the non-generic interface
            List<ITerrain>? terrains = buildingModel.GetTerrains<ITerrain>();
            Assert.NotNull(terrains);
            Assert.Equal(2, terrains!.Count);

            // ... and each one through its own type
            List<PlaneTerrain>? planeTerrains = buildingModel.GetTerrains<PlaneTerrain>();
            Assert.NotNull(planeTerrains);
            Assert.Single(planeTerrains!);
            Assert.Equal(planeTerrain.Guid, planeTerrains![0].Guid);

            List<IPointCloudTerrain>? pointCloudTerrains = buildingModel.GetTerrains<IPointCloudTerrain>();
            Assert.NotNull(pointCloudTerrains);
            Assert.Single(pointCloudTerrains!);
            Assert.Equal(pointCloudTerrain.Guid, pointCloudTerrains![0].Guid);
            Assert.NotNull(pointCloudTerrains[0].Geometry);
            Assert.Equal(point3Ds.Count, pointCloudTerrains[0].Geometry!.Count);

            // The model hands out clones, not the stored instances
            Assert.NotSame(planeTerrain, planeTerrains[0]);

            // Updating under the same identifier replaces rather than adds
            PlaneTerrain planeTerrain_Temp = new(planeTerrain.Guid, planeTerrain);
            Assert.True(buildingModel.Update(planeTerrain_Temp));
            Assert.Equal(2, buildingModel.GetTerrains<ITerrain>()?.Count);

            // Terrains survive a round trip of the whole model
            Core.xUnit.Query.SerializationCheck(buildingModel);

            BuildingModel? buildingModel_Temp = Core.Query.Clone(buildingModel);
            Assert.NotNull(buildingModel_Temp);
            Assert.Equal(2, buildingModel_Temp!.GetTerrains<ITerrain>()?.Count);
            Assert.Single(buildingModel_Temp.GetTerrains<IPointCloudTerrain>()!);

            // Removal - an emptied model reports an empty list, not null
            Assert.False(buildingModel.Remove((ITerrain?)null));
            Assert.True(buildingModel.Remove(planeTerrain));
            Assert.Empty(buildingModel.GetTerrains<PlaneTerrain>()!);
            Assert.Single(buildingModel.GetTerrains<ITerrain>()!);

            Assert.True(buildingModel.Remove(pointCloudTerrain));
            Assert.Empty(buildingModel.GetTerrains<ITerrain>()!);
        }
    }
}
