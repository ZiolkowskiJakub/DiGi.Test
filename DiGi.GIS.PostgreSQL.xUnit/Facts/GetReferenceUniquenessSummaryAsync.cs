using DiGi.GIS.PostgreSQL.Classes;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.GetReferenceUniquenessSummaryAsync(NpgsqlConnection?, int, System.Threading.CancellationToken)"/>
        /// and <see cref="Building2DPostgreSQLConverter.GetReferenceUniquenessSummaryAsync(int, System.Threading.CancellationToken)"/>
        /// return null when given a null connection or null connection data.
        /// </summary>
        [Fact]
        public async Task GetReferenceUniquenessSummaryAsync_NullConnection_ReturnsNull()
        {
            Building2DReferenceUniquenessSummary? summary_NullConnection = await Building2DPostgreSQLConverter.GetReferenceUniquenessSummaryAsync(null);
            Assert.Null(summary_NullConnection);

            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);
            Building2DReferenceUniquenessSummary? summary_NullConnectionData = await building2DPostgreSQLConverter.GetReferenceUniquenessSummaryAsync();
            Assert.Null(summary_NullConnectionData);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.GetReferenceUniquenessSummaryAsync(int, System.Threading.CancellationToken)"/>
        /// executes successfully against a live database with explicit command timeout.
        /// <para>Skipped by default: it executes an integration query requiring <c>GIS_PostgreSQL_Main.conf</c> pointing at a database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a database before running.")]
        public async Task GetReferenceUniquenessSummaryAsync_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            Building2DReferenceUniquenessSummary? summary = await building2DPostgreSQLConverter.GetReferenceUniquenessSummaryAsync(commandTimeout: 600);
            Assert.NotNull(summary);
        }
    }
}
