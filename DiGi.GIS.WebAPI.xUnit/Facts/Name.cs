namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the named HTTP client identifiers keep their exact wire values.
        /// </summary>
        [Fact]
        public void Name_Client_Identifiers()
        {
            Assert.Equal("GIS", Constants.Name.Client.GIS);
            Assert.Equal("Geoportal", Constants.Name.Client.Geoportal);
            Assert.Equal("GUGiK", Constants.Name.Client.GUGiK);
        }
    }
}
