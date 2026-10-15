using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies <see cref="Modify.ClassifyAsync(IEnumerable{Classes.LayaItem}?, IEnumerable{Classes.LayaLabelSet}?, Classes.LayaClassificationOptions?, Classes.LayaPredictionOptions?, IEnumerable{Classes.LayaLabelConstraint}?, System.Threading.CancellationToken)"/> end to end with a fake interpreter (a .cmd copying prepared responses into place) standing in for CPython: a run where every item is answered gives exit code 0, no run messages and the results in item order; in a mixed run an answered item is scored, suggested and constrained, while an item laya refused and an item without a response still get results, with no scores and only their own findings, taken from <see cref="Classes.LayaPredictionResult.RequestMessages"/> - including an item whose id holds a quote character; a single-choice set of 21 labels runs with one run-level warning that no item repeats.
        /// <para>Also verifies that a failing interpreter gives a non-zero exit code and the process exit and its standard error tail as run messages, with every item result unscored and carrying none of them; that label-level thresholds naming an unknown set or an unknown label run with one run-level warning each, that one naming a known label gives none, and that a set-level rule is never warned about; that label sets Laya cannot be asked - a colon in a set id, a repeated set id, a set without labels, a single-choice set of 101 labels - refuse the run before a process starts, with the reasons as run messages, exit code -1 and no item results; and that <c>null</c> items, sets or prediction options give <c>null</c>.</para>
        /// </summary>
        [Fact]
        public async Task Modify_ClassifyAsync_Responses()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                return;
            }

            string directory = Path.Combine(Path.GetTempPath(), string.Concat("Laya_ClassifyAsync_", Guid.NewGuid().ToString("N")));

            try
            {
                string directory_Checkpoint = Path.Combine(directory, "checkpoint");
                Directory.CreateDirectory(directory_Checkpoint);
                File.WriteAllText(Path.Combine(directory_Checkpoint, "rl_agent_config.json"), "{}");
                string? modelSHA256 = Python.Query.DirectorySHA256(directory_Checkpoint);

                List<string> labelIds = [.. Enumerable.Range(0, 21).Select(x => string.Concat("l", x.ToString("00", CultureInfo.InvariantCulture)))];
                Classes.LayaLabelSet layaLabelSet_Category = new("category", "Which category?", Enums.LayaLabelSetKind.SingleChoice, labelIds.Select(x => new Classes.LayaLabel(x, null, null)));
                Classes.LayaLabelSet layaLabelSet_Tags = new("tags", "The recipe is {label}.", Enums.LayaLabelSetKind.MultiLabel, [new Classes.LayaLabel("vegan", "vegan", null), new Classes.LayaLabel("quick", "quick", null)]);
                List<Classes.LayaLabelSet> layaLabelSets = [layaLabelSet_Category, layaLabelSet_Tags];

                List<Classes.LayaItem> layaItems = [new Classes.LayaItem("r1", "Lentil soup.", false), new Classes.LayaItem("r2", "Cheesecake.", false), new Classes.LayaItem("r3", "Goulash.", false), new Classes.LayaItem("r'4", "Shepherd's pie.", false)];

                //r1 answered, r2 and r'4 refused by laya, r3 without a line
                JsonObject jsonObject_Probabilities = [];
                foreach (string labelId in labelIds)
                {
                    jsonObject_Probabilities[labelId] = labelId == "l03" ? 0.8 : 0.01;
                }

                JsonObject jsonObject_Response = new()
                {
                    ["id"] = "r1",
                    ["answers"] = new JsonObject
                    {
                        ["category"] = new JsonObject { ["type"] = "choice", ["choice"] = "l03", ["probabilities"] = jsonObject_Probabilities, ["confidence"] = 0.9 },
                        ["tags:vegan"] = new JsonObject { ["type"] = "noul", ["noul"] = 0.8, ["confidence"] = 0.8 },
                        ["tags:quick"] = new JsonObject { ["type"] = "noul", ["noul"] = 0.7, ["confidence"] = 0.7 }
                    },
                    ["usage"] = new JsonObject { ["input_tokens"] = 40, ["truncated"] = false }
                };

                string Copy(string name, IEnumerable<string> lines)
                {
                    string path_Prepared = Path.Combine(directory, string.Concat(name, ".jsonl"));
                    File.WriteAllLines(path_Prepared, lines);

                    string path_Copy = Path.Combine(directory, string.Concat(name, ".cmd"));
                    File.WriteAllText(path_Copy, string.Concat("@copy /y \"", path_Prepared, "\" \"", Path.Combine(directory, name, "responses.jsonl"), "\" >nul\r\n@exit /b 0\r\n"));
                    return path_Copy;
                }

                string path_Answered = Copy("answered", [jsonObject_Response.ToJsonString()]);
                string path_Mixed = Copy("mixed", [jsonObject_Response.ToJsonString(), "{\"id\": \"r2\", \"error\": \"ValueError: refused\"}", "{\"id\": \"r'4\", \"error\": \"ValueError: too long\"}"]);

                Classes.LayaPredictionOptions Options(string pythonPath, string name)
                {
                    return new Classes.LayaPredictionOptions() { PythonPath = pythonPath, ModelPath = directory_Checkpoint, WorkingDirectory = Path.Combine(directory, name) };
                }

                List<Classes.LayaLabelConstraint> layaLabelConstraints = [new Classes.LayaLabelConstraint("r1", "tags", "vegan", Enums.LayaConstraintEffect.Forbid, "contains butter")];

                //Every item answered: exit code 0, no run messages, results in item order
                Classes.LayaClassificationRunResult? layaClassificationRunResult_Answered = await new List<Classes.LayaItem>() { layaItems[0] }.ClassifyAsync([layaLabelSet_Tags], null, Options(path_Answered, "answered"));
                Assert.NotNull(layaClassificationRunResult_Answered);
                Assert.Equal(0, layaClassificationRunResult_Answered!.ExitCode);
                Assert.False(layaClassificationRunResult_Answered.Stalled);
                Assert.False(layaClassificationRunResult_Answered.TimedOut);
                Assert.False(layaClassificationRunResult_Answered.Cancelled);
                Assert.NotNull(layaClassificationRunResult_Answered.Duration);
                Assert.Equal(modelSHA256, layaClassificationRunResult_Answered.ModelSHA256);
                Assert.Null(layaClassificationRunResult_Answered.CalibrationSHA256);
                Assert.Empty(layaClassificationRunResult_Answered.Messages!);
                Assert.Equal(["r1"], layaClassificationRunResult_Answered.Results!.Select(x => x.ItemId));
                Assert.Empty(layaClassificationRunResult_Answered.Results![0].Messages!);
                Assert.Equal(["vegan", "quick"], layaClassificationRunResult_Answered.Results![0].Scores!.Where(x => x.IsSuggested).Select(x => x.LabelId));

                //A mixed run: the 21-label warning once at run level, each unanswered item told only its own findings
                Classes.LayaClassificationRunResult? layaClassificationRunResult = await layaItems.ClassifyAsync(layaLabelSets, null, Options(path_Mixed, "mixed"), layaLabelConstraints);
                Assert.NotNull(layaClassificationRunResult);
                Assert.Equal(0, layaClassificationRunResult!.ExitCode);
                Assert.Equal(modelSHA256, layaClassificationRunResult.ModelSHA256);
                Assert.Contains("has 21 labels", Assert.Single(layaClassificationRunResult.Messages!));

                List<Classes.LayaClassificationResult> layaClassificationResults = layaClassificationRunResult.Results!;
                Assert.Equal(["r1", "r2", "r3", "r'4"], layaClassificationResults.Select(x => x.ItemId));
                Assert.All(layaClassificationResults, x => Assert.Equal(modelSHA256, x.ModelSHA256));
                Assert.All(layaClassificationResults, x => Assert.DoesNotContain(x.Messages!, y => y.Contains("has 21 labels")));

                Classes.LayaClassificationResult layaClassificationResult_1 = layaClassificationResults[0];
                Assert.Equal(23, layaClassificationResult_1.Scores!.Count);
                Assert.Equal(["l03", "quick"], layaClassificationResult_1.Scores!.Where(x => x.IsSuggested).Select(x => x.LabelId));
                Classes.LayaLabelScore layaLabelScore_Vegan = layaClassificationResult_1.Scores!.Single(x => x.LabelId == "vegan");
                Assert.True(layaLabelScore_Vegan.IsForbidden);
                Assert.False(layaLabelScore_Vegan.IsSuggested);
                Assert.Equal("contains butter", layaLabelScore_Vegan.Reason);
                Assert.Empty(layaClassificationResult_1.Messages!);

                Classes.LayaClassificationResult layaClassificationResult_2 = layaClassificationResults[1];
                Assert.Empty(layaClassificationResult_2.Scores!);
                Assert.Contains("'r2' was refused by Laya: ValueError: refused", Assert.Single(layaClassificationResult_2.Messages!));

                Classes.LayaClassificationResult layaClassificationResult_3 = layaClassificationResults[2];
                Assert.Empty(layaClassificationResult_3.Scores!);
                Assert.Contains("'r3' got no response", Assert.Single(layaClassificationResult_3.Messages!));

                //The quote in the id cut the old quoted-text parsing short at "r", so this message used to reach no item
                Classes.LayaClassificationResult layaClassificationResult_4 = layaClassificationResults[3];
                Assert.Empty(layaClassificationResult_4.Scores!);
                Assert.Contains("'r'4' was refused by Laya: ValueError: too long", Assert.Single(layaClassificationResult_4.Messages!));

                //A failing interpreter: the process exit and its standard error are run-level, no item repeats them
                string path_Fail = Path.Combine(directory, "fail.cmd");
                File.WriteAllText(path_Fail, "@echo CUDA out of memory 1>&2\r\n@exit /b 3\r\n");

                Classes.LayaClassificationRunResult? layaClassificationRunResult_Failed = await layaItems.ClassifyAsync([layaLabelSet_Tags], null, Options(path_Fail, "failed"));
                Assert.NotNull(layaClassificationRunResult_Failed);
                Assert.Equal(3, layaClassificationRunResult_Failed!.ExitCode);
                Assert.NotNull(layaClassificationRunResult_Failed.Duration);
                Assert.False(layaClassificationRunResult_Failed.TimedOut);
                Assert.Contains(layaClassificationRunResult_Failed.Messages!, x => x.Contains("exited with code 3"));
                Assert.Contains(layaClassificationRunResult_Failed.Messages!, x => x.Contains("CUDA out of memory"));
                Assert.Equal(["r1", "r2", "r3", "r'4"], layaClassificationRunResult_Failed.Results!.Select(x => x.ItemId));
                Assert.All(layaClassificationRunResult_Failed.Results!, x => Assert.Empty(x.Scores!));
                Assert.All(layaClassificationRunResult_Failed.Results!, x => Assert.Equal("The item got no response.", Assert.Single(x.Messages!)));

                //Label-level thresholds: an unknown set and an unknown label warn once at run level, a known label and a set-level rule do not
                Classes.LayaClassificationOptions layaClassificationOptions = new()
                {
                    Thresholds =
                    [
                        new Classes.LayaLabelSetThreshold("tags", "quick", 0.7, null),
                        new Classes.LayaLabelSetThreshold("tags", "ghost", 0.7, null),
                        new Classes.LayaLabelSetThreshold("nowhere", "quick", 0.7, null),
                        new Classes.LayaLabelSetThreshold("nowhere", null, 0.7, null)
                    ]
                };

                Classes.LayaClassificationRunResult? layaClassificationRunResult_Thresholds = await layaItems.ClassifyAsync(layaLabelSets, layaClassificationOptions, Options(path_Mixed, "mixed"), layaLabelConstraints);
                List<string> messages_Thresholds = layaClassificationRunResult_Thresholds!.Messages!;
                Assert.Single(messages_Thresholds, x => x.Contains("names a label that the label set 'tags' does not hold") && x.Contains("'ghost'"));
                Assert.Single(messages_Thresholds, x => x.Contains("names the label set 'nowhere'"));
                Assert.DoesNotContain(messages_Thresholds, x => x.Contains("for the label 'quick'") && !x.Contains("'nowhere'"));
                Assert.Equal(3, messages_Thresholds.Count);
                List<Classes.LayaClassificationResult> layaClassificationResults_Thresholds = layaClassificationRunResult_Thresholds.Results!;
                Assert.Equal(4, layaClassificationResults_Thresholds.Count);
                Assert.All(layaClassificationResults_Thresholds, x => Assert.DoesNotContain(x.Messages!, y => y.Contains("threshold")));
                Assert.Equal(["l03"], layaClassificationResults_Thresholds[0].Scores!.Where(x => x.IsSuggested && x.LabelSetId == "category").Select(x => x.LabelId));
                Assert.Equal(["l03", "quick"], layaClassificationResults_Thresholds[0].Scores!.Where(x => x.IsSuggested).Select(x => x.LabelId));

                //Label sets Laya cannot be asked: refused before any process, the reasons run-level and no item results
                string directory_Refused = Path.Combine(directory, "refused");
                List<Classes.LayaLabelSet> layaLabelSets_Invalid =
                [
                    new Classes.LayaLabelSet("bad:id", "Which?", Enums.LayaLabelSetKind.SingleChoice, [new Classes.LayaLabel("a", null, null)]),
                    new Classes.LayaLabelSet("tags", "It is {label}.", Enums.LayaLabelSetKind.MultiLabel, [new Classes.LayaLabel("x", null, null)]),
                    new Classes.LayaLabelSet("tags", "It is {label}.", Enums.LayaLabelSetKind.MultiLabel, [new Classes.LayaLabel("y", null, null)]),
                    new Classes.LayaLabelSet("empty", "Which?", Enums.LayaLabelSetKind.SingleChoice, []),
                    new Classes.LayaLabelSet("huge", "Which?", Enums.LayaLabelSetKind.SingleChoice, Enumerable.Range(0, 101).Select(x => new Classes.LayaLabel(x.ToString(CultureInfo.InvariantCulture), null, null)))
                ];

                Classes.LayaClassificationRunResult? layaClassificationRunResult_Refused = await layaItems.ClassifyAsync(layaLabelSets_Invalid, null, new Classes.LayaPredictionOptions() { PythonPath = path_Mixed, ModelPath = directory_Checkpoint, WorkingDirectory = directory_Refused });
                Assert.NotNull(layaClassificationRunResult_Refused);
                Assert.Equal(-1, layaClassificationRunResult_Refused!.ExitCode);
                Assert.Null(layaClassificationRunResult_Refused.Duration);
                Assert.Null(layaClassificationRunResult_Refused.ModelSHA256);
                Assert.Empty(layaClassificationRunResult_Refused.Results!);
                List<string> messages = layaClassificationRunResult_Refused.Messages!;
                Assert.Contains(messages, x => x.Contains("'bad:id' contains a colon"));
                Assert.Contains(messages, x => x.Contains("'tags' is used more than once"));
                Assert.Contains(messages, x => x.Contains("'empty' has no labels"));
                Assert.Contains(messages, x => x.Contains("'huge' has 101 labels"));
                Assert.False(Directory.Exists(directory_Refused));

                Assert.Null(await ((IEnumerable<Classes.LayaItem>?)null).ClassifyAsync(layaLabelSets, null, Options(path_Mixed, "mixed")));
                Assert.Null(await layaItems.ClassifyAsync(null, null, Options(path_Mixed, "mixed")));
                Assert.Null(await layaItems.ClassifyAsync(layaLabelSets, null, null));
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
