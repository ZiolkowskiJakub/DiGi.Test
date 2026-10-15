using DiGi.WebAPI.Classes;
using System.Collections.Generic;
using System.Globalization;

namespace DiGi.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a collection of integers is written as one occurrence of the parameter per value.
        /// <para>This is the only shape ASP.NET Core binds to an <c>int[]</c> action parameter: a single comma-separated value is handed to the converter whole and fails. The county update endpoints take every polygon part of a multi-part county this way, so a builder that could hold one value per name would silently post one part.</para>
        /// </summary>
        [Fact]
        public void UrlBuilder_RepeatsParameterPerValue()
        {
            UrlBuilder urlBuilder = new("gis/building/updateitemsbycountyids");
            urlBuilder.AddParameter("countyids", (IEnumerable<int>)[73482, 73485]);

            Assert.Equal("gis/building/updateitemsbycountyids?countyids=73482&countyids=73485", urlBuilder.Build());
        }

        /// <summary>
        /// Tests that a single-valued parameter still replaces rather than accumulates, and that the two kinds of parameter render together.
        /// <para>The collection overload was added to a builder that had always held one value per name, so the existing behaviour has to be unchanged - a caller setting the same name twice means the second value, not both.</para>
        /// </summary>
        [Fact]
        public void UrlBuilder_SingleValueReplaces()
        {
            UrlBuilder urlBuilder = new("gis/building/updateitems");
            urlBuilder.AddParameter("code", "2412");
            urlBuilder.AddParameter("code", "2405");

            Assert.Equal("gis/building/updateitems?code=2405", urlBuilder.Build());

            urlBuilder.AddParameter("countyids", (IEnumerable<int>)[1, 2]);

            Assert.Equal("gis/building/updateitems?code=2405&countyids=1&countyids=2", urlBuilder.Build());
        }

        /// <summary>
        /// Tests that nothing is added for a null or empty collection, and that a repeated parameter reads back as its first value.
        /// <para>An empty collection has to add nothing rather than a bare <c>name=</c>, which binds as one empty element rather than none.</para>
        /// </summary>
        [Fact]
        public void UrlBuilder_EmptyCollectionAndReadBack()
        {
            UrlBuilder urlBuilder = new("gis/building/updateitemsbycountyids");
            urlBuilder.AddParameter("countyids", (IEnumerable<int>?)null);
            urlBuilder.AddParameter("countyids", (IEnumerable<int>)[]);

            Assert.Equal("gis/building/updateitemsbycountyids", urlBuilder.Build());
            Assert.False(urlBuilder.TryGetValue("countyids", out int _));

            urlBuilder.AddParameter("countyids", (IEnumerable<int>)[73482, 73485]);

            Assert.True(urlBuilder.TryGetValue("countyids", out int countyId));
            Assert.Equal(73482, countyId);
        }

        /// <summary>
        /// Tests that numeric parameters are written in the invariant culture, whatever the current culture is.
        /// <para>ASP.NET Core binds a query string with <see cref="CultureInfo.InvariantCulture"/>, so a client running under pl-PL that wrote <c>radius=50,5</c> sent a value the server either rejects or reads differently - and an endpoint keyed by an exact <c>radius</c> then silently misses the stored row. sv-SE is checked as well because its negative sign is U+2212 rather than a hyphen-minus, which breaks a negative number the same way. The <see cref="long"/> above 2^53 guards the <c>long</c> overload: without it the argument binds to the <c>double</c> one and loses its last digit.</para>
        /// </summary>
        [Fact]
        public void UrlBuilder_InvariantCulture()
        {
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            try
            {
                foreach (string name in (string[])["pl-PL", "sv-SE"])
                {
                    CultureInfo.CurrentCulture = new(name);

                    UrlBuilder urlBuilder = new("gis/buildingsolarradiation/solarradiationbybuildingmodelid");
                    urlBuilder.AddParameter("radius", 50.5);
                    urlBuilder.AddParameter("x", -0.1 - 0.2);
                    urlBuilder.AddParameter("y", 1e-7);
                    urlBuilder.AddParameter("id", -12345);
                    urlBuilder.AddParameter("ids", (IEnumerable<int>)[-1, 2]);
                    urlBuilder.AddParameter("modelid", -9007199254740993L);

                    Assert.Equal("gis/buildingsolarradiation/solarradiationbybuildingmodelid?radius=50.5&x=-0.30000000000000004&y=1E-07&id=-12345&ids=-1&ids=2&modelid=-9007199254740993", urlBuilder.Build());

                    Assert.True(urlBuilder.TryGetValue("radius", out double radius));
                    Assert.Equal(50.5, radius);
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = cultureInfo;
            }
        }
    }
}
