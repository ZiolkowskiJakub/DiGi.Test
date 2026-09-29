using System;
using System.IO;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.ConsoleAppFilePath(string?, string?)"/> finds a file named relative to the runner in each of the three places it looks, and returns nothing for a file that is not there.
        /// <para>The runner resolves its relative defaults against its own folder, and the build flattens the git-ignored "user files" folder into that folder, so a default such as <c>user files/YOLO/models/model.pt</c> is found one segment shallower. A path resolved against this application instead would name a different file on each side without either reporting it - which is why a relative name that exists only in this process's current directory is asserted not to be found.</para>
        /// </summary>
        [Fact]
        public void ConsoleAppFilePath()
        {
            string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            string path_ConsoleApp = Path.Combine(directory, Constants.FileName.YearBuiltPredictionConsoleApp);
            string path_Runner = Path.Combine(directory, "Data.tsv");
            string path_Deployed = Path.Combine(directory, "YOLO", "models", Constants.FileName.Model);
            string directory_Caller = Path.Combine(directory, "caller");
            string currentDirectory = Directory.GetCurrentDirectory();

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path_Deployed)!);
                File.WriteAllText(path_ConsoleApp, string.Empty);
                File.WriteAllText(path_Runner, string.Empty);
                File.WriteAllText(path_Deployed, string.Empty);
                Directory.CreateDirectory(directory_Caller);
                File.WriteAllText(Path.Combine(directory_Caller, "Caller.tsv"), string.Empty);

                // As given: an absolute path that exists.
                Assert.Equal(path_Runner, Query.ConsoleAppFilePath(path_ConsoleApp, path_Runner));

                // Under the runner's folder.
                Assert.Equal(path_Runner, Query.ConsoleAppFilePath(path_ConsoleApp, "Data.tsv"));

                // Under the runner's folder with the "user files" segment removed, in either separator.
                Assert.Equal(path_Deployed, Query.ConsoleAppFilePath(path_ConsoleApp, "user files/YOLO/models/model.pt"));
                Assert.Equal(path_Deployed, Query.ConsoleAppFilePath(path_ConsoleApp, @"user files\YOLO\models\model.pt"));

                // Not found anywhere, and nothing to look for.
                Assert.Null(Query.ConsoleAppFilePath(path_ConsoleApp, "user files/YOLO/models/base/yolo26x.pt"));
                Assert.Null(Query.ConsoleAppFilePath(null, "Data.tsv"));
                Assert.Null(Query.ConsoleAppFilePath(path_ConsoleApp, null));

                // A relative name beside the calling process is not the runner's file: the runner starts in its own
                // folder and would never see it.
                Directory.SetCurrentDirectory(directory_Caller);
                Assert.Null(Query.ConsoleAppFilePath(path_ConsoleApp, "Caller.tsv"));
                Assert.Equal(path_Runner, Query.ConsoleAppFilePath(path_ConsoleApp, "Data.tsv"));
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDirectory);

                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        /// <summary>
        /// Verifies that <see cref="Query.ConsoleAppDeployedPath(string?, string?)"/> places a relative path under the runner's deployed layout whether or not anything is there, and leaves an absolute path where it is.
        /// <para>It answers for the folders the runner writes into - the reports folder does not exist before the first run - and for a file a preflight has to name when it is missing, which is why nothing here is created.</para>
        /// </summary>
        [Fact]
        public void ConsoleAppDeployedPath()
        {
            string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            string path_ConsoleApp = Path.Combine(directory, Constants.FileName.YearBuiltPredictionConsoleApp);

            Assert.Equal(Path.Combine(directory, "reports"), Query.ConsoleAppDeployedPath(path_ConsoleApp, "user files/reports"));
            Assert.Equal(Path.Combine(directory, "reports"), Query.ConsoleAppDeployedPath(path_ConsoleApp, "reports"));
            Assert.Equal(@"C:\YOLO\reports", Query.ConsoleAppDeployedPath(path_ConsoleApp, @"C:\YOLO\reports"));
            Assert.Equal(Path.GetFullPath("reports"), Query.ConsoleAppDeployedPath(null, "reports"));
            Assert.Null(Query.ConsoleAppDeployedPath(path_ConsoleApp, " "));
        }
    }
}
