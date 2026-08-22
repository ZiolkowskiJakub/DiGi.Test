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
        /// Verifies that <see cref="PostgreSQLUpdateOccupancyTask"/> correctly rolls up occupancy across all administrative hierarchy tiers,
        /// including subdivisions belonging directly to cities with county rights without an intermediate municipality layer.
        /// <para>Skipped by default: requires the PostgreSQL configuration files pointing at a database populated with administrative areal and occupancy data.</para>
        /// </summary>
        [Fact(Skip = "Requires the PostgreSQL configuration files pointing at a database.")]
        public async Task PostgreSQLUpdateOccupancyTask_AdministrativeAreal2D_Rollup_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            AdministrativeAreal2DOccupancyDataPostgreSQLConverter? administrativeAreal2DOccupancyDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DOccupancyDataPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DOccupancyDataPostgreSQLConverter);

            PostgreSQLUpdateOccupancyTask postgreSQLUpdateOccupancyTask = new(gISPostgreSQLConverterManager)
            {
                PostgreSQLUpdateOccupancyOptions = new PostgreSQLUpdateOccupancyOptions
                {
                    IncludeAdministrativeAreal2Ds = true,
                    IncludeBuilding2Ds = false,
                    Clear = true
                }
            };

            TaskCompletionSource<bool> taskCompletionSource = new();
            postgreSQLUpdateOccupancyTask.Stopped += (object? sender, EventArgs e) => taskCompletionSource.TrySetResult(true);

            postgreSQLUpdateOccupancyTask.Start();

            await taskCompletionSource.Task;

            Assert.Null(postgreSQLUpdateOccupancyTask.Exception);
            Assert.True(postgreSQLUpdateOccupancyTask.IsSucceeded);

            // Verify that total occupancy at the Country level matches the sum of all Subdivision occupancies.
            List<AdministrativeAreal2DReference>? administrativeAreal2DReferences_Subdivisions = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.Subdivision);
            Assert.NotNull(administrativeAreal2DReferences_Subdivisions);

            uint totalSubdivisionOccupancy = 0;
            foreach (AdministrativeAreal2DReference administrativeAreal2DReference in administrativeAreal2DReferences_Subdivisions)
            {
                if (await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DByIdAsync(administrativeAreal2DReference.Id) is AdministrativeAreal2D administrativeAreal2D_PostgreSQL &&
                    administrativeAreal2D_PostgreSQL.ToDiGi() is GIS.Classes.AdministrativeSubdivision administrativeSubdivision)
                {
                    totalSubdivisionOccupancy += administrativeSubdivision.Occupancy ?? 0;
                }
            }

            List<AdministrativeAreal2DReference>? administrativeAreal2DReferences_Country = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.Country, uniqueCode: true);
            Assert.NotNull(administrativeAreal2DReferences_Country);
            Assert.NotEmpty(administrativeAreal2DReferences_Country);

            uint totalCountryOccupancy = 0;
            foreach (AdministrativeAreal2DReference administrativeAreal2DReference in administrativeAreal2DReferences_Country)
            {
                if (!string.IsNullOrWhiteSpace(administrativeAreal2DReference.Reference) &&
                    (await administrativeAreal2DOccupancyDataPostgreSQLConverter.GetItemByReferenceAsync(administrativeAreal2DReference.Reference))?.ToDiGi() is GIS.Classes.OccupancyData occupancyData)
                {
                    totalCountryOccupancy += occupancyData.Occupancy ?? 0;
                }
            }

            Assert.Equal(totalSubdivisionOccupancy, totalCountryOccupancy);
        }
    }
}
