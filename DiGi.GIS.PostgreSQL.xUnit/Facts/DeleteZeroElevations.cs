using DiGi.GIS.PostgreSQL.Classes;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="TerrainPointPostgreSQLConverter.DeleteZeroElevationsAsync(NpgsqlConnection, IEnumerable{int}, int, System.Threading.CancellationToken)"/> safely returns null when connection is null.
        /// </summary>
        [Fact]
        public async Task DeleteZeroElevationsAsync_NullConnection()
        {
            NpgsqlConnection? npgsqlConnection_Null = null;
            long? result = await TerrainPointPostgreSQLConverter.DeleteZeroElevationsAsync(npgsqlConnection_Null);
            Assert.Null(result);
        }

        /// <summary>
        /// Tests deleting zero-elevation points on a live PostgreSQL database partition.
        /// <para>Skipped by default because it requires live PostgreSQL connection files.</para>
        /// </summary>
        [Fact(Skip = "Requires live PostgreSQL database connection.")]
        public async Task DeleteZeroElevationsAsync_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            TerrainPointPostgreSQLConverter? terrainPointPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<TerrainPointPostgreSQLConverter>();
            Assert.NotNull(terrainPointPostgreSQLConverter);

            long? count_Deleted = await terrainPointPostgreSQLConverter.DeleteZeroElevationsAsync([2405]);
            Assert.NotNull(count_Deleted);
            Assert.True(count_Deleted >= 0);
        }
    }
}
