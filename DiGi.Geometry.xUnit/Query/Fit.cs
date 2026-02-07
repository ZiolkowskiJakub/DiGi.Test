using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Query
    {
        [Fact]
        public void Fit()
        {
            BoundingBox2D boundingBox2D = new(new Point2D(0, 0), new Point2D(10, 10));

            BoundingBox2D boundingBox2D_Temp = new(new Point2D(-1, -1), new Point2D(11, 11));

            BoundingBox2D? boundingBox2D_Fit = boundingBox2D.Fit(boundingBox2D_Temp);
            Assert.NotNull(boundingBox2D_Fit);

            Assert.True(boundingBox2D.Min.Distance(boundingBox2D_Fit.Min) < DiGi.Core.Constants.Tolerance.Distance);
            Assert.True(boundingBox2D.Max.Distance(boundingBox2D_Fit.Max) < DiGi.Core.Constants.Tolerance.Distance);

            Segment2D segment2D = new(new Point2D(-1, -1), new Point2D(11, 11));

            Segment2D? segment2D_Fit = boundingBox2D.Fit(segment2D);
            Assert.NotNull(segment2D_Fit);

            Assert.True(boundingBox2D.Min.Distance(segment2D_Fit[0]) < DiGi.Core.Constants.Tolerance.Distance);
            Assert.True(boundingBox2D.Max.Distance(segment2D_Fit[1]) < DiGi.Core.Constants.Tolerance.Distance);
        }
    }
}