using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Analytical.Classes;
using DiGi.Geometry.Spatial;
using DiGi.GIS.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace DiGi.GIS.Analytical.xUnit
{
    public partial class Facts
    {
        private readonly ITestOutputHelper testOutputHelper;

        public Facts(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Tests that the BuildingModel extension method returns null when the input Building2D is null.
        /// </summary>
        [Fact]
        public void BuildingModel_NullInput()
        {
            Building2D? building2D = null;

            BuildingModel? buildingModel = Create.BuildingModel(building2D);

            Assert.Null(buildingModel);
        }

        /// <summary>
        /// Tests that the BuildingModel extension method returns null when the input Building2D has no PolygonalFace2D geometry.
        /// </summary>
        [Fact]
        public void BuildingModel_NullGeometry()
        {
            string json = "{\"_type\":\"DiGi.GIS.Classes.Building2D,DiGi.GIS\",\"Guid\":\"8531176b-ae1f-4dbc-b864-e71ab24a6af2\",\"Reference\":\"1fc24a0d-8d0c-4e15-b6d2-ea52124f30b7\",\"PolygonalFace2D\":null,\"Storeys\":1,\"BuildingGeneralFunction\":4,\"BuildingSpecificFunctions\":[7],\"BuildingPhase\":0}";

            Building2D? building2D = Core.Convert.ToDiGi<Building2D>(json)?.FirstOrDefault();

            Assert.NotNull(building2D);
            Assert.Null(building2D.PolygonalFace2D);

            BuildingModel? buildingModel = Create.BuildingModel(building2D);

            Assert.Null(buildingModel);
        }

        /// <summary>
        /// Tests that the BuildingModel extension method correctly builds a BuildingModel from a valid Building2D.
        /// </summary>
        [Fact]
        public void BuildingModel_ValidInput()
        {
            string json = "{\"_type\":\"DiGi.GIS.Classes.Building2D,DiGi.GIS\",\"Guid\":\"8531176b-ae1f-4dbc-b864-e71ab24a6af2\",\"Reference\":\"1fc24a0d-8d0c-4e15-b6d2-ea52124f30b7\",\"PolygonalFace2D\":{\"_type\":\"DiGi.Geometry.Planar.Classes.PolygonalFace2D,DiGi.Geometry\",\"ExternalEdge\":{\"_type\":\"DiGi.Geometry.Planar.Classes.Polygon2D,DiGi.Geometry\",\"Points\":[{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":482430.74,\"Y\":559048.76},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":482425.19,\"Y\":559050.29},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":482427.49,\"Y\":559058.66},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":482433.15,\"Y\":559057.01}]},\"InternalEdges\":null},\"Storeys\":2,\"BuildingGeneralFunction\":4,\"BuildingSpecificFunctions\":[7],\"BuildingPhase\":0}";

            Building2D? building2D = Core.Convert.ToDiGi<Building2D>(json)?.FirstOrDefault();

            Assert.NotNull(building2D);

            BuildingModel? buildingModel = Create.BuildingModel(building2D);

            Assert.NotNull(buildingModel);

            List<Space>? spaces = buildingModel.GetSpaces<Space>();
            Assert.NotNull(spaces);
            Assert.True(spaces.Count == 2);

            List<IComponent>? components = buildingModel.GetComponents<IComponent>();
            Assert.NotNull(components);
            Assert.NotEmpty(components);
        }

        /// <summary>
        /// Tests the conversion of Building2D objects from the "2476_GML.gmf" file to BuildingModel objects,
        /// verifying that the geometry and storey count are processed correctly.
        /// </summary>
        [Fact]
        public void BuildingModel_FromGmfFile()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "2476_GML.gmf");
            Assert.NotNull(path);
            Assert.True(System.IO.File.Exists(path));

            using GISModelFile gISModelFile = new(path);
            Assert.True(gISModelFile.Open());

            List<Building2D>? building2Ds = gISModelFile.Value?.GetObjects<Building2D>();
            Assert.NotNull(building2Ds);
            Assert.NotEmpty(building2Ds);

            foreach (Building2D building2D in building2Ds)
            {
                if (building2D.PolygonalFace2D is null)
                {
                    continue;
                }

                BuildingModel? buildingModel = Create.BuildingModel(building2D);

                Assert.NotNull(buildingModel);

                ushort expectedSpaces = building2D.Storeys == 0 ? (ushort)1 : building2D.Storeys;
                List<Space>? spaces = buildingModel.GetSpaces<Space>();
                Assert.NotNull(spaces);
                Assert.Equal(expectedSpaces, spaces.Count);

                List<IComponent>? components = buildingModel.GetComponents<IComponent>();
                Assert.NotNull(components);
                Assert.NotEmpty(components);
            }
        }

        /// <summary>
        /// Tests that every Shell produced by converting Building2D objects from the "2476_GML.gmf" file
        /// to BuildingModel is closed within <see cref="Core.Constants.Tolerance.MacroDistance"/>.
        /// Escalates tolerance from default (<see cref="Core.Constants.Tolerance.Distance"/>) upward
        /// until the shell is closed or the maximum tolerance is exceeded.
        /// </summary>
        [Fact]
        public void BuildingModel_FromGmfFile_ShellsAreClosed()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "2476_GML.gmf");
            Assert.NotNull(path);
            Assert.True(System.IO.File.Exists(path));

            using GISModelFile gISModelFile = new(path);
            Assert.True(gISModelFile.Open());

            List<Building2D>? building2Ds = gISModelFile.Value?.GetObjects<Building2D>();
            Assert.NotNull(building2Ds);
            Assert.NotEmpty(building2Ds);

            double[] tolerances = [Core.Constants.Tolerance.Distance, 1e-5, 1e-4, Core.Constants.Tolerance.MacroDistance];
            Dictionary<double, int> closedCounts = [];
            foreach (double tolerance in tolerances)
            {
                closedCounts[tolerance] = 0;
            }

            int totalBuildings = 0;
            int totalShells = 0;

            foreach (Building2D building2D in building2Ds)
            {
                if (building2D.PolygonalFace2D is null)
                {
                    continue;
                }

                totalBuildings++;

                BuildingModel? buildingModel = Create.BuildingModel(building2D);
                Assert.NotNull(buildingModel);

                List<Shell>? shells = buildingModel.GetShells<Space>();
                Assert.NotNull(shells);
                Assert.NotEmpty(shells);

                totalShells += shells.Count;

                double buildingClosedAt = 0;

                foreach (Shell shell in shells)
                {
                    Assert.NotNull(shell);

                    bool closed = false;
                    double closedAt = 0;

                    foreach (double tolerance in tolerances)
                    {
                        if (shell.IsClosed(tolerance))
                        {
                            closed = true;
                            closedAt = tolerance;
                            break;
                        }
                    }

                    Assert.True(closed, $"Shell for building {building2D.Guid} is not closed within Tolerance.MacroDistance ({Core.Constants.Tolerance.MacroDistance}).");

                    if (closedAt > buildingClosedAt)
                    {
                        buildingClosedAt = closedAt;
                    }
                }

                Assert.True(buildingClosedAt <= Core.Constants.Tolerance.MacroDistance, $"Building {building2D.Guid} closed at tolerance {buildingClosedAt} which exceeds Tolerance.MacroDistance ({Core.Constants.Tolerance.MacroDistance}).");

                closedCounts[buildingClosedAt]++;
            }

            testOutputHelper.WriteLine($"Total buildings: {totalBuildings}, total shells: {totalShells}");
            foreach (double tolerance in tolerances)
            {
                testOutputHelper.WriteLine($"  Closed at tolerance {tolerance:E2}: {closedCounts[tolerance]} buildings");
            }
        }
    }
}
