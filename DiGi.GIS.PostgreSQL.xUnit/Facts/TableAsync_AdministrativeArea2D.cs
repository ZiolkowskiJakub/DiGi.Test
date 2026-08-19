using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Constants;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Creates the administrative areal 2D table and verifies all expected indexes are present.
        /// <para>Skipped by default: it executes DDL on a live database, so it needs <c>GIS_PostgreSQL_Main.conf</c> beside the test assembly pointing at a scratch database.</para>
        /// </summary>
        [Fact(Skip = "Executes DDL against a live database. Point GIS_PostgreSQL_Main.conf at a scratch database before running.")]
        public async Task TableAsync_AdministrativeArea2D_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            ConnectionData? connectionData = administrativeAreal2DPostgreSQLConverter.ConnectionData;
            Assert.NotNull(connectionData);

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(connectionData);
            Assert.NotNull(npgsqlConnection);

            await npgsqlConnection.OpenAsync();

            Assert.True(await npgsqlConnection.TableAsync_AdministrativeArea2D());

            HashSet<string> indexNames = await IndexNamesAsync(npgsqlConnection, TableName.AdministrativeAreal2D);
            Assert.Contains($"idx_{TableName.AdministrativeAreal2D}_bbox", indexNames);
            Assert.Contains($"idx_{TableName.AdministrativeAreal2D}_type_id", indexNames);
            Assert.Contains($"idx_{TableName.AdministrativeAreal2D}_hierarchy", indexNames);
            Assert.Contains($"idx_{TableName.AdministrativeAreal2D}_type_code", indexNames);
            Assert.Contains($"idx_{TableName.AdministrativeAreal2D}_county_id", indexNames);
        }
    }
}
