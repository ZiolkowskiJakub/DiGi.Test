namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the Distance and Transform methods of the Segment3D class, verifying translation, rotation, and distance calculations under various conditions.
        /// </summary>
        [Fact]
        public void Segment3D()
        {
            // Setup a segment from (0, 0, 0) to (0, 0, 10)
            Spatial.Classes.Point3D point3D_Start = new(0.0, 0.0, 0.0);
            Spatial.Classes.Point3D point3D_End = new(0.0, 0.0, 10.0);
            Spatial.Classes.Segment3D segment3D_Line = new(point3D_Start, point3D_End);

            // 1. Test Distance method
            // Point exactly on the segment
            Spatial.Classes.Point3D point3D_On = new(0.0, 0.0, 5.0);
            double distance_On = segment3D_Line.Distance(point3D_On);
            Assert.Equal(0.0, distance_On, 9);

            // Point perpendicular to the segment (in-range)
            Spatial.Classes.Point3D point3D_Perp = new(3.0, 4.0, 5.0); // distance should be 5.0 (3-4-5 triangle in XY plane)
            double distance_Perp = segment3D_Line.Distance(point3D_Perp);
            Assert.Equal(5.0, distance_Perp, 9);

            // Point beyond the end of the segment (out-of-range, closest to end)
            Spatial.Classes.Point3D point3D_BeyondEnd = new(0.0, 3.0, 14.0); // distance should be sqrt(3^2 + 4^2) = 5.0 to (0,0,10)
            double distance_BeyondEnd = segment3D_Line.Distance(point3D_BeyondEnd);
            Assert.Equal(5.0, distance_BeyondEnd, 9);

            // Point before the start of the segment (out-of-range, closest to start)
            Spatial.Classes.Point3D point3D_BeforeStart = new(4.0, 0.0, -3.0); // distance should be 5.0 to (0,0,0)
            double distance_BeforeStart = segment3D_Line.Distance(point3D_BeforeStart);
            Assert.Equal(5.0, distance_BeforeStart, 9);

            // Null input
            double distance_Null = segment3D_Line.Distance(null);
            Assert.True(double.IsNaN(distance_Null));

            // 2. Test Transform method
            // Translate segment by (2, 3, 4)
            Spatial.Classes.Transform3D? transform3D_Translation = Spatial.Create.Transform3D.Translation(new Spatial.Classes.Vector3D(2.0, 3.0, 4.0));
            Assert.NotNull(transform3D_Translation);
            if (transform3D_Translation is null)
            {
                return;
            }

            bool success_Translate = segment3D_Line.Transform(transform3D_Translation);
            Assert.True(success_Translate);

            Spatial.Classes.Point3D? point3D_TransformedStart = segment3D_Line.Start;
            Spatial.Classes.Point3D? point3D_TransformedEnd = segment3D_Line.End;

            Assert.NotNull(point3D_TransformedStart);
            Assert.NotNull(point3D_TransformedEnd);
            if (point3D_TransformedStart is null || point3D_TransformedEnd is null)
            {
                return;
            }

            // Expected start: (2, 3, 4), expected end: (2, 3, 14)
            Assert.Equal(2.0, point3D_TransformedStart.X, 9);
            Assert.Equal(3.0, point3D_TransformedStart.Y, 9);
            Assert.Equal(4.0, point3D_TransformedStart.Z, 9);

            Assert.Equal(2.0, point3D_TransformedEnd.X, 9);
            Assert.Equal(3.0, point3D_TransformedEnd.Y, 9);
            Assert.Equal(14.0, point3D_TransformedEnd.Z, 9);
        }

        /// <summary>
        /// Tests the Inverse method of the Segment3D class, verifying that reversing the segment correctly swaps its start and end points and negates its direction without translating its physical position.
        /// </summary>
        [Fact]
        public void Segment3D_Inverse()
        {
            DiGi.Geometry.Spatial.Classes.Point3D point3D_Start = new(0.0, 0.0, 0.0);
            DiGi.Geometry.Spatial.Classes.Point3D point3D_End = new(0.0, 0.0, 10.0);
            DiGi.Geometry.Spatial.Classes.Segment3D segment3D_Line = new(point3D_Start, point3D_End);

            // Verify initial state
            Assert.NotNull(segment3D_Line.Start);
            Assert.NotNull(segment3D_Line.End);
            Assert.Equal(0.0, segment3D_Line.Start.Z, 9);
            Assert.Equal(10.0, segment3D_Line.End.Z, 9);

            // Invert segment
            bool success = segment3D_Line.Inverse();
            Assert.True(success);

            // Verify inverted state
            Assert.NotNull(segment3D_Line.Start);
            Assert.NotNull(segment3D_Line.End);
            Assert.Equal(10.0, segment3D_Line.Start.Z, 9);
            Assert.Equal(0.0, segment3D_Line.End.Z, 9);
            Assert.NotNull(segment3D_Line.Direction);
            Assert.Equal(-1.0, segment3D_Line.Direction.Z, 9);
        }

        /// <summary>
        /// Tests the state safety of a Segment3D object when a coordinate transformation fails.
        /// </summary>
        [Fact]
        public void Segment3D_TransformFailure()
        {
            DiGi.Geometry.Spatial.Classes.Point3D point3D_Start = new(1.0, 2.0, 3.0);
            DiGi.Geometry.Spatial.Classes.Point3D point3D_End = new(4.0, 5.0, 6.0);
            DiGi.Geometry.Spatial.Classes.Segment3D segment3D_Line = new(point3D_Start, point3D_End);

            DiGi.Geometry.Spatial.Classes.Transform3D transform3D_Invalid = new((DiGi.Math.Classes.Matrix4D?)null);
            bool bool_Result = segment3D_Line.Transform(transform3D_Invalid);

            Assert.False(bool_Result);
            Assert.NotNull(segment3D_Line.Start);
            Assert.NotNull(segment3D_Line.End);
            Assert.Equal(1.0, segment3D_Line.Start.X, 9);
            Assert.Equal(2.0, segment3D_Line.Start.Y, 9);
            Assert.Equal(3.0, segment3D_Line.Start.Z, 9);
            Assert.Equal(4.0, segment3D_Line.End.X, 9);
            Assert.Equal(5.0, segment3D_Line.End.Y, 9);
            Assert.Equal(6.0, segment3D_Line.End.Z, 9);
        }
    }
}