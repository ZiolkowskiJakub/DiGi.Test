using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the defaults, which are what the tray task uses when nothing is set.
        /// <para>Two of them decide what an unattended run does to stored data. <c>UpdateSubdivisionIds</c> is on, so a run writes to <c>orto_datas</c> as well as queuing downloads, and <c>OverrideExisting</c> is off, so it queues only what is missing rather than asking for every orthophoto in the county again.</para>
        /// <para>The timeout is well above the 30 second Npgsql default on purpose: these are bulk statements over a partitioned table, and the default turned a slow county into a failed run.</para>
        /// </summary>
        [Fact]
        public void PostgreSQLOrtoDatasRefreshOptions_Defaults()
        {
            PostgreSQLOrtoDatasRefreshOptions postgreSQLOrtoDatasRefreshOptions = new();

            Assert.Equal(1000, postgreSQLOrtoDatasRefreshOptions.BatchSize);
            Assert.Equal(600, postgreSQLOrtoDatasRefreshOptions.CommandTimeout);
            Assert.Null(postgreSQLOrtoDatasRefreshOptions.CountyIds);
            Assert.False(postgreSQLOrtoDatasRefreshOptions.OverrideExisting);
            Assert.True(postgreSQLOrtoDatasRefreshOptions.UpdateSubdivisionIds);
        }

        /// <summary>
        /// Verifies that a populated instance survives a JSON round trip and a clone, with every property carried over.
        /// </summary>
        [Fact]
        public void PostgreSQLOrtoDatasRefreshOptions_Serialization()
        {
            PostgreSQLOrtoDatasRefreshOptions postgreSQLOrtoDatasRefreshOptions = new()
            {
                BatchSize = 250,
                CommandTimeout = 120,
                CountyIds = [55417, 56029, 53477],
                OverrideExisting = true,
                UpdateSubdivisionIds = false
            };

            string? text = Core.Convert.ToSystem_String(postgreSQLOrtoDatasRefreshOptions);
            Assert.False(string.IsNullOrWhiteSpace(text));

            PostgreSQLOrtoDatasRefreshOptions? postgreSQLOrtoDatasRefreshOptions_Parsed = Core.Convert.ToDiGi<PostgreSQLOrtoDatasRefreshOptions>(text)?.FirstOrDefault();
            Assert.NotNull(postgreSQLOrtoDatasRefreshOptions_Parsed);

            Assert.Equal(250, postgreSQLOrtoDatasRefreshOptions_Parsed.BatchSize);
            Assert.Equal(120, postgreSQLOrtoDatasRefreshOptions_Parsed.CommandTimeout);
            Assert.True(postgreSQLOrtoDatasRefreshOptions_Parsed.OverrideExisting);
            Assert.False(postgreSQLOrtoDatasRefreshOptions_Parsed.UpdateSubdivisionIds);

            HashSet<int>? countyIds = postgreSQLOrtoDatasRefreshOptions_Parsed.CountyIds;
            Assert.NotNull(countyIds);
            Assert.Equal(3, countyIds.Count);
            Assert.Contains(55417, countyIds);
            Assert.Contains(56029, countyIds);
            Assert.Contains(53477, countyIds);

            PostgreSQLOrtoDatasRefreshOptions postgreSQLOrtoDatasRefreshOptions_Clone = new(postgreSQLOrtoDatasRefreshOptions);

            Assert.Equal(250, postgreSQLOrtoDatasRefreshOptions_Clone.BatchSize);
            Assert.Equal(120, postgreSQLOrtoDatasRefreshOptions_Clone.CommandTimeout);
            Assert.True(postgreSQLOrtoDatasRefreshOptions_Clone.OverrideExisting);
            Assert.False(postgreSQLOrtoDatasRefreshOptions_Clone.UpdateSubdivisionIds);

            // The clone has to hold its own set, or editing one set of options would rewrite the other.
            Assert.NotNull(postgreSQLOrtoDatasRefreshOptions_Clone.CountyIds);
            Assert.Equal(3, postgreSQLOrtoDatasRefreshOptions_Clone.CountyIds.Count);
            Assert.NotSame(postgreSQLOrtoDatasRefreshOptions.CountyIds, postgreSQLOrtoDatasRefreshOptions_Clone.CountyIds);

            Core.xUnit.Query.SerializationCheck(postgreSQLOrtoDatasRefreshOptions);
        }

        /// <summary>
        /// Verifies that an unset county collection stays unset through a round trip.
        /// <para>Null and empty mean opposite things here: null scopes a run to every county in the country, an empty set scopes it to none. A round trip that turned one into the other would silently change what an unattended run covers.</para>
        /// </summary>
        [Fact]
        public void PostgreSQLOrtoDatasRefreshOptions_Serialization_NullCountyIds()
        {
            PostgreSQLOrtoDatasRefreshOptions postgreSQLOrtoDatasRefreshOptions = new()
            {
                CountyIds = null
            };

            string? text = Core.Convert.ToSystem_String(postgreSQLOrtoDatasRefreshOptions);
            PostgreSQLOrtoDatasRefreshOptions? postgreSQLOrtoDatasRefreshOptions_Parsed = Core.Convert.ToDiGi<PostgreSQLOrtoDatasRefreshOptions>(text)?.FirstOrDefault();

            Assert.NotNull(postgreSQLOrtoDatasRefreshOptions_Parsed);
            Assert.Null(postgreSQLOrtoDatasRefreshOptions_Parsed.CountyIds);

            Core.xUnit.Query.SerializationCheck(postgreSQLOrtoDatasRefreshOptions);
        }
    }
}
