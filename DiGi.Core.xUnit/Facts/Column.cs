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
    }
}