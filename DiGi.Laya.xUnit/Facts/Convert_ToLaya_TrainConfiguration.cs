using System.Globalization;
using System.IO;
using System.Text.Json.Nodes;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Convert.ToLaya_TrainConfiguration(Classes.LayaTrainingOptions?)"/> writes nothing for options left to laya, and for set options exactly laya 0.4.7's TrainConfig fields - loss "soft-ce" and "rlcd", shuffle_options ["choice"], option_layout "parallel", freeze_encoder only when true, eval_data as a full path - with numbers that do not depend on the current culture (<c>pl-PL</c>); and that it leaves out what is not a TrainConfig field (paths, device, limits), giving <c>null</c> for <c>null</c>.
        /// </summary>
        [Fact]
        public void Convert_ToLaya_TrainConfiguration()
        {
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("pl-PL");

                Assert.Equal("{}", new Classes.LayaTrainingOptions() { BaseModelPath = "b", OutputDirectory = "o", TrainingFilePath = "t", Device = Enums.LayaDevice.Cuda }.ToLaya_TrainConfiguration()!.ToJsonString());

                Classes.LayaTrainingOptions layaTrainingOptions = new()
                {
                    Epochs = 8,
                    MicroBatch = 2,
                    GradientAccumulation = 16,
                    EncoderLearningRate = 2.5e-5,
                    HeadLearningRate = 0.0001,
                    Loss = Enums.LayaLoss.SoftCrossEntropy,
                    ShuffleChoiceOptions = true,
                    OptionLayout = Enums.LayaOptionLayout.Parallel,
                    FreezeEncoder = true,
                    LabelSmoothing = 0.1,
                    CalibrationMaximum = 200,
                    CalibrationFraction = 0.15,
                    CalibrationSeed = 7,
                    Seed = 3,
                    MaxLength = 1024,
                    HeadMaxLength = 256,
                    TargetError = 0.05,
                    EvaluationFilePath = @"D:\Laya\data\..\data\evaluation.jsonl"
                };

                Assert.Equal(
                    "{\"epochs\":8,\"micro_batch\":2,\"grad_accum\":16,\"encoder_lr\":2.5E-05,\"head_lr\":0.0001,\"loss\":\"soft-ce\",\"shuffle_options\":[\"choice\"],\"option_layout\":\"parallel\",\"freeze_encoder\":true," +
                    "\"label_smoothing\":0.1,\"calib_max\":200,\"calib_frac\":0.15,\"calib_seed\":7,\"seed\":3,\"max_len\":1024,\"head_max_len\":256,\"target_error\":0.05,\"eval_data\":\"D:\\\\Laya\\\\data\\\\evaluation.jsonl\"}",
                    layaTrainingOptions.ToLaya_TrainConfiguration()!.ToJsonString());

                JsonObject? jsonObject = new Classes.LayaTrainingOptions() { Loss = Enums.LayaLoss.Rlcd, OptionLayout = Enums.LayaOptionLayout.Sequential, FreezeEncoder = false }.ToLaya_TrainConfiguration();
                Assert.Equal("{\"loss\":\"rlcd\",\"option_layout\":\"sequential\"}", jsonObject!.ToJsonString());

                Assert.Null(((Classes.LayaTrainingOptions?)null).ToLaya_TrainConfiguration());
                Assert.Equal(Path.GetFullPath("relative.jsonl"), new Classes.LayaTrainingOptions() { EvaluationFilePath = "relative.jsonl" }.ToLaya_TrainConfiguration()!["eval_data"]!.GetValue<string>());
            }
            finally
            {
                CultureInfo.CurrentCulture = cultureInfo;
            }
        }
    }
}
