using DiGi.GIS.YOLO.UI.Enums;
using System;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the resume path of <c>Modify.RunYOLOTrainingAsync</c>: a finished checkpoint, one whose dataset is gone, one that cannot be read and one whose recorded run folder was moved or renamed are each refused by name before the training starts, and a valid interrupted folder passes, hands ultralytics only <c>resume</c> and <c>device</c>, and copies the produced weights the way a fresh run does.
        /// <para>The checkpoint is read through stand-in torch and ultralytics modules written into the working directory, so no real ultralytics or GPU is needed. The interpreter is machine specific, so the fact returns without asserting when none is installed.</para>
        /// </summary>
        [Fact]
        public async Task RunYOLOTrainingAsync_Resume()
        {
            string? pythonPath = PythonPath_Runnable();
            if (string.IsNullOrWhiteSpace(pythonPath))
            {
                return;
            }

            string directory = Path.Combine(Path.GetTempPath(), "DiGi.GIS.YOLO.UI.xUnit.Resume." + Guid.NewGuid().ToString("N"));

            try
            {
                Directory.CreateDirectory(directory);

                string path_Configuration = Path.Combine(directory, "conf.yaml");
                File.WriteAllText(path_Configuration, "path: .\nnames:\n  0: Building");

                string projectDirectory = Path.Combine(directory, "runs", "detect");
                string directory_Run = Path.Combine(projectDirectory, "train9");
                Directory.CreateDirectory(Path.Combine(directory_Run, "weights"));
                string path_Last = Path.Combine(directory_Run, "weights", "last.pt");
                File.WriteAllText(path_Last, "dummy checkpoint");

                WriteMockTorch(directory);
                WriteMockUltralyticsResume(directory);

                Classes.YOLOTrainingRunOptions Options()
                {
                    return new Classes.YOLOTrainingRunOptions()
                    {
                        DatasetOptions = new Classes.YOLOTrainingDatasetOptions() { OutputDirectory = directory },
                        Device = "cpu",
                        ProjectDirectory = projectDirectory,
                        PythonPath = pythonPath,
                        ResumeTraining = true,
                        RunName = "train9",
                        Steps = [YOLOTrainingStep.Train],
                        WorkingDirectory = directory
                    };
                }

                // Each bad checkpoint is refused by name, before the training process starts, and writes no output weights.
                (string Name, JsonNode? Checkpoint)[] checkpoints_Refused =
                [
                    ("finished", MockCheckpoint(-1, 6, path_Configuration, projectDirectory, "train9", false)),
                    ("nodata", MockCheckpoint(0, 6, Path.Combine(directory, "missing-data.yaml"), projectDirectory, "train9", true)),
                    ("unreadable", null),
                    ("moved", MockCheckpoint(0, 6, path_Configuration, Path.Combine(directory, "elsewhere"), "train9", true))
                ];

                foreach ((string name, JsonNode? checkpoint) in checkpoints_Refused)
                {
                    WriteMockCheckpoints(directory, checkpoint);
                    File.WriteAllText(path_Last, name);

                    Classes.YOLOTrainingRunResult? yOLOTrainingRunResult = await Modify.RunYOLOTrainingAsync(null, Options());
                    Assert.NotNull(yOLOTrainingRunResult);
                    Assert.Contains(nameof(DiGi.YOLO.Query.YOLOCheckpointInformation), yOLOTrainingRunResult!.FailedStepNames);
                    Assert.Equal(Enums.YearBuiltPredictionExitCode.Configuration, Query.YOLOTrainingRunExitCode(yOLOTrainingRunResult));
                    Assert.False(File.Exists(Path.Combine(directory_Run, "train9.pt")));
                }

                // A valid interrupted folder passes the preflight, and the tail copies and re-checks the produced weights.
                WriteMockCheckpoints(directory, MockCheckpoint(0, 6, path_Configuration, projectDirectory, "train9", true));
                File.WriteAllText(path_Last, "dummy checkpoint");

                Classes.YOLOTrainingRunResult? yOLOTrainingRunResult_Resume = await Modify.RunYOLOTrainingAsync(null, Options());
                Assert.NotNull(yOLOTrainingRunResult_Resume);
                Assert.Empty(yOLOTrainingRunResult_Resume!.FailedStepNames);
                Assert.Equal(Enums.YearBuiltPredictionExitCode.Succeeded, Query.YOLOTrainingRunExitCode(yOLOTrainingRunResult_Resume));
                Assert.True(yOLOTrainingRunResult_Resume.Resumed);
                Assert.Equal(2, yOLOTrainingRunResult_Resume.ResumedFromEpoch);
                Assert.Equal(path_Last, yOLOTrainingRunResult_Resume.StartWeightsPath);
                Assert.Equal(DiGi.YOLO.Query.FileSHA256(path_Last), yOLOTrainingRunResult_Resume.StartWeightsSHA256);

                string path_Weights = Path.Combine(directory_Run, "train9.pt");
                Assert.Equal(path_Weights, yOLOTrainingRunResult_Resume.WeightsPath);
                Assert.True(File.Exists(path_Weights));
                Assert.Equal(DiGi.YOLO.Query.FileSHA256(path_Weights), yOLOTrainingRunResult_Resume.WeightsSHA256);

                // The stand-in ultralytics received only resume and device; every other argument is restored from the checkpoint.
                JsonObject? jsonObject = JsonNode.Parse(File.ReadAllText(path_Weights)) as JsonObject;
                Assert.NotNull(jsonObject);
                Assert.True(jsonObject!["resume"]?.GetValue<bool>());
                Assert.Equal("cpu", jsonObject["device"]?.GetValue<string>());
                Assert.False(jsonObject.ContainsKey("data"));
                Assert.False(jsonObject.ContainsKey("epochs"));
                Assert.False(jsonObject.ContainsKey("patience"));
                Assert.False(jsonObject.ContainsKey("imgsz"));
                Assert.False(jsonObject.ContainsKey("batch"));
                Assert.False(jsonObject.ContainsKey("seed"));
                Assert.False(jsonObject.ContainsKey("amp"));
                Assert.False(jsonObject.ContainsKey("project"));
                Assert.False(jsonObject.ContainsKey("name"));
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        /// <summary>
        /// Returns the first interpreter on PATH that actually starts, or <c>null</c> when none does.
        /// <para>A path that exists is not enough: with no Python installed, Windows still puts the App Installer redirector (a Microsoft Store app execution alias) on PATH as <c>python.exe</c>, which exits non-zero without running anything.</para>
        /// </summary>
        /// <returns>The path of a working interpreter, or <c>null</c>.</returns>
        private static string? PythonPath_Runnable()
        {
            foreach (string pythonPath in DiGi.YOLO.Query.PythonPaths())
            {
                (int exitCode, _, _) = DiGi.YOLO.Query.ExecuteProcess(pythonPath, "-c \"import sys\"", Path.GetTempPath());
                if (exitCode == 0)
                {
                    return pythonPath;
                }
            }

            return null;
        }

        /// <summary>
        /// Writes a stand-in <c>torch</c> module into the working directory, so <c>checkpoint.py</c>, <c>check.py</c> and <c>train.py</c> read the checkpoint the accompanying <c>checkpoints.json</c> describes without real torch.
        /// </summary>
        /// <param name="directory">The working directory the scripts run in.</param>
        private static void WriteMockTorch(string directory)
        {
            File.WriteAllLines(Path.Combine(directory, "torch.py"),
            [
                "import json, os",
                "",
                "__version__ = '0.0.0'",
                "",
                "class _Cuda:",
                "    @staticmethod",
                "    def is_available():",
                "        return False",
                "",
                "cuda = _Cuda()",
                "",
                "with open(os.path.join(os.path.dirname(os.path.abspath(__file__)), 'checkpoints.json'), 'r', encoding='utf-8') as file:",
                "    _CHECKPOINTS = json.load(file)",
                "",
                "def load(path, map_location=None, weights_only=False):",
                "    return _CHECKPOINTS.get(os.path.basename(path), {})",
            ]);
        }

        /// <summary>
        /// Writes the single checkpoint the stand-in torch module answers for <c>last.pt</c>, so a case can be swapped without touching the module.
        /// </summary>
        /// <param name="directory">The working directory the scripts run in.</param>
        /// <param name="checkpoint">The checkpoint dictionary, or <c>null</c> for one torch answers as unreadable.</param>
        private static void WriteMockCheckpoints(string directory, JsonNode? checkpoint)
        {
            JsonObject checkpoints = new()
            {
                ["last.pt"] = checkpoint
            };

            File.WriteAllText(Path.Combine(directory, "checkpoints.json"), checkpoints.ToJsonString());
        }

        /// <summary>
        /// Writes a stand-in <c>ultralytics</c> package whose <c>YOLO</c> restores the run directory from the checkpoint on a resume, so the runner's arguments can be read back from the weights it writes.
        /// </summary>
        /// <param name="directory">The working directory the scripts run in.</param>
        private static void WriteMockUltralyticsResume(string directory)
        {
            string directory_Mock = Path.Combine(directory, "ultralytics");
            Directory.CreateDirectory(directory_Mock);

            File.WriteAllLines(Path.Combine(directory_Mock, "__init__.py"),
            [
                "import json, os, torch",
                "",
                "__version__ = '0.0.0'",
                "",
                "class _Trainer:",
                "    pass",
                "",
                "class YOLO:",
                "    def __init__(self, model):",
                "        self.model = model",
                "        self.trainer = None",
                "",
                "    def train(self, **kwargs):",
                "        checkpoint = torch.load(self.model) if kwargs.get('resume') else {}",
                "        train_args = checkpoint.get('train_args', {}) if isinstance(checkpoint, dict) else {}",
                "        kwargs['model'] = self.model",
                "        kwargs['YOLO_CONFIG_DIR'] = os.environ.get('YOLO_CONFIG_DIR')",
                "        project = kwargs.get('project') or train_args.get('project')",
                "        name = kwargs.get('name') or train_args.get('name') or 'train'",
                "        weights = os.path.join(project, name, 'weights')",
                "        os.makedirs(weights, exist_ok=True)",
                "        path = os.path.join(weights, 'best.pt')",
                "        with open(path, 'w') as file:",
                "            file.write(json.dumps(kwargs, sort_keys=True))",
                "        self.trainer = _Trainer()",
                "        self.trainer.best = path",
                "        self.trainer.amp = kwargs.get('amp', False)",
            ]);
        }

        /// <summary>
        /// Builds one stand-in checkpoint dictionary in the shape ultralytics stores.
        /// </summary>
        /// <param name="epoch">The stored 0-based epoch, or -1 for a finished checkpoint.</param>
        /// <param name="epochs">The epoch ceiling recorded in train_args.</param>
        /// <param name="data">The dataset path recorded in train_args.</param>
        /// <param name="project">The run directory parent recorded in train_args.</param>
        /// <param name="name">The run directory name recorded in train_args.</param>
        /// <param name="optimizer">Whether the checkpoint carries optimizer state.</param>
        /// <returns>The checkpoint dictionary.</returns>
        private static JsonObject MockCheckpoint(int epoch, int epochs, string data, string project, string name, bool optimizer)
        {
            return new JsonObject()
            {
                ["epoch"] = epoch,
                ["optimizer"] = optimizer ? new JsonObject() { ["state"] = new JsonObject() } : null,
                ["best_fitness"] = 0.5,
                ["train_args"] = new JsonObject()
                {
                    ["data"] = data,
                    ["epochs"] = epochs,
                    ["project"] = project,
                    ["name"] = name
                }
            };
        }
    }
}
