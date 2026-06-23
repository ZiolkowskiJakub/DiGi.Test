using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the functionality of splitting a <see cref="Polyhedron"/> using a plane to ensure that the polyhedron is correctly divided into multiple parts.
        /// </summary>
        [Fact]
        public void Split()
        {
            PolygonalFace3D? polygonalFace3D = Create.PolygonalFace3D(Spatial.Constants.Plane.WorldZ, new Point2D(0, 0), new Point2D(0, 10), new Point2D(10, 10), new Point2D(10, 0));

            Assert.NotNull(polygonalFace3D);

            if (polygonalFace3D is null)
            {
                return;
            }

            PolygonalFaceExtrusion polygonalFaceExtrusion = new(polygonalFace3D, Spatial.Constants.Vector3D.WorldZ * 10);

            Assert.NotNull(polygonalFaceExtrusion);

            if (polygonalFaceExtrusion is null)
            {
                return;
            }

            Polyhedron? polyhedron = Create.Polyhedron(polygonalFaceExtrusion);
            Assert.NotNull(polyhedron);

            Assert.True(Spatial.Query.TrySplit(new Plane(Create.Plane(1.0)), polyhedron, out List<Polyhedron>? polyhedrons));

            Assert.NotNull(polyhedrons);
            if (polyhedrons is null)
            {
                return;
            }

            foreach (Polyhedron polyhedron_Split in polyhedrons)
            {
                Point3D? internalPoint = polyhedron_Split?.GetInternalPoint();
                Assert.NotNull(internalPoint);

                if (internalPoint is null)
                {
                    continue;
                }

                bool inside = polyhedron.Inside(internalPoint);
                Assert.True(inside);
            }
        }

        /// <summary>
        /// Tests splitting a 3D polygonal face with other polygonal faces.
        /// </summary>
        [Fact]
        public void SplitFace()
        {
            Polygon2D polygon2D_1 = new(new List<Point2D>()
            {
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(0, 10)
            });

            PolygonalFace2D? polygonalFace2D_1 = Planar.Create.PolygonalFace2D(polygon2D_1);
            Assert.NotNull(polygonalFace2D_1);

            PolygonalFace3D polygonalFace3D_1 = new(Spatial.Constants.Plane.WorldZ, polygonalFace2D_1);

            Polygon2D polygon2D_2 = new(new List<Point2D>()
            {
                new Point2D(5, 5),
                new Point2D(20, 5),
                new Point2D(20, 20),
                new Point2D(5, 20)
            });

            Polygon3D? polygon3D_2 = Create.Polygon3D(new List<Point3D>()
            {
                new Point3D(2, 5, -1),
                new Point3D(2, 20, -1),
                new Point3D(2, 20, 1),
                new Point3D(2, 5, 1)
            });
            Assert.NotNull(polygon3D_2);

            PolygonalFace3D? polygonalFace3D_2 = Create.PolygonalFace3D(polygon3D_2);
            Assert.NotNull(polygonalFace3D_2);

            Polygon3D? polygon3D_3 = Create.Polygon3D(new List<Point3D>()
            {
                new Point3D(2, 5, -1),
                new Point3D(2, 5, 1),
                new Point3D(10, 5, 1),
                new Point3D(10, 5, -1)
            });
            Assert.NotNull(polygon3D_3);

            PolygonalFace3D? polygonalFace3D_3 = Create.PolygonalFace3D(polygon3D_3);
            Assert.NotNull(polygonalFace3D_3);

            bool splitSuccess = polygonalFace3D_1.TrySplit(new IPolygonalFace3D[] { polygonalFace3D_2!, polygonalFace3D_3! }, out List<PolygonalFace3D>? polygonalFace3Ds);
            Assert.True(splitSuccess);
            Assert.NotNull(polygonalFace3Ds);
            Assert.NotEmpty(polygonalFace3Ds);
        }
    }
}