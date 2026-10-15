using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.LayaFolds(IEnumerable{Classes.LayaLabelAssignment}?, int, string?, int)"/> puts every item in exactly one fold, never splits a group, balances each stratum label across the folds to within one unit, gives the same folds for the same seed whatever order the items arrive in by group, different folds for a different seed, and nothing for no assignments or fewer than one fold.
        /// </summary>
        [Fact]
        public void Query_LayaFolds()
        {
            List<Classes.LayaLabelAssignment> layaLabelAssignments = [];
            string[] labelIds = ["soup", "main", "dessert"];
            for (int i = 0; i < 30; i++)
            {
                string itemId = string.Concat("recipe-", i.ToString("00", CultureInfo.InvariantCulture));
                string? group = i < 6 ? string.Concat("family-", (i / 3).ToString(CultureInfo.InvariantCulture)) : null;
                layaLabelAssignments.Add(new Classes.LayaLabelAssignment(itemId, "category", labelIds[i % 3], true, 1.0, group));
                layaLabelAssignments.Add(new Classes.LayaLabelAssignment(itemId, "tags", "vegan", i % 2 == 0, 1.0, null));
            }

            List<List<string>> folds = Query.LayaFolds(layaLabelAssignments, 3, "category", 42);
            Assert.Equal(3, folds.Count);

            //Every item exactly once, each fold sorted
            List<string> itemIds_All = [.. folds.SelectMany(x => x)];
            Assert.Equal(30, itemIds_All.Count);
            Assert.Equal(30, itemIds_All.Distinct(StringComparer.Ordinal).Count());
            Assert.All(folds, x => Assert.Equal(x.OrderBy(y => y, StringComparer.Ordinal), x));

            //A group stays together: family-0 is recipe-00..02, family-1 is recipe-03..05
            Assert.Single(folds, x => x.Contains("recipe-00"));
            Assert.Contains("recipe-01", folds.Single(x => x.Contains("recipe-00")));
            Assert.Contains("recipe-02", folds.Single(x => x.Contains("recipe-00")));
            Assert.Contains("recipe-04", folds.Single(x => x.Contains("recipe-03")));
            Assert.Contains("recipe-05", folds.Single(x => x.Contains("recipe-03")));

            //Units of one stratum spread evenly. Both groups hold one item of each label, so the tie makes their stratum
            //"dessert" (ordinally first) and a group counts as one unit; soup and main are 8 single items each, so 2 or 3 per fold
            Dictionary<string, string> labelIds_ByItem = layaLabelAssignments.Where(x => x.LabelSetId == "category").ToDictionary(x => x.ItemId!, x => x.LabelId!);
            foreach (string labelId in new string[] { "soup", "main" })
            {
                int[] counts = [.. folds.Select(x => x.Count(y => labelIds_ByItem[y] == labelId && string.CompareOrdinal(y, "recipe-06") >= 0))];
                Assert.True(counts.Max() - counts.Min() <= 1, string.Join(",", counts));
            }

            int[] counts_Dessert = [.. folds.Select(x => x.Count(y => labelIds_ByItem[y] == "dessert" && string.CompareOrdinal(y, "recipe-06") >= 0) + (x.Contains("recipe-00") ? 1 : 0) + (x.Contains("recipe-03") ? 1 : 0))];
            Assert.True(counts_Dessert.Max() - counts_Dessert.Min() <= 1, string.Join(",", counts_Dessert));

            //Deterministic per seed, independent of the order the items arrive in, and seed-dependent
            List<Classes.LayaLabelAssignment> layaLabelAssignments_Reordered = [.. layaLabelAssignments.OrderByDescending(x => x.ItemId, StringComparer.Ordinal)];
            Assert.Equal(folds, Query.LayaFolds(layaLabelAssignments_Reordered, 3, "category", 42));
            Assert.Equal(folds, Query.LayaFolds(layaLabelAssignments, 3, "category", 42));
            Assert.NotEqual(folds, Query.LayaFolds(layaLabelAssignments, 3, "category", 7));

            //Without stratification, still every item once
            Assert.Equal(30, Query.LayaFolds(layaLabelAssignments, 4, null, 1).Sum(x => x.Count));

            Assert.Empty(Query.LayaFolds(layaLabelAssignments, 0, "category", 42));
            Assert.Empty(Query.LayaFolds([], 3, "category", 42));
            Assert.Empty(Query.LayaFolds(null, 3, "category", 42));
        }
    }
}
