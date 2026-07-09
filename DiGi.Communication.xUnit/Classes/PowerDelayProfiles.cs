using DiGi.Communication.Classes;

namespace DiGi.Communication.xUnit
{
    /// <summary>
    /// Reference normalized Power Delay Profiles (Fractional Power delay profiles) derived from macrocellular measurements for urban environments. Delays are expressed in seconds.
    /// </summary>
    public static class PowerDelayProfiles
    {
        /// <summary>
        /// Normalized Power Delay Profile for the "Bad Urban" macrocellular environment.
        /// </summary>
        public static List<PowerDelayProfilePoint> BadUrban =>
        [
            new(0.0e-6, 0.033),
            new(0.1e-6, 0.089),
            new(0.3e-6, 0.141),
            new(0.7e-6, 0.194),
            new(1.6e-6, 0.114),
            new(2.2e-6, 0.052),
            new(3.1e-6, 0.035),
            new(5.0e-6, 0.140),
            new(6.0e-6, 0.136),
            new(7.2e-6, 0.041),
            new(8.1e-6, 0.019),
            new(10.0e-6, 0.006)
        ];

        /// <summary>
        /// Normalized Power Delay Profile for the "Typical Urban" macrocellular environment.
        /// </summary>
        public static List<PowerDelayProfilePoint> TypicalUrban =>
        [
            new(0.0e-6, 0.092),
            new(0.1e-6, 0.115),
            new(0.3e-6, 0.231),
            new(0.5e-6, 0.127),
            new(0.8e-6, 0.115),
            new(1.1e-6, 0.074),
            new(1.3e-6, 0.046),
            new(1.7e-6, 0.074),
            new(2.3e-6, 0.051),
            new(3.1e-6, 0.032),
            new(3.2e-6, 0.018),
            new(5.0e-6, 0.025)
        ];
    }
}
