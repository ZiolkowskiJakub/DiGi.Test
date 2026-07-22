using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests 3D boolean union query extension method between two polyhedra.
        /// </summary>
        [Fact]
        public void Polyhedron_Union_Query()
        {
            Point3D point3D_Min1 = new(-2, -2, -2);
            Point3D point3D_Max1 = new(2, 2, 2);
            BoundingBox3D boundingBox3D_1 = new(point3D_Min1, point3D_Max1);
            Polyhedron? polyhedron_1 = Create.Polyhedron(boundingBox3D_1);
            Assert.NotNull(polyhedron_1);

            Point3D point3D_Min2 = new(0, 0, 0);
            Point3D point3D_Max2 = new(4, 4, 4);
            BoundingBox3D boundingBox3D_2 = new(point3D_Min2, point3D_Max2);
            Polyhedron? polyhedron_2 = Create.Polyhedron(boundingBox3D_2);
            Assert.NotNull(polyhedron_2);

            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<Polyhedron>? polyhedrons = Spatial.Query.Union(polyhedron_1, polyhedron_2);
            Assert.NotNull(polyhedrons);
            Assert.Single(polyhedrons);

            Polyhedron? resultPoly = polyhedrons.FirstOrDefault();
            Assert.NotNull(resultPoly);
            if (resultPoly != null)
            {
                BoundingBox3D? bbox = resultPoly.GetBoundingBox();
                Assert.NotNull(bbox);
                if (bbox != null)
                {
                    Assert.Equal(216.0, bbox.GetVolume(), 5);
                }
            }
        }

        /// <summary>
        /// Tests 3D boolean difference query extension method between two polyhedra.
        /// </summary>
        [Fact]
        public void Polyhedron_Difference_Query()
        {
            Point3D point3D_Min1 = new(-2, -2, -2);
            Point3D point3D_Max1 = new(2, 2, 2);
            BoundingBox3D boundingBox3D_1 = new(point3D_Min1, point3D_Max1);
            Polyhedron? polyhedron_1 = Create.Polyhedron(boundingBox3D_1);
            Assert.NotNull(polyhedron_1);

            Point3D point3D_Min2 = new(0, -2, -2);
            Point3D point3D_Max2 = new(2, 2, 2);
            BoundingBox3D boundingBox3D_2 = new(point3D_Min2, point3D_Max2);
            Polyhedron? polyhedron_2 = Create.Polyhedron(boundingBox3D_2);
            Assert.NotNull(polyhedron_2);

            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<Polyhedron>? polyhedrons = Spatial.Query.Difference(polyhedron_1, polyhedron_2);
            Assert.NotNull(polyhedrons);
            Assert.Single(polyhedrons);
        }

        /// <summary>
        /// Tests 3D boolean intersection query extension method between two polyhedra.
        /// </summary>
        [Fact]
        public void Polyhedron_Intersection_Query()
        {
            Point3D point3D_Min1 = new(-2, -2, -2);
            Point3D point3D_Max1 = new(2, 2, 2);
            BoundingBox3D boundingBox3D_1 = new(point3D_Min1, point3D_Max1);
            Polyhedron? polyhedron_1 = Create.Polyhedron(boundingBox3D_1);
            Assert.NotNull(polyhedron_1);

            Point3D point3D_Min2 = new(0, 0, 0);
            Point3D point3D_Max2 = new(4, 4, 4);
            BoundingBox3D boundingBox3D_2 = new(point3D_Min2, point3D_Max2);
            Polyhedron? polyhedron_2 = Create.Polyhedron(boundingBox3D_2);
            Assert.NotNull(polyhedron_2);

            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            List<Polyhedron>? polyhedrons = Spatial.Query.Intersection(polyhedron_1, polyhedron_2);
            Assert.NotNull(polyhedrons);
            Assert.Single(polyhedrons);
        }

        /// <summary>
        /// Tests 3D boolean union extension method on a collection of multiple disjoint polyhedra.
        /// </summary>
        [Fact]
        public void Polyhedron_Union_Multiple_Disjoint()
        {
            Polyhedron? poly1 = Create.Polyhedron(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(2, 2, 2)));
            Polyhedron? poly2 = Create.Polyhedron(new BoundingBox3D(new Point3D(10, 10, 10), new Point3D(12, 12, 12)));
            Polyhedron? poly3 = Create.Polyhedron(new BoundingBox3D(new Point3D(20, 20, 20), new Point3D(22, 22, 22)));

            Assert.NotNull(poly1);
            Assert.NotNull(poly2);
            Assert.NotNull(poly3);

            if (poly1 == null || poly2 == null || poly3 == null)
            {
                return;
            }

            List<Polyhedron> list = [poly1, poly2, poly3];
            List<Polyhedron>? result = Spatial.Query.Union(list);

            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        /// <summary>
        /// Tests 3D boolean union extension method on a collection of multiple overlapping polyhedra using spatial partitioning.
        /// </summary>
        [Fact]
        public void Polyhedron_Union_Multiple_Overlapping()
        {
            Polyhedron? poly1 = Create.Polyhedron(new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(2, 2, 2)));
            Polyhedron? poly2 = Create.Polyhedron(new BoundingBox3D(new Point3D(1, 0, 0), new Point3D(3, 2, 2)));
            Polyhedron? poly3 = Create.Polyhedron(new BoundingBox3D(new Point3D(2, 0, 0), new Point3D(4, 2, 2)));
            Polyhedron? poly4 = Create.Polyhedron(new BoundingBox3D(new Point3D(10, 0, 0), new Point3D(12, 2, 2)));

            Assert.NotNull(poly1);
            Assert.NotNull(poly2);
            Assert.NotNull(poly3);
            Assert.NotNull(poly4);

            if (poly1 == null || poly2 == null || poly3 == null || poly4 == null)
            {
                return;
            }

            List<Polyhedron> list = [poly1, poly2, poly3, poly4];
            List<Polyhedron>? result = Spatial.Query.Union(list);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }
    }
}
