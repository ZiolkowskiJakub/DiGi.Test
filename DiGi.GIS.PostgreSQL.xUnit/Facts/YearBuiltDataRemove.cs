using DiGi.GIS.PostgreSQL.Classes;
using DiGi.PostgreSQL.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies <see cref="YearBuiltDataPostgreSQLConverter.RemoveItemsAsync(IEnumerable{int}?, IEnumerable{string}?, bool, bool, int, int, int, System.Threading.CancellationToken)"/> on the throwaway main test database.
        /// <para>Two county parts, each holding empty and non-empty objects. Covers: a dry run counts and deletes nothing; the empty-only scope over every building of both parts deletes exactly the empty objects of both parts and keeps every object with an entry; a scope matching more rows than the limit deletes nothing; a delete by reference without the empty-only filter takes every object of that building and reports a reference nothing holds; and the refused arguments - no part, null references without empty-only - answer null.</para>
        /// <para>Medium test (a live database server; under 0.5 s measured alone): it writes the scratch partitions of 990201 and 990202 and drops them before and after.</para>
        /// </summary>
        [MediumSkippableFact]
        public async Task YearBuiltDataPostgreSQLConverter_RemoveItems()
        {
            (ConnectionData connectionData_Main, ConnectionData connectionData_Storage) = TestConnectionDatas();

            YearBuiltDataPostgreSQLConverter yearBuiltDataPostgreSQLConverter = new(connectionData_Main);

            Assert.Null(await yearBuiltDataPostgreSQLConverter.RemoveItemsAsync([], null, true, true, 10));
            Assert.Null(await yearBuiltDataPostgreSQLConverter.RemoveItemsAsync([countyId_YearBuilt_A], null, false, true, 10));
            Assert.Null(await yearBuiltDataPostgreSQLConverter.RemoveItemsAsync([countyId_YearBuilt_A], null, true, true, 0));

            DateTime dateTime = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

            await CleanupYearBuiltDataScratchAsync(connectionData_Main, connectionData_Storage);
            try
            {
                await SeedYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_A,
                    ScratchYearBuiltData("XUNIT-RI-EMPTY-1", null, null),
                    ScratchYearBuiltData("XUNIT-RI-EMPTY-1", null, null),
                    ScratchYearBuiltData("XUNIT-RI-FULL-1", null, null, new GIS.Classes.PredictedYearBuilt(dateTime, 1970)),
                    ScratchYearBuiltData("XUNIT-RI-MIXED", 1965, "reviewer@example.com"));

                await SeedYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_B,
                    ScratchYearBuiltData("XUNIT-RI-EMPTY-2", null, null),
                    ScratchYearBuiltData("XUNIT-RI-MIXED", null, null),
                    ScratchYearBuiltData("XUNIT-RI-FULL-2", null, null, new GIS.Classes.PredictedYearBuilt(dateTime, 1980)));

                int[] countyIds = [countyId_YearBuilt_A, countyId_YearBuilt_B];

                // Dry run: the four empty objects of both parts are counted, nothing is deleted.
                YearBuiltDataRemoveResult? yearBuiltDataRemoveResult = await yearBuiltDataPostgreSQLConverter.RemoveItemsAsync(countyIds, null, true, true, 100);
                Assert.NotNull(yearBuiltDataRemoveResult);
                Assert.True(yearBuiltDataRemoveResult.DryRun);
                Assert.Equal(4, yearBuiltDataRemoveResult.Matched);
                Assert.Equal(0, yearBuiltDataRemoveResult.Removed);
                Assert.Equal(2, (await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_A, "XUNIT-RI-EMPTY-1")).Count);

                // Over the limit: refused as a whole, nothing deleted.
                yearBuiltDataRemoveResult = await yearBuiltDataPostgreSQLConverter.RemoveItemsAsync(countyIds, null, true, false, 3);
                Assert.NotNull(yearBuiltDataRemoveResult);
                Assert.Equal(4, yearBuiltDataRemoveResult.Matched);
                Assert.Equal(0, yearBuiltDataRemoveResult.Removed);
                Assert.Single(await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_B, "XUNIT-RI-EMPTY-2"));

                // Empty-only over both parts: exactly the empty objects go, every object with an entry stays.
                yearBuiltDataRemoveResult = await yearBuiltDataPostgreSQLConverter.RemoveItemsAsync(countyIds, null, true, false, 4);
                Assert.NotNull(yearBuiltDataRemoveResult);
                Assert.False(yearBuiltDataRemoveResult.DryRun);
                Assert.Equal(4, yearBuiltDataRemoveResult.Matched);
                Assert.Equal(4, yearBuiltDataRemoveResult.Removed);
                Assert.Empty(yearBuiltDataRemoveResult.UnmatchedReferences);

                Assert.Empty(await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_A, "XUNIT-RI-EMPTY-1"));
                Assert.Empty(await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_B, "XUNIT-RI-EMPTY-2"));
                Assert.Empty(await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_B, "XUNIT-RI-MIXED"));
                Assert.Single(await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_A, "XUNIT-RI-MIXED"));
                Assert.Single(await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_A, "XUNIT-RI-FULL-1"));
                Assert.Single(await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_B, "XUNIT-RI-FULL-2"));

                // By reference, entries or not: the building's objects go; a reference nothing holds is reported.
                yearBuiltDataRemoveResult = await yearBuiltDataPostgreSQLConverter.RemoveItemsAsync(countyIds, ["XUNIT-RI-FULL-1", "XUNIT-RI-NONE"], false, false, 10);
                Assert.NotNull(yearBuiltDataRemoveResult);
                Assert.Equal(1, yearBuiltDataRemoveResult.Removed);
                Assert.Equal(["XUNIT-RI-NONE"], yearBuiltDataRemoveResult.UnmatchedReferences);
                Assert.Empty(await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_A, "XUNIT-RI-FULL-1"));

                // Empty-only by reference: a building whose object still carries an entry is reported, not deleted.
                yearBuiltDataRemoveResult = await yearBuiltDataPostgreSQLConverter.RemoveItemsAsync(countyIds, ["XUNIT-RI-FULL-2"], true, false, 10);
                Assert.NotNull(yearBuiltDataRemoveResult);
                Assert.Equal(0, yearBuiltDataRemoveResult.Removed);
                Assert.Equal(["XUNIT-RI-FULL-2"], yearBuiltDataRemoveResult.UnmatchedReferences);
                Assert.Single(await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_B, "XUNIT-RI-FULL-2"));
            }
            finally
            {
                await CleanupYearBuiltDataScratchAsync(connectionData_Main, connectionData_Storage);
            }
        }

        /// <summary>
        /// Verifies <see cref="YearBuiltDataPostgreSQLConverter.RemovePredictedYearBuiltsAsync(IEnumerable{int}?, DateTime, IEnumerable{string}?, bool, int, int, int, System.Threading.CancellationToken)"/> and <see cref="YearBuiltDataPostgreSQLConverter.GetPredictedYearBuiltRunsAsync(IEnumerable{int}?, int, System.Threading.CancellationToken)"/> on the throwaway main test database.
        /// <para>Two runs with two model identifiers across two county parts. Covers: the run listing reports one entry per part, stamp and model with the object counts, and its ticks are the keys the entries are stored under; a dry run and a run over the limit change nothing; removing one run takes only that stamp, keeps the other stamp and the user entry, rewrites each object under its own part, reports the object left empty and keeps it; the listing no longer shows the removed run; a second removal matches nothing; a page size smaller than the scope still reaches every object.</para>
        /// <para>Medium test (a live database server; under 0.5 s measured alone): it writes the scratch partitions of 990201 and 990202 and drops them before and after.</para>
        /// </summary>
        [MediumSkippableFact]
        public async Task YearBuiltDataPostgreSQLConverter_RemovePredictedYearBuilts()
        {
            (ConnectionData connectionData_Main, ConnectionData connectionData_Storage) = TestConnectionDatas();

            YearBuiltDataPostgreSQLConverter yearBuiltDataPostgreSQLConverter = new(connectionData_Main);

            // The stamp the runner writes is local time; the removal is keyed on ticks, so the kind must not matter.
            DateTime dateTime_Run1 = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Local);
            DateTime dateTime_Run2 = new(2026, 10, 5, 8, 30, 15, DateTimeKind.Utc);
            const string modelId_1 = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            const string modelId_2 = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

            await CleanupYearBuiltDataScratchAsync(connectionData_Main, connectionData_Storage);
            try
            {
                await SeedYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_A,
                    ScratchYearBuiltData("XUNIT-RP-1", null, null, new GIS.Classes.PredictedYearBuilt(dateTime_Run1, 1970, modelId_1)),
                    ScratchYearBuiltData("XUNIT-RP-2", 1955, "reviewer@example.com", new GIS.Classes.PredictedYearBuilt(dateTime_Run1, 1971, modelId_1), new GIS.Classes.PredictedYearBuilt(dateTime_Run2, 1972, modelId_2)),
                    ScratchYearBuiltData("XUNIT-RP-3", null, null, new GIS.Classes.PredictedYearBuilt(dateTime_Run2, 1973, modelId_2)));

                await SeedYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_B,
                    ScratchYearBuiltData("XUNIT-RP-4", null, null, new GIS.Classes.PredictedYearBuilt(dateTime_Run1, 1974, modelId_1)),
                    ScratchYearBuiltData("XUNIT-RP-5", null, null));

                int[] countyIds = [countyId_YearBuilt_A, countyId_YearBuilt_B];

                List<PredictedYearBuiltRunResult>? predictedYearBuiltRunResults = await yearBuiltDataPostgreSQLConverter.GetPredictedYearBuiltRunsAsync(countyIds);
                Assert.NotNull(predictedYearBuiltRunResults);
                Assert.Equal(3, predictedYearBuiltRunResults.Count);
                Assert.Contains(predictedYearBuiltRunResults, x => x.CountyId == countyId_YearBuilt_A && x.Ticks == dateTime_Run1.Ticks && x.ModelId == modelId_1 && x.Count == 2);
                Assert.Contains(predictedYearBuiltRunResults, x => x.CountyId == countyId_YearBuilt_A && x.Ticks == dateTime_Run2.Ticks && x.ModelId == modelId_2 && x.Count == 2);
                Assert.Contains(predictedYearBuiltRunResults, x => x.CountyId == countyId_YearBuilt_B && x.Ticks == dateTime_Run1.Ticks && x.ModelId == modelId_1 && x.Count == 1);

                // Dry run and over the limit: counted, nothing written.
                PredictedYearBuiltRemoveResult? predictedYearBuiltRemoveResult = await yearBuiltDataPostgreSQLConverter.RemovePredictedYearBuiltsAsync(countyIds, dateTime_Run1, null, true, 100);
                Assert.NotNull(predictedYearBuiltRemoveResult);
                Assert.Equal(3, predictedYearBuiltRemoveResult.Matched);
                Assert.Equal(0, predictedYearBuiltRemoveResult.Removed);
                Assert.Equal(["XUNIT-RP-1", "XUNIT-RP-2", "XUNIT-RP-4"], predictedYearBuiltRemoveResult.References.OrderBy(x => x));
                Assert.Equal(["XUNIT-RP-1", "XUNIT-RP-4"], predictedYearBuiltRemoveResult.EmptiedReferences.OrderBy(x => x));

                predictedYearBuiltRemoveResult = await yearBuiltDataPostgreSQLConverter.RemovePredictedYearBuiltsAsync(countyIds, dateTime_Run1, null, false, 2);
                Assert.NotNull(predictedYearBuiltRemoveResult);
                Assert.Equal(3, predictedYearBuiltRemoveResult.Matched);
                Assert.Equal(0, predictedYearBuiltRemoveResult.Removed);
                Assert.Empty(predictedYearBuiltRemoveResult.References);
                Assert.NotNull((await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_A, "XUNIT-RP-1")).Single().GetPredictedYearBuilt(dateTime_Run1));

                // The removal, paged two rows at a time so the keyset walk is exercised.
                predictedYearBuiltRemoveResult = await yearBuiltDataPostgreSQLConverter.RemovePredictedYearBuiltsAsync(countyIds, dateTime_Run1, null, false, 3, batchSize: 2);
                Assert.NotNull(predictedYearBuiltRemoveResult);
                Assert.False(predictedYearBuiltRemoveResult.DryRun);
                Assert.Equal(dateTime_Run1.Ticks, predictedYearBuiltRemoveResult.Ticks);
                Assert.Equal(3, predictedYearBuiltRemoveResult.Matched);
                Assert.Equal(3, predictedYearBuiltRemoveResult.Removed);
                Assert.Equal(["XUNIT-RP-1", "XUNIT-RP-4"], predictedYearBuiltRemoveResult.EmptiedReferences.OrderBy(x => x));

                // The emptied object is kept, under its own part.
                GIS.Classes.YearBuiltData yearBuiltData_1 = (await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_A, "XUNIT-RP-1")).Single();
                Assert.True(yearBuiltData_1.YearBuilts is null || !yearBuiltData_1.YearBuilts.Any());

                GIS.Classes.YearBuiltData yearBuiltData_4 = (await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_B, "XUNIT-RP-4")).Single();
                Assert.Null(yearBuiltData_4.GetPredictedYearBuilt(dateTime_Run1));
                Assert.Empty(await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_A, "XUNIT-RP-4"));

                // The other stamp and the user entry are untouched.
                GIS.Classes.YearBuiltData yearBuiltData_2 = (await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_A, "XUNIT-RP-2")).Single();
                Assert.Null(yearBuiltData_2.GetPredictedYearBuilt(dateTime_Run1));
                Assert.Equal((short)1972, yearBuiltData_2.GetPredictedYearBuilt(dateTime_Run2)?.Year);
                Assert.Equal(modelId_2, yearBuiltData_2.GetPredictedYearBuilt(dateTime_Run2)?.ModelId);
                Assert.Equal((short)1955, yearBuiltData_2.GetUserYearBuilt()?.Year);

                predictedYearBuiltRunResults = await yearBuiltDataPostgreSQLConverter.GetPredictedYearBuiltRunsAsync(countyIds);
                Assert.NotNull(predictedYearBuiltRunResults);
                PredictedYearBuiltRunResult predictedYearBuiltRunResult = Assert.Single(predictedYearBuiltRunResults);
                Assert.Equal(dateTime_Run2.Ticks, predictedYearBuiltRunResult.Ticks);

                // Removing it again matches nothing: the call is idempotent.
                predictedYearBuiltRemoveResult = await yearBuiltDataPostgreSQLConverter.RemovePredictedYearBuiltsAsync(countyIds, dateTime_Run1, null, false, 3);
                Assert.NotNull(predictedYearBuiltRemoveResult);
                Assert.Equal(0, predictedYearBuiltRemoveResult.Matched);

                // Narrowed to a reference: only that building loses the stamp.
                predictedYearBuiltRemoveResult = await yearBuiltDataPostgreSQLConverter.RemovePredictedYearBuiltsAsync(countyIds, dateTime_Run2, ["XUNIT-RP-3"], false, 10);
                Assert.NotNull(predictedYearBuiltRemoveResult);
                Assert.Equal(1, predictedYearBuiltRemoveResult.Removed);
                Assert.NotNull((await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_A, "XUNIT-RP-2")).Single().GetPredictedYearBuilt(dateTime_Run2));

                List<PredictedYearBuiltRunResult>? predictedYearBuiltRunResults_B = await yearBuiltDataPostgreSQLConverter.GetPredictedYearBuiltRunsAsync([countyId_YearBuilt_B]);
                Assert.NotNull(predictedYearBuiltRunResults_B);
                Assert.Empty(predictedYearBuiltRunResults_B);
            }
            finally
            {
                await CleanupYearBuiltDataScratchAsync(connectionData_Main, connectionData_Storage);
            }
        }

        /// <summary>
        /// Verifies <see cref="YearBuiltDataPostgreSQLConverter.RemoveUserYearBuiltsAsync(IEnumerable{int}?, IEnumerable{string}?, string?, bool, int, System.Threading.CancellationToken)"/> on the throwaway main test database.
        /// <para>Covers the owner path - the own entry is withdrawn from every object of the building across both parts, another user's building is reported and left whole, a building with no user entry is reported as not found - the dry run, the moderation path withdrawing any entry, and that predictions and the emptied objects stay.</para>
        /// <para>Medium test (a live database server; under 0.5 s measured alone): it writes the scratch partitions of 990201 and 990202 and drops them before and after.</para>
        /// </summary>
        [MediumSkippableFact]
        public async Task YearBuiltDataPostgreSQLConverter_RemoveUserYearBuilts()
        {
            (ConnectionData connectionData_Main, ConnectionData connectionData_Storage) = TestConnectionDatas();

            YearBuiltDataPostgreSQLConverter yearBuiltDataPostgreSQLConverter = new(connectionData_Main);

            DateTime dateTime = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

            await CleanupYearBuiltDataScratchAsync(connectionData_Main, connectionData_Storage);
            try
            {
                await SeedYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_A,
                    ScratchYearBuiltData("XUNIT-RU-OWN", 1960, "Owner@Example.com", new GIS.Classes.PredictedYearBuilt(dateTime, 1975)),
                    ScratchYearBuiltData("XUNIT-RU-OTHER", 1961, "other@example.com"),
                    ScratchYearBuiltData("XUNIT-RU-NONE", null, null, new GIS.Classes.PredictedYearBuilt(dateTime, 1976)));

                // The same building holds a second object under the sibling part.
                await SeedYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_B,
                    ScratchYearBuiltData("XUNIT-RU-OWN", 1960, "owner@example.com"));

                int[] countyIds = [countyId_YearBuilt_A, countyId_YearBuilt_B];
                string[] references = ["XUNIT-RU-OWN", "XUNIT-RU-OTHER", "XUNIT-RU-NONE", "XUNIT-RU-MISSING"];

                UserYearBuiltRemoveResult? userYearBuiltRemoveResult = await yearBuiltDataPostgreSQLConverter.RemoveUserYearBuiltsAsync(countyIds, references, "owner@example.com", true);
                Assert.NotNull(userYearBuiltRemoveResult);
                Assert.True(userYearBuiltRemoveResult.DryRun);
                Assert.Equal(["XUNIT-RU-OWN"], userYearBuiltRemoveResult.RemovedReferences);
                Assert.Equal(["XUNIT-RU-OTHER"], userYearBuiltRemoveResult.NotOwnedReferences);
                Assert.Equal(["XUNIT-RU-MISSING", "XUNIT-RU-NONE"], userYearBuiltRemoveResult.NotFoundReferences.OrderBy(x => x));
                Assert.NotNull((await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_A, "XUNIT-RU-OWN")).Single().GetUserYearBuilt());

                userYearBuiltRemoveResult = await yearBuiltDataPostgreSQLConverter.RemoveUserYearBuiltsAsync(countyIds, references, "owner@example.com", false);
                Assert.NotNull(userYearBuiltRemoveResult);
                Assert.Equal(["XUNIT-RU-OWN"], userYearBuiltRemoveResult.RemovedReferences);

                GIS.Classes.YearBuiltData yearBuiltData_A = (await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_A, "XUNIT-RU-OWN")).Single();
                Assert.Null(yearBuiltData_A.GetUserYearBuilt());
                Assert.NotNull(yearBuiltData_A.GetPredictedYearBuilt(dateTime));

                // The emptied object under the sibling part is withdrawn too, and kept.
                GIS.Classes.YearBuiltData yearBuiltData_B = (await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_B, "XUNIT-RU-OWN")).Single();
                Assert.Null(yearBuiltData_B.GetUserYearBuilt());

                Assert.Equal((short)1961, (await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_A, "XUNIT-RU-OTHER")).Single().GetUserYearBuilt()?.Year);

                // Moderation: any user's entry.
                userYearBuiltRemoveResult = await yearBuiltDataPostgreSQLConverter.RemoveUserYearBuiltsAsync(countyIds, ["XUNIT-RU-OTHER"], null, false);
                Assert.NotNull(userYearBuiltRemoveResult);
                Assert.Equal(["XUNIT-RU-OTHER"], userYearBuiltRemoveResult.RemovedReferences);
                Assert.Null((await ScratchYearBuiltDatasAsync(yearBuiltDataPostgreSQLConverter, countyId_YearBuilt_A, "XUNIT-RU-OTHER")).Single().GetUserYearBuilt());

                Assert.Null(await yearBuiltDataPostgreSQLConverter.RemoveUserYearBuiltsAsync([], references, null, true));
                Assert.Null(await yearBuiltDataPostgreSQLConverter.RemoveUserYearBuiltsAsync(countyIds, null, null, true));
            }
            finally
            {
                await CleanupYearBuiltDataScratchAsync(connectionData_Main, connectionData_Storage);
            }
        }

        /// <summary>
        /// Builds every new year built maintenance result with every member populated, asserts the properties, round-trips the string form, checks the copy constructor directly and runs the serialization check.
        /// </summary>
        [Fact]
        public void YearBuiltDataMaintenanceResults()
        {
            YearBuiltDataRemoveResult yearBuiltDataRemoveResult = new(false, 10, 4, 4, ["XUNIT-A"]);
            YearBuiltDataRemoveResult yearBuiltDataRemoveResult_Copy = new(yearBuiltDataRemoveResult);
            Assert.Equal((false, 10, 4, 4), (yearBuiltDataRemoveResult_Copy.DryRun, yearBuiltDataRemoveResult_Copy.Limit, yearBuiltDataRemoveResult_Copy.Matched, yearBuiltDataRemoveResult_Copy.Removed));
            Assert.Equal(["XUNIT-A"], yearBuiltDataRemoveResult_Copy.UnmatchedReferences);
            Assert.Equal(4, Core.Convert.ToDiGi<YearBuiltDataRemoveResult>(Core.Convert.ToSystem_String(yearBuiltDataRemoveResult))?.FirstOrDefault()?.Removed);
            Core.xUnit.Query.SerializationCheck(yearBuiltDataRemoveResult);

            PredictedYearBuiltRemoveResult predictedYearBuiltRemoveResult = new(true, 100, 638950000000000000, 3, 0, ["XUNIT-A", "XUNIT-B"], ["XUNIT-B"]);
            PredictedYearBuiltRemoveResult predictedYearBuiltRemoveResult_Copy = new(predictedYearBuiltRemoveResult);
            Assert.Equal((true, 100, 638950000000000000L, 3, 0), (predictedYearBuiltRemoveResult_Copy.DryRun, predictedYearBuiltRemoveResult_Copy.Limit, predictedYearBuiltRemoveResult_Copy.Ticks, predictedYearBuiltRemoveResult_Copy.Matched, predictedYearBuiltRemoveResult_Copy.Removed));
            Assert.Equal(2, predictedYearBuiltRemoveResult_Copy.References.Count);
            Assert.Equal(["XUNIT-B"], predictedYearBuiltRemoveResult_Copy.EmptiedReferences);
            Assert.Equal(638950000000000000, Core.Convert.ToDiGi<PredictedYearBuiltRemoveResult>(Core.Convert.ToSystem_String(predictedYearBuiltRemoveResult))?.FirstOrDefault()?.Ticks);
            Core.xUnit.Query.SerializationCheck(predictedYearBuiltRemoveResult);

            UserYearBuiltRemoveResult userYearBuiltRemoveResult = new(false, ["XUNIT-A"], ["XUNIT-B"], ["XUNIT-C"]);
            UserYearBuiltRemoveResult userYearBuiltRemoveResult_Copy = new(userYearBuiltRemoveResult);
            Assert.False(userYearBuiltRemoveResult_Copy.DryRun);
            Assert.Equal(["XUNIT-A"], userYearBuiltRemoveResult_Copy.RemovedReferences);
            Assert.Equal(["XUNIT-B"], userYearBuiltRemoveResult_Copy.NotOwnedReferences);
            Assert.Equal(["XUNIT-C"], userYearBuiltRemoveResult_Copy.NotFoundReferences);
            Core.xUnit.Query.SerializationCheck(userYearBuiltRemoveResult);

            PredictedYearBuiltRunResult predictedYearBuiltRunResult = new(73482, 638950000000000000, "2e120f49", 5541);
            PredictedYearBuiltRunResult predictedYearBuiltRunResult_Copy = new(predictedYearBuiltRunResult);
            Assert.Equal((73482, 638950000000000000L, "2e120f49", 5541), (predictedYearBuiltRunResult_Copy.CountyId, predictedYearBuiltRunResult_Copy.Ticks, predictedYearBuiltRunResult_Copy.ModelId, predictedYearBuiltRunResult_Copy.Count));
            Assert.Equal("2e120f49", Core.Convert.ToDiGi<PredictedYearBuiltRunResult>(Core.Convert.ToSystem_String(predictedYearBuiltRunResult))?.FirstOrDefault()?.ModelId);
            Core.xUnit.Query.SerializationCheck(predictedYearBuiltRunResult);
            Core.xUnit.Query.SerializationCheck(new PredictedYearBuiltRunResult(73482, 638950000000000000, null, 1));

            BuildingDataYearBuiltUpdateResult buildingDataYearBuiltUpdateResult = new(10, 7, 2);
            BuildingDataYearBuiltUpdateResult buildingDataYearBuiltUpdateResult_Copy = new(buildingDataYearBuiltUpdateResult);
            Assert.Equal((10, 7, 2), (buildingDataYearBuiltUpdateResult_Copy.Matched, buildingDataYearBuiltUpdateResult_Copy.Updated, buildingDataYearBuiltUpdateResult_Copy.Cleared));
            Core.xUnit.Query.SerializationCheck(buildingDataYearBuiltUpdateResult);
        }
    }
}
