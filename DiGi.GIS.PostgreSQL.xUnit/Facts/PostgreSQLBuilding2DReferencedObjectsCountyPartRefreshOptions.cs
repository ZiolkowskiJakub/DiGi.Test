using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the defaults, which are what the tray task uses when nothing is set.
        /// <para>DryRun true is the contract: the tray row reports by default, and moving rows between partitions is something to opt into. BatchSize and CommandTimeout match the mover's defaults, so the task and the mover cannot drift apart.</para>
        /// <para>A null code set scopes a run to every multi-part county code, and a null report directory puts the report in the directory the application was launched from.</para>
        /// </summary>
        [Fact]
        public void PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions_Defaults()
        {
            PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions = new();

            Assert.True(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions.DryRun);
            Assert.Equal(1000, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions.BatchSize);
            Assert.Equal(600, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions.CommandTimeout);
            Assert.Null(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions.Codes);
            Assert.Null(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions.ReportDirectory);
        }

        /// <summary>
        /// Verifies that a populated instance survives a JSON round trip and a clone, with every property carried over.
        /// <para>Every member is populated: a member the instance leaves at its default is invisible to SerializationCheck, and a clone that forgot a collection would pass on a member never set in the first place.</para>
        /// </summary>
        [Fact]
        public void PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions_Serialization()
        {
            PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions = new()
            {
                BatchSize = 250,
                Codes = ["55417", "56029"],
                CommandTimeout = 120,
                DryRun = false,
                ReportDirectory = "reports"
            };

            string? text = Core.Convert.ToSystem_String(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions);
            Assert.False(string.IsNullOrWhiteSpace(text));

            PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions? postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions_Parsed = Core.Convert.ToDiGi<PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions>(text)?.FirstOrDefault();
            Assert.NotNull(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions_Parsed);

            Assert.Equal(250, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions_Parsed.BatchSize);
            Assert.False(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions_Parsed.DryRun);
            Assert.Equal(120, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions_Parsed.CommandTimeout);

            List<string>? codes = postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions_Parsed.Codes;
            Assert.NotNull(codes);
            Assert.Equal(2, codes.Count);
            Assert.Contains("55417", codes);
            Assert.Contains("56029", codes);

            Assert.Equal("reports", postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions_Parsed.ReportDirectory);

            PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions_Clone = new(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions);

            Assert.Equal(250, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions_Clone.BatchSize);
            Assert.False(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions_Clone.DryRun);
            Assert.Equal(120, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions_Clone.CommandTimeout);

            // The clone has to hold its own collections, or editing one set of options would rewrite the other.
            List<string>? codes_Clone = postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions_Clone.Codes;
            Assert.NotNull(codes_Clone);
            Assert.Equal(2, codes_Clone.Count);
            Assert.Contains("55417", codes_Clone);
            Assert.NotSame(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions.Codes, postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions_Clone.Codes);

            Assert.Equal("reports", postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions_Clone.ReportDirectory);

            Core.xUnit.Query.SerializationCheck(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions);
        }

        /// <summary>
        /// Verifies that an unset code collection stays unset through a round trip.
        /// <para>Null and empty mean opposite things here: null scopes a run to every multi-part county code, an empty set scopes it to none. A round trip that turned one into the other would silently change what an unattended run covers.</para>
        /// </summary>
        [Fact]
        public void PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions_Serialization_NullCodes()
        {
            PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions = new()
            {
                BatchSize = 250,
                Codes = null,
                CommandTimeout = 120,
                DryRun = false,
                ReportDirectory = "reports"
            };

            string? text = Core.Convert.ToSystem_String(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions);
            PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions? postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions_Parsed = Core.Convert.ToDiGi<PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions>(text)?.FirstOrDefault();

            Assert.NotNull(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions_Parsed);
            Assert.Null(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions_Parsed.Codes);

            Core.xUnit.Query.SerializationCheck(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions);
        }
    }
}
