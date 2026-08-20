using DiGi.WebAPI.Classes;
using System.Collections.Generic;

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
    }
}
