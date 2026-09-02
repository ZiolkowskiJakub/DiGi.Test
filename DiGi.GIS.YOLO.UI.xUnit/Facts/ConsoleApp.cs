using DiGi.GIS.YOLO.UI.Classes;
using System.IO;
using System.Threading.Tasks;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="ConsoleApp.Program.Main(string[])"/> returns exit code 1 when the specified options file does not exist.
        /// </summary>
        [Fact]
        public async Task ConsoleApp_InvalidOptionsPath_ReturnsExitCode1()
        {
            string[] args = ["non_existent_file_path_12345.json"];
            int exitCode = await ConsoleApp.Program.Main(args);
            Assert.Equal(1, exitCode);
        }

        /// <summary>
        /// Tests that <see cref="ConsoleApp.Program.Main(string[])"/> returns exit code 1 when options JSON contains an empty county list.
        /// </summary>
        [Fact]
        public async Task ConsoleApp_EmptyCountyIds_ReturnsExitCode1()
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), $"options_test_{System.Guid.NewGuid()}.json");
            try
            {
                string json = "{\"CountyIds\":[],\"ScratchDirectory\":\"scratch\"}";
                File.WriteAllText(tempFilePath, json);

                string[] args = [tempFilePath];
                int exitCode = await ConsoleApp.Program.Main(args);
                Assert.Equal(1, exitCode);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        /// <summary>
        /// Tests that <see cref="ConsoleApp.Program.Main(string[])"/> returns exit code 1 when options JSON specifies an empty scratch directory.
        /// </summary>
        [Fact]
        public async Task ConsoleApp_EmptyScratchDirectory_ReturnsExitCode1()
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), $"options_test_{System.Guid.NewGuid()}.json");
            try
            {
                string json = "{\"CountyIds\":[73485],\"ScratchDirectory\":\"\"}";
                File.WriteAllText(tempFilePath, json);

                string[] args = [tempFilePath];
                int exitCode = await ConsoleApp.Program.Main(args);
                Assert.Equal(1, exitCode);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        /// <summary>
        /// Tests that <see cref="ConsoleApp.Program.Main(string[])"/> returns exit code 3 when the options are complete but the authorization key is not on disk.
        /// <para>Nothing the pipeline does is possible without it, so this is a separate code from a bad option file and from a step that failed while running.</para>
        /// </summary>
        [Fact]
        public async Task ConsoleApp_MissingKey_ReturnsExitCode3()
        {
            //Never overwrite a real key sitting beside the test assembly
            string? path_Key = Query.ConfigurationFilePath(Constants.FileName.GISWebAPIClientConfigurationFile);
            Assert.False(string.IsNullOrWhiteSpace(path_Key));
            Assert.False(File.Exists(path_Key), $"This fact requires no deployed '{Constants.FileName.GISWebAPIClientConfigurationFile}' beside the test assembly, but one is at '{path_Key}'.");

            string tempFilePath = Path.Combine(Path.GetTempPath(), $"options_test_{System.Guid.NewGuid()}.json");
            try
            {
                string json = "{\"CountyIds\":[73485],\"ScratchDirectory\":\"scratch\"}";
                File.WriteAllText(tempFilePath, json);

                string[] args = [tempFilePath];
                int exitCode = await ConsoleApp.Program.Main(args);
                Assert.Equal(3, exitCode);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        /// <summary>
        /// Tests that <see cref="ConsoleApp.Program.Main(string[])"/> returns exit code 2 when the machine cannot run the detector, and that it learns this before contacting anything.
        /// <para>The preflight is the orchestrator's own first step, so a named interpreter that does not exist fails the run without a single request going out - which is the whole point of gating on it rather than on the detector's standard error.</para>
        /// </summary>
        [Fact]
        public async Task ConsoleApp_FailedPreflight_ReturnsExitCode2()
        {
            string? path_Key = Query.ConfigurationFilePath(Constants.FileName.GISWebAPIClientConfigurationFile);
            Assert.False(string.IsNullOrWhiteSpace(path_Key));
            Assert.False(File.Exists(path_Key), $"This fact writes a placeholder '{Constants.FileName.GISWebAPIClientConfigurationFile}' beside the test assembly and one is already there, at '{path_Key}'.");

            string tempFilePath = Path.Combine(Path.GetTempPath(), $"options_test_{System.Guid.NewGuid()}.json");
            try
            {
                File.WriteAllText(path_Key!, "Key=\"preflight-fact-placeholder\"");

                //A named interpreter that cannot be started, so the preflight fails on this machine whatever it has installed
                string json = "{\"CountyIds\":[73485],\"ScratchDirectory\":\"scratch\",\"PythonPath\":\"C:\\\\non_existent_python.exe\",\"RunPrediction\":true,\"ExportImages\":false,\"Score\":false,\"UpdateDetections\":false,\"UpdateYearBuiltData\":false,\"UpdatePredictedYearBuilt\":false}";
                File.WriteAllText(tempFilePath, json);

                string[] args = [tempFilePath];
                int exitCode = await ConsoleApp.Program.Main(args);
                Assert.Equal(2, exitCode);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }

                if (File.Exists(path_Key))
                {
                    File.Delete(path_Key!);
                }
            }
        }

        /// <summary>
        /// Tests that <see cref="Query.Key(string?)"/> returns null when the configuration file does not exist.
        /// </summary>
        [Fact]
        public void ConsoleApp_QueryKey_NonExistentPath_ReturnsNull()
        {
            string? key = Query.Key("non_existent_config.conf");
            Assert.Null(key);
        }

        /// <summary>
        /// Tests that <see cref="Query.ModelPath(string?)"/> returns null when given null or empty path, and returns a resolved path for existing weights.
        /// </summary>
        [Fact]
        public void ConsoleApp_QueryModelPath_ResolvesExpectedly()
        {
            Assert.Null(Query.ModelPath(null));
            Assert.Null(Query.ModelPath("   "));

            string tempFile = Path.Combine(Path.GetTempPath(), $"model_{System.Guid.NewGuid()}.pt");
            try
            {
                File.WriteAllText(tempFile, "fake-weights");
                string? resolved = Query.ModelPath(tempFile);
                Assert.NotNull(resolved);
                Assert.True(File.Exists(resolved));
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
    }
}
