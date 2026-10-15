using System;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaPredictionOptions"/> starts from its documented defaults, keeps every member it is given - the two time limits included - through a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies every member.
        /// </summary>
        [Fact]
        public void LayaPredictionOptions()
        {
            Classes.LayaPredictionOptions layaPredictionOptions_Default = new();
            Assert.Equal(16, layaPredictionOptions_Default.BatchSize);
            Assert.Equal(Enums.LayaDevice.Auto, layaPredictionOptions_Default.Device);
            Assert.Null(layaPredictionOptions_Default.CalibrationPath);
            Assert.Null(layaPredictionOptions_Default.HeadMaxLength);
            Assert.Null(layaPredictionOptions_Default.InactivityTimeout);
            Assert.Null(layaPredictionOptions_Default.MaxLength);
            Assert.Null(layaPredictionOptions_Default.ModelPath);
            Assert.Null(layaPredictionOptions_Default.PythonPath);
            Assert.Null(layaPredictionOptions_Default.Timeout);
            Assert.Null(layaPredictionOptions_Default.WorkingDirectory);
            Core.xUnit.Query.SerializationCheck(layaPredictionOptions_Default);

            Classes.LayaPredictionOptions layaPredictionOptions = new()
            {
                BatchSize = 8,
                CalibrationPath = @"D:\Laya\runs\multilingual.calibration.json",
                Device = Enums.LayaDevice.Cuda,
                HeadMaxLength = 192,
                InactivityTimeout = TimeSpan.FromMinutes(5),
                MaxLength = 768,
                ModelPath = @"D:\Laya\checkpoints\convaiinnovations-laya\multilingual",
                PythonPath = @"D:\Laya\venv\Scripts\python.exe",
                Timeout = TimeSpan.FromHours(1.5),
                WorkingDirectory = @"D:\Laya\scratch\predict"
            };

            static void AssertPopulated(Classes.LayaPredictionOptions? layaPredictionOptions_Actual)
            {
                Assert.NotNull(layaPredictionOptions_Actual);
                Assert.Equal(8, layaPredictionOptions_Actual!.BatchSize);
                Assert.Equal(@"D:\Laya\runs\multilingual.calibration.json", layaPredictionOptions_Actual.CalibrationPath);
                Assert.Equal(Enums.LayaDevice.Cuda, layaPredictionOptions_Actual.Device);
                Assert.Equal(192, layaPredictionOptions_Actual.HeadMaxLength);
                Assert.Equal(TimeSpan.FromMinutes(5), layaPredictionOptions_Actual.InactivityTimeout);
                Assert.Equal(768, layaPredictionOptions_Actual.MaxLength);
                Assert.Equal(@"D:\Laya\checkpoints\convaiinnovations-laya\multilingual", layaPredictionOptions_Actual.ModelPath);
                Assert.Equal(@"D:\Laya\venv\Scripts\python.exe", layaPredictionOptions_Actual.PythonPath);
                Assert.Equal(TimeSpan.FromHours(1.5), layaPredictionOptions_Actual.Timeout);
                Assert.Equal(@"D:\Laya\scratch\predict", layaPredictionOptions_Actual.WorkingDirectory);
            }

            AssertPopulated(layaPredictionOptions);

            string? json = Core.Convert.ToSystem_String(layaPredictionOptions);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaPredictionOptions>? layaPredictionOptionsList = Core.Convert.ToDiGi<Classes.LayaPredictionOptions>(json);
            AssertPopulated(layaPredictionOptionsList?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaPredictionOptions);

            AssertPopulated(new Classes.LayaPredictionOptions(layaPredictionOptions));
        }
    }
}
