using DiGi.GIS.YOLO.UI.Enums;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that YOLOTrainingRunOptions keeps every value it is given, survives its string form, clones identically, that its copy constructor copies every member and owns its collections, and that the defaults are the hyper-parameters of the DiGi.YOLO README.
        /// <para>Every member is set to something other than its default, the nested dataset options and the step list included, because <c>SerializationCheck</c> never runs the copy constructor and passes for a member the copy constructor forgot unless the instance populated it.</para>
        /// </summary>
        [Fact]
        public void YOLOTrainingRunOptions()
        {
            Classes.YOLOTrainingRunOptions yOLOTrainingRunOptions = new()
            {
                Batch = 8,
                DatasetOptions = new Classes.YOLOTrainingDatasetOptions() { CountyIds = [73485], OutputDirectory = @"C:\YOLO\dataset", Seed = 3 },
                Device = "0",
                Epochs = 3,
                ImageSize = 320,
                Patience = 7,
                ProjectDirectory = @"C:\YOLO\runs",
                PythonPath = @"C:\Python\python.exe",
                ResumeTraining = true,
                RunName = "train9_fresh",
                Seed = 42,
                StartWeightsPath = @"C:\YOLO\models\base\yolo26x.pt",
                Steps = [YOLOTrainingStep.Train, YOLOTrainingStep.Validate],
                WorkingDirectory = @"C:\YOLO\work"
            };

            void AssertMembers(Classes.YOLOTrainingRunOptions? yOLOTrainingRunOptions_Actual)
            {
                Assert.NotNull(yOLOTrainingRunOptions_Actual);
                Assert.Equal(yOLOTrainingRunOptions.Batch, yOLOTrainingRunOptions_Actual!.Batch);
                Assert.NotNull(yOLOTrainingRunOptions_Actual.DatasetOptions);
                Assert.Equal(yOLOTrainingRunOptions.DatasetOptions!.OutputDirectory, yOLOTrainingRunOptions_Actual.DatasetOptions!.OutputDirectory);
                Assert.Equal(yOLOTrainingRunOptions.DatasetOptions.Seed, yOLOTrainingRunOptions_Actual.DatasetOptions.Seed);
                Assert.Equal(yOLOTrainingRunOptions.DatasetOptions.CountyIds, yOLOTrainingRunOptions_Actual.DatasetOptions.CountyIds);
                Assert.Equal(yOLOTrainingRunOptions.Device, yOLOTrainingRunOptions_Actual.Device);
                Assert.Equal(yOLOTrainingRunOptions.Epochs, yOLOTrainingRunOptions_Actual.Epochs);
                Assert.Equal(yOLOTrainingRunOptions.ImageSize, yOLOTrainingRunOptions_Actual.ImageSize);
                Assert.Equal(yOLOTrainingRunOptions.Patience, yOLOTrainingRunOptions_Actual.Patience);
                Assert.Equal(yOLOTrainingRunOptions.ProjectDirectory, yOLOTrainingRunOptions_Actual.ProjectDirectory);
                Assert.Equal(yOLOTrainingRunOptions.PythonPath, yOLOTrainingRunOptions_Actual.PythonPath);
                Assert.Equal(yOLOTrainingRunOptions.ResumeTraining, yOLOTrainingRunOptions_Actual.ResumeTraining);
                Assert.Equal(yOLOTrainingRunOptions.RunName, yOLOTrainingRunOptions_Actual.RunName);
                Assert.Equal(yOLOTrainingRunOptions.Seed, yOLOTrainingRunOptions_Actual.Seed);
                Assert.Equal(yOLOTrainingRunOptions.StartWeightsPath, yOLOTrainingRunOptions_Actual.StartWeightsPath);
                Assert.Equal(yOLOTrainingRunOptions.Steps, yOLOTrainingRunOptions_Actual.Steps);
                Assert.Equal(yOLOTrainingRunOptions.WorkingDirectory, yOLOTrainingRunOptions_Actual.WorkingDirectory);
            }

            string? json = Core.Convert.ToSystem_String(yOLOTrainingRunOptions);
            Assert.False(string.IsNullOrWhiteSpace(json));

            AssertMembers(Core.Convert.ToDiGi<Classes.YOLOTrainingRunOptions>(json)?.FirstOrDefault());

            Classes.YOLOTrainingRunOptions yOLOTrainingRunOptions_Copy = new(yOLOTrainingRunOptions);
            AssertMembers(yOLOTrainingRunOptions_Copy);

            // The copy owns its nested options and its list.
            yOLOTrainingRunOptions_Copy.Steps!.Add(YOLOTrainingStep.Evaluate);
            yOLOTrainingRunOptions_Copy.DatasetOptions!.Seed = 99;
            Assert.Equal(2, yOLOTrainingRunOptions.Steps!.Count);
            Assert.Equal(3, yOLOTrainingRunOptions.DatasetOptions!.Seed);

            Core.xUnit.Query.SerializationCheck(yOLOTrainingRunOptions);

            // The defaults are the hyper-parameters of the DiGi.YOLO README, and nothing that names a file or a folder has one.
            Classes.YOLOTrainingRunOptions yOLOTrainingRunOptions_Default = new();
            Assert.Equal(150, yOLOTrainingRunOptions_Default.Epochs);
            Assert.Equal(50, yOLOTrainingRunOptions_Default.Patience);
            Assert.Equal(640, yOLOTrainingRunOptions_Default.ImageSize);
            Assert.Equal(16, yOLOTrainingRunOptions_Default.Batch);
            Assert.Equal(0, yOLOTrainingRunOptions_Default.Seed);
            Assert.False(yOLOTrainingRunOptions_Default.ResumeTraining);
            Assert.Null(yOLOTrainingRunOptions_Default.StartWeightsPath);
            Assert.Null(yOLOTrainingRunOptions_Default.RunName);
            Assert.Null(yOLOTrainingRunOptions_Default.ProjectDirectory);
            Assert.Null(yOLOTrainingRunOptions_Default.Steps);
        }

        /// <summary>
        /// Verifies that the committed YOLOTrainingRunOptions template names exactly the members the options class declares, reads back, names no county, and runs only the counting dataset step.
        /// <para>A key the class does not declare is dropped in silence, so a misspelt template member reads as the default. A first run of the template must not train anything.</para>
        /// </summary>
        [Fact]
        public void YOLOTrainingRunOptions_Template()
        {
            string? directory_Files = Core.xUnit.Query.FilesDirectory(Assembly.GetExecutingAssembly());
            Assert.False(string.IsNullOrWhiteSpace(directory_Files));

            DirectoryInfo? directoryInfo_Workspace = Directory.GetParent(directory_Files!)?.Parent;
            Assert.NotNull(directoryInfo_Workspace);

            string path_Template = Path.Combine(directoryInfo_Workspace!.FullName, "DiGi.GIS.YOLO.UI", "files", Constants.FileName.YOLOTrainingRunOptions);
            Assert.True(File.Exists(path_Template), $"The committed options template was not found at '{path_Template}'.");

            List<string> names_Member = [];
            foreach (PropertyInfo propertyInfo in typeof(Classes.YOLOTrainingRunOptions).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (propertyInfo.CanRead && propertyInfo.CanWrite)
                {
                    names_Member.Add(propertyInfo.Name);
                }
            }

            JsonObject? jsonObject = JsonNode.Parse(File.ReadAllText(path_Template)) as JsonObject;
            Assert.NotNull(jsonObject);

            List<string> names_Template = [.. jsonObject!.Select(x => x.Key)];

            foreach (string name in names_Template)
            {
                Assert.True(names_Member.Contains(name), $"The template names '{name}', which is not a member of {nameof(Classes.YOLOTrainingRunOptions)}.");
            }

            foreach (string name in names_Member)
            {
                Assert.True(names_Template.Contains(name), $"{nameof(Classes.YOLOTrainingRunOptions)} declares '{name}', which the template does not name.");
            }

            Classes.YOLOTrainingRunOptions? yOLOTrainingRunOptions = Query.YOLOTrainingRunOptions(path_Template);
            Assert.NotNull(yOLOTrainingRunOptions);
            Assert.NotNull(yOLOTrainingRunOptions!.DatasetOptions);
            Assert.True(yOLOTrainingRunOptions.DatasetOptions!.CountOnly);
            Assert.True(yOLOTrainingRunOptions.DatasetOptions.CountyIds is null || yOLOTrainingRunOptions.DatasetOptions.CountyIds.Count == 0);
            Assert.False(yOLOTrainingRunOptions.ResumeTraining);
            Assert.Equal([YOLOTrainingStep.Dataset], yOLOTrainingRunOptions.Steps);
        }
    }
}
