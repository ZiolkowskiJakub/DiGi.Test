using System.Collections.Generic;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Modify.ApplyConstraints(Classes.LayaClassificationResult?, IEnumerable{Classes.LayaLabelConstraint}?)"/> marks a forbidden label forbidden and never suggested while keeping its probability and storing the constraint's reason, does not promote the next label of a single-choice set, keeps the first reason when a label is forbidden twice, ignores constraints of other items and of labels the result has no score for, and reports whether anything changed.
        /// </summary>
        [Fact]
        public void Modify_ApplyConstraints()
        {
            Classes.LayaClassificationResult layaClassificationResult = new(
                "recipe-1",
                [
                    new Classes.LayaLabelScore("category", "soup", 0.7, true, false, null),
                    new Classes.LayaLabelScore("category", "main", 0.3, false, false, null),
                    new Classes.LayaLabelScore("tags", "vegan", 0.9, true, false, null),
                    new Classes.LayaLabelScore("tags", "quick", 0.6, true, false, null)
                ],
                null, null, []);

            List<Classes.LayaLabelConstraint> layaLabelConstraints =
            [
                new Classes.LayaLabelConstraint("recipe-1", "tags", "vegan", Enums.LayaConstraintEffect.Forbid, "contains kiełbasa"),
                new Classes.LayaLabelConstraint("recipe-1", "tags", "vegan", Enums.LayaConstraintEffect.Forbid, "second reason"),
                new Classes.LayaLabelConstraint("recipe-1", "category", "soup", Enums.LayaConstraintEffect.Forbid, "not a soup by house rule"),
                new Classes.LayaLabelConstraint("recipe-2", "tags", "quick", Enums.LayaConstraintEffect.Forbid, "another item"),
                new Classes.LayaLabelConstraint("recipe-1", "tags", "spicy", Enums.LayaConstraintEffect.Forbid, "no score for it"),
                null!
            ];

            Assert.True(layaClassificationResult.ApplyConstraints(layaLabelConstraints));

            List<Classes.LayaLabelScore> layaLabelScores = layaClassificationResult.Scores!;
            Assert.Equal(4, layaLabelScores.Count);

            Classes.LayaLabelScore layaLabelScore_Vegan = layaLabelScores.Single(x => x.LabelId == "vegan");
            Assert.True(layaLabelScore_Vegan.IsForbidden);
            Assert.False(layaLabelScore_Vegan.IsSuggested);
            Assert.Equal(0.9, layaLabelScore_Vegan.Probability);
            Assert.Equal("contains kiełbasa", layaLabelScore_Vegan.Reason);

            Classes.LayaLabelScore layaLabelScore_Soup = layaLabelScores.Single(x => x.LabelId == "soup");
            Assert.True(layaLabelScore_Soup.IsForbidden);
            Assert.False(layaLabelScore_Soup.IsSuggested);
            Assert.Equal("not a soup by house rule", layaLabelScore_Soup.Reason);

            //The next label is not promoted, and other scores are untouched
            Classes.LayaLabelScore layaLabelScore_Main = layaLabelScores.Single(x => x.LabelId == "main");
            Assert.False(layaLabelScore_Main.IsSuggested);
            Assert.False(layaLabelScore_Main.IsForbidden);

            Classes.LayaLabelScore layaLabelScore_Quick = layaLabelScores.Single(x => x.LabelId == "quick");
            Assert.True(layaLabelScore_Quick.IsSuggested);
            Assert.False(layaLabelScore_Quick.IsForbidden);
            Assert.Null(layaLabelScore_Quick.Reason);

            //Applying them again changes nothing
            Assert.False(layaClassificationResult.ApplyConstraints(layaLabelConstraints));
            Assert.False(layaClassificationResult.ApplyConstraints(null));
            Assert.False(((Classes.LayaClassificationResult?)null).ApplyConstraints(layaLabelConstraints));
        }
    }
}
