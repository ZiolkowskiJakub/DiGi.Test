using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.CityGML.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;
using DiGi.GIS.Classes;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DiGi.GIS.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Reference of the three storey LOD2 building of county 55417 reported by issue 3 of DiGi.Geometry, which lost a roof surface and the wall sections beside it when the model was cut into storeys.
        /// </summary>
        private const string reference_55417 = "38F62226-C70F-F520-E053-CA2BA8C0BE14";

        /// <summary>
        /// Tests that the fixtures of the building reported by issue 3 of DiGi.Geometry hold the geometry the other facts of this file are built on.
        /// <para>The building carries fifty boundary surfaces - thirty seven walls, twelve roofs and one ground surface - of about four thousand eight hundred and fifty square metres in total, and the 2D building pairing with it names three storeys. Its surfaces meet within <see cref="Constants.Tolerance.Enclosure"/> rather than within <see cref="Constants.Tolerance.Coordinate"/>, which is what puts the storey split on the coarse tolerance the defect needed.</para>
        /// </summary>
        [Fact]
        public void CityGML_55417_Parses()
        {
            Building building = CityGML_Building_55417();

            Polyhedron? polyhedron = CityGML.Query.Polyhedron(building);
            Assert.NotNull(polyhedron);
            Assert.Equal(50, polyhedron.Count);

            double area = Area(PolygonalFace3Ds(polyhedron));
            Assert.True(System.Math.Abs(area - 4853.81) < 0.5, $"The fixture carries {area} square metres instead of 4853.81.");

            Assert.Equal(Constants.Tolerance.Enclosure, polyhedron.ClosingTolerance([Constants.Tolerance.Coordinate, 0.02, Constants.Tolerance.Enclosure, 0.1]));

            Building2D building2D = Building2D_55417();
            Assert.Equal(reference_55417, building2D.Reference);
            Assert.Equal(3, building2D.Storeys);
        }

        /// <summary>
        /// Tests that the storey split of the building reported by issue 3 of DiGi.Geometry keeps every boundary surface it was given.
        /// <para>The split used to drop a roof surface of about a hundred and five square metres together with the wall sections beside it, leaving a hole into the interior of the model. A cut only ever adds surface - it divides a face into pieces which sum back to it and lays a floor on the cutting plane - so no plane of the building may come out of a cut carrying less area than it went in with. The fact measures that plane by plane, which catches a dropped flat roof or a dropped wall as well as the sloped roof of the report.</para>
        /// <para>The cause was <see cref="Geometry.Planar.Query.Split(IEnumerable{Geometry.Planar.Classes.Segment2D}, double)"/> handing back the raw ends of a segment no cut crossed, so two edges meeting at a corner kept the two readings the source recorded for it and the ring assembled on the cutting plane never closed.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_FromCityGML_55417_SplitKeepsSurfaces()
        {
            Building building = CityGML_Building_55417();
            Building2D building2D = Building2D_55417();

            BuildingModel? buildingModel_Uncut = Create.BuildingModel(building, Constants.Tolerance.Enclosure);
            Assert.NotNull(buildingModel_Uncut);

            List<IPolygonalFace3D> polygonalFace3Ds_Uncut = Surface3Ds(buildingModel_Uncut);
            Assert.Equal(50, polygonalFace3Ds_Uncut.Count);

            BuildingModel? buildingModel = Create.BuildingModel(building, building2D);
            Assert.NotNull(buildingModel);

            List<IPolygonalFace3D> polygonalFace3Ds = Surface3Ds(buildingModel);

            Dictionary<string, double> areas_Uncut = AreasByPlane(polygonalFace3Ds_Uncut);
            Dictionary<string, double> areas = AreasByPlane(polygonalFace3Ds);

            foreach (KeyValuePair<string, double> keyValuePair in areas_Uncut)
            {
                // A cut moves the corners it lands on by up to the tolerance it is given, so a plane comes back a
                // fraction of a square metre off. The defect drops a whole plane, so one percent is slack enough to
                // absorb the one without hiding the other.
                double slack = System.Math.Max(Constants.Tolerance.Enclosure, keyValuePair.Value * 0.01);

                Assert.True(areas.TryGetValue(keyValuePair.Key, out double area), $"The plane {keyValuePair.Key} carrying {keyValuePair.Value} square metres is missing from the cut model.");
                Assert.True(area >= keyValuePair.Value - slack, $"The plane {keyValuePair.Key} carries {area} square metres in the cut model against the {keyValuePair.Value} it was given.");
            }

            Assert.True(Area(polygonalFace3Ds) >= Area(polygonalFace3Ds_Uncut), "The cut model carries less surface than the uncut one.");
        }

        /// <summary>
        /// Loads the LOD2 building of county 55417 reported by issue 3 of DiGi.Geometry.
        /// </summary>
        /// <returns>The building of the "55417_38F62226_Building.json" fixture.</returns>
        private static Building CityGML_Building_55417()
        {
            List<Building>? buildings = Core.Convert.ToDiGi<Building>(Text_55417("55417_38F62226_Building.json"));

            Assert.NotNull(buildings);
            Assert.Single(buildings);

            return buildings[0];
        }

        /// <summary>
        /// Loads the 2D building pairing with the LOD2 building of county 55417 reported by issue 3 of DiGi.Geometry.
        /// </summary>
        /// <returns>The 2D building of the "55417_38F62226_Building2D.json" fixture.</returns>
        private static Building2D Building2D_55417()
        {
            List<Building2D>? building2Ds = Core.Convert.ToDiGi<Building2D>(Text_55417("55417_38F62226_Building2D.json"));

            Assert.NotNull(building2Ds);
            Assert.Single(building2Ds);

            return building2Ds[0];
        }

        /// <summary>
        /// Reads a fixture of the building of county 55417 reported by issue 3 of DiGi.Geometry.
        /// </summary>
        /// <param name="fileName">The name of the fixture file.</param>
        /// <returns>The contents of the fixture file.</returns>
        private static string Text_55417(string fileName)
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), fileName);

            Assert.False(string.IsNullOrWhiteSpace(path));
            Assert.True(File.Exists(path));

            return File.ReadAllText(path!);
        }

        /// <summary>
        /// Takes the faces of a polyhedron.
        /// </summary>
        /// <param name="polyhedron">The polyhedron to be read.</param>
        /// <returns>The faces of the polyhedron.</returns>
        private static List<IPolygonalFace3D> PolygonalFace3Ds(Polyhedron polyhedron)
        {
            List<IPolygonalFace3D> result = [];
            for (int i = 0; i < polyhedron.Count; i++)
            {
                if (polyhedron[i] is IPolygonalFace3D polygonalFace3D)
                {
                    result.Add(polygonalFace3D);
                }
            }

            return result;
        }

        /// <summary>
        /// Takes the surface of every component of a building model, which is the geometry the model is drawn from.
        /// </summary>
        /// <param name="buildingModel">The building model to be read.</param>
        /// <returns>The surfaces of the components of the model.</returns>
        private static List<IPolygonalFace3D> Surface3Ds(BuildingModel buildingModel)
        {
            List<IPolygonalFace3D> result = [];

            List<IComponent>? components = buildingModel.GetComponents<IComponent>();
            if (components is null)
            {
                return result;
            }

            for (int i = 0; i < components.Count; i++)
            {
                if (DiGi.Analytical.Building.Query.Surface3D(components[i]) is IPolygonalFace3D polygonalFace3D)
                {
                    result.Add(polygonalFace3D);
                }
            }

            return result;
        }

        /// <summary>
        /// Sums the areas of a collection of faces.
        /// </summary>
        /// <param name="polygonalFace3Ds">The faces to be measured.</param>
        /// <returns>The total area of the faces.</returns>
        private static double Area(IEnumerable<IPolygonalFace3D> polygonalFace3Ds)
        {
            double result = 0;
            foreach (IPolygonalFace3D polygonalFace3D in polygonalFace3Ds)
            {
                result += polygonalFace3D.GetArea();
            }

            return result;
        }

        /// <summary>
        /// Groups a collection of faces by the plane they lie on and sums the area carried by each group.
        /// <para>The key is taken from the normal and the offset of the plane, the normal being flipped to a canonical direction first so that a face and a face turned the other way still land on the same plane. Both are rounded to a centimetre, the coordinate precision of the national 3D building model.</para>
        /// </summary>
        /// <param name="polygonalFace3Ds">The faces to be grouped.</param>
        /// <returns>The total area carried by each plane, keyed by the plane.</returns>
        private static Dictionary<string, double> AreasByPlane(IEnumerable<IPolygonalFace3D> polygonalFace3Ds)
        {
            Dictionary<string, double> result = [];

            foreach (IPolygonalFace3D polygonalFace3D in polygonalFace3Ds)
            {
                if (polygonalFace3D.Plane is not Plane plane || plane.Normal is not Vector3D normal || plane.Origin is not Point3D origin)
                {
                    continue;
                }

                double x = normal.X;
                double y = normal.Y;
                double z = normal.Z;

                if (z < -Constants.Tolerance.Coordinate || (System.Math.Abs(z) <= Constants.Tolerance.Coordinate && (y < -Constants.Tolerance.Coordinate || (System.Math.Abs(y) <= Constants.Tolerance.Coordinate && x < 0))))
                {
                    x = -x;
                    y = -y;
                    z = -z;
                }

                double offset = (x * origin.X) + (y * origin.Y) + (z * origin.Z);

                string key = $"{x:F2}:{y:F2}:{z:F2}:{offset:F2}";

                result.TryGetValue(key, out double area);
                result[key] = area + polygonalFace3D.GetArea();
            }

            return result;
        }
    }
}
