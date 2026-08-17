using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that each point is answered with the face containing it, that a point in no face is answered with null, and that the answers line up with the points one for one.
        /// </summary>
        [Fact]
        public void IdsByPoint2Ds_InsideAndOutside()
        {
            Dictionary<int, PolygonalFace2D> polygonalFace2Ds_ById = new()
            {
                { 10, PolygonalFace2D_Square(0, 0, 100) },
                { 20, PolygonalFace2D_Square(1000, 0, 100) }
            };

            List<Point2D> point2Ds = [new(50, 50), new(1050, 50), new(500, 5000), new(10, 90)];

            int?[]? ids = polygonalFace2Ds_ById.IdsByPoint2Ds(point2Ds);

            Assert.NotNull(ids);
            Assert.Equal(point2Ds.Count, ids.Length);
            Assert.Equal(10, ids[0]);
            Assert.Equal(20, ids[1]);
            Assert.Null(ids[2]);
            Assert.Equal(10, ids[3]);
        }

        /// <summary>
        /// Verifies that a point lying in a hole of a face is answered with null rather than with the face around it.
        /// <para>This is why the faces are derived whole rather than as outer rings. An administrative area that excludes a town inside it is stored as a face with a hole, and a point in that town belongs to the town - deciding against the outer ring alone would hand it to the surrounding area, silently and with nothing to show for it.</para>
        /// </summary>
        [Fact]
        public void IdsByPoint2Ds_PointInHole()
        {
            IPolygonal2D_Square(0, 0, 100, out Polygon2D polygon2D_External);
            IPolygonal2D_Square(40, 40, 20, out Polygon2D polygon2D_Internal);

            List<IPolygonal2D> polygonal2Ds_Internal = [polygon2D_Internal];

            PolygonalFace2D? polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D(polygon2D_External, polygonal2Ds_Internal);
            Assert.NotNull(polygonalFace2D);

            Dictionary<int, PolygonalFace2D> polygonalFace2Ds_ById = new() { { 10, polygonalFace2D } };

            List<Point2D> point2Ds = [new(10, 10), new(50, 50), new(90, 90)];

            int?[]? ids = polygonalFace2Ds_ById.IdsByPoint2Ds(point2Ds);

            Assert.NotNull(ids);
            Assert.Equal(10, ids[0]);
            Assert.Null(ids[1]);
            Assert.Equal(10, ids[2]);
        }

        /// <summary>
        /// Verifies that where two faces overlap the lowest identifier wins, whichever order the faces were handed over in, so the same point decided twice gives the same answer.
        /// </summary>
        [Fact]
        public void IdsByPoint2Ds_OverlapTakesLowestId()
        {
            List<Point2D> point2Ds = [new(75, 75)];

            Dictionary<int, PolygonalFace2D> polygonalFace2Ds_ById_1 = new()
            {
                { 10, PolygonalFace2D_Square(0, 0, 100) },
                { 20, PolygonalFace2D_Square(50, 50, 100) }
            };

            Dictionary<int, PolygonalFace2D> polygonalFace2Ds_ById_2 = new()
            {
                { 20, PolygonalFace2D_Square(50, 50, 100) },
                { 10, PolygonalFace2D_Square(0, 0, 100) }
            };

            int?[]? ids_1 = polygonalFace2Ds_ById_1.IdsByPoint2Ds(point2Ds);
            int?[]? ids_2 = polygonalFace2Ds_ById_2.IdsByPoint2Ds(point2Ds);

            Assert.NotNull(ids_1);
            Assert.NotNull(ids_2);
            Assert.Equal(10, ids_1[0]);
            Assert.Equal(10, ids_2[0]);
        }

        /// <summary>
        /// Verifies the tolerance boundary on both sides: a point lying just outside a face by less than the tolerance is taken as within it, and one lying further out is not.
        /// </summary>
        [Fact]
        public void IdsByPoint2Ds_ToleranceBoundary()
        {
            Dictionary<int, PolygonalFace2D> polygonalFace2Ds_ById = new() { { 10, PolygonalFace2D_Square(0, 0, 100) } };

            double tolerance = 0.1;

            List<Point2D> point2Ds = [new(100 + (tolerance / 2), 50), new(100 + (tolerance * 2), 50)];

            int?[]? ids = polygonalFace2Ds_ById.IdsByPoint2Ds(point2Ds, tolerance);

            Assert.NotNull(ids);
            Assert.Equal(10, ids[0]);
            Assert.Null(ids[1]);
        }

        /// <summary>
        /// Verifies that null arguments give null and that an empty set of faces still gives one answer per point.
        /// </summary>
        [Fact]
        public void IdsByPoint2Ds_NullAndEmpty()
        {
            Dictionary<int, PolygonalFace2D> polygonalFace2Ds_ById = new() { { 10, PolygonalFace2D_Square(0, 0, 100) } };

            Assert.Null(polygonalFace2Ds_ById.IdsByPoint2Ds(null));
            Assert.Null(((IDictionary<int, PolygonalFace2D>?)null).IdsByPoint2Ds([new Point2D(0, 0)]));

            Dictionary<int, PolygonalFace2D> polygonalFace2Ds_ById_Empty = [];

            int?[]? ids = polygonalFace2Ds_ById_Empty.IdsByPoint2Ds([new Point2D(0, 0), new Point2D(1, 1)]);
            Assert.NotNull(ids);
            Assert.Equal(2, ids.Length);
            Assert.Null(ids[0]);
            Assert.Null(ids[1]);
        }

        /// <summary>
        /// Verifies that deciding many points against many faces stays well inside a stated time, which is what shows the cell grid is being hit rather than every face being walked for every point.
        /// <para>Without the grid this is 2 000 faces times 50 000 points of ring walking, which does not finish in any time worth waiting for. The threshold is deliberately loose - it is there to catch the accelerator falling back to a linear scan, not to measure the machine.</para>
        /// </summary>
        [Fact]
        public void IdsByPoint2Ds_Performance()
        {
            Dictionary<int, PolygonalFace2D> polygonalFace2Ds_ById = [];

            // A 40 by 50 arrangement of 100 unit squares laid on a 110 unit pitch, so neighbours do not touch.
            int id = 1;
            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    polygonalFace2Ds_ById[id] = PolygonalFace2D_Square(i * 110, j * 110, 100);
                    id++;
                }
            }

            Assert.Equal(2000, polygonalFace2Ds_ById.Count);

            List<Point2D> point2Ds = [];
            for (int i = 0; i < 50000; i++)
            {
                point2Ds.Add(new Point2D((i * 7) % 4400, (i * 13) % 5500));
            }

            // Warm up, so the measurement is not dominated by the first call being compiled.
            polygonalFace2Ds_ById.IdsByPoint2Ds([.. point2Ds.GetRange(0, 100)]);

            Stopwatch stopwatch = Stopwatch.StartNew();
            int?[]? ids = polygonalFace2Ds_ById.IdsByPoint2Ds(point2Ds);
            stopwatch.Stop();

            Assert.NotNull(ids);
            Assert.Equal(point2Ds.Count, ids.Length);

            int count_Assigned = 0;
            foreach (int? id_Temp in ids)
            {
                if (id_Temp.HasValue)
                {
                    count_Assigned++;
                }
            }

            Assert.True(count_Assigned > 0, "The arrangement should place at least some points inside a face.");
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Deciding {point2Ds.Count} points against {polygonalFace2Ds_ById.Count} faces took {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Builds a square face.
        /// </summary>
        /// <param name="x">The X coordinate of the lower left corner.</param>
        /// <param name="y">The Y coordinate of the lower left corner.</param>
        /// <param name="size">The edge length of the square.</param>
        /// <returns>The face.</returns>
        private static PolygonalFace2D PolygonalFace2D_Square(double x, double y, double size)
        {
            IPolygonal2D_Square(x, y, size, out Polygon2D polygon2D);

            PolygonalFace2D? polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D(polygon2D);
            Assert.NotNull(polygonalFace2D);

            return polygonalFace2D;
        }
    }
}
