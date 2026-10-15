using System.IO;

namespace DiGi.Python.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.NormalizedPath(string?)"/> trims a trailing separator (which would escape a closing quote on a command line), keeps the separator of a drive root, resolves a relative path, and returns <c>null</c> for an empty path.
        /// </summary>
        [Fact]
        public void NormalizedPath()
        {
            Assert.Equal(@"C:\Python\Scripts", Query.NormalizedPath(@"C:\Python\Scripts\"));
            Assert.Equal(@"C:\Python\Scripts", Query.NormalizedPath(@"C:\Python\Scripts"));
            Assert.Equal(@"C:\Python\Scripts", Query.NormalizedPath("C:/Python/Scripts/"));
            Assert.Equal(@"C:\Python", Query.NormalizedPath(@"C:\Python\Scripts\.."));
            Assert.Equal(@"C:\", Query.NormalizedPath(@"C:\"));
            Assert.Equal(Path.GetFullPath("relative"), Query.NormalizedPath("relative"));

            Assert.Null(Query.NormalizedPath(null));
            Assert.Null(Query.NormalizedPath(string.Empty));
            Assert.Null(Query.NormalizedPath("   "));
        }
    }
}
