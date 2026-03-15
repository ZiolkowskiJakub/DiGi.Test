using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Query
    {
        [Fact]
        public void Difference()
        {
            PolygonalFace2D? polygonalFace2D_1 = DiGi.Core.Convert.ToDiGi<PolygonalFace2D>("{\"_type\":\"DiGi.Geometry.Planar.Classes.PolygonalFace2D,DiGi.Geometry\",\"ExternalEdge\":{\"_type\":\"DiGi.Geometry.Planar.Classes.Rectangle2D,DiGi.Geometry\",\"Height\":10.753366914694197,\"HeightDirection\":{\"_type\":\"DiGi.Geometry.Planar.Classes.Vector2D,DiGi.Geometry\",\"X\":0.09950371901616256,\"Y\":0.9950371902104728},\"Origin\":{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":621017.1408910891,\"Y\":520324.1189108909},\"Width\":13.958381704287603},\"InternalEdges\":null}")?.FirstOrDefault();
            Assert.NotNull(polygonalFace2D_1);

            PolygonalFace2D? polygonalFace2D_2 = DiGi.Core.Convert.ToDiGi<PolygonalFace2D>("{\"_type\":\"DiGi.Geometry.Planar.Classes.PolygonalFace2D,DiGi.Geometry\",\"ExternalEdge\":{\"_type\":\"DiGi.Geometry.Planar.Classes.Polygon2D,DiGi.Geometry\",\"Points\":[{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":621018.21,\"Y\":520334.81},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":621032.1,\"Y\":520333.43},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":621031.03,\"Y\":520322.73},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":621026.16,\"Y\":520323.22},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":621026.4,\"Y\":520325.61},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":621017.38,\"Y\":520326.51},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":621018.21,\"Y\":520334.81}]},\"InternalEdges\":null}")?.FirstOrDefault();
            Assert.NotNull(polygonalFace2D_2);

            List<PolygonalFace2D>? polygonalFace2Ds = Planar.Query.Difference(polygonalFace2D_1, polygonalFace2D_2);
            Assert.NotNull(polygonalFace2Ds);
            Assert.True(polygonalFace2Ds.Count == 1);

            PolygonalFace2D? polygonalFace2D = polygonalFace2Ds.FirstOrDefault();
            Assert.NotNull(polygonalFace2D);

            Assert.True(DiGi.Core.Query.AlmostEquals(polygonalFace2D_1.GetArea() - polygonalFace2D_2.GetArea(), polygonalFace2D.GetArea(), DiGi.Core.Constants.Tolerance.MacroDistance));
        }
    }
}