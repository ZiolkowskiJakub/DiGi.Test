namespace DiGi.Core.xUnit
{
    /// <summary>Represents a long-backed test enum used to verify unique identifier generation beyond the int range.</summary>
    public enum TestEnumLong : long
    {
        /// <summary>Represents a value beyond the int range.</summary>
        Max = long.MaxValue,
    }
}
