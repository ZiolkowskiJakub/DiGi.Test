using DiGi.Geometry.Planar.Classes;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the intersection over union of two boxes: identical boxes 1, disjoint or merely touching boxes 0, a half overlap one third, and a missing box NaN.
        /// </summary>
        [Fact]
        public void IntersectionOverUnion()
        {
            BoundingBox2D boundingBox2D = new(0, 0, 2, 2);

            Assert.Equal(1, Query.IntersectionOverUnion(boundingBox2D, new BoundingBox2D(0, 0, 2, 2)), 9);
            Assert.Equal(0, Query.IntersectionOverUnion(boundingBox2D, new BoundingBox2D(5, 5, 2, 2)), 9);
            Assert.Equal(0, Query.IntersectionOverUnion(boundingBox2D, new BoundingBox2D(2, 0, 2, 2)), 9);

            // Shifted by half its width: an overlap of 2 over a union of 6.
            Assert.Equal(1.0 / 3.0, Query.IntersectionOverUnion(boundingBox2D, new BoundingBox2D(1, 0, 2, 2)), 9);
            Assert.Equal(Query.IntersectionOverUnion(new BoundingBox2D(1, 0, 2, 2), boundingBox2D), Query.IntersectionOverUnion(boundingBox2D, new BoundingBox2D(1, 0, 2, 2)), 9);

            Assert.True(double.IsNaN(Query.IntersectionOverUnion(null, boundingBox2D)));
            Assert.True(double.IsNaN(Query.IntersectionOverUnion(boundingBox2D, null)));
        }
    }
}
