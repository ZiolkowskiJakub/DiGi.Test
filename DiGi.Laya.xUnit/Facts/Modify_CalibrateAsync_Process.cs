using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies <see cref="Modify.CalibrateAsync(Classes.LayaCalibrationOptions?, System.Threading.CancellationToken)"/> around the process with fake interpreters (.cmd files) standing in for CPython: a run that writes the calibration file and prints calibrate.py's report is read into record counts, temperatures by question type and null calibration errors (none held out), with the calibration file and the unchanged checkpoint hashed; a calibration file left by an earlier run is deleted before the process starts, so a failing run leaves none; a run that exits 0 without writing the file is reported.
        /// <para>Also verifies the refusals that start no process: a calibration path inside the checkpoint (it would change the checkpoint's SHA-256), no calibration path, a missing labelled file, and a model name instead of a checkpoint directory; <c>null</c> options give <c>null</c>.</para>
        /// </summary>
        [Fact]
        public async Task Modify_CalibrateAsync_Process()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                return;
            }

            string directory = Path.Combine(Path.GetTempPath(), string.Concat("Laya_CalibrateAsync_", Guid.NewGuid().ToString("N")));

            try
            {
                string directory_Model = Path.Combine(directory, "model");
                Directory.CreateDirectory(directory_Model);
                File.WriteAllText(Path.Combine(directory_Model, "rl_agent_config.json"), "{}");
                string sha256_Model = Python.Query.DirectorySHA256(directory_Model)!;

                string path_Labelled = Path.Combine(directory, "labelled.jsonl");
                File.WriteAllText(path_Labelled, "{}\n");

                string path_Calibration = Path.Combine(directory, "calibration", "multilingual.calibration.json");
                Directory.CreateDirectory(Path.GetDirectoryName(path_Calibration)!);

                string Interpreter(string name, bool write, int exitCode)
                {
                    string path = Path.Combine(directory, string.Concat(name, ".cmd"));
                    string text = string.Concat(
                        "@echo off\r\n",
                        write ? string.Concat("echo {\"temperature\": [0.5, 5.0, 2.25]}> \"", path_Calibration, "\"\r\n") : string.Empty,
                        exitCode == 0 ? "echo LAYA_CALIBRATE_JSON_BEGIN\r\necho {\"records\": 70, \"skipped\": 1, \"temperature\": [0.5, 5.0, 2.25], \"temperature_by_options\": {}, \"ece_before\": null, \"ece_after\": null, \"evaluation_records\": 0, \"seconds\": 1.5}\r\necho LAYA_CALIBRATE_JSON_END\r\n" : "echo laya refused the data 1>&2\r\n",
                        "exit /b ", exitCode.ToString(System.Globalization.CultureInfo.InvariantCulture), "\r\n");
                    File.WriteAllText(path, text);
                    return path;
                }

                Classes.LayaCalibrationOptions Options(string pythonPath)
                {
                    return new Classes.LayaCalibrationOptions() { PythonPath = pythonPath, ModelPath = directory_Model, CalibrationFilePath = path_Labelled, CalibrationPath = path_Calibration };
                }

                Classes.LayaCalibrationResult? layaCalibrationResult = await Options(Interpreter("ok", true, 0)).CalibrateAsync();
                Assert.NotNull(layaCalibrationResult);
                Assert.Equal(0, layaCalibrationResult!.ExitCode);
                Assert.Empty(layaCalibrationResult.Messages!);
                Assert.Equal(Path.GetFullPath(path_Calibration), layaCalibrationResult.CalibrationPath);
                Assert.Equal(Python.Query.FileSHA256(path_Calibration), layaCalibrationResult.CalibrationSHA256);
                Assert.Equal(sha256_Model, layaCalibrationResult.ModelSHA256);
                Assert.Equal(sha256_Model, Python.Query.DirectorySHA256(directory_Model));
                Assert.Equal(70, layaCalibrationResult.RecordCount);
                Assert.Equal(1, layaCalibrationResult.SkippedCount);
                Assert.Equal([0.5, 5.0, 2.25], layaCalibrationResult.Temperatures!.Select(x => x.Value));
                Assert.Equal(Enums.LayaQuestionType.Noul, layaCalibrationResult.Temperatures![2].QuestionType);
                Assert.Null(layaCalibrationResult.ExpectedCalibrationErrorBefore);
                Assert.Null(layaCalibrationResult.ExpectedCalibrationErrorAfter);
                Assert.Equal(0, layaCalibrationResult.EvaluationRecordCount);

                //The file from the run above is stale for the next one: a failing run removes it and writes none
                Assert.True(File.Exists(path_Calibration));
                Classes.LayaCalibrationResult? layaCalibrationResult_Failed = await Options(Interpreter("failed", false, 2)).CalibrateAsync();
                Assert.Equal(2, layaCalibrationResult_Failed!.ExitCode);
                Assert.False(File.Exists(path_Calibration));
                Assert.Null(layaCalibrationResult_Failed.CalibrationSHA256);
                Assert.Contains(layaCalibrationResult_Failed.Messages!, x => x == "calibrate.py exited with code 2.");
                Assert.Contains(layaCalibrationResult_Failed.Messages!, x => x.Contains("laya refused the data"));

                Classes.LayaCalibrationResult? layaCalibrationResult_NoFile = await Options(Interpreter("nofile", false, 0)).CalibrateAsync();
                Assert.Equal(0, layaCalibrationResult_NoFile!.ExitCode);
                Assert.Null(layaCalibrationResult_NoFile.CalibrationSHA256);
                Assert.Contains(layaCalibrationResult_NoFile.Messages!, x => x.Contains("wrote no calibration file"));

                //Refusals start no process
                string path_Marker = Path.Combine(directory, "marker.txt");
                string path_MarkerInterpreter = Path.Combine(directory, "marker.cmd");
                File.WriteAllText(path_MarkerInterpreter, string.Concat("@echo ran> \"", path_Marker, "\"\r\n@exit /b 0\r\n"));

                async Task AssertRefused(Classes.LayaCalibrationOptions layaCalibrationOptions_Refused, string message)
                {
                    Classes.LayaCalibrationResult? layaCalibrationResult_Refused = await layaCalibrationOptions_Refused.CalibrateAsync();
                    Assert.Equal(-1, layaCalibrationResult_Refused!.ExitCode);
                    Assert.Null(layaCalibrationResult_Refused.Duration);
                    Assert.Contains(layaCalibrationResult_Refused.Messages!, x => x.Contains(message));
                    Assert.False(File.Exists(path_Marker));
                }

                await AssertRefused(new Classes.LayaCalibrationOptions(Options(path_MarkerInterpreter)) { CalibrationPath = Path.Combine(directory_Model, "calibration.json") }, "inside the checkpoint");
                await AssertRefused(new Classes.LayaCalibrationOptions(Options(path_MarkerInterpreter)) { CalibrationPath = null }, "No calibration path");
                await AssertRefused(new Classes.LayaCalibrationOptions(Options(path_MarkerInterpreter)) { CalibrationFilePath = Path.Combine(directory, "missing.jsonl") }, "labelled file");
                await AssertRefused(new Classes.LayaCalibrationOptions(Options(path_MarkerInterpreter)) { ModelPath = "multilingual" }, "Hub id is not accepted");
                Assert.False(File.Exists(Path.Combine(directory_Model, "calibration.json")));

                Assert.Null(await ((Classes.LayaCalibrationOptions?)null).CalibrateAsync());
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }
    }
}
