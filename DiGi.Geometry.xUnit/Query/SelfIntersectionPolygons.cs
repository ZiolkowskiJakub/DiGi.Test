using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;

namespace DiGi.Geometry.xUnit
{
    public partial class Query
    {
        [Fact]
        public void SelfIntersectionPolygons()
        {
            string json = "{\"_type\":\"DiGi.Geometry.Planar.Classes.Polygon2D,DiGi.Geometry\",\"Points\":[{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494824.9566894796,\"Y\":271915.3948833548},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494839.579993895,\"Y\":271912.53001110983},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494839.5799920246,\"Y\":271912.5300015625},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494827.84,\"Y\":271914.83},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494827.13,\"Y\":271910.64},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494824.13,\"Y\":271911.15},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494823.41,\"Y\":271907.5},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494826.27632121724,\"Y\":271906.9307306293},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494823.40853467543,\"Y\":271907.4925627457}]}";

            IPolygonal2D? polygonal2D = DiGi.Core.Convert.ToDiGi<IPolygonal2D>(json)?.FirstOrDefault();

            Assert.NotNull(polygonal2D);

            if(polygonal2D is null)
            {
                return;
            }

            List<Polygon2D>? polygon2Ds = Planar.Query.SelfIntersectionPolygons(polygonal2D, 1.0);

            Assert.NotNull(polygon2Ds);

            Assert.NotEmpty(polygon2Ds);

            Assert.Equal(4, polygon2Ds.Count);

            Assert.Equal(polygonal2D.GetArea(), polygon2Ds.ConvertAll(x => x.GetArea()).Sum(), DiGi.Core.Constans.Tolerance.MacroDistance);
        }
    }
}