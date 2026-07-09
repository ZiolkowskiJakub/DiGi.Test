using DiGi.Geometry.Planar.Interfaces;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests 3D boolean union between two disjoint polyhedra, verifying multiple solid components.
        /// </summary>
        [Fact]
        public void Polyhedron_UnionResult3D_Disjoint()
        {
            Point3D point3D_Min1 = new(-2, -2, -2);
            Point3D point3D_Max1 = new(2, 2, 2);
            BoundingBox3D boundingBox3D_1 = new(point3D_Min1, point3D_Max1);
            Polyhedron? polyhedron_1 = Create.Polyhedron(boundingBox3D_1);
            Assert.NotNull(polyhedron_1);

            Point3D point3D_Min2 = new(10, 10, 10);
            Point3D point3D_Max2 = new(15, 15, 15);
            BoundingBox3D boundingBox3D_2 = new(point3D_Min2, point3D_Max2);
            Polyhedron? polyhedron_2 = Create.Polyhedron(boundingBox3D_2);
            Assert.NotNull(polyhedron_2);

            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            UnionResult3D? unionResult3D = polyhedron_1.UnionResult3D(polyhedron_2);
            Assert.NotNull(unionResult3D);
            Assert.True(unionResult3D.Any());
            Assert.Equal(2, unionResult3D.Count);

            List<Polyhedron>? polyhedrons = unionResult3D.GetGeometry3Ds<Polyhedron>();
            Assert.NotNull(polyhedrons);
            Assert.Equal(2, polyhedrons.Count);
        }

        /// <summary>
        /// Tests 3D boolean union between two identical polyhedra.
        /// </summary>
        [Fact]
        public void Polyhedron_UnionResult3D_Identical()
        {
            Point3D point3D_Min1 = new(-2, -2, -2);
            Point3D point3D_Max1 = new(2, 2, 2);
            BoundingBox3D boundingBox3D_1 = new(point3D_Min1, point3D_Max1);
            Polyhedron? polyhedron_1 = Create.Polyhedron(boundingBox3D_1);
            Assert.NotNull(polyhedron_1);

            if (polyhedron_1 == null)
            {
                return;
            }

            UnionResult3D? unionResult3D = polyhedron_1.UnionResult3D(polyhedron_1);
            Assert.NotNull(unionResult3D);
            Assert.True(unionResult3D.Any());
            Assert.Equal(1, unionResult3D.Count);

            List<Polyhedron>? polyhedrons = unionResult3D.GetGeometry3Ds<Polyhedron>();
            Assert.NotNull(polyhedrons);
            Assert.Single(polyhedrons);
        }

        /// <summary>
        /// Tests 3D boolean union between two overlapping polyhedra.
        /// </summary>
        [Fact]
        public void Polyhedron_UnionResult3D_Overlapping()
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

            UnionResult3D? unionResult3D = polyhedron_1.UnionResult3D(polyhedron_2);
            Assert.NotNull(unionResult3D);
            Assert.True(unionResult3D.Any());

            List<Polyhedron>? polyhedrons = unionResult3D.GetGeometry3Ds<Polyhedron>();
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
                    Assert.Equal(216.0, bbox.GetVolume(), 5); // [-2, 4]^3 has volume 6^3 = 216
                }
            }
        }

        /// <summary>
        /// Tests 3D boolean union for coplanar touching polyhedra.
        /// </summary>
        [Fact]
        public void Polyhedron_UnionResult3D_CoplanarTouch()
        {
            Point3D point3D_Min1 = new(-2, -2, -2);
            Point3D point3D_Max1 = new(2, 2, 2);
            BoundingBox3D boundingBox3D_1 = new(point3D_Min1, point3D_Max1);
            Polyhedron? polyhedron_1 = Create.Polyhedron(boundingBox3D_1);
            Assert.NotNull(polyhedron_1);

            Point3D point3D_Min2 = new(2, -2, -2);
            Point3D point3D_Max2 = new(6, 2, 2);
            BoundingBox3D boundingBox3D_2 = new(point3D_Min2, point3D_Max2);
            Polyhedron? polyhedron_2 = Create.Polyhedron(boundingBox3D_2);
            Assert.NotNull(polyhedron_2);

            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            UnionResult3D? unionResult3D = polyhedron_1.UnionResult3D(polyhedron_2);
            Assert.NotNull(unionResult3D);
            Assert.True(unionResult3D.Any());

            List<Polyhedron>? polyhedrons = unionResult3D.GetGeometry3Ds<Polyhedron>();
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
                    Assert.Equal(128.0, bbox.GetVolume(), 5); // [-2, 6] x [-2, 2] x [-2, 2] has volume 8 * 4 * 4 = 128
                }
            }
        }

        /// <summary>
        /// Tests the fallback behavior of UnionResult3D when non-solid geometry remnants are returned.
        /// </summary>
        [Fact]
        public void UnionResult3D_Fallback_LowerGeometries()
        {
            Point3D point3D_1 = new(0, 0, 0);
            Point3D point3D_2 = new(1, 1, 1);
            Segment3D segment3D = new(point3D_1, point3D_2);

            List<IGeometry3D> geomList = [segment3D];
            UnionResult3D unionResult3D = new(geomList);

            Assert.True(unionResult3D.Any());
            Assert.Equal(1, unionResult3D.Count);
            Assert.True(unionResult3D.Contains<Segment3D>());

            List<Segment3D>? retrievedSegments = unionResult3D.GetGeometry3Ds<Segment3D>();
            Assert.NotNull(retrievedSegments);
            Assert.Single(retrievedSegments);
            Assert.Equal(segment3D.Length, retrievedSegments[0].Length, 5);
        }
    }
}
