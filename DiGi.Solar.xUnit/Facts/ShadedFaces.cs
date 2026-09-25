using DiGi.Geometry.Planar.Classes;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that overlapping shadows are merged before they are measured: two 4 x 4 squares overlapping by 2 x 2 shade 28, not 32, and the merged shadow is one face.
        /// <para>The fallback of <see cref="Query.ShadowFaces(PolygonalFace2D?, IEnumerable{PolygonalFace2D})"/> would count the overlap twice here, so this fact also shows the shared post-processing takes the union path whenever the union succeeds.</para>
        /// </summary>
        [Fact]
        public void ShadedFaces_OverlappingShadows_AreMerged()
        {
            PolygonalFace2D polygonalFace2D_Receiver = ShadowFacesReceiver(10.0);

            List<PolygonalFace2D> polygonalFace2Ds_Shadow =
            [
                ShadowFacesRectangle(0.0, 0.0, 4.0, 4.0),
                ShadowFacesRectangle(2.0, 2.0, 6.0, 6.0)
            ];

            List<PolygonalFace2D>? polygonalFace2Ds_Result = polygonalFace2D_Receiver.ShadedFaces(polygonalFace2Ds_Shadow);
            Assert.NotNull(polygonalFace2Ds_Result);
            Assert.Single(polygonalFace2Ds_Result);
            Assert.Equal(28.0, ShadowFacesArea(polygonalFace2Ds_Result), 6);

            List<PolygonalFace2D>? polygonalFace2Ds_Fallback = polygonalFace2D_Receiver.ShadowFaces(polygonalFace2Ds_Shadow);
            Assert.NotNull(polygonalFace2Ds_Fallback);
            Assert.Equal(32.0, ShadowFacesArea(polygonalFace2Ds_Fallback), 6);
        }

        /// <summary>
        /// Tests that the merged shadow is clipped to the receiver: of a 4 x 4 square overhanging a 10 x 10 receiver corner by 2 in x and y, only the 2 x 2 part on the receiver is reported.
        /// </summary>
        [Fact]
        public void ShadedFaces_ShadowOutsideTheReceiver_IsClipped()
        {
            PolygonalFace2D polygonalFace2D_Receiver = ShadowFacesReceiver(10.0);

            List<PolygonalFace2D>? polygonalFace2Ds_Result = polygonalFace2D_Receiver.ShadedFaces([ShadowFacesRectangle(8.0, 8.0, 12.0, 12.0), ShadowFacesRectangle(20.0, 20.0, 22.0, 22.0)]);
            Assert.NotNull(polygonalFace2Ds_Result);
            Assert.Equal(4.0, ShadowFacesArea(polygonalFace2Ds_Result), 6);
        }

        /// <summary>
        /// Tests that a hole in the merged shadow is kept: four strips framing a 2 x 2 gap shade the frame only, so a ring-shaped shadow is not over-counted.
        /// </summary>
        [Fact]
        public void ShadedFaces_RingShadow_KeepsItsHole()
        {
            PolygonalFace2D polygonalFace2D_Receiver = ShadowFacesReceiver(10.0);

            List<PolygonalFace2D> polygonalFace2Ds_Shadow =
            [
                ShadowFacesRectangle(2.0, 2.0, 8.0, 4.0),
                ShadowFacesRectangle(2.0, 6.0, 8.0, 8.0),
                ShadowFacesRectangle(2.0, 4.0, 4.0, 6.0),
                ShadowFacesRectangle(6.0, 4.0, 8.0, 6.0)
            ];

            List<PolygonalFace2D>? polygonalFace2Ds_Result = polygonalFace2D_Receiver.ShadedFaces(polygonalFace2Ds_Shadow);
            Assert.NotNull(polygonalFace2Ds_Result);

            // A 6 x 6 frame with a 2 x 2 hole.
            Assert.Equal(32.0, ShadowFacesArea(polygonalFace2Ds_Result), 6);
        }

        /// <summary>
        /// Tests that a receiver reached by no shadow gives an empty list, not null, for both an empty and a null shadow set: the solvers emit a fully sunlit result (shaded area 0) for it.
        /// </summary>
        [Fact]
        public void ShadedFaces_NoShadow_ReturnsEmpty()
        {
            PolygonalFace2D polygonalFace2D_Receiver = ShadowFacesReceiver(10.0);

            List<PolygonalFace2D>? polygonalFace2Ds_Empty = polygonalFace2D_Receiver.ShadedFaces([]);
            Assert.NotNull(polygonalFace2Ds_Empty);
            Assert.Empty(polygonalFace2Ds_Empty);

            List<PolygonalFace2D>? polygonalFace2Ds_Null = polygonalFace2D_Receiver.ShadedFaces(null);
            Assert.NotNull(polygonalFace2Ds_Null);
            Assert.Empty(polygonalFace2Ds_Null);
        }

        /// <summary>
        /// Tests the null handling: without a receiver face there is nothing to clip against, so the caller gets null and emits no result rather than a factor it cannot bound.
        /// <para>The fallback taken when the union fails (ZiolkowskiJakub/DiGi.Solar#8) is <see cref="Query.ShadowFaces(PolygonalFace2D?, IEnumerable{PolygonalFace2D})"/>, covered by the <c>ShadowFaces_*</c> facts. No input found reaches it through <see cref="Query.ShadedFaces(PolygonalFace2D?, IEnumerable{PolygonalFace2D}?)"/>:
        /// since the snap-rounding retry of ZiolkowskiJakub/DiGi.Geometry#8, a bow-tie, a NaN, an infinite and a 1e300 coordinate all merge without failing (probed for ZiolkowskiJakub/DiGi.Solar#11).</para>
        /// </summary>
        [Fact]
        public void ShadedFaces_NullReceiver_ReturnsNull()
        {
            PolygonalFace2D? polygonalFace2D_Receiver = null;

            Assert.Null(polygonalFace2D_Receiver.ShadedFaces([ShadowFacesRectangle(0.0, 0.0, 5.0, 5.0)]));
        }
    }
}
