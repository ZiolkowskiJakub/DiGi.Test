using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.xUnit.Classes;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// The first scratch county part of the year built maintenance facts. Only a partition key in the throwaway test database - no <c>administrative_areal_2d</c> row is seeded for it.
        /// </summary>
        private const int countyId_YearBuilt_A = 990201;

        /// <summary>
        /// The second scratch county part of the year built maintenance facts, standing for a sibling polygon part of the same county.
        /// </summary>
        private const int countyId_YearBuilt_B = 990202;

        /// <summary>
        /// Drops every scratch partition and table the year built maintenance facts write, on both test databases. Called before seeding and again in <c>finally</c>, so a fact that failed half way leaves nothing behind for the next run.
        /// </summary>
        /// <param name="connectionData_Main">The main test database.</param>
        /// <param name="connectionData_Storage">The storage test database.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private static async Task CleanupYearBuiltDataScratchAsync(ConnectionData connectionData_Main, ConnectionData connectionData_Storage)
        {
            await using NpgsqlConnection? npgsqlConnection_Main = DiGi.PostgreSQL.Create.NpgsqlConnection(connectionData_Main);
            Assert.NotNull(npgsqlConnection_Main);
            await npgsqlConnection_Main.OpenAsync();

            foreach (int countyId in new int[] { countyId_YearBuilt_A, countyId_YearBuilt_B })
            {
                await ExecuteAsync(npgsqlConnection_Main, $"DROP TABLE IF EXISTS {Constants.TableName.YearBuiltData}_{countyId};");
                await ExecuteAsync(npgsqlConnection_Main, $"DROP TABLE IF EXISTS {Constants.TableName.Building2D}_{countyId};");
            }

            await using NpgsqlConnection? npgsqlConnection_Storage = DiGi.PostgreSQL.Create.NpgsqlConnection(connectionData_Storage);
            Assert.NotNull(npgsqlConnection_Storage);
            await npgsqlConnection_Storage.OpenAsync();

            await ExecuteAsync(npgsqlConnection_Storage, $"DROP TABLE IF EXISTS {new ScratchBuildingDataPostgreSQLConverter(null).TableName} CASCADE;");
        }

        /// <summary>
        /// Builds a year built data object for a scratch building.
        /// </summary>
        /// <param name="reference">The reference of the building.</param>
        /// <param name="userYear">The user-provided year, or null for no user entry.</param>
        /// <param name="userName">The user who recorded the user entry.</param>
        /// <param name="predictedYearBuilts">The predicted entries to add.</param>
        /// <returns>The object.</returns>
        private static GIS.Classes.YearBuiltData ScratchYearBuiltData(string reference, short? userYear, string? userName, params GIS.Classes.PredictedYearBuilt[] predictedYearBuilts)
        {
            GIS.Classes.YearBuiltData yearBuiltData = new(reference);

            if (userYear is not null)
            {
                Assert.True(yearBuiltData.SetUserYearBuilt(new GIS.Classes.UserYearBuilt(userYear.Value, GIS.Enums.YearBuiltRelation.Exact, new DateTimeOffset(2026, 9, 1, 10, 0, 0, TimeSpan.Zero), userName)));
            }

            foreach (GIS.Classes.PredictedYearBuilt predictedYearBuilt in predictedYearBuilts)
            {
                Assert.True(yearBuiltData.SetPredictedYearBuilt(predictedYearBuilt.DateTime, predictedYearBuilt.Year, predictedYearBuilt.ModelId));
            }

            return yearBuiltData;
        }

        /// <summary>
        /// Stores scratch year built data objects under one county part through the converter's own upsert, which also creates the table and the partition.
        /// </summary>
        /// <param name="yearBuiltDataPostgreSQLConverter">The converter of the main test database.</param>
        /// <param name="countyId">The county part to store the objects under.</param>
        /// <param name="yearBuiltDatas">The objects to store.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private static async Task SeedYearBuiltDatasAsync(YearBuiltDataPostgreSQLConverter yearBuiltDataPostgreSQLConverter, int countyId, params GIS.Classes.YearBuiltData[] yearBuiltDatas)
        {
            List<YearBuiltData> yearBuiltDatas_PostgreSQL = [.. yearBuiltDatas.Select(x => x.ToPostgreSQL(countyId)).OfType<YearBuiltData>()];
            Assert.Equal(yearBuiltDatas.Length, yearBuiltDatas_PostgreSQL.Count);

            PostgreSQLUpdateResult? postgreSQLUpdateResult = await yearBuiltDataPostgreSQLConverter.UpdateAsync(yearBuiltDatas_PostgreSQL);
            Assert.NotNull(postgreSQLUpdateResult);
            Assert.Equal(yearBuiltDatas.Length, postgreSQLUpdateResult.Ids.Count);
        }

        /// <summary>
        /// Reads back the stored objects of one scratch building under one county part.
        /// </summary>
        /// <param name="yearBuiltDataPostgreSQLConverter">The converter of the main test database.</param>
        /// <param name="countyId">The county part to read under.</param>
        /// <param name="reference">The reference of the building.</param>
        /// <returns>The stored objects, newest first.</returns>
        private static async Task<List<GIS.Classes.YearBuiltData>> ScratchYearBuiltDatasAsync(YearBuiltDataPostgreSQLConverter yearBuiltDataPostgreSQLConverter, int countyId, string reference)
        {
            List<YearBuiltData>? yearBuiltDatas = await yearBuiltDataPostgreSQLConverter.GetItemsByReferencesAsync([reference], countyId);
            Assert.NotNull(yearBuiltDatas);

            return [.. yearBuiltDatas.Select(x => x.ToDiGi()).OfType<GIS.Classes.YearBuiltData>()];
        }
    }
}
