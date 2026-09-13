using DiGi.PostgreSQL.Classes;
using Npgsql;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests constructor state, connection-string conversion, configuration-file plumbing, JSON round trip, and serialization integrity of <see cref="ConnectionData"/>, including the optional pool settings.
        /// </summary>
        [Fact]
        public void ConnectionData()
        {
            ConnectionData connectionData = new("host", "user", "pass", "db", 5432)
            {
                MaximumPoolSize = 5,
                PoolTimeout = 15
            };

            // Constructor state.
            Assert.Equal("host", connectionData.Host);
            Assert.Equal(5432, connectionData.Port);
            Assert.Equal("user", connectionData.Username);
            Assert.Equal("pass", connectionData.Password);
            Assert.Equal("db", connectionData.Database);
            Assert.Equal(5, connectionData.MaximumPoolSize);
            Assert.Equal(15, connectionData.PoolTimeout);

            // String conversion: Npgsql keywords present.
            string? connectionString = connectionData.ToString();
            Assert.Contains("Maximum Pool Size=5", connectionString);
            Assert.Contains("Timeout=15", connectionString);

            // Production shape unchanged: with the pool settings at their defaults the string is byte-identical to the legacy five-key form.
            ConnectionData connectionData_Production = new("host", "user", "pass", "db", 5432);
            Assert.Equal("Host=host;Port=5432;Username=user;Password=pass;Database=db", connectionData_Production.ToString());

            // Configuration-file plumbing: the optional pool keys flow through when present...
            PostgreSQLConfigurationFile configurationFile = new()
            {
                Host = "host",
                Username = "user",
                Password = "pass",
                Database = "db",
                Port = 5432,
                MaximumPoolSize = 5,
                PoolTimeout = 15
            };

            ConnectionData? connectionData_FromConf = PostgreSQL.Create.ConnectionData(configurationFile);
            Assert.NotNull(connectionData_FromConf);
            Assert.Equal(5, connectionData_FromConf!.MaximumPoolSize);
            Assert.Equal(15, connectionData_FromConf.PoolTimeout);

            // ...and answer nulls when absent, keeping the connection string unchanged.
            PostgreSQLConfigurationFile configurationFile_NoPool = new()
            {
                Host = "host",
                Username = "user",
                Password = "pass",
                Database = "db",
                Port = 5432
            };

            ConnectionData? connectionData_FromConf_NoPool = PostgreSQL.Create.ConnectionData(configurationFile_NoPool);
            Assert.NotNull(connectionData_FromConf_NoPool);
            Assert.Null(connectionData_FromConf_NoPool!.MaximumPoolSize);
            Assert.Null(connectionData_FromConf_NoPool.PoolTimeout);

            // JSON round trip preserves the pool settings.
            // Core.Convert is mandatory here: DiGi.PostgreSQL defines its own Convert class (ToPostgreSQL only), so a bare Convert would not compile in this namespace.
            string? json = Core.Convert.ToSystem_String(connectionData);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<ConnectionData>? connectionDatas = Core.Convert.ToDiGi<ConnectionData>(json);
            Assert.NotNull(connectionDatas);

            ConnectionData? connectionData_RoundTrip = connectionDatas!.FirstOrDefault();
            Assert.NotNull(connectionData_RoundTrip);
            Assert.Equal(5, connectionData_RoundTrip.MaximumPoolSize);
            Assert.Equal(15, connectionData_RoundTrip.PoolTimeout);

            Core.xUnit.Query.SerializationCheck(connectionData);
        }

        /// <summary>
        /// Smoke-tests that the per-storage-method configuration files still parse into <see cref="ConnectionData"/>.
        /// </summary>
        [SkippableFact]
        public void ConnectionData_ConfLoading()
        {
            if (!Create.IsAvailable(Enums.StorageMethod.PartitionReference, out _))
            {
                return;
            }

            _ = Create.ConnectionData(Enums.StorageMethod.PartitionReference);

            _ = Create.ConnectionData(Enums.StorageMethod.UniqueReference);

            _ = Create.ConnectionData(Enums.StorageMethod.PartitionUniqueReference);
        }

        /// <summary>
        /// Replicates pool exhaustion against the development database named by <c>PostgreSQL_PartitionReference.conf</c> (git-ignored <c>user files/</c>), holding the pool's single slot and asserting the exact transient shape the deployed 503 filter consumes (DiGi.GIS.WebAPI #30/#31).
        /// <para>Skips without a reachable development database; it opens and closes pooled connections only — no table in the target database is touched, because the failure happens before any SQL executes.</para>
        /// </summary>
        [SkippableFact]
        public async Task ConnectionData_PoolExhaustion()
        {
            if (!Create.IsAvailable(Enums.StorageMethod.PartitionReference, out ConnectionData? connectionData) || connectionData is null)
            {
                return;
            }

            connectionData.MaximumPoolSize = 1;
            connectionData.PoolTimeout = 5;

            // The holder occupies the pool's only slot (Npgsql pools are per connection string, per process).
            await using NpgsqlConnection? connection_Holder = PostgreSQL.Create.NpgsqlConnection(connectionData);
            if (connection_Holder is null)
            {
                return;
            }

            await connection_Holder.OpenAsync();

            // The waiter exhausts the pool after waiting out the timeout.
            await using NpgsqlConnection? connection_Waiter = PostgreSQL.Create.NpgsqlConnection(connectionData);
            if (connection_Waiter is null)
            {
                return;
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            NpgsqlException? npgsqlException = await Assert.ThrowsAsync<NpgsqlException>(() => connection_Waiter.OpenAsync());
            stopwatch.Stop();

            Assert.IsType<TimeoutException>(npgsqlException.InnerException);
            // This is the exact filter the deployed controllers use to answer 503 — asserting it closes #30's AC3.
            Assert.True(npgsqlException.IsTransient, "Pool exhaustion must classify as transient (NpgsqlException.IsTransient).");
            Assert.Contains("connection pool has been exhausted", npgsqlException.Message);
            Assert.True(stopwatch.ElapsedMilliseconds >= 4000, $"Waiter failed in {stopwatch.ElapsedMilliseconds} ms — expected it to wait out the 5 s pool timeout.");
        }
    }
}
