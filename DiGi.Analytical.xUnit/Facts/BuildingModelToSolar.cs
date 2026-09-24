using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Analytical.Building.Solar;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Solar.Classes;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that the components of the surrounding building models are added to the shading model as shading-only casters, while the components of the analysed model stay receivers.
        /// <para>A 10 x 10 x 3 box and a neighbour box 10 m away: six receivers from the analysed box, six shading-only elements from the neighbour.</para>
        /// </summary>
        [Fact]
        public void ToSolar_Surroundings_ShadingOnly()
        {
            BuildingModel buildingModel = BuildingModel_ToSolar_Box(0);
            BuildingModel buildingModel_Neighbour = BuildingModel_ToSolar_Box(20);

            ShadingModel? shadingModel = buildingModel.ToSolar([buildingModel_Neighbour]);
            Assert.NotNull(shadingModel);

            List<ShadingElement>? shadingElements_Receiver = shadingModel.GetShadingElements<ShadingElement>(false);
            List<ShadingElement>? shadingElements_ShadingOnly = shadingModel.GetShadingElements<ShadingElement>(true);
            Assert.NotNull(shadingElements_Receiver);
            Assert.NotNull(shadingElements_ShadingOnly);

            HashSet<string> references_Main = BuildingModel_ToSolar_References(buildingModel);
            HashSet<string> references_Neighbour = BuildingModel_ToSolar_References(buildingModel_Neighbour);

            Assert.Equal(6, shadingElements_Receiver.Count);
            Assert.All(shadingElements_Receiver, x => Assert.Contains(x.Reference!, references_Main));

            Assert.Equal(6, shadingElements_ShadingOnly.Count);
            Assert.All(shadingElements_ShadingOnly, x => Assert.Contains(x.Reference!, references_Neighbour));
        }

        /// <summary>
        /// Tests that a receiver filter limits the receivers to the accepted components of the analysed model, and that the rejected components still cast shade.
        /// </summary>
        [Fact]
        public void ToSolar_ReceiverFilter_RoofOnly()
        {
            BuildingModel buildingModel = BuildingModel_ToSolar_Box(0);

            ShadingModel? shadingModel = buildingModel.ToSolar(null, x => x is IRoof);
            Assert.NotNull(shadingModel);

            List<ShadingElement>? shadingElements_Receiver = shadingModel.GetShadingElements<ShadingElement>(false);
            List<ShadingElement>? shadingElements_ShadingOnly = shadingModel.GetShadingElements<ShadingElement>(true);
            Assert.NotNull(shadingElements_Receiver);
            Assert.NotNull(shadingElements_ShadingOnly);

            List<IRoof>? roofs = buildingModel.GetComponents<IRoof>();
            Assert.NotNull(roofs);
            string? reference_Roof = new Core.Classes.GuidReference(Assert.Single(roofs)).ToString();
            Assert.NotNull(reference_Roof);

            ShadingElement shadingElement_Receiver = Assert.Single(shadingElements_Receiver);
            Assert.Equal(reference_Roof, shadingElement_Receiver.Reference);

            Assert.Equal(5, shadingElements_ShadingOnly.Count);
            Assert.All(shadingElements_ShadingOnly, x => Assert.NotEqual(reference_Roof, x.Reference));
        }

        /// <summary>
        /// Tests that the analysed model passed again inside the surroundings, or a surrounding model passed twice, adds no element twice.
        /// </summary>
        [Fact]
        public void ToSolar_Surroundings_ContainsMain_NoDuplicates()
        {
            BuildingModel buildingModel = BuildingModel_ToSolar_Box(0);
            BuildingModel buildingModel_Neighbour = BuildingModel_ToSolar_Box(20);

            ShadingModel? shadingModel = buildingModel.ToSolar([buildingModel, buildingModel_Neighbour, buildingModel_Neighbour]);
            Assert.NotNull(shadingModel);

            Assert.Equal(6, shadingModel.GetShadingElements<ShadingElement>(false)?.Count);
            Assert.Equal(6, shadingModel.GetShadingElements<ShadingElement>(true)?.Count);
        }

        /// <summary>
        /// Tests that the shades of the analysed model reach the shading model as shading-only casters.
        /// <para>Guard for DiGi.Analytical#5: <see cref="IShade"/> is not an <see cref="IComponent"/>, so the conversion used to iterate the components only and drop every shade.</para>
        /// </summary>
        [Fact]
        public void ToSolar_Shades_Included()
        {
            BuildingModel buildingModel = BuildingModel_ToSolar_Box(0);

            SurfaceShade surfaceShade = new(BuildingModel_Footprints_Face([new Point3D(0, -2, 3), new Point3D(10, -2, 3), new Point3D(10, 0, 3), new Point3D(0, 0, 3)]));
            Assert.True(buildingModel.Update(surfaceShade));

            List<IShade>? shades = buildingModel.GetShades<IShade>();
            Assert.NotNull(shades);
            Assert.Equal(surfaceShade.Guid, Assert.Single(shades).Guid);

            ShadingModel? shadingModel = buildingModel.ToSolar();
            Assert.NotNull(shadingModel);

            Assert.Equal(6, shadingModel.GetShadingElements<ShadingElement>(false)?.Count);

            List<ShadingElement>? shadingElements_ShadingOnly = shadingModel.GetShadingElements<ShadingElement>(true);
            Assert.NotNull(shadingElements_ShadingOnly);
            Assert.Equal(new Core.Classes.GuidReference(surfaceShade).ToString(), Assert.Single(shadingElements_ShadingOnly).Reference);
        }

        /// <summary>
        /// Tests the single-argument overload: every component of the model is a receiver, one element per component, and nothing is shading-only.
        /// </summary>
        [Fact]
        public void ToSolar_SingleArgument()
        {
            BuildingModel buildingModel = BuildingModel_ToSolar_Box(0);

            ShadingModel? shadingModel = buildingModel.ToSolar();
            Assert.NotNull(shadingModel);

            List<ShadingElement>? shadingElements_Receiver = shadingModel.GetShadingElements<ShadingElement>(false);
            Assert.NotNull(shadingElements_Receiver);
            Assert.Equal(6, shadingElements_Receiver.Count);
            Assert.Equal(BuildingModel_ToSolar_References(buildingModel).Count, shadingElements_Receiver.Select(x => x.Reference).Distinct().Count());

            Assert.Empty(shadingModel.GetShadingElements<ShadingElement>(true) ?? []);

            Assert.Null(((BuildingModel?)null).ToSolar());
        }

        private static BuildingModel BuildingModel_ToSolar_Box(double x)
        {
            BuildingModel result = new();

            List<IComponent> components =
            [
                new FaceFloor(BuildingModel_Footprints_Face([new Point3D(x, 0, 0), new Point3D(x + 10, 0, 0), new Point3D(x + 10, 10, 0), new Point3D(x, 10, 0)])),
                new SurfaceRoof(BuildingModel_Footprints_Face([new Point3D(x, 0, 3), new Point3D(x + 10, 0, 3), new Point3D(x + 10, 10, 3), new Point3D(x, 10, 3)])),
                new SurfaceWall(BuildingModel_Footprints_Face([new Point3D(x, 0, 0), new Point3D(x + 10, 0, 0), new Point3D(x + 10, 0, 3), new Point3D(x, 0, 3)])),
                new SurfaceWall(BuildingModel_Footprints_Face([new Point3D(x + 10, 0, 0), new Point3D(x + 10, 10, 0), new Point3D(x + 10, 10, 3), new Point3D(x + 10, 0, 3)])),
                new SurfaceWall(BuildingModel_Footprints_Face([new Point3D(x + 10, 10, 0), new Point3D(x, 10, 0), new Point3D(x, 10, 3), new Point3D(x + 10, 10, 3)])),
                new SurfaceWall(BuildingModel_Footprints_Face([new Point3D(x, 10, 0), new Point3D(x, 0, 0), new Point3D(x, 0, 3), new Point3D(x, 10, 3)])),
            ];

            foreach (IComponent component in components)
            {
                Assert.True(result.Update(component));
            }

            return result;
        }

        private static HashSet<string> BuildingModel_ToSolar_References(BuildingModel buildingModel)
        {
            List<IComponent>? components = buildingModel.GetComponents<IComponent>();
            Assert.NotNull(components);

            HashSet<string> result = [];
            foreach (IComponent component in components)
            {
                string? reference = new Core.Classes.GuidReference(component).ToString();
                Assert.NotNull(reference);
                result.Add(reference);
            }

            return result;
        }
    }
}
