using DiGi.Core;
using DiGi.Core.Classes;
using DiGi.PostgreSQL.Classes;
using System.Reflection;
using Xunit.Sdk;

namespace DiGi.PostgreSQL.xUnit
{
    public partial class Tests
    {
        [Fact]
        public async Task InsertData()
        {
            string? directory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Assert.NotNull(directory);

            string path = System.IO.Path.Combine(directory, "PostgreSQL.conf");

            Assert.True(File.Exists(path));

            PostgreSQLConfigurationFile? postgreSQLConfigurationFile = Create.PostgreSQLConfigurationFile(path);
            Assert.NotNull(postgreSQLConfigurationFile);

            ConnectionData? connectionData = Create.ConnectionData(postgreSQLConfigurationFile);
            Assert.NotNull(connectionData);

            if (!Query.IsAvailable(connectionData.GetDefault()))
            {
                throw SkipException.ForSkip("PostgreSQL service is not available");
            }

            PostgreSQLConverter postgreSQLConverter = new (connectionData);

            Address address_1 = new("123 Main St", "Anytown", "CA", Core.Enums.CountryCode.Undefined);

            UniqueReference? uniqueReference_1 = await postgreSQLConverter.UpdateAsync(address_1);
            Assert.NotNull(uniqueReference_1);

            Address? address_2 = await postgreSQLConverter.GetAsync<Address>(uniqueReference_1);
            Assert.NotNull(uniqueReference_1);

            Assert.Equal(address_1.ToSystem_String(), address_2.ToSystem_String());

            UniqueReference? uniqueReference_2 = await postgreSQLConverter.RemoveAsync(uniqueReference_1);

            Assert.Equal(uniqueReference_1.ToSystem_String(), uniqueReference_2.ToSystem_String());
        }
    }
}