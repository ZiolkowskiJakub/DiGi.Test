using System.Text.Json;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a <see cref="System.TimeSpan"/> property within a <see cref="TimeSpanObject"/> is correctly serialized and deserialized.
        /// </summary>
        [Fact]
        public void TimeSpan()
        {
            TimeSpanObject timeSpanObject = new() { TimeSpan = new System.TimeSpan(1, 2, 3, 4, 5, 6) };

            string jsonString = JsonSerializer.Serialize(timeSpanObject);

            Query.SerializationCheck(timeSpanObject);
        }
    }
}