using System;

namespace DiGi.Core.xUnit
{
    public static partial class Query
    {
        /// <summary>
        /// Gets the longest <see cref="TestDuration"/> to run, from the <see cref="EnvironmentVariableName.MaxTestDuration"/>
        /// environment variable (case-insensitive).
        /// </summary>
        /// <returns>The parsed value, or <see cref="TestDuration.Medium"/> when the variable is not set or not a <see cref="TestDuration"/> name.</returns>
        public static TestDuration MaxTestDuration()
        {
            string? value = Environment.GetEnvironmentVariable(EnvironmentVariableName.MaxTestDuration);
            if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse(value.Trim(), true, out TestDuration testDuration) && Enum.IsDefined(testDuration))
            {
                return testDuration;
            }

            return TestDuration.Medium;
        }
    }
}
