using System.Globalization;
using System.IO;

namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LabelFile.ToString()"/> and <see cref="Create.LabelFile(string?)"/> write and read culture invariant floating point numbers under a comma-decimal culture.
        /// </summary>
        [Fact]
        public void LabelFile_Culture()
        {
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("pl-PL");

                Classes.LabelFile labelFile = new();
                labelFile.Add(0, new Classes.BoundingBox(0.4832, 0.5117, 0.0621, 0.0744));

                string formatted = labelFile.ToString();
                Assert.Equal("0 0.4832 0.5117 0.0621 0.0744", formatted);

                string tempPath = Path.GetTempFileName();
                try
                {
                    File.WriteAllText(tempPath, formatted);

                    Classes.LabelFile? readLabelFile = Create.LabelFile(tempPath);
                    Assert.NotNull(readLabelFile);
                    Assert.Equal(1, readLabelFile!.Count);
                    Assert.Equal(0, readLabelFile.GetLabelIndex(0));
                    Assert.Equal(0.4832, readLabelFile.GetBoundingBox(0).X);
                    Assert.Equal(0.5117, readLabelFile.GetBoundingBox(0).Y);
                    Assert.Equal(0.0621, readLabelFile.GetBoundingBox(0).Width);
                    Assert.Equal(0.0744, readLabelFile.GetBoundingBox(0).Height);
                }
                finally
                {
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = cultureInfo;
            }
        }
    }
}
