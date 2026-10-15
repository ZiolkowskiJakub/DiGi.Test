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
        /// Verifies <see cref="Modify.ClassifyAsync(IEnumerable{Classes.LayaItem}?, IEnumerable{Classes.LayaLabelSet}?, Classes.LayaClassificationOptions?, Classes.LayaPredictionOptions?, IEnumerable{Classes.LayaLabelConstraint}?, System.Threading.CancellationToken)"/> end to end with a fake interpreter (a .cmd copying prepared responses into place) standing in for CPython: an answered item is scored, suggested and constrained; an item laya refused and an item without a response still get results, with no scores and the prediction run's reason naming them; a single-choice set of 21 labels runs with a warning on every result.
        /// <para>Also verifies that label sets Laya cannot be asked - a colon in a set id, a repeated set id, a set without labels, a single-choice set of 101 labels - refuse the run before a process starts, giving every item a result with all the reasons, and that <c>null</c> items, sets or prediction options give <c>null</c>.</para>
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

                List<string> labelIds = [.. Enumerable.Range(0, 21).Select(x => string.Concat("l", x.ToString("00", CultureInfo.InvariantCulture)))];
                Classes.LayaLabelSet layaLabelSet_Category = new("category", "Which category?", Enums.LayaLabelSetKind.SingleChoice, labelIds.Select(x => new Classes.LayaLabel(x, null, null)));
                Classes.LayaLabelSet layaLabelSet_Tags = new("tags", "The recipe is {label}.", Enums.LayaLabelSetKind.MultiLabel, [new Classes.LayaLabel("vegan", "vegan", null), new Classes.LayaLabel("quick", "quick", null)]);
                List<Classes.LayaLabelSet> layaLabelSets = [layaLabelSet_Category, layaLabelSet_Tags];

                List<Classes.LayaItem> layaItems = [new Classes.LayaItem("r1", "Lentil soup.", false), new Classes.LayaItem("r2", "Cheesecake.", false), new Classes.LayaItem("r3", "Goulash.", false)];

                //r1 answered, r2 refused by laya, r3 without a line
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

                string path_Prepared = Path.Combine(directory, "prepared.jsonl");
                File.WriteAllLines(path_Prepared, [jsonObject_Response.ToJsonString(), "{\"id\": \"r2\", \"error\": \"ValueError: refused\"}"]);

                string directory_Working = Path.Combine(directory, "work");
                string path_Copy = Path.Combine(directory, "copy.cmd");
                File.WriteAllText(path_Copy, string.Concat("@copy /y \"", path_Prepared, "\" \"", Path.Combine(directory_Working, "responses.jsonl"), "\" >nul\r\n@exit /b 0\r\n"));

                Classes.LayaPredictionOptions layaPredictionOptions = new() { PythonPath = path_Copy, ModelPath = directory_Checkpoint, WorkingDirectory = directory_Working };
                List<Classes.LayaLabelConstraint> layaLabelConstraints = [new Classes.LayaLabelConstraint("r1", "tags", "vegan", Enums.LayaConstraintEffect.Forbid, "contains butter")];

                List<Classes.LayaClassificationResult>? layaClassificationResults = await layaItems.ClassifyAsync(layaLabelSets, null, layaPredictionOptions, layaLabelConstraints);
                Assert.NotNull(layaClassificationResults);
                Assert.Equal(["r1", "r2", "r3"], layaClassificationResults!.Select(x => x.ItemId));
                Assert.All(layaClassificationResults, x => Assert.Equal(Python.Query.DirectorySHA256(directory_Checkpoint), x.ModelSHA256));
                Assert.All(layaClassificationResults, x => Assert.Contains(x.Messages!, y => y.Contains("has 21 labels")));

                Classes.LayaClassificationResult layaClassificationResult_1 = layaClassificationResults[0];
                Assert.Equal(23, layaClassificationResult_1.Scores!.Count);
                Assert.Equal(["l03", "quick"], layaClassificationResult_1.Scores!.Where(x => x.IsSuggested).Select(x => x.LabelId));
                Classes.LayaLabelScore layaLabelScore_Vegan = layaClassificationResult_1.Scores!.Single(x => x.LabelId == "vegan");
                Assert.True(layaLabelScore_Vegan.IsForbidden);
                Assert.False(layaLabelScore_Vegan.IsSuggested);
                Assert.Equal("contains butter", layaLabelScore_Vegan.Reason);
                Assert.Single(layaClassificationResult_1.Messages!);

                Classes.LayaClassificationResult layaClassificationResult_2 = layaClassificationResults[1];
                Assert.Empty(layaClassificationResult_2.Scores!);
                Assert.Contains(layaClassificationResult_2.Messages!, x => x.Contains("'r2' was refused by Laya: ValueError: refused"));
                Assert.DoesNotContain(layaClassificationResult_2.Messages!, x => x.Contains("'r3'"));

                Classes.LayaClassificationResult layaClassificationResult_3 = layaClassificationResults[2];
                Assert.Empty(layaClassificationResult_3.Scores!);
                Assert.Contains(layaClassificationResult_3.Messages!, x => x.Contains("'r3' got no response"));
                Assert.DoesNotContain(layaClassificationResult_3.Messages!, x => x.Contains("'r2'"));

                //Label sets Laya cannot be asked: refused before any process, every item told every reason
                string directory_Refused = Path.Combine(directory, "refused");
                List<Classes.LayaLabelSet> layaLabelSets_Invalid =
                [
                    new Classes.LayaLabelSet("bad:id", "Which?", Enums.LayaLabelSetKind.SingleChoice, [new Classes.LayaLabel("a", null, null)]),
                    new Classes.LayaLabelSet("tags", "It is {label}.", Enums.LayaLabelSetKind.MultiLabel, [new Classes.LayaLabel("x", null, null)]),
                    new Classes.LayaLabelSet("tags", "It is {label}.", Enums.LayaLabelSetKind.MultiLabel, [new Classes.LayaLabel("y", null, null)]),
                    new Classes.LayaLabelSet("empty", "Which?", Enums.LayaLabelSetKind.SingleChoice, []),
                    new Classes.LayaLabelSet("huge", "Which?", Enums.LayaLabelSetKind.SingleChoice, Enumerable.Range(0, 101).Select(x => new Classes.LayaLabel(x.ToString(CultureInfo.InvariantCulture), null, null)))
                ];

                List<Classes.LayaClassificationResult>? layaClassificationResults_Refused = await layaItems.ClassifyAsync(layaLabelSets_Invalid, null, new Classes.LayaPredictionOptions() { PythonPath = path_Copy, ModelPath = directory_Checkpoint, WorkingDirectory = directory_Refused });
                Assert.Equal(3, layaClassificationResults_Refused!.Count);
                Assert.All(layaClassificationResults_Refused, x => Assert.Empty(x.Scores!));
                List<string> messages = layaClassificationResults_Refused[2].Messages!;
                Assert.Contains(messages, x => x.Contains("'bad:id' contains a colon"));
                Assert.Contains(messages, x => x.Contains("'tags' is used more than once"));
                Assert.Contains(messages, x => x.Contains("'empty' has no labels"));
                Assert.Contains(messages, x => x.Contains("'huge' has 101 labels"));
                Assert.False(Directory.Exists(directory_Refused));

                Assert.Null(await ((IEnumerable<Classes.LayaItem>?)null).ClassifyAsync(layaLabelSets, null, layaPredictionOptions));
                Assert.Null(await layaItems.ClassifyAsync(null, null, layaPredictionOptions));
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
