using System;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaCalibrationOptions"/> starts from its documented defaults, keeps every member it is given through a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies every member.
        /// </summary>
        [Fact]
        public void LayaCalibrationOptions()
        {
            Classes.LayaCalibrationOptions layaCalibrationOptions_Default = new();
            Assert.Equal(0, layaCalibrationOptions_Default.Seed);
            Assert.Equal(Enums.LayaDevice.Auto, layaCalibrationOptions_Default.Device);
            Assert.Null(layaCalibrationOptions_Default.CalibrationPath);
            Core.xUnit.Query.SerializationCheck(layaCalibrationOptions_Default);

            Classes.LayaCalibrationOptions layaCalibrationOptions = new()
            {
                CalibrationFilePath = @"D:\Laya\data\calibration.jsonl",
                CalibrationPath = @"D:\Laya\runs\multilingual.calibration.json",
                Device = Enums.LayaDevice.Cpu,
                InactivityTimeout = TimeSpan.FromMinutes(30),
                ModelPath = @"D:\Laya\checkpoints\convaiinnovations-laya\multilingual",
                PythonPath = @"D:\Laya\venv\Scripts\python.exe",
                Seed = 11,
                Timeout = TimeSpan.FromHours(2),
                WorkingDirectory = @"D:\Laya\scratch\calibrate"
            };

            static void AssertPopulated(Classes.LayaCalibrationOptions? layaCalibrationOptions_Actual)
            {
                Assert.NotNull(layaCalibrationOptions_Actual);
                Assert.Equal(@"D:\Laya\data\calibration.jsonl", layaCalibrationOptions_Actual!.CalibrationFilePath);
                Assert.Equal(@"D:\Laya\runs\multilingual.calibration.json", layaCalibrationOptions_Actual.CalibrationPath);
                Assert.Equal(Enums.LayaDevice.Cpu, layaCalibrationOptions_Actual.Device);
                Assert.Equal(TimeSpan.FromMinutes(30), layaCalibrationOptions_Actual.InactivityTimeout);
                Assert.Equal(@"D:\Laya\checkpoints\convaiinnovations-laya\multilingual", layaCalibrationOptions_Actual.ModelPath);
                Assert.Equal(@"D:\Laya\venv\Scripts\python.exe", layaCalibrationOptions_Actual.PythonPath);
                Assert.Equal(11, layaCalibrationOptions_Actual.Seed);
                Assert.Equal(TimeSpan.FromHours(2), layaCalibrationOptions_Actual.Timeout);
                Assert.Equal(@"D:\Laya\scratch\calibrate", layaCalibrationOptions_Actual.WorkingDirectory);
            }

            AssertPopulated(layaCalibrationOptions);

            string? json = Core.Convert.ToSystem_String(layaCalibrationOptions);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaCalibrationOptions>? layaCalibrationOptionsList = Core.Convert.ToDiGi<Classes.LayaCalibrationOptions>(json);
            AssertPopulated(layaCalibrationOptionsList?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(layaCalibrationOptions);

            AssertPopulated(new Classes.LayaCalibrationOptions(layaCalibrationOptions));
        }
    }
}
