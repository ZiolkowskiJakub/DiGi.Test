using System;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaTrainingOptions"/> starts with every training setting left to laya, keeps every member it is given - enums and time limits included - through a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies every member.
        /// </summary>
        [Fact]
        public void LayaTrainingOptions()
        {
            Classes.LayaTrainingOptions layaTrainingOptions_Default = new();
            Assert.Null(layaTrainingOptions_Default.Epochs);
            Assert.Equal(Enums.LayaLoss.Default, layaTrainingOptions_Default.Loss);
            Assert.Equal(Enums.LayaOptionLayout.Default, layaTrainingOptions_Default.OptionLayout);
            Assert.Equal(Enums.LayaDevice.Auto, layaTrainingOptions_Default.Device);
            Assert.False(layaTrainingOptions_Default.FreezeEncoder);
            Assert.False(layaTrainingOptions_Default.ShuffleChoiceOptions);
            Assert.Empty(layaTrainingOptions_Default.ToLaya_TrainConfiguration()!);
            Core.xUnit.Query.SerializationCheck(layaTrainingOptions_Default);

            Classes.LayaTrainingOptions layaTrainingOptions = new()
            {
                BaseModelPath = @"D:\Laya\checkpoints\convaiinnovations-laya\multilingual",
                CalibrationFraction = 0.2,
                CalibrationMaximum = 200,
                CalibrationSeed = 7,
                Device = Enums.LayaDevice.Cuda,
                EncoderLearningRate = 2e-5,
                Epochs = 8,
                EvaluationFilePath = @"D:\Laya\data\evaluation.jsonl",
                FreezeEncoder = true,
                GradientAccumulation = 4,
                HeadLearningRate = 5e-5,
                HeadMaxLength = 192,
                InactivityTimeout = TimeSpan.FromMinutes(20),
                LabelSmoothing = 0.1,
                Loss = Enums.LayaLoss.SoftCrossEntropy,
                MaxLength = 768,
                MicroBatch = 2,
                OptionLayout = Enums.LayaOptionLayout.Sequential,
                OutputDirectory = @"D:\Laya\runs\recipes-1",
                PythonPath = @"D:\Laya\venv\Scripts\python.exe",
                Seed = 3,
                ShuffleChoiceOptions = true,
                TargetError = 0.05,
                Timeout = TimeSpan.FromHours(6),
                TrainingFilePath = @"D:\Laya\data\training.jsonl",
                WorkingDirectory = @"D:\Laya\scratch\train"
            };

            static void AssertPopulated(Classes.LayaTrainingOptions? layaTrainingOptions_Actual)
            {
                Assert.NotNull(layaTrainingOptions_Actual);
                Assert.Equal(@"D:\Laya\checkpoints\convaiinnovations-laya\multilingual", layaTrainingOptions_Actual!.BaseModelPath);
                Assert.Equal(0.2, layaTrainingOptions_Actual.CalibrationFraction);
                Assert.Equal(200, layaTrainingOptions_Actual.CalibrationMaximum);
                Assert.Equal(7, layaTrainingOptions_Actual.CalibrationSeed);
                Assert.Equal(Enums.LayaDevice.Cuda, layaTrainingOptions_Actual.Device);
                Assert.Equal(2e-5, layaTrainingOptions_Actual.EncoderLearningRate);
                Assert.Equal(8, layaTrainingOptions_Actual.Epochs);
                Assert.Equal(@"D:\Laya\data\evaluation.jsonl", layaTrainingOptions_Actual.EvaluationFilePath);
                Assert.True(layaTrainingOptions_Actual.FreezeEncoder);
                Assert.Equal(4, layaTrainingOptions_Actual.GradientAccumulation);
                Assert.Equal(5e-5, layaTrainingOptions_Actual.HeadLearningRate);
                Assert.Equal(192, layaTrainingOptions_Actual.HeadMaxLength);
                Assert.Equal(TimeSpan.FromMinutes(20), layaTrainingOptions_Actual.InactivityTimeout);
                Assert.Equal(0.1, layaTrainingOptions_Actual.LabelSmoothing);
                Assert.Equal(Enums.LayaLoss.SoftCrossEntropy, layaTrainingOptions_Actual.Loss);
                Assert.Equal(768, layaTrainingOptions_Actual.MaxLength);
                Assert.Equal(2, layaTrainingOptions_Actual.MicroBatch);
                Assert.Equal(Enums.LayaOptionLayout.Sequential, layaTrainingOptions_Actual.OptionLayout);
                Assert.Equal(@"D:\Laya\runs\recipes-1", layaTrainingOptions_Actual.OutputDirectory);
                Assert.Equal(@"D:\Laya\venv\Scripts\python.exe", layaTrainingOptions_Actual.PythonPath);
                Assert.Equal(3, layaTrainingOptions_Actual.Seed);
                Assert.True(layaTrainingOptions_Actual.ShuffleChoiceOptions);
                Assert.Equal(0.05, layaTrainingOptions_Actual.TargetError);
                Assert.Equal(TimeSpan.FromHours(6), layaTrainingOptions_Actual.Timeout);
                Assert.Equal(@"D:\Laya\data\training.jsonl", layaTrainingOptions_Actual.TrainingFilePath);
                Assert.Equal(@"D:\Laya\scratch\train", layaTrainingOptions_Actual.WorkingDirectory);
            }

            AssertPopulated(layaTrainingOptions);

            string? json = Core.Convert.ToSystem_String(layaTrainingOptions);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaTrainingOptions>? layaTrainingOptionsList = Core.Convert.ToDiGi<Classes.LayaTrainingOptions>(json);
            AssertPopulated(layaTrainingOptionsList?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaTrainingOptions);

            AssertPopulated(new Classes.LayaTrainingOptions(layaTrainingOptions));
        }
    }
}
