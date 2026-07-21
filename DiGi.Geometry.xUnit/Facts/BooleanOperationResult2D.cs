using DiGi.Geometry.Core.Enums;
using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the 2D boolean operation dispatcher: each <see cref="BooleanOpertaionType"/> value returns the matching concrete result type reporting the same operation type.
        /// </summary>
        [Fact]
        public void BooleanOpertaionType_BooleanOperationResult2D_Dispatch()
        {
            PolygonalFace2D? polygonalFace2D_1 = Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(4, 0), new Point2D(4, 4), new Point2D(0, 4));
            PolygonalFace2D? polygonalFace2D_2 = Create.PolygonalFace2D(new Point2D(2, 2), new Point2D(6, 2), new Point2D(6, 6), new Point2D(2, 6));
            Assert.NotNull(polygonalFace2D_1);
            Assert.NotNull(polygonalFace2D_2);

            BooleanOperationResult2D? booleanOperationResult2D_Intersection = Create.BooleanOperationResult2D(BooleanOpertaionType.Intersection, polygonalFace2D_1, polygonalFace2D_2);
            Assert.NotNull(booleanOperationResult2D_Intersection);
            Assert.IsType<IntersectionResult2D>(booleanOperationResult2D_Intersection);
            Assert.Equal(BooleanOpertaionType.Intersection, booleanOperationResult2D_Intersection.BooleanOpertaionType);
            Assert.Equal(4.0, booleanOperationResult2D_Intersection.GetGeometry2Ds<PolygonalFace2D>()!.Sum(x => x.GetArea()), 3);

            BooleanOperationResult2D? booleanOperationResult2D_Difference = Create.BooleanOperationResult2D(BooleanOpertaionType.Difference, polygonalFace2D_1, polygonalFace2D_2);
            Assert.NotNull(booleanOperationResult2D_Difference);
            Assert.IsType<DifferenceResult2D>(booleanOperationResult2D_Difference);
            Assert.Equal(BooleanOpertaionType.Difference, booleanOperationResult2D_Difference.BooleanOpertaionType);
            Assert.Equal(12.0, booleanOperationResult2D_Difference.GetGeometry2Ds<PolygonalFace2D>()!.Sum(x => x.GetArea()), 3);

            BooleanOperationResult2D? booleanOperationResult2D_Union = Create.BooleanOperationResult2D(BooleanOpertaionType.Union, polygonalFace2D_1, polygonalFace2D_2);
            Assert.NotNull(booleanOperationResult2D_Union);
            Assert.IsType<UnionResult2D>(booleanOperationResult2D_Union);
            Assert.Equal(BooleanOpertaionType.Union, booleanOperationResult2D_Union.BooleanOpertaionType);
            Assert.Equal(28.0, booleanOperationResult2D_Union.GetGeometry2Ds<PolygonalFace2D>()!.Sum(x => x.GetArea()), 3);
        }

        /// <summary>
        /// Tests the lower-dimensional intersection path: two faces sharing only a full edge intersect in a <see cref="Segment2D"/> of the shared-edge length, with no area result.
        /// </summary>
        [Fact]
        public void PolygonalFace2D_IntersectionResult2D_EdgeTouch_Segment()
        {
            PolygonalFace2D? polygonalFace2D_1 = Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2));
            PolygonalFace2D? polygonalFace2D_2 = Create.PolygonalFace2D(new Point2D(2, 0), new Point2D(4, 0), new Point2D(4, 2), new Point2D(2, 2));
            Assert.NotNull(polygonalFace2D_1);
            Assert.NotNull(polygonalFace2D_2);

            IntersectionResult2D? intersectionResult2D = Create.IntersectionResult2D(polygonalFace2D_1, polygonalFace2D_2);
            Assert.NotNull(intersectionResult2D);
            Assert.True(intersectionResult2D.Any());
            Assert.False(intersectionResult2D.Contains<PolygonalFace2D>());
            Assert.True(intersectionResult2D.Contains<Segment2D>());

            List<Segment2D>? segment2Ds = intersectionResult2D.GetGeometry2Ds<Segment2D>();
            Assert.NotNull(segment2Ds);
            Assert.Single(segment2Ds);
            Assert.Equal(2.0, segment2Ds[0].Length, 3);
        }

        /// <summary>
        /// Tests the lower-dimensional intersection path: two faces touching at a single corner intersect in a <see cref="Point2D"/> at the shared vertex, with no area result.
        /// </summary>
        [Fact]
        public void PolygonalFace2D_IntersectionResult2D_CornerTouch_Point()
        {
            PolygonalFace2D? polygonalFace2D_1 = Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2));
            PolygonalFace2D? polygonalFace2D_2 = Create.PolygonalFace2D(new Point2D(2, 2), new Point2D(4, 2), new Point2D(4, 4), new Point2D(2, 4));
            Assert.NotNull(polygonalFace2D_1);
            Assert.NotNull(polygonalFace2D_2);

            IntersectionResult2D? intersectionResult2D = Create.IntersectionResult2D(polygonalFace2D_1, polygonalFace2D_2);
            Assert.NotNull(intersectionResult2D);
            Assert.True(intersectionResult2D.Any());
            Assert.False(intersectionResult2D.Contains<PolygonalFace2D>());
            Assert.True(intersectionResult2D.Contains<Point2D>());

            List<Point2D>? point2Ds = intersectionResult2D.GetGeometry2Ds<Point2D>();
            Assert.NotNull(point2Ds);
            Assert.Single(point2Ds);
            Assert.Equal(2.0, point2Ds[0].X, 3);
            Assert.Equal(2.0, point2Ds[0].Y, 3);
        }

        /// <summary>
        /// Tests the polymorphic access of the base class: the indexer and <c>GetGeometry2Ds</c> return defensive clones, so mutations of retrieved geometries do not affect the stored result.
        /// </summary>
        [Fact]
        public void BooleanOperationResult2D_DefensiveCloning()
        {
            PolygonalFace2D? polygonalFace2D = Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2));
            Assert.NotNull(polygonalFace2D);

            UnionResult2D unionResult2D = new(polygonalFace2D);
            Assert.Equal(1, unionResult2D.Count);

            DiGi.Geometry.Planar.Interfaces.IGeometry2D? geometry2D_1 = unionResult2D[0];
            DiGi.Geometry.Planar.Interfaces.IGeometry2D? geometry2D_2 = unionResult2D[0];
            Assert.NotNull(geometry2D_1);
            Assert.NotNull(geometry2D_2);
            Assert.NotSame(geometry2D_1, geometry2D_2);

            DiGi.Geometry.Planar.Interfaces.IGeometry2D? geometry2D_OutOfRange = unionResult2D[5];
            Assert.Null(geometry2D_OutOfRange);
        }

        /// <summary>
        /// Tests that the refactored <c>Query.Union</c> and <c>Query.Difference</c> face prototypes built on the 2D result objects preserve their established behavior for overlapping, disjoint and equal inputs.
        /// </summary>
        [Fact]
        public void PolygonalFace2D_Query_BooleanOperations_Consistency()
        {
            PolygonalFace2D? polygonalFace2D_1 = Create.PolygonalFace2D(new Point2D(0, 0), new Point2D(4, 0), new Point2D(4, 4), new Point2D(0, 4));
            PolygonalFace2D? polygonalFace2D_2 = Create.PolygonalFace2D(new Point2D(2, 2), new Point2D(6, 2), new Point2D(6, 6), new Point2D(2, 6));
            PolygonalFace2D? polygonalFace2D_Far = Create.PolygonalFace2D(new Point2D(100, 100), new Point2D(102, 100), new Point2D(102, 102), new Point2D(100, 102));
            Assert.NotNull(polygonalFace2D_1);
            Assert.NotNull(polygonalFace2D_2);
            Assert.NotNull(polygonalFace2D_Far);

            List<PolygonalFace2D>? polygonalFace2Ds_Union = Query.Union(polygonalFace2D_1, polygonalFace2D_2);
            Assert.NotNull(polygonalFace2Ds_Union);
            Assert.Single(polygonalFace2Ds_Union);
            Assert.Equal(28.0, polygonalFace2Ds_Union[0].GetArea(), 3);

            List<PolygonalFace2D>? polygonalFace2Ds_UnionDisjoint = Query.Union(polygonalFace2D_1, polygonalFace2D_Far);
            Assert.NotNull(polygonalFace2Ds_UnionDisjoint);
            Assert.Equal(2, polygonalFace2Ds_UnionDisjoint.Count);

            List<PolygonalFace2D>? polygonalFace2Ds_Difference = Query.Difference(polygonalFace2D_1, polygonalFace2D_2);
            Assert.NotNull(polygonalFace2Ds_Difference);
            Assert.Equal(12.0, polygonalFace2Ds_Difference.Sum(x => x.GetArea()), 3);

            List<PolygonalFace2D>? polygonalFace2Ds_DifferenceEqual = Query.Difference(polygonalFace2D_1, polygonalFace2D_1);
            Assert.NotNull(polygonalFace2Ds_DifferenceEqual);
            Assert.Empty(polygonalFace2Ds_DifferenceEqual);

            List<PolygonalFace2D>? polygonalFace2Ds_DifferenceDisjoint = Query.Difference(polygonalFace2D_1, polygonalFace2D_Far);
            Assert.NotNull(polygonalFace2Ds_DifferenceDisjoint);
            Assert.Single(polygonalFace2Ds_DifferenceDisjoint);
            Assert.Equal(16.0, polygonalFace2Ds_DifferenceDisjoint[0].GetArea(), 3);
        }
    }
}