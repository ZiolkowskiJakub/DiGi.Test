using DiGi.Geometry.Planar.Classes;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Validates that <see cref="Query.TerrainQueryCircle(Circle2D?, double, double, double)"/> answers null for a missing circle and for a radius that cannot be requested.
        /// </summary>
        [Fact]
        public void TerrainQueryCircle_Null()
        {
            Circle2D? circle2D = null;
            Assert.Null(circle2D.TerrainQueryCircle());

            Assert.Null(new Circle2D(new Point2D(0, 0), 0).TerrainQueryCircle());
            Assert.Null(new Circle2D(new Point2D(0, 0), double.NaN).TerrainQueryCircle());
        }

        /// <summary>
        /// Validates that <see cref="Query.TerrainQueryCircle(Circle2D?, double, double, double)"/> grows the display circle by the clip buffer plus one diagonal of the coarsest lattice, keeping the centre.
        /// </summary>
        [Fact]
        public void TerrainQueryCircle_AddsBufferAndLatticeDiagonal()
        {
            Point2D center = new(627958.35, 500316.89);
            Circle2D circle2D = new(center, Constants.Default.TerrainRadius);

            Circle2D? circle2D_Query = circle2D.TerrainQueryCircle();
            Assert.NotNull(circle2D_Query);
            Assert.NotNull(circle2D_Query.Center);
            Assert.Equal(center.X, circle2D_Query.Center.X, 6);
            Assert.Equal(center.Y, circle2D_Query.Center.Y, 6);

            double radius_Expected = Constants.Default.TerrainRadius + Constants.Default.TerrainBuffer + (Constants.Default.TerrainLatticeStepMax * System.Math.Sqrt(2));
            Assert.Equal(radius_Expected, circle2D_Query.Radius, 3);

            // A finer lattice and no buffer grow the query by that lattice's diagonal only.
            Circle2D? circle2D_Query_Fine = circle2D.TerrainQueryCircle(0, 10);
            Assert.NotNull(circle2D_Query_Fine);
            Assert.Equal(Constants.Default.TerrainRadius + (10 * System.Math.Sqrt(2)), circle2D_Query_Fine.Radius, 3);
        }

        /// <summary>
        /// Validates that <see cref="Query.TerrainQueryCircle(Circle2D?, double, double, double)"/> never asks the terrain service for more than it accepts: the extension is cut where it would cross the cap, and a display radius beyond the cap is itself capped.
        /// </summary>
        [Fact]
        public void TerrainQueryCircle_ClampsToMaximumRadius()
        {
            Point2D center = new(0, 0);

            Circle2D? circle2D_Query_Near = new Circle2D(center, Constants.Default.TerrainRadiusMax - 50).TerrainQueryCircle();
            Assert.NotNull(circle2D_Query_Near);
            Assert.Equal(Constants.Default.TerrainRadiusMax, circle2D_Query_Near.Radius, 6);

            Circle2D? circle2D_Query_Beyond = new Circle2D(center, Constants.Default.TerrainRadiusMax + 1000).TerrainQueryCircle();
            Assert.NotNull(circle2D_Query_Beyond);
            Assert.Equal(Constants.Default.TerrainRadiusMax, circle2D_Query_Beyond.Radius, 6);

            // Just under the room needed for the full extension: the whole extension still fits.
            double extension = Constants.Default.TerrainBuffer + (Constants.Default.TerrainLatticeStepMax * System.Math.Sqrt(2));
            Circle2D? circle2D_Query_Fits = new Circle2D(center, Constants.Default.TerrainRadiusMax - extension - 1).TerrainQueryCircle();
            Assert.NotNull(circle2D_Query_Fits);
            Assert.Equal(Constants.Default.TerrainRadiusMax - 1, circle2D_Query_Fits.Radius, 6);
        }

        /// <summary>
        /// Validates that <see cref="Query.TerrainQueryBoundingBox(BoundingBox2D?, double, double)"/> moves every side of the display rectangle outwards by the clip buffer plus one diagonal of the coarsest lattice, and answers null for a missing rectangle.
        /// </summary>
        [Fact]
        public void TerrainQueryBoundingBox_GrowsEverySide()
        {
            BoundingBox2D? boundingBox2D_Null = null;
            Assert.Null(boundingBox2D_Null.TerrainQueryBoundingBox());

            BoundingBox2D boundingBox2D = new(new Point2D(100, 200), new Point2D(300, 500));

            BoundingBox2D? boundingBox2D_Query = boundingBox2D.TerrainQueryBoundingBox();
            Assert.NotNull(boundingBox2D_Query);

            double extension = Constants.Default.TerrainBuffer + (Constants.Default.TerrainLatticeStepMax * System.Math.Sqrt(2));
            Assert.Equal(100 - extension, boundingBox2D_Query.Min.X, 6);
            Assert.Equal(200 - extension, boundingBox2D_Query.Min.Y, 6);
            Assert.Equal(300 + extension, boundingBox2D_Query.Max.X, 6);
            Assert.Equal(500 + extension, boundingBox2D_Query.Max.Y, 6);
        }
    }
}
