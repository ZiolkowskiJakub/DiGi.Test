using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.Classes;
using System.Linq;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the Update_Building2D method to verify that orthophotomap image links are formatted using the default base URL and a custom API base URL.
        /// </summary>
        [Fact]
        public void Update_Building2D_ApiBaseUrl()
        {
            string json = "{\"_type\":\"DiGi.GIS.Classes.Building2D,DiGi.GIS\",\"Guid\":\"8531176b-ae1f-4dbc-b864-e71ab24a6af2\",\"Reference\":\"1fc24a0d-8d0c-4e15-b6d2-ea52124f30b7\",\"PolygonalFace2D\":{\"_type\":\"DiGi.Geometry.Planar.Classes.PolygonalFace2D,DiGi.Geometry\",\"ExternalEdge\":{\"_type\":\"DiGi.Geometry.Planar.Classes.Polygon2D,DiGi.Geometry\",\"Points\":[{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":482430.74,\"Y\":559048.76},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":482425.19,\"Y\":559050.29},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":482427.49,\"Y\":559058.66},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":482433.15,\"Y\":559057.01}]},\"InternalEdges\":null},\"Storeys\":1,\"BuildingGeneralFunction\":4,\"BuildingSpecificFunctions\":[7],\"BuildingPhase\":0}";

            Building2D? building2D = Core.Convert.ToDiGi<Building2D>(json)?.FirstOrDefault();
            Assert.NotNull(building2D);

            Table table_Default = new();
            GIS.IO.Modify.Update_Building2D(table_Default, 2212, [building2D]);

            Row? row_Default = table_Default.GetRow(0);
            Assert.NotNull(row_Default);

            string? columnName_2008 = GIS.IO.Create.Column_OrthophotomapImage(2008)?.Name;
            Assert.NotNull(columnName_2008);

            Column? column_2008 = table_Default.Columns?.FirstOrDefault(c => c.Name == columnName_2008);
            Assert.NotNull(column_2008);

            Assert.True(row_Default.TryGetValue(column_2008.Index, out string? link_Default));
            Assert.NotNull(link_Default);
            Assert.StartsWith(GIS.IO.Constants.WebAPI.BaseUri, link_Default);
            Assert.Contains("reference=1fc24a0d-8d0c-4e15-b6d2-ea52124f30b7", link_Default);

            Table table_Custom = new();
            string customBaseUrl = "https://staging.digiproject.uk";
            GIS.IO.Modify.Update_Building2D(table_Custom, 2212, [building2D], customBaseUrl);

            Row? row_Custom = table_Custom.GetRow(0);
            Assert.NotNull(row_Custom);

            Column? column_2008_Custom = table_Custom.Columns?.FirstOrDefault(c => c.Name == columnName_2008);
            Assert.NotNull(column_2008_Custom);

            Assert.True(row_Custom.TryGetValue(column_2008_Custom.Index, out string? link_Custom));
            Assert.NotNull(link_Custom);
            Assert.StartsWith(customBaseUrl, link_Custom);
            Assert.Contains("reference=1fc24a0d-8d0c-4e15-b6d2-ea52124f30b7", link_Custom);
        }
    }
}
