namespace DiGi.Core.xUnit
{
    /// <summary>Represents a uint-backed test enum used to verify unique identifier generation beyond the int range.</summary>
    public enum TestEnumUInt : uint
    {
        /// <summary>Represents a value beyond the int range.</summary>
        Max = uint.MaxValue,
    }
}
