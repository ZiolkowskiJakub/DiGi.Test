namespace DiGi.Core.xUnit
{
    public static partial class Query
    {
        /// <summary>
        /// Gets the skip reason for a test of the given <see cref="TestDuration"/> under the current <see cref="MaxTestDuration"/>.
        /// </summary>
        /// <param name="testDuration">The duration class of the test.</param>
        /// <returns><see langword="null"/> when the test runs; otherwise a reason naming the setting that includes it.</returns>
        public static string? SkipReason(TestDuration testDuration)
        {
            if (testDuration <= MaxTestDuration())
            {
                return null;
            }

            string limit = testDuration == TestDuration.Medium ? " (2-30 s)" : testDuration == TestDuration.Long ? " (> 30 s)" : string.Empty;

            return $"{testDuration} test{limit}. Set {EnvironmentVariableName.MaxTestDuration}={testDuration} or select TestDuration.{testDuration}.runsettings to run it.";
        }
    }
}
