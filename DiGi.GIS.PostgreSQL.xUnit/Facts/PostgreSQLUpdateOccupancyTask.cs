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

        /// <summary>
        /// Verifies that <see cref="PostgreSQLUpdateOccupancyTask"/> pairs buildings with the subdivisions holding their occupancy across sibling county polygon parts.
        /// <para>A multi-part county can store its buildings under one part while the subdivisions they belong to are parented under a sibling part of the same code. Keying the pairing on the part alone left every such building without a stored occupancy record, which is what left calculated_occupancy unwritten for the counties of DiGi.GIS.PostgreSQL#67. The run clears and rebuilds both occupancy datasets.</para>
        /// <para>Skipped by default: requires the PostgreSQL configuration files pointing at a database populated with administrative areal, building and occupancy data carrying the multi-part county layout.</para>
        /// </summary>
        [Fact(Skip = "Requires the PostgreSQL configuration files pointing at a database populated with administrative areal, building and occupancy data.")]
        public async Task PostgreSQLUpdateOccupancyTask_Building2D_SiblingCountyPart_Pairing_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            Building2DOccupancyDataPostgreSQLConverter? building2DOccupancyDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DOccupancyDataPostgreSQLConverter>();
            Assert.NotNull(building2DOccupancyDataPostgreSQLConverter);

            PostgreSQLUpdateOccupancyTask postgreSQLUpdateOccupancyTask = new(gISPostgreSQLConverterManager)
            {
                PostgreSQLUpdateOccupancyOptions = new PostgreSQLUpdateOccupancyOptions
                {
                    IncludeAdministrativeAreal2Ds = true,
                    IncludeBuilding2Ds = true,
                    Clear = true
                }
            };

            TaskCompletionSource<bool> taskCompletionSource = new();
            postgreSQLUpdateOccupancyTask.Stopped += (object? sender, EventArgs e) => taskCompletionSource.TrySetResult(true);

            postgreSQLUpdateOccupancyTask.Start();

            await taskCompletionSource.Task;

            Assert.Null(postgreSQLUpdateOccupancyTask.Exception);
            Assert.True(postgreSQLUpdateOccupancyTask.IsSucceeded);

            List<AdministrativeAreal2DReference>? countyReferences = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.County, commandTimeout: 600);
            Assert.NotNull(countyReferences);

            List<AdministrativeAreal2DReference>? subdivisionReferences = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.Subdivision, commandTimeout: 600);
            Assert.NotNull(subdivisionReferences);

            Dictionary<int, HashSet<int>> siblingCountyGroups = countyReferences.SiblingCountyGroups();

            Dictionary<int, int> subdivisionParentCountyId_ById = [];
            foreach (AdministrativeAreal2DReference subdivisionReference in subdivisionReferences)
            {
                if (subdivisionReference is not null && subdivisionReference.CountyId is int parentCountyId)
                {
                    subdivisionParentCountyId_ById[subdivisionReference.Id] = parentCountyId;
                }
            }

            Building2D? building2D_CrossPart = null;
            int countyId_CrossPart = 0;

            foreach (AdministrativeAreal2DReference countyReference in countyReferences)
            {
                if (countyReference is null || !siblingCountyGroups.TryGetValue(countyReference.Id, out HashSet<int>? siblingCountyIds) || siblingCountyIds.Count < 2)
                {
                    continue;
                }

                List<Building2D>? countyBuildings = await building2DPostgreSQLConverter.GetBuilding2DsByCountyIdAsync(countyReference.Id, commandTimeout: 600);
                if (countyBuildings is null)
                {
                    continue;
                }

                foreach (Building2D building2D in countyBuildings)
                {
                    if (building2D?.SubdivisionId is not int subdivisionId ||
                        !subdivisionParentCountyId_ById.TryGetValue(subdivisionId, out int subdivisionParentCountyId) ||
                        subdivisionParentCountyId == countyReference.Id ||
                        !siblingCountyIds.Contains(subdivisionParentCountyId))
                    {
                        continue;
                    }

                    building2D_CrossPart = building2D;
                    countyId_CrossPart = countyReference.Id;
                    break;
                }

                if (building2D_CrossPart is not null)
                {
                    break;
                }
            }

            Assert.True(building2D_CrossPart is not null, "The database holds no building whose subdivision is parented under a sibling county part, so the multi-part layout this fact requires is absent.");

            List<Building2DOccupancyData>? building2DOccupancyDatas = await building2DOccupancyDataPostgreSQLConverter.GetItemsByReferenceAsync(building2D_CrossPart!.Reference!, countyId_CrossPart, commandTimeout: 600);
            Assert.NotNull(building2DOccupancyDatas);
            Assert.NotEmpty(building2DOccupancyDatas);
        }
    }
}
