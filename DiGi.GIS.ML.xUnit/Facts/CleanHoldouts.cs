using DiGi.Core.IO.DelimitedData;
using DiGi.Core.IO.DelimitedData.Enums;
using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.ML;
using System.Collections.Generic;
using System.Reflection;

namespace DiGi.GIS.ML.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the clean holdout is exactly the holdout rows the manifest flags <c>Legacy = false</c>.
        /// <para>Rows are covered for each way a reference can fail to be clean: not in the holdout, flagged legacy, and absent from the manifest (unknown, so not clean).</para>
        /// </summary>
        [Fact]
        public void CleanHoldouts_IsHoldoutAndNotLegacy()
        {
            string path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "dataset_references_Fixture.tsv")!;
            Table? table = DiGi.Core.IO.DelimitedData.Create.Table(path, DelimitedDataSeparator.Tab);
            Dictionary<string, bool>? legacyFlags = table.LegacyFlags();

            Assert.NotNull(legacyFlags);
            Assert.Equal(4, legacyFlags!.Count);
            Assert.True(legacyFlags["B"]);
            Assert.False(legacyFlags["A"]);

            List<string?> references = ["A", "B", "C", "D", "E", null];
            List<bool> holdouts = [true, true, false, true, true, true];

            List<bool> result = references.CleanHoldouts(holdouts, legacyFlags);

            Assert.Equal([true, false, false, true, false, false], result);
        }

        /// <summary>
        /// Verifies that a manifest without the <c>Legacy</c> column, or with a flag that is not a boolean, is refused rather than read as all-clean.
        /// </summary>
        [Fact]
        public void LegacyFlags_RefusesUntrustworthyManifest()
        {
            string path_NoColumn = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "dataset_references_NoLegacyColumn.tsv")!;
            string path_BadFlag = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "dataset_references_BadLegacy.tsv")!;
            Assert.Null(DiGi.Core.IO.DelimitedData.Create.Table(path_NoColumn, DelimitedDataSeparator.Tab).LegacyFlags());
            Assert.Null(DiGi.Core.IO.DelimitedData.Create.Table(path_BadFlag, DelimitedDataSeparator.Tab).LegacyFlags());
            Assert.Null(((Table?)null).LegacyFlags());
        }
    }
}
