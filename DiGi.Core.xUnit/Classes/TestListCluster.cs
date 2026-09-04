using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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

        /// <summary>
        /// Minimal concrete <see cref="ValueCluster{TKey_1, TKey_2, TValue}"/> extracting keys from colon-separated strings.
        /// </summary>
        public class TestValueCluster : ValueCluster<string, string, string>
        {
            protected override string? GetKey_1(string? value)
            {
                string[]? parts = value?.Split(':');
                return parts != null && parts.Length >= 2 ? parts[0] : null;
            }

            protected override string? GetKey_2(string? value)
            {
                string[]? parts = value?.Split(':');
                return parts != null && parts.Length >= 2 ? parts[1] : null;
            }
        }

        /// <summary>
        /// Minimal concrete <see cref="Cluster{TKey_1, TKey_2, TValue}"/> (the abstract base, not a list or value cluster) with plain list storage.
        /// <para>The first key is the constant group <c>1</c> (int); the second key is the middle colon-separated segment (string).
        /// Because the first key is a value type, the base <c>TKey_1?</c> key parameters close to non-nullable <c>int</c> in the derived signatures.</para>
        /// Used to pin the base-class enumeration and key/value retrieval behavior.
        /// </summary>
        public class TestBaseCluster : Cluster<int, string, string>
        {
            [JsonIgnore]
            private readonly List<string> values = [];

            public override bool Add(string? value)
            {
                if (value == null)
                {
                    return false;
                }

                values.Add(value);
                return true;
            }

            public override void Clear()
            {
                values.Clear();
            }

            public override bool Contains(int key_1)
            {
                return key_1 == 1 && values.Count != 0;
            }

            public override bool Contains(int key_1, string? key_2)
            {
                if (key_1 != 1 || key_2 == null)
                {
                    return false;
                }

                foreach (string value in values)
                {
                    if (GetKey_2(value) == key_2)
                    {
                        return true;
                    }
                }

                return false;
            }

            public override bool Contains(string? value)
            {
                return value != null && values.Contains(value);
            }

            public override List<UValue>? GetValues<UValue>()
            {
                List<UValue> uValues = [];
                foreach (string value in values)
                {
                    if (value is UValue uValue)
                    {
                        uValues.Add(uValue);
                    }
                }

                return uValues;
            }

            public override bool Remove(int key_1)
            {
                int countBefore = values.Count;
                values.RemoveAll(x => GetKey_1(x) == key_1);
                return values.Count != countBefore;
            }

            public override bool Remove(int key_1, string? key_2)
            {
                if (key_1 != 1 || key_2 == null)
                {
                    return false;
                }

                int countBefore = values.Count;
                values.RemoveAll(x => GetKey_2(x) == key_2);
                return values.Count != countBefore;
            }

            public override bool Remove(string? value)
            {
                return value != null && values.Remove(value);
            }

            protected override int GetKey_1(string? value)
            {
                return value == null ? default : 1;
            }

            protected override string? GetKey_2(string? value)
            {
                string[]? parts = value?.Split(':');
                return parts != null && parts.Length >= 2 ? parts[1] : null;
            }
        }

        /// <summary>
        /// Minimal concrete <see cref="List{TKey_1, TKey_2, TValue}"/> extracting keys from colon-separated strings.
        /// </summary>
        public class TestKeyedListCluster : List<string, string, string>
        {
            public TestKeyedListCluster()
            {
            }

            public TestKeyedListCluster(IEnumerable<string>? values)
                : base(values)
            {
            }

            public bool SetValues_Test(IEnumerable<string>? values)
            {
                return SetValues(values);
            }

            protected override string? GetKey_1(string? value)
            {
                string[]? parts = value?.Split(':');
                return parts != null && parts.Length >= 2 ? parts[0] : null;
            }

            protected override string? GetKey_2(string? value)
            {
                string[]? parts = value?.Split(':');
                return parts != null && parts.Length >= 2 ? parts[1] : null;
            }
        }
    }
}