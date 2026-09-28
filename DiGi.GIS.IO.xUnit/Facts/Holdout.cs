using System.Collections.Generic;
using System.Text;

namespace DiGi.GIS.IO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Pins the holdout membership to FNV-1a values computed independently of the implementation (Python, UTF-8 bytes, 32-bit FNV-1a), so a reimplementation can never silently shift the holdout.
        /// <para>"a" is the published FNV-1a 32-bit test vector (0xe40c292c). The GUID references are real building references from the 2025-05-27 training table, of which 4 059 of 20 497 fall in the holdout; the BDOT-style keys exercise the grouped carve, and the non-ASCII key proves the hash runs over the UTF-8 bytes, not the UTF-16 code units.</para>
        /// </summary>
        [Fact]
        public void Holdout_GoldenValues()
        {
            (string Key, uint Hash, bool Expected)[] goldenValues =
            [
                ("a", 3826002220u, true),
                ("PL.PZGiK.994.BDOT10k.BUBD_A.1", 2820662861u, false),
                ("PL.PZGiK.994.BDOT10k.BUBD_A.5", 2753552385u, true),
                ("PL.PZGiK.994.BDOT10k.BUBD_A.8", 2669664290u, true),
                ("PL.PZGiK.994.BDOT10k.BUBD_A.10", 657970887u, false),
                ("Świętochłowice", 734899785u, true),
                ("ABC_12", 480077631u, false),
                ("272D6AAF-72C6-9B0E-E053-CC2BA8C0B5EA", 91523630u, true),
                ("272D6AAF-72D0-9B0E-E053-CC2BA8C0B5EA", 1688984305u, true),
                ("272D6AAF-793F-9B0E-E053-CC2BA8C0B5EA", 2555014005u, true),
                ("272D6AAF-7D62-9B0E-E053-CC2BA8C0B5EA", 2866770347u, false),
                ("272D6AAF-7D64-9B0E-E053-CC2BA8C0B5EA", 3893195613u, false),
                ("272D6AAF-98A8-9B0E-E053-CC2BA8C0B5EA", 1257050858u, false)
            ];

            foreach ((string key, uint hash, bool expected) in goldenValues)
            {
                // The recomputation guards the recorded table; the golden membership was established independently of this code.
                uint hash_Computed = 2166136261;
                foreach (byte value in Encoding.UTF8.GetBytes(key))
                {
                    hash_Computed ^= value;
                    hash_Computed *= 16777619;
                }

                Assert.Equal(hash, hash_Computed);
                Assert.Equal(expected, Query.Holdout(key));
            }
        }

        /// <summary>
        /// Pins the denominator handling: below 2 behaves as 2, and the membership at 2 and 5 follows the FNV-1a values of the keys.
        /// </summary>
        [Fact]
        public void Holdout_Denominator()
        {
            // 1257050858 is even but not a multiple of 5: in for 2, out for 5.
            string key_InTwoNotFive = "272D6AAF-98A8-9B0E-E053-CC2BA8C0B5EA";
            Assert.True(Query.Holdout(key_InTwoNotFive, 2));
            Assert.False(Query.Holdout(key_InTwoNotFive, 5));

            // 2753552385 is odd but a multiple of 5: out for 2, in for 5.
            string key_InFiveNotTwo = "PL.PZGiK.994.BDOT10k.BUBD_A.5";
            Assert.False(Query.Holdout(key_InFiveNotTwo, 2));
            Assert.True(Query.Holdout(key_InFiveNotTwo, 5));

            // 2669664290 is in for both, 2820662861 in for neither.
            Assert.True(Query.Holdout("PL.PZGiK.994.BDOT10k.BUBD_A.8", 2));
            Assert.True(Query.Holdout("PL.PZGiK.994.BDOT10k.BUBD_A.8", 5));
            Assert.False(Query.Holdout("PL.PZGiK.994.BDOT10k.BUBD_A.1", 2));
            Assert.False(Query.Holdout("PL.PZGiK.994.BDOT10k.BUBD_A.1", 5));

            // 0 and 1 behave as 2, on a key that is in the holdout for 2 and one that is not.
            Assert.Equal(Query.Holdout(key_InTwoNotFive, 2), Query.Holdout(key_InTwoNotFive, 0));
            Assert.Equal(Query.Holdout(key_InTwoNotFive, 2), Query.Holdout(key_InTwoNotFive, 1));
            Assert.Equal(Query.Holdout("PL.PZGiK.994.BDOT10k.BUBD_A.1", 2), Query.Holdout("PL.PZGiK.994.BDOT10k.BUBD_A.1", 0));
            Assert.Equal(Query.Holdout("PL.PZGiK.994.BDOT10k.BUBD_A.1", 2), Query.Holdout("PL.PZGiK.994.BDOT10k.BUBD_A.1", 1));
        }

        /// <summary>
        /// Pins the absent-input handling: null or empty keys are out of the holdout, and a null key list gives an empty result rather than throwing.
        /// </summary>
        [Fact]
        public void Holdout_NullAndEmpty()
        {
            Assert.False(Query.Holdout(null));
            Assert.False(Query.Holdout(""));

            Assert.Empty(Query.Holdouts(null));

            List<bool> booleans = [false, true];
            Assert.Equal(booleans, Query.Holdouts([null, "a"]));
        }
    }
}
