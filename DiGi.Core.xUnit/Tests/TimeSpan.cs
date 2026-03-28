using System.Text.Json;

namespace DiGi.Core.xUnit
{
    public partial class Tests
    {
        [Fact]
        public void TimeSpan()
        {
            TimeSpanObject timeSpanObject = new() { TimeSpan = new System.TimeSpan(1, 2, 3, 4, 5, 6) };

            string jsonString = JsonSerializer.Serialize(timeSpanObject);

            Query.SerializationCheck(timeSpanObject);
        }
    }
}