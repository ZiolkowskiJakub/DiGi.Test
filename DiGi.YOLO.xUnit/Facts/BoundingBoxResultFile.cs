using System.Globalization;
using System.IO;
using System.Reflection;

namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Create.BoundingBoxResultFile(string?)"/> reads a bounding box result file written by predict.py, keeping the detections and skipping the lines that record an image with none.
        /// <para>Read under a comma decimal culture, because the file holds Python floats and is invariant by construction wherever it is read.</para>
        /// </summary>
        [Fact]
        public void BoundingBoxResultFile()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "YOLO_Prediction.bbrf");
            Assert.False(string.IsNullOrWhiteSpace(path));
            Assert.True(File.Exists(path));

            CultureInfo cultureInfo = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("pl-PL");

                Classes.BoundingBoxResultFile? boundingBoxResultFile = Create.BoundingBoxResultFile(path);

                Assert.NotNull(boundingBoxResultFile);

                //Four lines, one of which records an image with no detections
                Assert.Equal(3, boundingBoxResultFile!.Count);

                Assert.Equal("0207_2021", boundingBoxResultFile[0].Name);
                Assert.Equal(0, boundingBoxResultFile[0].LabelIndex);
                Assert.Equal(1043.2799072265625, boundingBoxResultFile[0].X);
                Assert.Equal(0.9153577089309692, boundingBoxResultFile[0].Confidence);
                Assert.Equal(1, boundingBoxResultFile[1].LabelIndex);
                Assert.Equal("0209_2021", boundingBoxResultFile[2].Name);
            }
            finally
            {
                CultureInfo.CurrentCulture = cultureInfo;
            }

            Assert.Null(Create.BoundingBoxResultFile(Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Test_" + Path.GetRandomFileName())));
        }

        /// <summary>
        /// Verifies that <see cref="Classes.BoundingBoxResultFile.ToString()"/> renders bounding box results from the collection rather than returning an empty string.
        /// </summary>
        [Fact]
        public void BoundingBoxResultFile_ToString()
        {
            Classes.BoundingBoxResultFile file = [new Classes.BoundingBoxResult("img1", 0, 10.0, 20.0, 30.0, 40.0, 0.9)];
            string? result = file.ToString();

            Assert.False(string.IsNullOrWhiteSpace(result));
            Assert.Contains("img1\t0\t10\t20\t30\t40\t0.9", result);
        }
    }
}
