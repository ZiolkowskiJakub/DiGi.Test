using DiGi.GIS.PostgreSQL.Classes;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.GetDuplicateReferencesAsync(NpgsqlConnection?, int, int, System.Threading.CancellationToken)"/>
        /// and <see cref="Building2DPostgreSQLConverter.GetDuplicateReferencesAsync(int, int, System.Threading.CancellationToken)"/>
        /// return null when given a null connection or null connection data.
        /// </summary>
        [Fact]
        public async Task GetDuplicateReferencesAsync_NullConnection_ReturnsNull()
        {
            List<Building2DReferenceDuplicate>? duplicates_NullConnection = await Building2DPostgreSQLConverter.GetDuplicateReferencesAsync(null);
            Assert.Null(duplicates_NullConnection);

            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);
            List<Building2DReferenceDuplicate>? duplicates_NullConnectionData = await building2DPostgreSQLConverter.GetDuplicateReferencesAsync();
            Assert.Null(duplicates_NullConnectionData);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.GetDuplicateReferencesAsync(int, int, System.Threading.CancellationToken)"/>
        /// executes successfully against a live database with explicit command timeout.
        /// <para>Skipped by default: it executes an integration query requiring <c>GIS_PostgreSQL_Main.conf</c> pointing at a database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a database before running.")]
        public async Task GetDuplicateReferencesAsync_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            List<Building2DReferenceDuplicate>? duplicates = await building2DPostgreSQLConverter.GetDuplicateReferencesAsync(limit: 10, commandTimeout: 600);
            Assert.NotNull(duplicates);
        }
    }
}
