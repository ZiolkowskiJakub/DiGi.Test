using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the defaults, which are what the tray task uses when nothing is set.
        /// <para>The default update types are the two that do not reach outside the building table, so an unattended run does not pull occupancy records or measure neighbourhoods unless it is asked to.</para>
        /// <para>The timeout is well above the 30 second Npgsql default on purpose: a single subdivision is tens of thousands of buildings and the write carries every derived column of each of them.</para>
        /// <para>A null county set scopes a run to the whole country, and the radiuses are the four the stored table already has columns for.</para>
        /// </summary>
        [Fact]
        public void PostgreSQLBuildingDataUpdateOptions_Defaults()
        {
            PostgreSQLBuildingDataUpdateOptions postgreSQLBuildingDataUpdateOptions = new();

            Assert.Equal(600, postgreSQLBuildingDataUpdateOptions.CommandTimeout);
            Assert.Null(postgreSQLBuildingDataUpdateOptions.CountyIds);

            HashSet<BuildingDataUpdateType>? buildingDataUpdateTypes = postgreSQLBuildingDataUpdateOptions.BuildingDataUpdateTypes;
            Assert.NotNull(buildingDataUpdateTypes);
            Assert.Equal(2, buildingDataUpdateTypes.Count);
            Assert.Contains(BuildingDataUpdateType.General, buildingDataUpdateTypes);
            Assert.Contains(BuildingDataUpdateType.Database, buildingDataUpdateTypes);

            List<double>? radiuses = postgreSQLBuildingDataUpdateOptions.Radiuses;
            Assert.NotNull(radiuses);
            Assert.Equal<double>([200, 400, 600, 1000], radiuses);
        }

        /// <summary>
        /// Verifies that a populated instance survives a JSON round trip and a clone, with every property carried over.
        /// <para>The update types matter most here: they travel as an enum collection, and a round trip that dropped or renumbered them would silently change which columns a stored set of options writes.</para>
        /// </summary>
        [Fact]
        public void PostgreSQLBuildingDataUpdateOptions_Serialization()
        {
            PostgreSQLBuildingDataUpdateOptions postgreSQLBuildingDataUpdateOptions = new()
            {
                BuildingDataUpdateTypes = [BuildingDataUpdateType.Occupancy, BuildingDataUpdateType.RadialRatios],
                CommandTimeout = 120,
                CountyIds = [55417, 56029, 53477],
                Radiuses = [50, 150]
            };

            string? text = Core.Convert.ToSystem_String(postgreSQLBuildingDataUpdateOptions);
            Assert.False(string.IsNullOrWhiteSpace(text));

            PostgreSQLBuildingDataUpdateOptions? postgreSQLBuildingDataUpdateOptions_Parsed = Core.Convert.ToDiGi<PostgreSQLBuildingDataUpdateOptions>(text)?.FirstOrDefault();
            Assert.NotNull(postgreSQLBuildingDataUpdateOptions_Parsed);

            Assert.Equal(120, postgreSQLBuildingDataUpdateOptions_Parsed.CommandTimeout);

            HashSet<BuildingDataUpdateType>? buildingDataUpdateTypes = postgreSQLBuildingDataUpdateOptions_Parsed.BuildingDataUpdateTypes;
            Assert.NotNull(buildingDataUpdateTypes);
            Assert.Equal(2, buildingDataUpdateTypes.Count);
            Assert.Contains(BuildingDataUpdateType.Occupancy, buildingDataUpdateTypes);
            Assert.Contains(BuildingDataUpdateType.RadialRatios, buildingDataUpdateTypes);
            Assert.DoesNotContain(BuildingDataUpdateType.General, buildingDataUpdateTypes);

            HashSet<int>? countyIds = postgreSQLBuildingDataUpdateOptions_Parsed.CountyIds;
            Assert.NotNull(countyIds);
            Assert.Equal(3, countyIds.Count);
            Assert.Contains(55417, countyIds);
            Assert.Contains(56029, countyIds);
            Assert.Contains(53477, countyIds);

            Assert.NotNull(postgreSQLBuildingDataUpdateOptions_Parsed.Radiuses);
            Assert.Equal<double>([50, 150], postgreSQLBuildingDataUpdateOptions_Parsed.Radiuses);

            PostgreSQLBuildingDataUpdateOptions postgreSQLBuildingDataUpdateOptions_Clone = new(postgreSQLBuildingDataUpdateOptions);

            Assert.Equal(120, postgreSQLBuildingDataUpdateOptions_Clone.CommandTimeout);

            // The clone has to hold its own collections, or editing one set of options would rewrite the other.
            Assert.NotNull(postgreSQLBuildingDataUpdateOptions_Clone.BuildingDataUpdateTypes);
            Assert.Equal(2, postgreSQLBuildingDataUpdateOptions_Clone.BuildingDataUpdateTypes.Count);
            Assert.NotSame(postgreSQLBuildingDataUpdateOptions.BuildingDataUpdateTypes, postgreSQLBuildingDataUpdateOptions_Clone.BuildingDataUpdateTypes);

            Assert.NotNull(postgreSQLBuildingDataUpdateOptions_Clone.CountyIds);
            Assert.Equal(3, postgreSQLBuildingDataUpdateOptions_Clone.CountyIds.Count);
            Assert.NotSame(postgreSQLBuildingDataUpdateOptions.CountyIds, postgreSQLBuildingDataUpdateOptions_Clone.CountyIds);

            Assert.NotNull(postgreSQLBuildingDataUpdateOptions_Clone.Radiuses);
            Assert.Equal(2, postgreSQLBuildingDataUpdateOptions_Clone.Radiuses.Count);
            Assert.NotSame(postgreSQLBuildingDataUpdateOptions.Radiuses, postgreSQLBuildingDataUpdateOptions_Clone.Radiuses);

            Core.xUnit.Query.SerializationCheck(postgreSQLBuildingDataUpdateOptions);
        }

        /// <summary>
        /// Verifies that an unset county collection stays unset through a round trip.
        /// <para>Null and empty mean opposite things here: null scopes a run to every county in the country, an empty set scopes it to none. A round trip that turned one into the other would silently change what an unattended run covers.</para>
        /// </summary>
        [Fact]
        public void PostgreSQLBuildingDataUpdateOptions_Serialization_NullCountyIds()
        {
            PostgreSQLBuildingDataUpdateOptions postgreSQLBuildingDataUpdateOptions = new()
            {
                CountyIds = null
            };

            string? text = Core.Convert.ToSystem_String(postgreSQLBuildingDataUpdateOptions);
            PostgreSQLBuildingDataUpdateOptions? postgreSQLBuildingDataUpdateOptions_Parsed = Core.Convert.ToDiGi<PostgreSQLBuildingDataUpdateOptions>(text)?.FirstOrDefault();

            Assert.NotNull(postgreSQLBuildingDataUpdateOptions_Parsed);
            Assert.Null(postgreSQLBuildingDataUpdateOptions_Parsed.CountyIds);

            Core.xUnit.Query.SerializationCheck(postgreSQLBuildingDataUpdateOptions);
        }
    }
}
