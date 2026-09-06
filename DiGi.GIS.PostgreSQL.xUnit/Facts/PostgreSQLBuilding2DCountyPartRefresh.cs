using DiGi.GIS.PostgreSQL.Classes;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the defaults of the county part refresh, which are what the tray task runs with when nothing is set.
        /// <para>Two of them decide what an unattended run does to stored data. <c>DryRun</c> is on, so a run reports and writes nothing until someone turns it off, and <c>ReferencedObjects</c> is on, so the rows keyed on a moved building travel with it rather than being left unreachable under the part it came from.</para>
        /// </summary>
        [Fact]
        public void PostgreSQLBuilding2DCountyPartRefreshOptions_Defaults()
        {
            PostgreSQLBuilding2DCountyPartRefreshOptions postgreSQLBuilding2DCountyPartRefreshOptions = new();

            Assert.True(postgreSQLBuilding2DCountyPartRefreshOptions.DryRun);
            Assert.True(postgreSQLBuilding2DCountyPartRefreshOptions.ReferencedObjects);
            Assert.Null(postgreSQLBuilding2DCountyPartRefreshOptions.Codes);
            Assert.Null(postgreSQLBuilding2DCountyPartRefreshOptions.ReportDirectory);
            Assert.Equal(5000, postgreSQLBuilding2DCountyPartRefreshOptions.BatchSize);
            Assert.Equal(Core.Constants.Tolerance.MacroDistance, postgreSQLBuilding2DCountyPartRefreshOptions.Tolerance);
        }

        /// <summary>
        /// Verifies that populated options survive a JSON round trip and a clone, with every property carried over.
        /// </summary>
        [Fact]
        public void PostgreSQLBuilding2DCountyPartRefreshOptions_Serialization()
        {
            PostgreSQLBuilding2DCountyPartRefreshOptions postgreSQLBuilding2DCountyPartRefreshOptions = new()
            {
                BatchSize = 2500,
                Codes = ["3020", "1206", "2401"],
                DryRun = false,
                ReferencedObjects = false,
                ReportDirectory = "C:/reports",
                Tolerance = 0.01
            };

            string? text = Core.Convert.ToSystem_String(postgreSQLBuilding2DCountyPartRefreshOptions);
            Assert.False(string.IsNullOrWhiteSpace(text));

            PostgreSQLBuilding2DCountyPartRefreshOptions? postgreSQLBuilding2DCountyPartRefreshOptions_Parsed = Core.Convert.ToDiGi<PostgreSQLBuilding2DCountyPartRefreshOptions>(text)?.FirstOrDefault();
            Assert.NotNull(postgreSQLBuilding2DCountyPartRefreshOptions_Parsed);

            Assert.Equal(2500, postgreSQLBuilding2DCountyPartRefreshOptions_Parsed.BatchSize);
            Assert.NotNull(postgreSQLBuilding2DCountyPartRefreshOptions_Parsed.Codes);
            Assert.Equal(3, postgreSQLBuilding2DCountyPartRefreshOptions_Parsed.Codes.Count);
            Assert.False(postgreSQLBuilding2DCountyPartRefreshOptions_Parsed.DryRun);
            Assert.False(postgreSQLBuilding2DCountyPartRefreshOptions_Parsed.ReferencedObjects);
            Assert.Equal("C:/reports", postgreSQLBuilding2DCountyPartRefreshOptions_Parsed.ReportDirectory);
            Assert.Equal(0.01, postgreSQLBuilding2DCountyPartRefreshOptions_Parsed.Tolerance);

            PostgreSQLBuilding2DCountyPartRefreshOptions postgreSQLBuilding2DCountyPartRefreshOptions_Clone = new(postgreSQLBuilding2DCountyPartRefreshOptions);

            Assert.NotNull(postgreSQLBuilding2DCountyPartRefreshOptions_Clone.Codes);
            Assert.Equal(3, postgreSQLBuilding2DCountyPartRefreshOptions_Clone.Codes.Count);
            Assert.False(postgreSQLBuilding2DCountyPartRefreshOptions_Clone.DryRun);
            Assert.False(postgreSQLBuilding2DCountyPartRefreshOptions_Clone.ReferencedObjects);

            Core.xUnit.Query.SerializationCheck(postgreSQLBuilding2DCountyPartRefreshOptions);
        }

        /// <summary>
        /// Verifies that a refresh result carries every tally through the constructor, a JSON round trip and a clone.
        /// <para>A dry run reports what it would move and nothing written, which is the shape asserted here: the decided count stands while the moved count stays at zero.</para>
        /// </summary>
        [Fact]
        public void PostgreSQLBuilding2DCountyPartRefreshResult_Serialization()
        {
            PostgreSQLBuilding2DCountyPartRefreshResult postgreSQLBuilding2DCountyPartRefreshResult = new(18, 781_470, 781_468, 0, 0, 2, 0, false);

            Assert.Equal(18, postgreSQLBuilding2DCountyPartRefreshResult.CodeCount);
            Assert.Equal(781_470, postgreSQLBuilding2DCountyPartRefreshResult.ReadCount);
            Assert.Equal(781_468, postgreSQLBuilding2DCountyPartRefreshResult.MoveCount);
            Assert.Equal(0, postgreSQLBuilding2DCountyPartRefreshResult.MovedCount);
            Assert.Equal(0, postgreSQLBuilding2DCountyPartRefreshResult.BlockedCount);
            Assert.Equal(2, postgreSQLBuilding2DCountyPartRefreshResult.UnresolvedCount);
            Assert.Equal(0, postgreSQLBuilding2DCountyPartRefreshResult.ReferencedObjectMovedCount);
            Assert.False(postgreSQLBuilding2DCountyPartRefreshResult.Cancelled);

            string? json = Core.Convert.ToSystem_String(postgreSQLBuilding2DCountyPartRefreshResult);
            Assert.NotNull(json);

            PostgreSQLBuilding2DCountyPartRefreshResult? postgreSQLBuilding2DCountyPartRefreshResult_Json = Core.Convert.ToDiGi<PostgreSQLBuilding2DCountyPartRefreshResult>(json)?.FirstOrDefault();
            Assert.NotNull(postgreSQLBuilding2DCountyPartRefreshResult_Json);

            Assert.Equal(781_468, postgreSQLBuilding2DCountyPartRefreshResult_Json.MoveCount);
            Assert.Equal(0, postgreSQLBuilding2DCountyPartRefreshResult_Json.MovedCount);
            Assert.Equal(2, postgreSQLBuilding2DCountyPartRefreshResult_Json.UnresolvedCount);

            PostgreSQLBuilding2DCountyPartRefreshResult postgreSQLBuilding2DCountyPartRefreshResult_Clone = new(postgreSQLBuilding2DCountyPartRefreshResult);

            Assert.Equal(18, postgreSQLBuilding2DCountyPartRefreshResult_Clone.CodeCount);
            Assert.Equal(781_470, postgreSQLBuilding2DCountyPartRefreshResult_Clone.ReadCount);

            Core.xUnit.Query.SerializationCheck(postgreSQLBuilding2DCountyPartRefreshResult);
        }

        /// <summary>
        /// Verifies that a live run reports what it wrote and what the destination refused, separately.
        /// <para>A move blocked by the destination part is not a failure and not a success: both rows still exist, and someone has to decide which to keep. Counting it as moved would report the county as repaired while a copy of the building was left behind.</para>
        /// </summary>
        [Fact]
        public void PostgreSQLBuilding2DCountyPartRefreshResult_Blocked()
        {
            PostgreSQLBuilding2DCountyPartRefreshResult postgreSQLBuilding2DCountyPartRefreshResult = new(2, 44_810, 2, 1, 1, 0, 3, false);

            Assert.Equal(2, postgreSQLBuilding2DCountyPartRefreshResult.MoveCount);
            Assert.Equal(1, postgreSQLBuilding2DCountyPartRefreshResult.MovedCount);
            Assert.Equal(1, postgreSQLBuilding2DCountyPartRefreshResult.BlockedCount);
            Assert.Equal(3, postgreSQLBuilding2DCountyPartRefreshResult.ReferencedObjectMovedCount);

            Core.xUnit.Query.SerializationCheck(postgreSQLBuilding2DCountyPartRefreshResult);
        }
    }
}
