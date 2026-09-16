using DiGi.Geometry.Planar.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies <see cref="Query.ContainerIds(IReadOnlyDictionary{int, PolygonalFace2D}, double)"/> on a nested layer shaped like Warsaw's: a city holding a district holding a neighbourhood, a second district sharing the city's outer edge, and a flat area off to the side.
        /// <para>The neighbourhood lists its district first and the city last (smallest first); each district lists the city alone, including the one whose box touches the city's box on two sides - a strict box test would miss it, and every Warsaw district touches the city boundary somewhere; the city and the flat area are top level. Without this the municipality roll-up sums the city, its districts and its neighbourhoods together, which wrote Warsaw as 4.6 million against a city of 1 622 594 (<see href="https://github.com/ZiolkowskiJakub/DiGi.GIS.PostgreSQL/issues/77">DiGi.GIS.PostgreSQL#77</see>).</para>
        /// </summary>
        [Fact]
        public void ContainerIds_NestedLayer()
        {
            Dictionary<int, PolygonalFace2D> polygonalFace2Ds_ById = new()
            {
                [1] = PolygonalFace2D_Square(0, 0, 100),
                [2] = PolygonalFace2D_Square(10, 10, 40),
                [3] = PolygonalFace2D_Square(20, 20, 10),
                [4] = PolygonalFace2D_Square(60, 60, 40),
                [5] = PolygonalFace2D_Square(200, 0, 30),
            };

            Dictionary<int, List<int>> containerIds_ById = polygonalFace2Ds_ById.ContainerIds();

            Assert.Equal(5, containerIds_ById.Count);
            Assert.Empty(containerIds_ById[1]);
            Assert.Equal([1], containerIds_ById[2]);
            Assert.Equal([2, 1], containerIds_ById[3]);
            Assert.Equal([1], containerIds_ById[4]);
            Assert.Empty(containerIds_ById[5]);
        }

        /// <summary>
        /// Verifies that a box inside a box is not enough: an area whose bounding box lies within a ring-shaped area's box, but whose interior point falls in that area's hole, has no container; and that two areas of the same footprint contain neither the other.
        /// <para>The first is a town excluded from the area around it - a face with a hole - where a box test alone would file the town under its surroundings. The second guards the strict "larger" requirement, without which a duplicated row would name itself nested and drop out of the roll-up.</para>
        /// </summary>
        [Fact]
        public void ContainerIds_HoleAndEqualFootprint()
        {
            Polygon2D polygon2D_External = new([new Point2D(0, 0), new Point2D(100, 0), new Point2D(100, 100), new Point2D(0, 100)]);
            Polygon2D polygon2D_Internal = new([new Point2D(30, 30), new Point2D(70, 30), new Point2D(70, 70), new Point2D(30, 70)]);

            PolygonalFace2D? polygonalFace2D_Ring = Geometry.Planar.Create.PolygonalFace2D(polygon2D_External, [polygon2D_Internal]);
            Assert.NotNull(polygonalFace2D_Ring);

            Dictionary<int, PolygonalFace2D> polygonalFace2Ds_ById = new()
            {
                [1] = polygonalFace2D_Ring,
                [2] = PolygonalFace2D_Square(40, 40, 20),
                [3] = PolygonalFace2D_Square(5, 5, 10),
                [4] = PolygonalFace2D_Square(5, 5, 10),
            };

            Dictionary<int, List<int>> containerIds_ById = polygonalFace2Ds_ById.ContainerIds();

            Assert.Empty(containerIds_ById[1]);
            Assert.Empty(containerIds_ById[2]);
            Assert.Equal([1], containerIds_ById[3]);
            Assert.Equal([1], containerIds_ById[4]);
        }

        /// <summary>
        /// Verifies the empty and null answers: no faces, or none given, is an empty map rather than an exception.
        /// </summary>
        [Fact]
        public void ContainerIds_Empty()
        {
            Assert.Empty(Query.ContainerIds(null));
            Assert.Empty(new Dictionary<int, PolygonalFace2D>().ContainerIds());
        }
    }
}
