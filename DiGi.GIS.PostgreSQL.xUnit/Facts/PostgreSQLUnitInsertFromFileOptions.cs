using DiGi.GIS.PostgreSQL.Classes;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies default property values of <see cref="PostgreSQLUnitInsertFromFileOptions"/>.
        /// </summary>
        [Fact]
        public void PostgreSQLUnitInsertFromFileOptions_Defaults()
        {
            PostgreSQLUnitInsertFromFileOptions options = new();

            Assert.Null(options.Path);
            Assert.False(options.Clear);
            Assert.Equal(1000, options.BatchSize);
            Assert.Equal(600, options.CommandTimeout);
        }

        /// <summary>
        /// Verifies that <see cref="PostgreSQLUnitInsertFromFileOptions"/> survives JSON serialization and copy constructor cloning.
        /// </summary>
        [Fact]
        public void PostgreSQLUnitInsertFromFileOptions_Serialization()
        {
            PostgreSQLUnitInsertFromFileOptions options = new()
            {
                Path = "C:\\Data\\units.json",
                Clear = true,
                BatchSize = 500,
                CommandTimeout = 300
            };

            string? text = Core.Convert.ToSystem_String(options);
            Assert.False(string.IsNullOrWhiteSpace(text));

            PostgreSQLUnitInsertFromFileOptions? options_Parsed = Core.Convert.ToDiGi<PostgreSQLUnitInsertFromFileOptions>(text)?.FirstOrDefault();
            Assert.NotNull(options_Parsed);

            Assert.Equal("C:\\Data\\units.json", options_Parsed.Path);
            Assert.True(options_Parsed.Clear);
            Assert.Equal(500, options_Parsed.BatchSize);
            Assert.Equal(300, options_Parsed.CommandTimeout);

            PostgreSQLUnitInsertFromFileOptions options_Clone = new(options);
            Assert.Equal("C:\\Data\\units.json", options_Clone.Path);
            Assert.True(options_Clone.Clear);
            Assert.Equal(500, options_Clone.BatchSize);
            Assert.Equal(300, options_Clone.CommandTimeout);

            Core.xUnit.Query.SerializationCheck(options);
        }
    }
}
