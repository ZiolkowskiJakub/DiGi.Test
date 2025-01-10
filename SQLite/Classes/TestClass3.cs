using DiGi.Core.Classes;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.SQLite.Test.Classes
{
    public class TestClass3 : GuidObject
    {
        [JsonInclude, JsonPropertyName("Parent")]
        public TestClass2 Parent { get; set; }

        [JsonInclude, JsonPropertyName("TestClasses")]
        public List<TestClass1> TestClasses { get; set; }

        public TestClass3()
            :base()
        {

        }

        public TestClass3(Guid guid)
            : base(guid)
        {

        }

        public TestClass3(JsonObject jsonObject)
            : base(jsonObject)
        {

        }
    }
}
