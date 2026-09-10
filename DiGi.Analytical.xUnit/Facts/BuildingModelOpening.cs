using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Creates a vertical <see cref="Plane"/> used as the geometry of the openings under test.
        /// </summary>
        /// <returns>The created <see cref="Plane"/>.</returns>
        private static Plane BuildingModelOpening_Plane()
        {
            return new Plane(new Point3D(0, 0, 0), new Vector3D(0, 1, 0));
        }

        /// <summary>
        /// Tests that <see cref="BuildingModel.Assign(IComponent, IOpening)"/> hosts the opening on the component and keeps the openings already hosted on it.
        /// </summary>
        [Fact]
        public void BuildingModelAssign_Component_Opening()
        {
            FaceFloor faceFloor = new(BuildingModelUnassign_PolygonalFace3D());

            Opening opening_1 = new(BuildingModelOpening_Plane());
            Opening opening_2 = new(BuildingModelOpening_Plane());

            BuildingModel buildingModel = new();
            Assert.True(buildingModel.Assign(faceFloor, opening_1));
            Assert.True(buildingModel.Assign(faceFloor, opening_2));

            List<IOpening>? openings = buildingModel.GetOpenings<IOpening>(faceFloor);
            Assert.NotNull(openings);
            Assert.Equal(2, openings!.Count);
            Assert.Contains(openings, x => x.Guid == opening_1.Guid);
            Assert.Contains(openings, x => x.Guid == opening_2.Guid);

            Assert.False(buildingModel.Assign(null, opening_1));
            Assert.False(buildingModel.Assign(faceFloor, null));
        }

        /// <summary>
        /// Tests that <see cref="BuildingModel.Assign(IComponent, IOpening)"/> moves the opening from the component that hosted it before to the new host.
        /// </summary>
        [Fact]
        public void BuildingModelAssign_Component_Opening_Rehost()
        {
            FaceFloor faceFloor_1 = new(BuildingModelUnassign_PolygonalFace3D());
            FaceFloor faceFloor_2 = new(BuildingModelUnassign_PolygonalFace3D());

            Opening opening = new(BuildingModelOpening_Plane());

            BuildingModel buildingModel = new();
            Assert.True(buildingModel.Assign(faceFloor_1, opening));

            Assert.True(buildingModel.Assign(faceFloor_2, opening));

            List<IOpening>? openings_1 = buildingModel.GetOpenings<IOpening>(faceFloor_1);
            Assert.True(openings_1 == null || openings_1.Count == 0);

            List<IOpening>? openings_2 = buildingModel.GetOpenings<IOpening>(faceFloor_2);
            Assert.NotNull(openings_2);
            Assert.Single(openings_2!);
            Assert.Equal(opening.Guid, openings_2![0].Guid);
        }

        /// <summary>
        /// Tests that <see cref="BuildingModel.Unassign(IComponent, IOpening)"/> removes the opening relation of a component hosting a single opening, leaving the component and the opening stored.
        /// </summary>
        [Fact]
        public void BuildingModelUnassign_Component_Opening_LastOpening()
        {
            FaceFloor faceFloor = new(BuildingModelUnassign_PolygonalFace3D());

            Opening opening_1 = new(BuildingModelOpening_Plane());

            BuildingModel buildingModel = new();
            Assert.True(buildingModel.Assign(faceFloor, opening_1));

            Assert.True(buildingModel.Unassign(faceFloor, opening_1));

            List<IOpening>? openings = buildingModel.GetOpenings<IOpening>(faceFloor);
            Assert.True(openings == null || openings.Count == 0);

            Assert.Null(buildingModel.GetRelation<OpeningRelation>(faceFloor));

            List<IOpening>? openings_Model = buildingModel.GetOpenings<IOpening>();
            Assert.NotNull(openings_Model);
            Assert.Single(openings_Model!);

            List<IFloor>? floors = buildingModel.GetComponents<IFloor>();
            Assert.NotNull(floors);
            Assert.Single(floors!);
        }

        /// <summary>
        /// Tests that <see cref="BuildingModel.Unassign(IComponent, IOpening)"/> removes only the requested opening of a component hosting two openings, keeping the other one.
        /// </summary>
        [Fact]
        public void BuildingModelUnassign_Component_Opening_SecondOfTwo()
        {
            FaceFloor faceFloor = new(BuildingModelUnassign_PolygonalFace3D());

            Opening opening_1 = new(BuildingModelOpening_Plane());
            Opening opening_2 = new(BuildingModelOpening_Plane());

            BuildingModel buildingModel = new();
            Assert.True(buildingModel.Assign(faceFloor, opening_1));
            Assert.True(buildingModel.Assign(faceFloor, opening_2));

            Assert.True(buildingModel.Unassign(faceFloor, opening_2));

            List<IOpening>? openings = buildingModel.GetOpenings<IOpening>(faceFloor);
            Assert.NotNull(openings);
            Assert.Single(openings!);
            Assert.Equal(opening_1.Guid, openings![0].Guid);

            OpeningRelation? openingRelation = buildingModel.GetRelation<OpeningRelation>(faceFloor);
            Assert.NotNull(openingRelation);
        }

        /// <summary>
        /// Tests that <see cref="BuildingModel.Unassign(IComponent, IOpening)"/> returns false and keeps the relation unchanged when the opening is not hosted by the component - unassigning an unrelated opening must not destroy the hosted one.
        /// </summary>
        [Fact]
        public void BuildingModelUnassign_Component_Opening_Unrelated()
        {
            FaceFloor faceFloor = new(BuildingModelUnassign_PolygonalFace3D());

            Opening opening_1 = new(BuildingModelOpening_Plane());
            Opening opening_2 = new(BuildingModelOpening_Plane());

            BuildingModel buildingModel = new();
            Assert.True(buildingModel.Assign(faceFloor, opening_1));

            Assert.False(buildingModel.Unassign(faceFloor, opening_2));

            List<IOpening>? openings = buildingModel.GetOpenings<IOpening>(faceFloor);
            Assert.NotNull(openings);
            Assert.Single(openings!);
            Assert.Equal(opening_1.Guid, openings![0].Guid);
        }

        /// <summary>
        /// Tests that <see cref="BuildingModel.GetOpenings{TOpening}(IComponent)"/> returns detached clones - two consecutive calls never hand out the same instance, so mutating a returned opening cannot reach the stored one.
        /// </summary>
        [Fact]
        public void BuildingModelGetOpenings_Clone()
        {
            FaceFloor faceFloor = new(BuildingModelUnassign_PolygonalFace3D());

            Opening opening = new(BuildingModelOpening_Plane());

            BuildingModel buildingModel = new();
            Assert.True(buildingModel.Assign(faceFloor, opening));

            List<IOpening>? openings_1 = buildingModel.GetOpenings<IOpening>(faceFloor);
            Assert.NotNull(openings_1);
            Assert.Single(openings_1!);

            List<IOpening>? openings_2 = buildingModel.GetOpenings<IOpening>(faceFloor);
            Assert.NotNull(openings_2);
            Assert.Single(openings_2!);

            Assert.NotSame(openings_1![0], openings_2![0]);

            List<IOpening>? openings_3 = buildingModel.GetOpenings<IOpening>(faceFloor);
            Assert.NotNull(openings_3);
            Assert.Single(openings_3!);
            Assert.NotSame(openings_1[0], openings_3![0]);
        }
    }
}
