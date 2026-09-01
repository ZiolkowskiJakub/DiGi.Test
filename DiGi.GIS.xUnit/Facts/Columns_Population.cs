using DiGi.Core.Classes;
using DiGi.Core.IO.Table.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that Create.Column_Population and Create.Columns_Population create properly configured columns across single years, sequences, and ranges.
        /// </summary>
        [Fact]
        public void Columns_Population()
        {
            Column column_Single = GIS.IO.Create.Column_Population(2020);
            Assert.NotNull(column_Single);
            Assert.Equal("Population 2020", column_Single.Name);
            Assert.IsType<ExtendedColumn>(column_Single);
            ExtendedColumn extendedColumn = (ExtendedColumn)column_Single;
            Assert.Equal(typeof(int), extendedColumn.Type);
            Assert.Equal(Core.Query.Description(GIS.IO.Enums.Category.Population), extendedColumn.Category);

            List<Column> columns_SingleYear = GIS.IO.Create.Columns_Population(2020);
            Assert.NotNull(columns_SingleYear);
            Assert.Single(columns_SingleYear);
            Assert.Equal("Population 2020", columns_SingleYear[0].Name);

            Range<int> range = new(2010, 2015);
            List<Column> columns_Range = GIS.IO.Create.Columns_Population(range);
            Assert.NotNull(columns_Range);
            Assert.Equal(6, columns_Range.Count);
            Assert.Equal("Population 2010", columns_Range[0].Name);
            Assert.Equal("Population 2015", columns_Range[5].Name);

            List<Column> columns_Enumerable = GIS.IO.Create.Columns_Population((IEnumerable<int>)[2008, 2012, 2024]);
            Assert.NotNull(columns_Enumerable);
            Assert.Equal(3, columns_Enumerable.Count);
            Assert.Equal("Population 2008", columns_Enumerable[0].Name);
            Assert.Equal("Population 2012", columns_Enumerable[1].Name);
            Assert.Equal("Population 2024", columns_Enumerable[2].Name);

            List<Column> columns_NullRange = GIS.IO.Create.Columns_Population((Range<int>?)null);
            Assert.NotNull(columns_NullRange);
            Assert.Empty(columns_NullRange);

            List<Column> columns_NullEnumerable = GIS.IO.Create.Columns_Population((IEnumerable<int>?)null);
            Assert.NotNull(columns_NullEnumerable);
            Assert.Empty(columns_NullEnumerable);
        }
    }
}
