using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Modify.ClassifyAsync(IEnumerable{Classes.LayaItem}?, IEnumerable{Classes.LayaLabelSet}?, Classes.LayaClassificationOptions?, Classes.LayaPredictionOptions?, IEnumerable{Classes.LayaLabelConstraint}?, System.Threading.CancellationToken)"/> classifies two items against a single-choice and a multi-label set with the real interpreter and checkpoint of DiGi.Laya_Preflight.conf: a run result with exit code 0, a duration and no run messages, one result per item without messages, every label scored with a probability between 0 and 1, exactly one suggestion in the single-choice set, and the checkpoint's SHA-256. It asserts the workflow, not the model's judgement.
        /// <para>The interpreter and checkpoint are machine specific, so the fact returns without asserting when the conf, its interpreter or its checkpoint is absent.</para>
        /// <para>Medium test (7 s on an RTX 5090, 2026-10-15): runs when DIGI_TEST_MAX_DURATION is Medium (the default) or Long.</para>
        /// </summary>
        [MediumFact]
        public async Task Modify_ClassifyAsync()
        {
            string directory = Path.Combine(Path.GetTempPath(), string.Concat("Laya_Classify_", Guid.NewGuid().ToString("N")));

            try
            {
                Classes.LayaPredictionOptions? layaPredictionOptions = PreflightOptions(directory);
                if (layaPredictionOptions == null)
                {
                    return;
                }

                List<Classes.LayaLabelSet> layaLabelSets =
                [
                    new Classes.LayaLabelSet("course", "Which course is this recipe?", Enums.LayaLabelSetKind.SingleChoice, [new Classes.LayaLabel("soup", "a soup or broth", null), new Classes.LayaLabel("dessert", "a sweet dessert", null)]),
                    new Classes.LayaLabelSet("tags", "The recipe is {label}.", Enums.LayaLabelSetKind.MultiLabel, [new Classes.LayaLabel("vegan", "vegan", null), new Classes.LayaLabel("sweet", "sweet", null)])
                ];

                List<Classes.LayaItem> layaItems =
                [
                    new Classes.LayaItem("recipe-1", "{\"title\":\"Barszcz czerwony\",\"ingredients\":[\"buraki\",\"bulion warzywny\",\"czosnek\"]}", true),
                    new Classes.LayaItem("recipe-2", "Cheesecake with sugar, cream cheese, eggs and a biscuit base.", false)
                ];

                Classes.LayaClassificationRunResult? layaClassificationRunResult = await layaItems.ClassifyAsync(layaLabelSets, null, layaPredictionOptions);
                Assert.NotNull(layaClassificationRunResult);
                Assert.True(layaClassificationRunResult!.Messages!.Count == 0, string.Join(Environment.NewLine, layaClassificationRunResult.Messages));
                Assert.Equal(0, layaClassificationRunResult.ExitCode);
                Assert.NotNull(layaClassificationRunResult.Duration);
                Assert.Equal(Python.Query.DirectorySHA256(layaPredictionOptions.ModelPath), layaClassificationRunResult.ModelSHA256);

                List<Classes.LayaClassificationResult> layaClassificationResults = layaClassificationRunResult.Results!;
                Assert.Equal(["recipe-1", "recipe-2"], layaClassificationResults.Select(x => x.ItemId));

                foreach (Classes.LayaClassificationResult layaClassificationResult in layaClassificationResults)
                {
                    Assert.True(layaClassificationResult.Messages!.Count == 0, string.Join(Environment.NewLine, layaClassificationResult.Messages));
                    Assert.Equal(Python.Query.DirectorySHA256(layaPredictionOptions.ModelPath), layaClassificationResult.ModelSHA256);
                    Assert.Equal(["course:soup", "course:dessert", "tags:vegan", "tags:sweet"], layaClassificationResult.Scores!.Select(x => string.Concat(x.LabelSetId, ":", x.LabelId)));
                    Assert.All(layaClassificationResult.Scores!, x => Assert.InRange(x.Probability, 0.0, 1.0));
                    Assert.Single(layaClassificationResult.Scores!, x => x.LabelSetId == "course" && x.IsSuggested);
                }
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
