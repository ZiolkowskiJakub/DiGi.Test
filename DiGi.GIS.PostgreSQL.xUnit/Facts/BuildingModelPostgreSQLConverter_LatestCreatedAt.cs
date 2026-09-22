using DiGi.GIS.PostgreSQL.Classes;
using DiGi.Analytical.Building.Enums;
using System;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Building2DReferencedObjectPostgreSQLConverter{TBuilding2DReferencedObject, TUniqueObject}.GetLatestCreatedAtAsync(Npgsql.NpgsqlConnection?, int?, int, System.Threading.CancellationToken)"/> and its instance overload answer null, never throw, when there is no connection.
        /// <para>The endpoint built on it turns null into 404, so a null here must mean "nothing to report" rather than a fault.</para>
        /// </summary>
        [Fact]
        public async Task BuildingModelPostgreSQLConverter_GetLatestCreatedAtAsync_NoConnection_ReturnsNull()
        {
            BuildingModelPostgreSQLConverter buildingModelPostgreSQLConverter = new(null, BuildingModelDetailLevel.Component);

            DateTimeOffset? dateTimeOffset_Static = await buildingModelPostgreSQLConverter.GetLatestCreatedAtAsync(null, 15678);
            Assert.Null(dateTimeOffset_Static);

            DateTimeOffset? dateTimeOffset_Instance = await buildingModelPostgreSQLConverter.GetLatestCreatedAtAsync(15678);
            Assert.Null(dateTimeOffset_Instance);

            DateTimeOffset? dateTimeOffset_WholeTable = await buildingModelPostgreSQLConverter.GetLatestCreatedAtAsync(null);
            Assert.Null(dateTimeOffset_WholeTable);
        }

        /// <summary>
        /// Verifies against a database that the latest creation stamp of a county partition is a plausible UTC instant no later than now, and that a county without a partition answers null rather than throwing.
        /// <para>Skipped by default: it executes an integration query requiring <c>GIS_PostgreSQL_Main.conf</c> pointing at a database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a database before running.")]
        public async Task BuildingModelPostgreSQLConverter_GetLatestCreatedAtAsync_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            BuildingModelPostgreSQLConverter? buildingModelPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingModelPostgreSQLConverter>();
            Assert.NotNull(buildingModelPostgreSQLConverter);

            // The whole table: null only when nothing has ever been written.
            DateTimeOffset? dateTimeOffset_Table = await buildingModelPostgreSQLConverter.GetLatestCreatedAtAsync(null);
            Assert.NotNull(dateTimeOffset_Table);
            Assert.Equal(TimeSpan.Zero, dateTimeOffset_Table.Value.Offset);
            Assert.True(dateTimeOffset_Table.Value <= DateTimeOffset.UtcNow);

            // A county id no partition can carry.
            DateTimeOffset? dateTimeOffset_Missing = await buildingModelPostgreSQLConverter.GetLatestCreatedAtAsync(-1);
            Assert.Null(dateTimeOffset_Missing);
        }
    }
}
