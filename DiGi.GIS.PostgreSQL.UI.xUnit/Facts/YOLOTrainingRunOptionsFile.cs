using System;
using System.IO;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a tray training run's options file never overwrites an earlier one (ZiolkowskiJakub/DiGi.GIS.PostgreSQL.UI#20).
        /// <para>The file is the record of what a run was asked to do. A validation-only or evaluation-only run started with the previous run name still in the dialog used to write <c>&lt;RunName&gt;.YOLOTrainingRunOptions.json</c> again and replace the training run's record without a word. A resume writes <c>&lt;RunName&gt;.resume-&lt;timestamp&gt;.YOLOTrainingRunOptions.json</c> for the same reason, leaving the original run's file byte-identical.</para>
        /// </summary>
        [Fact]
        public void YOLOTrainingRunOptionsFile()
        {
            string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            DateTimeOffset dateTimeOffset = new(2026, 9, 30, 14, 5, 7, TimeSpan.Zero);

            try
            {
                Directory.CreateDirectory(directory);

                // A training run writes under its run name.
                string? path_Train = Create.YOLOTrainingRunOptionsFile(directory, "run1", true, "train", dateTimeOffset);
                Assert.Equal(Path.Combine(directory, "run1" + Constants.FileName.YOLOTrainingRunOptionsSuffix), path_Train);
                Assert.Equal("train", File.ReadAllText(path_Train!));

                // A run without training that carries the same name leaves that file byte-identical and writes a timestamped one.
                byte[] bytes = File.ReadAllBytes(path_Train!);
                string? path_Evaluate = Create.YOLOTrainingRunOptionsFile(directory, "run1", false, "evaluate", dateTimeOffset);
                Assert.Equal(Path.Combine(directory, "20260930_140507" + Constants.FileName.YOLOTrainingRunOptionsSuffix), path_Evaluate);
                Assert.Equal(bytes, File.ReadAllBytes(path_Train!));
                Assert.Equal("evaluate", File.ReadAllText(path_Evaluate!));

                // A second run without training in the same second gets a suffix rather than replacing the first.
                string? path_Evaluate2 = Create.YOLOTrainingRunOptionsFile(directory, null, false, "evaluate2", dateTimeOffset);
                Assert.Equal(Path.Combine(directory, "20260930_140507_2" + Constants.FileName.YOLOTrainingRunOptionsSuffix), path_Evaluate2);
                Assert.Equal("evaluate", File.ReadAllText(path_Evaluate!));

                // A resume writes a file of its own and leaves the original run's file byte-identical.
                string? path_Resume = Create.YOLOTrainingRunOptionsFile(directory, "run1", true, "resume", dateTimeOffset, true);
                Assert.Equal(Path.Combine(directory, "run1.resume-20260930_140507" + Constants.FileName.YOLOTrainingRunOptionsSuffix), path_Resume);
                Assert.Equal(bytes, File.ReadAllBytes(path_Train!));
                Assert.Equal("resume", File.ReadAllText(path_Resume!));

                // A second resume in the same second gets a suffix rather than replacing the first.
                string? path_Resume2 = Create.YOLOTrainingRunOptionsFile(directory, "run1", true, "resume2", dateTimeOffset, true);
                Assert.Equal(Path.Combine(directory, "run1.resume-20260930_140507_2" + Constants.FileName.YOLOTrainingRunOptionsSuffix), path_Resume2);
                Assert.Equal("resume", File.ReadAllText(path_Resume!));

                // A training run whose file exists anyway is refused rather than written under another name.
                Assert.Null(Create.YOLOTrainingRunOptionsFile(directory, "run1", true, "again", dateTimeOffset));
                Assert.Equal(bytes, File.ReadAllBytes(path_Train!));
                Assert.Equal(5, Directory.GetFiles(directory).Length);
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
