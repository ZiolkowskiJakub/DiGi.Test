using System;
using System.IO;

namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.YOLOEnvironmentResult"/> keeps all properties given to it, round-trips cleanly through JSON serialization, and passes contract validation.
        /// </summary>
        [Fact]
        public void YOLOEnvironmentResult()
        {
            DateTimeOffset checkedTime = new(2026, 8, 28, 17, 0, 0, TimeSpan.FromHours(2));

            Classes.YOLOEnvironmentResult yOLOEnvironmentResult = new(
                true,
                @"C:\Users\AppData\Local\Programs\Python\Python313\python.exe",
                "3.13.14",
                "8.3.130",
                "2.7.0+cu128",
                true,
                @"C:\YOLO\models\best.pt",
                "8.3.130",
                ["CUDA hardware acceleration available."],
                checkedTime);

            Assert.True(yOLOEnvironmentResult.Runnable);
            Assert.Equal(@"C:\Users\AppData\Local\Programs\Python\Python313\python.exe", yOLOEnvironmentResult.PythonPath);
            Assert.Equal("3.13.14", yOLOEnvironmentResult.PythonVersion);
            Assert.Equal("8.3.130", yOLOEnvironmentResult.UltralyticsVersion);
            Assert.Equal("2.7.0+cu128", yOLOEnvironmentResult.TorchVersion);
            Assert.True(yOLOEnvironmentResult.CudaAvailable);
            Assert.Equal(@"C:\YOLO\models\best.pt", yOLOEnvironmentResult.ModelPath);
            Assert.Equal("8.3.130", yOLOEnvironmentResult.ModelUltralyticsVersion);
            Assert.Single(yOLOEnvironmentResult.Messages!);
            Assert.Equal(checkedTime, yOLOEnvironmentResult.Checked);

            Core.xUnit.Query.SerializationCheck(yOLOEnvironmentResult);
        }

        /// <summary>
        /// Verifies that probing a missing or garbage interpreter path returns a non-runnable environment result with populated messages rather than throwing an exception.
        /// </summary>
        [Fact]
        public void YOLOEnvironmentResult_InterpreterMissing()
        {
            string invalidPythonPath = Path.Combine(Path.GetTempPath(), "no_such_interpreter_" + Path.GetRandomFileName() + ".exe");

            Classes.YOLOEnvironmentResult yOLOEnvironmentResult = Query.YOLOEnvironmentResult(invalidPythonPath, null);

            Assert.NotNull(yOLOEnvironmentResult);
            Assert.False(yOLOEnvironmentResult.Runnable);
            Assert.Equal(invalidPythonPath, yOLOEnvironmentResult.PythonPath);
            Assert.Null(yOLOEnvironmentResult.PythonVersion);
            Assert.Null(yOLOEnvironmentResult.UltralyticsVersion);
            Assert.Null(yOLOEnvironmentResult.TorchVersion);
            Assert.NotEmpty(yOLOEnvironmentResult.Messages!);

            Core.xUnit.Query.SerializationCheck(yOLOEnvironmentResult);
        }

        /// <summary>
        /// Verifies that probing an environment with a <c>null</c> model path does not throw and handles missing interpreters cleanly.
        /// </summary>
        [Fact]
        public void YOLOEnvironmentResult_NullModel()
        {
            string invalidPythonPath = Path.Combine(Path.GetTempPath(), "no_such_interpreter_" + Path.GetRandomFileName() + ".exe");

            Classes.YOLOEnvironmentResult yOLOEnvironmentResult = Query.YOLOEnvironmentResult(invalidPythonPath, null);

            Assert.NotNull(yOLOEnvironmentResult);
            Assert.False(yOLOEnvironmentResult.Runnable);
            Assert.Null(yOLOEnvironmentResult.ModelPath);
            Assert.Null(yOLOEnvironmentResult.ModelUltralyticsVersion);
        }

        /// <summary>
        /// Verifies that probing a missing or invalid interpreter path completes rapidly so a preflight check does not block execution.
        /// </summary>
        [Fact]
        public void YOLOEnvironmentResult_Performance()
        {
            string invalidPythonPath = Path.Combine(Path.GetTempPath(), "no_such_interpreter_" + Path.GetRandomFileName() + ".exe");

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Classes.YOLOEnvironmentResult yOLOEnvironmentResult = Query.YOLOEnvironmentResult(invalidPythonPath, null);
            stopwatch.Stop();

            Assert.NotNull(yOLOEnvironmentResult);
            Assert.False(yOLOEnvironmentResult.Runnable);
            Assert.True(stopwatch.ElapsedMilliseconds < 2000, string.Format("Preflight check took {0} ms, expected under 2000 ms.", stopwatch.ElapsedMilliseconds));
        }
    }
}
