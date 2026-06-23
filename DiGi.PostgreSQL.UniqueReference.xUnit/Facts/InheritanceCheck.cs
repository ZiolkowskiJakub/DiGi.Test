using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.UniqueReference.Classes;

namespace DiGi.PostgreSQL.UniqueReference.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the <see cref="UniqueReferencePostgreSQLConverter"/> correctly handles inheritance and polymorphism when persisting, retrieving, and removing objects that implement <see cref="ISerializableObject"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation of the test.</returns>
        [Fact]
        public async Task InheritanceCheck()
        {
            if (!PostgreSQL.xUnit.Create.IsAvailable(Enums.StorageMethod.UniqueReference, out ConnectionData? connectionData))
            {
                return;
            }

            UniqueReferencePostgreSQLConverter uniqueReferencePostgreSQLConverter = new(connectionData);

            Address address_1 = new("123 Main St", "Anytown", "CA", Core.Enums.CountryCode.Undefined);
            Address address_3 = new("1234 Main St", "Anytown", "CA", Core.Enums.CountryCode.Undefined);

            Size size_1 = new() { Height = 10.0, Width = 5.0 };
            Size size_2 = new() { Height = 20.0, Width = 15.0 };

            HashSet<Core.Classes.UniqueReference>? uniqueReferences_1 = await uniqueReferencePostgreSQLConverter.UpdateAsync([(ISerializableObject)address_1, address_3, size_1, size_2]);
            Assert.NotNull(uniqueReferences_1);

            List<ISerializableObject>? serializableObjects = await uniqueReferencePostgreSQLConverter.GetSerializableObjectsAsync<ISerializableObject>();
            Assert.NotNull(serializableObjects);

            Assert.Equal(uniqueReferences_1.Count, serializableObjects.Count);

            List<Core.Classes.UniqueReference>? uniqueReferences_2 = await uniqueReferencePostgreSQLConverter.RemoveAsync(uniqueReferences_1);
            Assert.NotNull(uniqueReferences_2);

            Assert.Equal(uniqueReferences_2.Count, uniqueReferences_1.Count);
        }
    }
}