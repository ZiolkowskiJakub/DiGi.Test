namespace DiGi.Core.xUnit
{
    /// <summary>
    /// Classifies a test by how long a single run of it takes. Values are ordered, so a test runs when its
    /// duration is less than or equal to <see cref="Query.MaxTestDuration"/>.
    /// </summary>
    public enum TestDuration
    {
        /// <summary>Takes at most 2 seconds. Plain <see cref="FactAttribute"/> tests are short.</summary>
        Short = 0,

        /// <summary>Takes more than 2 and at most 30 seconds, or depends on an external service. Marked with <see cref="MediumFactAttribute"/>.</summary>
        Medium = 1,

        /// <summary>Takes more than 30 seconds. Marked with <see cref="LongFactAttribute"/>.</summary>
        Long = 2,
    }
}
