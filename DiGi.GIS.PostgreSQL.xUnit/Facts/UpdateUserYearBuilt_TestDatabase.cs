using DiGi.GIS.PostgreSQL.Classes;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies, on the throwaway main test database, that <see cref="YearBuiltDataPostgreSQLConverter.UpdateUserYearBuiltAsync"/> still commits with the row lock it now takes before reading, and that the entry it writes is the one <see cref="YearBuiltDataPostgreSQLConverter.RemoveUserYearBuiltsAsync(System.Collections.Generic.IEnumerable{int}?, System.Collections.Generic.IEnumerable{string}?, string?, bool, int, System.Threading.CancellationToken)"/> withdraws for its owner.
        /// <para>Medium test (a live database server; under 0.5 s measured alone): it writes the scratch partitions of 990201 and drops them before and after.</para>
        /// </summary>
        [MediumSkippableFact]
        public async Task UpdateUserYearBuilt_TestDatabase()
        {
            (ConnectionData connectionData_Main, ConnectionData connectionData_Storage) = TestConnectionDatas();

            YearBuiltDataPostgreSQLConverter yearBuiltDataPostgreSQLConverter = new(connectionData_Main);

            DateTime dateTime = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

            await CleanupYearBuiltDataScratchAsync(connectionData_Main, connectionData_Storage);
            try
            {
                await using (NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(connectionData_Main))
                {
                    Assert.NotNull(npgsqlConnection);
                    await npgsqlConnection.OpenAsync();

                    Assert.True(await npgsqlConnection.TableAsync_Building2D());
                    Assert.True(await npgsqlConnection.TableAsync_Building2D_Partition(countyId_YearBuilt_A));
                    await ExecuteAsync(npgsqlConnection, $"INSERT INTO {Constants.TableName.Building2D} (county_id, reference) VALUES ({countyId_YearBuilt_A}, 'XUNIT-UU-LOCK');");
                }

                await SeedYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_A, ScratchYearBuiltData("XUNIT-UU-LOCK", null, null, new GIS.Classes.PredictedYearBuilt(dateTime, 1980)));

                GIS.Classes.UserYearBuilt userYearBuilt = new(1966, GIS.Enums.YearBuiltRelation.Exact, new DateTimeOffset(2026, 10, 8, 9, 0, 0, TimeSpan.Zero), "owner@example.com");
                Assert.True(await yearBuiltDataPostgreSQLConverter.UpdateUserYearBuiltAsync(countyId_YearBuilt_A, "XUNIT-UU-LOCK", userYearBuilt));

                GIS.Classes.YearBuiltData yearBuiltData = Assert.Single(await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_A, "XUNIT-UU-LOCK"));
                Assert.Equal((short)1966, yearBuiltData.GetUserYearBuilt()?.Year);
                Assert.Equal((short)1980, yearBuiltData.GetPredictedYearBuilt(dateTime)?.Year);

                UserYearBuiltRemoveResult? userYearBuiltRemoveResult = await yearBuiltDataPostgreSQLConverter.RemoveUserYearBuiltsAsync([countyId_YearBuilt_A], ["XUNIT-UU-LOCK"], "owner@example.com", false);
                Assert.NotNull(userYearBuiltRemoveResult);
                Assert.Equal(["XUNIT-UU-LOCK"], userYearBuiltRemoveResult.RemovedReferences);
                Assert.Null(Assert.Single(await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_A, "XUNIT-UU-LOCK")).GetUserYearBuilt());
            }
            finally
            {
                await CleanupYearBuiltDataScratchAsync(connectionData_Main, connectionData_Storage);
            }
        }
    }
}
