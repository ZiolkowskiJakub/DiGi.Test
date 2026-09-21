using DiGi.GIS.Classes;
using System;
using System.Text.Json.Nodes;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Convert.ToSystem_Bytes(JsonNode)"/> decodes the shape the DiGi serializer actually writes for a <c>byte[]</c> - a JSON array of numbers - both as the live <see cref="JsonNode"/> of <see cref="OrtoData"/>.ToJsonObject() and after a text round trip, which is what a <c>v-&gt;'Bytes'</c> projection reads back from a stored row.
        /// <para>This is the regression for DiGi.GIS.PostgreSQL#90: the photo read decoded the member as base64 and threw on every stored row, and the only fact covering it was skipped and seeded with base64.</para>
        /// </summary>
        [Fact]
        public void ToSystem_Bytes_SerializerShape()
        {
            byte[] bytes = [255, 216, 255, 224, 0, 16, 74, 70];
            OrtoData ortoData = new(new DateTime(2010, 1, 1), bytes, 2.5, null);

            JsonObject? jsonObject = ortoData.ToJsonObject();
            Assert.NotNull(jsonObject);

            JsonNode? jsonNode_Bytes = jsonObject["Bytes"];
            Assert.IsType<JsonArray>(jsonNode_Bytes);

            // The live node of the serializer (CLR-backed values).
            Assert.Equal(bytes, Convert.ToSystem_Bytes(jsonNode_Bytes));

            // The same member after the text round trip a jsonb projection performs (element-backed values).
            Assert.Equal(bytes, Convert.ToSystem_Bytes(JsonNode.Parse(jsonNode_Bytes!.ToJsonString())));

            // The exact text the reader gets from the database column.
            Assert.Equal(bytes, Convert.ToSystem_Bytes(JsonNode.Parse("[255, 216, 255, 224, 0, 16, 74, 70]")));
        }

        /// <summary>
        /// Verifies that <see cref="Convert.ToSystem_Bytes(JsonNode)"/> also accepts the base64 string System.Text.Json writes for a <c>byte[]</c>, so a row written that way decodes instead of failing.
        /// </summary>
        [Fact]
        public void ToSystem_Bytes_Base64String()
        {
            byte[] bytes = [1, 2, 3];

            Assert.Equal(bytes, Convert.ToSystem_Bytes(JsonNode.Parse("\"AQID\"")));
            Assert.Equal(bytes, Convert.ToSystem_Bytes(JsonValue.Create("AQID")));
        }

        /// <summary>
        /// Verifies that <see cref="Convert.ToSystem_Bytes(JsonNode)"/> answers null - never throws - for null, a non-base64 string, an element outside the byte range and a node that is neither an array nor a string.
        /// </summary>
        [Fact]
        public void ToSystem_Bytes_UnknownShape_ReturnsNull()
        {
            Assert.Null(Convert.ToSystem_Bytes(null));
            Assert.Null(Convert.ToSystem_Bytes(JsonNode.Parse("\"[not base64]\"")));
            Assert.Null(Convert.ToSystem_Bytes(JsonNode.Parse("[1, 256]")));
            Assert.Null(Convert.ToSystem_Bytes(JsonNode.Parse("[1, \"2\"]")));
            Assert.Null(Convert.ToSystem_Bytes(JsonNode.Parse("{\"Bytes\":[1]}")));
            Assert.Null(Convert.ToSystem_Bytes(JsonNode.Parse("12")));
            Assert.Empty(Convert.ToSystem_Bytes(JsonNode.Parse("[]"))!);
        }
    }
}
