using System.IO;
using System.Linq;

namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.YOLOValidationOptions"/> keeps the values it is given, validates on the test split by default, survives the round trip through its string form, and clones identically.
        /// </summary>
        [Fact]
        public void YOLOValidationOptions()
        {
            Classes.YOLOValidationOptions yOLOValidationOptions_Default = new();
            Assert.Equal(Enums.Category.Test, yOLOValidationOptions_Default.Split);
            Assert.Null(yOLOValidationOptions_Default.Confidence);
            Assert.Equal(16, yOLOValidationOptions_Default.Batch);
            Assert.Equal(640, yOLOValidationOptions_Default.ImageSize);

            Classes.YOLOValidationOptions yOLOValidationOptions = new()
            {
                Batch = 4,
                Confidence = 0.25,
                ConfigurationFilePath = @"C:\YOLO\training\conf.yaml",
                Device = "cpu",
                ImageSize = 640,
                ModelPath = @"C:\YOLO\runs\detect\train9_fresh\weights\best.pt",
                PythonPath = @"C:\Python\python.exe",
                Split = Enums.Category.Validate,
                WorkingDirectory = @"C:\YOLO\training"
            };

            string? json = Core.Convert.ToSystem_String(yOLOValidationOptions);
            Classes.YOLOValidationOptions? yOLOValidationOptions_Actual = Core.Convert.ToDiGi<Classes.YOLOValidationOptions>(json)?.FirstOrDefault();
            Assert.NotNull(yOLOValidationOptions_Actual);
            Assert.Equal(Enums.Category.Validate, yOLOValidationOptions_Actual!.Split);
            Assert.Equal(0.25, yOLOValidationOptions_Actual.Confidence);
            Assert.Equal("cpu", yOLOValidationOptions_Actual.Device);

            Core.xUnit.Query.SerializationCheck(yOLOValidationOptions);
        }

        /// <summary>
        /// Verifies that <see cref="Create.YOLOValidationOptions(string?, string?, string?, string?)"/> rejects weights that are not an existing .pt checkpoint and a missing conf.yaml.
        /// </summary>
        [Fact]
        public void YOLOValidationOptions_Create()
        {
            string directory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Test_" + Path.GetRandomFileName());

            try
            {
                Directory.CreateDirectory(directory);

                string path_Checkpoint = Path.Combine(directory, "best.pt");
                string path_Definition = Path.Combine(directory, "yolo26x.yaml");
                string path_Configuration = Path.Combine(directory, "conf.yaml");
                File.WriteAllText(path_Checkpoint, "dummy");
                File.WriteAllText(path_Definition, "dummy");
                File.WriteAllText(path_Configuration, "path: .");

                string? pythonPath = Query.PythonPaths().FirstOrDefault();
                if (string.IsNullOrWhiteSpace(pythonPath))
                {
                    return;
                }

                Assert.Null(Create.YOLOValidationOptions(pythonPath, path_Definition, path_Configuration));
                Assert.Null(Create.YOLOValidationOptions(pythonPath, Path.Combine(directory, "missing.pt"), path_Configuration));
                Assert.Null(Create.YOLOValidationOptions(pythonPath, path_Checkpoint, Path.Combine(directory, "missing.yaml")));

                Classes.YOLOValidationOptions? yOLOValidationOptions = Create.YOLOValidationOptions(pythonPath, path_Checkpoint, path_Configuration);
                Assert.NotNull(yOLOValidationOptions);
                Assert.Equal(Query.NormalizedPath(directory), yOLOValidationOptions!.WorkingDirectory);
                Assert.Equal(Enums.Category.Test, yOLOValidationOptions.Split);
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
