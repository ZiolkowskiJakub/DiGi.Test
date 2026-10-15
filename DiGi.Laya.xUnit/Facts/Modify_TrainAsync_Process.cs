using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies <see cref="Modify.TrainAsync(Classes.LayaTrainingOptions?, System.Threading.CancellationToken)"/> around the process with fake interpreters (.cmd files) standing in for CPython: a run that prints train.py's report and writes a complete checkpoint is trusted - the report read into counts, skipped reasons, epoch losses and temperatures, the new and base checkpoints hashed, train_config.json written from the options; a run without model.safetensors or without a report exits 0 yet is not trusted; a failing run reports its exit code and the end of standard error.
        /// <para>Also verifies the refusals that start no process (a fake interpreter that would leave a marker file never runs): an output directory inside Laya\models, a non-empty one, the base checkpoint or a folder inside it, no output directory, a missing training or evaluation file, and a model name instead of a base checkpoint directory; <c>null</c> options give <c>null</c>.</para>
        /// </summary>
        [Fact]
        public async Task Modify_TrainAsync_Process()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                return;
            }

            string directory = Path.Combine(Path.GetTempPath(), string.Concat("Laya_TrainAsync_", Guid.NewGuid().ToString("N")));

            try
            {
                string directory_Base = Path.Combine(directory, "base");
                Directory.CreateDirectory(directory_Base);
                File.WriteAllText(Path.Combine(directory_Base, "rl_agent_config.json"), "{\"max_len\": 1024}");

                string path_Training = Path.Combine(directory, "training.jsonl");
                File.WriteAllText(path_Training, "{}\n");

                string Interpreter(string name, string output, bool weights, bool report, int exitCode)
                {
                    string path = Path.Combine(directory, string.Concat(name, ".cmd"));
                    string text = string.Concat(
                        "@echo off\r\n",
                        "mkdir \"", output, "\" 2>nul\r\n",
                        "echo {}> \"", Path.Combine(output, "rl_agent_config.json"), "\"\r\n",
                        weights ? string.Concat("echo weights> \"", Path.Combine(output, "model.safetensors"), "\"\r\n") : string.Empty,
                        report ? "echo LAYA_TRAIN_JSON_BEGIN\r\necho {\"train_items\": 12, \"calibration_items\": 3, \"eval_items\": 3, \"skipped\": {\"empty_text\": 2, \"options_collapsed\": 1}, \"epoch_loss\": [0.5, 0.25], \"temperature\": [1.5, 2.0, 0.5], \"seconds\": 1.0}\r\necho LAYA_TRAIN_JSON_END\r\n" : string.Empty,
                        exitCode != 0 ? "echo Traceback: finetune failed 1>&2\r\n" : string.Empty,
                        "exit /b ", exitCode.ToString(System.Globalization.CultureInfo.InvariantCulture), "\r\n");
                    File.WriteAllText(path, text);
                    return path;
                }

                Classes.LayaTrainingOptions Options(string pythonPath, string output)
                {
                    return new Classes.LayaTrainingOptions() { PythonPath = pythonPath, BaseModelPath = directory_Base, TrainingFilePath = path_Training, OutputDirectory = output, Epochs = 2, Loss = Enums.LayaLoss.SoftCrossEntropy };
                }

                //A trusted run
                string directory_Output = Path.Combine(directory, "runs", "ok");
                string directory_Working = Path.Combine(directory, "work");
                Classes.LayaTrainingOptions layaTrainingOptions = Options(Interpreter("ok", directory_Output, true, true, 0), directory_Output);
                layaTrainingOptions.WorkingDirectory = directory_Working;

                Classes.LayaTrainingResult? layaTrainingResult = await layaTrainingOptions.TrainAsync();
                Assert.NotNull(layaTrainingResult);
                Assert.Equal(0, layaTrainingResult!.ExitCode);
                Assert.Empty(layaTrainingResult.Messages!);
                Assert.Equal(Path.GetFullPath(directory_Output), layaTrainingResult.CheckpointDirectory);
                Assert.Equal(Python.Query.DirectorySHA256(directory_Output), layaTrainingResult.ModelSHA256);
                Assert.Equal(Python.Query.DirectorySHA256(directory_Base), layaTrainingResult.BaseModelSHA256);
                Assert.Equal(12, layaTrainingResult.TrainItemCount);
                Assert.Equal(3, layaTrainingResult.CalibrationItemCount);
                Assert.Equal(3, layaTrainingResult.EvaluationItemCount);
                Assert.Equal(["empty_text:2", "options_collapsed:1"], layaTrainingResult.SkippedCounts!.Select(x => string.Concat(x.Reason, ":", x.Count.ToString(System.Globalization.CultureInfo.InvariantCulture))));
                Assert.Equal([0.5, 0.25], layaTrainingResult.EpochLosses!);
                Assert.Equal([Enums.LayaQuestionType.Choice, Enums.LayaQuestionType.Score, Enums.LayaQuestionType.Noul], layaTrainingResult.Temperatures!.Select(x => x.QuestionType));
                Assert.Equal([1.5, 2.0, 0.5], layaTrainingResult.Temperatures!.Select(x => x.Value));
                Assert.NotNull(layaTrainingResult.Duration);
                Assert.True(JsonNode.DeepEquals(layaTrainingOptions.ToLaya_TrainConfiguration(), JsonNode.Parse(File.ReadAllText(Path.Combine(directory_Working, "train_config.json")))));
                Assert.True(File.Exists(Path.Combine(directory_Working, "train.py")));

                //Exit 0, but no weights: not trusted
                string directory_NoWeights = Path.Combine(directory, "runs", "no-weights");
                Classes.LayaTrainingResult? layaTrainingResult_NoWeights = await Options(Interpreter("noweights", directory_NoWeights, false, true, 0), directory_NoWeights).TrainAsync();
                Assert.Equal(0, layaTrainingResult_NoWeights!.ExitCode);
                Assert.Null(layaTrainingResult_NoWeights.ModelSHA256);
                Assert.Equal(12, layaTrainingResult_NoWeights.TrainItemCount);
                Assert.Contains(layaTrainingResult_NoWeights.Messages!, x => x.Contains("holds no complete checkpoint"));

                //Exit 0, weights, but no report: not trusted
                string directory_NoReport = Path.Combine(directory, "runs", "no-report");
                Classes.LayaTrainingResult? layaTrainingResult_NoReport = await Options(Interpreter("noreport", directory_NoReport, true, false, 0), directory_NoReport).TrainAsync();
                Assert.Null(layaTrainingResult_NoReport!.ModelSHA256);
                Assert.Null(layaTrainingResult_NoReport.TrainItemCount);
                Assert.Contains(layaTrainingResult_NoReport.Messages!, x => x.Contains("printed no report"));

                //A failing run
                string directory_Failed = Path.Combine(directory, "runs", "failed");
                Classes.LayaTrainingResult? layaTrainingResult_Failed = await Options(Interpreter("failed", directory_Failed, false, false, 1), directory_Failed).TrainAsync();
                Assert.Equal(1, layaTrainingResult_Failed!.ExitCode);
                Assert.Null(layaTrainingResult_Failed.ModelSHA256);
                Assert.Contains(layaTrainingResult_Failed.Messages!, x => x == "train.py exited with code 1.");
                Assert.Contains(layaTrainingResult_Failed.Messages!, x => x.Contains("finetune failed"));

                //Refusals: the marker interpreter must never run
                string path_Marker = Path.Combine(directory, "marker.txt");
                string path_MarkerInterpreter = Path.Combine(directory, "marker.cmd");
                File.WriteAllText(path_MarkerInterpreter, string.Concat("@echo ran> \"", path_Marker, "\"\r\n@exit /b 0\r\n"));

                async Task AssertRefused(Classes.LayaTrainingOptions layaTrainingOptions_Refused, string message)
                {
                    Classes.LayaTrainingResult? layaTrainingResult_Refused = await layaTrainingOptions_Refused.TrainAsync();
                    Assert.NotNull(layaTrainingResult_Refused);
                    Assert.Equal(-1, layaTrainingResult_Refused!.ExitCode);
                    Assert.Null(layaTrainingResult_Refused.Duration);
                    Assert.Null(layaTrainingResult_Refused.ModelSHA256);
                    Assert.Contains(layaTrainingResult_Refused.Messages!, x => x.Contains(message));
                    Assert.False(File.Exists(path_Marker));
                }

                await AssertRefused(Options(path_MarkerInterpreter, Path.Combine(directory, "host", "Laya", "models", "run")), "Laya\\models");
                await AssertRefused(Options(path_MarkerInterpreter, Path.Combine(directory, "Software", "DiGi.Laya", "models", "recipe_classification")), "DiGi.Laya\\models");
                await AssertRefused(Options(path_MarkerInterpreter, directory_Output), "is not empty");
                await AssertRefused(Options(path_MarkerInterpreter, directory_Base), "is not empty");
                await AssertRefused(Options(path_MarkerInterpreter, Path.Combine(directory_Base, "inner")), "is the base checkpoint or inside it");
                await AssertRefused(Options(path_MarkerInterpreter, " "), "No output directory");
                await AssertRefused(new Classes.LayaTrainingOptions(Options(path_MarkerInterpreter, Path.Combine(directory, "runs", "x"))) { TrainingFilePath = Path.Combine(directory, "missing.jsonl") }, "training file");
                await AssertRefused(new Classes.LayaTrainingOptions(Options(path_MarkerInterpreter, Path.Combine(directory, "runs", "x"))) { EvaluationFilePath = Path.Combine(directory, "missing.jsonl") }, "evaluation file");
                await AssertRefused(new Classes.LayaTrainingOptions(Options(path_MarkerInterpreter, Path.Combine(directory, "runs", "x"))) { BaseModelPath = "multilingual" }, "Hub id is not accepted");
                await AssertRefused(new Classes.LayaTrainingOptions(Options(Path.Combine(directory, "no-python.exe"), Path.Combine(directory, "runs", "x"))), "was not found");

                Assert.Null(await ((Classes.LayaTrainingOptions?)null).TrainAsync());
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
