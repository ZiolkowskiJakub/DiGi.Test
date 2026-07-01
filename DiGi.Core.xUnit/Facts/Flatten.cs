using System.Text.Json.Nodes;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="Core.Query.Flatten(JsonArray)"/> correctly flattens an array nested two levels deep.
        /// Previously the inner loop checked the outer loop variable instead of the inner one, so elements
        /// nested two or more levels deep were never correctly recursed into (the same outer array was
        /// re-flattened for every inner element instead of the inner element itself).
        /// </summary>
        [Fact]
        public void Flatten_TwoLevelsDeep_FlattensCompletely()
        {
            // [1, [2, [3, 4], 5], 6]
            JsonArray jsonArray = new(1, new JsonArray(2, new JsonArray(3, 4), 5), 6);

            JsonArray? result = jsonArray.Flatten();

            Assert.NotNull(result);

            int[] values = new int[result.Count];
            for (int i = 0; i < result.Count; i++)
            {
                values[i] = result[i]!.GetValue<int>();
            }

            Assert.Equal([1, 2, 3, 4, 5, 6], values);
        }

        /// <summary>
        /// Tests that a single-level array (no nesting) is returned unchanged.
        /// </summary>
        [Fact]
        public void Flatten_SingleLevel_ReturnsSameElements()
        {
            JsonArray jsonArray = new(1, 2, 3);

            JsonArray? result = jsonArray.Flatten();

            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[0]!.GetValue<int>());
            Assert.Equal(2, result[1]!.GetValue<int>());
            Assert.Equal(3, result[2]!.GetValue<int>());
        }

        /// <summary>
        /// Tests that null input returns null.
        /// </summary>
        [Fact]
        public void Flatten_NullInput_ReturnsNull()
        {
            JsonArray? jsonArray = null;
            Assert.Null(jsonArray.Flatten());
        }
    }
}