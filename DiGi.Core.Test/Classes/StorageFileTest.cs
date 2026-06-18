using DiGi.Core.Classes;
using DiGi.Core.IO.File.Classes;
using System.Text.Json.Nodes;

namespace DiGi.Core.Test.Classes
{
    /// <summary>
    /// Represents a storage file specifically designed for handling <see cref="TestObject"/> instances.
    /// </summary>
    public class TestObjectStorageFile : StorageFile<TestObject>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestObjectStorageFile"/> class using the specified path.
        /// </summary>
        /// <param name="path">The path to the storage file.</param>
        public TestObjectStorageFile(string path)
            : base(path)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestObjectStorageFile"/> class using the specified <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> used to initialize the current instance.</param>
        public TestObjectStorageFile(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestObjectStorageFile"/> class based on an existing instance.
        /// </summary>
        /// <param name="testObjectStorageFile">The existing <see cref="TestObjectStorageFile"/> instance used to initialize the new instance.</param>
        public TestObjectStorageFile(TestObjectStorageFile testObjectStorageFile)
            : base(testObjectStorageFile)
        {
        }

        /// <summary>
        /// Generates a unique reference for the specified <see cref="TestObject"/>.
        /// </summary>
        /// <param name="testObject">The test object for which to create a unique reference.</param>
        /// <returns>A <see cref="UniqueReference"/> that uniquely identifies the provided <see cref="TestObject"/>.</returns>
        public override UniqueReference GetUniqueReference(TestObject testObject)
        {
            return new UniqueIdReference(typeof(TestObject), testObject.Name);
        }
    }
}
