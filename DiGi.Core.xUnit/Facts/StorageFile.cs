using DiGi.Core.Classes;
using DiGi.Core.Enums;
using System;
using System.Collections.Generic;
using System.IO;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="IO.File.Classes.StorageFile{TSerializableObject}.Remove(IEnumerable{UniqueReference})"/>
        /// still removes every matching reference even when a non-matching reference is processed first.
        /// Previously the removal loop used `for (int i = Count - 1; i >= 0; i--) { var x = set.ElementAt(0); ... }`
        /// without removing non-matching elements from the set, so a non-matching first element would get
        /// re-fetched on every iteration and starve the rest of the references from ever being processed.
        /// </summary>
        [Fact]
        public void StorageFile_Remove_ProcessesAllReferencesEvenWhenFirstIsMissing()
        {
            string path = System.IO.Path.GetTempFileName();

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                using IO.File.Classes.StorageFile<Address> storageFile = new(path);
                storageFile.Open();

                Address address_1 = new("Street 1", "City 1", "Code 1", CountryCode.PL);
                Address address_2 = new("Street 2", "City 2", "Code 2", CountryCode.PL);
                Address address_3 = new("Street 3", "City 3", "Code 3", CountryCode.PL);

                UniqueReference? uniqueReference_1 = storageFile.AddValue(address_1);
                UniqueReference? uniqueReference_2 = storageFile.AddValue(address_2);
                UniqueReference? uniqueReference_3 = storageFile.AddValue(address_3);

                Assert.NotNull(uniqueReference_1);
                Assert.NotNull(uniqueReference_2);
                Assert.NotNull(uniqueReference_3);

                // A reference that was never added - simulates the element that fails the dictionary lookup
                // and previously could starve the rest of the loop regardless of where it lands in the set.
                UniqueReference uniqueReference_Missing = new GuidReference(new TypeReference(typeof(Address)), Guid.NewGuid());

                HashSet<UniqueReference>? removed = storageFile.Remove([uniqueReference_Missing, uniqueReference_1!, uniqueReference_2!, uniqueReference_3!]);

                Assert.NotNull(removed);
                Assert.Contains(uniqueReference_1!, removed);
                Assert.Contains(uniqueReference_2!, removed);
                Assert.Contains(uniqueReference_3!, removed);
                Assert.DoesNotContain(uniqueReference_Missing, removed);

                // Removal clears the stored value (the reference itself remains a known key by design).
                Assert.Null(storageFile.GetValue(uniqueReference_1));
                Assert.Null(storageFile.GetValue(uniqueReference_2));
                Assert.Null(storageFile.GetValue(uniqueReference_3));
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        /// <summary>
        /// Tests that GetValues still
        /// returns the correct values by index after replacing the Count()+ElementAt(index) double-enumeration over
        /// a HashSet of unique references with a single materialization into an indexable list.
        /// </summary>
        [Fact]
        public void StorageFile_GetValues_ByIndex_ReturnsCorrectValues()
        {
            string path = System.IO.Path.GetTempFileName();

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                using IO.File.Classes.StorageFile<Address> storageFile = new(path);
                storageFile.Open();

                Address address_1 = new("Street 1", "City 1", "Code 1", CountryCode.PL);
                Address address_2 = new("Street 2", "City 2", "Code 2", CountryCode.PL);
                Address address_3 = new("Street 3", "City 3", "Code 3", CountryCode.PL);

                storageFile.AddValue(address_1);
                storageFile.AddValue(address_2);
                storageFile.AddValue(address_3);

                Assert.True(storageFile.Save());

                IEnumerable<Address?>? addresses = storageFile.GetValues([0, 1, 2]);

                Assert.NotNull(addresses);

                List<Address?> addresses_List = [.. addresses];

                Assert.Equal(3, addresses_List.Count);
                Assert.All(addresses_List, address => Assert.NotNull(address));

                // An out-of-range index must not throw; it yields a null placeholder at that position
                // instead, preserving positional correspondence with the requested indexes.
                IEnumerable<Address?>? addresses_OutOfRange = storageFile.GetValues([0, 99]);
                Assert.NotNull(addresses_OutOfRange);

                List<Address?> addresses_OutOfRange_List = [.. addresses_OutOfRange];
                Assert.Equal(2, addresses_OutOfRange_List.Count);
                Assert.NotNull(addresses_OutOfRange_List[0]);
                Assert.Null(addresses_OutOfRange_List[1]);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }
}