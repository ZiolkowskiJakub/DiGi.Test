using System;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.LayaEnvironmentResult"/> keeps its own checkpoint members and every member of <see cref="Python.Classes.PythonEnvironmentResult"/> through a string round trip and <c>SerializationCheck</c>, nested package list included, and that its copy constructor copies them all and clones the packages, for a runnable result and for one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void LayaEnvironmentResult()
        {
            DateTimeOffset checkedTime = new(2026, 10, 15, 6, 0, 0, TimeSpan.FromHours(2));

            Classes.LayaEnvironmentResult layaEnvironmentResult = new(
                true,
                @"D:\Laya\venv\Scripts\python.exe",
                "3.13.14",
                true,
                [new Python.Classes.PythonPackageVersion("laya", "0.4.7"), new Python.Classes.PythonPackageVersion("transformers", "5.19.0"), new Python.Classes.PythonPackageVersion("numpy", null)],
                [],
                ["No CUDA device: Laya runs on the CPU, which is much slower for training"],
                checkedTime,
                @"D:\Laya\checkpoints\convaiinnovations-laya\multilingual",
                "0d3f1e2a5b6c7d8e9f00112233445566778899aabbccddeeff00112233445566",
                "sequential",
                1024,
                256);

            static void AssertPopulated(Classes.LayaEnvironmentResult? layaEnvironmentResult_Actual, DateTimeOffset checkedTime)
            {
                Assert.NotNull(layaEnvironmentResult_Actual);
                Assert.True(layaEnvironmentResult_Actual!.Runnable);
                Assert.Equal(@"D:\Laya\venv\Scripts\python.exe", layaEnvironmentResult_Actual.PythonPath);
                Assert.Equal("3.13.14", layaEnvironmentResult_Actual.PythonVersion);
                Assert.True(layaEnvironmentResult_Actual.CudaAvailable);
                Assert.Equal(["laya", "transformers", "numpy"], layaEnvironmentResult_Actual.Packages!.Select(x => x.Name));
                Assert.Equal(["0.4.7", "5.19.0", null], layaEnvironmentResult_Actual.Packages!.Select(x => x.Version));
                Assert.Empty(layaEnvironmentResult_Actual.Messages!);
                Assert.Single(layaEnvironmentResult_Actual.Warnings!);
                Assert.Equal(checkedTime, layaEnvironmentResult_Actual.Checked);
                Assert.Equal(@"D:\Laya\checkpoints\convaiinnovations-laya\multilingual", layaEnvironmentResult_Actual.ModelPath);
                Assert.Equal("0d3f1e2a5b6c7d8e9f00112233445566778899aabbccddeeff00112233445566", layaEnvironmentResult_Actual.ModelSHA256);
                Assert.Equal("sequential", layaEnvironmentResult_Actual.OptionLayout);
                Assert.Equal(1024, layaEnvironmentResult_Actual.MaxLength);
                Assert.Equal(256, layaEnvironmentResult_Actual.HeadMaxLength);
            }

            AssertPopulated(layaEnvironmentResult, checkedTime);

            string? json = Core.Convert.ToSystem_String(layaEnvironmentResult);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.LayaEnvironmentResult>? layaEnvironmentResults = Core.Convert.ToDiGi<Classes.LayaEnvironmentResult>(json);
            AssertPopulated(layaEnvironmentResults?.FirstOrDefault(), checkedTime);

            Core.xUnit.Query.SerializationCheck(layaEnvironmentResult);

            Classes.LayaEnvironmentResult layaEnvironmentResult_Copy = new(layaEnvironmentResult);
            AssertPopulated(layaEnvironmentResult_Copy, checkedTime);
            Assert.NotSame(layaEnvironmentResult.Packages![0], layaEnvironmentResult_Copy.Packages![0]);

            Classes.LayaEnvironmentResult layaEnvironmentResult_Empty = new(false, null, null, null, null, ["No Python interpreter was found on PATH."], null, null, null, null, null, null, null);
            Core.xUnit.Query.SerializationCheck(layaEnvironmentResult_Empty);

            Classes.LayaEnvironmentResult layaEnvironmentResult_EmptyCopy = new(layaEnvironmentResult_Empty);
            Assert.False(layaEnvironmentResult_EmptyCopy.Runnable);
            Assert.Equal(["No Python interpreter was found on PATH."], layaEnvironmentResult_EmptyCopy.Messages!);
            Assert.Null(layaEnvironmentResult_EmptyCopy.Packages);
            Assert.Null(layaEnvironmentResult_EmptyCopy.ModelPath);
            Assert.Null(layaEnvironmentResult_EmptyCopy.ModelSHA256);
            Assert.Null(layaEnvironmentResult_EmptyCopy.OptionLayout);
            Assert.Null(layaEnvironmentResult_EmptyCopy.MaxLength);
            Assert.Null(layaEnvironmentResult_EmptyCopy.HeadMaxLength);
        }
    }
}
