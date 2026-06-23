using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the creation of a <see cref="Rectangle2D"/> object from a list of points and a direction vector, verifying that all input points are contained within the generated rectangle.
        /// </summary>
        [Fact]
        public void Rectangle2D()
        {
            string json_Point2Ds = "[{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494827.54,\"Y\":271923.08},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494828.25,\"Y\":271926.77},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494831.07,\"Y\":271931.26},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494842.75,\"Y\":271928.98},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494841.06,\"Y\":271920.38},{\"_type\":\"DiGi.Geometry.Planar.Classes.Point2D,DiGi.Geometry\",\"X\":494829.35,\"Y\":271922.68}]";
            string json_Direction = "{\"_type\":\"DiGi.Geometry.Planar.Classes.Vector2D,DiGi.Geometry\",\"X\":0.7100000000209548,\"Y\":3.6900000000023283}";

            List<Point2D>? point2Ds = DiGi.Core.Convert.ToDiGi<Point2D>(json_Point2Ds);

            Assert.NotNull(point2Ds);
            Assert.NotEmpty(point2Ds);

            Vector2D? direction = DiGi.Core.Convert.ToDiGi<Vector2D>(json_Direction)?.FirstOrDefault();

            Assert.NotNull(direction);

            Rectangle2D? rectangle2D = Planar.Create.Rectangle2D(point2Ds, direction);

            Assert.NotNull(direction);

            if (rectangle2D is null)
            {
                return;
            }

            foreach (Point2D point2D in point2Ds)
            {
                Assert.True(rectangle2D.InRange(point2D));
            }
        }
    }
}