using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that intersecting a self-intersecting (invalid) polygon with a valid one repairs the input instead of throwing.
        /// <para>A "bow-tie" quadrilateral (0,0)-(2,0)-(0,2)-(2,2) is invalid: NetTopologySuite would raise a TopologyException ("non-noded intersection") during the overlay. The query must repair the geometry first (as the sibling Union/Difference operations do) and return the intersection of the repaired region.</para>
        /// <para>The repaired bow-tie is two triangles of total area 2, both contained in the 2x2 square, so the intersection area is 2.</para>
        /// </summary>
        [Fact]
        public void Intersection_SelfIntersectingInput_Repaired()
        {
            Polygon2D polygon2D_Bowtie = new([new Point2D(0, 0), new Point2D(2, 0), new Point2D(0, 2), new Point2D(2, 2)]);
            Polygon2D polygon2D_Square = new([new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2)]);

            List<Polygon2D>? polygon2Ds_Pairwise = Planar.Query.Intersection(polygon2D_Bowtie, polygon2D_Square);
            Assert.NotNull(polygon2Ds_Pairwise);
            Assert.NotEmpty(polygon2Ds_Pairwise);
            Assert.Equal(2.0, polygon2Ds_Pairwise.Sum(x => x.GetArea()), 3);

            List<Polygon2D>? polygon2Ds_Collection = Planar.Query.Intersection<Polygon2D, Polygon2D>([polygon2D_Bowtie, polygon2D_Square]);
            Assert.NotNull(polygon2Ds_Collection);
            Assert.Equal(2.0, polygon2Ds_Collection.Sum(x => x.GetArea()), 3);
        }

        /// <summary>
        /// Verifies that a single-element intersection of a self-intersecting polygon repairs it rather than returning the invalid polygon untouched.
        /// <para>Before repair the bow-tie reports a shoelace area of 0 due to sign cancellation; the repaired region has area 2.</para>
        /// </summary>
        [Fact]
        public void Intersection_SingleSelfIntersectingElement_Repaired()
        {
            Polygon2D polygon2D_Bowtie = new([new Point2D(0, 0), new Point2D(2, 0), new Point2D(0, 2), new Point2D(2, 2)]);

            List<Polygon2D>? polygon2Ds = Planar.Query.Intersection<Polygon2D, Polygon2D>([polygon2D_Bowtie]);
            Assert.NotNull(polygon2Ds);
            Assert.NotEmpty(polygon2Ds);
            Assert.Equal(2.0, polygon2Ds.Sum(x => x.GetArea()), 3);
        }

        /// <summary>
        /// Verifies that intersecting a polygon fully contained inside another returns the inner polygon.
        /// </summary>
        [Fact]
        public void Intersection_NestedContainment()
        {
            Polygon2D polygon2D_Outer = new([new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 10), new Point2D(0, 10)]);
            Polygon2D polygon2D_Inner = new([new Point2D(4, 4), new Point2D(6, 4), new Point2D(6, 6), new Point2D(4, 6)]);

            List<Polygon2D>? polygon2Ds = Planar.Query.Intersection(polygon2D_Outer, polygon2D_Inner);
            Assert.NotNull(polygon2Ds);
            Assert.Single(polygon2Ds);
            Assert.Equal(4.0, polygon2Ds[0].GetArea(), 3);
        }

        /// <summary>
        /// Verifies that two squares meeting only at a single corner produce an empty intersection (a point carries no area).
        /// </summary>
        [Fact]
        public void Intersection_CornerTouch_Empty()
        {
            Polygon2D polygon2D_1 = new([new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2)]);
            Polygon2D polygon2D_2 = new([new Point2D(2, 2), new Point2D(4, 2), new Point2D(4, 4), new Point2D(2, 4)]);

            List<Polygon2D>? polygon2Ds = Planar.Query.Intersection(polygon2D_1, polygon2D_2);
            Assert.NotNull(polygon2Ds);
            Assert.Empty(polygon2Ds);
        }

        /// <summary>
        /// Verifies that the intersection of a face with a hole against a solid excludes the overlap with the hole.
        /// <para>The holed face is a 10x10 square with a central 4x4 void at (3,3)-(7,7). Intersecting it with the right half-plane solid (5,0)-(10,10), area 50, removes the part of the void inside that half (a 2x4 region, area 8), giving a net area of 42.</para>
        /// </summary>
        [Fact]
        public void Intersection_FaceWithHole_ExcludesHole()
        {
            Polygon2D polygon2D_Outer = new([new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 10), new Point2D(0, 10)]);
            Polygon2D polygon2D_Hole = new([new Point2D(3, 3), new Point2D(7, 3), new Point2D(7, 7), new Point2D(3, 7)]);
            PolygonalFace2D? polygonalFace2D_Holed = Planar.Create.PolygonalFace2D(polygon2D_Outer, [polygon2D_Hole]);
            Assert.NotNull(polygonalFace2D_Holed);
            Assert.NotNull(polygonalFace2D_Holed.InternalEdges);
            Assert.Single(polygonalFace2D_Holed.InternalEdges);

            PolygonalFace2D? polygonalFace2D_RightHalf = Planar.Create.PolygonalFace2D(new Polygon2D([new Point2D(5, 0), new Point2D(10, 0), new Point2D(10, 10), new Point2D(5, 10)]));
            Assert.NotNull(polygonalFace2D_RightHalf);

            List<PolygonalFace2D>? polygonalFace2Ds = Planar.Query.Intersection(polygonalFace2D_Holed, polygonalFace2D_RightHalf);
            Assert.NotNull(polygonalFace2Ds);
            Assert.Equal(42.0, polygonalFace2Ds.Sum(x => x.GetArea()), 3);
        }

        /// <summary>
        /// Verifies that unioning two squares sharing a full common edge merges them into a single rectangle.
        /// </summary>
        [Fact]
        public void Union_EdgeTouchingSquares_Merge()
        {
            Polygon2D polygon2D_Left = new([new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2)]);
            Polygon2D polygon2D_Right = new([new Point2D(2, 0), new Point2D(4, 0), new Point2D(4, 2), new Point2D(2, 2)]);

            List<Polygon2D>? polygon2Ds = Planar.Query.Union(polygon2D_Left, polygon2D_Right);
            Assert.NotNull(polygon2Ds);
            Assert.Single(polygon2Ds);
            Assert.Equal(8.0, polygon2Ds[0].GetArea(), 3);
        }

        /// <summary>
        /// Verifies that unioning a single self-intersecting face repairs it rather than returning the invalid polygon untouched.
        /// <para>The single-element fast path used to return the input verbatim, so a "bow-tie" face came back with a shoelace area of 0. The union must fall through to the repair path, yielding the repaired region of area 2.</para>
        /// </summary>
        [Fact]
        public void Union_SingleSelfIntersectingFace_Repaired()
        {
            PolygonalFace2D? polygonalFace2D_Bowtie = Planar.Create.PolygonalFace2D(new Polygon2D([new Point2D(0, 0), new Point2D(2, 0), new Point2D(0, 2), new Point2D(2, 2)]));
            Assert.NotNull(polygonalFace2D_Bowtie);

            List<IPolygonalFace2D> polygonalFace2Ds_Input = [polygonalFace2D_Bowtie];

            List<PolygonalFace2D>? polygonalFace2Ds = Planar.Query.Union(polygonalFace2Ds_Input);
            Assert.NotNull(polygonalFace2Ds);
            Assert.NotEmpty(polygonalFace2Ds);
            Assert.Equal(2.0, polygonalFace2Ds.Sum(x => x.GetArea()), 3);
        }

        /// <summary>
        /// Verifies that a difference with a self-intersecting subtrahend repairs it instead of throwing.
        /// <para>Subtracting the repaired bow-tie (two triangles, total area 2) from the 2x2 square leaves the two opposite side triangles, total area 2.</para>
        /// </summary>
        [Fact]
        public void Difference_SelfIntersectingSubtrahend_DoesNotThrow()
        {
            PolygonalFace2D? polygonalFace2D_Square = Planar.Create.PolygonalFace2D(new Polygon2D([new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2)]));
            PolygonalFace2D? polygonalFace2D_Bowtie = Planar.Create.PolygonalFace2D(new Polygon2D([new Point2D(0, 0), new Point2D(2, 0), new Point2D(0, 2), new Point2D(2, 2)]));
            Assert.NotNull(polygonalFace2D_Square);
            Assert.NotNull(polygonalFace2D_Bowtie);

            List<PolygonalFace2D>? polygonalFace2Ds = Planar.Query.Difference(polygonalFace2D_Square, polygonalFace2D_Bowtie);
            Assert.NotNull(polygonalFace2Ds);
            Assert.Equal(2.0, polygonalFace2Ds.Sum(x => x.GetArea()), 3);
        }

        /// <summary>
        /// Verifies that subtracting a fully enclosed polygon produces a single face that preserves the resulting hole.
        /// <para>A 10x10 square minus a central 4x4 square yields one face of net area 84 carrying exactly one internal edge (the hole).</para>
        /// </summary>
        [Fact]
        public void Difference_CreatesHole_PreservesHole()
        {
            PolygonalFace2D? polygonalFace2D_Outer = Planar.Create.PolygonalFace2D(new Polygon2D([new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 10), new Point2D(0, 10)]));
            PolygonalFace2D? polygonalFace2D_Inner = Planar.Create.PolygonalFace2D(new Polygon2D([new Point2D(3, 3), new Point2D(7, 3), new Point2D(7, 7), new Point2D(3, 7)]));
            Assert.NotNull(polygonalFace2D_Outer);
            Assert.NotNull(polygonalFace2D_Inner);

            List<PolygonalFace2D>? polygonalFace2Ds = Planar.Query.Difference(polygonalFace2D_Outer, polygonalFace2D_Inner);
            Assert.NotNull(polygonalFace2Ds);
            Assert.Single(polygonalFace2Ds);
            Assert.Equal(84.0, polygonalFace2Ds[0].GetArea(), 3);

            List<IPolygonal2D>? internalEdges = polygonalFace2Ds[0].InternalEdges;
            Assert.NotNull(internalEdges);
            Assert.Single(internalEdges);
        }

        /// <summary>
        /// Documents a lossy edge case: the flat <see cref="IPolygonal2D"/> difference overload returns a hole result as separate boundary rings.
        /// <para>Subtracting a fully enclosed 4x4 square from a 10x10 square is a face with a hole (net area 84). The <see cref="IPolygonal2D"/> overload cannot express a hole, so it returns two positive rings — the outer boundary (area 100) and the hole boundary (area 16). Summing the returned areas therefore overstates the true net area (100 + 16 = 116, not 84). Callers must not treat these rings as independent solids; the face-based overload is the correct choice when holes are possible.</para>
        /// <para>This test pins the current behaviour so any change to the overload's contract is deliberate and visible.</para>
        /// </summary>
        [Fact]
        public void Difference_FlatOverload_FlattensHoleRings()
        {
            Polygon2D polygon2D_Outer = new([new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 10), new Point2D(0, 10)]);
            Polygon2D polygon2D_Inner = new([new Point2D(3, 3), new Point2D(7, 3), new Point2D(7, 7), new Point2D(3, 7)]);

            List<IPolygonal2D>? polygonal2Ds = Planar.Query.Difference((IPolygonal2D)polygon2D_Outer, polygon2D_Inner);
            Assert.NotNull(polygonal2Ds);
            Assert.Equal(2, polygonal2Ds.Count);

            // The naive sum of ring areas is 100 + 16 = 116, which is NOT the true net area of 84.
            Assert.Equal(116.0, polygonal2Ds.Sum(x => x.GetArea()), 3);
        }

        /// <summary>
        /// Verifies that a subtraction which splits the minuend into two disconnected regions returns two faces.
        /// <para>A full-width bar (0,4)-(10,6) removed from a 10x10 square leaves a bottom band and a top band, each 10x4, total area 80.</para>
        /// </summary>
        [Fact]
        public void Difference_SplitsIntoTwoFaces()
        {
            PolygonalFace2D? polygonalFace2D_Outer = Planar.Create.PolygonalFace2D(new Polygon2D([new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 10), new Point2D(0, 10)]));
            PolygonalFace2D? polygonalFace2D_Bar = Planar.Create.PolygonalFace2D(new Polygon2D([new Point2D(0, 4), new Point2D(10, 4), new Point2D(10, 6), new Point2D(0, 6)]));
            Assert.NotNull(polygonalFace2D_Outer);
            Assert.NotNull(polygonalFace2D_Bar);

            List<PolygonalFace2D>? polygonalFace2Ds = Planar.Query.Difference(polygonalFace2D_Outer, polygonalFace2D_Bar);
            Assert.NotNull(polygonalFace2Ds);
            Assert.Equal(2, polygonalFace2Ds.Count);
            Assert.Equal(80.0, polygonalFace2Ds.Sum(x => x.GetArea()), 3);
        }

        /// <summary>
        /// Verifies that subtracting a polygon that completely encloses the minuend yields an empty result.
        /// </summary>
        [Fact]
        public void Difference_SubtrahendSuperset_Empty()
        {
            PolygonalFace2D? polygonalFace2D_Small = Planar.Create.PolygonalFace2D(new Polygon2D([new Point2D(1, 1), new Point2D(2, 1), new Point2D(2, 2), new Point2D(1, 2)]));
            PolygonalFace2D? polygonalFace2D_Big = Planar.Create.PolygonalFace2D(new Polygon2D([new Point2D(0, 0), new Point2D(5, 0), new Point2D(5, 5), new Point2D(0, 5)]));
            Assert.NotNull(polygonalFace2D_Small);
            Assert.NotNull(polygonalFace2D_Big);

            List<PolygonalFace2D>? polygonalFace2Ds = Planar.Query.Difference(polygonalFace2D_Small, polygonalFace2D_Big);
            Assert.NotNull(polygonalFace2Ds);
            Assert.Empty(polygonalFace2Ds);
        }

        /// <summary>
        /// Verifies that subtracting a topologically identical polygon wound in the opposite orientation still yields an empty result.
        /// <para>Winding order must not affect topological equality, so a clockwise and a counter-clockwise unit square cancel to nothing.</para>
        /// </summary>
        [Fact]
        public void Difference_ReversedOrientationEqual_Empty()
        {
            PolygonalFace2D? polygonalFace2D_Ccw = Planar.Create.PolygonalFace2D(new Polygon2D([new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2)]));
            PolygonalFace2D? polygonalFace2D_Cw = Planar.Create.PolygonalFace2D(new Polygon2D([new Point2D(0, 0), new Point2D(0, 2), new Point2D(2, 2), new Point2D(2, 0)]));
            Assert.NotNull(polygonalFace2D_Ccw);
            Assert.NotNull(polygonalFace2D_Cw);

            List<PolygonalFace2D>? polygonalFace2Ds = Planar.Query.Difference(polygonalFace2D_Ccw, polygonalFace2D_Cw);
            Assert.NotNull(polygonalFace2Ds);
            Assert.Empty(polygonalFace2Ds);
        }
    }
}
