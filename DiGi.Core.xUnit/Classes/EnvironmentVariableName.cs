namespace DiGi.Core.xUnit
{
    /// <summary>
    /// Names of the environment variables read by the test projects.
    /// </summary>
    public static class EnvironmentVariableName
    {
        /// <summary>
        /// Longest <see cref="TestDuration"/> to run (<c>Short</c>, <c>Medium</c> or <c>Long</c>). Set by the
        /// <c>TestDuration.*.runsettings</c> files; <see cref="TestDuration.Medium"/> when not set.
        /// </summary>
        public const string MaxTestDuration = "DIGI_TEST_MAX_DURATION";
    }
}
