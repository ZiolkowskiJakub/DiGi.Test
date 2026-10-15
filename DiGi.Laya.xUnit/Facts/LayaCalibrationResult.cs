using System;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaCalibrationResult"/> keeps every member through a string round trip and <c>SerializationCheck</c> with the nested temperatures, and that its copy constructor copies every member, for a populated instance and for one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaCalibrationResult()
        {
            Classes.LayaCalibrationResult layaCalibrationResult = new(
                @"D:\Laya\runs\multilingual.calibration.json",
                "cc11bb22cc33dd44ee55ff6600778899aabbccddeeff00112233445566778899",
                "0d3f1e2a5b6c7d8e9f00112233445566778899aabbccddeeff00112233445566",
                70,
                1,
                [new Classes.LayaTemperature(Enums.LayaQuestionType.Choice, 0.5), new Classes.LayaTemperature(Enums.LayaQuestionType.Score, 5.0), new Classes.LayaTemperature(Enums.LayaQuestionType.Noul, 2.4319565296173096)],
                0.0522,
                0.0313,
                2500,
                TimeSpan.FromSeconds(25.34),
                0,
                false,
                false,
                false,
                ["laya.train: choice calibration: not fitted"]);

            static void AssertPopulated(Classes.LayaCalibrationResult? layaCalibrationResult_Actual)
            {
                Assert.NotNull(layaCalibrationResult_Actual);
                Assert.Equal(@"D:\Laya\runs\multilingual.calibration.json", layaCalibrationResult_Actual!.CalibrationPath);
                Assert.Equal("cc11bb22cc33dd44ee55ff6600778899aabbccddeeff00112233445566778899", layaCalibrationResult_Actual.CalibrationSHA256);
                Assert.Equal("0d3f1e2a5b6c7d8e9f00112233445566778899aabbccddeeff00112233445566", layaCalibrationResult_Actual.ModelSHA256);
                Assert.Equal(70, layaCalibrationResult_Actual.RecordCount);
                Assert.Equal(1, layaCalibrationResult_Actual.SkippedCount);
                Assert.Equal([0.5, 5.0, 2.4319565296173096], layaCalibrationResult_Actual.Temperatures!.Select(x => x.Value));
                Assert.Equal(0.0522, layaCalibrationResult_Actual.ExpectedCalibrationErrorBefore);
                Assert.Equal(0.0313, layaCalibrationResult_Actual.ExpectedCalibrationErrorAfter);
                Assert.Equal(2500, layaCalibrationResult_Actual.EvaluationRecordCount);
                Assert.Equal(TimeSpan.FromSeconds(25.34), layaCalibrationResult_Actual.Duration);
                Assert.Equal(0, layaCalibrationResult_Actual.ExitCode);
                Assert.False(layaCalibrationResult_Actual.Stalled);
                Assert.False(layaCalibrationResult_Actual.TimedOut);
                Assert.False(layaCalibrationResult_Actual.Cancelled);
                Assert.Single(layaCalibrationResult_Actual.Messages!);
            }

            AssertPopulated(layaCalibrationResult);

            string? json = Core.Convert.ToSystem_String(layaCalibrationResult);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaCalibrationResult>? layaCalibrationResults = Core.Convert.ToDiGi<Classes.LayaCalibrationResult>(json);
            AssertPopulated(layaCalibrationResults?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaCalibrationResult);

            Classes.LayaCalibrationResult layaCalibrationResult_Copy = new(layaCalibrationResult);
            AssertPopulated(layaCalibrationResult_Copy);
            Assert.NotSame(layaCalibrationResult.Temperatures![0], layaCalibrationResult_Copy.Temperatures![0]);
            Assert.NotSame(layaCalibrationResult.Messages, layaCalibrationResult_Copy.Messages);

            Classes.LayaCalibrationResult layaCalibrationResult_Empty = new(null, null, null, null, null, null, null, null, null, null, -1, true, true, true, null);
            Core.xUnit.Query.SerializationCheck(layaCalibrationResult_Empty);

            Classes.LayaCalibrationResult layaCalibrationResult_EmptyCopy = new(layaCalibrationResult_Empty);
            Assert.Null(layaCalibrationResult_EmptyCopy.CalibrationPath);
            Assert.Null(layaCalibrationResult_EmptyCopy.CalibrationSHA256);
            Assert.Null(layaCalibrationResult_EmptyCopy.ModelSHA256);
            Assert.Null(layaCalibrationResult_EmptyCopy.RecordCount);
            Assert.Null(layaCalibrationResult_EmptyCopy.Temperatures);
            Assert.Null(layaCalibrationResult_EmptyCopy.ExpectedCalibrationErrorBefore);
            Assert.Null(layaCalibrationResult_EmptyCopy.ExpectedCalibrationErrorAfter);
            Assert.Null(layaCalibrationResult_EmptyCopy.EvaluationRecordCount);
            Assert.Null(layaCalibrationResult_EmptyCopy.Duration);
            Assert.Equal(-1, layaCalibrationResult_EmptyCopy.ExitCode);
            Assert.True(layaCalibrationResult_EmptyCopy.Stalled);
            Assert.True(layaCalibrationResult_EmptyCopy.TimedOut);
            Assert.True(layaCalibrationResult_EmptyCopy.Cancelled);
            Assert.Null(layaCalibrationResult_EmptyCopy.Messages);
        }
    }
}
