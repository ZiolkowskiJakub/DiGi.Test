using System;
using System.Collections.Generic;
using DiGi.Core.Enums;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the string Compare query methods, verifying null logic correctness, case sensitivity, and case insensitivity.
        /// </summary>
        [Fact]
        public void Compare_String()
        {
            // 1. Case-sensitive and case-insensitive Equals
            Assert.True(DiGi.Core.Query.Compare("Hello", "Hello", TextComparisonType.Equals, true));
            Assert.False(DiGi.Core.Query.Compare("Hello", "hello", TextComparisonType.Equals, true));
            Assert.True(DiGi.Core.Query.Compare("Hello", "hello", TextComparisonType.Equals, false));

            // 2. Contains and NotContains
            Assert.True(DiGi.Core.Query.Compare("Hello World", "World", TextComparisonType.Contains, true));
            Assert.True(DiGi.Core.Query.Compare("Hello World", "world", TextComparisonType.Contains, false));
            Assert.True(DiGi.Core.Query.Compare("Hello World", "Door", TextComparisonType.NotContains, true));

            // 3. Null logic for positive and negative types (Verifying the fix for Bug 3)
            string? string_Null = null;
            Assert.False(DiGi.Core.Query.Compare(string_Null, "World", TextComparisonType.Contains, false));
            Assert.True(DiGi.Core.Query.Compare(string_Null, "World", TextComparisonType.NotContains, false));

            Assert.False(DiGi.Core.Query.Compare(string_Null, "World", TextComparisonType.StartsWith, false));
            Assert.True(DiGi.Core.Query.Compare(string_Null, "World", TextComparisonType.NotStartsWith, false));

            Assert.False(DiGi.Core.Query.Compare(string_Null, "World", TextComparisonType.EndsWith, false));
            Assert.True(DiGi.Core.Query.Compare(string_Null, "World", TextComparisonType.NotEndsWith, false));

            // Both null
            Assert.True(DiGi.Core.Query.Compare(string_Null, null, TextComparisonType.Equals, false));
            Assert.False(DiGi.Core.Query.Compare(string_Null, null, TextComparisonType.NotEquals, false));
            Assert.True(DiGi.Core.Query.Compare(string_Null, null, TextComparisonType.Contains, false));
            Assert.False(DiGi.Core.Query.Compare(string_Null, null, TextComparisonType.NotContains, false));
            Assert.True(DiGi.Core.Query.Compare(string_Null, null, TextComparisonType.StartsWith, false));
            Assert.False(DiGi.Core.Query.Compare(string_Null, null, TextComparisonType.NotStartsWith, false));
            Assert.True(DiGi.Core.Query.Compare(string_Null, null, TextComparisonType.EndsWith, false));
            Assert.False(DiGi.Core.Query.Compare(string_Null, null, TextComparisonType.NotEndsWith, false));
        }

        /// <summary>
        /// Tests the numeric Compare query methods for double and DateTime values.
        /// </summary>
        [Fact]
        public void Compare_Numeric()
        {
            // Double comparison
            Assert.True(DiGi.Core.Query.Compare(5.0, 5.0, NumberComparisonType.Equals));
            Assert.True(DiGi.Core.Query.Compare(5.0, 3.0, NumberComparisonType.Greater));
            Assert.True(DiGi.Core.Query.Compare(5.0, 5.0, NumberComparisonType.GreaterOrEquals));

            // DateTime comparison
            DateTime dateTime_1 = new DateTime(2026, 6, 23);
            DateTime dateTime_2 = new DateTime(2026, 6, 24);
            Assert.True(DiGi.Core.Query.Compare(dateTime_1, dateTime_2, NumberComparisonType.Less));
            Assert.True(DiGi.Core.Query.Compare(dateTime_2, dateTime_1, NumberComparisonType.Greater));
        }
    }
}
