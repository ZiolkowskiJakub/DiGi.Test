using System.IO;
using System.Linq;

namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.YOLOTrainingOptions"/> keeps the values it is given, carries the train8 defaults, survives the round trip through its string form, and clones identically.
        /// </summary>
        [Fact]
        public void YOLOTrainingOptions()
        {
            Classes.YOLOTrainingOptions yOLOTrainingOptions_Default = new();
            Assert.True(yOLOTrainingOptions_Default.Amp);
            Assert.Equal(16, yOLOTrainingOptions_Default.Batch);
            Assert.Equal(150, yOLOTrainingOptions_Default.Epochs);
            Assert.Equal(640, yOLOTrainingOptions_Default.ImageSize);
            Assert.Equal(50, yOLOTrainingOptions_Default.Patience);
            Assert.Equal(0, yOLOTrainingOptions_Default.Seed);
            Assert.Null(yOLOTrainingOptions_Default.Device);
            Assert.Null(yOLOTrainingOptions_Default.Name);
            Assert.Null(yOLOTrainingOptions_Default.ResumePath);

            Classes.YOLOTrainingOptions yOLOTrainingOptions = new()
            {
                Amp = false,
                Batch = 8,
                ConfigurationFilePath = @"C:\YOLO\training\conf.yaml",
                Device = "0",
                Epochs = 300,
                ImageSize = 640,
                ModelPath = @"C:\YOLO\models\base\yolo26x.pt",
                Name = "train9_fresh",
                Patience = 75,
                Project = @"C:\YOLO\runs\detect",
                PythonPath = @"C:\Python\python.exe",
                ResumePath = @"C:\YOLO\runs\detect\train9\weights\last.pt",
                Seed = 3,
                WorkingDirectory = @"C:\YOLO\training"
            };

            string? json = Core.Convert.ToSystem_String(yOLOTrainingOptions);
            Assert.False(string.IsNullOrWhiteSpace(json));

            Classes.YOLOTrainingOptions? yOLOTrainingOptions_Actual = Core.Convert.ToDiGi<Classes.YOLOTrainingOptions>(json)?.FirstOrDefault();
            Assert.NotNull(yOLOTrainingOptions_Actual);
            Assert.False(yOLOTrainingOptions_Actual!.Amp);
            Assert.Equal(8, yOLOTrainingOptions_Actual.Batch);
            Assert.Equal("train9_fresh", yOLOTrainingOptions_Actual.Name);
            Assert.Equal(75, yOLOTrainingOptions_Actual.Patience);
            Assert.Equal(3, yOLOTrainingOptions_Actual.Seed);
            Assert.Equal(@"C:\YOLO\runs\detect\train9\weights\last.pt", yOLOTrainingOptions_Actual.ResumePath);

            Core.xUnit.Query.SerializationCheck(yOLOTrainingOptions);
        }

        /// <summary>
        /// Verifies that <see cref="Create.YOLOTrainingOptions(string?, string?, string?, string?)"/> rejects a combination that cannot make a run - a missing conf.yaml, a missing start file, a start file that is neither .pt nor .yaml - and returns absolute paths and an absolute project folder otherwise.
        /// </summary>
        [Fact]
        public void YOLOTrainingOptions_Create()
        {
            string directory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Test_" + Path.GetRandomFileName());

            try
            {
                Directory.CreateDirectory(directory);

                string path_Checkpoint = Path.Combine(directory, "model.pt");
                string path_Definition = Path.Combine(directory, "yolo26x.yaml");
                string path_Other = Path.Combine(directory, "model.onnx");
                string path_Configuration = Path.Combine(directory, "conf.yaml");
                File.WriteAllText(path_Checkpoint, "dummy");
                File.WriteAllText(path_Definition, "dummy");
                File.WriteAllText(path_Other, "dummy");
                File.WriteAllText(path_Configuration, "path: .");

                string? pythonPath = Query.PythonPaths().FirstOrDefault();
                if (string.IsNullOrWhiteSpace(pythonPath))
                {
                    return;
                }

                Assert.Null(Create.YOLOTrainingOptions(pythonPath, path_Checkpoint, Path.Combine(directory, "missing.yaml")));
                Assert.Null(Create.YOLOTrainingOptions(pythonPath, Path.Combine(directory, "missing.pt"), path_Configuration));
                Assert.Null(Create.YOLOTrainingOptions(pythonPath, path_Other, path_Configuration));
                Assert.Null(Create.YOLOTrainingOptions(pythonPath, null, path_Configuration));

                Assert.NotNull(Create.YOLOTrainingOptions(pythonPath, path_Definition, path_Configuration));

                string directory_Current = Directory.GetCurrentDirectory();
                try
                {
                    Directory.SetCurrentDirectory(directory);

                    Classes.YOLOTrainingOptions? yOLOTrainingOptions = Create.YOLOTrainingOptions(pythonPath, "model.pt", "conf.yaml");
                    Assert.NotNull(yOLOTrainingOptions);
                    Assert.Equal(path_Checkpoint, yOLOTrainingOptions!.ModelPath);
                    Assert.Equal(path_Configuration, yOLOTrainingOptions.ConfigurationFilePath);
                    Assert.Equal(Query.NormalizedPath(directory), yOLOTrainingOptions.WorkingDirectory);
                    Assert.Equal(Path.Combine(Query.NormalizedPath(directory)!, "runs", "detect"), yOLOTrainingOptions.Project);
                    Assert.True(Path.IsPathRooted(yOLOTrainingOptions.PythonPath));
                }
                finally
                {
                    Directory.SetCurrentDirectory(directory_Current);
                }
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
