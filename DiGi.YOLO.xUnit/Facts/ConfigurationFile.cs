using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.ConfigurationFile.ToString()"/> formats configuration parameters correctly and can be parsed back by <see cref="Create.ConfigurationFile(string?)"/>.
        /// </summary>
        [Fact]
        public void ConfigurationFile()
        {
            List<Classes.Label> labels = [new Classes.Label(0, "building"), new Classes.Label(1, "roof")];
            Classes.ConfigurationFile configurationFile = new(@"C:\YOLO\dataset", "images/train", "images/val", "images/test", labels);

            string? formatted = configurationFile.ToString();
            Assert.False(string.IsNullOrWhiteSpace(formatted));
            Assert.Contains(@"path: C:/YOLO/dataset", formatted);
            Assert.Contains("train: images/train", formatted);
            Assert.Contains("val: images/val", formatted);
            Assert.Contains("test: images/test", formatted);
            Assert.Contains("names:", formatted);
            Assert.Contains("0: building", formatted);
            Assert.Contains("1: roof", formatted);

            string tempPath = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempPath, formatted);

                Classes.ConfigurationFile? parsed = Create.ConfigurationFile(tempPath);
                Assert.NotNull(parsed);
                Assert.Equal(@"C:\YOLO\dataset", parsed!.Directory);
                Assert.Equal(@"images\train", parsed.GetDirectoryNames(Enums.Category.Train));
                Assert.Equal(@"images\val", parsed.GetDirectoryNames(Enums.Category.Validate));
                Assert.Equal(@"images\test", parsed.GetDirectoryNames(Enums.Category.Test));

                Assert.Equal(Path.Combine(@"C:\YOLO\dataset", @"images\train"), parsed.GetDirectory(Enums.Category.Train));
                Assert.Contains(Enums.Category.Train, parsed.GetCategories());

                List<Classes.Label> parsedLabels = [.. parsed.Labels];
                Assert.Equal(2, parsedLabels.Count);
                Assert.Equal(0, parsedLabels[0].Index);
                Assert.Equal("building", parsedLabels[0].Name);
                Assert.Equal(1, parsedLabels[1].Index);
                Assert.Equal("roof", parsedLabels[1].Name);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }

            Assert.Null(Create.ConfigurationFile(null));
            Assert.Null(Create.ConfigurationFile(string.Empty));
            Assert.Null(Create.ConfigurationFile(Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Test_" + Path.GetRandomFileName())));
        }
    }
}
