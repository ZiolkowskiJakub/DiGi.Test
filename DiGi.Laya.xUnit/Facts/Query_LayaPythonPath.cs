using System;
using System.IO;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.LayaPythonPath(string?)"/> accepts only the interpreter that was configured: an existing file path is returned normalized, a missing path or an unknown bare name is refused even though PATH holds some Python that DiGi.Python's <c>Query.PythonPath</c> would fall back to, a bare name found on PATH is returned with that name (cmd, on Windows), and a blank value searches PATH as DiGi.Python does.
        /// </summary>
        [Fact]
        public void Query_LayaPythonPath()
        {
            string path_File = Path.Combine(Path.GetTempPath(), string.Concat("Laya_Python_", Guid.NewGuid().ToString("N"), ".cmd"));

            try
            {
                File.WriteAllText(path_File, "@exit /b 0\r\n");

                Assert.Equal(Path.GetFullPath(path_File), Query.LayaPythonPath(path_File));
                Assert.Null(Query.LayaPythonPath(Path.Combine(Path.GetTempPath(), "no-such-python.exe")));
                Assert.Null(Query.LayaPythonPath(@".\no-such-python.exe"));
                Assert.Null(Query.LayaPythonPath(string.Concat("no-such-python-", Guid.NewGuid().ToString("N"))));
                Assert.Equal(Python.Query.PythonPath(null), Query.LayaPythonPath(null));
                Assert.Equal(Python.Query.PythonPath(null), Query.LayaPythonPath(" "));

                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    string? path_Cmd = Query.LayaPythonPath("cmd");
                    Assert.NotNull(path_Cmd);
                    Assert.Equal("cmd.exe", Path.GetFileName(path_Cmd), StringComparer.OrdinalIgnoreCase);
                }
            }
            finally
            {
                if (File.Exists(path_File))
                {
                    File.Delete(path_File);
                }
            }
        }
    }
}
