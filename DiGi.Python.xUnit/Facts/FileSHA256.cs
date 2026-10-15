using System.IO;
using System.Text;

namespace DiGi.Python.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.FileSHA256(string?)"/> returns the lowercase hexadecimal SHA-256 digest of a file (the FIPS 180-2 "abc" test vector), and <c>null</c> for a missing file or an empty path.
        /// </summary>
        [Fact]
        public void FileSHA256()
        {
            string path = Path.Combine(Path.GetTempPath(), "DiGi_Python_Test_" + Path.GetRandomFileName());

            try
            {
                File.WriteAllBytes(path, Encoding.ASCII.GetBytes("abc"));

                Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", Query.FileSHA256(path));
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            Assert.Null(Query.FileSHA256(path));
            Assert.Null(Query.FileSHA256(null));
            Assert.Null(Query.FileSHA256(string.Empty));
        }
    }
}
