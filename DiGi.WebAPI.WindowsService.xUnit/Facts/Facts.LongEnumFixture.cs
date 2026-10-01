namespace DiGi.WebAPI.WindowsService.xUnit
{
    public partial class Facts
    {
        // A long-backed enum with a value beyond the int32 range: its schema needs the int64 format.
        private enum LongEnumFixture : long
        {
            Small = -1,
            Large = 5_000_000_000
        }
    }
}
