using DiGi.Core.xUnit;

// Declared in the Xunit namespace beside [SkippableFact], for the reason given in DiGi.Core.xUnit's MediumFactAttribute.cs.
namespace Xunit
{
    /// <summary>
    /// A <see cref="SkippableFactAttribute"/> for a <see cref="TestDuration.Medium"/> test (2-30 s).
    /// <para>Skipped unless <see cref="Query.MaxTestDuration"/> is <see cref="TestDuration.Medium"/> (the default) or <see cref="TestDuration.Long"/>;
    /// <see cref="Skip"/> calls inside the test keep working.</para>
    /// </summary>
    public sealed class MediumSkippableFactAttribute : SkippableFactAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediumSkippableFactAttribute"/> class, setting <see cref="FactAttribute.Skip"/> from <see cref="Query.SkipReason(TestDuration)"/>.
        /// </summary>
        public MediumSkippableFactAttribute()
        {
            Skip = Query.SkipReason(TestDuration.Medium);
        }
    }
}
