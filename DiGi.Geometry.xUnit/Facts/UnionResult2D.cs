using DiGi.Geometry.Core.Enums;
using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests 2D boolean union between two disjoint polygonal faces: both faces are returned.
        /// </summary>
        [Fact]
        public void PolygonalFace2D_UnionResult2D_Disjoint()
        {
            PolygonalFace2D? polygonalFace2D_1 = Planar.Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2));
            PolygonalFace2D? polygonalFace2D_2 = Planar.Create.PolygonalFace2D(new Point2D(10, 10), new Point2D(12, 10), new Point2D(12, 12), new Point2D(10, 12));
            Assert.NotNull(polygonalFace2D_1);
            Assert.NotNull(polygonalFace2D_2);

            UnionResult2D? unionResult2D = Planar.Create.UnionResult2D(polygonalFace2D_1, polygonalFace2D_2);
            Assert.NotNull(unionResult2D);
            Assert.Equal(BooleanOpertaionType.Union, unionResult2D.BooleanOpertaionType);
            Assert.Equal(2, unionResult2D.Count);

            List<PolygonalFace2D>? polygonalFace2Ds = unionResult2D.GetGeometry2Ds<PolygonalFace2D>();
            Assert.NotNull(polygonalFace2Ds);
            Assert.Equal(2, polygonalFace2Ds.Count);
            Assert.Equal(8.0, polygonalFace2Ds.Sum(x => x.GetArea()), 3);
        }

        /// <summary>
        /// Tests 2D boolean union between two overlapping polygonal faces: a single merged face whose area excludes the double-counted overlap.
        /// </summary>
        [Fact]
        public void PolygonalFace2D_UnionResult2D_Overlapping()
        {
            PolygonalFace2D? polygonalFace2D_1 = Planar.Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(4, 0), new Point2D(4, 4), new Point2D(0, 4));
            PolygonalFace2D? polygonalFace2D_2 = Planar.Create.PolygonalFace2D(new Point2D(2, 2), new Point2D(6, 2), new Point2D(6, 6), new Point2D(2, 6));
            Assert.NotNull(polygonalFace2D_1);
            Assert.NotNull(polygonalFace2D_2);

            UnionResult2D? unionResult2D = Planar.Create.UnionResult2D(polygonalFace2D_1, polygonalFace2D_2);
            Assert.NotNull(unionResult2D);
            Assert.True(unionResult2D.Any());

            List<PolygonalFace2D>? polygonalFace2Ds = unionResult2D.GetGeometry2Ds<PolygonalFace2D>();
            Assert.NotNull(polygonalFace2Ds);
            Assert.Single(polygonalFace2Ds);
            Assert.Equal(28.0, polygonalFace2Ds[0].GetArea(), 3);
        }

        /// <summary>
        /// Tests that unioning two squares sharing a full common edge merges them into a single rectangle.
        /// </summary>
        [Fact]
        public void PolygonalFace2D_UnionResult2D_EdgeTouch_Merge()
        {
            PolygonalFace2D? polygonalFace2D_1 = Planar.Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2));
            PolygonalFace2D? polygonalFace2D_2 = Planar.Create.PolygonalFace2D(new Point2D(2, 0), new Point2D(4, 0), new Point2D(4, 2), new Point2D(2, 2));
            Assert.NotNull(polygonalFace2D_1);
            Assert.NotNull(polygonalFace2D_2);

            UnionResult2D? unionResult2D = Planar.Create.UnionResult2D(polygonalFace2D_1, polygonalFace2D_2);
            Assert.NotNull(unionResult2D);

            List<PolygonalFace2D>? polygonalFace2Ds = unionResult2D.GetGeometry2Ds<PolygonalFace2D>();
            Assert.NotNull(polygonalFace2Ds);
            Assert.Single(polygonalFace2Ds);
            Assert.Equal(8.0, polygonalFace2Ds[0].GetArea(), 3);
        }

        /// <summary>
        /// Tests 2D boolean union between two identical polygonal faces: a single face with the original area.
        /// </summary>
        [Fact]
        public void PolygonalFace2D_UnionResult2D_Identical()
        {
            PolygonalFace2D? polygonalFace2D = Planar.Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2));
            Assert.NotNull(polygonalFace2D);

            UnionResult2D? unionResult2D = Planar.Create.UnionResult2D(polygonalFace2D, polygonalFace2D);
            Assert.NotNull(unionResult2D);

            List<PolygonalFace2D>? polygonalFace2Ds = unionResult2D.GetGeometry2Ds<PolygonalFace2D>();
            Assert.NotNull(polygonalFace2Ds);
            Assert.Single(polygonalFace2Ds);
            Assert.Equal(4.0, polygonalFace2Ds[0].GetArea(), 3);
        }

        /// <summary>
        /// Tests 2D boolean union between two squares touching at a single corner: the total area is preserved regardless of whether the result is one self-touching face or two faces.
        /// </summary>
        [Fact]
        public void PolygonalFace2D_UnionResult2D_CornerTouch()
        {
            PolygonalFace2D? polygonalFace2D_1 = Planar.Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2));
            PolygonalFace2D? polygonalFace2D_2 = Planar.Create.PolygonalFace2D(new Point2D(2, 2), new Point2D(4, 2), new Point2D(4, 4), new Point2D(2, 4));
            Assert.NotNull(polygonalFace2D_1);
            Assert.NotNull(polygonalFace2D_2);

            UnionResult2D? unionResult2D = Planar.Create.UnionResult2D(polygonalFace2D_1, polygonalFace2D_2);
            Assert.NotNull(unionResult2D);
            Assert.True(unionResult2D.Any());

            List<PolygonalFace2D>? polygonalFace2Ds = unionResult2D.GetGeometry2Ds<PolygonalFace2D>();
            Assert.NotNull(polygonalFace2Ds);
            Assert.Equal(8.0, polygonalFace2Ds.Sum(x => x.GetArea()), 3);
        }

        /// <summary>
        /// Tests that unioning a U-shaped face with a capping rectangle encloses a hole: a single face with one internal edge.
        /// <para>The U-shape (6x6 outline with a 2x4 notch, area 28) capped by a 6x2 rectangle (area 12, overlap 8) yields a face of area 32 with a 2x2 hole.</para>
        /// </summary>
        [Fact]
        public void PolygonalFace2D_UnionResult2D_HoleCreation()
        {
            PolygonalFace2D? polygonalFace2D_U = Planar.Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(6, 0), new Point2D(6, 6), new Point2D(4, 6), new Point2D(4, 2), new Point2D(2, 2), new Point2D(2, 6), new Point2D(0, 6));
            PolygonalFace2D? polygonalFace2D_Cap = Planar.Create.PolygonalFace2D(new Point2D(0, 4), new Point2D(6, 4), new Point2D(6, 6), new Point2D(0, 6));
            Assert.NotNull(polygonalFace2D_U);
            Assert.NotNull(polygonalFace2D_Cap);

            UnionResult2D? unionResult2D = Planar.Create.UnionResult2D(polygonalFace2D_U, polygonalFace2D_Cap);
            Assert.NotNull(unionResult2D);

            List<PolygonalFace2D>? polygonalFace2Ds = unionResult2D.GetGeometry2Ds<PolygonalFace2D>();
            Assert.NotNull(polygonalFace2Ds);
            Assert.Single(polygonalFace2Ds);
            Assert.Equal(32.0, polygonalFace2Ds[0].GetArea(), 3);
            Assert.NotNull(polygonalFace2Ds[0].InternalEdges);
            Assert.Single(polygonalFace2Ds[0].InternalEdges!);

            DiGi.Core.xUnit.Query.SerializationCheck(unionResult2D);
        }

        /// <summary>
        /// Tests null handling of the 2D union: a single null input returns the other face; two null inputs return null.
        /// </summary>
        [Fact]
        public void PolygonalFace2D_UnionResult2D_Null()
        {
            PolygonalFace2D? polygonalFace2D = Planar.Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2));
            Assert.NotNull(polygonalFace2D);

            UnionResult2D? unionResult2D_NullSecond = Planar.Create.UnionResult2D(polygonalFace2D, null);
            Assert.NotNull(unionResult2D_NullSecond);
            Assert.Equal(1, unionResult2D_NullSecond.Count);

            UnionResult2D? unionResult2D_NullFirst = Planar.Create.UnionResult2D(null, polygonalFace2D);
            Assert.NotNull(unionResult2D_NullFirst);
            Assert.Equal(1, unionResult2D_NullFirst.Count);

            UnionResult2D? unionResult2D_BothNull = Planar.Create.UnionResult2D(null, null);
            Assert.Null(unionResult2D_BothNull);
        }

        /// <summary>
        /// Tests that a self-intersecting (invalid) input face is repaired before the union is computed.
        /// <para>The bow-tie (0,0)-(2,0)-(0,2)-(2,2) repairs to two unit triangles (area 2); unioned with the covering 2x2 square the result is the square itself (area 4).</para>
        /// </summary>
        [Fact]
        public void PolygonalFace2D_UnionResult2D_SelfIntersecting_Repaired()
        {
            Polygon2D polygon2D_Bowtie = new([new Point2D(0, 0), new Point2D(2, 0), new Point2D(0, 2), new Point2D(2, 2)]);
            PolygonalFace2D? polygonalFace2D_Bowtie = Planar.Create.PolygonalFace2D(polygon2D_Bowtie);
            Assert.NotNull(polygonalFace2D_Bowtie);

            PolygonalFace2D? polygonalFace2D_Square = Planar.Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2));
            Assert.NotNull(polygonalFace2D_Square);

            UnionResult2D? unionResult2D = Planar.Create.UnionResult2D(polygonalFace2D_Bowtie, polygonalFace2D_Square);
            Assert.NotNull(unionResult2D);

            List<PolygonalFace2D>? polygonalFace2Ds = unionResult2D.GetGeometry2Ds<PolygonalFace2D>();
            Assert.NotNull(polygonalFace2Ds);
            Assert.Equal(4.0, polygonalFace2Ds.Sum(x => x.GetArea()), 3);
        }

        /// <summary>
        /// Tests construction and serialization round-trip of a <see cref="UnionResult2D"/> built from multiple faces, covering the JSON and <c>Clone()</c> copy-constructor paths.
        /// </summary>
        [Fact]
        public void UnionResult2D_SerializationCheck()
        {
            PolygonalFace2D? polygonalFace2D_1 = Planar.Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2));
            PolygonalFace2D? polygonalFace2D_2 = Planar.Create.PolygonalFace2D(new Point2D(10, 10), new Point2D(12, 10), new Point2D(12, 12), new Point2D(10, 12));
            Assert.NotNull(polygonalFace2D_1);
            Assert.NotNull(polygonalFace2D_2);

            UnionResult2D unionResult2D = new([polygonalFace2D_1, polygonalFace2D_2]);
            Assert.Equal(2, unionResult2D.Count);
            Assert.Equal(BooleanOpertaionType.Union, unionResult2D.BooleanOpertaionType);
            Assert.True(unionResult2D.Contains<PolygonalFace2D>());
            Assert.False(unionResult2D.Contains<Segment2D>());

            DiGi.Core.xUnit.Query.SerializationCheck(unionResult2D);
        }
    }
}
