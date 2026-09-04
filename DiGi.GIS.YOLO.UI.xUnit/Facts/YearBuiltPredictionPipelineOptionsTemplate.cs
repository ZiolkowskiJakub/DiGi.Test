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
        /// Verifies that every committed options template names exactly the members the options class declares, and that the combined template's write steps are off.
        /// <para>The serializer matches members by name, so a key the class does not declare is dropped in silence and the member keeps its default. A template naming a flag that no longer exists therefore reads as a flag the operator has turned off while the run has it on - which, for the steps that write deployed data, is the worst direction for that mistake to go.</para>
        /// <para>The template is also the only documentation of the option set an operator sees, so a member missing from it is a member nobody knows to set.</para>
        /// <para>All three committed templates are checked, not just the combined one: the split pair is what the manual detections-then-score recovery workflow is copied from, and a key missing from one of those is dropped in exactly the same silence.</para>
        /// </summary>
        [Fact]
        public void YearBuiltPredictionPipelineOptions_Template()
        {
            string? directory_Files = Core.xUnit.Query.FilesDirectory(Assembly.GetExecutingAssembly());
            Assert.False(string.IsNullOrWhiteSpace(directory_Files));

            //DiGi.Test/files -> DiGi.Test -> the workspace root every HintPath in this project already assumes
            DirectoryInfo? directoryInfo_Workspace = Directory.GetParent(directory_Files!)?.Parent;
            Assert.NotNull(directoryInfo_Workspace);

            string directory_Templates = Path.Combine(directoryInfo_Workspace!.FullName, "DiGi.GIS.YOLO.UI", "files");

            string path_Template = Path.Combine(directory_Templates, $"{Constants.FileName.YearBuiltPredictionPipelineOptions}.template");

            //The combined template first, then the split pair the manual recovery workflow is copied from. Named
            //rather than globbed, so a template that stops being committed fails here instead of quietly not being checked.
            List<string> paths_Template =
            [
                path_Template,
                Path.Combine(directory_Templates, "YearBuiltPredictionPipelineOptions.Detections.json.template"),
                Path.Combine(directory_Templates, "YearBuiltPredictionPipelineOptions.Score.json.template")
            ];

            List<string> names_Member = [];
            foreach (PropertyInfo propertyInfo in typeof(Classes.YearBuiltPredictionPipelineOptions).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (propertyInfo.CanRead && propertyInfo.CanWrite)
                {
                    names_Member.Add(propertyInfo.Name);
                }
            }

            Assert.NotEmpty(names_Member);

            foreach (string path_Template_Temp in paths_Template)
            {
                Assert.True(File.Exists(path_Template_Temp), $"A committed options template was not found at '{path_Template_Temp}'.");

                JsonObject? jsonObject = JsonNode.Parse(File.ReadAllText(path_Template_Temp)) as JsonObject;
                Assert.NotNull(jsonObject);

                List<string> names_Template = [];
                foreach (KeyValuePair<string, JsonNode?> keyValuePair in jsonObject!)
                {
                    names_Template.Add(keyValuePair.Key);
                }

                foreach (string name in names_Template)
                {
                    Assert.True(names_Member.Contains(name), $"'{Path.GetFileName(path_Template_Temp)}' names '{name}', which is not a member of {nameof(Classes.YearBuiltPredictionPipelineOptions)} and is therefore dropped in silence.");
                }

                foreach (string name in names_Member)
                {
                    Assert.True(names_Template.Contains(name), $"{nameof(Classes.YearBuiltPredictionPipelineOptions)} declares '{name}', which '{Path.GetFileName(path_Template_Temp)}' does not name.");
                }
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

            //The class default and the template must agree on the write steps, in the safe direction: a member the
            //file does not name keeps the class default, so an options file that omits a write flag reads it back as
            //the class default - and that default must be off, the same as the template it is copied from.
            Classes.YearBuiltPredictionPipelineOptions yearBuiltPredictionPipelineOptions_Defaults = new();
            Assert.False(yearBuiltPredictionPipelineOptions_Defaults.UpdateDetections);
            Assert.Equal(yearBuiltPredictionPipelineOptions_Defaults.UpdateDetections, yearBuiltPredictionPipelineOptions.UpdateDetections);
            Assert.False(yearBuiltPredictionPipelineOptions_Defaults.UpdateYearBuiltData);
            Assert.Equal(yearBuiltPredictionPipelineOptions_Defaults.UpdateYearBuiltData, yearBuiltPredictionPipelineOptions.UpdateYearBuiltData);
            Assert.False(yearBuiltPredictionPipelineOptions_Defaults.UpdatePredictedYearBuilt);
            Assert.Equal(yearBuiltPredictionPipelineOptions_Defaults.UpdatePredictedYearBuilt, yearBuiltPredictionPipelineOptions.UpdatePredictedYearBuilt);

            //The combined run cleans up after itself, because nothing downstream of it reads what it wrote to disk
            Assert.True(yearBuiltPredictionPipelineOptions.CleanScratchDirectory);

            //The split pair must not. Its second run rebuilds its building list from the first run's results file,
            //so a detections pass that cleaned up would leave the scoring pass nothing to score - and a county whose
            //detections are already stored would then be skipped reporting a legitimate looking zero.
            foreach (string name_Template in new string[] { "YearBuiltPredictionPipelineOptions.Detections.json.template", "YearBuiltPredictionPipelineOptions.Score.json.template" })
            {
                Classes.YearBuiltPredictionPipelineOptions? yearBuiltPredictionPipelineOptions_Split = Query.YearBuiltPredictionPipelineOptions(Path.Combine(directory_Templates, name_Template));
                Assert.NotNull(yearBuiltPredictionPipelineOptions_Split);
                Assert.False(yearBuiltPredictionPipelineOptions_Split!.CleanScratchDirectory, $"'{name_Template}' must set CleanScratchDirectory to false - the split workflow depends on the scratch directory outliving the run that filled it.");
            }
        }
    }
}
