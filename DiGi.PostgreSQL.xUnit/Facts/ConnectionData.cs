namespace DiGi.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the creation of connection data using various storage methods including partition references, unique references, and partition unique references.
        /// </summary>
        [SkippableFact]
        public void ConnectionData()
        {
            if (!Create.IsAvailable(Enums.StorageMethod.PartitionReference, out _))
            {
                return;
            }

            _ = Create.ConnectionData(Enums.StorageMethod.PartitionReference);

            _ = Create.ConnectionData(Enums.StorageMethod.UniqueReference);

            _ = Create.ConnectionData(Enums.StorageMethod.PartitionUniqueReference);
        }
    }
}