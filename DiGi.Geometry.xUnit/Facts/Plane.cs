using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Enums;
using System.Collections.Generic;
using System.Diagnostics;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that constructing a Plane with three null points does not throw exceptions and results in null properties.
        /// </summary>
        [Fact]
        public void Plane_ThreeNullPoints()
        {
            Plane plane_Null = new((Point3D?)null, (Point3D?)null, (Point3D?)null);

            Assert.Null(plane_Null.Origin);
            Assert.Null(plane_Null.Normal);
            Assert.Null(plane_Null.AxisY);
            Assert.True(double.IsNaN(plane_Null.A));
            Assert.True(double.IsNaN(plane_Null.B));
            Assert.True(double.IsNaN(plane_Null.C));
            Assert.True(double.IsNaN(plane_Null.D));
        }

        /// <summary>
        /// Tests that constructing a Plane with three collinear points results in a null normal and null AxisY without throwing exceptions.
        /// </summary>
        [Fact]
        public void Plane_CollinearPoints()
        {
            Point3D point3D_1 = new(0.0, 0.0, 0.0);
            Point3D point3D_2 = new(1.0, 1.0, 1.0);
            Point3D point3D_3 = new(2.0, 2.0, 2.0);

            Plane plane_Collinear = new(point3D_1, point3D_2, point3D_3);

            Assert.NotNull(plane_Collinear.Origin);
            Assert.Null(plane_Collinear.Normal);
            Assert.Null(plane_Collinear.AxisY);
            Assert.True(double.IsNaN(plane_Collinear.A));
        }

        /// <summary>
        /// Tests that copy-constructing a Plane with null properties on the source plane correctly preserves those properties as null.
        /// </summary>
        [Fact]
        public void Plane_CopyConstructorNullProperties()
        {
            Plane plane_Source = new((Point3D?)null, (Spatial.Classes.Vector3D?)null);

            Assert.Null(plane_Source.Origin);
            Assert.Null(plane_Source.Normal);
            Assert.Null(plane_Source.AxisY);

            Plane plane_Copy1 = new(plane_Source);
            Assert.Null(plane_Copy1.Origin);
            Assert.Null(plane_Copy1.Normal);
            Assert.Null(plane_Copy1.AxisY);
            Assert.True(double.IsNaN(plane_Copy1.A));

            Plane plane_Copy2 = new(plane_Source, (Point3D?)null);
            Assert.Null(plane_Copy2.Origin);
            Assert.Null(plane_Copy2.Normal);
            Assert.Null(plane_Copy2.AxisY);
            Assert.True(double.IsNaN(plane_Copy2.A));
        }

        /// <summary>
        /// Tests the serialization round-trip and copy constructor of the Plane class using Core.xUnit.Query.SerializationCheck.
        /// </summary>
        [Fact]
        public void Plane_Serialization()
        {
            Point3D point3D_Origin = new(10.0, 20.0, 30.0);
            Spatial.Classes.Vector3D vector3D_Normal = Spatial.Constants.Vector3D.WorldZ;
            Plane plane = new(point3D_Origin, vector3D_Normal);

            Assert.NotNull(plane.Origin);
            Assert.NotNull(plane.Normal);

            DiGi.Core.xUnit.Query.SerializationCheck(plane);
        }

        /// <summary>
        /// Tests factory creation methods, elevation constructors, dimension indices, and plane equation factors A, B, C, D, and K.
        /// </summary>
        [Fact]
        public void Plane_ConstructorsAndEquations()
        {
            Point3D point3D_1 = new(0.0, 0.0, 5.0);
            Point3D point3D_2 = new(10.0, 0.0, 5.0);
            Point3D point3D_3 = new(0.0, 10.0, 5.0);

            Plane? plane_3Pts = Create.Plane(point3D_1, point3D_2, point3D_3);
            Assert.NotNull(plane_3Pts);
            Assert.Equal(0.0, plane_3Pts!.A, 6);
            Assert.Equal(0.0, plane_3Pts.B, 6);
            Assert.Equal(1.0, plane_3Pts.C, 6);
            Assert.Equal(-5.0, plane_3Pts.D, 6);
            Assert.Equal(5.0, plane_3Pts.K, 6);

            Plane? plane_Elevation = Create.Plane(15.0);
            Assert.NotNull(plane_Elevation);
            Assert.Equal(15.0, plane_Elevation!.Origin!.Z, 6);
            Assert.Equal(-15.0, plane_Elevation.D, 6);

            Plane? plane_DimX = Create.Plane(3.0, 0);
            Assert.NotNull(plane_DimX);
            Assert.Equal(3.0, plane_DimX!.Origin!.X, 6);
            Assert.Equal(1.0, plane_DimX.A, 6);

            Plane? plane_DimY = Create.Plane(4.0, 1);
            Assert.NotNull(plane_DimY);
            Assert.Equal(4.0, plane_DimY!.Origin!.Y, 6);
            Assert.Equal(1.0, plane_DimY.B, 6);

            Plane? plane_DimZ = Create.Plane(5.0, 2);
            Assert.NotNull(plane_DimZ);
            Assert.Equal(5.0, plane_DimZ!.Origin!.Z, 6);
            Assert.Equal(1.0, plane_DimZ.C, 6);

            Assert.Null(Create.Plane(double.NaN));
            Assert.Null(Create.Plane(1.0, 99));
        }

        /// <summary>
        /// Tests ClosestPoint and Distance calculations on Plane instances.
        /// </summary>
        [Fact]
        public void Plane_ClosestPointAndDistance()
        {
            Plane plane = Spatial.Constants.Plane.WorldZ;
            Point3D point3D_Test = new(5.0, -10.0, 7.5);

            Point3D? point3D_Closest = plane.ClosestPoint(point3D_Test);
            Assert.NotNull(point3D_Closest);
            Assert.Equal(5.0, point3D_Closest!.X, 6);
            Assert.Equal(-10.0, point3D_Closest.Y, 6);
            Assert.Equal(0.0, point3D_Closest.Z, 6);

            double double_Distance = plane.Distance(point3D_Test);
            Assert.Equal(7.5, double_Distance, 6);

            Assert.Null(plane.ClosestPoint(null));
            Assert.True(double.IsNaN(plane.Distance(null)));
        }

        /// <summary>
        /// Tests Coplanar evaluation, ensuring identical or inverted normal planes on the same geometric plane are coplanar, while parallel offset planes return false.
        /// </summary>
        [Fact]
        public void Plane_Coplanar()
        {
            Plane plane_Z0 = Spatial.Constants.Plane.WorldZ;
            Plane plane_Z0_Inverted = new(new Point3D(0.0, 0.0, 0.0), Spatial.Constants.Vector3D.WorldZ.GetInversed());
            Plane plane_Z10 = Create.Plane(10.0)!;
            Plane plane_X0 = Spatial.Constants.Plane.WorldX;

            Assert.True(plane_Z0.Coplanar(plane_Z0));
            Assert.True(plane_Z0.Coplanar(plane_Z0_Inverted));
            Assert.False(plane_Z0.Coplanar(plane_Z10));
            Assert.False(plane_Z0.Coplanar(plane_X0));
            Assert.False(plane_Z0.Coplanar(null));
        }

        /// <summary>
        /// Tests Perpendicular evaluation between orthogonal and non-orthogonal planes.
        /// </summary>
        [Fact]
        public void Plane_Perpendicular()
        {
            Plane plane_WorldX = Spatial.Constants.Plane.WorldX;
            Plane plane_WorldY = Spatial.Constants.Plane.WorldY;
            Plane plane_WorldZ = Spatial.Constants.Plane.WorldZ;

            Assert.True(plane_WorldX.Perpendicular(plane_WorldY));
            Assert.True(plane_WorldX.Perpendicular(plane_WorldZ));
            Assert.True(plane_WorldY.Perpendicular(plane_WorldZ));

            Assert.False(plane_WorldX.Perpendicular(plane_WorldX));
            Assert.False(plane_WorldX.Perpendicular(null));
        }

        /// <summary>
        /// Tests Above and On queries across points, segments, and vectors including exact tolerance boundaries.
        /// </summary>
        [Fact]
        public void Plane_AboveAndOn_ToleranceBoundaries()
        {
            Plane plane = Spatial.Constants.Plane.WorldZ;
            double tolerance = 1e-3;

            Point3D point3D_OnInside = new(2.0, 3.0, 1e-3 - 1e-9);
            Point3D point3D_AboveOutside = new(2.0, 3.0, 1e-3 + 1e-9);

            Assert.True(Query.On(plane, point3D_OnInside, tolerance));
            Assert.False(Query.Above(plane, point3D_OnInside, tolerance));

            Assert.False(Query.On(plane, point3D_AboveOutside, tolerance));
            Assert.True(Query.Above(plane, point3D_AboveOutside, tolerance));

            Segment3D segment3D_On = new(new Point3D(0.0, 0.0, 0.0), new Point3D(5.0, 5.0, 0.0));
            Assert.True(Query.On(plane, segment3D_On, tolerance));

            Spatial.Classes.Vector3D vector3D_Parallel = new(10.0, -5.0, 0.0);
            Spatial.Classes.Vector3D vector3D_Oblique = new(10.0, -5.0, 2.0);

            Assert.True(Query.On(plane, vector3D_Parallel, tolerance));
            Assert.False(Query.On(plane, vector3D_Oblique, tolerance));
        }

        /// <summary>
        /// Tests 2D to 3D and 3D to 2D conversions and vector/point projections onto a Plane.
        /// </summary>
        [Fact]
        public void Plane_ProjectAndConvert()
        {
            Plane plane = Spatial.Constants.Plane.WorldZ;
            Point3D point3D_Spatial = new(3.0, 4.0, 10.0);

            Point2D? point2D_Local = plane.Convert(point3D_Spatial);
            Assert.NotNull(point2D_Local);
            Assert.Equal(3.0, point2D_Local!.X, 6);
            Assert.Equal(4.0, point2D_Local.Y, 6);

            Point3D? point3D_Reconverted = plane.Convert(point2D_Local);
            Assert.NotNull(point3D_Reconverted);
            Assert.Equal(3.0, point3D_Reconverted!.X, 6);
            Assert.Equal(4.0, point3D_Reconverted.Y, 6);
            Assert.Equal(0.0, point3D_Reconverted.Z, 6);

            Spatial.Classes.Vector3D vector3D_Spatial = new(1.0, 2.0, 5.0);
            Spatial.Classes.Vector3D? vector3D_Projected = plane.Project(vector3D_Spatial);
            Assert.NotNull(vector3D_Projected);
            Assert.Equal(1.0, vector3D_Projected!.X, 6);
            Assert.Equal(2.0, vector3D_Projected.Y, 6);
            Assert.Equal(0.0, vector3D_Projected.Z, 6);
        }

        /// <summary>
        /// Tests the Flip method to ensure spatial orientation vectors invert as expected.
        /// </summary>
        [Fact]
        public void Plane_Flip()
        {
            Plane plane = new(new Point3D(0.0, 0.0, 0.0), Spatial.Constants.Vector3D.WorldZ);
            Spatial.Classes.Vector3D? vector3D_NormalBefore = plane.Normal;
            Assert.NotNull(vector3D_NormalBefore);
            Assert.Equal(1.0, vector3D_NormalBefore!.Z, 6);

            bool bool_Flipped = plane.Flip(SpatialAxis.Z, SpatialAxis.Y);
            Assert.True(bool_Flipped);

            Spatial.Classes.Vector3D? vector3D_NormalAfter = plane.Normal;
            Assert.NotNull(vector3D_NormalAfter);
            Assert.Equal(-1.0, vector3D_NormalAfter!.Z, 6);

            Assert.False(plane.Flip(SpatialAxis.Z, SpatialAxis.Z));
        }

        /// <summary>
        /// Tests the performance of batch operations on Plane over a large dataset.
        /// </summary>
        [Fact]
        public void Plane_Performance()
        {
            Plane plane = Spatial.Constants.Plane.WorldZ;
            int count = 10000;
            List<Point3D> point3Ds = new(count);

            for (int i = 0; i < count; i++)
            {
                point3Ds.Add(new Point3D(i * 0.1, i * 0.2, 5.0));
            }

            // Warmup / JIT
            _ = plane.Above(point3Ds[0]);
            _ = plane.Convert(point3Ds[0]);

            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < count; i++)
            {
                _ = plane.Above(point3Ds[i]);
                _ = plane.Convert(point3Ds[i]);
            }

            stopwatch.Stop();

            Assert.True(stopwatch.ElapsedMilliseconds < 50, $"Batch operation on Plane took {stopwatch.ElapsedMilliseconds} ms, expected under 50 ms.");
        }
    }
}