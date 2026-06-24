using DiGi.Geometry.Spatial.Classes;
using System.Collections.Generic;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the Centroid calculation for various collections of Point3D objects, verifying single points, segments, triangles (exactly three points), planar polygons, and boundary/edge conditions.
        /// </summary>
        [Fact]
        public void Centroid()
        {
            // Null input
            List<Point3D>? point3Ds_Null = null;
            Point3D? point3D_ResultNull = DiGi.Geometry.Spatial.Query.Centroid(point3Ds_Null);
            Assert.Null(point3D_ResultNull);

            // Empty input
            List<Point3D> point3Ds_Empty = [];
            Point3D? point3D_ResultEmpty = DiGi.Geometry.Spatial.Query.Centroid(point3Ds_Empty);
            Assert.Null(point3D_ResultEmpty);

            // Single point
            Point3D point3D_Single = new(1.0, 2.0, 3.0);
            List<Point3D> point3Ds_One = [point3D_Single];
            Point3D? point3D_ResultOne = DiGi.Geometry.Spatial.Query.Centroid(point3Ds_One);
            Assert.NotNull(point3D_ResultOne);
            Assert.Equal(1.0, point3D_ResultOne.X);
            Assert.Equal(2.0, point3D_ResultOne.Y);
            Assert.Equal(3.0, point3D_ResultOne.Z);

            // Two points (segment midpoint)
            Point3D point3D_A = new(0.0, 0.0, 0.0);
            Point3D point3D_B = new(2.0, 4.0, 6.0);
            List<Point3D> point3Ds_Two = [point3D_A, point3D_B];
            Point3D? point3D_ResultTwo = DiGi.Geometry.Spatial.Query.Centroid(point3Ds_Two);
            Assert.NotNull(point3D_ResultTwo);
            Assert.Equal(1.0, point3D_ResultTwo.X);
            Assert.Equal(2.0, point3D_ResultTwo.Y);
            Assert.Equal(3.0, point3D_ResultTwo.Z);

            // Three points (triangle - previously bugged because it retrieved index 1 instead of index 2)
            Point3D point3D_C = new(0.0, 0.0, 0.0);
            Point3D point3D_D = new(3.0, 0.0, 0.0);
            Point3D point3D_E = new(0.0, 3.0, 6.0);
            List<Point3D> point3Ds_Three = [point3D_C, point3D_D, point3D_E];
            Point3D? point3D_ResultThree = DiGi.Geometry.Spatial.Query.Centroid(point3Ds_Three);
            Assert.NotNull(point3D_ResultThree);
            // Expected centroid coordinates: (0+3+0)/3 = 1.0, (0+0+3)/3 = 1.0, (0+0+6)/3 = 2.0
            Assert.Equal(1.0, point3D_ResultThree.X);
            Assert.Equal(1.0, point3D_ResultThree.Y);
            Assert.Equal(2.0, point3D_ResultThree.Z);

            // Four points (planar square in XY plane at Z=5)
            Point3D point3D_P1 = new(0.0, 0.0, 5.0);
            Point3D point3D_P2 = new(2.0, 0.0, 5.0);
            Point3D point3D_P3 = new(2.0, 2.0, 5.0);
            Point3D point3D_P4 = new(0.0, 2.0, 5.0);
            List<Point3D> point3Ds_Four = [point3D_P1, point3D_P2, point3D_P3, point3D_P4];
            Point3D? point3D_ResultFour = DiGi.Geometry.Spatial.Query.Centroid(point3Ds_Four);
            Assert.NotNull(point3D_ResultFour);
            Assert.Equal(1.0, point3D_ResultFour.X);
            Assert.Equal(1.0, point3D_ResultFour.Y);
            Assert.Equal(5.0, point3D_ResultFour.Z);
        }
    }
}
