using System.Collections.Generic;
using System.IO;

namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.ConfigEnvironmentVariables(string)"/> returns the YOLO_CONFIG_DIR variable pointing at the .yolo-config folder of the working directory, and that it creates the folder when it is missing.
        /// </summary>
        [Fact]
        public void ConfigEnvironmentVariables()
        {
            string workingDirectory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_ConfigEnv_" + Path.GetRandomFileName());

            try
            {
                Directory.CreateDirectory(workingDirectory);

                Dictionary<string, string> environmentVariables = Query.ConfigEnvironmentVariables(workingDirectory);

                Assert.Equal(".yolo-config", Constants.DirectoryName.YoloConfig);
                Assert.Single(environmentVariables);
                Assert.Equal(Path.Combine(workingDirectory, Constants.DirectoryName.YoloConfig), environmentVariables["YOLO_CONFIG_DIR"]);
                Assert.True(Directory.Exists(Path.Combine(workingDirectory, Constants.DirectoryName.YoloConfig)));

                //A second call gives the same answer and does not disturb the folder
                Dictionary<string, string> environmentVariables_Again = Query.ConfigEnvironmentVariables(workingDirectory);
                Assert.Equal(environmentVariables["YOLO_CONFIG_DIR"], environmentVariables_Again["YOLO_CONFIG_DIR"]);
            }
            finally
            {
                if (Directory.Exists(workingDirectory))
                {
                    Directory.Delete(workingDirectory, true);
                }
            }
        }
    }
}
