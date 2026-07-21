using System.Collections.Generic;
using System.Diagnostics;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Performance guard for the SerializableObject pipeline: serializes and deserializes a batch of
        /// <see cref="TestObject"/> instances (covering nested objects, enums, colors, dictionaries and key-value pairs),
        /// verifying that every round trip produces identical JSON and completes within the time threshold.
        /// </summary>
        [Fact]
        public void SerializableObject_Performance()
        {
            int count = 500;

            List<TestObject> testObjects = new(count);
            for (int i = 0; i < count; i++)
            {
                testObjects.Add(new TestObject());
            }

            // Warm-up: JIT compilation and serialization metadata creation.
            string? json_WarmUp = Convert.ToSystem_String(testObjects[0]);
            Assert.NotNull(json_WarmUp);

            List<TestObject>? testObjects_WarmUp = Convert.ToDiGi<TestObject>(json_WarmUp);
            Assert.NotNull(testObjects_WarmUp);
            Assert.NotEmpty(testObjects_WarmUp);

            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < count; i++)
            {
                string? json = Convert.ToSystem_String(testObjects[i]);
                Assert.NotNull(json);

                List<TestObject>? testObjects_Temp = Convert.ToDiGi<TestObject>(json);
                Assert.NotNull(testObjects_Temp);
                Assert.NotEmpty(testObjects_Temp);

                string? json_Temp = Convert.ToSystem_String(testObjects_Temp[0]);
                Assert.Equal(json, json_Temp);
            }

            stopwatch.Stop();

            Assert.True(stopwatch.ElapsedMilliseconds < 2000, $"Serialization round trip of {count} objects took {stopwatch.ElapsedMilliseconds} ms which exceeds the 2000 ms threshold.");
        }
    }
}