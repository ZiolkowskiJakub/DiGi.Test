namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="IO.Query.RelativePath(string?, string?)"/> strips the base directory from a path that sits below it.
        /// <para>The CityGML import records each building's source this way so that resuming an interrupted run survives the data being moved to another root, drive or machine.</para>
        /// </summary>
        [Fact]
        public void RelativePath_BelowBaseDirectory()
        {
            Assert.Equal(@"2862.zip\x.gml", IO.Query.RelativePath(@"C:\data", @"C:\data\2862.zip\x.gml"));

            // A trailing separator on the base directory must not change the result.
            Assert.Equal(@"2862.zip\x.gml", IO.Query.RelativePath(@"C:\data\", @"C:\data\2862.zip\x.gml"));

            // Nested folders below the base are preserved.
            Assert.Equal(@"LOD2\2023\2862.zip\x.gml", IO.Query.RelativePath(@"C:\data", @"C:\data\LOD2\2023\2862.zip\x.gml"));
        }

        /// <summary>
        /// Verifies that <see cref="IO.Query.RelativePath(string?, string?)"/> handles paths containing characters that are percent-encoded in a URI.
        /// <para>The implementation routes through <see cref="System.Uri"/>, so spaces and similar characters would come back escaped if they were not unescaped again; folder names chosen by a user routinely contain them.</para>
        /// </summary>
        [Fact]
        public void RelativePath_EscapedCharacters()
        {
            Assert.Equal(@"2862.zip\x.gml", IO.Query.RelativePath(@"C:\my data", @"C:\my data\2862.zip\x.gml"));
            Assert.Equal(@"my zip.zip\my file.gml", IO.Query.RelativePath(@"C:\data", @"C:\data\my zip.zip\my file.gml"));
        }

        /// <summary>
        /// Verifies the edge cases of <see cref="IO.Query.RelativePath(string?, string?)"/>: null input, an identical path, and a path outside the base directory.
        /// <para>Callers that assume a path below the base must cope with null, because that is what a missing argument returns rather than an exception.</para>
        /// </summary>
        [Fact]
        public void RelativePath_EdgeCases()
        {
            Assert.Null(IO.Query.RelativePath(null, @"C:\data\x.gml"));
            Assert.Null(IO.Query.RelativePath(@"C:\data", null));

            Assert.Equal(".", IO.Query.RelativePath(@"C:\data", @"C:\data"));

            // Outside the base directory the result walks up rather than staying absolute.
            Assert.Equal(@"..\other\x.gml", IO.Query.RelativePath(@"C:\data", @"C:\other\x.gml"));
        }
    }
}
