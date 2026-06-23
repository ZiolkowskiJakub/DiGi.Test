using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the functionality of the <see cref="Grid2D"/> class, verifying the creation of the grid,
        /// the correctness of the bounding rectangle, the number and area of generated cell rectangles,
        /// and that their centroids are contained within the grid boundaries.
        /// </summary>
        [Fact]
        public void Grid2D()
        {
            Grid2D grid = new(new Point2D(1, 1), 10, 5, new Vector2D(0, 1), 5, 5);

            Rectangle2D? rectiangle2D_Grid2D = grid.Rectangle2D;
            Assert.NotNull(rectiangle2D_Grid2D);

            List<Rectangle2D>? rectangle2Ds = grid.GetRectangles();

            Assert.NotNull(rectangle2Ds);
            Assert.NotEmpty(rectangle2Ds);
            Assert.Equal(25, rectangle2Ds.Count);

            foreach (Rectangle2D rectangle2D in rectangle2Ds)
            {
                Assert.NotNull(rectangle2D);

                Assert.True(DiGi.Core.Query.AlmostEquals(rectangle2D.GetArea(), 2));

                Point2D? centroid = rectangle2D.GetCentroid();
                Assert.NotNull(centroid);

                Assert.True(rectiangle2D_Grid2D.Inside(centroid));
            }

            DiGi.Core.xUnit.Query.SerializationCheck(grid);
        }
    }
}