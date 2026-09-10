using DiGi.Core.IO.Table.Classes;
using DiGi.Typology.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that ColumnUniqueIds reports the columns a filter chain groups by, root level first, without repeating one and without looping on a chain that links back on itself.
        /// </summary>
        [Fact]
        public void ColumnUniqueIds()
        {
            List<string>? uniqueIds = Query.ColumnUniqueIds(TypologyFilter());

            Assert.NotNull(uniqueIds);
            Assert.Equal(["county_name", "is_occupied", "predicted_year_built"], uniqueIds);

            Assert.Null(Query.ColumnUniqueIds(null));
            Assert.Null(Query.ColumnUniqueIds(new ColumnTypologyFilter<Column>()));

            ColumnTypologyFilter<Column> columnTypologyFilter_Repeated = new()
            {
                Value = IO.Constants.Column.CountyName,
                Rule = new UniqueValueFilterRule(),
                Filter = new ColumnTypologyFilter<Column>()
                {
                    Value = IO.Constants.Column.CountyName,
                    Rule = new UniqueValueFilterRule()
                }
            };

            List<string>? uniqueIds_Repeated = Query.ColumnUniqueIds(columnTypologyFilter_Repeated);
            Assert.NotNull(uniqueIds_Repeated);
            Assert.Equal(["county_name"], uniqueIds_Repeated);

            ColumnTypologyFilter<Column> columnTypologyFilter_Cyclic = new()
            {
                Value = IO.Constants.Column.CountyName,
                Rule = new UniqueValueFilterRule()
            };
            columnTypologyFilter_Cyclic.Filter = columnTypologyFilter_Cyclic;

            List<string>? uniqueIds_Cyclic = Query.ColumnUniqueIds(columnTypologyFilter_Cyclic);
            Assert.NotNull(uniqueIds_Cyclic);
            Assert.Equal(["county_name"], uniqueIds_Cyclic);
        }
    }
}
