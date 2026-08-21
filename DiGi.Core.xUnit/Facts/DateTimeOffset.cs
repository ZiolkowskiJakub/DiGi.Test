using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the serialization, deserialization, and cloning of serializable objects containing DateTimeOffset properties.
        /// </summary>
        [Fact]
        public void DateTimeOffset_SerializationCheck()
        {
            DateTimeOffset dateTimeOffset_Utc = new(2026, 8, 21, 10, 30, 0, System.TimeSpan.Zero);
            DateTimeOffset dateTimeOffset_Offset = new(2026, 8, 21, 12, 30, 0, System.TimeSpan.FromHours(2));
            DateTimeOffset[] dateTimeOffsets = [dateTimeOffset_Utc, dateTimeOffset_Offset];

            DateTimeOffsetObject dateTimeOffsetObject_1 = new(dateTimeOffset_Utc, dateTimeOffset_Offset, dateTimeOffsets);

            Query.SerializationCheck(dateTimeOffsetObject_1);

            // Verify clone preserves exact offsets and values
            ISerializableObject? clone = dateTimeOffsetObject_1.Clone();
            Assert.NotNull(clone);
            DateTimeOffsetObject? dateTimeOffsetObject_Clone = clone as DateTimeOffsetObject;
            Assert.NotNull(dateTimeOffsetObject_Clone);
            Assert.Equal(dateTimeOffset_Utc, dateTimeOffsetObject_Clone.Timestamp);
            Assert.Equal(dateTimeOffset_Utc.Offset, dateTimeOffsetObject_Clone.Timestamp.Offset);
            Assert.NotNull(dateTimeOffsetObject_Clone.NullableTimestamp);
            Assert.Equal(dateTimeOffset_Offset, dateTimeOffsetObject_Clone.NullableTimestamp);
            Assert.Equal(dateTimeOffset_Offset.Offset, dateTimeOffsetObject_Clone.NullableTimestamp.Value.Offset);
            Assert.NotNull(dateTimeOffsetObject_Clone.Timestamps);
            Assert.Equal(2, dateTimeOffsetObject_Clone.Timestamps.Length);
            Assert.Equal(dateTimeOffset_Utc, dateTimeOffsetObject_Clone.Timestamps[0]);
            Assert.Equal(dateTimeOffset_Offset, dateTimeOffsetObject_Clone.Timestamps[1]);

            // Test with null optional fields
            DateTimeOffsetObject dateTimeOffsetObject_Nulls = new(dateTimeOffset_Utc);
            Query.SerializationCheck(dateTimeOffsetObject_Nulls);

            // Test string convert round-trip
            string? json = Convert.ToSystem_String(dateTimeOffsetObject_1);
            Assert.NotNull(json);
            DateTimeOffsetObject? dateTimeOffsetObject_Deserialized = Convert.ToDiGi<DateTimeOffsetObject>(json)?.FirstOrDefault();
            Assert.NotNull(dateTimeOffsetObject_Deserialized);
            Assert.Equal(dateTimeOffset_Utc, dateTimeOffsetObject_Deserialized.Timestamp);
            Assert.Equal(dateTimeOffset_Utc.Offset, dateTimeOffsetObject_Deserialized.Timestamp.Offset);
        }

        /// <summary>
        /// Tests TryConvert for DateTimeOffset across various input types including strings, numbers, JsonNodes, and DateTimes.
        /// </summary>
        [Fact]
        public void DateTimeOffset_TryConvert()
        {
            DateTimeOffset dateTimeOffset_Original = new(2026, 8, 21, 14, 45, 30, System.TimeSpan.FromHours(2));

            // From DateTimeOffset
            Assert.True(Core.Query.TryConvert(dateTimeOffset_Original, out DateTimeOffset? result_FromOffset));
            Assert.Equal(dateTimeOffset_Original, result_FromOffset);

            // From DateTime
            DateTime dateTime_Utc = new(2026, 8, 21, 12, 45, 30, DateTimeKind.Utc);
            Assert.True(Core.Query.TryConvert(dateTime_Utc, out DateTimeOffset? result_FromDateTime));
            Assert.Equal(dateTime_Utc, result_FromDateTime!.Value.UtcDateTime);

            // From ISO 8601 string with offset
            string string_IsoOffset = "2026-08-21T14:45:30+02:00";
            Assert.True(Core.Query.TryConvert(string_IsoOffset, out DateTimeOffset? result_FromStringOffset));
            Assert.Equal(dateTimeOffset_Original, result_FromStringOffset);

            // From ISO 8601 string with UTC Z
            string string_IsoUtc = "2026-08-21T12:45:30Z";
            Assert.True(Core.Query.TryConvert(string_IsoUtc, out DateTimeOffset? result_FromStringUtc));
            Assert.Equal(System.TimeSpan.Zero, result_FromStringUtc!.Value.Offset);

            // From in-memory JsonValue holding DateTimeOffset
            JsonNode jsonNode_InMemory = JsonValue.Create(dateTimeOffset_Original);
            Assert.True(Core.Query.TryConvert(jsonNode_InMemory, out DateTimeOffset? result_FromJsonNodeInMemory));
            Assert.Equal(dateTimeOffset_Original, result_FromJsonNodeInMemory);

            // From parsed JsonNode
            JsonNode? jsonNode_Parsed = JsonNode.Parse($"\"{string_IsoOffset}\"");
            Assert.NotNull(jsonNode_Parsed);
            Assert.True(Core.Query.TryConvert(jsonNode_Parsed, out DateTimeOffset? result_FromJsonNodeParsed));
            Assert.Equal(dateTimeOffset_Original, result_FromJsonNodeParsed);

            // From Unix epoch seconds
            long unixSeconds = 1787319930L;
            Assert.True(Core.Query.TryConvert(unixSeconds, out DateTimeOffset? result_FromUnixSeconds));
            Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(unixSeconds), result_FromUnixSeconds);

            // From null
            Assert.False(Core.Query.TryConvert(null, out DateTimeOffset result_NullNonNullable));
            Assert.True(Core.Query.TryConvert(null, out DateTimeOffset? result_NullNullable));
            Assert.Null(result_NullNullable);

            // From Type (invalid)
            Assert.False(Core.Query.TryConvert(typeof(DateTimeOffset), out DateTimeOffset? result_FromType));
            Assert.Null(result_FromType);

            // Generic TryConvert<DateTimeOffset>
            Assert.True(Core.Query.TryConvert(string_IsoOffset, out DateTimeOffset result_Generic));
            Assert.Equal(dateTimeOffset_Original, result_Generic);

            // Convert DateTimeOffset to DateTime
            Assert.True(Core.Query.TryConvert(dateTimeOffset_Original, out DateTime? result_ToDateTime));
            Assert.Equal(dateTimeOffset_Original.DateTime, result_ToDateTime);
        }

        /// <summary>
        /// Tests Query.Value extraction for scalar, nullable, array, and list DateTimeOffset types from JsonNodes.
        /// </summary>
        [Fact]
        public void DateTimeOffset_QueryValue()
        {
            DateTimeOffset dateTimeOffset_Val = new(2026, 8, 21, 10, 0, 0, System.TimeSpan.FromHours(1));

            // In-memory JsonValue
            JsonNode jsonNode_InMemory = JsonValue.Create(dateTimeOffset_Val);
            object? value_InMemory = jsonNode_InMemory.Value(typeof(DateTimeOffset));
            Assert.NotNull(value_InMemory);
            Assert.IsType<DateTimeOffset>(value_InMemory);
            Assert.Equal(dateTimeOffset_Val, (DateTimeOffset)value_InMemory);

            // Parsed JsonNode
            JsonNode? jsonNode_Parsed = JsonNode.Parse("\"2026-08-21T10:00:00+01:00\"");
            Assert.NotNull(jsonNode_Parsed);
            object? value_Parsed = jsonNode_Parsed.Value(typeof(DateTimeOffset));
            Assert.NotNull(value_Parsed);
            Assert.IsType<DateTimeOffset>(value_Parsed);
            Assert.Equal(dateTimeOffset_Val, (DateTimeOffset)value_Parsed);

            // Nullable DateTimeOffset
            JsonNode? jsonNode_Null = JsonNode.Parse("null");
            object? value_Null = jsonNode_Null.Value(typeof(DateTimeOffset?));
            Assert.Null(value_Null);

            // DateTimeOffset array
            JsonNode? jsonNode_Array = JsonNode.Parse("[\"2026-08-21T10:00:00+01:00\", \"2026-08-21T11:00:00+01:00\"]");
            Assert.NotNull(jsonNode_Array);
            object? value_Array = jsonNode_Array.Value(typeof(DateTimeOffset[]));
            Assert.NotNull(value_Array);
            Assert.IsType<DateTimeOffset[]>(value_Array);
            DateTimeOffset[] array = (DateTimeOffset[])value_Array;
            Assert.Equal(2, array.Length);
            Assert.Equal(dateTimeOffset_Val, array[0]);

            // DateTimeOffset List
            object? value_List = jsonNode_Array.Value(typeof(List<DateTimeOffset>));
            Assert.NotNull(value_List);
            Assert.IsType<List<DateTimeOffset>>(value_List);
            List<DateTimeOffset> list = (List<DateTimeOffset>)value_List;
            Assert.Equal(2, list.Count);
            Assert.Equal(dateTimeOffset_Val, list[0]);
        }

        /// <summary>
        /// Tests Range of DateTimeOffset serialization and boundary behavior.
        /// </summary>
        [Fact]
        public void DateTimeOffset_Range()
        {
            DateTimeOffset min = new(2026, 1, 1, 0, 0, 0, System.TimeSpan.Zero);
            DateTimeOffset max = new(2026, 12, 31, 23, 59, 59, System.TimeSpan.Zero);
            Range<DateTimeOffset> range = new(min, max);

            Assert.Equal(min, range.Min);
            Assert.Equal(max, range.Max);
            Assert.True(range.In(new DateTimeOffset(2026, 6, 15, 12, 0, 0, System.TimeSpan.Zero)));
            Assert.False(range.In(new DateTimeOffset(2027, 1, 1, 0, 0, 0, System.TimeSpan.Zero)));

            Query.SerializationCheck(range);
        }
    }
}
