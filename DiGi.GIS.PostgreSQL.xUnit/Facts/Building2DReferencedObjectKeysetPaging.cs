using DiGi.Analytical.Building.Enums;
using DiGi.GIS.PostgreSQL.Classes;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// A <see cref="BuildingModelPostgreSQLConverter"/> pointed at a scratch table of the building model shape, so the integration facts of this file never touch the rows a development database holds.
        /// </summary>
        private sealed class ScratchBuildingModelPostgreSQLConverter : BuildingModelPostgreSQLConverter
        {
            private readonly string tableName;

            /// <summary>
            /// Initializes a new instance of the <see cref="ScratchBuildingModelPostgreSQLConverter"/> class.
            /// </summary>
            /// <param name="connectionData">The connection data the scratch table is created in.</param>
            public ScratchBuildingModelPostgreSQLConverter(ConnectionData? connectionData)
                : base(connectionData, BuildingModelDetailLevel.Component)
            {
                tableName = $"scratch_building_model_{Guid.NewGuid():N}";
            }

            /// <summary>
            /// Gets the name of the scratch table.
            /// </summary>
            public override string TableName => tableName;
        }

        /// <summary>
        /// Seeds the scratch table with one stored object per identifier through the upsert the application itself uses, so the rows the facts read back are exactly what a real run would hold.
        /// </summary>
        /// <param name="buildingModelPostgreSQLConverter">The scratch converter the rows are written through.</param>
        /// <param name="countyId">The identifier of the scratch county partition the rows are filed under.</param>
        /// <param name="objects">The stored objects to seed.</param>
        /// <returns>A task representing the seeding. The identifiers are written back onto the objects the upsert returned them for.</returns>
        private static async Task SeedAsync(BuildingModelPostgreSQLConverter buildingModelPostgreSQLConverter, int countyId, List<BuildingModel> objects)
        {
            PostgreSQLUpdateResult? result = await buildingModelPostgreSQLConverter.UpdateAsync(objects);
            Assert.NotNull(result);
            Assert.Equal(objects.Count, result.Ids.Count);
        }

        /// <summary>
        /// Drops the scratch table and every partition the facts created in it.
        /// </summary>
        /// <param name="buildingModelPostgreSQLConverter">The scratch converter naming the table.</param>
        /// <returns>A task representing the cleanup.</returns>
        private static async Task DropAsync(BuildingModelPostgreSQLConverter buildingModelPostgreSQLConverter)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(buildingModelPostgreSQLConverter.ConnectionData);
            Assert.NotNull(npgsqlConnection);

            await npgsqlConnection.OpenAsync();

            await using NpgsqlCommand npgsqlCommand = new($"DROP TABLE IF EXISTS {buildingModelPostgreSQLConverter.TableName};", npgsqlConnection);
            await npgsqlCommand.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Serializes a JSON node with the members of every object ordered by key, so two nodes carrying the same content compare equal regardless of the order their members were inserted in or stored in.
        /// <para>PostgreSQL's jsonb sorts the keys of an object on write, so a value read back from the table carries a different member order than the value that was sent; the comparisons this exists for are over content, not order.</para>
        /// </summary>
        /// <param name="node">The node to serialize.</param>
        /// <returns>The canonical JSON text of the node.</returns>
        private static string CanonicalJson(JsonNode? node)
        {
            if (node is JsonObject jsonObject)
            {
                List<string> parts = [];
                foreach ((string key, JsonNode? value) in jsonObject.OrderBy(x => x.Key, StringComparer.Ordinal))
                {
                    parts.Add("\"" + key + "\":" + CanonicalJson(value));
                }

                return "{" + string.Join(",", parts) + "}";
            }

            return node?.ToJsonString() ?? "null";
        }

        /// <summary>
        /// Parses the JSON text of an object into a <see cref="JsonObject"/>.
        /// <para><see cref="JsonNode"/> is the type the parse methods of this runtime answer, so the object is selected with a cast - the idiom the converter itself uses when it maps the <c>object</c> column.</para>
        /// </summary>
        /// <param name="json">The JSON text of an object.</param>
        /// <returns>The parsed object.</returns>
        private static JsonObject ParseJsonObject(string json)
        {
            return (JsonObject)JsonNode.Parse(json)!;
        }

        /// <summary>
        /// Verifies that <see cref="Building2DReferencedObjectPostgreSQLConverter{TBuilding2DReferencedObject,TUniqueObject}.GetMaxIdAsync(Npgsql.NpgsqlConnection?, int, int, System.Threading.CancellationToken)"/> and its instance overload answer null, never throw, when there is no connection.
        /// </summary>
        [Fact]
        public async Task GetMaxIdAsync_NoConnection_ReturnsNull()
        {
            BuildingModelPostgreSQLConverter buildingModelPostgreSQLConverter = new(null, BuildingModelDetailLevel.Component);

            long? result_Static = await buildingModelPostgreSQLConverter.GetMaxIdAsync(null, 15678);
            Assert.Null(result_Static);

            long? result_Instance = await buildingModelPostgreSQLConverter.GetMaxIdAsync(15678);
            Assert.Null(result_Instance);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DReferencedObjectPostgreSQLConverter{TBuilding2DReferencedObject,TUniqueObject}.GetItemsByIdRangeAsync(Npgsql.NpgsqlConnection?, int, long, long, int, int, System.Threading.CancellationToken)"/> and its instance overload answer null, never throw, when there is no connection.
        /// </summary>
        [Fact]
        public async Task GetItemsByIdRangeAsync_NoConnection_ReturnsNull()
        {
            BuildingModelPostgreSQLConverter buildingModelPostgreSQLConverter = new(null, BuildingModelDetailLevel.Component);

            List<BuildingModel>? result_Static = await buildingModelPostgreSQLConverter.GetItemsByIdRangeAsync(null, 15678, 0, 10);
            Assert.Null(result_Static);

            List<BuildingModel>? result_Instance = await buildingModelPostgreSQLConverter.GetItemsByIdRangeAsync(15678, 0, 10);
            Assert.Null(result_Instance);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DReferencedObjectPostgreSQLConverter{TBuilding2DReferencedObject,TUniqueObject}.UpdateObjectPropertiesAsync(Npgsql.NpgsqlConnection?, Npgsql.NpgsqlTransaction?, int, string, System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{long, System.Text.Json.Nodes.JsonNode?}}, int, System.Threading.CancellationToken)"/> and its instance overload answer null, never throw, when there is no connection or no values.
        /// </summary>
        [Fact]
        public async Task UpdateObjectPropertiesAsync_NoConnectionOrValues_ReturnsNull()
        {
            BuildingModelPostgreSQLConverter buildingModelPostgreSQLConverter = new(null, BuildingModelDetailLevel.Component);

            HashSet<long>? result_NoConnection = await buildingModelPostgreSQLConverter.UpdateObjectPropertiesAsync(null, null, 15678, "BuildingInformation", [new KeyValuePair<long, JsonNode?>(1L, ParseJsonObject("{}"))]);
            Assert.Null(result_NoConnection);

            HashSet<long>? result_NoValues = await buildingModelPostgreSQLConverter.UpdateObjectPropertiesAsync(15678, "BuildingInformation", (IEnumerable<KeyValuePair<long, JsonNode?>>?)null);
            Assert.Null(result_NoValues);
        }

        /// <summary>
        /// Verifies against a database that the keyset paging the backfill walks on reads the rows of a county partition in identifier order, with the lower bound exclusive, the upper bound inclusive, and the page size respected.
        /// <para>The scratch table is created by the upsert and dropped in <see langword="finally"/>, so the rows of the development database are never touched. The conf resolves to a development database, so nothing measured here describes the deployed estate.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Storage.conf at a database before running.")]
        public async Task GetMaxIdAndItemsByIdRange_KeysetPaging()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            BuildingModelPostgreSQLConverter? buildingModelPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingModelPostgreSQLConverter>();
            Assert.NotNull(buildingModelPostgreSQLConverter);

            const int countyId = 999001;

            ScratchBuildingModelPostgreSQLConverter scratchConverter = new(buildingModelPostgreSQLConverter.ConnectionData);

            try
            {
                List<BuildingModel> objects = [];
                for (int i = 0; i < 5; i++)
                {
                    objects.Add(new BuildingModel
                    {
                        CountyId = countyId,
                        UniqueId = $"unique-{i}",
                        Reference = $"reference-{i}",
                        Object = ParseJsonObject($"{{\"Marker\": {i}}}")
                    });
                }

                await SeedAsync(scratchConverter, countyId, objects);

                long id_First = objects[0].Id;
                long id_Last = objects[4].Id;
                Assert.True(id_First < id_Last, "The identity identifiers are not in insertion order.");

                long? idMax = await scratchConverter.GetMaxIdAsync(countyId);
                Assert.Equal(id_Last, idMax);

                // An empty partition answers null, so a part with no rows is recorded rather than paged.
                long? idMax_Empty = await scratchConverter.GetMaxIdAsync(999002);
                Assert.Null(idMax_Empty);

                // The whole range in one page: ascending, both bounds as documented.
                List<BuildingModel>? items_All = await scratchConverter.GetItemsByIdRangeAsync(countyId, id_First - 1, id_Last, 100);
                Assert.NotNull(items_All);
                Assert.Equal(5, items_All.Count);

                for (int i = 1; i < items_All.Count; i++)
                {
                    Assert.True(items_All[i - 1].Id < items_All[i].Id, "The identifiers are not in ascending order.");
                }

                Assert.Equal(id_First, items_All[0].Id);
                Assert.Equal(id_Last, items_All[^1].Id);

                // The lower bound is exclusive: asking after the first identifier starts at the second row.
                List<BuildingModel>? items_AfterFirst = await scratchConverter.GetItemsByIdRangeAsync(countyId, id_First, id_Last, 100);
                Assert.NotNull(items_AfterFirst);
                Assert.Equal(4, items_AfterFirst.Count);
                Assert.Equal(objects[1].Id, items_AfterFirst[0].Id);

                // The page size is the ceiling of the page, not a suggestion.
                List<BuildingModel>? items_Limited = await scratchConverter.GetItemsByIdRangeAsync(countyId, 0, id_Last, 2);
                Assert.NotNull(items_Limited);
                Assert.Equal(2, items_Limited.Count);

                // Walking the range page by page lands on every row exactly once.
                HashSet<long> ids_Walked = [];
                long idAfter = 0;
                int pages = 0;
                while (true)
                {
                    List<BuildingModel>? items_Page = await scratchConverter.GetItemsByIdRangeAsync(countyId, idAfter, id_Last, 2);
                    Assert.NotNull(items_Page);
                    if (items_Page.Count == 0)
                    {
                        break;
                    }

                    pages++;
                    foreach (BuildingModel item in items_Page)
                    {
                        Assert.True(ids_Walked.Add(item.Id), $"The identifier {item.Id} was read twice.");
                    }

                    idAfter = items_Page[^1].Id;
                }

                Assert.Equal(3, pages);
                Assert.Equal(5, ids_Walked.Count);
            }
            finally
            {
                await DropAsync(scratchConverter);
            }
        }

        /// <summary>
        /// Verifies against a database that the single-key update the backfill writes with changes exactly the named key of the object JSON - nothing else of the stored row - answers with the identifiers it actually updated, and leaves a row untouched when the transaction is rolled back.
        /// <para>The scratch table is created by the upsert and dropped in <see langword="finally"/>, so the rows of the development database are never touched. The conf resolves to a development database, so nothing measured here describes the deployed estate.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Storage.conf at a database before running.")]
        public async Task UpdateObjectProperties_OnlyNamedKeyChanges()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            BuildingModelPostgreSQLConverter? buildingModelPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingModelPostgreSQLConverter>();
            Assert.NotNull(buildingModelPostgreSQLConverter);

            const int countyId = 999001;

            JsonObject object_Seeded = ParseJsonObject(
                """
                {
                    "BuildingInformation": { "Coordinates": { "Latitude": 0.0, "Longitude": 0.0 }, "UTC": 0 },
                    "Address": { "Street": "ul. Testowa 1", "City": "Warszawa" },
                    "ExtraMember": "written by older code"
                }
                """);

            ScratchBuildingModelPostgreSQLConverter scratchConverter = new(buildingModelPostgreSQLConverter.ConnectionData);

            try
            {
                List<BuildingModel> objects =
                [
                    new()
                    {
                        CountyId = countyId,
                        UniqueId = "unique-0",
                        Reference = "reference-0",
                        Object = (JsonObject)object_Seeded.DeepClone()!
                    },
                    new()
                    {
                        CountyId = countyId,
                        UniqueId = "unique-1",
                        Reference = "reference-1",
                        Object = (JsonObject)object_Seeded.DeepClone()!
                    }
                ];

                await SeedAsync(scratchConverter, countyId, objects);

                long id_0 = objects[0].Id;
                long id_1 = objects[1].Id;

                // The stamped value the backfill writes: the rest of the row must come back unchanged.
                JsonObject value_Stamped = ParseJsonObject("""{ "Coordinates": { "Latitude": 52.2543, "Longitude": 20.9108 }, "UTC": 16 }""");

                // The third pair names an identifier the partition does not hold - a row a concurrent run removed in the meantime.
                HashSet<long>? ids = await scratchConverter.UpdateObjectPropertiesAsync(countyId, "BuildingInformation",
                [
                    new KeyValuePair<long, JsonNode?>(id_0, value_Stamped),
                    new KeyValuePair<long, JsonNode?>(id_1, value_Stamped),
                    new KeyValuePair<long, JsonNode?>(id_0 + 1000000, value_Stamped)
                ]);
                Assert.NotNull(ids);
                Assert.Equal(2, ids.Count);
                Assert.Contains(id_0, ids);
                Assert.Contains(id_1, ids);

                List<BuildingModel>? items = await scratchConverter.GetItemsByIdsAsync([id_0, id_1], countyId);
                Assert.NotNull(items);
                Assert.Equal(2, items.Count);

                foreach (BuildingModel item in items)
                {
                    Assert.NotNull(item.Object);

                    // The named key carries the new value. The comparison is over content, not over the
                    // member order: jsonb sorts the keys of an object on write, so the value read back
                    // carries a different order than the value that was sent.
                    Assert.Equal(CanonicalJson(value_Stamped), CanonicalJson(item.Object!["BuildingInformation"]));

                    // Every other member of the object is untouched, member for member.
                    JsonObject object_Before = (JsonObject)object_Seeded.DeepClone()!;
                    object_Before.Remove("BuildingInformation");
                    JsonObject object_After = (JsonObject)item.Object!.DeepClone()!;
                    object_After.Remove("BuildingInformation");
                    Assert.Equal(CanonicalJson(object_Before), CanonicalJson(object_After));

                    // The addressing columns of the row are untouched.
                    Assert.Equal(countyId, item.CountyId);
                    Assert.NotNull(item.UniqueId);
                    Assert.StartsWith("unique-", item.UniqueId);
                    Assert.NotNull(item.Reference);
                    Assert.StartsWith("reference-", item.Reference);
                }

                // A rolled-back transaction leaves the row exactly as the committed update left it.
                await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(buildingModelPostgreSQLConverter.ConnectionData);
                Assert.NotNull(npgsqlConnection);
                await npgsqlConnection.OpenAsync();

                JsonObject value_RolledBack = ParseJsonObject("""{ "Coordinates": { "Latitude": 48.9, "Longitude": 14.05 }, "UTC": 16 }""");

                await using (NpgsqlTransaction? npgsqlTransaction = await npgsqlConnection.BeginTransactionAsync())
                {
                    HashSet<long>? ids_RolledBack = await scratchConverter.UpdateObjectPropertiesAsync(npgsqlConnection, npgsqlTransaction, countyId, "BuildingInformation",
                    [
                        new KeyValuePair<long, JsonNode?>(id_0, value_RolledBack)
                    ]);
                    Assert.NotNull(ids_RolledBack);
                    Assert.Single(ids_RolledBack);

                    await npgsqlTransaction.RollbackAsync();
                }

                List<BuildingModel>? items_AfterRollback = await scratchConverter.GetItemsByIdsAsync([id_0], countyId);
                Assert.NotNull(items_AfterRollback);
                Assert.Single(items_AfterRollback);
                Assert.Equal(CanonicalJson(value_Stamped), CanonicalJson(items_AfterRollback[0].Object!["BuildingInformation"]));
            }
            finally
            {
                await DropAsync(scratchConverter);
            }
        }
    }
}
