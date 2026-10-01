using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.GIS.WebAPI.UI.Classes;
using DiGi.GIS.WebAPI.UI.ViewModels;
using DiGi.GLTF.Analytical;
using DiGi.GLTF.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the solar radiation view the Building Viewer applies in place: every surface names a node of the Building Viewer scene of the same building (<c>ToGLTF_GLTFNodes</c> with the root <c>PostgreSQL.Create.Reference(buildingModel, null, countyId)</c>), takes the colour of <see cref="Query.SolarIrradiationColor(double)"/> for its irradiation and carries its <see cref="SurfaceSolarRadiationResult"/>; the floor, which receives nothing, is left out.
        /// <para>Without a root the references fall back to the component's own unique reference, as the viewer conversion does. A missing building or missing results give no view.</para>
        /// <para>Medium test (2.1 s): runs when DIGI_TEST_MAX_DURATION is Medium (the default) or Long.</para>
        /// </summary>
        [MediumFact]
        public void Create_SolarRadiationViewModel()
        {
            BuildingModel buildingModel = SolarFixture_Box();
            List<SurfaceSolarRadiationResult>? surfaceSolarRadiationResults = buildingModel.SurfaceSolarRadiationResults(null, SolarFixture_EPWFile(), SolarFixture_ShadingSolverOptions());
            Assert.NotNull(surfaceSolarRadiationResults);
            Assert.Equal(5, surfaceSolarRadiationResults.Count);

            IReference? reference = PostgreSQL.Create.Reference(buildingModel, null, 1465);
            Assert.NotNull(reference);

            SolarRadiationViewModel? solarRadiationViewModel = buildingModel.SolarRadiationViewModel(surfaceSolarRadiationResults, reference, 30, "WARSAW, IWEC", "/epwfile/item?x=1&y=2");
            Assert.NotNull(solarRadiationViewModel);
            Assert.Equal(30, solarRadiationViewModel.Radius);
            Assert.Equal("WARSAW, IWEC", solarRadiationViewModel.StationName);
            Assert.Equal("/epwfile/item?x=1&y=2", solarRadiationViewModel.StationUrl);

            // Four walls and the roof; the floor has no result and keeps its appearance in the viewer.
            Assert.Equal(5, solarRadiationViewModel.Surfaces.Count);

            List<GLTFNode>? gLTFNodes = buildingModel.ToGLTF_GLTFNodes(reference);
            Assert.NotNull(gLTFNodes);
            HashSet<string> references_Node = [];
            foreach (GLTFNode gLTFNode in gLTFNodes)
            {
                if (gLTFNode.Reference is not null)
                {
                    references_Node.Add(gLTFNode.Reference);
                }
            }

            Dictionary<string, SurfaceSolarRadiationResult> surfaceSolarRadiationResults_ByReference = [];
            foreach (SurfaceSolarRadiationResult surfaceSolarRadiationResult in surfaceSolarRadiationResults)
            {
                surfaceSolarRadiationResults_ByReference[surfaceSolarRadiationResult.Reference!] = surfaceSolarRadiationResult;
            }

            Dictionary<string, IComponent> components_ByNodeReference = [];
            foreach (IComponent component in buildingModel.GetComponents<IComponent>()!)
            {
                components_ByNodeReference[Core.Create.Reference(reference, Core.Create.UniqueReference(component))!.ToString()!] = component;
            }

            HashSet<string> references_Surface = [];
            foreach (SolarSurfaceViewModel solarSurfaceViewModel in solarRadiationViewModel.Surfaces)
            {
                Assert.Contains(solarSurfaceViewModel.Reference, references_Node);
                Assert.True(references_Surface.Add(solarSurfaceViewModel.Reference));

                IComponent component = components_ByNodeReference[solarSurfaceViewModel.Reference];
                Assert.IsNotType<FaceFloor>(component);

                SurfaceSolarRadiationResult surfaceSolarRadiationResult = surfaceSolarRadiationResults_ByReference[new GuidReference(component).ToString()!];
                Assert.Equal(Query.SolarIrradiationColor(surfaceSolarRadiationResult.Irradiation).Hex(), solarSurfaceViewModel.Color);
                Assert.Matches("^#[0-9a-f]{6}$", solarSurfaceViewModel.Color);

                SurfaceSolarRadiationResult? surfaceSolarRadiationResult_Properties = Core.Convert.ToDiGi<SurfaceSolarRadiationResult>(solarSurfaceViewModel.Properties)?[0];
                Assert.NotNull(surfaceSolarRadiationResult_Properties);
                Assert.Equal(surfaceSolarRadiationResult.Reference, surfaceSolarRadiationResult_Properties.Reference);
                Assert.Equal(surfaceSolarRadiationResult.Irradiation, surfaceSolarRadiationResult_Properties.Irradiation, 9);
            }

            // No root: the component's own unique reference, as ToGLTF_GLTFNodes(null) gives a component node.
            SolarRadiationViewModel? solarRadiationViewModel_NoRoot = buildingModel.SolarRadiationViewModel(surfaceSolarRadiationResults, null, 30, null, null);
            Assert.NotNull(solarRadiationViewModel_NoRoot);
            HashSet<string> references_Unique = [];
            foreach (IComponent component in buildingModel.GetComponents<IComponent>()!)
            {
                references_Unique.Add(Core.Create.UniqueReference(component)!.ToString()!);
            }

            Assert.All(solarRadiationViewModel_NoRoot.Surfaces, x => Assert.Contains(x.Reference, references_Unique));

            Assert.Null(((BuildingModel?)null).SolarRadiationViewModel(surfaceSolarRadiationResults, reference, 30, null, null));
            Assert.Null(buildingModel.SolarRadiationViewModel(null, reference, 30, null, null));
            Assert.Null(buildingModel.SolarRadiationViewModel([], reference, 30, null, null));
        }
    }
}
