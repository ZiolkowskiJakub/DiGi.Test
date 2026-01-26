using DiGi.PostgreSQL;
using DiGi.PostgreSQL.Classes;

namespace DiGi.PostgreSQL.xUnit
{
    public partial class Tests
    {
        [Fact]
        public void Round()
        {
            ConnectionData connectionData = new ConnectionData();
            
            Assert.Equal(0.0, 0.0);
        }
    }
}