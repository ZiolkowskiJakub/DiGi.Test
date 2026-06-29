using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using System;
using System.Text.Json.Nodes;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="Core.Create.SerializationMethodCollection(Type)"/> still places ordered members
        /// (by ascending <see cref="System.Text.Json.Serialization.JsonPropertyOrderAttribute"/> value) before
        /// unordered members, after replacing the O(n^2) `tuples.Insert(0, ...)` loop with a single
        /// `InsertRange(0, ...)` call.
        /// </summary>
        [Fact]
        public void SerializationMethodCollection_OrdersMembersCorrectly()
        {
            Classes.TestOrderedSerializationObject testObject = new();

            SerializationMethodCollection? serializationMethodCollection = typeof(Classes.TestOrderedSerializationObject).SerializationMethodCollection();
            Assert.NotNull(serializationMethodCollection);

            JsonObject? jsonObject = serializationMethodCollection.Create(testObject);
            Assert.NotNull(jsonObject);

            string json = jsonObject.ToJsonString();

            int index_Alpha = json.IndexOf("\"Alpha\"");
            int index_Beta = json.IndexOf("\"Beta\"");
            int index_Zulu = json.IndexOf("\"Zulu\"");
            int index_Yankee = json.IndexOf("\"Yankee\"");

            Assert.True(index_Alpha >= 0 && index_Beta >= 0 && index_Zulu >= 0 && index_Yankee >= 0);

            // Ordered members (Alpha=1, Beta=2) come first, in ascending order...
            Assert.True(index_Alpha < index_Beta);

            // ...and both come before the unordered members.
            Assert.True(index_Beta < index_Zulu);
            Assert.True(index_Beta < index_Yankee);
        }
    }
}
