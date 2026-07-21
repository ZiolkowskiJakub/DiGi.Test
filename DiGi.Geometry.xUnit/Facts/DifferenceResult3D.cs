using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests 3D boolean difference between two disjoint polyhedra.
        /// </summary>
        [Fact]
        public void Polyhedron_DifferenceResult3D_Disjoint()
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

            DifferenceResult3D? differenceResult3D = polyhedron_1.DifferenceResult3D(polyhedron_2);
            Assert.NotNull(differenceResult3D);
            Assert.True(differenceResult3D.Any());

            List<Polyhedron>? polyhedrons = differenceResult3D.GetGeometry3Ds<Polyhedron>();
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
                    Assert.Equal(64.0, bbox.GetVolume(), 5);
                }
            }
        }

        /// <summary>
        /// Tests 3D boolean difference between two identical polyhedra.
        /// </summary>
        [Fact]
        public void Polyhedron_DifferenceResult3D_Identical()
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

            DifferenceResult3D? differenceResult3D = polyhedron_1.DifferenceResult3D(polyhedron_1);
            Assert.NotNull(differenceResult3D);
            Assert.False(differenceResult3D.Any());
            Assert.Equal(0, differenceResult3D.Count);
        }

        /// <summary>
        /// Tests 3D boolean difference between two overlapping polyhedra.
        /// </summary>
        [Fact]
        public void Polyhedron_DifferenceResult3D_Overlapping()
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

            DifferenceResult3D? differenceResult3D = polyhedron_1.DifferenceResult3D(polyhedron_2);
            Assert.NotNull(differenceResult3D);
            Assert.True(differenceResult3D.Any());

            List<Polyhedron>? polyhedrons = differenceResult3D.GetGeometry3Ds<Polyhedron>();
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
                    Assert.Equal(64.0, bbox.GetVolume(), 5);
                }
            }
        }

        /// <summary>
        /// Tests 3D boolean difference for coplanar touching polyhedra.
        /// </summary>
        [Fact]
        public void Polyhedron_DifferenceResult3D_CoplanarTouch()
        {
            Point3D point3D_Min1 = new(-2, -2, -2);
            Point3D point3D_Max1 = new(2, 2, 2);
            BoundingBox3D boundingBox3D_1 = new(point3D_Min1, point3D_Max1);
            Polyhedron? polyhedron_1 = Create.Polyhedron(boundingBox3D_1);
            Assert.NotNull(polyhedron_1);

            // Touches at x=2 face
            Point3D point3D_Min2 = new(2, -2, -2);
            Point3D point3D_Max2 = new(6, 2, 2);
            BoundingBox3D boundingBox3D_2 = new(point3D_Min2, point3D_Max2);
            Polyhedron? polyhedron_2 = Create.Polyhedron(boundingBox3D_2);
            Assert.NotNull(polyhedron_2);

            if (polyhedron_1 == null || polyhedron_2 == null)
            {
                return;
            }

            DifferenceResult3D? differenceResult3D = polyhedron_1.DifferenceResult3D(polyhedron_2);
            Assert.NotNull(differenceResult3D);
            Assert.True(differenceResult3D.Any());

            List<Polyhedron>? polyhedrons = differenceResult3D.GetGeometry3Ds<Polyhedron>();
            Assert.NotNull(polyhedrons);
            Assert.Single(polyhedrons);
        }

        /// <summary>
        /// Tests the fallback behavior of DifferenceResult3D when non-solid geometry remnants are returned.
        /// </summary>
        [Fact]
        public void DifferenceResult3D_Fallback_LowerGeometries()
        {
            // Verify that DifferenceResult3D successfully holds and retrieves lower-dimensional elements (such as face remnants)
            Point3D point3D_1 = new(0, 0, 0);
            Point3D point3D_2 = new(1, 1, 1);
            Segment3D segment3D = new(point3D_1, point3D_2);

            List<IGeometry3D> geomList = [segment3D];
            DifferenceResult3D differenceResult3D = new(geomList);

            Assert.True(differenceResult3D.Any());
            Assert.Equal(1, differenceResult3D.Count);
            Assert.True(differenceResult3D.Contains<Segment3D>());

            List<Segment3D>? retrievedSegments = differenceResult3D.GetGeometry3Ds<Segment3D>();
            Assert.NotNull(retrievedSegments);
            Assert.Single(retrievedSegments);
            Assert.Equal(segment3D.Length, retrievedSegments[0].Length, 5);
        }
    }
}