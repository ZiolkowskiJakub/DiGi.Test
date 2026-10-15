namespace DiGi.Python.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.MarkedJson(System.Collections.Generic.IEnumerable{string}?, string, string)"/> returns the payload of the last marker pair, ignores whatever is printed around the markers, joins a payload spread over several lines, and returns <c>null</c> when a marker is missing, the markers are out of order or nothing lies between them.
        /// </summary>
        [Fact]
        public void MarkedJson()
        {
            const string begin = "DIGI_JSON_BEGIN";
            const string end = "DIGI_JSON_END";

            //A single pair, surrounded by a settings notice and a progress line, with markers indented
            Assert.Equal("{\"a\":1}", Query.MarkedJson(["Settings updated", "  DIGI_JSON_BEGIN", "", "{\"a\":1}  ", "DIGI_JSON_END ", "100%|##########|"], begin, end));

            //Two pairs: the last report wins
            Assert.Equal("{\"a\":2}", Query.MarkedJson([begin, "{\"a\":1}", end, "noise", begin, "{\"a\":2}", end], begin, end));

            //A payload printed over several lines comes back joined, blank lines dropped
            Assert.Equal("{\n\"a\": 1\n}", Query.MarkedJson([begin, "{", "", "  \"a\": 1", "}", end], begin, end));

            //Garbled lines between the markers are returned as they are: parsing stays with the caller
            Assert.Equal("not json", Query.MarkedJson([begin, "not json", end], begin, end));

            //Missing end marker
            Assert.Null(Query.MarkedJson(["noise", begin, "{\"a\":1}"], begin, end));

            //Missing begin marker
            Assert.Null(Query.MarkedJson(["{\"a\":1}", end], begin, end));

            //End marker before begin marker
            Assert.Null(Query.MarkedJson([end, "{\"a\":1}", begin], begin, end));

            //A complete pair followed by an unterminated one: the last begin marker decides
            Assert.Null(Query.MarkedJson([begin, "{\"a\":1}", end, begin, "{\"a\":2}"], begin, end));

            //Nothing but blank lines between the markers
            Assert.Null(Query.MarkedJson([begin, "", "   ", end], begin, end));

            //No output at all
            Assert.Null(Query.MarkedJson(null, begin, end));
            Assert.Null(Query.MarkedJson([], begin, end));
        }
    }
}
