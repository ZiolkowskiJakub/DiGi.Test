namespace DiGi.PostgreSQL.xUnit
{
    public partial class Tests
    {
        /// <summary>
        /// Tests the creation of connection data using various storage methods including partition references, unique references, and partition unique references.
        /// </summary>
        [SkippableFact]
        public void ConnectionData()
        {
            _ = Create.ConnectionData(Enums.StorageMethod.PartitionReference);

            _ = Create.ConnectionData(Enums.StorageMethod.UniqueReference);

            _ = Create.ConnectionData(Enums.StorageMethod.PartitionUniqueReference);
        }
    }
}
