using DiGi.Analytical.Building;
using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.CityGML.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;
using DiGi.GIS.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Counts the components of the given building model whose geometry does not sit on a finite plane.
        /// </summary>
        /// <param name="buildingModel">The building model to be inspected.</param>
        /// <returns>The number of components carrying a non-finite plane.</returns>
        private static int Components_NonFinitePlane(BuildingModel? buildingModel)
        {
            List<IComponent>? components = buildingModel?.GetComponents<IComponent>();
            if (components is null)
            {
                return 0;
            }

            int result = 0;
            foreach (IComponent component in components)
            {
                if (component.Surface3D() is not IPolygonalFace3D polygonalFace3D)
                {
                    continue;
                }

                Plane? plane = polygonalFace3D.Plane;

                Vector3D? normal = plane?.Normal;
                Point3D? origin = plane?.Origin;

                if (normal is null || origin is null)
                {
                    result++;
                    continue;
                }

                if (double.IsNaN(normal.X) || double.IsNaN(normal.Y) || double.IsNaN(normal.Z)
                    || double.IsNaN(origin.X) || double.IsNaN(origin.Y) || double.IsNaN(origin.Z))
                {
                    result++;
                }
            }

            return result;
        }

        /// <summary>
        /// Tests that no component of a building model created from real LOD2 geometry sits on a non-finite plane.
        /// <para>Guards the defect that filled the database with unusable building models: the boundary surfaces of the national 3D building model are survey geometry and are never planar to a micrometre, and deriving the normal of such a surface used to yield a degenerate vector, which turned the plane of the component - and every point projected onto it - into NaN. The models were stored regardless, because nothing between the creator and the database validates coordinates, and the corruption only surfaced much later as an ArgumentException from NetTopologySuite while the stored model was being rendered.</para>
        /// <para>The three buildings of the fixture are exactly the ones whose surfaces are not planar within <see cref="Core.Constants.Tolerance.Distance"/>, so they are the ones that used to be lost. The plane of every component has to be finite, at the default tolerance and at the finer one.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_FromCityGMLFile_2476_FinitePlanes()
        {
            Dictionary<string, Building> buildings = CityGML_Buildings_2476();

            string[] references = [reference_NonPlanar_1, reference_NonPlanar_2, reference_NonPlanar_3, reference_Residential_1, reference_Residential_2, reference_NonResidential];

            Dictionary<string, Building2D> building2Ds = Building2Ds_2476(references);

            for (int i = 0; i < references.Length; i++)
            {
                Building building = buildings[references[i]];
                Building2D building2D = building2Ds[references[i]];

                BuildingModel? buildingModel = Create.BuildingModel(building, building2D);
                Assert.NotNull(buildingModel);

                List<IComponent>? components = buildingModel.GetComponents<IComponent>();
                Assert.NotNull(components);
                Assert.NotEmpty(components);

                Assert.Equal(0, Components_NonFinitePlane(buildingModel));
                Assert.True(buildingModel.IsValid());

                // The storey split is the only step this overload adds over the single argument one, so the
                // geometry has to survive both the split and the path that leaves the model whole.
                BuildingModel? buildingModel_Fine = Create.BuildingModel(building, building2D, tolerance: Core.Constants.Tolerance.Distance);
                Assert.NotNull(buildingModel_Fine);
                Assert.Equal(0, Components_NonFinitePlane(buildingModel_Fine));
                Assert.True(buildingModel_Fine.IsValid());

                // The geometry is reached through the single argument overload, so it has to hold on its own.
                BuildingModel? buildingModel_Single = Create.BuildingModel(building);
                Assert.NotNull(buildingModel_Single);
                Assert.Equal(0, Components_NonFinitePlane(buildingModel_Single));
                Assert.True(buildingModel_Single.IsValid());
            }
        }

        /// <summary>
        /// Tests that a building model holds one component carrying geometry for every boundary surface of the CityGML building it was created from.
        /// <para>Nothing may be dropped on the way: <see cref="Convert.ToAnalytical(CityGML.Interfaces.ISurface)"/> returns null for a surface type it does not recognise and <see cref="Create.Component(IPolygonalFace3D, Polyhedron, double)"/> returns null for a face without a plane, and the creator used to move on to the next surface in both cases. A component is also stored under its own identifier, so two components sharing one identifier collapse into a single entry.</para>
        /// <para>Measured against the source of the national 3D building model this invariant holds exactly - 2,182,201 boundary surfaces produced 2,182,201 components across three LOD2 and two LOD1 counties - and this fact keeps it that way.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_FromCityGMLFile_2476_ComponentPerSurface()
        {
            Dictionary<string, Building> buildings = CityGML_Buildings_2476();
            Assert.NotEmpty(buildings);

            foreach (KeyValuePair<string, Building> keyValuePair in buildings)
            {
                Building building = keyValuePair.Value;

                List<CityGML.Interfaces.ISurface>? surfaces = building.Surfaces?.ToList();
                Assert.NotNull(surfaces);
                Assert.NotEmpty(surfaces);

                BuildingModel? buildingModel = Create.BuildingModel(building);
                Assert.NotNull(buildingModel);

                List<IComponent>? components = buildingModel.GetComponents<IComponent>();
                Assert.NotNull(components);

                Assert.True(surfaces.Count == components.Count, $"Building {keyValuePair.Key} has {surfaces.Count} boundary surfaces but produced {components.Count} components.");

                // Every component has to carry usable geometry, otherwise the count alone means nothing.
                foreach (IComponent component in components)
                {
                    Assert.True(component.Surface3D() is IPolygonalFace3D, $"Building {keyValuePair.Key} produced a component without polygonal geometry.");
                }

                Assert.Equal(0, Components_NonFinitePlane(buildingModel));
            }
        }
    }
}
