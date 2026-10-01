using System;

namespace DiGi.WebAPI.WindowsService.xUnit
{
    public partial class Facts
    {
        // A [Flags] enum: a combined value (Read | Write = 3) travels the wire as an integer that no list of member values
        // contains, so its schema cannot enumerate the allowed values.
        [Flags]
        private enum FlagsEnumFixture
        {
            None = 0,
            Read = 1,
            Write = 2,
            Execute = 4
        }
    }
}
