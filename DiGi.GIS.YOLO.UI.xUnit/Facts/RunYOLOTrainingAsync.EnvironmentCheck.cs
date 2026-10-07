using DiGi.GIS.YOLO.UI.Enums;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the interpreter preflight of <c>Modify.RunYOLOTrainingAsync</c> does not write its scripts into a dataset directory that the <c>Dataset</c> step of the same run is still to build: with no working directory the preflight probes in its own scratch folder, so a new dataset directory stays empty and the dataset builder no longer refuses it as a foreign folder.
        /// <para>The interpreter is a file that exists but is not a real one, so the run reaches the preflight and then stops there, and no real Python, ultralytics, GPU or Web API client is needed.</para>
        /// <para>Short test: runs when DIGI_TEST_MAX_DURATION is Short or higher (the default is Medium).</para>
        /// </summary>
        [Fact]
        public async Task RunYOLOTrainingAsync_EnvironmentCheckDoesNotWriteIntoDataset()
        {
            string directory = Path.Combine(Path.GetTempPath(), "DiGi.GIS.YOLO.UI.xUnit.EnvCheck." + Guid.NewGuid().ToString("N"));

            try
            {
                Directory.CreateDirectory(directory);

                string datasetDirectory = Path.Combine(directory, "dataset");
                Directory.CreateDirectory(datasetDirectory);

                string path_Start = Path.Combine(directory, "start.pt");
                File.WriteAllText(path_Start, "not weights");

                // Exists, so the preflight's existence check passes and the run reaches the interpreter preflight; not a
                // real interpreter, so that preflight fails and the run stops there, before any step could write.
                string path_Python = Path.Combine(directory, "python.exe");
                File.WriteAllText(path_Python, "not an interpreter");

                string runsDirectory = Path.Combine(directory, "runs");

                Classes.YOLOTrainingRunOptions yOLOTrainingRunOptions = new()
                {
                    DatasetOptions = new Classes.YOLOTrainingDatasetOptions() { OutputDirectory = datasetDirectory },
                    Device = "cpu",
                    ProjectDirectory = runsDirectory,
                    PythonPath = path_Python,
                    RunName = "train9",
                    StartWeightsPath = path_Start,
                    Steps = [YOLOTrainingStep.Train]
                    // WorkingDirectory is left null - the scenario under test
                };

                Classes.YOLOTrainingRunResult? yOLOTrainingRunResult = await Modify.RunYOLOTrainingAsync(null, yOLOTrainingRunOptions);
                Assert.NotNull(yOLOTrainingRunResult);
                Assert.Contains(nameof(DiGi.YOLO.Query.YOLOEnvironmentResult), yOLOTrainingRunResult!.FailedStepNames);

                // The point of the fix: the preflight left the still-to-be-built dataset directory empty.
                Assert.Empty(Directory.EnumerateFileSystemEntries(datasetDirectory));
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }
    }
}
