using System;
using System.IO;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the shared interrupted-run detection used by both the window offer and the task preflight: a folder with an unfinished checkpoint is reported with its epoch and ceiling, and a missing folder, a missing checkpoint, a completed run, and a finished or unreadable checkpoint are not.
        /// <para>The checkpoint is read through a stand-in torch module written into the working directory, so no real ultralytics or GPU is needed. The interpreter is machine specific, so the fact returns without asserting when none is installed.</para>
        /// <para>Medium test (0.4 s): runs when DIGI_TEST_MAX_DURATION is Medium (the default) or Long.</para>
        /// </summary>
        [MediumFact]
        public void InterruptedYOLOTrainingRun()
        {
            string? pythonPath = PythonPath_Runnable();
            if (string.IsNullOrWhiteSpace(pythonPath))
            {
                return;
            }

            string directory = Path.Combine(Path.GetTempPath(), "DiGi.GIS.PostgreSQL.UI.xUnit.Interrupted." + Guid.NewGuid().ToString("N"));

            try
            {
                Directory.CreateDirectory(directory);

                string path_Configuration = Path.Combine(directory, "conf.yaml");
                File.WriteAllText(path_Configuration, "path: .\nnames:\n  0: Building");

                string projectDirectory = Path.Combine(directory, "runs", "detect");
                string directory_Run = Path.Combine(projectDirectory, "train9");
                Directory.CreateDirectory(Path.Combine(directory_Run, "weights"));
                string path_Last = Path.Combine(directory_Run, "weights", "last.pt");

                WriteMockTorch(directory);

                // A folder with no checkpoint is not an interrupted run.
                Assert.Null(Query.InterruptedYOLOTrainingRun(projectDirectory, "train9", pythonPath, directory));

                // An unfinished checkpoint is reported with its epoch and ceiling.
                File.WriteAllText(path_Last, "dummy checkpoint");
                WriteMockCheckpoints(directory, MockCheckpoint(0, 100, path_Configuration, projectDirectory, "train9", true));

                DiGi.YOLO.Classes.YOLOCheckpointInformation? yOLOCheckpointInformation = Query.InterruptedYOLOTrainingRun(projectDirectory, "train9", pythonPath, directory);
                Assert.NotNull(yOLOCheckpointInformation);
                Assert.Equal(1, yOLOCheckpointInformation!.Epoch);
                Assert.Equal(100, yOLOCheckpointInformation.Epochs);
                Assert.False(yOLOCheckpointInformation.Finished);

                // A completed run is not an interrupted one.
                File.WriteAllText(Path.Combine(directory_Run, "train9.pt"), "completed");
                Assert.Null(Query.InterruptedYOLOTrainingRun(projectDirectory, "train9", pythonPath, directory));
                File.Delete(Path.Combine(directory_Run, "train9.pt"));

                // A finished checkpoint is not resumable.
                WriteMockCheckpoints(directory, MockCheckpoint(-1, 100, path_Configuration, projectDirectory, "train9", false));
                Assert.Null(Query.InterruptedYOLOTrainingRun(projectDirectory, "train9", pythonPath, directory));

                // An unreadable checkpoint is not offered.
                WriteMockCheckpoints(directory, null);
                Assert.Null(Query.InterruptedYOLOTrainingRun(projectDirectory, "train9", pythonPath, directory));

                // A missing folder is not offered.
                Assert.Null(Query.InterruptedYOLOTrainingRun(projectDirectory, "missing", pythonPath, directory));
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
