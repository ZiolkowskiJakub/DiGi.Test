using System.Collections.Generic;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="Core.Classes.List{TKey_1, TKey_2, TValue}.Remove(IEnumerable{Core.Classes.ListClusterReference{TKey_1, TKey_2}})"/>
        /// still removes exactly the referenced values after replacing the List.Contains() check inside RemoveAll()
        /// (an O(n*m) pattern) with a HashSet-based lookup built once per call.
        /// </summary>
        [Fact]
        public void ListCluster_Remove_ByClusterReferences_RemovesOnlySpecifiedIndexes()
        {
            Classes.TestListCluster testListCluster = new();

            Assert.True(testListCluster.Add("X"));
            Assert.True(testListCluster.Add("Y"));
            Assert.True(testListCluster.Add("Z"));

            List<Core.Classes.ListClusterReference<string, string>> listClusterReferences =
            [
                new Classes.TestListClusterReference("k1", "k2", 0),
                new Classes.TestListClusterReference("k1", "k2", 2),
            ];

            List<string>? removed = testListCluster.Remove(listClusterReferences);

            Assert.NotNull(removed);
            Assert.Equal(2, removed.Count);
            Assert.Contains("X", removed);
            Assert.Contains("Z", removed);

            List<string>? remaining = testListCluster.GetValues<string>("k1");
            Assert.NotNull(remaining);
            Assert.Equal(["Y"], remaining);
        }
    }
}
