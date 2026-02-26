namespace DiGi.PostgreSQL.xUnit
{
    public partial class Tests
    {
        [SkippableFact]
        public void ConnectionData()
        {
            _ = Create.ConnectionData(Enums.StorageMethod.PartitionReference);

            _ = Create.ConnectionData(Enums.StorageMethod.UniqueReference);

            _ = Create.ConnectionData(Enums.StorageMethod.PartitionUniqueReference);
        }
    }
}