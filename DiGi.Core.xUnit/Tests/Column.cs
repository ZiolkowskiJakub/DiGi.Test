using DiGi.Core.IO.Table.Classes;

namespace DiGi.Core.xUnit
{
    public partial class Tests
    {
        [Fact]
        public void Column()
        {
            Column column; ;

            column = new Column(2, "AAAA", typeof(string));

            Query.SerializationCheck(column);

            column = new Column(2, "BBBB", typeof(Core.Classes.Address));

            Query.SerializationCheck(column);
        }
    }
}