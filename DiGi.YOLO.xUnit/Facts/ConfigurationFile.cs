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
            string directory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Test_" + Path.GetRandomFileName());

            try
            {
                string directory_Dataset = Path.Combine(directory, "dataset");
                Directory.CreateDirectory(directory_Dataset);

                List<Classes.Label> labels = [new Classes.Label(0, "building"), new Classes.Label(1, "roof")];
                Classes.ConfigurationFile configurationFile = new(directory_Dataset, "images/train", "images/val", "images/test", labels);

                string? formatted = configurationFile.ToString();
                Assert.False(string.IsNullOrWhiteSpace(formatted));
                Assert.Contains(string.Concat("path: ", directory_Dataset.Replace('\\', '/')), formatted);
                Assert.Contains("train: images/train", formatted);
                Assert.Contains("val: images/val", formatted);
                Assert.Contains("test: images/test", formatted);
                Assert.Contains("names:", formatted);
                Assert.Contains("0: building", formatted);
                Assert.Contains("1: roof", formatted);

                string path = Path.Combine(directory, "conf.yaml");
                File.WriteAllText(path, formatted);

                Classes.ConfigurationFile? parsed = Create.ConfigurationFile(path);
                Assert.NotNull(parsed);
                Assert.Equal(directory_Dataset, parsed!.Directory);
                Assert.Null(parsed.Messages);
                Assert.Equal(@"images\train", parsed.GetDirectoryNames(Enums.Category.Train));
                Assert.Equal(@"images\val", parsed.GetDirectoryNames(Enums.Category.Validate));
                Assert.Equal(@"images\test", parsed.GetDirectoryNames(Enums.Category.Test));

                Assert.Equal(Path.Combine(directory_Dataset, @"images\train"), parsed.GetDirectory(Enums.Category.Train));
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
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }

            Assert.Null(Create.ConfigurationFile(null));
            Assert.Null(Create.ConfigurationFile(string.Empty));
            Assert.Null(Create.ConfigurationFile(Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Test_" + Path.GetRandomFileName())));
        }

        /// <summary>
        /// Verifies that <see cref="Create.ConfigurationFile(string?)"/> reads a hand-edited ultralytics conf.yaml without throwing: a blank line and a comment inside the label block, a trailing comment on a value, quoted names, and keys after the label block.
        /// <para>Before ZiolkowskiJakub/DiGi.YOLO#16 a blank line or any line without a colon after "names:" reached <c>Substring(0, -1)</c> and threw, and a file with no "names:" at all started the label loop at index -1 and threw.</para>
        /// </summary>
        [Fact]
        public void ConfigurationFile_Robust()
        {
            string directory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Test_" + Path.GetRandomFileName());

            try
            {
                Directory.CreateDirectory(directory);

                string path = Path.Combine(directory, "conf.yaml");
                File.WriteAllLines(path,
                [
                    "# dataset",
                    "path: .  # the directory of this file",
                    "train: images/train",
                    "val: images/val",
                    "names:",
                    "",
                    "  # the only class",
                    "  0: 'Building'",
                    "  1: roof # trailing",
                    "nc: 2",
                    "test: images/test"
                ]);

                Classes.ConfigurationFile? configurationFile = Create.ConfigurationFile(path);
                Assert.NotNull(configurationFile);
                Assert.Equal(directory, configurationFile!.Directory);
                Assert.Null(configurationFile.Messages);
                Assert.Equal(@"images\test", configurationFile.GetDirectoryNames(Enums.Category.Test));

                List<Classes.Label> labels = [.. configurationFile.Labels.OrderBy(x => x.Index)];
                Assert.Equal(2, labels.Count);
                Assert.Equal("Building", labels[0].Name);
                Assert.Equal("roof", labels[1].Name);

                //No names: at all - no labels, no exception
                File.WriteAllLines(path, ["path: .", "train: images/train", "val: images/val"]);
                configurationFile = Create.ConfigurationFile(path);
                Assert.NotNull(configurationFile);
                Assert.Empty(configurationFile!.Labels);

                //names: as the last line, and a line with no colon after it
                File.WriteAllLines(path, ["path: .", "names:", "  0: Building", "garbage line", "  1: ignored"]);
                configurationFile = Create.ConfigurationFile(path);
                Assert.NotNull(configurationFile);
                Assert.Single(configurationFile!.Labels);
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        /// <summary>
        /// Verifies that the "path:" of a conf.yaml is resolved against the directory of the file, and that a path which does not exist falls back to that directory with a message rather than yielding an empty dataset silently.
        /// <para>Also verifies that a path with spaces round-trips unencoded: ultralytics hands the value to the file system as it is, so the "%20" written before ZiolkowskiJakub/DiGi.YOLO#16 named a directory that did not exist.</para>
        /// </summary>
        [Fact]
        public void ConfigurationFile_Path()
        {
            string directory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Test_" + Path.GetRandomFileName(), "user files");

            try
            {
                string directory_Dataset = Path.Combine(directory, "training");
                Directory.CreateDirectory(directory_Dataset);

                string path = Path.Combine(directory, "conf.yaml");

                //Relative - against the file, not the current directory
                File.WriteAllLines(path, ["path: training", "names:", "  0: Building"]);
                Classes.ConfigurationFile? configurationFile = Create.ConfigurationFile(path);
                Assert.Equal(directory_Dataset, configurationFile?.Directory);
                Assert.Null(configurationFile?.Messages);

                //Missing - falls back to the directory of the file and says so
                string directory_Missing = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Missing_" + Path.GetRandomFileName());
                File.WriteAllLines(path, [string.Concat("path: ", directory_Missing.Replace('\\', '/')), "names:", "  0: Building"]);
                configurationFile = Create.ConfigurationFile(path);
                Assert.Equal(directory, configurationFile?.Directory);
                Assert.Single(configurationFile?.Messages ?? []);
                Assert.Contains(directory_Missing, configurationFile!.Messages![0]);

                //Spaces are written as they are and read back to the same directory; legacy %20 still reads
                string? formatted = new Classes.ConfigurationFile(directory_Dataset, "images/train", "images/val", null, [new Classes.Label(0, "Building")]).ToString();
                Assert.DoesNotContain("%20", formatted);
                File.WriteAllText(path, formatted);
                Assert.Equal(directory_Dataset, Create.ConfigurationFile(path)?.Directory);

                File.WriteAllLines(path, [string.Concat("path: ", directory_Dataset.Replace('\\', '/').Replace(" ", "%20"))]);
                Assert.Equal(directory_Dataset, Create.ConfigurationFile(path)?.Directory);

                //A value YAML would read as a comment is quoted and read back
                Assert.Equal("'C:/a #b'", Query.Encode(@"C:\a #b"));
                Assert.Equal(@"C:\a #b", Query.Decode("'C:/a #b'"));
            }
            finally
            {
                string? directory_Root = Path.GetDirectoryName(directory);
                if (directory_Root != null && Directory.Exists(directory_Root))
                {
                    Directory.Delete(directory_Root, true);
                }
            }
        }
    }
}
