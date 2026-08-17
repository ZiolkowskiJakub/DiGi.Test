using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the defaults, which are what a run uses when nothing is set.
        /// <para>The grid size is the one that governs the cost of a whole run - it enters as a square, so 50 rather than 10 is the difference between days and most of a year - and the origin has to stay at zero for a coarse sampling to be reusable by a finer one.</para>
        /// </summary>
        [Fact]
        public void PostgreSQLTerrainPointCreateTableOptions_Defaults()
        {
            PostgreSQLTerrainPointCreateTableOptions postgreSQLTerrainPointCreateTableOptions = new();

            Assert.Null(postgreSQLTerrainPointCreateTableOptions.CountyIds);
            Assert.Equal(50, postgreSQLTerrainPointCreateTableOptions.GridSize);
            Assert.Equal(0, postgreSQLTerrainPointCreateTableOptions.OriginX);
            Assert.Equal(0, postgreSQLTerrainPointCreateTableOptions.OriginY);
            Assert.Equal(128, postgreSQLTerrainPointCreateTableOptions.TileSize);
            Assert.Equal(16, postgreSQLTerrainPointCreateTableOptions.MaxConcurrentRequests);
            Assert.Equal(3, postgreSQLTerrainPointCreateTableOptions.RetryCount);
            Assert.Equal(500, postgreSQLTerrainPointCreateTableOptions.RetryDelayMilliseconds);
            Assert.False(postgreSQLTerrainPointCreateTableOptions.OverrideExisting);
            Assert.Equal(Core.Constants.Tolerance.MacroDistance, postgreSQLTerrainPointCreateTableOptions.Tolerance);
        }

        /// <summary>
        /// Verifies that a populated instance survives a JSON round trip and a clone, with every property carried over.
        /// <para>The tolerance is asserted on the clone in particular: the copy constructor used to leave it out, so a copied set of options silently reverted to the default while every other value came across intact.</para>
        /// </summary>
        [Fact]
        public void PostgreSQLTerrainPointCreateTableOptions_Serialization()
        {
            PostgreSQLTerrainPointCreateTableOptions postgreSQLTerrainPointCreateTableOptions = new()
            {
                CountyIds = [2405, 2412, 1465],
                GridSize = 10,
                MaxConcurrentRequests = 24,
                OriginX = 5,
                OriginY = 5,
                OverrideExisting = true,
                RetryCount = 5,
                RetryDelayMilliseconds = 250,
                TileSize = 64,
                Tolerance = 0.25
            };

            string? json = Core.Convert.ToSystem_String(postgreSQLTerrainPointCreateTableOptions);
            Assert.NotNull(json);

            PostgreSQLTerrainPointCreateTableOptions? postgreSQLTerrainPointCreateTableOptions_Json = Core.Convert.ToDiGi<PostgreSQLTerrainPointCreateTableOptions>(json)?.FirstOrDefault();
            Assert.NotNull(postgreSQLTerrainPointCreateTableOptions_Json);
            Assert.NotNull(postgreSQLTerrainPointCreateTableOptions_Json.CountyIds);
            Assert.Equal(3, postgreSQLTerrainPointCreateTableOptions_Json.CountyIds.Count);
            Assert.Contains(2412, postgreSQLTerrainPointCreateTableOptions_Json.CountyIds);
            Assert.Equal(10, postgreSQLTerrainPointCreateTableOptions_Json.GridSize);
            Assert.Equal(24, postgreSQLTerrainPointCreateTableOptions_Json.MaxConcurrentRequests);
            Assert.Equal(5, postgreSQLTerrainPointCreateTableOptions_Json.OriginX);
            Assert.Equal(5, postgreSQLTerrainPointCreateTableOptions_Json.OriginY);
            Assert.True(postgreSQLTerrainPointCreateTableOptions_Json.OverrideExisting);
            Assert.Equal(5, postgreSQLTerrainPointCreateTableOptions_Json.RetryCount);
            Assert.Equal(250, postgreSQLTerrainPointCreateTableOptions_Json.RetryDelayMilliseconds);
            Assert.Equal(64, postgreSQLTerrainPointCreateTableOptions_Json.TileSize);
            Assert.Equal(0.25, postgreSQLTerrainPointCreateTableOptions_Json.Tolerance);

            PostgreSQLTerrainPointCreateTableOptions postgreSQLTerrainPointCreateTableOptions_Clone = new(postgreSQLTerrainPointCreateTableOptions);

            Assert.Equal(0.25, postgreSQLTerrainPointCreateTableOptions_Clone.Tolerance);
            Assert.Equal(10, postgreSQLTerrainPointCreateTableOptions_Clone.GridSize);
            Assert.Equal(24, postgreSQLTerrainPointCreateTableOptions_Clone.MaxConcurrentRequests);
            Assert.Equal(5, postgreSQLTerrainPointCreateTableOptions_Clone.OriginX);
            Assert.Equal(5, postgreSQLTerrainPointCreateTableOptions_Clone.OriginY);
            Assert.True(postgreSQLTerrainPointCreateTableOptions_Clone.OverrideExisting);
            Assert.Equal(5, postgreSQLTerrainPointCreateTableOptions_Clone.RetryCount);
            Assert.Equal(250, postgreSQLTerrainPointCreateTableOptions_Clone.RetryDelayMilliseconds);
            Assert.Equal(64, postgreSQLTerrainPointCreateTableOptions_Clone.TileSize);

            // The clone has to hold its own set, or editing one set of options would rewrite the other.
            Assert.NotNull(postgreSQLTerrainPointCreateTableOptions_Clone.CountyIds);
            Assert.Equal(3, postgreSQLTerrainPointCreateTableOptions_Clone.CountyIds.Count);
            Assert.NotSame(postgreSQLTerrainPointCreateTableOptions.CountyIds, postgreSQLTerrainPointCreateTableOptions_Clone.CountyIds);

            Core.xUnit.Query.SerializationCheck(postgreSQLTerrainPointCreateTableOptions);
        }

        /// <summary>
        /// Verifies that a null set of counties survives a round trip and a clone as null, which is what asks for every county rather than for none.
        /// </summary>
        [Fact]
        public void PostgreSQLTerrainPointCreateTableOptions_NullCountyIds()
        {
            PostgreSQLTerrainPointCreateTableOptions postgreSQLTerrainPointCreateTableOptions = new()
            {
                CountyIds = null,
                GridSize = 100
            };

            PostgreSQLTerrainPointCreateTableOptions postgreSQLTerrainPointCreateTableOptions_Clone = new(postgreSQLTerrainPointCreateTableOptions);

            Assert.Null(postgreSQLTerrainPointCreateTableOptions_Clone.CountyIds);
            Assert.Equal(100, postgreSQLTerrainPointCreateTableOptions_Clone.GridSize);

            Core.xUnit.Query.SerializationCheck(postgreSQLTerrainPointCreateTableOptions);
        }

        /// <summary>
        /// Verifies that the grid sizes meant to be used together nest, so a county sampled coarsely can later be sampled finely without paying for the points it already holds.
        /// <para>This is a property of the numbers rather than of any one method, and it is the reason the options say to keep every grid size a whole multiple of the finest one intended.</para>
        /// </summary>
        [Fact]
        public void PostgreSQLTerrainPointCreateTableOptions_GridSizesNest()
        {
            double[] gridSizes = [100, 50, 10];

            for (int i = 0; i < gridSizes.Length - 1; i++)
            {
                Assert.Equal(0d, gridSizes[i] % gridSizes[i + 1]);
            }

            // A size sharing only a smaller factor does not nest, which is what the guidance warns against.
            Assert.NotEqual(0d, 100d % 30d);
        }
    }
}
