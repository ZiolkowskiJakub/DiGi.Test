namespace DiGi.PostgreSQL.xUnit
{
    public partial class Tests
    {
        [SkippableFact]
        public void ConnectionData()
        {
            _ = Create.ConnectionData();
        }
    }
}