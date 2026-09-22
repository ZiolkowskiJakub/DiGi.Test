using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies <see cref="Convert.ToSystem_JsonObject(IEnumerable{Building2DCentroid}?)"/>, the body of <c>centroidsbyadministrativeareal2Did</c> (DiGi.GIS.WebAPI#40).
        /// <para>Four arrays of one length in input order; coordinates rounded to 0.01 m, including the binary noise of <c>474823.57999999996</c> the full endpoint relays today; centroids without a reference or county identifier skipped; four empty arrays for an empty input, and null for null.</para>
        /// </summary>
        [Fact]
        public void Convert_ToSystem_JsonObject_Building2DCentroids()
        {
            List<Building2DCentroid> building2DCentroids =
            [
                new() { Reference = "R1", CountyId = 78244, X = 474823.57999999996, Y = 251870.004 },
                new() { Reference = null, CountyId = 78244, X = 1, Y = 2 },
                new() { Reference = "R3", CountyId = null, X = 3, Y = 4 },
                new() { Reference = "R2", CountyId = 5, X = -10.125, Y = 0.005 },
            ];

            JsonObject? jsonObject = Convert.ToSystem_JsonObject(building2DCentroids);
            Assert.NotNull(jsonObject);
            Assert.Equal(4, jsonObject.Count);

            JsonArray references = jsonObject["References"]!.AsArray();
            JsonArray countyIds = jsonObject["CountyIds"]!.AsArray();
            JsonArray xs = jsonObject["X"]!.AsArray();
            JsonArray ys = jsonObject["Y"]!.AsArray();

            Assert.Equal(2, references.Count);
            Assert.Equal(2, countyIds.Count);
            Assert.Equal(2, xs.Count);
            Assert.Equal(2, ys.Count);

            Assert.Equal("R1", references[0]!.GetValue<string>());
            Assert.Equal("R2", references[1]!.GetValue<string>());
            Assert.Equal(78244, countyIds[0]!.GetValue<int>());
            Assert.Equal(5, countyIds[1]!.GetValue<int>());
            Assert.Equal(474823.58, xs[0]!.GetValue<double>());
            Assert.Equal(251870.0, ys[0]!.GetValue<double>());
            Assert.Equal(-10.13, xs[1]!.GetValue<double>());
            Assert.Equal(0.01, ys[1]!.GetValue<double>());

            // The serialized text carries the rounded value, not the binary noise.
            string json = jsonObject.ToJsonString();
            Assert.Contains("474823.58", json);
            Assert.DoesNotContain("474823.57999", json);
            Assert.DoesNotContain("_type", json);

            JsonObject? jsonObject_Empty = Convert.ToSystem_JsonObject([]);
            Assert.NotNull(jsonObject_Empty);
            Assert.Equal("{\"References\":[],\"CountyIds\":[],\"X\":[],\"Y\":[]}", jsonObject_Empty.ToJsonString());

            Assert.Null(Convert.ToSystem_JsonObject(null));
        }
    }
}
