using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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
                [],
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
            Assert.Empty(yOLOEnvironmentResult.Messages!);
            Assert.Single(yOLOEnvironmentResult.Warnings!);
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

        /// <summary>
        /// Verifies that the preflight probe reports a readable checkpoint as runnable and names the ultralytics version that wrote it, and that a present but unreadable model is reported as a warning rather than making the machine unrunnable.
        /// <para>The interpreter and checkpoint are machine specific - a CPython carrying ultralytics and torch plus the frozen model - so both are read from a git-ignored conf (DiGi.YOLO_Preflight.conf) and the fact returns without asserting when that conf is absent. The split between <see cref="Classes.YOLOEnvironmentResult.Messages"/> and <see cref="Classes.YOLOEnvironmentResult.Warnings"/> is what makes the second half hold: runnable is about the machine, and the model header is diagnostic.</para>
        /// </summary>
        [Fact]
        public void YOLOEnvironmentResult_Model()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            string? directory_UserFiles = Core.xUnit.Query.UserFilesDirectory(assembly);
            if (string.IsNullOrWhiteSpace(directory_UserFiles))
            {
                return;
            }

            string path_Configuration = Path.Combine(directory_UserFiles!, "DiGi.YOLO_Preflight.conf");
            if (!File.Exists(path_Configuration))
            {
                return;
            }

            Dictionary<string, string> settings = [];
            foreach (string line in File.ReadAllLines(path_Configuration))
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                int index = line.IndexOf('=');
                if (index <= 0)
                {
                    continue;
                }

                settings[line.Substring(0, index).Trim()] = line.Substring(index + 1).Trim();
            }

            settings.TryGetValue("PythonPath", out string? path_Python);
            settings.TryGetValue("ModelPath", out string? path_Model);

            if (string.IsNullOrWhiteSpace(path_Python) || !File.Exists(path_Python) || string.IsNullOrWhiteSpace(path_Model) || !File.Exists(path_Model))
            {
                return;
            }

            // A readable checkpoint reports the ultralytics version recorded inside it and leaves the machine runnable
            Classes.YOLOEnvironmentResult yOLOEnvironmentResult_Readable = Query.YOLOEnvironmentResult(path_Python, path_Model);
            Assert.NotNull(yOLOEnvironmentResult_Readable);
            Assert.True(yOLOEnvironmentResult_Readable.Runnable);
            Assert.Equal("8.3.130", yOLOEnvironmentResult_Readable.ModelUltralyticsVersion);
            Assert.Empty(yOLOEnvironmentResult_Readable.Warnings ?? []);

            // A present model whose header cannot be parsed is a warning, not a refusal: runnable stays true and the
            // reason surfaces in Warnings rather than Messages
            string path_Model_Unreadable = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Unreadable_" + Path.GetRandomFileName() + ".pt");
            try
            {
                File.WriteAllText(path_Model_Unreadable, "this is not a torch checkpoint");

                Classes.YOLOEnvironmentResult yOLOEnvironmentResult_Unreadable = Query.YOLOEnvironmentResult(path_Python, path_Model_Unreadable);
                Assert.NotNull(yOLOEnvironmentResult_Unreadable);
                Assert.True(yOLOEnvironmentResult_Unreadable.Runnable);
                Assert.Null(yOLOEnvironmentResult_Unreadable.ModelUltralyticsVersion);
                Assert.NotEmpty(yOLOEnvironmentResult_Unreadable.Warnings ?? []);
            }
            finally
            {
                if (File.Exists(path_Model_Unreadable))
                {
                    File.Delete(path_Model_Unreadable);
                }
            }
        }
    }
}
