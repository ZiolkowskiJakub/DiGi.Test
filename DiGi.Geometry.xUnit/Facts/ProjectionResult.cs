using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the functionality of projecting a 3D ellipse onto a plane.
        /// </summary>
        [Fact]
        public void ProjectionResult_Ellipse3D()
        {
            // Case 1: Null Inputs
            Plane? plane_Null = null;
            Ellipse3D? ellipse3D_Null = null;
            Plane plane_Valid = Spatial.Constants.Plane.WorldZ;
            Ellipse3D ellipse3D_Valid = new(Spatial.Constants.Plane.WorldZ, new Ellipse2D(new Point2D(0, 0), 5, 3));

            ProjectionResult? projectionResult_NullPlane = Create.ProjectionResult(plane_Null, ellipse3D_Valid);
            Assert.Null(projectionResult_NullPlane);

            ProjectionResult? projectionResult_NullEllipse = Create.ProjectionResult(plane_Valid, ellipse3D_Null);
            Assert.Null(projectionResult_NullEllipse);

            // Case 2: Parallel Plane Projection (Same Orientation)
            Plane plane_Parallel1 = new(new Point3D(0, 0, 5), Spatial.Constants.Vector3D.WorldZ);
            ProjectionResult? projectionResult_Parallel1 = Create.ProjectionResult(plane_Parallel1, ellipse3D_Valid);
            Assert.NotNull(projectionResult_Parallel1);
            List<Ellipse2D>? ellipse2Ds_Parallel1 = projectionResult_Parallel1!.GetGeometry2Ds<Ellipse2D>();
            Assert.NotNull(ellipse2Ds_Parallel1);
            Assert.Single(ellipse2Ds_Parallel1);
            Ellipse2D ellipse2D_Parallel1 = ellipse2Ds_Parallel1[0];
            Assert.Equal(0, ellipse2D_Parallel1.Center!.X, 6);
            Assert.Equal(0, ellipse2D_Parallel1.Center.Y, 6);
            Assert.Equal(5, ellipse2D_Parallel1.A, 6);
            Assert.Equal(3, ellipse2D_Parallel1.B, 6);

            // Case 3: Parallel Plane Projection (Rotated Coordinate System)
            Plane plane_Parallel2 = new(new Point3D(0, 0, 0), Spatial.Constants.Vector3D.WorldY, Spatial.Constants.Vector3D.WorldX.GetInversed());
            ProjectionResult? projectionResult_Parallel2 = Create.ProjectionResult(plane_Parallel2, ellipse3D_Valid);
            Assert.NotNull(projectionResult_Parallel2);
            List<Ellipse2D>? ellipse2Ds_Parallel2 = projectionResult_Parallel2!.GetGeometry2Ds<Ellipse2D>();
            Assert.NotNull(ellipse2Ds_Parallel2);
            Assert.Single(ellipse2Ds_Parallel2);

            // Case 4: Perpendicular Plane Projection (Edge-on along Major Axis)
            Plane plane_Perpendicular1 = Spatial.Constants.Plane.WorldY; // Normal is WorldY, ellipse has major axis along WorldX
            ProjectionResult? projectionResult_Perp1 = Create.ProjectionResult(plane_Perpendicular1, ellipse3D_Valid);
            Assert.NotNull(projectionResult_Perp1);
            List<Segment2D>? segment2Ds_Perp1 = projectionResult_Perp1!.GetGeometry2Ds<Segment2D>();
            Assert.NotNull(segment2Ds_Perp1);
            Assert.Single(segment2Ds_Perp1);
            Segment2D segment2D_Perp1 = segment2Ds_Perp1[0];
            Assert.Equal(10.0, segment2D_Perp1.Length, 6); // Length is 2 * A = 2 * 5 = 10

            // Case 5: Perpendicular Plane Projection (Edge-on along Minor Axis)
            Plane plane_Perpendicular2 = Spatial.Constants.Plane.WorldX; // Normal is WorldX, ellipse has major axis along WorldX
            ProjectionResult? projectionResult_Perp2 = Create.ProjectionResult(plane_Perpendicular2, ellipse3D_Valid);
            Assert.NotNull(projectionResult_Perp2);
            List<Segment2D>? segment2Ds_Perp2 = projectionResult_Perp2!.GetGeometry2Ds<Segment2D>();
            Assert.NotNull(segment2Ds_Perp2);
            Assert.Single(segment2Ds_Perp2);
            Segment2D segment2D_Perp2 = segment2Ds_Perp2[0];
            Assert.Equal(6.0, segment2D_Perp2.Length, 6); // Length is 2 * B = 2 * 3 = 6

            // Case 6: Tilted Plane Projection
            Spatial.Classes.Vector3D vector3D_TiltedNormal = new(1.0 / System.Math.Sqrt(2), 0, 1.0 / System.Math.Sqrt(2));
            Plane plane_Tilted = new(Spatial.Constants.Point3D.Zero, vector3D_TiltedNormal);
            ProjectionResult? projectionResult_Tilted = Create.ProjectionResult(plane_Tilted, ellipse3D_Valid);
            Assert.NotNull(projectionResult_Tilted);
            List<Ellipse2D>? ellipse2Ds_Tilted = projectionResult_Tilted!.GetGeometry2Ds<Ellipse2D>();
            Assert.NotNull(ellipse2Ds_Tilted);
            Assert.Single(ellipse2Ds_Tilted);
            Ellipse2D ellipse2D_Tilted = ellipse2Ds_Tilted[0];
            Assert.Equal(5.0 / System.Math.Sqrt(2), ellipse2D_Tilted.A, 6);
            Assert.Equal(3.0, ellipse2D_Tilted.B, 6);

            // Case 7: Degenerate Ellipse (Zero Size)
            Ellipse3D ellipse3D_Zero = new(Spatial.Constants.Plane.WorldZ, new Ellipse2D(new Point2D(1, 2), 0.0, 0.0));
            ProjectionResult? projectionResult_Zero = Create.ProjectionResult(plane_Valid, ellipse3D_Zero);
            Assert.NotNull(projectionResult_Zero);
            List<Point2D>? point2Ds_Zero = projectionResult_Zero!.GetGeometry2Ds<Point2D>();
            Assert.NotNull(point2Ds_Zero);
            Assert.Single(point2Ds_Zero);
            Point2D point2D_Zero = point2Ds_Zero[0];
            Assert.Equal(1.0, point2D_Zero.X, 6);
            Assert.Equal(2.0, point2D_Zero.Y, 6);

            // Case 8: Degenerate Ellipse (Segment-like)
            Ellipse3D ellipse3D_Segment = new(Spatial.Constants.Plane.WorldZ, new Ellipse2D(new Point2D(0, 0), 5.0, 0.0));
            ProjectionResult? projectionResult_Segment = Create.ProjectionResult(plane_Valid, ellipse3D_Segment);
            Assert.NotNull(projectionResult_Segment);
            List<Segment2D>? segment2Ds_Segment = projectionResult_Segment!.GetGeometry2Ds<Segment2D>();
            Assert.NotNull(segment2Ds_Segment);
            Assert.Single(segment2Ds_Segment);
            Segment2D segment2D_Segment = segment2Ds_Segment[0];
            Assert.Equal(10.0, segment2D_Segment.Length, 6);

            // Case 9: Degenerate Projection (Tolerance-based Segment)
            Spatial.Classes.Vector3D vector3D_NearPerpNormal = new(System.Math.Cos(0.001 * System.Math.PI / 180.0), 0, System.Math.Sin(0.001 * System.Math.PI / 180.0));
            Plane plane_NearPerp = new(Spatial.Constants.Point3D.Zero, vector3D_NearPerpNormal);
            ProjectionResult? projectionResult_NearPerp = Create.ProjectionResult(plane_NearPerp, ellipse3D_Valid, 0.01);
            Assert.NotNull(projectionResult_NearPerp);
            List<Segment2D>? segment2Ds_NearPerp = projectionResult_NearPerp!.GetGeometry2Ds<Segment2D>();
            Assert.NotNull(segment2Ds_NearPerp);
            Assert.Single(segment2Ds_NearPerp);

            // Case 10: Degenerate Projection (Tolerance-based Point)
            Ellipse3D ellipse3D_Tiny = new(Spatial.Constants.Plane.WorldZ, new Ellipse2D(new Point2D(0, 0), 0.0001, 0.0001));
            ProjectionResult? projectionResult_Tiny = Create.ProjectionResult(plane_Valid, ellipse3D_Tiny, 0.01);
            Assert.NotNull(projectionResult_Tiny);
            List<Point2D>? point2Ds_Tiny = projectionResult_Tiny!.GetGeometry2Ds<Point2D>();
            Assert.NotNull(point2Ds_Tiny);
            Assert.Single(point2Ds_Tiny);
        }
    }
}