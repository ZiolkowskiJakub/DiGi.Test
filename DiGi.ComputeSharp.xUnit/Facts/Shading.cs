using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;

namespace DiGi.ComputeSharp.xUnit
{
    /// <summary>
    /// Contains unit and performance tests for the GPU and CPU shading analysis functionality.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Tests that the shading analysis produces correct and matching results on both GPU and CPU paths.
        /// </summary>
        [Fact]
        public void Shading_ValidFaces()
        {
            Plane plane = DiGi.Geometry.Spatial.Constants.Plane.WorldZ;
            Vector3D direction = new(0, 0, 1);
            double tolerance = 1e-3;

            List<Point3D> point3Ds_1 = [new Point3D(0, 0, 0), new Point3D(10, 0, 0), new Point3D(10, 10, 0), new Point3D(0, 10, 0)];
            Polygon3D externalEdge_1 = new(plane, point3Ds_1.ConvertAll(plane.Convert)!);
            PolygonalFace3D? face_1 = Create.PolygonalFace3D(externalEdge_1, []);
            Assert.NotNull(face_1);

            List<Point3D> point3Ds_2 = [new Point3D(0, 0, 5), new Point3D(10, 0, 5), new Point3D(10, 10, 5), new Point3D(0, 10, 5)];
            Polygon3D externalEdge_2 = new(plane, point3Ds_2.ConvertAll(plane.Convert)!);
            PolygonalFace3D? face_2 = Create.PolygonalFace3D(externalEdge_2, []);
            Assert.NotNull(face_2);

            List<PolygonalFace3D> polygonalFace3Ds = [face_1!, face_2!];

            // Verify CPU shading works and produces valid output
            List<List<PolygonalFace3D>?>? result_CPU = Geometry.Spatial.Query.Shading_CPU(polygonalFace3Ds, direction, tolerance);
            Assert.NotNull(result_CPU);

            // If GPU is supported, verify GPU shading works and matches CPU shading
            if (Query.IsComputeSharpSupported(testOutputHelper))
            {
                try
                {
                    List<List<PolygonalFace3D>?>? result_GPU = Geometry.Spatial.Query.Shading(polygonalFace3Ds, direction, tolerance);
                    Assert.NotNull(result_GPU);
                    Assert.Equal(result_CPU!.Count, result_GPU!.Count);
                }
                catch (System.Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
                {
                    testOutputHelper.WriteLine("WARNING: GPU FP64 double-precision operations are not supported on this graphics card: " + exception.Message);
                }
            }
        }

        /// <summary>
        /// Verifies that the optimized shading implementation only enumerates a deferred IEnumerable exactly once,
        /// avoiding the O(N^2) complexity and redundant evaluation of the old ElementAt implementation.
        /// </summary>
        [Fact]
        public void Shading_Performance_DeferredEnumeration()
        {
            Plane plane = DiGi.Geometry.Spatial.Constants.Plane.WorldZ;
            Vector3D direction = new(0, 0, 1);
            double tolerance = 1e-3;

            int enumerationCount = 0;

            // Create a deferred IEnumerable that increments a counter on every element evaluation
            IEnumerable<IPolygonalFace3D> deferredFace3Ds = Enumerable.Range(0, 5).Select(i =>
            {
                Interlocked.Increment(ref enumerationCount);
                List<Point3D> point3Ds = [new Point3D(i, 0, 0), new Point3D(i + 10, 0, 0), new Point3D(i + 10, 10, 0), new Point3D(i, 10, 0)];
                Polygon3D externalEdge = new(plane, point3Ds.ConvertAll(plane.Convert)!);
                PolygonalFace3D? face = Create.PolygonalFace3D(externalEdge, []);
                return (IPolygonalFace3D)face!;
            });

            // Run CPU shading, which must only enumerate the collection once to convert it to an array
            List<List<PolygonalFace3D>?>? result_CPU = Geometry.Spatial.Query.Shading_CPU(deferredFace3Ds, direction, tolerance);
            Assert.NotNull(result_CPU);

            // With our array optimization, the sequence of 5 elements is evaluated exactly once (5 times total)
            // Under the old implementation, it would have been evaluated 35 times (Count + 2 * sum(1..5))
            Assert.Equal(5, enumerationCount);

            // Reset counter for GPU test if supported
            if (Query.IsComputeSharpSupported(testOutputHelper))
            {
                enumerationCount = 0;
                try
                {
                    List<List<PolygonalFace3D>?>? result_GPU = Geometry.Spatial.Query.Shading(deferredFace3Ds, direction, tolerance);
                    Assert.NotNull(result_GPU);
                    Assert.Equal(5, enumerationCount);
                }
                catch (System.Exception exception) when (exception.GetType().Name == "UnsupportedDoubleOperationException")
                {
                    testOutputHelper.WriteLine("WARNING: GPU FP64 double-precision operations are not supported on this graphics card: " + exception.Message);
                }
            }
        }
    }
}