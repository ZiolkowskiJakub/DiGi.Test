using DiGi.GIS.YOLO.UI.Classes;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json.Nodes;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the committed options template names exactly the members the options class declares, and that its write steps are off.
        /// <para>The serializer matches members by name, so a key the class does not declare is dropped in silence and the member keeps its default. A template naming a flag that no longer exists therefore reads as a flag the operator has turned off while the run has it on - which, for the steps that write deployed data, is the worst direction for that mistake to go.</para>
        /// <para>The template is also the only documentation of the option set an operator sees, so a member missing from it is a member nobody knows to set.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictionPipelineOptions_Template()
        {
            string? directory_Files = Core.xUnit.Query.FilesDirectory(Assembly.GetExecutingAssembly());
            Assert.False(string.IsNullOrWhiteSpace(directory_Files));

            //DiGi.Test/files -> DiGi.Test -> the workspace root every HintPath in this project already assumes
            DirectoryInfo? directoryInfo_Workspace = Directory.GetParent(directory_Files!)?.Parent;
            Assert.NotNull(directoryInfo_Workspace);

            string path_Template = Path.Combine(directoryInfo_Workspace!.FullName, "DiGi.GIS.YOLO.UI", "files", $"{Constants.FileName.YearBuiltPredictionPipelineOptions}.template");
            Assert.True(File.Exists(path_Template), $"The committed options template was not found at '{path_Template}'.");

            JsonObject? jsonObject = JsonNode.Parse(File.ReadAllText(path_Template)) as JsonObject;
            Assert.NotNull(jsonObject);

            List<string> names_Template = [];
            foreach (KeyValuePair<string, JsonNode?> keyValuePair in jsonObject!)
            {
                names_Template.Add(keyValuePair.Key);
            }

            List<string> names_Member = [];
            foreach (PropertyInfo propertyInfo in typeof(Classes.YearBuiltPredictionPipelineOptions).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (propertyInfo.CanRead && propertyInfo.CanWrite)
                {
                    names_Member.Add(propertyInfo.Name);
                }
            }

            Assert.NotEmpty(names_Member);

            foreach (string name in names_Template)
            {
                Assert.True(names_Member.Contains(name), $"The options template names '{name}', which is not a member of {nameof(Classes.YearBuiltPredictionPipelineOptions)} and is therefore dropped in silence.");
            }

            foreach (string name in names_Member)
            {
                Assert.True(names_Template.Contains(name), $"{nameof(Classes.YearBuiltPredictionPipelineOptions)} declares '{name}', which the options template does not name.");
            }

            //The template is what a first run is copied from, so its write steps are off: the pipeline writes
            //deployed data and the master plan keeps each of those behind an explicit opt-in
            Classes.YearBuiltPredictionPipelineOptions? yearBuiltPredictionPipelineOptions = Query.YearBuiltPredictionPipelineOptions(path_Template);
            Assert.NotNull(yearBuiltPredictionPipelineOptions);
            Assert.False(yearBuiltPredictionPipelineOptions!.UpdateDetections);
            Assert.False(yearBuiltPredictionPipelineOptions.UpdateYearBuiltData);
            Assert.False(yearBuiltPredictionPipelineOptions.UpdatePredictedYearBuilt);

            //And it names no county, so a copied template cannot run against one nobody chose
            Assert.True(yearBuiltPredictionPipelineOptions.CountyIds is null || yearBuiltPredictionPipelineOptions.CountyIds.Count == 0);
        }
    }
}
