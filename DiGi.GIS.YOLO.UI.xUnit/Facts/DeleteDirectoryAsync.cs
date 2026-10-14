using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Modify.DeleteDirectoryAsync(string?, int, TimeSpan?)"/> removes a folder whose file is held open only briefly, and reports rather than throws when the hold outlasts every attempt (DiGi-Organization/DiGi.GIS.YOLO.UI#30).
        /// <para>The 55417 run lost its clean-up to a monitoring tool reading <c>results.bbrf</c> at the moment the folder was removed: the images went and the results file stayed, so a re-run could not tell a half-cleaned folder from a failed one.</para>
        /// </summary>
        [Fact]
        public async Task DeleteDirectoryAsync()
        {
            string? directory_Reports = Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly());
            Assert.False(string.IsNullOrWhiteSpace(directory_Reports));

            string directory = Path.Combine(directory_Reports!, nameof(DeleteDirectoryAsync));

            string Setup()
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }

                Directory.CreateDirectory(Path.Combine(directory, "images"));
                File.WriteAllBytes(Path.Combine(directory, "images", "0207_2021.jpeg"), [1, 2, 3]);

                string path_Results = Path.Combine(directory, Constants.FileName.PredictionResults);
                File.WriteAllText(path_Results, "0207_2021\n");

                return path_Results;
            }

            //Held for less than the back-off: the second attempt removes what the first one could not
            string path_Results = Setup();
            FileStream fileStream_Brief = new(path_Results, FileMode.Open, FileAccess.Read, FileShare.None);
            Task task_Release = Task.Run(async () =>
            {
                await Task.Delay(100);
                fileStream_Brief.Dispose();
            });

            Assert.True(await Modify.DeleteDirectoryAsync(directory, 3, TimeSpan.FromMilliseconds(300)));
            await task_Release;
            Assert.False(Directory.Exists(directory));

            //Held throughout: reported, not thrown, and what is left stays
            path_Results = Setup();
            using (FileStream fileStream_Held = new(path_Results, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                Assert.False(await Modify.DeleteDirectoryAsync(directory, 2, TimeSpan.FromMilliseconds(50)));
                Assert.True(File.Exists(path_Results));
            }

            Assert.True(await Modify.DeleteDirectoryAsync(directory));
            Assert.False(Directory.Exists(directory));

            //A folder that is not there is already deleted; a blank path is not a folder
            Assert.True(await Modify.DeleteDirectoryAsync(directory));
            Assert.False(await Modify.DeleteDirectoryAsync(" "));
        }
    }
}
