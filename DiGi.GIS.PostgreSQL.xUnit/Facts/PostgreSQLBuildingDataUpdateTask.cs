using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="PostgreSQLBuildingDataUpdateTask"/> initializes its default properties and validates null arguments.
        /// </summary>
        [Fact]
        public void PostgreSQLBuildingDataUpdateTask_Constructor()
        {
            Assert.Throws<ArgumentNullException>(() => new PostgreSQLBuildingDataUpdateTask(null!));

            GISPostgreSQLConverterManager gISPostgreSQLConverterManager = new();
            PostgreSQLBuildingDataUpdateTask postgreSQLBuildingDataUpdateTask = new(gISPostgreSQLConverterManager);

            Assert.NotNull(postgreSQLBuildingDataUpdateTask.PostgreSQLBuildingDataUpdateOptions);
            Assert.Equal(0, postgreSQLBuildingDataUpdateTask.FailedSubdivisionCount);
            Assert.Equal(0, postgreSQLBuildingDataUpdateTask.ProcessedSubdivisionCount);
            Assert.Equal(0, postgreSQLBuildingDataUpdateTask.SkippedSubdivisionCount);
            Assert.Equal(0, postgreSQLBuildingDataUpdateTask.UnassignedSubdivisionBuildingCount);
            Assert.Equal(0, postgreSQLBuildingDataUpdateTask.CrossCountySubdivisionBuildingCount);
            Assert.Equal(0, postgreSQLBuildingDataUpdateTask.UpdatedRowCount);
        }

        /// <summary>
        /// Verifies that <see cref="PostgreSQLBuildingDataUpdateTask"/> correctly processes buildings for multi-part counties where subdivisions and buildings reside on different sibling county parts.
        /// <para>Skipped by default: requires PostgreSQL configuration files pointing at a database populated with administrative areal and building data.</para>
        /// </summary>
        [Fact(Skip = "Requires the PostgreSQL configuration files pointing at a database.")]
        public async Task PostgreSQLBuildingDataUpdateTask_MultiPartCounty_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingDataPostgreSQLConverter>();
            Assert.NotNull(buildingDataPostgreSQLConverter);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            // Test county: czestochowski (TERYT 2404, building partition 76453, subdivision parent 76454)
            int countyId = 76453;

            PostgreSQLBuildingDataUpdateTask postgreSQLBuildingDataUpdateTask = new(gISPostgreSQLConverterManager)
            {
                PostgreSQLBuildingDataUpdateOptions = new PostgreSQLBuildingDataUpdateOptions
                {
                    BuildingDataUpdateTypes = [BuildingDataUpdateType.General, BuildingDataUpdateType.Database],
                    CountyIds = [countyId]
                }
            };

            TaskCompletionSource<bool> taskCompletionSource = new();
            postgreSQLBuildingDataUpdateTask.Stopped += (object? sender, EventArgs e) => taskCompletionSource.TrySetResult(true);

            postgreSQLBuildingDataUpdateTask.Start();

            await taskCompletionSource.Task;

            Assert.Null(postgreSQLBuildingDataUpdateTask.Exception);
            Assert.True(postgreSQLBuildingDataUpdateTask.IsSucceeded);
            Assert.Equal(0, postgreSQLBuildingDataUpdateTask.FailedSubdivisionCount);

            BuildingDataCoverageResult? buildingDataCoverageResult = await buildingDataPostgreSQLConverter.BuildingDataCoverageResultAsync(building2DPostgreSQLConverter, countyId);
            Assert.NotNull(buildingDataCoverageResult);
            Assert.Equal(0, buildingDataCoverageResult.MissingReferenceCount);
        }

        /// <summary>
        /// Verifies that a subdivision filed under one county part is in scope only for the parts of that part's code group, never for an unrelated part.
        /// <para>The figures are the live data of the defect: subdivision 80392 (Baranowice) is filed under part 80379 (Miasto Żory), so the buildings it carries in part 77971 are invisible to the subdivision loop and only the fallback can write them.</para>
        /// </summary>
        [Fact]
        public void InScopeSubdivisionIds_CrossCountySubdivision()
        {
            List<AdministrativeAreal2DReference> subdivisions =
            [
                new()
                {
                    Id = 80392,
                    CountyId = 80379,
                    Code = "2479011"
                }
            ];

            Dictionary<int, HashSet<int>> siblingCountyGroups = new()
            {
                [77971] = [77971],
                [80379] = [80379]
            };

            Dictionary<int, HashSet<int>> inScopeSubdivisionIds = Query.InScopeSubdivisionIds(subdivisions, siblingCountyGroups);

            Assert.Contains(80379, inScopeSubdivisionIds);
            Assert.Contains(80392, inScopeSubdivisionIds[80379]);

            // The defect case: 80392 must not be treated as in scope under 77971, or the fallback would re-process what the loop already wrote.
            Assert.DoesNotContain(77971, inScopeSubdivisionIds);
        }

        /// <summary>
        /// Verifies that a subdivision filed under one part of a multi-part county is in scope for every part of the code group, because the subdivision loop visits all of them.
        /// </summary>
        [Fact]
        public void InScopeSubdivisionIds_SiblingPartsShareScope()
        {
            List<AdministrativeAreal2DReference> subdivisions =
            [
                new()
                {
                    Id = 50000,
                    CountyId = 73482
                }
            ];

            Dictionary<int, HashSet<int>> siblingCountyGroups = new()
            {
                [73482] = [73482, 73485],
                [73485] = [73482, 73485]
            };

            Dictionary<int, HashSet<int>> inScopeSubdivisionIds = Query.InScopeSubdivisionIds(subdivisions, siblingCountyGroups);

            Assert.Contains(50000, inScopeSubdivisionIds[73482]);
            Assert.Contains(50000, inScopeSubdivisionIds[73485]);
        }

        /// <summary>
        /// Verifies that the instance and static overloads of <c>Building2DPostgreSQLConverter.GetBuilding2DsUnreachedByCountyAsync</c> return null over a null connection.
        /// </summary>
        [Fact]
        public async Task GetBuilding2DsUnreachedByCountyAsync_NullConnection_ReturnsNull()
        {
            List<Building2D>? buildings_StaticNullConnection = await Building2DPostgreSQLConverter.GetBuilding2DsUnreachedByCountyAsync(null, 77971, [80392]);
            Assert.Null(buildings_StaticNullConnection);

            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);

            List<Building2D>? buildings_InstanceNullConnectionData = await building2DPostgreSQLConverter.GetBuilding2DsUnreachedByCountyAsync(77971, [80392]);
            Assert.Null(buildings_InstanceNullConnectionData);
        }

        /// <summary>
        /// Verifies that cross-county buildings - filed under a county part but subdivided under a neighbouring county - get a building data row after a run.
        /// <para>County part 77971 carries two such buildings under subdivision 80392 (filed under neighbouring part 80379), which the subdivision loop cannot reach. Before the fix the fallback read only <c>subdivision_id IS NULL</c> buildings, so both were left out and the coverage reported two missing references; after the fix the set difference is empty.</para>
        /// <para>Skipped by default: requires PostgreSQL configuration files pointing at a database.</para>
        /// </summary>
        [Fact(Skip = "Requires the PostgreSQL configuration files pointing at a database.")]
        public async Task PostgreSQLBuildingDataUpdateTask_CrossCountySubdivision_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingDataPostgreSQLConverter>();
            Assert.NotNull(buildingDataPostgreSQLConverter);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            // Test county: part 77971, whose two cross-county buildings sit under subdivision 80392 (parent part 80379).
            int countyId = 77971;

            PostgreSQLBuildingDataUpdateTask postgreSQLBuildingDataUpdateTask = new(gISPostgreSQLConverterManager)
            {
                PostgreSQLBuildingDataUpdateOptions = new PostgreSQLBuildingDataUpdateOptions
                {
                    BuildingDataUpdateTypes = [BuildingDataUpdateType.General, BuildingDataUpdateType.Database],
                    CountyIds = [countyId]
                }
            };

            TaskCompletionSource<bool> taskCompletionSource = new();
            postgreSQLBuildingDataUpdateTask.Stopped += (object? sender, EventArgs e) => taskCompletionSource.TrySetResult(true);

            postgreSQLBuildingDataUpdateTask.Start();

            await taskCompletionSource.Task;

            Assert.Null(postgreSQLBuildingDataUpdateTask.Exception);
            Assert.True(postgreSQLBuildingDataUpdateTask.IsSucceeded);
            Assert.Equal(0, postgreSQLBuildingDataUpdateTask.FailedSubdivisionCount);
            Assert.True(postgreSQLBuildingDataUpdateTask.CrossCountySubdivisionBuildingCount > 0);

            BuildingDataCoverageResult? buildingDataCoverageResult = await buildingDataPostgreSQLConverter.BuildingDataCoverageResultAsync(building2DPostgreSQLConverter, countyId);
            Assert.NotNull(buildingDataCoverageResult);
            Assert.Equal(0, buildingDataCoverageResult.MissingReferenceCount);
        }
    }
}
