using System;
using System.IO;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the resolver of the headless Year Built prediction runner answers a path only when an executable is actually there.
        /// <para>The runner is a separate deployment unit rather than an assembly this application loads, so it has to be found rather than linked. A resolver that returned the path the runner <i>would</i> have would move the failure from the moment the task starts to the moment it tries to start a process - after the counties had been chosen, the imagery scoped and the operator had walked away.</para>
        /// <para>The candidates it probes are all absent under a test run, so the fact that matters here is the negative one: nothing, rather than something plausible. The positive case is exercised by pointing it at a real file.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictionConsoleAppPath()
        {
            // A path that names nothing is not a path to the runner, and neither is a malformed one - both have to
            // fall through to the probing rather than come back as an answer.
            Assert.Null(Query.YearBuiltPredictionConsoleAppPath(System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"), Constants.FileName.YearBuiltPredictionConsoleApp)));
            Assert.Null(Query.YearBuiltPredictionConsoleAppPath(string.Empty));
            Assert.Null(Query.YearBuiltPredictionConsoleAppPath("   "));

            // An explicit path that does exist is taken as given, whatever the file is - the resolver's job is to
            // say whether something is there, not to vouch for what it is.
            string directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            string path = System.IO.Path.Combine(directory, Constants.FileName.YearBuiltPredictionConsoleApp);

            try
            {
                Directory.CreateDirectory(directory);
                File.WriteAllText(path, string.Empty);

                string? path_Resolved = Query.YearBuiltPredictionConsoleAppPath(path);

                Assert.NotNull(path_Resolved);
                Assert.True(File.Exists(path_Resolved));
                Assert.Equal(System.IO.Path.GetFullPath(path), path_Resolved);
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }

            // Whatever the probing finds, it never answers with something that is not there.
            if (Query.YearBuiltPredictionConsoleAppPath() is string path_Probed)
            {
                Assert.True(File.Exists(path_Probed));
            }
        }
    }
}
