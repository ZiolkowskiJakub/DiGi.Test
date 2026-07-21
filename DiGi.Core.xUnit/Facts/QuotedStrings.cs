using System.Collections.Generic;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the QuotedStrings query method, verifying CSV-style splitting with quotes, escaped quotes, and empty/trailing fields.
        /// </summary>
        [Fact]
        public void QuotedStrings()
        {
            // 1. Basic splitting with quotes and escaped quotes
            string string_Input1 = "abc,\"d\"\"e\",fgh";
            List<string>? list_Fields1 = Core.Query.QuotedStrings(string_Input1, ",");
            Assert.NotNull(list_Fields1);
            Assert.Equal(3, list_Fields1.Count);
            Assert.Equal("abc", list_Fields1[0]);
            Assert.Equal("d\"e", list_Fields1[1]);
            Assert.Equal("fgh", list_Fields1[2]);

            // 2. Trailing and consecutive empty fields (Verifying the fix for Bug 1)
            string string_Input2 = "abc,fgh,";
            List<string>? list_Fields2 = Core.Query.QuotedStrings(string_Input2, ",");
            Assert.NotNull(list_Fields2);
            Assert.Equal(3, list_Fields2.Count);
            Assert.Equal("abc", list_Fields2[0]);
            Assert.Equal("fgh", list_Fields2[1]);
            Assert.Equal(string.Empty, list_Fields2[2]);

            // 3. Consecutive separators
            string string_Input3 = "abc,,fgh";
            List<string>? list_Fields3 = Core.Query.QuotedStrings(string_Input3, ",");
            Assert.NotNull(list_Fields3);
            Assert.Equal(3, list_Fields3.Count);
            Assert.Equal("abc", list_Fields3[0]);
            Assert.Equal(string.Empty, list_Fields3[1]);
            Assert.Equal("fgh", list_Fields3[2]);

            // 4. Single separator (should yield two empty fields)
            string string_Input4 = ",";
            List<string>? list_Fields4 = Core.Query.QuotedStrings(string_Input4, ",");
            Assert.NotNull(list_Fields4);
            Assert.Equal(2, list_Fields4.Count);
            Assert.Equal(string.Empty, list_Fields4[0]);
            Assert.Equal(string.Empty, list_Fields4[1]);

            // 5. Null input handling
            List<string>? list_FieldsNull = Core.Query.QuotedStrings(null, ",");
            Assert.Null(list_FieldsNull);
        }
    }
}