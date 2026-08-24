using DiGi.Analytical.Building.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        private static BuildingModel CreateTestBuildingModel(Polygon2D footprint)
        {
            PolygonalFace2D? face2D = Geometry.Planar.Create.PolygonalFace2D(footprint);
            Plane plane = new(new Point3D(0, 0, 0), new Vector3D(1, 0, 0), new Vector3D(0, 1, 0));
            PolygonalFace3D face3D = new(plane, face2D);
            FaceFloor floor = new(face3D);
            BuildingModel buildingModel = new();
            buildingModel.Update(floor);
            return buildingModel;
        }

        /// <summary>
        /// Validates that <see cref="Query.TerrainCircle(BuildingModel?, double, double)"/> returns null when the building model is null.
        /// </summary>
        [Fact]
        public void TerrainCircle_SingleBuilding_Null()
        {
            BuildingModel? buildingModel = null;
            Circle2D? circle2D = buildingModel.TerrainCircle();

            Assert.Null(circle2D);
        }

        /// <summary>
        /// Validates that <see cref="Query.TerrainCircle(BuildingModel?, double, double)"/> applies the default minimum radius of 100m when the building is small.
        /// </summary>
        [Fact]
        public void TerrainCircle_SingleBuilding_SmallBuilding_AppliesMinimumRadius()
        {
            Point2D[] footprint =
            [
                new(100.0, 200.0),
                new(120.0, 200.0),
                new(120.0, 210.0),
                new(100.0, 210.0)
            ];

            BuildingModel buildingModel = CreateTestBuildingModel(new Polygon2D(footprint));
            Circle2D? circle2D = buildingModel.TerrainCircle();

            Assert.NotNull(circle2D);
            Assert.NotNull(circle2D.Center);
            Assert.Equal(110.0, circle2D.Center.X, 3);
            Assert.Equal(205.0, circle2D.Center.Y, 3);
            Assert.Equal(Constants.Default.TerrainRadius, circle2D.Radius, 3);
        }

        /// <summary>
        /// Validates that <see cref="Query.TerrainCircle(BuildingModel?, double, double)"/> dynamically expands the terrain radius beyond the minimum when the building bounding radius plus padding exceeds 100m.
        /// </summary>
        [Fact]
        public void TerrainCircle_SingleBuilding_LargeBuilding_ExpandsRadius()
        {
            Point2D[] footprint =
            [
                new(0.0, 0.0),
                new(200.0, 0.0),
                new(200.0, 100.0),
                new(0.0, 100.0)
            ];

            BuildingModel buildingModel = CreateTestBuildingModel(new Polygon2D(footprint));
            Circle2D? circle2D = buildingModel.TerrainCircle();

            Assert.NotNull(circle2D);
            Assert.NotNull(circle2D.Center);
            Assert.Equal(100.0, circle2D.Center.X, 3);
            Assert.Equal(50.0, circle2D.Center.Y, 3);

            // Bounding radius = sqrt(100^2 + 50^2) = sqrt(12500) ≈ 111.803 m. With padding 50m -> ~161.803 m > 100m.
            double expectedBoundingRadius = System.Math.Sqrt((100.0 * 100.0) + (50.0 * 50.0));
            double expectedRadius = expectedBoundingRadius + Constants.Default.TerrainPadding;

            Assert.Equal(expectedRadius, circle2D.Radius, 3);
        }

        /// <summary>
        /// Validates that <see cref="Query.TerrainCircle(IEnumerable{BuildingModel}?, Circle2D?, double, double)"/> calculates dynamic coverage circle encompassing multiple buildings and expanding beyond the search circle.
        /// </summary>
        [Fact]
        public void TerrainCircle_MultipleBuildings_ExpandsBeyondSearchRadius()
        {
            Point2D[] footprint_1 =
            [
                new(0.0, 0.0),
                new(20.0, 0.0),
                new(20.0, 20.0),
                new(0.0, 20.0)
            ];

            Point2D[] footprint_2 =
            [
                new(80.0, 80.0),
                new(120.0, 80.0),
                new(120.0, 110.0),
                new(80.0, 110.0)
            ];

            BuildingModel buildingModel_1 = CreateTestBuildingModel(new Polygon2D(footprint_1));
            BuildingModel buildingModel_2 = CreateTestBuildingModel(new Polygon2D(footprint_2));

            Circle2D searchCircle = new(new Point2D(0.0, 0.0), 100.0);
            Circle2D? circle2D = Query.TerrainCircle([buildingModel_1, buildingModel_2], searchCircle);

            Assert.NotNull(circle2D);
            Assert.NotNull(circle2D.Center);
            Assert.Equal(0.0, circle2D.Center.X, 3);
            Assert.Equal(0.0, circle2D.Center.Y, 3);

            // Corner (120, 110) distance from (0,0) is sqrt(120^2 + 110^2) = sqrt(14400 + 12100) = sqrt(26500) ≈ 162.788 m.
            // With 50m padding -> ~212.788 m.
            double expectedMaxDist = System.Math.Sqrt((120.0 * 120.0) + (110.0 * 110.0));
            double expectedRadius = expectedMaxDist + Constants.Default.TerrainPadding;

            Assert.Equal(expectedRadius, circle2D.Radius, 3);
        }

        /// <summary>
        /// Validates that <see cref="Query.TerrainCircle(IEnumerable{BuildingModel}?, Circle2D?, double, double)"/> returns the search circle when the building list is empty.
        /// </summary>
        [Fact]
        public void TerrainCircle_MultipleBuildings_EmptyList_ReturnsSearchCircle()
        {
            Circle2D searchCircle = new(new Point2D(10.0, 20.0), 50.0);
            Circle2D? circle2D = Query.TerrainCircle([], searchCircle);

            Assert.NotNull(circle2D);
            Assert.NotNull(circle2D.Center);
            Assert.Equal(10.0, circle2D.Center.X, 3);
            Assert.Equal(20.0, circle2D.Center.Y, 3);
            Assert.Equal(50.0, circle2D.Radius, 3);
        }

        /// <summary>
        /// Validates that <see cref="Query.TerrainRequestUri(Circle2D?, double?)"/> clamps the query radius to <see cref="Constants.Default.TerrainRadiusMax"/> to prevent exceeding the terrain service ceiling.
        /// </summary>
        [Fact]
        public void TerrainRequestUri_ClampsToTerrainRadiusMax()
        {
            Circle2D circle2D = new(new Point2D(629671.3, 489136.8), 2500.0);
            string? uri = circle2D.TerrainRequestUri();

            Assert.NotNull(uri);
            Assert.Contains($"radius={Constants.Default.TerrainRadiusMax}", uri);
        }
    }
}
