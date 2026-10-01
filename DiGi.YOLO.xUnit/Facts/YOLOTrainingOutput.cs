using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.YOLOTrainingOutput(IEnumerable{string}?)"/> reads the success block train.py prints last - under a pl-PL culture too, whose decimal and group separators must not leak into the parse - and ignores settings notices and training progress around it.
        /// </summary>
        [Fact]
        public void YOLOTrainingOutput()
        {
            List<string> standardOutput =
            [
                "Creating new Ultralytics Settings v0.0.8 file",
                @"Start model: C:\YOLO\models\base\yolo26x.pt",
                "Start model SHA256: 9fdd44a31c504547ffb81d2c6d9e6dac3493c8eaa8b0398d3f43bae6c7003e92",
                "Start model kind: checkpoint",
                "      1/3      8.1G      2.242      10.69          6        640: 100%",
                @"Weights: C:\YOLO\runs\detect\train9 fresh\weights\best.pt",
                "Bytes: 118311525",
                "SHA256: A9D76B442833F35A6BD38BDD4937A40209E40ECBAE32121088F2E26ECA10DEB5",
                "AMP: True",
                ""
            ];

            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            try
            {
                foreach (string name in new string[] { "en-US", "pl-PL" })
                {
                    CultureInfo.CurrentCulture = new CultureInfo(name);

                    (string? weightsPath, long? bytes, string? sHA256, bool? amp, int? resumedFromEpoch, int? resumedEpochs) = Query.YOLOTrainingOutput(standardOutput);
                    Assert.Equal(@"C:\YOLO\runs\detect\train9 fresh\weights\best.pt", weightsPath);
                    Assert.Equal(118311525, bytes);
                    Assert.Equal("a9d76b442833f35a6bd38bdd4937a40209e40ecbae32121088f2e26eca10deb5", sHA256);
                    Assert.True(amp);
                    Assert.Null(resumedFromEpoch);
                    Assert.Null(resumedEpochs);
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = cultureInfo;
            }

            //A run that failed prints no success block; the start model SHA256 line must not be read as the output digest
            (string? weightsPath_Failed, long? bytes_Failed, string? sHA256_Failed, bool? amp_Failed, int? resumedFromEpoch_Failed, int? resumedEpochs_Failed) = Query.YOLOTrainingOutput(standardOutput.GetRange(0, 5));
            Assert.Null(weightsPath_Failed);
            Assert.Null(bytes_Failed);
            Assert.Null(sHA256_Failed);
            Assert.Null(amp_Failed);
            Assert.Null(resumedFromEpoch_Failed);
            Assert.Null(resumedEpochs_Failed);

            //A resumed run repeats its resume lines in the success block; "Resume epoch:" must not swallow the plural "Resume epochs:"
            List<string> standardOutput_Resume =
            [
                @"Start model: C:\YOLO\runs\detect\train9\weights\last.pt",
                "Start model kind: resume",
                "Resume epoch: 2",
                "Resume epochs: 6",
                @"Weights: C:\YOLO\runs\detect\train9\weights\best.pt",
                "Bytes: 118311525",
                "SHA256: a9d76b442833f35a6bd38bdd4937a40209e40ecbae32121088f2e26eca10deb5",
                "AMP: True",
                "Resume epoch: 2",
                "Resume epochs: 6"
            ];

            (_, _, _, _, int? resumedFromEpoch_Resume, int? resumedEpochs_Resume) = Query.YOLOTrainingOutput(standardOutput_Resume);
            Assert.Equal(2, resumedFromEpoch_Resume);
            Assert.Equal(6, resumedEpochs_Resume);

            //A grouped or truncated number is rejected rather than misread
            (_, long? bytes_Grouped, string? sHA256_Short, _, _, _) = Query.YOLOTrainingOutput(["Bytes: 118 311 525", "SHA256: a9d7"]);
            Assert.Null(bytes_Grouped);
            Assert.Null(sHA256_Short);

            Assert.Null(Query.YOLOTrainingOutput(null).WeightsPath);
        }

        /// <summary>
        /// Verifies that <see cref="Query.YOLOValidationOutput(IEnumerable{string}?)"/> reads the mAP lines val.py prints - under a pl-PL culture too - keeps "mAP50:" and "mAP50-95:" apart, and rejects values outside [0, 1].
        /// </summary>
        [Fact]
        public void YOLOValidationOutput()
        {
            List<string> standardOutput =
            [
                "                   all         40         41      0.912      0.878      0.938      0.696",
                "mAP50: 0.9385",
                "mAP50-95: 0.69585"
            ];

            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            try
            {
                foreach (string name in new string[] { "en-US", "pl-PL" })
                {
                    CultureInfo.CurrentCulture = new CultureInfo(name);

                    (double? mAP50, double? mAP50_95) = Query.YOLOValidationOutput(standardOutput);
                    Assert.Equal(0.9385, mAP50);
                    Assert.Equal(0.69585, mAP50_95);
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = cultureInfo;
            }

            (double? mAP50_Comma, double? mAP50_95_Range) = Query.YOLOValidationOutput(["mAP50: 0,9385", "mAP50-95: 1.5"]);
            Assert.Null(mAP50_Comma);
            Assert.Null(mAP50_95_Range);

            (double? mAP50_Scientific, _) = Query.YOLOValidationOutput(["mAP50: 5e-05"]);
            Assert.Equal(5e-05, mAP50_Scientific);
        }

        /// <summary>
        /// Verifies that <see cref="Query.FileSHA256(string?)"/> answers the lowercase hexadecimal digest train.py and export.py print, and <c>null</c> for a missing file.
        /// </summary>
        [Fact]
        public void FileSHA256()
        {
            string path = Path.GetTempFileName();
            try
            {
                File.WriteAllText(path, "abc");
                Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", Query.FileSHA256(path));
            }
            finally
            {
                File.Delete(path);
            }

            Assert.Null(Query.FileSHA256(path));
            Assert.Null(Query.FileSHA256(null));
        }
    }
}
