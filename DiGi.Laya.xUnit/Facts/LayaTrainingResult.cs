using System;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaTrainingResult"/> keeps every member through a string round trip and <c>SerializationCheck</c> with the nested skipped counts, epoch losses and temperatures, and that its copy constructor copies every member, for a populated instance and for one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaTrainingResult()
        {
            Classes.LayaTrainingResult layaTrainingResult = new(
                @"D:\Laya\runs\multilingual-recipes",
                "aa11bb22cc33dd44ee55ff6600778899aabbccddeeff00112233445566778899",
                "0d3f1e2a5b6c7d8e9f00112233445566778899aabbccddeeff00112233445566",
                56,
                14,
                14,
                [new Classes.LayaSkippedCount("empty_text", 2)],
                [0.3554402009717056, 0.21],
                [new Classes.LayaTemperature(Enums.LayaQuestionType.Choice, 1.0), new Classes.LayaTemperature(Enums.LayaQuestionType.Noul, 0.5)],
                TimeSpan.FromSeconds(25.34),
                0,
                false,
                false,
                false,
                ["laya.train: choice calibration: not fitted"]);

            static void AssertPopulated(Classes.LayaTrainingResult? layaTrainingResult_Actual)
            {
                Assert.NotNull(layaTrainingResult_Actual);
                Assert.Equal(@"D:\Laya\runs\multilingual-recipes", layaTrainingResult_Actual!.CheckpointDirectory);
                Assert.Equal("aa11bb22cc33dd44ee55ff6600778899aabbccddeeff00112233445566778899", layaTrainingResult_Actual.ModelSHA256);
                Assert.Equal("0d3f1e2a5b6c7d8e9f00112233445566778899aabbccddeeff00112233445566", layaTrainingResult_Actual.BaseModelSHA256);
                Assert.Equal(56, layaTrainingResult_Actual.TrainItemCount);
                Assert.Equal(14, layaTrainingResult_Actual.CalibrationItemCount);
                Assert.Equal(14, layaTrainingResult_Actual.EvaluationItemCount);
                Assert.Equal("empty_text", Assert.Single(layaTrainingResult_Actual.SkippedCounts!).Reason);
                Assert.Equal([0.3554402009717056, 0.21], layaTrainingResult_Actual.EpochLosses!);
                Assert.Equal([Enums.LayaQuestionType.Choice, Enums.LayaQuestionType.Noul], layaTrainingResult_Actual.Temperatures!.Select(x => x.QuestionType));
                Assert.Equal(TimeSpan.FromSeconds(25.34), layaTrainingResult_Actual.Duration);
                Assert.Equal(0, layaTrainingResult_Actual.ExitCode);
                Assert.False(layaTrainingResult_Actual.Stalled);
                Assert.False(layaTrainingResult_Actual.TimedOut);
                Assert.False(layaTrainingResult_Actual.Cancelled);
                Assert.Single(layaTrainingResult_Actual.Messages!);
            }

            AssertPopulated(layaTrainingResult);

            string? json = Core.Convert.ToSystem_String(layaTrainingResult);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaTrainingResult>? layaTrainingResults = Core.Convert.ToDiGi<Classes.LayaTrainingResult>(json);
            AssertPopulated(layaTrainingResults?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaTrainingResult);

            Classes.LayaTrainingResult layaTrainingResult_Copy = new(layaTrainingResult);
            AssertPopulated(layaTrainingResult_Copy);
            Assert.NotSame(layaTrainingResult.SkippedCounts![0], layaTrainingResult_Copy.SkippedCounts![0]);
            Assert.NotSame(layaTrainingResult.EpochLosses, layaTrainingResult_Copy.EpochLosses);
            Assert.NotSame(layaTrainingResult.Temperatures![0], layaTrainingResult_Copy.Temperatures![0]);

            Classes.LayaTrainingResult layaTrainingResult_Empty = new(null, null, null, null, null, null, null, null, null, null, -1, true, true, true, null);
            Core.xUnit.Query.SerializationCheck(layaTrainingResult_Empty);

            Classes.LayaTrainingResult layaTrainingResult_EmptyCopy = new(layaTrainingResult_Empty);
            Assert.Null(layaTrainingResult_EmptyCopy.CheckpointDirectory);
            Assert.Null(layaTrainingResult_EmptyCopy.ModelSHA256);
            Assert.Null(layaTrainingResult_EmptyCopy.BaseModelSHA256);
            Assert.Null(layaTrainingResult_EmptyCopy.TrainItemCount);
            Assert.Null(layaTrainingResult_EmptyCopy.SkippedCounts);
            Assert.Null(layaTrainingResult_EmptyCopy.EpochLosses);
            Assert.Null(layaTrainingResult_EmptyCopy.Temperatures);
            Assert.Null(layaTrainingResult_EmptyCopy.Duration);
            Assert.Equal(-1, layaTrainingResult_EmptyCopy.ExitCode);
            Assert.True(layaTrainingResult_EmptyCopy.Stalled);
            Assert.True(layaTrainingResult_EmptyCopy.TimedOut);
            Assert.True(layaTrainingResult_EmptyCopy.Cancelled);
            Assert.Null(layaTrainingResult_EmptyCopy.Messages);
        }
    }
}
