using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the training path end to end with the real interpreter and base checkpoint of DiGi.Laya_Preflight.conf: <see cref="Create.LayaTrainingSamples(IEnumerable{Classes.LayaItem}?, IEnumerable{Classes.LayaLabelSet}?, IEnumerable{Classes.LayaLabelAssignment}?, int)"/> from a labelled ticket set, <see cref="Modify.WriteTrainingFile(IEnumerable{Classes.LayaTrainingSample}?, string?)"/>, one epoch of <see cref="Modify.TrainAsync(Classes.LayaTrainingOptions?, System.Threading.CancellationToken)"/> - a trusted checkpoint with every sample trained or held out, none skipped, three temperatures, no checkpoint_latest left, and the base checkpoint unchanged - and finally <see cref="Modify.ClassifyAsync(IEnumerable{Classes.LayaItem}?, IEnumerable{Classes.LayaLabelSet}?, Classes.LayaClassificationOptions?, Classes.LayaPredictionOptions?, IEnumerable{Classes.LayaLabelConstraint}?, System.Threading.CancellationToken)"/> with the new checkpoint, which must load and answer every label.
        /// <para>The interpreter and checkpoint are machine specific, so the fact returns without asserting when the conf, its interpreter or its checkpoint is absent. The new checkpoint (about 644 MB for laya-multilingual) is written to the temporary folder and removed afterwards. A GPU is assumed for the duration below.</para>
        /// <para>Long test (44 s on an RTX 5090, 2026-10-15): runs only when DIGI_TEST_MAX_DURATION is Long.</para>
        /// </summary>
        [LongFact]
        public async Task Modify_TrainAsync()
        {
            string directory = Path.Combine(Path.GetTempPath(), string.Concat("Laya_Train_", Guid.NewGuid().ToString("N")));

            try
            {
                Classes.LayaPredictionOptions? layaPredictionOptions = PreflightOptions(Path.Combine(directory, "work"));
                if (layaPredictionOptions == null)
                {
                    return;
                }

                string sha256_Base = Python.Query.DirectorySHA256(layaPredictionOptions.ModelPath)!;

                (List<Classes.LayaItem> layaItems, List<Classes.LayaLabelSet> layaLabelSets, List<Classes.LayaLabelAssignment> layaLabelAssignments) = TicketData();
                List<Classes.LayaTrainingSample> layaTrainingSamples = Create.LayaTrainingSamples(layaItems, layaLabelSets, layaLabelAssignments, 0);
                Assert.Equal(48, layaTrainingSamples.Count);

                string path_Training = Path.Combine(directory, "training.jsonl");
                Assert.Equal(48, layaTrainingSamples.WriteTrainingFile(path_Training));

                string directory_Output = Path.Combine(directory, "checkpoint");
                Classes.LayaTrainingOptions layaTrainingOptions = new()
                {
                    PythonPath = layaPredictionOptions.PythonPath,
                    BaseModelPath = layaPredictionOptions.ModelPath,
                    TrainingFilePath = path_Training,
                    OutputDirectory = directory_Output,
                    WorkingDirectory = Path.Combine(directory, "work"),
                    Epochs = 1,
                    MicroBatch = 4,
                    GradientAccumulation = 1,
                    CalibrationMaximum = 8,
                    CalibrationFraction = 0.25
                };

                Classes.LayaTrainingResult? layaTrainingResult = await layaTrainingOptions.TrainAsync();
                Assert.NotNull(layaTrainingResult);
                Assert.True(layaTrainingResult!.ExitCode == 0, string.Join(Environment.NewLine, layaTrainingResult.Messages ?? []));
                Assert.Empty(layaTrainingResult.Messages!);
                Assert.NotNull(layaTrainingResult.ModelSHA256);
                Assert.Equal(sha256_Base, layaTrainingResult.BaseModelSHA256);
                Assert.Equal(sha256_Base, Python.Query.DirectorySHA256(layaPredictionOptions.ModelPath));
                Assert.Equal(48, layaTrainingResult.TrainItemCount + layaTrainingResult.CalibrationItemCount);
                Assert.Equal(8, layaTrainingResult.CalibrationItemCount);
                Assert.Empty(layaTrainingResult.SkippedCounts!);
                Assert.Single(layaTrainingResult.EpochLosses!);
                Assert.Equal(3, layaTrainingResult.Temperatures!.Count);
                Assert.False(Directory.Exists(Path.Combine(directory_Output, "checkpoint_latest")));

                Classes.LayaPredictionOptions layaPredictionOptions_Trained = new(layaPredictionOptions) { ModelPath = directory_Output };
                Classes.LayaClassificationRunResult? layaClassificationRunResult = await layaItems.Take(2).ClassifyAsync(layaLabelSets, null, layaPredictionOptions_Trained);
                Assert.NotNull(layaClassificationRunResult);
                Assert.Equal(0, layaClassificationRunResult!.ExitCode);
                Assert.Empty(layaClassificationRunResult.Messages!);
                Assert.Equal(layaTrainingResult.ModelSHA256, layaClassificationRunResult.ModelSHA256);
                List<Classes.LayaClassificationResult> layaClassificationResults = layaClassificationRunResult.Results!;
                Assert.Equal(2, layaClassificationResults.Count);
                Assert.All(layaClassificationResults, x => Assert.Empty(x.Messages!));
                Assert.All(layaClassificationResults, x => Assert.Equal(3, x.Scores!.Count));
                Assert.All(layaClassificationResults, x => Assert.Equal(layaTrainingResult.ModelSHA256, x.ModelSHA256));
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
