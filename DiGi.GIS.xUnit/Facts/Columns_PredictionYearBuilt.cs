using DiGi.Core.Classes;
using DiGi.Core.IO.Table.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that Create.Columns_PredictionYearBuilt generates the 5 standard detection columns for a single year and across ranges.
        /// </summary>
        [Fact]
        public void Columns_PredictionYearBuilt()
        {
            List<Column> columns_Single = GIS.IO.Create.Columns_PredictionYearBuilt(2020);
            Assert.NotNull(columns_Single);
            Assert.Equal(5, columns_Single.Count);
            Assert.Contains(columns_Single, c => c.Name == "Prediction Confidence 2020");
            Assert.Contains(columns_Single, c => c.Name == "Prediction BoundingBox X 2020");
            Assert.Contains(columns_Single, c => c.Name == "Prediction BoundingBox Y 2020");
            Assert.Contains(columns_Single, c => c.Name == "Prediction BoundingBox Width 2020");
            Assert.Contains(columns_Single, c => c.Name == "Prediction BoundingBox Height 2020");

            Range<int> range = new(2020, 2022);
            List<Column> columns_Range = GIS.IO.Create.Columns_PredictionYearBuilt(range);
            Assert.NotNull(columns_Range);
            Assert.Equal(15, columns_Range.Count);

            List<Column> columns_NullRange = GIS.IO.Create.Columns_PredictionYearBuilt((Range<int>?)null);
            Assert.NotNull(columns_NullRange);
            Assert.Empty(columns_NullRange);

            List<Column> columns_NullEnumerable = GIS.IO.Create.Columns_PredictionYearBuilt((IEnumerable<int>?)null);
            Assert.NotNull(columns_NullEnumerable);
            Assert.Empty(columns_NullEnumerable);
        }
    }
}
