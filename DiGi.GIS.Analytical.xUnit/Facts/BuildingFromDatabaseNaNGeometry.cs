using DiGi.Analytical.Building;
using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.CityGML.Classes;
using DiGi.CityGML.Interfaces;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DiGi.GIS.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Reference of the first building of the "Building_NaNGeometry.json" fixture, three of its nine boundary surfaces carrying a not-a-number plane.
        /// </summary>
        private const string reference_NaN_Small = "38F62224-D448-F520-E053-CA2BA8C0BE14";

        /// <summary>
        /// Reference of the second building of the "Building_NaNGeometry.json" fixture, forty nine of its seventy three boundary surfaces carrying a not-a-number plane.
        /// </summary>
        private const string reference_NaN_Large = "38F62226-CD99-F520-E053-CA2BA8C0BE14";

        /// <summary>
        /// Loads the CityGML buildings taken from the database and stored in the shared test files directory.
        /// </summary>
        /// <returns>The buildings of the fixture keyed by their reference, or <see langword="null"/> when the fixture is not available.</returns>
        private static Dictionary<string, Building>? CityGML_Buildings_NaNGeometry()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "Building_NaNGeometry.json");
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }

            List<Building>? buildings = DiGi.Core.Convert.ToDiGi<Building>((DiGi.Core.Classes.Path)path);
            if (buildings is null)
            {
                return null;
            }

            Dictionary<string, Building> result = [];
            foreach (Building building in buildings)
            {
                if (CityGML.Query.Reference(building) is string reference)
                {
                    result[reference] = building;
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether the given vector is missing or carries a non-finite component.
        /// </summary>
        /// <param name="vector3D">The vector to be checked.</param>
        /// <returns><see langword="true"/> when the vector cannot be used; otherwise, <see langword="false"/>.</returns>
        private static bool NonFinite(Vector3D? vector3D)
        {
            if (vector3D is null)
            {
                return true;
            }

            return double.IsNaN(vector3D.X) || double.IsNaN(vector3D.Y) || double.IsNaN(vector3D.Z);
        }

        /// <summary>
        /// Counts the boundary surfaces of the given building whose geometry sits on a non-finite plane.
        /// </summary>
        /// <param name="building">The CityGML building to be inspected.</param>
        /// <returns>The number of surfaces carrying a non-finite plane.</returns>
        private static int Surfaces_NonFinitePlane(Building? building)
        {
            IEnumerable<ISurface>? surfaces = building?.Surfaces;
            if (surfaces is null)
            {
                return 0;
            }

            int result = 0;
            foreach (ISurface surface in surfaces)
            {
                if (surface?.Geometry is IPolygonalFace3D polygonalFace3D && NonFinite(polygonalFace3D.Plane?.Normal))
                {
                    result++;
                }
            }

            return result;
        }

        /// <summary>
        /// Tests that the two CityGML buildings taken from the database still carry the corrupt geometry they were captured for.
        /// <para>Both were read from the <c>building</c> table of the production database through <c>gis/building/itembyreference</c> (county 55417). Their boundary surfaces are stored with a not-a-number plane normal while the plane origin stays finite, which is the shape the whole defect is recognised by: the corruption was introduced while deriving the normal of a boundary surface that is not planar, not by the source coordinates.</para>
        /// <para>This fact pins the fixture. If a regenerated capture no longer carries the corruption the fixture stops guarding anything, and that has to fail loudly rather than pass quietly.</para>
        /// </summary>
        [Fact]
        public void CityGMLBuildings_NaNGeometry_Parse()
        {
            Dictionary<string, Building>? buildings = CityGML_Buildings_NaNGeometry();
            if (buildings is null)
            {
                return;
            }

            Assert.Equal(2, buildings.Count);
            Assert.True(buildings.ContainsKey(reference_NaN_Small), $"Building {reference_NaN_Small} is missing from the fixture.");
            Assert.True(buildings.ContainsKey(reference_NaN_Large), $"Building {reference_NaN_Large} is missing from the fixture.");

            Assert.Equal(3, Surfaces_NonFinitePlane(buildings[reference_NaN_Small]));
            Assert.Equal(49, Surfaces_NonFinitePlane(buildings[reference_NaN_Large]));

            // The origin of a corrupt plane stays finite - only the normal is lost. Anything else would point
            // at a different defect than the one these fixtures were captured for.
            foreach (KeyValuePair<string, Building> keyValuePair in buildings)
            {
                IEnumerable<ISurface>? surfaces = keyValuePair.Value.Surfaces;
                Assert.NotNull(surfaces);

                foreach (ISurface surface in surfaces)
                {
                    if (surface?.Geometry is not IPolygonalFace3D polygonalFace3D || !NonFinite(polygonalFace3D.Plane?.Normal))
                    {
                        continue;
                    }

                    Point3D? origin = polygonalFace3D.Plane?.Origin;
                    Assert.NotNull(origin);
                    Assert.False(double.IsNaN(origin.X) || double.IsNaN(origin.Y) || double.IsNaN(origin.Z), $"Building {keyValuePair.Key} has a corrupt plane whose origin is also NaN.");
                }
            }
        }

        /// <summary>
        /// Tests that a building model created from the corrupt CityGML of the database is produced without throwing, and that no unusable geometry escapes into the mesh.
        /// <para>Regression guard for "System.ArgumentException: 'points must form a closed linestring'", which aborted the rendering of a whole district. NetTopologySuite tests a ring for closure by comparing its first and last coordinate, and NaN never equals NaN, so a correctly closed ring built on a corrupt plane was rejected and the exception took down every building of the scene rather than the single unusable surface.</para>
        /// <para>A surface sitting on a non-finite plane has to report itself as not triangulable, and every surface that does triangulate has to yield finite vertices only. This is the source-data counterpart of <see cref="BuildingModel_NaNGeometry_Triangulate"/>, which guards the same contract for the models already stored in the database.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_FromCityGMLBuildings_NaNGeometry_Triangulate()
        {
            Dictionary<string, Building>? buildings = CityGML_Buildings_NaNGeometry();
            if (buildings is null)
            {
                return;
            }

            foreach (KeyValuePair<string, Building> keyValuePair in buildings)
            {
                BuildingModel? buildingModel = Create.BuildingModel(keyValuePair.Value);
                Assert.NotNull(buildingModel);

                List<IComponent>? components = buildingModel.GetComponents<IComponent>();
                Assert.NotNull(components);
                Assert.NotEmpty(components);

                int count_NonFinite = 0;

                foreach (IComponent component in components)
                {
                    if (component.Surface3D() is not IPolygonalFace3D polygonalFace3D)
                    {
                        continue;
                    }

                    List<Triangle3D>? triangle3Ds = polygonalFace3D.Triangulate(DiGi.Core.Constants.Tolerance.Distance);

                    if (NonFinite(polygonalFace3D.Plane?.Normal))
                    {
                        count_NonFinite++;
                        Assert.True(triangle3Ds is null || triangle3Ds.Count == 0, $"Building {keyValuePair.Key} triangulated a component sitting on a not-a-number plane instead of reporting it as not convertible.");
                        continue;
                    }

                    Assert.NotNull(triangle3Ds);

                    foreach (Triangle3D triangle3D in triangle3Ds)
                    {
                        foreach (Point3D? point3D in new Point3D?[] { triangle3D[0], triangle3D[1], triangle3D[2] })
                        {
                            Assert.NotNull(point3D);
                            Assert.False(double.IsNaN(point3D.X) || double.IsNaN(point3D.Y) || double.IsNaN(point3D.Z), $"Building {keyValuePair.Key} handed out a triangle built from a not-a-number point.");
                        }
                    }
                }

                Assert.True(count_NonFinite > 0, $"Building {keyValuePair.Key} no longer carries a component on a non-finite plane.");
            }
        }
    }
}
