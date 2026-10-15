using System.IO;

namespace DiGi.Python.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.PythonPath(string?)"/> returns a path naming an existing file as given (normalized), and falls back to the PATH search for a path that names no file - the same answer as searching with <c>null</c>.
        /// <para>Whether PATH yields an interpreter depends on the machine; the fallback is compared with the <c>null</c> search either way, and a found interpreter is checked to exist.</para>
        /// </summary>
        [Fact]
        public void PythonPath()
        {
            string path = Path.Combine(Path.GetTempPath(), "DiGi_Python_Test_" + Path.GetRandomFileName() + ".exe");

            try
            {
                File.WriteAllText(path, string.Empty);

                Assert.Equal(Query.NormalizedPath(path), Query.PythonPath(path));
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            string? pythonPath_Search = Query.PythonPath(null);
            Assert.Equal(pythonPath_Search, Query.PythonPath(path));
            Assert.Equal(pythonPath_Search, Query.PythonPath("   "));

            if (pythonPath_Search != null)
            {
                Assert.True(File.Exists(pythonPath_Search));
                Assert.True(Path.IsPathRooted(pythonPath_Search));
            }
        }
    }
}
