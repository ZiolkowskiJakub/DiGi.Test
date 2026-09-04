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

        /// <summary>
        /// Verifies that the resolver of the headless Year Built prediction runner finds the runner in the folder beside this application's own, the layout the deployment ships.
        /// <para>The runner is a deployment unit of its own under the software directory rather than a file beside this application's executable - so on a deployed machine it sits beside this application's folder, not beside it. A resolver that only looked beside the executable, the layout a developer has while building, would answer nothing on the machine the task is meant to run. The baseDirectory seam lets this probe a laid-out folder without deploying anything.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictionConsoleAppPath_SiblingFolder()
        {
            // The deployed layout: the runner in its own folder beside this application's, with nothing beside the executable
            // itself - so the answer has to come from the sibling candidate and not from a path that is not there.
            string root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            string directory = System.IO.Path.Combine(root, "DiGi.GIS.PostgreSQL.UI");
            string directory_Sibling = System.IO.Path.Combine(root, "DiGi.GIS.YOLO.UI");
            string path = System.IO.Path.Combine(directory_Sibling, Constants.FileName.YearBuiltPredictionConsoleApp);

            try
            {
                Directory.CreateDirectory(directory);
                Directory.CreateDirectory(directory_Sibling);
                File.WriteAllText(path, string.Empty);

                string? path_Resolved = Query.YearBuiltPredictionConsoleAppPath(baseDirectory: directory);

                Assert.Equal(System.IO.Path.GetFullPath(path), path_Resolved);
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
            }

            // An application folder with no runner beside it, in its own folder, or in a workspace checkout is not answered with a path.
            string root_Empty = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            string directory_Empty = System.IO.Path.Combine(root_Empty, "DiGi.GIS.PostgreSQL.UI");

            try
            {
                Directory.CreateDirectory(directory_Empty);

                Assert.Null(Query.YearBuiltPredictionConsoleAppPath(baseDirectory: directory_Empty));
            }
            finally
            {
                if (Directory.Exists(root_Empty))
                {
                    Directory.Delete(root_Empty, true);
                }
            }
        }
        /// <summary>
        /// Verifies that the resolver finds the runner in the extensions folder inside this application's own output, which is the layout the deployment now ships.
        /// <para>The runner is assembled into this application's build output under <c>extensions</c>, in a folder of its own, before the deployment copies that output to the host - so a workspace checkout and a deployed machine resolve it identically, and a machine that will never score a building is deployed without the folder at all.</para>
        /// <para>The folder name is a contract with <c>DiGi.Maintenance/Scripts/SyncDirectories.ps1</c>, which creates it, and nothing checks the two against each other at compile time. A rename on either side would leave the task quietly unoffered on every host, which is why the expected layout is stated here rather than only in the script.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictionConsoleAppPath_ExtensionFolder()
        {
            string directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            string directory_Extension = System.IO.Path.Combine(directory, Constants.DirectoryName.Extensions, Constants.DirectoryName.YearBuiltPredictionExtension);
            string path = System.IO.Path.Combine(directory_Extension, Constants.FileName.YearBuiltPredictionConsoleApp);

            try
            {
                Directory.CreateDirectory(directory_Extension);
                File.WriteAllText(path, string.Empty);

                string? path_Resolved = Query.YearBuiltPredictionConsoleAppPath(baseDirectory: directory);

                Assert.Equal(System.IO.Path.GetFullPath(path), path_Resolved);
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }

            // The extensions folder is optional, and an empty one is not a deployment of the runner. A task
            // offered against a folder with no executable in it could only ever fail to start a process.
            string directory_Empty = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));

            try
            {
                Directory.CreateDirectory(System.IO.Path.Combine(directory_Empty, Constants.DirectoryName.Extensions, Constants.DirectoryName.YearBuiltPredictionExtension));

                Assert.Null(Query.YearBuiltPredictionConsoleAppPath(baseDirectory: directory_Empty));
            }
            finally
            {
                if (Directory.Exists(directory_Empty))
                {
                    Directory.Delete(directory_Empty, true);
                }
            }
        }
    }
}
