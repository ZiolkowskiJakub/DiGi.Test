using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies calibration end to end with the real interpreter and checkpoint of DiGi.Laya_Preflight.conf: <see cref="Modify.CalibrateAsync(Classes.LayaCalibrationOptions?, System.Threading.CancellationToken)"/> fits temperatures for every question type from a labelled ticket set and writes them to a calibration file outside the checkpoint, which stays byte-for-byte unchanged; <see cref="Modify.PredictAsync(IEnumerable{Classes.LayaRequest}?, Classes.LayaPredictionOptions?, System.Threading.CancellationToken)"/> then loads the checkpoint with that file and reports its SHA-256.
        /// <para>The interpreter and checkpoint are machine specific, so the fact returns without asserting when the conf, its interpreter or its checkpoint is absent.</para>
        /// <para>Medium test (15 s on an RTX 5090, 2026-10-15): runs when DIGI_TEST_MAX_DURATION is Medium (the default) or Long.</para>
        /// </summary>
        [MediumFact]
        public async Task Modify_CalibrateAsync()
        {
            string directory = Path.Combine(Path.GetTempPath(), string.Concat("Laya_Calibrate_", Guid.NewGuid().ToString("N")));

            try
            {
                Classes.LayaPredictionOptions? layaPredictionOptions = PreflightOptions(Path.Combine(directory, "work"));
                if (layaPredictionOptions == null)
                {
                    return;
                }

                string sha256_Model = Python.Query.DirectorySHA256(layaPredictionOptions.ModelPath)!;

                (List<Classes.LayaItem> layaItems, List<Classes.LayaLabelSet> layaLabelSets, List<Classes.LayaLabelAssignment> layaLabelAssignments) = TicketData();
                string path_Labelled = Path.Combine(directory, "labelled.jsonl");
                Assert.Equal(48, Create.LayaTrainingSamples(layaItems, layaLabelSets, layaLabelAssignments, 1).WriteTrainingFile(path_Labelled));

                string path_Calibration = Path.Combine(directory, "multilingual.calibration.json");
                Classes.LayaCalibrationOptions layaCalibrationOptions = new()
                {
                    PythonPath = layaPredictionOptions.PythonPath,
                    ModelPath = layaPredictionOptions.ModelPath,
                    CalibrationFilePath = path_Labelled,
                    CalibrationPath = path_Calibration,
                    WorkingDirectory = Path.Combine(directory, "work")
                };

                Classes.LayaCalibrationResult? layaCalibrationResult = await layaCalibrationOptions.CalibrateAsync();
                Assert.NotNull(layaCalibrationResult);
                Assert.True(layaCalibrationResult!.ExitCode == 0, string.Join(Environment.NewLine, layaCalibrationResult.Messages ?? []));
                Assert.Empty(layaCalibrationResult.Messages!);
                Assert.Equal(Python.Query.FileSHA256(path_Calibration), layaCalibrationResult.CalibrationSHA256);
                Assert.Equal(sha256_Model, layaCalibrationResult.ModelSHA256);
                Assert.Equal(sha256_Model, Python.Query.DirectorySHA256(layaPredictionOptions.ModelPath));
                Assert.Equal(48, layaCalibrationResult.RecordCount);
                Assert.Equal(0, layaCalibrationResult.SkippedCount);
                Assert.Equal(3, layaCalibrationResult.Temperatures!.Count);
                Assert.All(layaCalibrationResult.Temperatures!, x => Assert.InRange(x.Value, 0.5, 5.0));

                Classes.LayaPredictionOptions layaPredictionOptions_Calibrated = new(layaPredictionOptions) { CalibrationPath = path_Calibration };
                Classes.LayaPredictionResult? layaPredictionResult = await Create.LayaRequests(layaItems.GetRange(0, 2), layaLabelSets).PredictAsync(layaPredictionOptions_Calibrated);
                Assert.True(layaPredictionResult!.ExitCode == 0, string.Join(Environment.NewLine, layaPredictionResult.Messages ?? []));
                Assert.Equal(2, layaPredictionResult.Responses!.Count);
                Assert.Equal(layaCalibrationResult.CalibrationSHA256, layaPredictionResult.CalibrationSHA256);
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
