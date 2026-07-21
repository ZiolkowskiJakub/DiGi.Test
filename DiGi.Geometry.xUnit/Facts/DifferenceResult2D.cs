using DiGi.Geometry.Core.Enums;
using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests 2D boolean difference between two disjoint polygonal faces: the first face is returned unchanged.
        /// </summary>
        [Fact]
        public void PolygonalFace2D_DifferenceResult2D_Disjoint()
        {
            PolygonalFace2D? polygonalFace2D_1 = Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2));
            PolygonalFace2D? polygonalFace2D_2 = Create.PolygonalFace2D(new Point2D(10, 10), new Point2D(12, 10), new Point2D(12, 12), new Point2D(10, 12));
            Assert.NotNull(polygonalFace2D_1);
            Assert.NotNull(polygonalFace2D_2);

            DifferenceResult2D? differenceResult2D = Create.DifferenceResult2D(polygonalFace2D_1, polygonalFace2D_2);
            Assert.NotNull(differenceResult2D);
            Assert.Equal(BooleanOpertaionType.Difference, differenceResult2D.BooleanOpertaionType);
            Assert.True(differenceResult2D.Any());

            List<PolygonalFace2D>? polygonalFace2Ds = differenceResult2D.GetGeometry2Ds<PolygonalFace2D>();
            Assert.NotNull(polygonalFace2Ds);
            Assert.Single(polygonalFace2Ds);
            Assert.Equal(4.0, polygonalFace2Ds[0].GetArea(), 3);
        }

        /// <summary>
        /// Tests 2D boolean difference between two identical polygonal faces: the result is empty.
        /// </summary>
        [Fact]
        public void PolygonalFace2D_DifferenceResult2D_Identical()
        {
            PolygonalFace2D? polygonalFace2D = Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2));
            Assert.NotNull(polygonalFace2D);

            DifferenceResult2D? differenceResult2D = Create.DifferenceResult2D(polygonalFace2D, polygonalFace2D);
            Assert.NotNull(differenceResult2D);
            Assert.False(differenceResult2D.Any());
            Assert.Equal(0, differenceResult2D.Count);
        }

        /// <summary>
        /// Tests 2D boolean difference between two overlapping polygonal faces: an L-shaped remainder of the expected area.
        /// </summary>
        [Fact]
        public void PolygonalFace2D_DifferenceResult2D_Overlapping()
        {
            PolygonalFace2D? polygonalFace2D_1 = Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(4, 0), new Point2D(4, 4), new Point2D(0, 4));
            PolygonalFace2D? polygonalFace2D_2 = Create.PolygonalFace2D(new Point2D(2, 2), new Point2D(6, 2), new Point2D(6, 6), new Point2D(2, 6));
            Assert.NotNull(polygonalFace2D_1);
            Assert.NotNull(polygonalFace2D_2);

            DifferenceResult2D? differenceResult2D = Create.DifferenceResult2D(polygonalFace2D_1, polygonalFace2D_2);
            Assert.NotNull(differenceResult2D);
            Assert.True(differenceResult2D.Any());

            List<PolygonalFace2D>? polygonalFace2Ds = differenceResult2D.GetGeometry2Ds<PolygonalFace2D>();
            Assert.NotNull(polygonalFace2Ds);
            Assert.Equal(12.0, polygonalFace2Ds.Sum(x => x.GetArea()), 3);
        }

        /// <summary>
        /// Tests that subtracting a face fully contained inside another produces a single face with a hole (internal edge).
        /// </summary>
        [Fact]
        public void PolygonalFace2D_DifferenceResult2D_HoleCreation()
        {
            PolygonalFace2D? polygonalFace2D_Outer = Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 10), new Point2D(0, 10));
            PolygonalFace2D? polygonalFace2D_Inner = Create.PolygonalFace2D(new Point2D(3, 3), new Point2D(7, 3), new Point2D(7, 7), new Point2D(3, 7));
            Assert.NotNull(polygonalFace2D_Outer);
            Assert.NotNull(polygonalFace2D_Inner);

            DifferenceResult2D? differenceResult2D = Create.DifferenceResult2D(polygonalFace2D_Outer, polygonalFace2D_Inner);
            Assert.NotNull(differenceResult2D);

            List<PolygonalFace2D>? polygonalFace2Ds = differenceResult2D.GetGeometry2Ds<PolygonalFace2D>();
            Assert.NotNull(polygonalFace2Ds);
            Assert.Single(polygonalFace2Ds);
            Assert.Equal(84.0, polygonalFace2Ds[0].GetArea(), 3);
            Assert.NotNull(polygonalFace2Ds[0].InternalEdges);
            Assert.Single(polygonalFace2Ds[0].InternalEdges!);

            DiGi.Core.xUnit.Query.SerializationCheck(differenceResult2D);
        }

        /// <summary>
        /// Tests that subtracting a covering face produces an empty result (the first face is fully consumed).
        /// </summary>
        [Fact]
        public void PolygonalFace2D_DifferenceResult2D_ContainedInSecond()
        {
            PolygonalFace2D? polygonalFace2D_Small = Create.PolygonalFace2D(new Point2D(4, 4), new Point2D(6, 4), new Point2D(6, 6), new Point2D(4, 6));
            PolygonalFace2D? polygonalFace2D_Big = Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 10), new Point2D(0, 10));
            Assert.NotNull(polygonalFace2D_Small);
            Assert.NotNull(polygonalFace2D_Big);

            DifferenceResult2D? differenceResult2D = Create.DifferenceResult2D(polygonalFace2D_Small, polygonalFace2D_Big);
            Assert.NotNull(differenceResult2D);
            Assert.False(differenceResult2D.Any());
        }

        /// <summary>
        /// Tests that subtracting an edge-touching face removes nothing: the first face keeps its full area.
        /// </summary>
        [Fact]
        public void PolygonalFace2D_DifferenceResult2D_EdgeTouch()
        {
            PolygonalFace2D? polygonalFace2D_1 = Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2));
            PolygonalFace2D? polygonalFace2D_2 = Create.PolygonalFace2D(new Point2D(2, 0), new Point2D(4, 0), new Point2D(4, 2), new Point2D(2, 2));
            Assert.NotNull(polygonalFace2D_1);
            Assert.NotNull(polygonalFace2D_2);

            DifferenceResult2D? differenceResult2D = Create.DifferenceResult2D(polygonalFace2D_1, polygonalFace2D_2);
            Assert.NotNull(differenceResult2D);

            List<PolygonalFace2D>? polygonalFace2Ds = differenceResult2D.GetGeometry2Ds<PolygonalFace2D>();
            Assert.NotNull(polygonalFace2Ds);
            Assert.Single(polygonalFace2Ds);
            Assert.Equal(4.0, polygonalFace2Ds[0].GetArea(), 3);
        }

        /// <summary>
        /// Tests the lower-dimensional difference path: a degenerate (zero-area, collinear) first face collapses to its boundary linework, so the difference consists of the segments lying outside the second face.
        /// <para>The degenerate face spans (0,0)-(10,0); the subtracted square covers x in [4,6], leaving the segments (0,0)-(4,0) and (6,0)-(10,0) with a total length of 8.</para>
        /// </summary>
        [Fact]
        public void PolygonalFace2D_DifferenceResult2D_Degenerate_LowerDimensional()
        {
            Polygon2D polygon2D_Degenerate = new([new Point2D(0, 0), new Point2D(10, 0), new Point2D(5, 0)]);
            PolygonalFace2D? polygonalFace2D_Degenerate = Create.PolygonalFace2D(polygon2D_Degenerate);
            Assert.NotNull(polygonalFace2D_Degenerate);

            PolygonalFace2D? polygonalFace2D_2 = Create.PolygonalFace2D(new Point2D(4, -1), new Point2D(6, -1), new Point2D(6, 1), new Point2D(4, 1));
            Assert.NotNull(polygonalFace2D_2);

            DifferenceResult2D? differenceResult2D = Create.DifferenceResult2D(polygonalFace2D_Degenerate, polygonalFace2D_2);
            Assert.NotNull(differenceResult2D);
            Assert.True(differenceResult2D.Any());
            Assert.False(differenceResult2D.Contains<PolygonalFace2D>());
            Assert.True(differenceResult2D.Contains<Segment2D>());

            List<Segment2D>? segment2Ds = differenceResult2D.GetGeometry2Ds<Segment2D>();
            Assert.NotNull(segment2Ds);
            Assert.Equal(8.0, segment2Ds.Sum(x => x.Length), 3);
        }

        /// <summary>
        /// Tests that a self-intersecting (invalid) first face is repaired before the difference is computed.
        /// <para>The bow-tie (0,0)-(2,0)-(0,2)-(2,2) repairs to two unit triangles; subtracting the right half-plane square x in [1,2] removes the right triangle, leaving an area of 1.</para>
        /// </summary>
        [Fact]
        public void PolygonalFace2D_DifferenceResult2D_SelfIntersecting_Repaired()
        {
            Polygon2D polygon2D_Bowtie = new([new Point2D(0, 0), new Point2D(2, 0), new Point2D(0, 2), new Point2D(2, 2)]);
            PolygonalFace2D? polygonalFace2D_Bowtie = Create.PolygonalFace2D(polygon2D_Bowtie);
            Assert.NotNull(polygonalFace2D_Bowtie);

            PolygonalFace2D? polygonalFace2D_Right = Create.PolygonalFace2D(new Point2D(1, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(1, 2));
            Assert.NotNull(polygonalFace2D_Right);

            DifferenceResult2D? differenceResult2D = Create.DifferenceResult2D(polygonalFace2D_Bowtie, polygonalFace2D_Right);
            Assert.NotNull(differenceResult2D);

            List<PolygonalFace2D>? polygonalFace2Ds = differenceResult2D.GetGeometry2Ds<PolygonalFace2D>();
            Assert.NotNull(polygonalFace2Ds);
            Assert.Equal(1.0, polygonalFace2Ds.Sum(x => x.GetArea()), 3);
        }

        /// <summary>
        /// Tests null handling of the 2D difference: a null first face yields null; a null second face returns the first face unchanged.
        /// </summary>
        [Fact]
        public void PolygonalFace2D_DifferenceResult2D_Null()
        {
            PolygonalFace2D? polygonalFace2D = Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2));
            Assert.NotNull(polygonalFace2D);

            DifferenceResult2D? differenceResult2D_NullFirst = Create.DifferenceResult2D(null, polygonalFace2D);
            Assert.Null(differenceResult2D_NullFirst);

            DifferenceResult2D? differenceResult2D_NullSecond = Create.DifferenceResult2D(polygonalFace2D, null);
            Assert.NotNull(differenceResult2D_NullSecond);

            List<PolygonalFace2D>? polygonalFace2Ds = differenceResult2D_NullSecond.GetGeometry2Ds<PolygonalFace2D>();
            Assert.NotNull(polygonalFace2Ds);
            Assert.Single(polygonalFace2Ds);
            Assert.Equal(4.0, polygonalFace2Ds[0].GetArea(), 3);
        }

        /// <summary>
        /// Tests construction and serialization round-trip of a <see cref="DifferenceResult2D"/> holding mixed-dimensional geometries (face, segment and point), covering the JSON and <c>Clone()</c> copy-constructor paths.
        /// </summary>
        [Fact]
        public void DifferenceResult2D_SerializationCheck()
        {
            PolygonalFace2D? polygonalFace2D = Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2));
            Assert.NotNull(polygonalFace2D);

            Segment2D segment2D = new(new Point2D(5, 0), new Point2D(9, 0));
            Point2D point2D = new(12, 3);

            DifferenceResult2D differenceResult2D = new([polygonalFace2D, segment2D, point2D]);
            Assert.Equal(3, differenceResult2D.Count);
            Assert.True(differenceResult2D.Contains<PolygonalFace2D>());
            Assert.True(differenceResult2D.Contains<Segment2D>());
            Assert.True(differenceResult2D.Contains<Point2D>());
            Assert.Equal(BooleanOpertaionType.Difference, differenceResult2D.BooleanOpertaionType);

            DiGi.Core.xUnit.Query.SerializationCheck(differenceResult2D);
        }
    }
}