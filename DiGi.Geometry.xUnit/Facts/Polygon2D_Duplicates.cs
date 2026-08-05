using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using System.Collections.Generic;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a polygon created from an explicitly closed ring drops the repeated closing vertex.
        /// <para>A polygon holds its ring open - <see cref="Polygon2D.GetSegments"/> adds the closing segment itself - so a ring whose last point repeats its first would otherwise carry a segment of no length. That segment has no direction, and it makes the ring report as self intersecting.</para>
        /// <para>A ring which is already open comes back with the same points it went in with, which is what keeps the factory safe to use everywhere.</para>
        /// </summary>
        [Fact]
        public void Polygon2D_Create_ClosingVertexRemoved()
        {
            Polygon2D? polygon2D_Closed = Planar.Create.Polygon2D([new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 10), new Point2D(0, 10), new Point2D(0, 0)]);

            Assert.NotNull(polygon2D_Closed);

            List<Point2D>? point2Ds_Closed = polygon2D_Closed.GetPoints();

            Assert.NotNull(point2Ds_Closed);
            Assert.Equal(4, point2Ds_Closed.Count);
            Assert.False(Planar.Query.SelfIntersect(polygon2D_Closed));
            Assert.Equal(100.0, polygon2D_Closed.GetArea(), 6);

            Polygon2D? polygon2D_Open = Planar.Create.Polygon2D([new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 10), new Point2D(0, 10)]);

            Assert.NotNull(polygon2D_Open);

            List<Point2D>? point2Ds_Open = polygon2D_Open.GetPoints();

            Assert.NotNull(point2Ds_Open);
            Assert.Equal(4, point2Ds_Open.Count);
            Assert.Equal(polygon2D_Open.GetArea(), polygon2D_Closed.GetArea(), 6);
        }

        /// <summary>
        /// Tests that the constructor stores the points exactly as given, leaving the repeated closing vertex in place.
        /// <para>Constructors carry no calculation, so removing repeated points is the job of <see cref="Planar.Create.Polygon2D(IEnumerable{Point2D?}?, double)"/> alone. This pins that split: a caller who already holds clean points pays nothing for the constructor, and a caller who does not knows to reach for the factory.</para>
        /// </summary>
        [Fact]
        public void Polygon2D_Constructor_KeepsPointsAsGiven()
        {
            Polygon2D polygon2D = new([new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 10), new Point2D(0, 10), new Point2D(0, 0)]);

            List<Point2D>? point2Ds = polygon2D.GetPoints();

            Assert.NotNull(point2Ds);
            Assert.Equal(5, point2Ds.Count);
        }

        /// <summary>
        /// Tests that a triangle given as a closed four point ring triangulates into one triangle rather than two.
        /// <para><see cref="Polygon2D.Triangulate(double)"/> takes a fast path for a four point polygon and splits it across the diagonal. A triangle written as three corners plus a repeat of the first hit that path and produced a second triangle spanning the repeated corner, which has no area. Dropping the repeat puts the ring back on the three point path.</para>
        /// </summary>
        [Fact]
        public void Polygon2D_Create_ClosedTriangleTriangulates()
        {
            Polygon2D? polygon2D = Planar.Create.Polygon2D([new Point2D(0, 0), new Point2D(10, 0), new Point2D(0, 10), new Point2D(0, 0)]);

            Assert.NotNull(polygon2D);

            List<Point2D>? point2Ds = polygon2D.GetPoints();

            Assert.NotNull(point2Ds);
            Assert.Equal(3, point2Ds.Count);

            List<Triangle2D>? triangle2Ds = polygon2D.Triangulate();

            Assert.NotNull(triangle2Ds);
            Assert.Single(triangle2Ds);
            Assert.Equal(50.0, triangle2Ds[0].GetArea(), 6);
        }

        /// <summary>
        /// Tests that a point repeated in the middle of a ring is dropped as well as one repeating the first point.
        /// <para>A segment of no length is the same defect wherever it sits, so the factory removes every point coinciding with the one before it, not only the closing one.</para>
        /// </summary>
        [Fact]
        public void Polygon2D_Create_InteriorDuplicateRemoved()
        {
            Polygon2D? polygon2D = Planar.Create.Polygon2D([new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 0), new Point2D(10, 10), new Point2D(0, 10)]);

            Assert.NotNull(polygon2D);

            List<Point2D>? point2Ds = polygon2D.GetPoints();

            Assert.NotNull(point2Ds);
            Assert.Equal(4, point2Ds.Count);
            Assert.Equal(100.0, polygon2D.GetArea(), 6);
        }

        /// <summary>
        /// Tests that a ring which only reaches three positions by repeating a corner is rejected rather than turned into a two corner polygon.
        /// <para>The count is checked after the repeats are removed, so the guard measures distinct corners instead of positions. Without that ordering a ring of two corners written as three positions would pass a check for three corners and become a polygon holding fewer corners than a polygon can have.</para>
        /// </summary>
        [Fact]
        public void Polygon2D_Create_TooShort()
        {
            // Two distinct corners written as a closed ring of three positions.
            Assert.Null(Planar.Create.Polygon2D([new Point2D(0, 0), new Point2D(10, 0), new Point2D(0, 0)]));

            // One corner repeated three times collapses to a single point.
            Assert.Null(Planar.Create.Polygon2D([new Point2D(5, 5), new Point2D(5, 5), new Point2D(5, 5)]));

            Assert.Null(Planar.Create.Polygon2D([new Point2D(0, 0), new Point2D(10, 0)]));
            Assert.Null(Planar.Create.Polygon2D((IEnumerable<Point2D?>?)null));
        }

        /// <summary>
        /// Tests that a closed ring in space is normalised before a plane is fitted to it.
        /// <para>The plane takes the average of the points as its origin, so a repeated corner pulls that origin towards itself and every point then projects onto a plane sitting slightly off the ring. Trimming first puts the origin on the centre of the distinct corners.</para>
        /// </summary>
        [Fact]
        public void Polygon3D_Create_ClosingVertexRemoved()
        {
            Polygon3D? polygon3D = Spatial.Create.Polygon3D([new Point3D(0, 0, 5), new Point3D(10, 0, 5), new Point3D(10, 10, 5), new Point3D(0, 10, 5), new Point3D(0, 0, 5)]);

            Assert.NotNull(polygon3D);

            List<Point3D>? point3Ds = polygon3D.GetPoints();

            Assert.NotNull(point3Ds);
            Assert.Equal(4, point3Ds.Count);
            Assert.Equal(100.0, polygon3D.GetArea(), 6);

            Plane? plane = polygon3D.Plane;

            Assert.NotNull(plane);

            Point3D? point3D_Origin = plane.Origin;

            Assert.NotNull(point3D_Origin);
            Assert.Equal(5.0, point3D_Origin.X, 6);
            Assert.Equal(5.0, point3D_Origin.Y, 6);
            Assert.Equal(5.0, point3D_Origin.Z, 6);
        }

        /// <summary>
        /// Tests that a ring in space which only reaches three positions by repeating a corner is rejected.
        /// </summary>
        [Fact]
        public void Polygon3D_Create_TooShort()
        {
            Assert.Null(Spatial.Create.Polygon3D([new Point3D(0, 0, 0), new Point3D(10, 0, 0), new Point3D(0, 0, 0)]));
        }
    }
}
