using DiGi.Core.Classes;
using DiGi.Core.IO.File.Classes;
using System.Text.Json.Nodes;

namespace DiGi.Core.Test.Classes
{
    public class TestObjectStorageFile : StorageFile<TestObject>
    {
        public TestObjectStorageFile(string path)
            :base(path)
        {

        }

        public TestObjectStorageFile(JsonObject jsonObject)
            : base(jsonObject)
        {

        }

        public TestObjectStorageFile(TestObjectStorageFile testObjectStorageFile)
            : base(testObjectStorageFile)
        {

        }

        public override UniqueReference GetUniqueReference(TestObject testObject)
        {
            return new UniqueIdReference(typeof(TestObject), testObject.Name);
        }
    }
}
