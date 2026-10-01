using DiGi.Core.xUnit;

// Declared in the Xunit namespace for the reason given in MediumFactAttribute.cs.
namespace Xunit
{
    /// <summary>
    /// A <see cref="FactAttribute"/> for a <see cref="TestDuration.Long"/> test (more than 30 s).
    /// <para>Skipped unless <see cref="Query.MaxTestDuration"/> is <see cref="TestDuration.Long"/>.</para>
    /// </summary>
    public sealed class LongFactAttribute : FactAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LongFactAttribute"/> class, setting <see cref="FactAttribute.Skip"/> from <see cref="Query.SkipReason(TestDuration)"/>.
        /// </summary>
        public LongFactAttribute()
        {
            Skip = Query.SkipReason(TestDuration.Long);
        }
    }
}
