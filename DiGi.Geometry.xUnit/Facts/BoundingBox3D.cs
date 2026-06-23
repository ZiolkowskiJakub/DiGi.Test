using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the InRange, On, and Inside checks on a <see cref="BoundingBox3D"/> instance.
        /// </summary>
        [Fact]
        public void BoundingBox3D()
        {
            BoundingBox3D boundingBox3D = new(new Point3D(0, 0, 0), new Point3D(10, 10, 10));

            Point3D point3D = new(5, 5, 5);

            bool inRange = boundingBox3D.InRange(point3D);
            bool on = boundingBox3D.On(point3D);
            bool inside = boundingBox3D.Inside(point3D);

            Assert.True(inRange);
            Assert.False(on);
            Assert.True(inside);
        }
    }
}