using DiGi.Core.xUnit;

// Declared in the Xunit namespace, like Xunit.SkippableFact's [SkippableFact], so that every test project picks it
// up through its global `using Xunit`. A `using DiGi.Core.xUnit;` in a fact file would instead bring in a second
// Query class and make an unqualified Query in code or <see cref="Query..."/> ambiguous (CS0104 / CS1574).
namespace Xunit
{
    /// <summary>
    /// A <see cref="FactAttribute"/> for a <see cref="TestDuration.Medium"/> test (2-30 s, or one that depends on an external service).
    /// <para>Skipped unless <see cref="Query.MaxTestDuration"/> is <see cref="TestDuration.Medium"/> (the default) or <see cref="TestDuration.Long"/>.</para>
    /// </summary>
    public sealed class MediumFactAttribute : FactAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediumFactAttribute"/> class, setting <see cref="FactAttribute.Skip"/> from <see cref="Query.SkipReason(TestDuration)"/>.
        /// </summary>
        public MediumFactAttribute()
        {
            Skip = Query.SkipReason(TestDuration.Medium);
        }
    }
}
