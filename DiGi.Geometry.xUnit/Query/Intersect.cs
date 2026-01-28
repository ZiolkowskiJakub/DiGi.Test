using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;

namespace DiGi.Geometry.xUnit
{
    public partial class Query
    {
        [Fact]
        public void Intersect()
        {
            string json_1;
            string json_2;

            ISegmentable2D? segmentable2D_1;
            ISegmentable2D? segmentable2D_2;

            Action<ISegmentable2D, ISegmentable2D, bool> check = new((x, y, intersect) =>
            {
                bool intersect_Temp = Planar.Query.Intersect(x, y);
                Assert.Equal(intersect, intersect_Temp);

                List<Point2D>? point2Ds = Planar.Query.IntersectionPoints(x, y);
                if (intersect)
                {
                    Assert.NotNull(point2Ds);
                    Assert.NotEmpty(point2Ds);
                }
                else
                {
                    Assert.True(point2Ds == null || point2Ds.Count == 0);
                }
            });

            json_1 = "{\"_type\":\"DiGi.Geometry.Planar.Classes.Polygon2D,DiGi.Geometry\",\"Points\":[{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494485.178687,\"Y\":267377.063008},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494485.18,\"Y\":267376.94},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494485.24,\"Y\":267371.32},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494485.29,\"Y\":267370},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494485.3,\"Y\":267369.72},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494483.06,\"Y\":267369.64},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494483.08,\"Y\":267370.21},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494482.61,\"Y\":267370.26},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494482.528093,\"Y\":267377.030945}]}";
            json_2 = "{\"_type\":\"DiGi.Geometry.Planar.Classes.Segment2D,DiGi.Geometry\",\"Start\":{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494483.73936444987,\"Y\":267359.57192031393},\"Vector\":{\"_type\":\"DiGi.Geometry.Planar.Classes.Vector2D,DiGi.Geometry\",\"X\":-0.19924830744275823,\"Y\":16.471193411620334}}";

            segmentable2D_1 = DiGi.Core.Convert.ToDiGi<ISegmentable2D>(json_1)?.FirstOrDefault();
            Assert.NotNull(segmentable2D_1);

            segmentable2D_2 = DiGi.Core.Convert.ToDiGi<ISegmentable2D>(json_2)?.FirstOrDefault();
            Assert.NotNull(segmentable2D_2);

            check.Invoke(segmentable2D_1, segmentable2D_2, true);

            json_1 = "{\"_type\":\"DiGi.Geometry.Planar.Classes.Segment2D,DiGi.Geometry\",\"Start\":{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494483.73936444987,\"Y\":267359.57192031393},\"Vector\":{\"_type\":\"DiGi.Geometry.Planar.Classes.Vector2D,DiGi.Geometry\",\"X\":-0.19924830744275823,\"Y\":16.471193411620334}}";
            json_2 = "{\"_type\":\"DiGi.Geometry.Planar.Classes.Polygon2D,DiGi.Geometry\",\"Points\":[{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494485.178687,\"Y\":267377.063008},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494485.18,\"Y\":267376.94},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494485.24,\"Y\":267371.32},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494485.29,\"Y\":267370},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494485.3,\"Y\":267369.72},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494483.06,\"Y\":267369.64},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494483.08,\"Y\":267370.21},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494482.61,\"Y\":267370.26},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494482.528093,\"Y\":267377.030945}]}";

            segmentable2D_1 = DiGi.Core.Convert.ToDiGi<ISegmentable2D>(json_1)?.FirstOrDefault();
            Assert.NotNull(segmentable2D_1);

            segmentable2D_2 = DiGi.Core.Convert.ToDiGi<ISegmentable2D>(json_2)?.FirstOrDefault();
            Assert.NotNull(segmentable2D_2);

            check.Invoke(segmentable2D_1, segmentable2D_2, true);

            segmentable2D_1 = new Segment2D(0, 0, 0, 10);
            Assert.NotNull(segmentable2D_1);

            segmentable2D_2 = new Segment2D(1, 1, 1, 11);
            Assert.NotNull(segmentable2D_2);

            check.Invoke(segmentable2D_1, segmentable2D_2, false);
        }
    }
}