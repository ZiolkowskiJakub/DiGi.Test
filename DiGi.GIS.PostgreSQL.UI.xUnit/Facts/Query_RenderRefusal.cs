using System;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies <see cref="Query.RenderRefusal"/> turns a Serilog message template and its positional values into the exact text the task row shows, the same text Serilog writes to the log, for both tasks that share it: the YOLO training run-name refusal fills its {Path} slot, the value-less refusal names only the field, the Year Built runner refusal fills {FileName} with the very constant its log line is logged with, the detector refusal carries the joined {Messages}, and a template with no property tokens returns itself.
        /// </summary>
        [Fact]
        public void Query_RenderRefusal()
        {
            string runName = @"D:\YOLO\runs\train9_continue_smoke";

            Assert.Equal(
                "YOLO training refused - RunName: the run " + runName + " already exists; choose a new run name",
                Query.RenderRefusal(
                    "YOLO training refused - {Name}: the run {Path} already exists; choose a new run name",
                    "RunName", runName));

            Assert.Equal(
                "YOLO training refused - WeightsPaths: the evaluation without training needs at least one gate weights file",
                Query.RenderRefusal(
                    "YOLO training refused - {Name}: the evaluation without training needs at least one gate weights file",
                    "WeightsPaths"));

            Assert.Equal(
                Constants.FileName.YearBuiltPredictionConsoleApp + " was not found beside this application or in the workspace - the Year Built prediction run cannot be started",
                Query.RenderRefusal(
                    "{FileName} was not found beside this application or in the workspace - the Year Built prediction run cannot be started",
                    Constants.FileName.YearBuiltPredictionConsoleApp));

            Assert.Equal(
                "This machine cannot run the detector - no CPython carrying ultralytics was found",
                Query.RenderRefusal(
                    "This machine cannot run the detector - {Messages}",
                    "no CPython carrying ultralytics was found"));

            Assert.Equal(
                "No WPF application is running - the Year Built prediction options cannot be asked for",
                Query.RenderRefusal("No WPF application is running - the Year Built prediction options cannot be asked for"));
        }
    }
}
