using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.WebAPI.UI.Classes;
using System.Text.Json;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Round-trips the <c>POST /typology/buildings</c> request body through System.Text.Json (#22): the id, code and the
        /// area type (as an integer on the wire) survive, and an omitted area type deserializes to null - the value the
        /// action rejects rather than reading as <see cref="AdministrativeArealType.Country"/>.
        /// </summary>
        [Fact]
        public void TypologySolveParameter_Serialization()
        {
            TypologySolveParameter parameter = new()
            {
                Id = 1234,
                Code = "2212100",
                AdministrativeArealType = AdministrativeArealType.Municipality
            };

            string json = JsonSerializer.Serialize(parameter);
            TypologySolveParameter? roundTrip = JsonSerializer.Deserialize<TypologySolveParameter>(json);
            Assert.NotNull(roundTrip);
            Assert.Equal(1234, roundTrip.Id);
            Assert.Equal("2212100", roundTrip.Code);
            Assert.Equal(AdministrativeArealType.Municipality, roundTrip.AdministrativeArealType);
            Assert.Null(roundTrip.Definition);

            // An omitted area type deserializes to null, the value the action rejects.
            TypologySolveParameter? noType = JsonSerializer.Deserialize<TypologySolveParameter>("{ \"Id\": 1 }");
            Assert.NotNull(noType);
            Assert.Equal(1, noType!.Id);
            Assert.Null(noType.AdministrativeArealType);
        }
    }
}
