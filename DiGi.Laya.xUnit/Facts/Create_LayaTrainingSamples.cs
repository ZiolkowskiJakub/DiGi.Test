using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Create.LayaTrainingSamples(IEnumerable{Classes.LayaItem}?, IEnumerable{Classes.LayaLabelSet}?, IEnumerable{Classes.LayaLabelAssignment}?, int)"/> asks exactly the questions inference asks (<see cref="Create.LayaQuestions(Classes.LayaLabelSet?)"/>), gives one choice sample per item with a true single-choice assignment and one noul sample per multi-label assignment, maps a weight below 1 to the soft target 0.5 +/- 0.5 w and a weight of 1 to a hard label, skips weight-0 assignments, unknown items and unknown labels, carries the item state and the assignment group, gives the same list for the same seed and a different order for another, and produces samples every one of which converts to a training line.
        /// </summary>
        [Fact]
        public void Create_LayaTrainingSamples()
        {
            Classes.LayaLabelSet layaLabelSet_Category = new("category", "Which course?", Enums.LayaLabelSetKind.SingleChoice, [new Classes.LayaLabel("soup", "a soup", null), new Classes.LayaLabel("dessert", "a dessert", null)]);
            Classes.LayaLabelSet layaLabelSet_Tags = new("tags", "The recipe is {label}.", Enums.LayaLabelSetKind.MultiLabel, [new Classes.LayaLabel("vegan", "vegan", null), new Classes.LayaLabel("quick", "quick", null)]);
            List<Classes.LayaLabelSet> layaLabelSets = [layaLabelSet_Category, layaLabelSet_Tags];

            List<Classes.LayaItem> layaItems = [new Classes.LayaItem("r1", "Barszcz.", false), new Classes.LayaItem("r2", "{\"title\":\"Sernik\"}", true), new Classes.LayaItem("r3", "Unlabelled.", false)];

            List<Classes.LayaLabelAssignment> layaLabelAssignments =
            [
                new Classes.LayaLabelAssignment("r1", "category", "soup", true, 1.0, "family-1"),
                new Classes.LayaLabelAssignment("r1", "tags", "vegan", true, 1.0, "family-1"),
                new Classes.LayaLabelAssignment("r1", "tags", "quick", false, 0.5, "family-1"),
                new Classes.LayaLabelAssignment("r2", "category", "dessert", false, 1.0, null),
                new Classes.LayaLabelAssignment("r2", "tags", "vegan", true, 0.5, null),
                new Classes.LayaLabelAssignment("r2", "tags", "quick", true, 0.0, null),
                new Classes.LayaLabelAssignment("r2", "tags", "spicy", true, 1.0, null),
                new Classes.LayaLabelAssignment("r9", "category", "soup", true, 1.0, null)
            ];

            List<Classes.LayaTrainingSample> layaTrainingSamples = Create.LayaTrainingSamples(layaItems, layaLabelSets, layaLabelAssignments, 5);

            //r1: category + vegan + quick; r2: vegan only (dessert is not true, quick has weight 0, spicy is not a label); r3 and r9 nothing
            Dictionary<string, Classes.LayaTrainingSample> layaTrainingSamples_ById = layaTrainingSamples.ToDictionary(x => x.Id!);
            Assert.Equal(["r1|category", "r1|tags:quick", "r1|tags:vegan", "r2|tags:vegan"], layaTrainingSamples_ById.Keys.OrderBy(x => x, System.StringComparer.Ordinal));

            Classes.LayaTrainingSample layaTrainingSample_Category = layaTrainingSamples_ById["r1|category"];
            Assert.Equal("soup", layaTrainingSample_Category.Label);
            Assert.Equal("Barszcz.", layaTrainingSample_Category.State);
            Assert.Equal("family-1", layaTrainingSample_Category.Group);
            Assert.Null(layaTrainingSample_Category.Probability);
            Assert.Equal(["soup", "dessert"], Assert.IsType<Classes.LayaChoiceQuestion>(layaTrainingSample_Category.Question).Criteria!.Select(x => x.Label));

            Classes.LayaTrainingSample layaTrainingSample_VeganHard = layaTrainingSamples_ById["r1|tags:vegan"];
            Assert.True(layaTrainingSample_VeganHard.Truth);
            Assert.Null(layaTrainingSample_VeganHard.Probability);
            Assert.Equal("The recipe is vegan.", Assert.IsType<Classes.LayaNoulQuestion>(layaTrainingSample_VeganHard.Question).Instructions);

            Assert.Equal(0.25, layaTrainingSamples_ById["r1|tags:quick"].Probability);
            Assert.False(layaTrainingSamples_ById["r1|tags:quick"].Truth);

            Classes.LayaTrainingSample layaTrainingSample_VeganSoft = layaTrainingSamples_ById["r2|tags:vegan"];
            Assert.Equal(0.75, layaTrainingSample_VeganSoft.Probability);
            Assert.True(layaTrainingSample_VeganSoft.StateIsJson);
            Assert.Null(layaTrainingSample_VeganSoft.Group);

            //The same questions inference asks
            List<string?> names_Inference = [.. Create.LayaRequests(layaItems, layaLabelSets)[0].Questions!.Select(x => x.Name)];
            Assert.All(layaTrainingSamples, x => Assert.Contains(x.Question!.Name, names_Inference));

            Assert.All(layaTrainingSamples, x => Assert.NotNull(x.ToLaya_TrainingLine()));

            //Deterministic per seed; a larger set reveals the shuffle
            List<Classes.LayaItem> layaItems_Many = [.. Enumerable.Range(0, 40).Select(x => new Classes.LayaItem(string.Concat("m", x.ToString(System.Globalization.CultureInfo.InvariantCulture)), "State.", false))];
            List<Classes.LayaLabelAssignment> layaLabelAssignments_Many = [.. layaItems_Many.Select(x => new Classes.LayaLabelAssignment(x.Id, "category", "soup", true, 1.0, null))];
            List<string?> ids_Seed5 = [.. Create.LayaTrainingSamples(layaItems_Many, layaLabelSets, layaLabelAssignments_Many, 5).Select(x => x.Id)];
            Assert.Equal(ids_Seed5, Create.LayaTrainingSamples(layaItems_Many, layaLabelSets, layaLabelAssignments_Many, 5).Select(x => x.Id));
            Assert.NotEqual(ids_Seed5, Create.LayaTrainingSamples(layaItems_Many, layaLabelSets, layaLabelAssignments_Many, 6).Select(x => x.Id));
            Assert.Equal(40, ids_Seed5.Count);

            Assert.Empty(Create.LayaTrainingSamples(null, layaLabelSets, layaLabelAssignments, 5));
            Assert.Empty(Create.LayaTrainingSamples(layaItems, null, layaLabelAssignments, 5));
            Assert.Empty(Create.LayaTrainingSamples(layaItems, layaLabelSets, null, 5));
        }
    }
}
