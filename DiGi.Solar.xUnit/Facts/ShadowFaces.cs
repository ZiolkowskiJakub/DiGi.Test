using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that shadows that do not overlap each other come out of <see cref="DiGi.Solar.Query"/> at their own areas: for a non-overlapping set the clip-and-cap fallback is the union, not an approximation of it.
        /// </summary>
        [Fact]
        public void ShadowFaces_DisjointShadows_ArePassedThroughAtTheirExactArea()
        {
            PolygonalFace2D polygonalFace2D_Receiver = ShadowFacesReceiver(10.0);

            List<PolygonalFace2D> polygonalFace2Ds_Shadow =
            [
                ShadowFacesRectangle(0.0, 0.0, 2.0, 2.0),
                ShadowFacesRectangle(3.0, 0.0, 5.0, 2.0),
                ShadowFacesRectangle(6.0, 0.0, 8.0, 2.0)
            ];

            List<PolygonalFace2D>? polygonalFace2Ds_Result = polygonalFace2D_Receiver.ShadowFaces(polygonalFace2Ds_Shadow);
            Assert.NotNull(polygonalFace2Ds_Result);
            Assert.Equal(3, polygonalFace2Ds_Result.Count);

            // Four plus four plus four: nothing merged away, nothing dropped by the cap, the area the union of the same three faces would give.
            Assert.Equal(12.0, ShadowFacesArea(polygonalFace2Ds_Result), 6);
        }

        /// <summary>
        /// Tests that overlapping shadows are counted only up to the area of the receiver: the fallback has no merge, so the receiver is the only bound left, and a shaded receiver must never read as covered by more than itself.
        /// <para>The guard bites here: with the cap check removed from the fallback this fact fails on the summed area, which is then double counted.</para>
        /// </summary>
        [Fact]
        public void ShadowFaces_OverlappingShadows_AreCappedAtTheReceiverArea()
        {
            PolygonalFace2D polygonalFace2D_Receiver = ShadowFacesReceiver(10.0);

            // Three 10 x 5 m strips, each fully inside the receiver and each overlapping both others: 150 m2 of shadow on a 100 m2 receiver.
            List<PolygonalFace2D> polygonalFace2Ds_Shadow =
            [
                ShadowFacesRectangle(0.0, 0.0, 10.0, 5.0),
                ShadowFacesRectangle(0.0, 1.0, 10.0, 6.0),
                ShadowFacesRectangle(0.0, 2.0, 10.0, 7.0)
            ];

            List<PolygonalFace2D>? polygonalFace2Ds_Result = polygonalFace2D_Receiver.ShadowFaces(polygonalFace2Ds_Shadow);
            Assert.NotNull(polygonalFace2Ds_Result);

            double area_Total = ShadowFacesArea(polygonalFace2Ds_Result);
            Assert.True(area_Total > 0.0, $"Overlapping shadows read {area_Total}; a failed merge must still report shade.");
            Assert.True(area_Total <= 100.0 + 1e-6, $"Overlapping shadows sum to {area_Total}, over the 100 m2 receiver.");

            // The strips are accepted in input order while they fit: two of them fill the cap exactly, and the third is dropped rather than pushed past it.
            Assert.Equal(2, polygonalFace2Ds_Result.Count);
            Assert.Equal(100.0, area_Total, 6);
        }

        /// <summary>
        /// Tests that the fallback clips each shadow to the receiver: only the part of a shadow that lies on the receiver is reported.
        /// </summary>
        [Fact]
        public void ShadowFaces_FacesOutsideTheReceiver_AreClipped()
        {
            PolygonalFace2D polygonalFace2D_Receiver = ShadowFacesReceiver(10.0);

            // A 10 x 10 m square reaching from y = -5 to y = 5: half of it, 25 m2, is on the receiver.
            List<PolygonalFace2D> polygonalFace2Ds_Shadow = [ShadowFacesRectangle(5.0, -5.0, 15.0, 5.0)];

            List<PolygonalFace2D>? polygonalFace2Ds_Result = polygonalFace2D_Receiver.ShadowFaces(polygonalFace2Ds_Shadow);
            Assert.NotNull(polygonalFace2Ds_Result);

            // The union path clips the same way (it intersects each merged piece with the receiver), so the fallback is not the only thing relying on the clip.
            Assert.Equal(25.0, ShadowFacesArea(polygonalFace2Ds_Result), 6);
        }

        /// <summary>
        /// Tests the null handling of the fallback: without a receiver face there is nothing to clip or cap against, so the caller gets null and emits no result rather than a factor it cannot bound.
        /// </summary>
        [Fact]
        public void ShadowFaces_NullReceiver_ReturnsNull()
        {
            PolygonalFace2D? polygonalFace2D_Receiver = null;
            List<PolygonalFace2D> polygonalFace2Ds_Shadow = [ShadowFacesRectangle(0.0, 0.0, 5.0, 5.0)];

            Assert.Null(polygonalFace2D_Receiver.ShadowFaces(polygonalFace2Ds_Shadow));
        }

        /// <summary>
        /// Tests that a receiver reached by no shadow stays empty rather than null, matching the fully sunlit result the solvers emit for it.
        /// </summary>
        [Fact]
        public void ShadowFaces_EmptyShadow_ReturnsEmpty()
        {
            PolygonalFace2D polygonalFace2D_Receiver = ShadowFacesReceiver(10.0);

            List<PolygonalFace2D>? polygonalFace2Ds_Result = polygonalFace2D_Receiver.ShadowFaces([]);
            Assert.NotNull(polygonalFace2Ds_Result);
            Assert.Empty(polygonalFace2Ds_Result);
        }

        /// <summary>
        /// Tests that the cap of the fallback is the divisor of the shading factor: the area of a solver's 3D receiver face is the area of its planar 2D face, which is what the fallback caps against.
        /// </summary>
        [Fact]
        public void ShadowFaces_CapEqualsSolverDivisor()
        {
            Plane plane_Receiver = new(new Point3D(0.0, 0.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
            PolygonalFace3D? polygonalFace3D_Receiver = Geometry.Spatial.Create.PolygonalFace3D(plane_Receiver, new Point2D(0.0, 0.0), new Point2D(4.0, 0.0), new Point2D(4.0, 4.0), new Point2D(0.0, 4.0));
            Assert.NotNull(polygonalFace3D_Receiver);
            Assert.IsType<PolygonalFace2D>(polygonalFace3D_Receiver.Geometry2D);

            Assert.Equal(polygonalFace3D_Receiver.GetArea(), ((PolygonalFace2D)polygonalFace3D_Receiver.Geometry2D).GetArea(), 9);
        }

        /// <summary>
        /// Builds the receiver face the fallback clips and caps against: a square from (0, 0) to (sideLength, sideLength).
        /// </summary>
        /// <param name="sideLength">The side length of the square receiver face, in metres.</param>
        /// <returns>The receiver face.</returns>
        private static PolygonalFace2D ShadowFacesReceiver(double sideLength)
        {
            PolygonalFace2D? polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D(new Point2D(0.0, 0.0), new Point2D(sideLength, 0.0), new Point2D(sideLength, sideLength), new Point2D(0.0, sideLength));
            Assert.NotNull(polygonalFace2D);
            return polygonalFace2D;
        }

        /// <summary>
        /// Builds an axis-aligned rectangle face from two opposite corners.
        /// </summary>
        /// <param name="x_Min">The minimum x of the rectangle.</param>
        /// <param name="y_Min">The minimum y of the rectangle.</param>
        /// <param name="x_Max">The maximum x of the rectangle.</param>
        /// <param name="y_Max">The maximum y of the rectangle.</param>
        /// <returns>The rectangle face.</returns>
        private static PolygonalFace2D ShadowFacesRectangle(double x_Min, double y_Min, double x_Max, double y_Max)
        {
            PolygonalFace2D? polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D(new Point2D(x_Min, y_Min), new Point2D(x_Max, y_Min), new Point2D(x_Max, y_Max), new Point2D(x_Min, y_Max));
            Assert.NotNull(polygonalFace2D);
            return polygonalFace2D;
        }

        /// <summary>
        /// Sums the areas of shadow faces, the way the solver results do.
        /// </summary>
        /// <param name="polygonalFace2Ds">The faces to sum.</param>
        /// <returns>The summed area.</returns>
        private static double ShadowFacesArea(IEnumerable<PolygonalFace2D> polygonalFace2Ds)
        {
            double area = 0;
            foreach (PolygonalFace2D polygonalFace2D in polygonalFace2Ds)
            {
                area += polygonalFace2D.GetArea();
            }

            return area;
        }
    }
}
