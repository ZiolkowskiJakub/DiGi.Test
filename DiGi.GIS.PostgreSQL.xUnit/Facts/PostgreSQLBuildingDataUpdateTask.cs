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
    }
}
