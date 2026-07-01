using System;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that GuidParameterValue.TryConvert (inherited from the base ParameterValue.TryConvert) correctly
        /// converts a string to a Guid. Previously
        /// the ParameterType.Guid branch declared its local conversion target as `int` instead of `Guid`, so the
        /// generic TryConvert dispatcher silently bound to the integer-conversion overload, meaning Guid-typed
        /// parameters could never round-trip correctly through TryConvert.
        /// </summary>
        [Fact]
        public void GuidParameterValue_TryConvert_ConvertsStringToGuid()
        {
            DiGi.Core.Parameter.Classes.GuidParameterValue guidParameterValue = new();

            Guid guid = Guid.NewGuid();
            string guidString = guid.ToString();

            bool result = guidParameterValue.TryConvert(guidString, out object? value_Out);

            Assert.True(result);
            Assert.IsType<Guid>(value_Out);
            Assert.Equal(guid, (Guid)value_Out!);
        }

        /// <summary>
        /// Tests that GuidParameterValue.TryConvert correctly converts an already-typed Guid value (identity
        /// conversion), and that a null input is handled according to the nullable setting rather than throwing.
        /// </summary>
        [Fact]
        public void GuidParameterValue_TryConvert_HandlesGuidAndNull()
        {
            DiGi.Core.Parameter.Classes.GuidParameterValue guidParameterValue = new();

            Guid guid = Guid.NewGuid();

            bool result = guidParameterValue.TryConvert(guid, out object? value_Out);
            Assert.True(result);
            Assert.Equal(guid, (Guid)value_Out!);

            bool result_Null = guidParameterValue.TryConvert(null, out object? value_Out_Null);
            Assert.True(result_Null);
            Assert.Null(value_Out_Null);
        }

        /// <summary>
        /// Tests that GuidParameterValue.TryConvert returns false (rather than throwing or silently succeeding
        /// with a wrong type) for an input that cannot be parsed as a Guid.
        /// </summary>
        [Fact]
        public void GuidParameterValue_TryConvert_InvalidInput_ReturnsFalse()
        {
            DiGi.Core.Parameter.Classes.GuidParameterValue guidParameterValue = new();

            bool result = guidParameterValue.TryConvert("not-a-guid", out object? value_Out);

            Assert.False(result);
        }

        /// <summary>
        /// Tests that the base ParameterValue.TryConvert DateTime branch correctly converts a string to a DateTime.
        /// Previously this branch also declared its local conversion target as `int` instead of `DateTime`. No
        /// concrete DateTimeParameterValue class exists yet, so a minimal test-only subclass
        /// (Classes.TestDateTimeParameterValue) is used to exercise this branch directly.
        /// </summary>
        [Fact]
        public void ParameterValue_TryConvert_DateTimeBranch_ConvertsStringToDateTime()
        {
            Classes.TestDateTimeParameterValue dateTimeParameterValue = new();

            DateTime dateTime = new(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);
            string dateTimeString = dateTime.ToString("o");

            bool result = dateTimeParameterValue.TryConvert(dateTimeString, out object? value_Out);

            Assert.True(result);
            Assert.IsType<DateTime>(value_Out);

            // Compare via UTC to avoid local-timezone-offset sensitivity in the round-trip.
            Assert.Equal(dateTime.ToUniversalTime(), ((DateTime)value_Out!).ToUniversalTime());
        }
    }
}