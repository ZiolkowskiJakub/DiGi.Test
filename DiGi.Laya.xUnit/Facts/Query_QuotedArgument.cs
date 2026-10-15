namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.QuotedArgument(string?)"/> wraps a value in quotes, doubles a trailing backslash so it cannot escape the closing quote, and quotes <c>null</c> as an empty argument.
        /// </summary>
        [Fact]
        public void Query_QuotedArgument()
        {
            Assert.Equal("\"C:\\a b\\c.py\"", Query.QuotedArgument(@"C:\a b\c.py"));
            Assert.Equal("\"D:\\\\\"", Query.QuotedArgument(@"D:\"));
            Assert.Equal("\"\"", Query.QuotedArgument(null));
        }
    }
}
