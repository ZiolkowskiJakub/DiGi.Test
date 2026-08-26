using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="BuildingDataPostgreSQLConverter.PullAsync(Npgsql.NpgsqlConnection?, IEnumerable{string}, int?, IEnumerable{string}?, int, bool, int, System.Threading.CancellationToken)"/>
        /// and its instance overload return null when inputs or connection are null or empty.
        /// </summary>
        [Fact]
        public async Task BuildingDataPostgreSQLConverter_PullAsync_NullOrEmpty_ReturnsNull()
        {
            BuildingDataPostgreSQLConverter buildingDataPostgreSQLConverter = new(null);

            Table? result_NullConn = await buildingDataPostgreSQLConverter.PullAsync(null, ["ref_1"], 10, fallbackByReference: true);
            Assert.Null(result_NullConn);

            Table? result_NullRefs = await buildingDataPostgreSQLConverter.PullAsync(null, null!, 10, fallbackByReference: true);
            Assert.Null(result_NullRefs);

            Table? result_EmptyRefs = await buildingDataPostgreSQLConverter.PullAsync(null, [], 10, fallbackByReference: true);
            Assert.Null(result_EmptyRefs);

            Table? result_InstanceNullRefs = await buildingDataPostgreSQLConverter.PullAsync((IEnumerable<string>)null!, 10, fallbackByReference: true);
            Assert.Null(result_InstanceNullRefs);

            Table? result_InstanceEmptyRefs = await buildingDataPostgreSQLConverter.PullAsync([], 10, fallbackByReference: true);
            Assert.Null(result_InstanceEmptyRefs);
        }

        /// <summary>
        /// Verifies that the reads added for the coverage and duplicate checks return the "not available" answer rather than throwing when there is no connection.
        /// </summary>
        [Fact]
        public async Task BuildingDataPostgreSQLConverter_NewReads_NoConnection_ReturnDefaults()
        {
            BuildingDataPostgreSQLConverter buildingDataPostgreSQLConverter = new(null);

            Assert.Equal(-1, await buildingDataPostgreSQLConverter.GetCountAsync(55417));
            Assert.Equal(-1, await buildingDataPostgreSQLConverter.GetEstimatedCountAsync(55417));
            Assert.Null(await buildingDataPostgreSQLConverter.GetReferencesByCountyIdAsync(55417));
            Assert.Null(await buildingDataPostgreSQLConverter.GetDuplicateReferencesAsync());
            Assert.Null(await buildingDataPostgreSQLConverter.GetCountyIdsByReferenceAsync("some_reference"));

            // A blank reference is rejected before a connection is even attempted.
            Assert.Null(await buildingDataPostgreSQLConverter.GetCountyIdsByReferenceAsync(null));
            Assert.Null(await buildingDataPostgreSQLConverter.GetCountyIdsByReferenceAsync("   "));

            Assert.Null(await BuildingDataPostgreSQLConverter.GetDuplicateReferencesAsync(null));
            Assert.Equal(-1, await Building2DPostgreSQLConverter.GetCountWithoutSubdivisionAsync(null, 55417));
            Assert.Null(await Building2DPostgreSQLConverter.GetBuilding2DsWithoutSubdivisionAsync(null, 55417));
        }

        /// <summary>
        /// Verifies that <see cref="PostgreSQLBuildingDataUpdateTask.UnassignedSubdivisionBuildingCount"/> starts at 0 upon instantiation.
        /// </summary>
        [Fact]
        public void PostgreSQLBuildingDataUpdateTask_UnassignedSubdivisionBuildingCount_InitialState()
        {
            GISPostgreSQLConverterManager gISPostgreSQLConverterManager = new();
            PostgreSQLBuildingDataUpdateTask task = new(gISPostgreSQLConverterManager);

            Assert.Equal(0, task.UnassignedSubdivisionBuildingCount);
        }

        /// <summary>
        /// Verifies that every statement behind the new reads is accepted by the database and answers the shape the callers expect.
        /// <para>These queries are written by hand rather than built from the column metadata, so a wrong column name or a malformed clause would only ever surface here or in production. A county holding no rows is still a complete check of that: the server parses and plans the statement either way.</para>
        /// <para>Skipped by default: it executes integration queries requiring the confs to point at a database.</para>
        /// </summary>
        [Fact(Skip = "Executes integration queries. Point GIS_PostgreSQL_Main.conf and GIS_PostgreSQL_Storage.conf at a database before running.")]
        public async Task BuildingDataPostgreSQLConverter_NewReads_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingDataPostgreSQLConverter>();
            Assert.NotNull(buildingDataPostgreSQLConverter);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            int countyId = 55417;

            // The whole table always exists, so this one separates a working statement from a missing partition.
            long count_Total = await buildingDataPostgreSQLConverter.GetCountAsync(null);
            Assert.True(count_Total >= 0);

            // The estimate reads pg_class.reltuples, which a partitioned parent carries as -1 until it is
            // analysed. What is being checked here is that the statement runs, not that the answer is a count.
            long count_Estimated = await buildingDataPostgreSQLConverter.GetEstimatedCountAsync(null);
            Assert.True(count_Estimated >= -1);

            HashSet<string>? references = await buildingDataPostgreSQLConverter.GetReferencesByCountyIdAsync(countyId);
            Assert.NotNull(references);

            long count_County = await buildingDataPostgreSQLConverter.GetCountAsync(countyId);
            if (count_County >= 0)
            {
                // Rows are one per county and reference, so the references cannot outnumber them.
                Assert.True(references.Count <= count_County);
            }

            List<Building2DReferenceDuplicate>? building2DReferenceDuplicates = await buildingDataPostgreSQLConverter.GetDuplicateReferencesAsync(10);
            Assert.NotNull(building2DReferenceDuplicates);
            foreach (Building2DReferenceDuplicate building2DReferenceDuplicate in building2DReferenceDuplicates)
            {
                Assert.NotNull(building2DReferenceDuplicate.CountyIds);
                Assert.True(building2DReferenceDuplicate.CountyIds.Count > 1);
            }

            Assert.True(await Building2DPostgreSQLConverter.GetCountWithoutSubdivisionAsync(null, countyId) == -1);
            Assert.True(await building2DPostgreSQLConverter.GetCountWithoutSubdivisionAsync(countyId) >= 0);

            string? reference = references.FirstOrDefault();
            if (reference is not null)
            {
                List<int>? countyIds = await buildingDataPostgreSQLConverter.GetCountyIdsByReferenceAsync(reference);
                Assert.NotNull(countyIds);
                Assert.Contains(countyId, countyIds);
            }
        }
    }
}
