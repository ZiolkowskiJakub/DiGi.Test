using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DiGi.Python.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.PythonPaths(string?)"/> returns an existing file as the single candidate, returns a rooted path that names no file as given, and that a PATH search yields distinct candidates headed by the one <see cref="Query.PythonPath(string?)"/> picks.
        /// </summary>
        [Fact]
        public void PythonPaths()
        {
            string path = Path.Combine(Path.GetTempPath(), "DiGi_Python_Test_" + Path.GetRandomFileName() + ".exe");

            try
            {
                File.WriteAllText(path, string.Empty);

                Assert.Equal([Query.NormalizedPath(path)!], Query.PythonPaths(path));
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            //An interpreter named by a path that does not exist is reported as asked for, never replaced by another one
            Assert.Equal([Query.NormalizedPath(path)!], Query.PythonPaths(path));

            List<string> pythonPaths = Query.PythonPaths();
            Assert.Equal(pythonPaths.Count, pythonPaths.Distinct(StringComparer.OrdinalIgnoreCase).Count());
            Assert.Equal(Query.PythonPath(null), pythonPaths.FirstOrDefault());
        }
    }
}
