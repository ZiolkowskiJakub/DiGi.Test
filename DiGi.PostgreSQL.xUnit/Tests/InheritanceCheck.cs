using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.PostgreSQL.Classes;
using System.Reflection;
using Xunit.Sdk;

namespace DiGi.PostgreSQL.xUnit
{
    public partial class Tests
    {
        [Fact]
        public async Task InheritanceCheck()
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
            Address address_3 = new("1234 Main St", "Anytown", "CA", Core.Enums.CountryCode.Undefined);

            Size size_1 = new() { Height = 10.0, Width = 5.0 };
            Size size_2 = new() { Height = 20.0, Width = 15.0 };

            List<UniqueReference>? uniqueReferences_1 = await postgreSQLConverter.UpdateAsync([(ISerializableObject)address_1, address_3, size_1, size_2]);
            Assert.NotNull(uniqueReferences_1);

            List<ISerializableObject>? serializableObjects = await postgreSQLConverter.GetSerializableObjects<ISerializableObject>();
            Assert.NotNull(serializableObjects);

            Assert.Equal(uniqueReferences_1.Count, serializableObjects.Count);

            List<UniqueReference>? uniqueReferences_2 = await postgreSQLConverter.RemoveAsync(uniqueReferences_1);
            Assert.NotNull(uniqueReferences_2);

            Assert.Equal(uniqueReferences_2.Count, uniqueReferences_1.Count);
        }
    }
}