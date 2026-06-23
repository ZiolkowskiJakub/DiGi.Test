#pragma warning disable CA1416 // Validate platform compatibility

using System.Drawing;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Drawing;
using System.Collections.Generic;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the drawing extensions in DiGi.Geometry.Drawing, verifying safe handling of null parameters and valid drawing operations on a Graphics context.
        /// </summary>
        [Fact]
        public void Draw_Extensions()
        {
            // Null argument checks to verify safe handling and early exits
            Graphics? graphics_Null = null;
            Pen? pen_Null = null;
            Segment2D? segment2D_Null = null;

            // These calls should not throw exceptions
            graphics_Null.Draw(segment2D_Null, pen_Null);
            graphics_Null.Draw((Segment2D?)null, pen_Null);

            // Valid draw execution test using a memory-backed Graphics context
            using (Bitmap bitmap_Context = new(100, 100))
            using (Graphics graphics_Active = Graphics.FromImage(bitmap_Context))
            using (Pen pen_Active = new(Color.Red, 2f))
            {
                // 1. Draw Segment2D
                Segment2D segment2D_DrawTarget = new(new Point2D(0.0, 0.0), new Point2D(50.0, 50.0));
                graphics_Active.Draw(segment2D_DrawTarget, pen_Active);

                // 2. Draw BoundingBox2D
                BoundingBox2D boundingBox2D_DrawTarget = new(new Point2D(10.0, 10.0), new Point2D(40.0, 40.0));
                graphics_Active.Draw(boundingBox2D_DrawTarget, pen_Active, false);
                graphics_Active.Draw(boundingBox2D_DrawTarget, pen_Active, true);

                // 3. Draw Point2D
                Point2D point2D_DrawTarget = new(25.0, 25.0);
                graphics_Active.Draw(point2D_DrawTarget, pen_Active, false);
                graphics_Active.Draw(point2D_DrawTarget, pen_Active, true);

                // 4. Draw IPolygonal2D (Polygon2D)
                List<Point2D> point2Ds =
                [
                    new(10.0, 10.0),
                    new(20.0, 10.0),
                    new(20.0, 20.0),
                    new(10.0, 20.0)
                ];
                Polygon2D polygon2D_DrawTarget = new(point2Ds);
                graphics_Active.Draw(polygon2D_DrawTarget, pen_Active, false);
                graphics_Active.Draw(polygon2D_DrawTarget, pen_Active, true);

                // 5. Draw Mesh2D
                Mesh2D mesh2D_DrawTarget = new(point2Ds, new List<int[]> { new int[] { 0, 1, 2 }, new int[] { 0, 2, 3 } });
                graphics_Active.Draw(mesh2D_DrawTarget, pen_Active, false);
                graphics_Active.Draw(mesh2D_DrawTarget, pen_Active, true);
            }
        }
    }
}
