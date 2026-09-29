using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.Classes;
using System;

namespace DiGi.GIS.YOLO.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the projection of a footprint box onto an orthophoto: the pixel corners for a known scale and location, the growth by the offset, clamping at the image edge, and a box wholly outside the image dropped.
        /// <para>The orthophoto&apos;s location is its top-left corner in world coordinates and its scale is pixels per metre, so a world point one metre right and one metre below the corner lands at (scale, scale). The offset is applied to a copy, so projecting the same box twice gives the same answer - otherwise the offset would grow year after year.</para>
        /// </summary>
        [Fact]
        public void PixelBoundingBox()
        {
            OrtoData ortoData = new(new DateTime(2015, 1, 1), null, 10, new Point2D(100, 200));

            BoundingBox2D boundingBox2D = new(new Point2D(101, 195), new Point2D(103, 199));

            BoundingBox2D? boundingBox2D_Pixel = Query.PixelBoundingBox(ortoData, boundingBox2D, 0, 100, 100, out bool clamped);
            Assert.NotNull(boundingBox2D_Pixel);
            Assert.False(clamped);
            Assert.Equal(10, boundingBox2D_Pixel!.Min.X, 9);
            Assert.Equal(10, boundingBox2D_Pixel.Min.Y, 9);
            Assert.Equal(30, boundingBox2D_Pixel.Max.X, 9);
            Assert.Equal(50, boundingBox2D_Pixel.Max.Y, 9);

            // Normalised the way the label file stores it: centre and size over the saved image's pixel size.
            DiGi.YOLO.Classes.BoundingBox? boundingBox = DiGi.YOLO.Create.BoundingBox(100, 100, boundingBox2D_Pixel.Min.X, boundingBox2D_Pixel.Min.Y, boundingBox2D_Pixel.Width, boundingBox2D_Pixel.Height);
            Assert.NotNull(boundingBox);
            Assert.Equal(0.2, boundingBox!.X, 9);
            Assert.Equal(0.3, boundingBox.Y, 9);
            Assert.Equal(0.2, boundingBox.Width, 9);
            Assert.Equal(0.4, boundingBox.Height, 9);

            // Grown by one metre - ten pixels - on every side, reaching exactly the image corner without crossing it.
            BoundingBox2D? boundingBox2D_Offset = Query.PixelBoundingBox(ortoData, boundingBox2D, 1, 100, 100, out bool clamped_Offset);
            Assert.NotNull(boundingBox2D_Offset);
            Assert.False(clamped_Offset);
            Assert.Equal(0, boundingBox2D_Offset!.Min.X, 9);
            Assert.Equal(0, boundingBox2D_Offset.Min.Y, 9);
            Assert.Equal(40, boundingBox2D_Offset.Max.X, 9);
            Assert.Equal(60, boundingBox2D_Offset.Max.Y, 9);

            // The source box is not grown by the call, so a second year of the same building sees the same box.
            Assert.Equal(101, boundingBox2D.Min.X, 9);
            BoundingBox2D? boundingBox2D_Offset_Again = Query.PixelBoundingBox(ortoData, boundingBox2D, 1, 100, 100, out _);
            Assert.Equal(boundingBox2D_Offset.Max.X, boundingBox2D_Offset_Again!.Max.X, 9);

            // Just inside the edge is left alone; just outside it is clamped.
            BoundingBox2D? boundingBox2D_Inside = Query.PixelBoundingBox(ortoData, new BoundingBox2D(new Point2D(100.001, 195), new Point2D(103, 199)), 0, 100, 100, out bool clamped_Inside);
            Assert.NotNull(boundingBox2D_Inside);
            Assert.False(clamped_Inside);

            BoundingBox2D? boundingBox2D_Crossing = Query.PixelBoundingBox(ortoData, new BoundingBox2D(new Point2D(99.999, 195), new Point2D(103, 199)), 0, 100, 100, out bool clamped_Crossing);
            Assert.NotNull(boundingBox2D_Crossing);
            Assert.True(clamped_Crossing);
            Assert.Equal(0, boundingBox2D_Crossing!.Min.X, 9);

            BoundingBox2D? boundingBox2D_Wide = Query.PixelBoundingBox(ortoData, new BoundingBox2D(new Point2D(95, 195), new Point2D(111, 199)), 0, 100, 100, out bool clamped_Wide);
            Assert.NotNull(boundingBox2D_Wide);
            Assert.True(clamped_Wide);
            Assert.Equal(0, boundingBox2D_Wide!.Min.X, 9);
            Assert.Equal(100, boundingBox2D_Wide.Max.X, 9);

            // Wholly outside the image: nothing is left once clamped, so there is no box.
            Assert.Null(Query.PixelBoundingBox(ortoData, new BoundingBox2D(new Point2D(50, 50), new Point2D(60, 60)), 0, 100, 100, out _));

            Assert.Null(Query.PixelBoundingBox(null, boundingBox2D, 0, 100, 100, out _));
            Assert.Null(Query.PixelBoundingBox(ortoData, null, 0, 100, 100, out _));
            Assert.Null(Query.PixelBoundingBox(ortoData, boundingBox2D, 0, 0, 100, out _));
        }
    }
}
