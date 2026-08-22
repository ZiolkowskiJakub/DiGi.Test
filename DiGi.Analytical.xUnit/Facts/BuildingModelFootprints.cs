using DiGi.Analytical.Building;
using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using DiGi.Geometry.Spatial.Classes;
using System.Diagnostics;
using System.Reflection;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that the outline of a set of components is the ground they cover between them, with the walls left out and the horizontal components merged into one face.
        /// <para>The walls are left out by the area they cover seen from above rather than by their type, which is what a box standing on a 10 x 10 floor with a 10 x 10 roof three metres above it proves: the outline is one face of 100, not three overlapping ones and not six.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_Footprints_Components()
        {
            List<IComponent> components = [];

            components.Add(new FaceFloor(BuildingModel_Footprints_Face([new Point3D(0, 0, 0), new Point3D(10, 0, 0), new Point3D(10, 10, 0), new Point3D(0, 10, 0)])));
            components.Add(new SurfaceRoof(BuildingModel_Footprints_Face([new Point3D(0, 0, 3), new Point3D(10, 0, 3), new Point3D(10, 10, 3), new Point3D(0, 10, 3)])));

            components.Add(new SurfaceWall(BuildingModel_Footprints_Face([new Point3D(0, 0, 0), new Point3D(10, 0, 0), new Point3D(10, 0, 3), new Point3D(0, 0, 3)])));
            components.Add(new SurfaceWall(BuildingModel_Footprints_Face([new Point3D(10, 0, 0), new Point3D(10, 10, 0), new Point3D(10, 10, 3), new Point3D(10, 0, 3)])));
            components.Add(new SurfaceWall(BuildingModel_Footprints_Face([new Point3D(10, 10, 0), new Point3D(0, 10, 0), new Point3D(0, 10, 3), new Point3D(10, 10, 3)])));
            components.Add(new SurfaceWall(BuildingModel_Footprints_Face([new Point3D(0, 10, 0), new Point3D(0, 0, 0), new Point3D(0, 0, 3), new Point3D(0, 10, 3)])));

            List<PolygonalFace2D>? polygonalFace2Ds = Building.Query.Footprints(components);
            Assert.NotNull(polygonalFace2Ds);
            Assert.Single(polygonalFace2Ds);
            Assert.Equal(100, polygonalFace2Ds[0].GetArea(), 6);

            Assert.Null(Building.Query.Footprints((IEnumerable<IComponent>?)null));
            Assert.Empty(Building.Query.Footprints(new List<IComponent>())!);
        }

        /// <summary>
        /// Tests the outline of the building models stored in the shared fixture of valid geometry.
        /// <para>Every face of an outline has to cover ground, has to stay within the plan bounds of the model it came from, and the faces of one model together may not cover more than those bounds, which is what the joining of the projected components guarantees.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_Footprints()
        {
            List<BuildingModel>? buildingModels = BuildingModel_Footprints_File("BuildingModel_ValidGeometry.json");
            if (buildingModels is null || buildingModels.Count == 0)
            {
                return;
            }

            int count = 0;
            foreach (BuildingModel buildingModel in buildingModels)
            {
                BoundingBox3D? boundingBox3D = buildingModel.GetBoundingBox();
                if (boundingBox3D is null)
                {
                    continue;
                }

                List<PolygonalFace2D>? polygonalFace2Ds = buildingModel.Footprints();
                Assert.NotNull(polygonalFace2Ds);

                if (polygonalFace2Ds.Count == 0)
                {
                    continue;
                }

                count++;

                double area = 0;
                foreach (PolygonalFace2D polygonalFace2D in polygonalFace2Ds)
                {
                    Assert.True(polygonalFace2D.GetArea() >= DiGi.Core.Constants.Tolerance.Distance, "An outline was returned for a component covering no ground.");

                    BoundingBox2D? boundingBox2D = polygonalFace2D.GetBoundingBox();
                    Assert.NotNull(boundingBox2D);

                    Assert.True(boundingBox2D.Min.X >= boundingBox3D.Min.X - DiGi.Core.Constants.Tolerance.MacroDistance, "An outline reached outside the plan bounds of its building.");
                    Assert.True(boundingBox2D.Min.Y >= boundingBox3D.Min.Y - DiGi.Core.Constants.Tolerance.MacroDistance, "An outline reached outside the plan bounds of its building.");
                    Assert.True(boundingBox2D.Max.X <= boundingBox3D.Max.X + DiGi.Core.Constants.Tolerance.MacroDistance, "An outline reached outside the plan bounds of its building.");
                    Assert.True(boundingBox2D.Max.Y <= boundingBox3D.Max.Y + DiGi.Core.Constants.Tolerance.MacroDistance, "An outline reached outside the plan bounds of its building.");

                    area += polygonalFace2D.GetArea();
                }

                double area_Bounds = (boundingBox3D.Max.X - boundingBox3D.Min.X) * (boundingBox3D.Max.Y - boundingBox3D.Min.Y);
                Assert.True(area > 0, "The outline of a building covers no ground at all.");
                Assert.True(area <= area_Bounds * (1 + 1e-6), $"The outline of a building covers {area}, more than the {area_Bounds} of its plan bounds; the projected components were not joined.");
            }

            Assert.True(count > 0, "Not one of the stored building models produced an outline.");
        }

        /// <summary>
        /// Tests that the corrupt geometry found in the database does not put a not-a-number corner into an outline, nor throw while being projected.
        /// <para>Companion of the triangulation guard on the same fixture: a component stored with a not-a-number plane normal projects to nothing usable, and it has to be left out rather than take the outline of its whole building with it.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_Footprints_NaN()
        {
            List<BuildingModel>? buildingModels = BuildingModel_Footprints_File("BuildingModel_NaNGeometry.json");
            if (buildingModels is null || buildingModels.Count == 0)
            {
                return;
            }

            List<PolygonalFace2D>? polygonalFace2Ds = buildingModels.Footprints();
            Assert.NotNull(polygonalFace2Ds);

            foreach (PolygonalFace2D polygonalFace2D in polygonalFace2Ds)
            {
                List<IPolygonal2D>? edges = polygonalFace2D.Edges;
                Assert.NotNull(edges);

                foreach (IPolygonal2D edge in edges)
                {
                    Assert.True(Geometry.Planar.Query.IsValid(edge.GetPoints()), "A not-a-number corner reached an outline.");
                }
            }
        }

        /// <summary>
        /// Tests that the outlines of a collection of building models are the outlines of each of them, kept apart rather than joined across buildings.
        /// </summary>
        [Fact]
        public void BuildingModel_Footprints_Batch()
        {
            List<BuildingModel>? buildingModels = BuildingModel_Footprints_File("BuildingModel_ValidGeometry.json");
            if (buildingModels is null || buildingModels.Count == 0)
            {
                return;
            }

            int count = 0;
            foreach (BuildingModel buildingModel in buildingModels)
            {
                count += buildingModel.Footprints()?.Count ?? 0;
            }

            List<PolygonalFace2D>? polygonalFace2Ds = buildingModels.Footprints();
            Assert.NotNull(polygonalFace2Ds);
            Assert.Equal(count, polygonalFace2Ds.Count);

            Assert.Null(((BuildingModel?)null).Footprints());
            Assert.Null(((IEnumerable<BuildingModel>?)null).Footprints());
            Assert.Empty(new List<BuildingModel>().Footprints()!);
        }

        /// <summary>
        /// Measures taking the outlines of the number of building models the 3D view of the largest area it offers has to carry.
        /// <para>The cost sits mostly in reaching the components: <see cref="BuildingModel.GetComponents{TComponent}()"/> hands out a clone of every component it returns.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_Footprints_Performance()
        {
            List<BuildingModel>? buildingModels = BuildingModel_Footprints_File("BuildingModel_ValidGeometry.json");
            if (buildingModels is null || buildingModels.Count == 0)
            {
                return;
            }

            _ = buildingModels[0].Footprints();

            List<BuildingModel> buildingModels_Temp = [];
            while (buildingModels_Temp.Count < 200)
            {
                foreach (BuildingModel buildingModel in buildingModels)
                {
                    BuildingModel? buildingModel_Temp = DiGi.Core.Query.Clone(buildingModel) as BuildingModel;
                    if (buildingModel_Temp is null)
                    {
                        continue;
                    }

                    buildingModels_Temp.Add(buildingModel_Temp);
                }

                if (buildingModels_Temp.Count == 0)
                {
                    return;
                }
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            List<PolygonalFace2D>? polygonalFace2Ds = buildingModels_Temp.Footprints();
            stopwatch.Stop();

            Assert.NotNull(polygonalFace2Ds);

            string? path_Reports = DiGi.Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly());
            if (!string.IsNullOrWhiteSpace(path_Reports))
            {
                File.WriteAllText(Path.Combine(path_Reports, "BuildingModel_Footprints_Performance.txt"), $"Building models {buildingModels_Temp.Count}, outlines {polygonalFace2Ds.Count}, elapsed {stopwatch.ElapsedMilliseconds} ms.");
            }

            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Taking the outlines of {buildingModels_Temp.Count} building models failed the threshold! Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Tests cutting the real terrain with the real building model 5072294 (county 12668).
        /// </summary>
        [Fact]
        public void BuildingModel_Footprints_RealBuilding()
        {
            string? path_Building = DiGi.Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "buildingmodel_5072294.json");
            string? path_Terrain = DiGi.Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "terrain_response.json");

            Assert.NotNull(path_Building);
            Assert.NotNull(path_Terrain);

            List<BuildingModel>? buildingModels = DiGi.Core.Convert.ToDiGi<BuildingModel>((DiGi.Core.Classes.Path)path_Building);
            List<Mesh3D>? mesh3Ds = DiGi.Core.Convert.ToDiGi<Mesh3D>((DiGi.Core.Classes.Path)path_Terrain);

            Assert.NotNull(buildingModels);
            Assert.NotEmpty(buildingModels);
            Assert.NotNull(mesh3Ds);
            Assert.NotEmpty(mesh3Ds);

            BuildingModel buildingModel = buildingModels[0];
            Mesh3D mesh3D = mesh3Ds[0];

            List<PolygonalFace2D>? footprints = buildingModel.Footprints();
            Assert.NotNull(footprints);
            Assert.NotEmpty(footprints);

            List<PolygonalFace2D>? footprints_Offset = Geometry.Planar.Query.Offset(footprints, 0.05);
            Assert.NotNull(footprints_Offset);
            Assert.NotEmpty(footprints_Offset);

            double area_Before = mesh3D.GetArea();
            int triangles_Before = mesh3D.TrianglesCount;

            Mesh3D? mesh3D_Cut = Geometry.Spatial.Query.Difference(mesh3D, footprints_Offset);
            Assert.NotNull(mesh3D_Cut);

            double area_After = mesh3D_Cut.GetArea();
            int triangles_After = mesh3D_Cut.TrianglesCount;

            string? path_Reports = DiGi.Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly());
            if (!string.IsNullOrWhiteSpace(path_Reports))
            {
                File.WriteAllText(Path.Combine(path_Reports, "BuildingModel_5072294_Cuts.txt"),
                    $"Footprints count: {footprints.Count}, Footprint area: {footprints[0].GetArea():F3}.\n" +
                    $"Footprints offset (0.05m) area: {footprints_Offset[0].GetArea():F3}.\n" +
                    $"Terrain before: {area_Before:F2} m2 ({triangles_Before} triangles).\n" +
                    $"Terrain after: {area_After:F2} m2 ({triangles_After} triangles).\n" +
                    $"Building BoundingBox: {buildingModel.GetBoundingBox()?.ToString()}\n" +
                    $"Terrain BoundingBox: {mesh3D.GetBoundingBox()?.ToString()}\n" +
                    $"Cut Mesh BoundingBox: {mesh3D_Cut.GetBoundingBox()?.ToString()}");
            }
        }

        private static List<BuildingModel>? BuildingModel_Footprints_File(string fileName)
        {
            string? path = DiGi.Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), fileName);
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }

            return DiGi.Core.Convert.ToDiGi<BuildingModel>((DiGi.Core.Classes.Path)path);
        }

        private static PolygonalFace3D BuildingModel_Footprints_Face(List<Point3D> point3Ds)
        {
            Polygon3D? polygon3D = Geometry.Spatial.Create.Polygon3D(point3Ds);
            Assert.NotNull(polygon3D);

            PolygonalFace3D? polygonalFace3D = Geometry.Spatial.Create.PolygonalFace3D(polygon3D);
            Assert.NotNull(polygonalFace3D);

            return polygonalFace3D;
        }
    }
}
