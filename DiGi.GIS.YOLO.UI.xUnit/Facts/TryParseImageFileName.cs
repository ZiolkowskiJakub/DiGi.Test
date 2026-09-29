using System.Globalization;
using System.Threading;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that an image file name is split at its last underscore, so a reference that itself contains underscores is read whole and never confused with a shorter one.
        /// <para>The resume clean-up decides by this parse which files belong to a finished building. A prefix test would let <c>ABC_1_2015.jpeg</c> count as a file of <c>ABC</c> and delete or keep another building&apos;s image. The assertions run again under pl-PL, whose number formatting differs from the invariant one the file names are written in.</para>
        /// </summary>
        [Fact]
        public void TryParseImageFileName()
        {
            void Assertions()
            {
                Assert.True(Query.TryParseImageFileName("ABC_1_2015.jpeg", out string? reference, out short year));
                Assert.Equal("ABC_1", reference);
                Assert.Equal(2015, year);
                Assert.NotEqual("ABC", reference);
                Assert.NotEqual("ABC_12", reference);

                Assert.True(Query.TryParseImageFileName(@"C:\dataset\images\train\ABC_12_2015.jpeg", out string? reference_12, out _));
                Assert.Equal("ABC_12", reference_12);

                Assert.True(Query.TryParseImageFileName("272D6AAF-72C6-9B0E-E053-CC2BA8C0B5EA_2008.jpeg", out string? reference_Guid, out short year_Guid));
                Assert.Equal("272D6AAF-72C6-9B0E-E053-CC2BA8C0B5EA", reference_Guid);
                Assert.Equal(2008, year_Guid);

                Assert.False(Query.TryParseImageFileName("ABC.jpeg", out _, out _));
                Assert.False(Query.TryParseImageFileName("_2015.jpeg", out _, out _));
                Assert.False(Query.TryParseImageFileName("ABC_.jpeg", out _, out _));
                Assert.False(Query.TryParseImageFileName("ABC_20x5.jpeg", out _, out _));
                Assert.False(Query.TryParseImageFileName("ABC_-2015.jpeg", out _, out _));
                Assert.False(Query.TryParseImageFileName(null, out _, out _));
            }

            Assertions();

            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("pl-PL");
                Assertions();
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = cultureInfo;
            }
        }
    }
}
