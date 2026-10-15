using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.LayaClassificationResults(Classes.LayaPredictionResult?, IEnumerable{Classes.LayaLabelSet}?, Classes.LayaClassificationOptions?)"/> scores every label from canned responses and suggests by the documented rules, at their boundaries: a single-choice set suggests the most probable label by default, the earlier label on a tie, and exactly <see cref="Classes.LayaLabelSetThreshold.TopCount"/> labels when set; a multi-label set suggests a label at exactly the threshold and not one just below it, at 0.5 by default and at a set threshold; an answer whose confidence is just below <see cref="Classes.LayaClassificationOptions.MinimumConfidence"/> suggests nothing but keeps its scores, while one exactly at it still suggests.
        /// <para>Also verifies that a label-level threshold wins over the set's for that label alone (0.7 suggested, 0.6999 not, beside a set threshold of 0.5), that another label of the set keeps the set's threshold, that a label-level rule without a threshold falls back to the set's, that without a set-level rule the other labels use 0.5, and that a label-level rule on a single-choice set leaves <see cref="Classes.LayaLabelSetThreshold.TopCount"/> alone.</para>
        /// <para>Also verifies that a missing answer, a label missing from a choice answer and a truncated request are named in the messages while the readable scores are kept, that the model hash is carried, and that <c>null</c> inputs give no results.</para>
        /// </summary>
        [Fact]
        public void Query_LayaClassificationResults()
        {
            Classes.LayaLabelSet layaLabelSet_Category = new("category", "Which course?", Enums.LayaLabelSetKind.SingleChoice, [new Classes.LayaLabel("soup", null, null), new Classes.LayaLabel("main", null, null), new Classes.LayaLabel("dessert", null, null)]);
            Classes.LayaLabelSet layaLabelSet_Tags = new("tags", "The recipe is {label}.", Enums.LayaLabelSetKind.MultiLabel, [new Classes.LayaLabel("vegan", null, null), new Classes.LayaLabel("spicy", null, null), new Classes.LayaLabel("quick", null, null)]);
            List<Classes.LayaLabelSet> layaLabelSets = [layaLabelSet_Category, layaLabelSet_Tags];

            static Classes.LayaChoiceAnswer Choice(double confidence, double soup, double main, double dessert)
            {
                return new Classes.LayaChoiceAnswer("category", confidence, null, null, [new Classes.LayaProbability("soup", soup), new Classes.LayaProbability("main", main), new Classes.LayaProbability("dessert", dessert)]);
            }

            static Classes.LayaNoulAnswer Noul(string labelId, double probability)
            {
                return new Classes.LayaNoulAnswer(string.Concat("tags:", labelId), System.Math.Max(probability, 1 - probability), null, probability);
            }

            Classes.LayaPredictionResult layaPredictionResult = new(
                [
                    //Default rules: the top label; at exactly 0.5 suggested, just below not
                    new Classes.LayaResponse("item-1", [Choice(0.9, 0.2, 0.7, 0.1), Noul("vegan", 0.5), Noul("spicy", 0.4999), Noul("quick", 0.9)], 100, false),
                    //A tie between soup and main goes to soup, the earlier label
                    new Classes.LayaResponse("item-2", [Choice(0.9, 0.45, 0.45, 0.1), Noul("vegan", 0.1), Noul("spicy", 0.2), Noul("quick", 0.3)], 100, false),
                    //No category answer, a missing tag answer, and a truncated request
                    new Classes.LayaResponse("item-3", [Noul("vegan", 0.8), Noul("spicy", 0.8)], 1024, true),
                    //A choice answer without a probability for dessert
                    new Classes.LayaResponse("item-4", [new Classes.LayaChoiceAnswer("category", 0.9, null, "soup", [new Classes.LayaProbability("soup", 0.6), new Classes.LayaProbability("main", 0.4)]), Noul("vegan", 0.9), Noul("spicy", 0.9), Noul("quick", 0.9)], 100, false)
                ],
                "model-hash", null, null, 0, false, false, false, [], []);

            List<Classes.LayaClassificationResult> layaClassificationResults = Query.LayaClassificationResults(layaPredictionResult, layaLabelSets, null);
            Assert.Equal(["item-1", "item-2", "item-3", "item-4"], layaClassificationResults.Select(x => x.ItemId));
            Assert.All(layaClassificationResults, x => Assert.Equal("model-hash", x.ModelSHA256));
            Assert.All(layaClassificationResults, x => Assert.NotNull(x.CreatedAt));

            static string[] Suggested(Classes.LayaClassificationResult layaClassificationResult)
            {
                return [.. layaClassificationResult.Scores!.Where(x => x.IsSuggested).Select(x => string.Concat(x.LabelSetId, ":", x.LabelId))];
            }

            Classes.LayaClassificationResult layaClassificationResult_1 = layaClassificationResults[0];
            Assert.Equal(["category:main", "tags:vegan", "tags:quick"], Suggested(layaClassificationResult_1));
            Assert.Equal(6, layaClassificationResult_1.Scores!.Count);
            Assert.Equal(0.4999, layaClassificationResult_1.Scores!.Single(x => x.LabelId == "spicy").Probability);
            Assert.Empty(layaClassificationResult_1.Messages!);
            Assert.All(layaClassificationResult_1.Scores!, x => Assert.False(x.IsForbidden));

            Assert.Equal(["category:soup"], Suggested(layaClassificationResults[1]));

            Classes.LayaClassificationResult layaClassificationResult_3 = layaClassificationResults[2];
            Assert.Equal(["tags:vegan", "tags:spicy"], Suggested(layaClassificationResult_3));
            Assert.Contains(layaClassificationResult_3.Messages!, x => x.Contains("truncated"));
            Assert.Contains(layaClassificationResult_3.Messages!, x => x == "No answer for the label set 'category'.");
            Assert.Contains(layaClassificationResult_3.Messages!, x => x == "No answer for the label 'quick' of the label set 'tags'.");

            Classes.LayaClassificationResult layaClassificationResult_4 = layaClassificationResults[3];
            Assert.Equal(["soup", "main", "vegan", "spicy", "quick"], layaClassificationResult_4.Scores!.Select(x => x.LabelId));
            Assert.Contains(layaClassificationResult_4.Messages!, x => x.Contains("no probability for the label 'dessert'"));

            //TopCount 2 suggests exactly two; a set threshold of 0.9 takes 0.9 and not 0.8999
            Classes.LayaClassificationOptions layaClassificationOptions = new()
            {
                Thresholds = [new Classes.LayaLabelSetThreshold("category", null, null, 2), new Classes.LayaLabelSetThreshold("tags", null, 0.9, null)]
            };

            Classes.LayaPredictionResult layaPredictionResult_Rules = new([new Classes.LayaResponse("item-5", [Choice(0.9, 0.5, 0.3, 0.2), Noul("vegan", 0.9), Noul("spicy", 0.8999), Noul("quick", 0.95)], 100, false)], null, null, null, 0, false, false, false, [], []);
            Assert.Equal(["category:soup", "category:main", "tags:vegan", "tags:quick"], Suggested(Query.LayaClassificationResults(layaPredictionResult_Rules, layaLabelSets, layaClassificationOptions)[0]));

            //A confidence floor: just below abstains (scores kept), exactly at it suggests
            Classes.LayaPredictionResult layaPredictionResult_Floor = new(
                [
                    new Classes.LayaResponse("item-6", [Choice(0.7999, 0.8, 0.1, 0.1), Noul("vegan", 0.79), Noul("spicy", 0.8), Noul("quick", 0.1)], 100, false)
                ],
                null, null, null, 0, false, false, false, [], []);

            Classes.LayaClassificationResult layaClassificationResult_Floor = Query.LayaClassificationResults(layaPredictionResult_Floor, layaLabelSets, new Classes.LayaClassificationOptions() { MinimumConfidence = 0.8 })[0];
            Assert.Equal(["tags:spicy"], Suggested(layaClassificationResult_Floor));
            Assert.Equal(6, layaClassificationResult_Floor.Scores!.Count);
            Assert.Equal(0.8, layaClassificationResult_Floor.Scores!.Single(x => x.LabelId == "soup").Probability);

            //A label-level threshold of 0.7 beside a set-level 0.5: vegan at 0.7 is suggested, spicy at 0.6999 is not, quick keeps 0.5
            Classes.LayaPredictionResult layaPredictionResult_Label = new(
                [
                    new Classes.LayaResponse("item-7", [Choice(0.9, 0.5, 0.3, 0.2), Noul("vegan", 0.7), Noul("spicy", 0.6999), Noul("quick", 0.5)], 100, false),
                    new Classes.LayaResponse("item-8", [Choice(0.9, 0.5, 0.3, 0.2), Noul("vegan", 0.6999), Noul("spicy", 0.7), Noul("quick", 0.4999)], 100, false)
                ],
                null, null, null, 0, false, false, false, [], []);

            Classes.LayaClassificationOptions layaClassificationOptions_Label = new()
            {
                Thresholds = [new Classes.LayaLabelSetThreshold("tags", null, 0.5, null), new Classes.LayaLabelSetThreshold("tags", "vegan", 0.7, null), new Classes.LayaLabelSetThreshold("tags", "spicy", 0.7, null)]
            };

            List<Classes.LayaClassificationResult> layaClassificationResults_Label = Query.LayaClassificationResults(layaPredictionResult_Label, layaLabelSets, layaClassificationOptions_Label);
            Assert.Equal(["category:soup", "tags:vegan", "tags:quick"], Suggested(layaClassificationResults_Label[0]));
            Assert.Equal(["category:soup", "tags:spicy"], Suggested(layaClassificationResults_Label[1]));

            //The label-level rule is the label's alone and needs no set-level rule: the other labels use 0.5
            Classes.LayaClassificationOptions layaClassificationOptions_LabelOnly = new() { Thresholds = [new Classes.LayaLabelSetThreshold("tags", "vegan", 0.7, null)] };
            Assert.Equal(["category:soup", "tags:vegan", "tags:spicy", "tags:quick"], Suggested(Query.LayaClassificationResults(layaPredictionResult_Label, layaLabelSets, layaClassificationOptions_LabelOnly)[0]));
            Assert.Equal(["category:soup", "tags:spicy"], Suggested(Query.LayaClassificationResults(layaPredictionResult_Label, layaLabelSets, layaClassificationOptions_LabelOnly)[1]));

            //A label-level rule without a threshold falls back to the set's 0.9
            Classes.LayaClassificationOptions layaClassificationOptions_Fallback = new() { Thresholds = [new Classes.LayaLabelSetThreshold("tags", null, 0.9, null), new Classes.LayaLabelSetThreshold("tags", "vegan", null, null)] };
            Assert.Equal(["category:soup"], Suggested(Query.LayaClassificationResults(layaPredictionResult_Label, layaLabelSets, layaClassificationOptions_Fallback)[0]));

            //A label-level rule on a single-choice set does not change how many labels are suggested
            Classes.LayaClassificationOptions layaClassificationOptions_Single = new() { Thresholds = [new Classes.LayaLabelSetThreshold("category", null, null, 2), new Classes.LayaLabelSetThreshold("category", "main", 0.1, 3)] };
            Assert.Equal(["category:soup", "category:main", "tags:vegan", "tags:spicy", "tags:quick"], Suggested(Query.LayaClassificationResults(layaPredictionResult_Label, layaLabelSets, layaClassificationOptions_Single)[0]));

            Assert.Empty(Query.LayaClassificationResults(null, layaLabelSets, null));
            Assert.Empty(Query.LayaClassificationResults(layaPredictionResult, null, null));
        }
    }
}
