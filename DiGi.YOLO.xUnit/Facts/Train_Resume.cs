using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;

namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the whole resume path of <see cref="Modify.Train(Classes.YOLOTrainingOptions?, System.Threading.CancellationToken)"/> against stand-in torch and ultralytics packages: the checkpoint is read, only --resume and --device reach ultralytics (data, epochs, patience, imgsz, batch, seed, amp, project and name are deliberately not sent), the success block reports the epoch entered, and a finished checkpoint, a checkpoint whose dataset is gone and one inside a YOLO\models folder are refused before a process starts.
        /// <para>The stand-in torch answers from a generated checkpoints.json keyed by file name, so no real ultralytics or GPU is needed. The stand-in ultralytics writes the keyword arguments it receives into best.pt, so the fact can read back exactly what the runner asked for.</para>
        /// </summary>
        [Fact]
        public void Train_Mock_Resume()
        {
            string? pythonPath = PythonPath_Runnable();
            if (string.IsNullOrWhiteSpace(pythonPath))
            {
                return;
            }

            string directory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Test_" + Path.GetRandomFileName(), "working dir");

            try
            {
                Directory.CreateDirectory(directory);

                string path_Configuration = Path.Combine(directory, "conf.yaml");
                File.WriteAllText(path_Configuration, "path: .\nnames:\n  0: Building");

                string path_Project = Path.Combine(directory, "runs", "detect");
                string directory_Run = Path.Combine(path_Project, "train9");
                Directory.CreateDirectory(Path.Combine(directory_Run, "weights"));

                string path_Last = Path.Combine(directory_Run, "weights", "last.pt");
                File.WriteAllText(path_Last, "dummy checkpoint");

                string path_Finished = Path.Combine(directory, "finished.pt");
                string path_NoData = Path.Combine(directory, "nodata.pt");
                string path_Models = Path.Combine(directory, "models.pt");
                File.WriteAllText(path_Finished, "dummy checkpoint");
                File.WriteAllText(path_NoData, "dummy checkpoint");
                File.WriteAllText(path_Models, "dummy checkpoint");

                JsonObject checkpoints = new()
                {
                    ["last.pt"] = MockCheckpoint(0, 6, path_Configuration, path_Project, "train9", true),
                    ["finished.pt"] = MockCheckpoint(-1, 6, path_Configuration, path_Project, "train9", false),
                    ["nodata.pt"] = MockCheckpoint(0, 6, Path.Combine(directory, "missing-data.yaml"), path_Project, "train9", true),
                    ["models.pt"] = MockCheckpoint(0, 6, path_Configuration, Path.Combine(directory, "YOLO", "models", "runs", "detect"), "train9", true)
                };

                WriteMockTorch(directory, checkpoints);

                //A train.py of an older build ignored every argument; the runner must never execute it
                File.WriteAllText(Path.Combine(directory, "train.py"), "raise SystemExit(7)");

                WriteMockUltralyticsResume(directory);

                Classes.YOLOTrainingResult? yOLOTrainingResult = Modify.Train(new Classes.YOLOTrainingOptions()
                {
                    //Set to prove they are ignored: a resume sends none of them
                    Batch = 4,
                    Epochs = 99,
                    ImageSize = 320,
                    Name = "train9",
                    Project = path_Project,
                    Seed = 11,
                    Device = "cpu",
                    PythonPath = pythonPath,
                    ResumePath = path_Last,
                    WorkingDirectory = directory
                });

                Assert.NotNull(yOLOTrainingResult);
                Assert.True(yOLOTrainingResult!.Succeeded, string.Join(Environment.NewLine, [.. yOLOTrainingResult.StandardOutput ?? [], .. yOLOTrainingResult.StandardError ?? []]));
                Assert.Equal(0, yOLOTrainingResult.ExitCode);
                Assert.True(yOLOTrainingResult.Resumed);
                Assert.Equal(2, yOLOTrainingResult.ResumedFromEpoch);
                Assert.Equal(path_Last, yOLOTrainingResult.StartModelPath);
                Assert.Equal(Query.FileSHA256(path_Last), yOLOTrainingResult.StartModelSHA256);
                Assert.Equal(Enums.ModelKind.Checkpoint, yOLOTrainingResult.StartModelKind);

                string path_Weights = Path.Combine(directory_Run, "weights", "best.pt");
                Assert.Equal(path_Weights, yOLOTrainingResult.WeightsPath);
                Assert.Equal(Query.FileSHA256(path_Weights), yOLOTrainingResult.SHA256);

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

                //The stale train.py was replaced by the one this build embeds
                Assert.Contains("argparse", File.ReadAllText(Path.Combine(directory, "train.py")));

                //Finished, dataset-gone and models-folder checkpoints are all refused before a process starts
                Classes.YOLOTrainingResult? ResumeRefused(string name)
                {
                    return Modify.Train(new Classes.YOLOTrainingOptions()
                    {
                        PythonPath = pythonPath,
                        ResumePath = Path.Combine(directory, name),
                        WorkingDirectory = directory
                    });
                }

                foreach (Classes.YOLOTrainingResult? yOLOTrainingResult_Refused in new Classes.YOLOTrainingResult?[]
                {
                    ResumeRefused("finished.pt"),
                    ResumeRefused("nodata.pt"),
                    ResumeRefused("models.pt")
                })
                {
                    Assert.NotNull(yOLOTrainingResult_Refused);
                    Assert.Equal(-1, yOLOTrainingResult_Refused!.ExitCode);
                    Assert.False(yOLOTrainingResult_Refused.Succeeded);
                    Assert.True(yOLOTrainingResult_Refused.Resumed);
                    Assert.Single(yOLOTrainingResult_Refused.StandardError ?? []);
                    Assert.Null(yOLOTrainingResult_Refused.StandardOutput);
                }
            }
            finally
            {
                string? directory_Root = Path.GetDirectoryName(directory);
                if (directory_Root != null && Directory.Exists(directory_Root))
                {
                    Directory.Delete(directory_Root, true);
                }
            }
        }

        /// <summary>
        /// Verifies that <see cref="Classes.YOLOCheckpointInformation"/> keeps what it is given through the string round trip and the clone - including the raw train_args, held as JSON text and answered as a JSON object - and that <see cref="Query.YOLOCheckpointInformation(string?, string?, string?, System.Threading.CancellationToken)"/> reads the epoch, ceiling and finished state of real ultralytics checkpoints written by the pinned version, and answers <c>null</c> for a missing or unreadable file.
        /// <para>The real checkpoints are machine specific, so they are read from the git-ignored DiGi.YOLO_Preflight.conf; the fact still asserts the serialization and missing-file behaviour without it.</para>
        /// </summary>
        [Fact]
        public void YOLOCheckpointInformation()
        {
            Classes.YOLOCheckpointInformation yOLOCheckpointInformation_Serialized = new(0.31, @"C:\data\conf.yaml", 2, 6, false, "train9", @"C:\YOLO\runs\detect", "{\"data\":\"conf.yaml\",\"epochs\":6}");

            Assert.False(yOLOCheckpointInformation_Serialized.Finished);
            Assert.Equal("conf.yaml", yOLOCheckpointInformation_Serialized.TrainArguments?["data"]?.GetValue<string>());

            string? json = Core.Convert.ToSystem_String(yOLOCheckpointInformation_Serialized);
            Classes.YOLOCheckpointInformation? yOLOCheckpointInformation_Actual = Core.Convert.ToDiGi<Classes.YOLOCheckpointInformation>(json)?.FirstOrDefault();
            Assert.NotNull(yOLOCheckpointInformation_Actual);
            Assert.Equal(0.31, yOLOCheckpointInformation_Actual!.BestFitness);
            Assert.Equal(@"C:\data\conf.yaml", yOLOCheckpointInformation_Actual.DataPath);
            Assert.Equal(2, yOLOCheckpointInformation_Actual.Epoch);
            Assert.Equal(6, yOLOCheckpointInformation_Actual.Epochs);
            Assert.False(yOLOCheckpointInformation_Actual.Finished);
            Assert.Equal("train9", yOLOCheckpointInformation_Actual.Name);
            Assert.Equal(@"C:\YOLO\runs\detect", yOLOCheckpointInformation_Actual.Project);
            Assert.Equal("conf.yaml", yOLOCheckpointInformation_Actual.TrainArguments?["data"]?.GetValue<string>());
            Assert.Equal(6, yOLOCheckpointInformation_Actual.TrainArguments?["epochs"]?.GetValue<int>());

            Core.xUnit.Query.SerializationCheck(yOLOCheckpointInformation_Serialized);

            Assert.Null(Query.YOLOCheckpointInformation(null));
            Assert.Null(Query.YOLOCheckpointInformation(Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Test_" + Path.GetRandomFileName() + ".pt")));

            Assembly assembly = Assembly.GetExecutingAssembly();
            Dictionary<string, string> settings = PreflightSettings(assembly);
            settings.TryGetValue("PythonPath", out string? path_Python);
            settings.TryGetValue("ModelPath", out string? path_Model);

            if (string.IsNullOrWhiteSpace(path_Python) || !File.Exists(path_Python) || string.IsNullOrWhiteSpace(path_Model) || !File.Exists(path_Model))
            {
                return;
            }

            //model.pt is the frozen train8 checkpoint: finished, its train_args the only surviving record of the run
            Classes.YOLOCheckpointInformation? yOLOCheckpointInformation = Query.YOLOCheckpointInformation(path_Model, path_Python);
            Assert.NotNull(yOLOCheckpointInformation);
            Assert.True(yOLOCheckpointInformation!.Finished);
            Assert.Null(yOLOCheckpointInformation.Epoch);
            Assert.Equal(150, yOLOCheckpointInformation.Epochs);
            Assert.Equal("conf.yaml", yOLOCheckpointInformation.DataPath);
            Assert.NotNull(yOLOCheckpointInformation.TrainArguments);
            Assert.Equal(150, yOLOCheckpointInformation.TrainArguments!["epochs"]?.GetValue<int>());

            //A file that exists but is not a readable checkpoint is answered as null rather than guessed at
            string path_Unreadable = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Test_" + Path.GetRandomFileName() + ".pt");

            try
            {
                File.WriteAllText(path_Unreadable, "not a checkpoint");
                Assert.Null(Query.YOLOCheckpointInformation(path_Unreadable, path_Python));
            }
            finally
            {
                if (File.Exists(path_Unreadable))
                {
                    File.Delete(path_Unreadable);
                }
            }
        }

        /// <summary>
        /// Continues a real interrupted training run through <see cref="Modify.Train(Classes.YOLOTrainingOptions?, System.Threading.CancellationToken)"/>: a 2-epoch run with save_period is cut back to its first per-epoch checkpoint, read by <see cref="Query.YOLOCheckpointInformation(string?, string?, string?, System.Threading.CancellationToken)"/>, and resumed, on a synthetic dataset.
        /// <para>The interpreter and start checkpoint are machine specific, so they are read from the git-ignored DiGi.YOLO_Preflight.conf and the fact returns without asserting when either is absent. It needs a GPU to finish in reasonable time and is meant to be run on its own. The frozen model.pt is hashed before and after: no run may change it.</para>
        /// </summary>
        [Fact]
        public void Train_Smoke_Resume()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            Dictionary<string, string> settings = PreflightSettings(assembly);
            settings.TryGetValue("PythonPath", out string? path_Python);
            settings.TryGetValue("ModelPath", out string? path_Model);

            if (string.IsNullOrWhiteSpace(path_Python) || !File.Exists(path_Python) || string.IsNullOrWhiteSpace(path_Model) || !File.Exists(path_Model))
            {
                return;
            }

            string directory = Path.Combine(Core.xUnit.Query.ReportsDirectory(assembly)!, "YOLO_Train_Smoke_Resume");
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }

            Directory.CreateDirectory(directory);

            string path_Configuration = BuildSyntheticDataset(path_Python, directory);
            string path_Project = Path.Combine(directory, "training", "runs", "detect");

            //An interrupted checkpoint is not something ultralytics leaves behind on a clean exit - last.pt is stripped
            //to epoch -1. A 2-epoch run with save_period=1 does leave the per-epoch checkpoints, so one is trained and
            //the first (epoch 0, optimizer state intact) is copied over last.pt to stand in for the cut run
            string path_Script = Path.Combine(directory, "interrupt.py");
            File.WriteAllLines(path_Script,
            [
                "import sys",
                "from ultralytics import YOLO",
                "model = YOLO(sys.argv[1])",
                "model.train(data=sys.argv[2], epochs=2, save_period=1, imgsz=320, batch=8, amp=False, project=sys.argv[3], name='interrupted', seed=0, workers=0)",
            ]);

            (int exitCode_Interrupt, _, List<string> standardError_Interrupt) = Query.ExecuteProcess(path_Python!, string.Format(CultureInfo.InvariantCulture, "\"{0}\" \"{1}\" \"{2}\" \"{3}\"", path_Script, path_Model, path_Configuration, path_Project), directory, Query.ConfigEnvironmentVariables(directory));
            Assert.True(exitCode_Interrupt == 0, string.Join(Environment.NewLine, standardError_Interrupt));

            string directory_Weights = Path.Combine(path_Project, "interrupted", "weights");
            string path_Last = Path.Combine(directory_Weights, "last.pt");
            string path_Epoch = Path.Combine(directory_Weights, "epoch0.pt");
            Assert.True(File.Exists(path_Epoch), string.Join(Environment.NewLine, Directory.GetFiles(directory_Weights)));
            File.Copy(path_Epoch, path_Last, true);

            Classes.YOLOCheckpointInformation? yOLOCheckpointInformation = Query.YOLOCheckpointInformation(path_Last, path_Python);
            Assert.NotNull(yOLOCheckpointInformation);
            Assert.False(yOLOCheckpointInformation!.Finished);
            Assert.Equal(1, yOLOCheckpointInformation.Epoch);
            Assert.Equal(2, yOLOCheckpointInformation.Epochs);
            Assert.NotNull(yOLOCheckpointInformation.TrainArguments);

            Classes.YOLOCheckpointInformation? yOLOCheckpointInformation_Finished = Query.YOLOCheckpointInformation(path_Model, path_Python);
            Assert.NotNull(yOLOCheckpointInformation_Finished);
            Assert.True(yOLOCheckpointInformation_Finished!.Finished);

            string? sHA256_Model = Query.FileSHA256(path_Model);
            //Hashed before the run: the resumed training overwrites last.pt in place, so the file no longer holds the start identity afterwards
            string? sHA256_Last = Query.FileSHA256(path_Last);

            Classes.YOLOTrainingOptions yOLOTrainingOptions = new()
            {
                //Ignored on resume, and left at values a fresh run would reject silently here - a resume sends neither
                Epochs = 99,
                ImageSize = 320,
                PythonPath = path_Python,
                ResumePath = path_Last,
                WorkingDirectory = path_Configuration.Substring(0, path_Configuration.LastIndexOf(Path.DirectorySeparatorChar))
            };

            Classes.YOLOTrainingResult? yOLOTrainingResult = Modify.Train(yOLOTrainingOptions);
            Assert.NotNull(yOLOTrainingResult);
            Assert.True(yOLOTrainingResult!.Succeeded, string.Join(Environment.NewLine, yOLOTrainingResult.StandardError ?? []));

            Assert.True(yOLOTrainingResult.Resumed);
            Assert.Equal(2, yOLOTrainingResult.ResumedFromEpoch);
            Assert.Equal(Enums.ModelKind.Checkpoint, yOLOTrainingResult.StartModelKind);
            Assert.Equal(path_Last, yOLOTrainingResult.StartModelPath);
            Assert.Equal(sHA256_Last, yOLOTrainingResult.StartModelSHA256);
            Assert.Equal(Path.Combine(directory_Weights, "best.pt"), yOLOTrainingResult.WeightsPath);
            Assert.Equal(Query.FileSHA256(yOLOTrainingResult.WeightsPath), yOLOTrainingResult.SHA256);

            //The best.pt of a finished run is stripped as surely as its last.pt, so it too reports finished
            Classes.YOLOCheckpointInformation? yOLOCheckpointInformation_Best = Query.YOLOCheckpointInformation(yOLOTrainingResult.WeightsPath, path_Python);
            Assert.NotNull(yOLOCheckpointInformation_Best);
            Assert.True(yOLOCheckpointInformation_Best!.Finished);

            Assert.Equal(sHA256_Model, Query.FileSHA256(path_Model));
        }

        /// <summary>
        /// Writes a stand-in torch module and the checkpoints.json it answers from into the working directory, so checkpoint.py, check.py and train.py read the checkpoints the fact describes without real torch.
        /// </summary>
        /// <param name="directory">The working directory the scripts run in.</param>
        /// <param name="checkpoints">The checkpoint dictionaries, keyed by file name.</param>
        private static void WriteMockTorch(string directory, JsonObject checkpoints)
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

            File.WriteAllText(Path.Combine(directory, "checkpoints.json"), checkpoints.ToJsonString());
        }

        /// <summary>
        /// Writes a stand-in ultralytics package whose YOLO restores the run directory from the checkpoint on a resume, so the runner's arguments can be read back from the weights it writes.
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

        /// <summary>
        /// Generates a small synthetic dataset - a light rectangle on noise, labelled "Building" - and writes it with <see cref="Modify.Write(Classes.YOLOModel?)"/>, returning the path of its conf.yaml.
        /// </summary>
        /// <param name="path_Python">The interpreter that runs the image generator (it uses Pillow, which ultralytics depends on).</param>
        /// <param name="directory">The report directory the dataset is written under.</param>
        /// <returns>The path of the dataset configuration file.</returns>
        private static string BuildSyntheticDataset(string path_Python, string directory)
        {
            string directory_Source = Path.Combine(directory, "source");
            Directory.CreateDirectory(directory_Source);

            string path_Script = Path.Combine(directory, "images.py");
            File.WriteAllLines(path_Script,
            [
                "import random, sys",
                "from PIL import Image, ImageDraw",
                "random.seed(0)",
                "for i in range(int(sys.argv[2])):",
                "    image = Image.effect_noise((320, 320), 40).convert('RGB')",
                "    w, h = random.randint(60, 160), random.randint(60, 160)",
                "    x, y = random.randint(0, 320 - w), random.randint(0, 320 - h)",
                "    ImageDraw.Draw(image).rectangle([x, y, x + w, y + h], fill=(230, 230, 230))",
                "    name = 'image%03d' % i",
                "    image.save('%s/%s.jpeg' % (sys.argv[1], name), 'JPEG')",
                "    print('%s %r %r %r %r' % (name, (x + w / 2) / 320, (y + h / 2) / 320, w / 320, h / 320))",
            ]);

            (int exitCode, List<string> standardOutput, List<string> standardError) = Query.ExecuteProcess(path_Python, string.Format(CultureInfo.InvariantCulture, "\"{0}\" \"{1}\" 24", path_Script, directory_Source), directory);
            Assert.True(exitCode == 0, string.Join(Environment.NewLine, standardError));

            string directory_Dataset = Path.Combine(directory, "training");
            Classes.YOLOModel yOLOModel = new(directory_Dataset);
            yOLOModel.Add("Building");

            int index = 0;
            foreach (string line in standardOutput)
            {
                string[] values = line.Split(' ');
                if (values.Length != 5)
                {
                    continue;
                }

                string path_Image = Path.Combine(directory_Source, values[0] + ".jpeg");
                Enums.Category category = index < 16 ? Enums.Category.Train : index < 20 ? Enums.Category.Validate : Enums.Category.Test;
                yOLOModel.Add(path_Image, category);
                yOLOModel.Add(path_Image, "Building", new Classes.BoundingBox(double.Parse(values[1], CultureInfo.InvariantCulture), double.Parse(values[2], CultureInfo.InvariantCulture), double.Parse(values[3], CultureInfo.InvariantCulture), double.Parse(values[4], CultureInfo.InvariantCulture)));
                index++;
            }

            Assert.Equal(24, index);
            Assert.True(Modify.Write(yOLOModel));

            return Path.Combine(directory_Dataset, "conf.yaml");
        }
    }
}
