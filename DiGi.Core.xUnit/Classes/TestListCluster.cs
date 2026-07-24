using DiGi.Core.Classes;
using DiGi.Core.Interfaces;

namespace DiGi.Core.xUnit
{
    public partial class Classes
    {
        /// <summary>
        /// Minimal concrete <see cref="ListClusterReference{TKey_1, TKey_2}"/> used to exercise
        /// <see cref="List{TKey_1, TKey_2, TValue}.Remove(System.Collections.Generic.IEnumerable{ListClusterReference{TKey_1, TKey_2}})"/>.
        /// </summary>
        public class TestListClusterReference : ListClusterReference<string, string>
        {
            public TestListClusterReference(string key_1, string key_2, int index)
                : base(key_1, key_2, index)
            {
            }

            public override bool Equals(IReference reference)
            {
                return ReferenceEquals(this, reference);
            }
        }

        /// <summary>
        /// Minimal concrete <see cref="List{TKey_1, TKey_2, TValue}"/> grouping every value
        /// under a single (key_1, key_2) pair, used to exercise removal-by-index behavior.
        /// </summary>
        public class TestListCluster : List<string, string, string>
        {
            protected override string? GetKey_1(string? value)
            {
                return "k1";
            }

            protected override string? GetKey_2(string? value)
            {
                return "k2";
            }
        }
    }
}