using DiGi.Core.Classes;
using DiGi.GIS.Classes;
using DiGi.GIS.Interfaces;
using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.PostgreSQL.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="PostgreSQLStatisticalDataCollectionPopulateOptions"/> property assignment, copy constructor, and serialization work correctly.
        /// </summary>
        [Fact]
        public void PostgreSQLStatisticalDataCollectionPopulateOptions_RoundTrip()
        {
            PostgreSQLStatisticalDataCollectionPopulateOptions options = new()
            {
                Path = "test_directory/data.sdcf",
                Clear = true,
                BatchSize = 500,
                CommandTimeout = 300
            };

            Assert.Equal("test_directory/data.sdcf", options.Path);
            Assert.True(options.Clear);
            Assert.Equal(500, options.BatchSize);
            Assert.Equal(300, options.CommandTimeout);

            PostgreSQLStatisticalDataCollectionPopulateOptions clone = new(options);
            Assert.Equal("test_directory/data.sdcf", clone.Path);
            Assert.True(clone.Clear);
            Assert.Equal(500, clone.BatchSize);
            Assert.Equal(300, clone.CommandTimeout);

            Core.xUnit.Query.SerializationCheck(options);
        }

        /// <summary>
        /// Verifies that <see cref="PostgreSQLBuildingDataUpdateOptions"/> with <see cref="PostgreSQLBuildingDataUpdateOptions.Years"/> round trips and clones correctly.
        /// </summary>
        [Fact]
        public void PostgreSQLBuildingDataUpdateOptions_Years_Serialization()
        {
            PostgreSQLBuildingDataUpdateOptions options = new()
            {
                Years = new Range<int>(2010, 2024),
                BuildingDataUpdateTypes = [BuildingDataUpdateType.General, BuildingDataUpdateType.Statistical]
            };

            Assert.NotNull(options.Years);
            Assert.Equal(2010, options.Years.Min);
            Assert.Equal(2024, options.Years.Max);
            Assert.NotNull(options.BuildingDataUpdateTypes);
            Assert.Contains(BuildingDataUpdateType.Statistical, options.BuildingDataUpdateTypes);

            PostgreSQLBuildingDataUpdateOptions clone = new(options);
            Assert.NotNull(clone.Years);
            Assert.Equal(2010, clone.Years.Min);
            Assert.Equal(2024, clone.Years.Max);
            Assert.NotNull(clone.BuildingDataUpdateTypes);
            Assert.Contains(BuildingDataUpdateType.Statistical, clone.BuildingDataUpdateTypes);

            Core.xUnit.Query.SerializationCheck(options);
        }

        /// <summary>
        /// Verifies null checks and safety guards on <see cref="StatisticalDataCollectionPostgreSQLConverter"/> methods.
        /// </summary>
        [Fact]
        public async Task StatisticalDataCollectionPostgreSQLConverter_NullGuards()
        {
            StatisticalDataCollectionPostgreSQLConverter converter = new(null);

            Assert.Equal("statistical_data_collection", StatisticalDataCollectionPostgreSQLConverter.TableName);
            Assert.False(await converter.CreateTableAsync());
            Assert.False(await converter.ClearAsync());
            Assert.Empty(await converter.InsertAsync(null));
            Assert.Empty(await converter.InsertAsync([]));
            Assert.Null(await converter.GetStatisticalDataCollectionByIdAsync(null));
            Assert.Null(await converter.GetStatisticalDataCollectionByIdAsync(string.Empty));
            Assert.Null(await converter.GetStatisticalDataCollectionsByIdsAsync(null));
            Assert.Null(await converter.GetStatisticalDataCollectionDictionaryByIdsAsync(null));
            Assert.Null(await converter.GetStatisticalDataCollectionAsync(null));
            Assert.Null(await converter.GetStatisticalDataCollectionDictionaryAsync(null));
            Assert.Equal(-1, await converter.GetCountAsync());
            Assert.Null(await converter.GetEstimatedCountAsync());
            Assert.Null(await converter.GetIdsAsync());
            Assert.False(await converter.ContainsAsync((string?)null));
            Assert.False(await converter.ContainsAsync((StatisticalUnit?)null));
            Assert.Null(await converter.GetStatisticalDataNamesAsync((string?)null));
            Assert.Null(await converter.GetStatisticalDataNamesAsync((StatisticalUnit?)null));
            Assert.False(await converter.PopulateAsync(null));
            Assert.False(await converter.PopulateAsync("non_existent_file.sdcf"));
        }

        /// <summary>
        /// Verifies in-memory <see cref="StatisticalUnit"/> code resolution and dictionary creation against <see cref="StatisticalDataCollection"/>.
        /// </summary>
        [Fact]
        public void StatisticalDataCollectionPostgreSQLConverter_StatisticalUnitMatching()
        {
            string unitCodeString = "020101100000";
            UnitCode? unitCode = GIS.Create.UnitCode(unitCodeString);
            Assert.NotNull(unitCode);

            StatisticalUnit statisticalUnit = new(Guid.NewGuid(), unitCode, "Gmina Test", null);
            Assert.Equal(unitCodeString, statisticalUnit.Code);

            StatisticalDataCollection collection = new(Guid.NewGuid(), unitCode);
            Assert.Equal(unitCodeString, collection.Code);

            Dictionary<string, StatisticalDataCollection> dictionaryByCode = new()
            {
                [unitCodeString] = collection
            };

            Assert.True(dictionaryByCode.TryGetValue(statisticalUnit.Code!, out StatisticalDataCollection? matchedCollection));
            Assert.NotNull(matchedCollection);
            Assert.Equal(collection.Guid, matchedCollection.Guid);
        }

        /// <summary>
        /// Verifies that merging incoming <see cref="IStatisticalData"/> series into an existing <see cref="StatisticalDataCollection"/> preserves all series.
        /// </summary>
        [Fact]
        public void StatisticalDataCollectionPostgreSQLConverter_MergeLogic()
        {
            UnitCode? unitCode = GIS.Create.UnitCode("020101100000");

            StatisticalDataCollection existing = new(Guid.NewGuid(), unitCode);
            Dictionary<short, double> popValues = new() { { 2020, 15000 }, { 2021, 15200 } };
            StatisticalYearlyDoubleData populationData = new("Population", "P3142", popValues);
            existing.Add(populationData);

            Assert.True(existing.Contains("Population"));
            Assert.False(existing.Contains("Dwellings"));

            StatisticalDataCollection incoming = new(Guid.NewGuid(), unitCode);
            Dictionary<short, double> dwellValues = new() { { 2020, 5000 }, { 2021, 5100 } };
            StatisticalYearlyDoubleData dwellingsData = new("Dwellings", "P3143", dwellValues);
            incoming.Add(dwellingsData);

            IEnumerable<IStatisticalData> incomingDatas = incoming.GetStatisticalDatas<IStatisticalData>();
            foreach (IStatisticalData incomingData in incomingDatas)
            {
                existing.Add(incomingData);
            }

            Assert.True(existing.Contains("Population"));
            Assert.True(existing.Contains("Dwellings"));
            Assert.Equal(2, existing.Names.Count());

            IStatisticalData? retrievedPopulation = existing.GetStatisticalData("Population");
            IStatisticalData? retrievedDwellings = existing.GetStatisticalData("Dwellings");

            Assert.NotNull(retrievedPopulation);
            Assert.NotNull(retrievedDwellings);
        }

        /// <summary>
        /// Verifies CRUD, batch retrieval, StatisticalUnit matching, and merge-aware UPSERT operations in PostgreSQL against a live database.
        /// </summary>
        [Fact]
        public async Task StatisticalDataCollectionPostgreSQLConverter_Integration()
        {
            string path = System.IO.Path.Combine(AppContext.BaseDirectory, Constants.FileName.PostgreSQL_Main);
            if (!System.IO.File.Exists(path) || DiGi.PostgreSQL.Create.PostgreSQLConfigurationFile(path) is not PostgreSQLConfigurationFile postgreSQLConfigurationFile)
            {
                return;
            }

            ConnectionData? connectionData = DiGi.PostgreSQL.Create.ConnectionData(postgreSQLConfigurationFile);
            if (connectionData is null)
            {
                return;
            }

            StatisticalDataCollectionPostgreSQLConverter converter = new(connectionData);

            bool tableCreated = await converter.CreateTableAsync();
            Assert.True(tableCreated);

            string testCode1 = "999999999001";
            string testCode2 = "999999999002";

            UnitCode? unitCode1 = GIS.Create.UnitCode(testCode1);
            UnitCode? unitCode2 = GIS.Create.UnitCode(testCode2);

            StatisticalDataCollection collection1 = new(Guid.NewGuid(), unitCode1);
            collection1.Add(new StatisticalYearlyDoubleData("Population", "P1", new Dictionary<short, double> { { 2020, 1000 } }));

            StatisticalDataCollection collection2 = new(Guid.NewGuid(), unitCode2);
            collection2.Add(new StatisticalYearlyDoubleData("Population", "P1", new Dictionary<short, double> { { 2020, 2000 } }));

            List<string> inserted = await converter.InsertAsync([collection1, collection2]);
            Assert.Equal(2, inserted.Count);

            // Test Single retrieval
            StatisticalDataCollection? retrieved1 = await converter.GetStatisticalDataCollectionByIdAsync(testCode1);
            Assert.NotNull(retrieved1);
            Assert.Equal(testCode1, retrieved1.Code);
            Assert.True(retrieved1.Contains("Population"));

            // Test Batch retrieval
            List<StatisticalDataCollection>? batchRetrieved = await converter.GetStatisticalDataCollectionsByIdsAsync([testCode1, testCode2]);
            Assert.NotNull(batchRetrieved);
            Assert.Equal(2, batchRetrieved.Count);

            // Test Dictionary retrieval
            Dictionary<string, StatisticalDataCollection>? dict = await converter.GetStatisticalDataCollectionDictionaryByIdsAsync([testCode1, testCode2]);
            Assert.NotNull(dict);
            Assert.Equal(2, dict.Count);
            Assert.True(dict.ContainsKey(testCode1));
            Assert.True(dict.ContainsKey(testCode2));

            // Test StatisticalUnit retrieval
            StatisticalUnit unit1 = new(Guid.NewGuid(), unitCode1, "Unit 1", null);
            StatisticalUnit unit2 = new(Guid.NewGuid(), unitCode2, "Unit 2", null);

            StatisticalDataCollection? retrievedByUnit = await converter.GetStatisticalDataCollectionAsync(unit1);
            Assert.NotNull(retrievedByUnit);
            Assert.Equal(testCode1, retrievedByUnit.Code);

            Dictionary<StatisticalUnit, StatisticalDataCollection>? unitDict = await converter.GetStatisticalDataCollectionDictionaryAsync([unit1, unit2]);
            Assert.NotNull(unitDict);
            Assert.Equal(2, unitDict.Count);
            Assert.True(unitDict.ContainsKey(unit1));
            Assert.True(unitDict.ContainsKey(unit2));

            // Test Diagnostics
            long count = await converter.GetCountAsync();
            Assert.True(count >= 2);

            bool contains1 = await converter.ContainsAsync(testCode1);
            Assert.True(contains1);

            bool containsUnit = await converter.ContainsAsync(unit1);
            Assert.True(containsUnit);

            List<string>? names = await converter.GetStatisticalDataNamesAsync(testCode1);
            Assert.NotNull(names);
            Assert.Contains("Population", names);

            // Test Merge on re-insert: add "Dwellings" to collection 1
            StatisticalDataCollection collection1_Update = new(Guid.NewGuid(), unitCode1);
            collection1_Update.Add(new StatisticalYearlyDoubleData("Dwellings", "P2", new Dictionary<short, double> { { 2020, 400 } }));

            List<string> mergedInsert = await converter.InsertAsync([collection1_Update]);
            Assert.Single(mergedInsert);

            StatisticalDataCollection? mergedRetrieved = await converter.GetStatisticalDataCollectionByIdAsync(testCode1);
            Assert.NotNull(mergedRetrieved);
            Assert.True(mergedRetrieved.Contains("Population"), "Original series 'Population' must be preserved after merge.");
            Assert.True(mergedRetrieved.Contains("Dwellings"), "New series 'Dwellings' must be added after merge.");
        }
    }
}
