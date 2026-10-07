using DiGi.GIS.PostgreSQL.UI.Classes;
using System;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies <see cref="UIYOLOTrainingTask.RenderRefusal"/> turns a Serilog message template and its positional values into the exact text the task row shows, the same text Serilog writes to the log: the issue's live run-name refusal fills its {Path} slot, a value-less refusal names only the field, and a template with no property tokens returns itself.
        /// </summary>
        [Fact]
        public void UIYOLOTrainingTask_RenderRefusal()
        {
            string runName = @"D:\YOLO\runs\train9_continue_smoke";

            Assert.Equal(
                "YOLO training refused - RunName: the run " + runName + " already exists; choose a new run name",
                UIYOLOTrainingTask.RenderRefusal(
                    "YOLO training refused - {Name}: the run {Path} already exists; choose a new run name",
                    "RunName", runName));

            Assert.Equal(
                "YOLO training refused - WeightsPaths: the evaluation without training needs at least one gate weights file",
                UIYOLOTrainingTask.RenderRefusal(
                    "YOLO training refused - {Name}: the evaluation without training needs at least one gate weights file",
                    "WeightsPaths"));

            Assert.Equal(
                "No WPF application is running - the YOLO training options cannot be asked for",
                UIYOLOTrainingTask.RenderRefusal("No WPF application is running - the YOLO training options cannot be asked for"));
        }
    }
}
