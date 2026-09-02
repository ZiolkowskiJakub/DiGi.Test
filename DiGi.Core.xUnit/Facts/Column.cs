using DiGi.Core.IO.Table.Classes;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the serialization of the <see cref="Column"/> class by verifying that it can be correctly processed using different data types.
        /// </summary>
        [Fact]
        public void Column()
        {
            Column column;

            column = new Column(2, "AAAA", typeof(string));

            Query.SerializationCheck(column);

            column = new Column(2, "BBBB", typeof(Core.Classes.Address));

            Query.SerializationCheck(column);
        }

        /// <summary>
        /// Tests that <see cref="Core.IO.Query.UniqueId(Core.IO.Table.Interfaces.IColumn?)"/> normalizes column names into slugs.
        /// </summary>
        [Fact]
        public void Column_UniqueId()
        {
            Column column_Null = null!;
            Assert.Null(Core.IO.Query.UniqueId(column_Null));

            Column column_EmptyName = new(0, null, typeof(string));
            Assert.Null(Core.IO.Query.UniqueId(column_EmptyName));

            Column column_FloorArea = new(0, "Floor area", typeof(double));
            Assert.Equal("floor_area", Core.IO.Query.UniqueId(column_FloorArea));

            Column column_GridCell = new(1, "Grid cell coverage [0,0]", typeof(double));
            Assert.Equal("grid_cell_coverage_0_0", Core.IO.Query.UniqueId(column_GridCell));

            Column column_Dotted = new(2, "Feature.Sub.Name,Test", typeof(string));
            Assert.Equal("feature_sub_name_test", Core.IO.Query.UniqueId(column_Dotted));
        }
    }
}