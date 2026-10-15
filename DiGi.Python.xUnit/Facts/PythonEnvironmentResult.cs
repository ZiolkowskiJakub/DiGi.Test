using System;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.Python.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.PythonEnvironmentResult"/> keeps the values given to it, survives a string round trip and <c>SerializationCheck</c> with a nested package list, and that its copy constructor copies every member and clones the packages, for a runnable environment and for one with every optional member <c>null</c>.
        /// </summary>
        [Fact]
        public void PythonEnvironmentResult()
        {
            DateTimeOffset checkedTime = new(2026, 10, 15, 17, 0, 0, TimeSpan.FromHours(2));

            Classes.PythonEnvironmentResult pythonEnvironmentResult = new(
                true,
                @"C:\Python\Python313\python.exe",
                "3.13.14",
                true,
                [new Classes.PythonPackageVersion("torch", "2.7.0+cu128"), new Classes.PythonPackageVersion("transformers", null)],
                ["no blocking findings"],
                ["CUDA hardware acceleration available."],
                checkedTime);

            void AssertPopulated(Classes.PythonEnvironmentResult? pythonEnvironmentResult_Actual)
            {
                Assert.NotNull(pythonEnvironmentResult_Actual);
                Assert.True(pythonEnvironmentResult_Actual!.Runnable);
                Assert.Equal(@"C:\Python\Python313\python.exe", pythonEnvironmentResult_Actual.PythonPath);
                Assert.Equal("3.13.14", pythonEnvironmentResult_Actual.PythonVersion);
                Assert.True(pythonEnvironmentResult_Actual.CudaAvailable);
                Assert.Equal(2, pythonEnvironmentResult_Actual.Packages!.Count);
                Assert.Equal("torch", pythonEnvironmentResult_Actual.Packages[0].Name);
                Assert.Equal("2.7.0+cu128", pythonEnvironmentResult_Actual.Packages[0].Version);
                Assert.Equal("transformers", pythonEnvironmentResult_Actual.Packages[1].Name);
                Assert.Null(pythonEnvironmentResult_Actual.Packages[1].Version);
                Assert.Equal(["no blocking findings"], pythonEnvironmentResult_Actual.Messages!);
                Assert.Equal(["CUDA hardware acceleration available."], pythonEnvironmentResult_Actual.Warnings!);
                Assert.Equal(checkedTime, pythonEnvironmentResult_Actual.Checked);
            }

            AssertPopulated(pythonEnvironmentResult);

            string? json = Core.Convert.ToSystem_String(pythonEnvironmentResult);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.PythonEnvironmentResult>? pythonEnvironmentResults = Core.Convert.ToDiGi<Classes.PythonEnvironmentResult>(json);
            AssertPopulated(pythonEnvironmentResults?.FirstOrDefault());

            Core.xUnit.Query.SerializationCheck(pythonEnvironmentResult);

            Classes.PythonEnvironmentResult pythonEnvironmentResult_Copy = new(pythonEnvironmentResult);
            AssertPopulated(pythonEnvironmentResult_Copy);
            Assert.NotSame(pythonEnvironmentResult.Packages, pythonEnvironmentResult_Copy.Packages);
            Assert.NotSame(pythonEnvironmentResult.Packages![0], pythonEnvironmentResult_Copy.Packages![0]);
            Assert.NotSame(pythonEnvironmentResult.Messages, pythonEnvironmentResult_Copy.Messages);
            Assert.NotSame(pythonEnvironmentResult.Warnings, pythonEnvironmentResult_Copy.Warnings);

            //An interpreter that could not be run: every optional member null
            Classes.PythonEnvironmentResult pythonEnvironmentResult_Empty = new(false, null, null, null, null, null, null, null);
            Assert.False(pythonEnvironmentResult_Empty.Runnable);
            Assert.Null(pythonEnvironmentResult_Empty.Packages);
            Assert.Null(pythonEnvironmentResult_Empty.Checked);
            Core.xUnit.Query.SerializationCheck(pythonEnvironmentResult_Empty);

            Classes.PythonEnvironmentResult pythonEnvironmentResult_EmptyCopy = new(pythonEnvironmentResult_Empty);
            Assert.False(pythonEnvironmentResult_EmptyCopy.Runnable);
            Assert.Null(pythonEnvironmentResult_EmptyCopy.PythonPath);
            Assert.Null(pythonEnvironmentResult_EmptyCopy.PythonVersion);
            Assert.Null(pythonEnvironmentResult_EmptyCopy.CudaAvailable);
            Assert.Null(pythonEnvironmentResult_EmptyCopy.Packages);
            Assert.Null(pythonEnvironmentResult_EmptyCopy.Messages);
            Assert.Null(pythonEnvironmentResult_EmptyCopy.Warnings);
            Assert.Null(pythonEnvironmentResult_EmptyCopy.Checked);
        }
    }
}
