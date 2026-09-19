using DiGi.GIS.PostgreSQL.Classes;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="YearBuiltDataPostgreSQLConverter.GetUserYearBuiltsByCountyIdAsync(NpgsqlConnection, int?, int, System.Threading.CancellationToken)"/>
        /// returns null when the connection is missing.
        /// </summary>
        [Fact]
        public async Task GetUserYearBuiltsByCountyIdAsync_NullConnection_ReturnsNull()
        {
            YearBuiltDataPostgreSQLConverter yearBuiltDataPostgreSQLConverter = new(null);
            Dictionary<string, short>? result = await yearBuiltDataPostgreSQLConverter.GetUserYearBuiltsByCountyIdAsync((NpgsqlConnection?)null, 5);
            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that the projected label read answers the same dictionary the incumbent selection keeps, over a fixture that exercises every branch of the rule.
        /// <para>The label is the user entry of a record where the deserialized object holds one, otherwise its first non-prediction entry - the object keeping the last entry of each source in stored order is what <c>DiGi.GIS.ML.Query.YearBuiltLabels</c> walks -; a reference holding several rows answers with the year of the oldest labelled row, in <c>(created_at, id)</c> order - the row <c>DiGi.GIS.ML.Query.YearBuiltLabels</c> keeps, because the bulk read of the same rows orders <c>created_at DESC, id DESC</c> and the selection overwrites by reference while walking it. The fixture pins both halves: <c>XUNIT-YB-MULTI</c> carries a user year of 1980 on its newer row and 1930 on its older one, and the answer has to be 1930.</para>
        /// <para>The branches: a record with both a user entry and a prediction answers the user year; a record of predictions only contributes nothing; an entry stored after the prediction still counts; an empty or absent <c>YearBuilts</c> contributes nothing; a non-prediction entry of a kind this code base does not implement yet answers its year, because the selection is on the source not being a prediction rather than on the entry being a user one; two entries of one source answer the one stored last of it, because that is the one the deserialized object keeps - <c>XUNIT-YB-TWOUSER</c> and <c>XUNIT-YB-DUPNONPRED</c> pin it; and a row filed under the sibling part of the same county is out of scope of the named part and in scope of the county-less read.</para>
        /// <para>The <c>XUNIT-YB-FUTURESRC</c> row names a type that does not exist, and it is the one fixture the incumbent path cannot read - it pins the branch rather than a parity case, and the parity itself is the gate of the issue against the deployed data.</para>
        /// <para>Skipped by default: it creates and drops county partitions <c>990101</c>, <c>990102</c> and <c>990103</c>, so it needs <c>GIS_PostgreSQL_Main.conf</c> beside the test assembly pointing at a scratch database - never the deployed one. On a database holding real county partitions the county-less read answers their labels too, which is why that half asserts the fixture's entries rather than the whole dictionary.</para>
        /// </summary>
        [Fact(Skip = "Creates and drops partitions. Point GIS_PostgreSQL_Main.conf at a scratch database before running.")]
        public async Task GetUserYearBuiltsByCountyIdAsync_LabelSelection_Integration()
        {
            const int countyId = 990101;
            const int countyId_Sibling = 990102;

            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            YearBuiltDataPostgreSQLConverter? yearBuiltDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<YearBuiltDataPostgreSQLConverter>();
            Assert.NotNull(yearBuiltDataPostgreSQLConverter);

            ConnectionData? connectionData = yearBuiltDataPostgreSQLConverter.ConnectionData;
            Assert.NotNull(connectionData);

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(connectionData);
            Assert.NotNull(npgsqlConnection);

            await npgsqlConnection.OpenAsync();

            try
            {
                string tableName = yearBuiltDataPostgreSQLConverter.TableName;
                await ExecuteAsync(npgsqlConnection, $"DROP TABLE IF EXISTS {tableName}_{countyId};");
                await ExecuteAsync(npgsqlConnection, $"DROP TABLE IF EXISTS {tableName}_{countyId_Sibling};");

                Assert.True(await npgsqlConnection.TableAsync_Building2DReferencedObject(tableName));
                Assert.True(await npgsqlConnection.TableAsync_Building2DReferencedObject_Partition(tableName, countyId));
                Assert.True(await npgsqlConnection.TableAsync_Building2DReferencedObject_Partition(tableName, countyId_Sibling));

                // The stored shape of the object column, from the round-trip fact of the same table: the entry
                // is discriminated by _type, and the Source the in-memory dictionary keys on is not a field of it.
                string userType = "DiGi.GIS.Classes.UserYearBuilt,DiGi.GIS";
                string predictionType = "DiGi.GIS.Classes.PredictedYearBuilt,DiGi.GIS";
                string futureType = "DiGi.GIS.Classes.OtherYearBuilt,DiGi.GIS";

                string createdAt_Old = "2026-01-01T00:00:00Z";
                string createdAt_New = "2026-06-01T00:00:00Z";

                List<string> inserts =
                [
                    // The user year and the incumbent model's answer on one record: the user year is the label.
                    $"INSERT INTO {tableName} (county_id, unique_id, reference, object, created_at) VALUES ({countyId}, 'xunit-yb-1', 'XUNIT-YB-USER-PRED', '{{\"_type\":\"DiGi.GIS.Classes.YearBuiltData,DiGi.GIS\",\"Guid\":\"11111111-0000-0000-0000-000000000001\",\"YearBuilts\":[{{\"_type\":\"{userType}\",\"Year\":1975}},{{\"_type\":\"{predictionType}\",\"Year\":2008,\"DateTime\":\"2025-05-29T09:41:47.8773778+02:00\"}}],\"Reference\":\"XUNIT-YB-USER-PRED\"}}'::jsonb, '{createdAt_Old}')",

                    // A prediction without a user year is an unlabelled building, not a building whose year is zero.
                    $"INSERT INTO {tableName} (county_id, unique_id, reference, object, created_at) VALUES ({countyId}, 'xunit-yb-2', 'XUNIT-YB-PRED-ONLY', '{{\"_type\":\"DiGi.GIS.Classes.YearBuiltData,DiGi.GIS\",\"Guid\":\"11111111-0000-0000-0000-000000000002\",\"YearBuilts\":[{{\"_type\":\"{predictionType}\",\"Year\":2008,\"DateTime\":\"2025-05-29T09:41:47.8773778+02:00\"}}],\"Reference\":\"XUNIT-YB-PRED-ONLY\"}}'::jsonb, '{createdAt_Old}')",

                    // Two rows of one reference, the newer one carrying the later year: the oldest usable row wins.
                    $"INSERT INTO {tableName} (county_id, unique_id, reference, object, created_at) VALUES ({countyId}, 'xunit-yb-3', 'XUNIT-YB-MULTI', '{{\"_type\":\"DiGi.GIS.Classes.YearBuiltData,DiGi.GIS\",\"Guid\":\"11111111-0000-0000-0000-000000000003\",\"YearBuilts\":[{{\"_type\":\"{userType}\",\"Year\":1930}}],\"Reference\":\"XUNIT-YB-MULTI\"}}'::jsonb, '{createdAt_Old}')",
                    $"INSERT INTO {tableName} (county_id, unique_id, reference, object, created_at) VALUES ({countyId}, 'xunit-yb-4', 'XUNIT-YB-MULTI', '{{\"_type\":\"DiGi.GIS.Classes.YearBuiltData,DiGi.GIS\",\"Guid\":\"11111111-0000-0000-0000-000000000004\",\"YearBuilts\":[{{\"_type\":\"{userType}\",\"Year\":1980}}],\"Reference\":\"XUNIT-YB-MULTI\"}}'::jsonb, '{createdAt_New}')",

                    // The user entry stored after the prediction: array order decides nothing for it.
                    $"INSERT INTO {tableName} (county_id, unique_id, reference, object, created_at) VALUES ({countyId}, 'xunit-yb-5', 'XUNIT-YB-PRED-FIRST', '{{\"_type\":\"DiGi.GIS.Classes.YearBuiltData,DiGi.GIS\",\"Guid\":\"11111111-0000-0000-0000-000000000005\",\"YearBuilts\":[{{\"_type\":\"{predictionType}\",\"Year\":2008,\"DateTime\":\"2025-05-29T09:41:47.8773778+02:00\"}},{{\"_type\":\"{userType}\",\"Year\":1955}}],\"Reference\":\"XUNIT-YB-PRED-FIRST\"}}'::jsonb, '{createdAt_Old}')",

                    // An empty history and an absent history both contribute nothing.
                    $"INSERT INTO {tableName} (county_id, unique_id, reference, object, created_at) VALUES ({countyId}, 'xunit-yb-6', 'XUNIT-YB-EMPTY', '{{\"_type\":\"DiGi.GIS.Classes.YearBuiltData,DiGi.GIS\",\"Guid\":\"11111111-0000-0000-0000-000000000006\",\"YearBuilts\":[],\"Reference\":\"XUNIT-YB-EMPTY\"}}'::jsonb, '{createdAt_Old}')",
                    $"INSERT INTO {tableName} (county_id, unique_id, reference, object, created_at) VALUES ({countyId}, 'xunit-yb-7', 'XUNIT-YB-NOARR', '{{\"_type\":\"DiGi.GIS.Classes.YearBuiltData,DiGi.GIS\",\"Guid\":\"11111111-0000-0000-0000-000000000007\",\"Reference\":\"XUNIT-YB-NOARR\"}}'::jsonb, '{createdAt_Old}')",

                    // A non-prediction source this code base does not implement yet still counts as ground truth,
                    // because the selection is on the source not being a prediction.
                    $"INSERT INTO {tableName} (county_id, unique_id, reference, object, created_at) VALUES ({countyId}, 'xunit-yb-8', 'XUNIT-YB-FUTURESRC', '{{\"_type\":\"DiGi.GIS.Classes.YearBuiltData,DiGi.GIS\",\"Guid\":\"11111111-0000-0000-0000-000000000008\",\"YearBuilts\":[{{\"_type\":\"{futureType}\",\"Year\":1999}}],\"Reference\":\"XUNIT-YB-FUTURESRC\"}}'::jsonb, '{createdAt_Old}')",

                    // The deserialized object holds its entries as a dictionary keyed by source, so of a source the
                    // last entry in stored order is the one the incumbent selection answers: two user entries keep
                    // the later year, and a repeated non-prediction source keeps the entry stored last of it.
                    $"INSERT INTO {tableName} (county_id, unique_id, reference, object, created_at) VALUES ({countyId}, 'xunit-yb-11', 'XUNIT-YB-TWOUSER', '{{\"_type\":\"DiGi.GIS.Classes.YearBuiltData,DiGi.GIS\",\"Guid\":\"11111111-0000-0000-0000-000000000011\",\"YearBuilts\":[{{\"_type\":\"{userType}\",\"Year\":1935}},{{\"_type\":\"{userType}\",\"Year\":1940}}],\"Reference\":\"XUNIT-YB-TWOUSER\"}}'::jsonb, '{createdAt_Old}')",
                    $"INSERT INTO {tableName} (county_id, unique_id, reference, object, created_at) VALUES ({countyId}, 'xunit-yb-12', 'XUNIT-YB-DUPNONPRED', '{{\"_type\":\"DiGi.GIS.Classes.YearBuiltData,DiGi.GIS\",\"Guid\":\"11111111-0000-0000-0000-000000000012\",\"YearBuilts\":[{{\"_type\":\"{futureType}\",\"Year\":1999}},{{\"_type\":\"{futureType}\",\"Year\":1988}}],\"Reference\":\"XUNIT-YB-DUPNONPRED\"}}'::jsonb, '{createdAt_Old}')",

                    // The sibling part holds its own label: out of scope of the named part, in scope of the county-less read.
                    $"INSERT INTO {tableName} (county_id, unique_id, reference, object, created_at) VALUES ({countyId_Sibling}, 'xunit-yb-9', 'XUNIT-YB-SIBLING', '{{\"_type\":\"DiGi.GIS.Classes.YearBuiltData,DiGi.GIS\",\"Guid\":\"11111111-0000-0000-0000-000000000009\",\"YearBuilts\":[{{\"_type\":\"{userType}\",\"Year\":1920}}],\"Reference\":\"XUNIT-YB-SIBLING\"}}'::jsonb, '{createdAt_Old}')"
                ];

                foreach (string insert in inserts)
                {
                    await ExecuteAsync(npgsqlConnection, insert);
                }

                Dictionary<string, short>? years_County = await yearBuiltDataPostgreSQLConverter.GetUserYearBuiltsByCountyIdAsync(countyId);
                Assert.NotNull(years_County);

                Dictionary<string, short> expected = new() { ["XUNIT-YB-USER-PRED"] = 1975, ["XUNIT-YB-MULTI"] = 1930, ["XUNIT-YB-PRED-FIRST"] = 1955, ["XUNIT-YB-FUTURESRC"] = 1999, ["XUNIT-YB-TWOUSER"] = 1940, ["XUNIT-YB-DUPNONPRED"] = 1988 };

                Assert.Equal(expected, years_County);

                // The county-less read answers every part: the sibling part's label joins the set, and on a
                // database holding other counties their labels join it too, so the fixture's entries are
                // asserted rather than the whole dictionary.
                Dictionary<string, short>? years_All = await yearBuiltDataPostgreSQLConverter.GetUserYearBuiltsByCountyIdAsync(null);
                Assert.NotNull(years_All);

                Assert.Equal((short)1920, years_All["XUNIT-YB-SIBLING"]);
                foreach (KeyValuePair<string, short> keyValuePair in expected)
                {
                    Assert.Equal(keyValuePair.Value, years_All[keyValuePair.Key]);
                }

                // A part holding rows but no label answers an empty set, not null: a result, not a failure.
                Assert.True(await npgsqlConnection.TableAsync_Building2DReferencedObject_Partition(tableName, 990103));
                await ExecuteAsync(npgsqlConnection, $"INSERT INTO {tableName} (county_id, unique_id, reference, object, created_at) VALUES (990103, 'xunit-yb-10', 'XUNIT-YB-NONE', '{{\"_type\":\"DiGi.GIS.Classes.YearBuiltData,DiGi.GIS\",\"Guid\":\"11111111-0000-0000-0000-000000000010\",\"YearBuilts\":[{{\"_type\":\"{predictionType}\",\"Year\":2008,\"DateTime\":\"2025-05-29T09:41:47.8773778+02:00\"}}],\"Reference\":\"XUNIT-YB-NONE\"}}'::jsonb, '{createdAt_Old}')");

                Dictionary<string, short>? years_Empty = await yearBuiltDataPostgreSQLConverter.GetUserYearBuiltsByCountyIdAsync(990103);
                Assert.NotNull(years_Empty);
                Assert.Empty(years_Empty);
            }
            finally
            {
                await ExecuteAsync(npgsqlConnection, $"DROP TABLE IF EXISTS {yearBuiltDataPostgreSQLConverter.TableName}_{countyId};");
                await ExecuteAsync(npgsqlConnection, $"DROP TABLE IF EXISTS {yearBuiltDataPostgreSQLConverter.TableName}_{countyId_Sibling};");
                await ExecuteAsync(npgsqlConnection, $"DROP TABLE IF EXISTS {yearBuiltDataPostgreSQLConverter.TableName}_990103;");
            }
        }

        /// <summary>
        /// Verifies, measured on the development database, that a bounded user entry is excluded from the label while a legacy exact entry is included.
        /// <para>A <c>YearBuiltRelation</c> of <c>AtOrBefore</c> or <c>After</c> is a verification, not a label: the read's <c>COALESCE((value-&gt;&gt;'YearBuiltRelation')::int, 0) = 0</c> filter admits only exact entries and legacy entries that carry no relation member at all.</para>
        /// <para>Skipped by default: it creates and drops county partitions and needs <c>GIS_PostgreSQL_Main.conf</c> beside the test assembly pointing at a scratch database - never the deployed one.</para>
        /// </summary>
        [Fact(Skip = "Creates and drops partitions. Point GIS_PostgreSQL_Main.conf at a scratch database before running.")]
        public async Task UserYearBuilts_ExcludeBounds_DevDb()
        {
            const int countyId = 990101;

            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            YearBuiltDataPostgreSQLConverter? yearBuiltDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<YearBuiltDataPostgreSQLConverter>();
            Assert.NotNull(yearBuiltDataPostgreSQLConverter);

            ConnectionData? connectionData = yearBuiltDataPostgreSQLConverter.ConnectionData;
            Assert.NotNull(connectionData);

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(connectionData);
            Assert.NotNull(npgsqlConnection);

            await npgsqlConnection.OpenAsync();

            string tableName = yearBuiltDataPostgreSQLConverter.TableName;
            try
            {
                await ExecuteAsync(npgsqlConnection, $"DROP TABLE IF EXISTS {tableName}_{countyId};");
                Assert.True(await npgsqlConnection.TableAsync_Building2DReferencedObject(tableName));
                Assert.True(await npgsqlConnection.TableAsync_Building2DReferencedObject_Partition(tableName, countyId));

                string userType = "DiGi.GIS.Classes.UserYearBuilt,DiGi.GIS";
                string createdAt = "2026-01-01T00:00:00Z";

                // A bounded user entry (AtOrBefore) is a verification, not a label: excluded from the result.
                await ExecuteAsync(npgsqlConnection, $"INSERT INTO {tableName} (county_id, unique_id, reference, object, created_at) VALUES ({countyId}, 'xunit-yb-bound-1', 'XUNIT-YB-BOUNDED', '{{\"_type\":\"DiGi.GIS.Classes.YearBuiltData,DiGi.GIS\",\"Guid\":\"33333333-0000-0000-0000-000000000001\",\"YearBuilts\":[{{\"_type\":\"{userType}\",\"Year\":1950,\"YearBuiltRelation\":1}}],\"Reference\":\"XUNIT-YB-BOUNDED\"}}'::jsonb, '{createdAt}')");

                // A legacy exact user entry (no YearBuiltRelation member) is a label: included in the result.
                await ExecuteAsync(npgsqlConnection, $"INSERT INTO {tableName} (county_id, unique_id, reference, object, created_at) VALUES ({countyId}, 'xunit-yb-exact-1', 'XUNIT-YB-EXACT', '{{\"_type\":\"DiGi.GIS.Classes.YearBuiltData,DiGi.GIS\",\"Guid\":\"33333333-0000-0000-0000-000000000002\",\"YearBuilts\":[{{\"_type\":\"{userType}\",\"Year\":1965}}],\"Reference\":\"XUNIT-YB-EXACT\"}}'::jsonb, '{createdAt}')");

                Dictionary<string, short>? result = await yearBuiltDataPostgreSQLConverter.GetUserYearBuiltsByCountyIdAsync(countyId);
                Assert.NotNull(result);

                Assert.Equal((short)1965, result["XUNIT-YB-EXACT"]);
                Assert.False(result.ContainsKey("XUNIT-YB-BOUNDED"), "a bounded user entry must not appear as a label");
            }
            finally
            {
                await ExecuteAsync(npgsqlConnection, $"DROP TABLE IF EXISTS {tableName}_{countyId};");
            }
        }
    }
}
