using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Query
    {
        [Fact]
        public void Normals()
        {
            Polygon2D polygon2D = new([new Point2D(0, 0), new Point2D(0, 2), new Point2D(2, 2), new Point2D(2, 0)]);

            List<Segment2D>? segment2Ds = polygon2D.GetSegments();
            Assert.NotNull(segment2Ds);
            Assert.Equal(4, segment2Ds.Count);

            List<Vector2D?>? normals;

            normals = Planar.Query.Normals(polygon2D, Core.Enums.Side.External);
            Assert.NotNull(normals);
            Assert.Equal(4, normals.Count);

            for (int i = 0; i < normals.Count; i++)
            {
                Point2D? point2D = segment2Ds[i].Mid();
                Assert.NotNull(point2D);

                point2D.Move(normals[i]);

                Assert.True(!polygon2D.Inside(point2D));
            }

            normals = Planar.Query.Normals(polygon2D, Core.Enums.Side.Internal);
            Assert.NotNull(normals);
            Assert.Equal(4, normals.Count);

            for (int i = 0; i < normals.Count; i++)
            {
                Point2D? point2D = segment2Ds[i].Mid();
                Assert.NotNull(point2D);

                point2D.Move(normals[i]);

                Assert.True(polygon2D.Inside(point2D));
            }
        }
    }
}