using DiGi.BDL.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="UnitPostgreSQLConverter.PopulateAsync(Npgsql.NpgsqlConnection?, string?, bool, int, System.IProgress{long}?, int, System.Threading.CancellationToken)"/> returns false when path is null, empty or invalid.
        /// </summary>
        [Fact]
        public async Task UnitPostgreSQLConverter_PopulateAsync_InvalidPath()
        {
            UnitPostgreSQLConverter converter = new(null);

            bool resultNull = await converter.PopulateAsync(path: null);
            Assert.False(resultNull);

            bool resultEmpty = await converter.PopulateAsync(path: string.Empty);
            Assert.False(resultEmpty);

            bool resultNonExistent = await converter.PopulateAsync(path: "C:\\NonExistentPath\\nonexistent.json");
            Assert.False(resultNonExistent);
        }

        /// <summary>
        /// Verifies JSON deserialization compatibility of BDL <see cref="Unit"/> collections from file structures.
        /// </summary>
        [Fact]
        public void Unit_JsonDeserialization_ValidJson()
        {
            List<Unit> originalUnits =
            [
                new Unit { id = "000000000000", name = "Polska", level = 0, hasDescription = true },
                new Unit { id = "010000000000", name = "Makroregion Południowy", level = 1, hasDescription = false }
            ];

            string jsonText = JsonSerializer.Serialize(originalUnits);
            List<Unit>? deserializedUnits = JsonSerializer.Deserialize<List<Unit>>(jsonText);

            Assert.NotNull(deserializedUnits);
            Assert.Equal(2, deserializedUnits.Count);
            Assert.Equal("000000000000", deserializedUnits[0].id);
            Assert.Equal("Polska", deserializedUnits[0].name);
            Assert.Equal(0, deserializedUnits[0].level);
            Assert.True(deserializedUnits[0].hasDescription);
        }
    }
}
