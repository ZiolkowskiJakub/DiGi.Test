using DiGi.GIS.PostgreSQL.UI.Windows;
using System;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies <see cref="YOLOTrainingOptionsWindow.DirectoryRefusal"/> refuses a relative dataset directory with the dataset warning, a relative project directory (when training is on) with the project warning, and accepts directories that are already absolute: a relative path would otherwise be resolved against the tray's current directory, where the runner would not look for it.
        /// </summary>
        [Fact]
        public void YOLOTrainingOptionsWindow_DirectoryRefusal()
        {
            Assert.Equal(
                "The dataset directory has to be an absolute path.",
                YOLOTrainingOptionsWindow.DirectoryRefusal(@"runs\dataset", @"C:\proj", train: true));

            Assert.Equal(
                "The project directory has to be an absolute path.",
                YOLOTrainingOptionsWindow.DirectoryRefusal(@"C:\dataset", "proj", train: true));

            Assert.Null(YOLOTrainingOptionsWindow.DirectoryRefusal(@"C:\dataset", @"C:\proj", train: true));
        }
    }
}
