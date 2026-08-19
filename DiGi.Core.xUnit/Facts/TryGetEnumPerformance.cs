using DiGi.Core.Enums;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Measures and asserts execution performance of TryGetEnum across small and large enums.
        /// </summary>
        [Fact]
        public void TryGetEnum_Performance()
        {
            int iterationsSmall = 10_000;
            int iterationsLarge = 1_000;

            // Warm-up / JIT compilation
            _ = Core.Query.TryGetEnum("Aliased", typeof(TestWireToken), out _);
            _ = Core.Query.TryGetEnum("ZW", typeof(CountryCode), out _);
            _ = Core.Query.TryConvert_Enum("2", out _, typeof(TestWireToken));

            // Small enum: Exact name lookup
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterationsSmall; i++)
            {
                Assert.True(Core.Query.TryGetEnum("Aliased", typeof(TestWireToken), out _));
            }
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds < 100, $"Small enum exact name lookups failed performance threshold. Elapsed: {stopwatch.ElapsedMilliseconds} ms.");

            // Large enum: Exact name fast path (1,000 lookups)
            stopwatch.Restart();
            for (int i = 0; i < iterationsLarge; i++)
            {
                Assert.True(Core.Query.TryGetEnum("ZW", typeof(CountryCode), out _));
            }
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds < 50, $"Large enum exact name lookups failed performance threshold. Elapsed: {stopwatch.ElapsedMilliseconds} ms.");

            // Large enum: Numeric string lookups (1,000 lookups)
            stopwatch.Restart();
            for (int i = 0; i < iterationsLarge; i++)
            {
                Assert.True(Core.Query.TryGetEnum("248", typeof(CountryCode), out _));
            }
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds < 100, $"Large enum numeric lookups failed performance threshold. Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }
}
