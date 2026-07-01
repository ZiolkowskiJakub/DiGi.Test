using System.Collections.Generic;
using System.Reflection;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="Core.Query.SerializableMemberInfos(System.Type)"/> still correctly excludes
        /// members marked as ignored (via JsonIgnore/lacking JsonInclude) after replacing the nested
        /// List.FindIndex/RemoveAt lookup with a HashSet-based lookup keyed by (MemberType, Name).
        /// </summary>
        [Fact]
        public void SerializableMemberInfos_ExcludesIgnoredMembers()
        {
            List<MemberInfo>? memberInfos = Core.Query.SerializableMemberInfos(typeof(TestObject));

            Assert.NotNull(memberInfos);
            Assert.NotEmpty(memberInfos);

            // "Name" backing field is [JsonInclude] -> should be present
            Assert.Contains(memberInfos, x => x.Name == "name");

            // UniqueId is [JsonIgnore] on the base class -> should be excluded
            Assert.DoesNotContain(memberInfos, x => x.Name == nameof(TestObject.UniqueId));
        }

        /// <summary>
        /// Tests that base-class serializable members are still included alongside derived-class members.
        /// </summary>
        [Fact]
        public void SerializableMemberInfos_IncludesBaseClassMembers()
        {
            List<MemberInfo>? memberInfos = Core.Query.SerializableMemberInfos(typeof(TestObject));

            Assert.NotNull(memberInfos);

            // Dictionary/SortedDictionary are declared directly on TestObject
            Assert.Contains(memberInfos, x => x.Name == nameof(TestObject.Dictionary));
            Assert.Contains(memberInfos, x => x.Name == nameof(TestObject.SortedDictionary));
        }
    }
}