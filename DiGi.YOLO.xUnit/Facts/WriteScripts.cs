using DiGi.YOLO;
using System.IO;

namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Modify.WriteScripts(string?)"/> copies all Python scripts and configuration files from output YOLO folder into the specified directory.
        /// </summary>
        [Fact]
        public void WriteScripts()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Test_" + Path.GetRandomFileName());
            try
            {
                bool success = Modify.WriteScripts(tempDirectory);
                Assert.True(success);

                string trainPath = Path.Combine(tempDirectory, "train.py");
                string predictPath = Path.Combine(tempDirectory, "predict.py");
                string utilsPath = Path.Combine(tempDirectory, "utils.py");
                string requirementsPath = Path.Combine(tempDirectory, "requirements.txt");
                string confPath = Path.Combine(tempDirectory, "conf.yaml");

                Assert.True(File.Exists(trainPath));
                Assert.True(File.Exists(predictPath));
                Assert.True(File.Exists(utilsPath));
                Assert.True(File.Exists(requirementsPath));
                Assert.True(File.Exists(confPath));

                string predictContent = File.ReadAllText(predictPath);
                Assert.Contains("argparse", predictContent);
                Assert.Contains("--model", predictContent);
                Assert.Contains("--source", predictContent);
                Assert.Contains("--conf", predictContent);
                Assert.Contains("--output", predictContent);

                string trainContent = File.ReadAllText(trainPath);
                Assert.Contains("epochs=150", trainContent);

                string utilsContent = File.ReadAllText(utilsPath);
                Assert.Contains("isdigit()", utilsContent);
                Assert.Contains("model.pt", utilsContent);

                string requirementsContent = File.ReadAllText(requirementsPath);
                Assert.Contains("ultralytics", requirementsContent);
                Assert.Contains("torch", requirementsContent);

                string confContent = File.ReadAllText(confPath);
                Assert.Contains("path: training", confContent);
            }
            finally
            {
                if (Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, true);
                }
            }
        }
    }
}
