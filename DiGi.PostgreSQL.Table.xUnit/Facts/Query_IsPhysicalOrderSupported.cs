using DiGi.PostgreSQL.Classes;
using Npgsql;

namespace DiGi.PostgreSQL.Table.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies <see cref="Query.IsPhysicalOrderSupported(NpgsqlConnection?)"/> (DiGi.PostgreSQL#8): <see langword="false"/> for a null connection and for one that is not open, and <see langword="true"/> on an open connection to the local test server, which runs PostgreSQL 14 or later.
        /// <para>The branch for a server older than 14 cannot be reached here: no such server is available locally.</para>
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task Query_IsPhysicalOrderSupported()
        {
            Assert.False(Query.IsPhysicalOrderSupported(null));

            if (!PostgreSQL.xUnit.Create.IsAvailable(PostgreSQL.Enums.StorageMethod.Table, out ConnectionData? connectionData))
            {
                return;
            }

            await using NpgsqlConnection? npgsqlConnection = PostgreSQL.Create.NpgsqlConnection(connectionData);
            Assert.NotNull(npgsqlConnection);
            Assert.False(npgsqlConnection.IsPhysicalOrderSupported());

            await npgsqlConnection.OpenAsync();
            Assert.True(npgsqlConnection.PostgreSqlVersion.Major >= 14, $"The local test server is PostgreSQL {npgsqlConnection.PostgreSqlVersion}; this fact expects 14 or later.");
            Assert.True(npgsqlConnection.IsPhysicalOrderSupported());

            await npgsqlConnection.CloseAsync();
            Assert.False(npgsqlConnection.IsPhysicalOrderSupported());
        }
    }
}
