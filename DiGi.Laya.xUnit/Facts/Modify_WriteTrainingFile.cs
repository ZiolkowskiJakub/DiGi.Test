using System;
using System.IO;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Modify.WriteTrainingFile(System.Collections.Generic.IEnumerable{Classes.LayaTrainingSample}?, string?)"/> writes one training line per sample the converter accepts, leaves out a sample it refuses and counts only the written ones, creates the directory, writes UTF-8 without a byte order mark and with "\n" line ends, replaces an existing file, leaves no temporary file behind, and gives <c>null</c> for <c>null</c> samples or path.
        /// </summary>
        [Fact]
        public void Modify_WriteTrainingFile()
        {
            string directory = Path.Combine(Path.GetTempPath(), string.Concat("Laya_TrainingFile_", Guid.NewGuid().ToString("N")));

            try
            {
                string path = Path.Combine(directory, "data", "training.jsonl");

                Classes.LayaNoulQuestion layaNoulQuestion = new("tags:vegan", "The recipe is vegan.");
                Classes.LayaTrainingSample[] layaTrainingSamples =
                [
                    new Classes.LayaTrainingSample("r1|tags:vegan", "Barszcz z burakami.", false, layaNoulQuestion, null, null, true, null, null),
                    new Classes.LayaTrainingSample("r2|tags:vegan", "Sernik.", false, layaNoulQuestion, null, null, null, null, null),
                    new Classes.LayaTrainingSample("r3|tags:vegan", "Żurek z kiełbasą.", false, layaNoulQuestion, null, null, false, 0.25, null)
                ];

                Assert.Equal(2, layaTrainingSamples.WriteTrainingFile(path));

                byte[] bytes = File.ReadAllBytes(path);
                Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);
                Assert.DoesNotContain((byte)'\r', bytes);

                string[] lines = File.ReadAllText(path).Split('\n');
                Assert.Equal(3, lines.Length);
                Assert.Equal(string.Empty, lines[2]);
                Assert.Equal(layaTrainingSamples[0].ToLaya_TrainingLine(), lines[0]);
                Assert.Equal(layaTrainingSamples[2].ToLaya_TrainingLine(), lines[1]);
                Assert.False(File.Exists(string.Concat(path, ".tmp")));

                //Replaces the file
                Assert.Equal(1, layaTrainingSamples.Take(1).WriteTrainingFile(path));
                Assert.Single(File.ReadAllLines(path));

                Assert.Null(((Classes.LayaTrainingSample[]?)null).WriteTrainingFile(path));
                Assert.Null(layaTrainingSamples.WriteTrainingFile(null));
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
