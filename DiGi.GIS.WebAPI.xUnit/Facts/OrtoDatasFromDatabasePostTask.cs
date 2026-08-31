using DiGi.GIS.WebAPI.Classes;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="OrtoDatasFromDatabasePostTask"/> initializes with expected default property values and implements expected task interfaces.
        /// </summary>
        [Fact]
        public void OrtoDatasFromDatabasePostTask_Defaults()
        {
            GISWebAPIManager gISWebAPIManager = new(null);
            OrtoDatasFromDatabasePostTask task = new(gISWebAPIManager);

            Assert.NotNull(task);
            Assert.Equal(5, task.Count);
            Assert.NotNull(task.OrtoDatasBuilding2DOptions);
            Assert.IsAssignableFrom<OrtoDatasPostTask>(task);
            Assert.IsAssignableFrom<DiGi.GIS.WebAPI.Interfaces.IGISWebAPIObject>(task);
        }
    }
}
