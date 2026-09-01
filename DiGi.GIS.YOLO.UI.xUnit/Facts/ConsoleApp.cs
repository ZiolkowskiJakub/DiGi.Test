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
                string json = "{\"CountyIds\":[2212],\"ScratchDirectory\":\"\"}";
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
