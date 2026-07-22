using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests slicing a <see cref="BuildingModel"/> into storeys with the <see cref="BuildingModelFloorUpdater"/>.
        /// <para>Verifies that the update terminates and reports success for a valid floor count, that the model gains spaces, and that a floor count lower than two leaves the model untouched.</para>
        /// </summary>
        [Fact]
        public void BuildingModelFloorUpdater_Update()
        {
            BuildingModel buildingModel = new();

            Space space = new(new Point3D(5, 5, 2), "Space 1");

            Assert.NotNull(AddBoxSpace(buildingModel, new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10)), space, null, null, null));

            List<ISpace>? spaces = buildingModel.GetSpaces<ISpace>();
            Assert.NotNull(spaces);
            Assert.Single(spaces);

            // A floor count lower than two leaves the model untouched
            BuildingModelFloorUpdater buildingModelFloorUpdater = new(buildingModel)
            {
                FloorCount = 1
            };

            Assert.False(buildingModelFloorUpdater.Update());

            spaces = buildingModel.GetSpaces<ISpace>();
            Assert.NotNull(spaces);
            Assert.Single(spaces);

            // Two storeys out of a ten meter high space, cut by a single plane placed on the elevation of five
            buildingModelFloorUpdater.FloorCount = 2;

            Assert.True(buildingModelFloorUpdater.Update());

            spaces = buildingModel.GetSpaces<ISpace>();
            Assert.NotNull(spaces);
            Assert.Equal(2, spaces.Count);

            // The plane of the cut carries an air component, no floor construction was given
            List<IAir>? airs = buildingModel.GetComponents<IAir>();
            Assert.NotNull(airs);
            Assert.Single(airs);

            List<ISpace>? spaces_Air = buildingModel.GetSpaces(airs[0]);
            Assert.NotNull(spaces_Air);
            Assert.Equal(2, spaces_Air.Count);
        }
    }
}
