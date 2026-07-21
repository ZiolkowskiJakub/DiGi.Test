using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;
using System.Diagnostics;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the generation of a <see cref="Mesh3D"/> from an <see cref="Ellipsoid"/>.
        /// <para>Verifies the exact vertex and triangle counts for the requested resolution, watertightness of the generated mesh, rejection of degenerate resolutions, and the JSON serialization round-trip.</para>
        /// </summary>
        [Fact]
        public void Mesh3D_Ellipsoid()
        {
            Ellipsoid ellipsoid = new(new Point3D(1, 2, 3), 1, 2, 3);

            Mesh3D? mesh3D = ellipsoid.Mesh3D(8, 12);
            Assert.NotNull(mesh3D);
            Assert.Equal(((8 - 1) * 12) + 2, mesh3D.PointsCount);
            Assert.Equal(2 * 12 * (8 - 1), mesh3D.TrianglesCount);
            Assert.True(mesh3D.IsClosed());

            List<Polyloop>? polyloops = mesh3D.GetBoundaryEdges();
            Assert.NotNull(polyloops);
            Assert.Empty(polyloops);

            Assert.Null(ellipsoid.Mesh3D(1, 12));
            Assert.Null(ellipsoid.Mesh3D(8, 2));
            Assert.Null(((Ellipsoid?)null).Mesh3D(8, 12));
            Assert.Null(ellipsoid.Mesh3D(0.0));

            Mesh3D? mesh3D_AngleFactor = ellipsoid.Mesh3D(System.Math.PI / 8);
            Assert.NotNull(mesh3D_AngleFactor);
            Assert.True(mesh3D_AngleFactor.IsClosed());

            DiGi.Core.xUnit.Query.SerializationCheck(mesh3D);
        }

        /// <summary>
        /// Tests the <see cref="DiGi.Geometry.Core.Classes.Mesh{TPoint}.IsClosed"/> topology check.
        /// <para>Verifies that a watertight mesh is reported as closed, that removing a single triangle makes it open, and that meshes below the minimum triangle count are never closed.</para>
        /// </summary>
        [Fact]
        public void Mesh3D_IsClosed()
        {
            Ellipsoid ellipsoid = new(new Point3D(0, 0, 0), 2, 2, 2);

            Mesh3D? mesh3D = ellipsoid.Mesh3D(6, 8);
            Assert.NotNull(mesh3D);
            Assert.True(mesh3D.IsClosed());

            List<Point3D>? point3Ds = mesh3D.GetPoints();
            Assert.NotNull(point3Ds);

            List<int[]>? indexes = mesh3D.GetIndexes();
            Assert.NotNull(indexes);
            indexes.RemoveAt(indexes.Count - 1);

            Mesh3D mesh3D_Open = new(point3Ds, indexes);
            Assert.False(mesh3D_Open.IsClosed());

            List<Polyloop>? polyloops = mesh3D_Open.GetBoundaryEdges();
            Assert.NotNull(polyloops);
            Assert.Single(polyloops);

            Mesh3D mesh3D_Triangle = new([new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0)], [new int[] { 0, 1, 2 }]);
            Assert.False(mesh3D_Triangle.IsClosed());
        }

        /// <summary>
        /// Tests the <see cref="Mesh3D.GetArea"/> and <see cref="Mesh3D.GetVolume"/> calculations against analytic sphere and ellipsoid values.
        /// <para>A finely tessellated mesh must reproduce the analytic surface area and volume within one percent, and the mesh values must underestimate the analytic ones because the mesh is inscribed.</para>
        /// </summary>
        [Fact]
        public void Mesh3D_AreaVolume()
        {
            double radius = 2;
            Ellipsoid sphere = new(new Point3D(5, -3, 7), radius, radius, radius);

            Mesh3D? mesh3D = sphere.Mesh3D(64, 128);
            Assert.NotNull(mesh3D);
            Assert.True(mesh3D.IsClosed());

            double area = mesh3D.GetArea();
            double area_Analytic = 4 * System.Math.PI * radius * radius;
            Assert.True(area < area_Analytic);
            Assert.True(area > 0.99 * area_Analytic);

            double volume = mesh3D.GetVolume();
            double volume_Analytic = (4.0 / 3.0) * System.Math.PI * radius * radius * radius;
            Assert.True(volume < volume_Analytic);
            Assert.True(volume > 0.99 * volume_Analytic);

            Ellipsoid ellipsoid = new(new Point3D(0, 0, 0), 1, 2, 3);

            Mesh3D? mesh3D_Ellipsoid = ellipsoid.Mesh3D(64, 128);
            Assert.NotNull(mesh3D_Ellipsoid);

            double volume_Ellipsoid = mesh3D_Ellipsoid.GetVolume();
            double volume_Ellipsoid_Analytic = ellipsoid.GetVolume();
            Assert.True(volume_Ellipsoid < volume_Ellipsoid_Analytic);
            Assert.True(volume_Ellipsoid > 0.99 * volume_Ellipsoid_Analytic);
        }

        /// <summary>
        /// Tests the performance of the ellipsoid mesh generation, the closedness check, and the area and volume calculations on a large mesh.
        /// <para>After a warm-up call, generating a mesh with 79602 vertices and 159200 triangles plus running <see cref="DiGi.Geometry.Core.Classes.Mesh{TPoint}.IsClosed"/>, <see cref="Mesh3D.GetArea"/> and <see cref="Mesh3D.GetVolume"/> must complete within the stated threshold.</para>
        /// </summary>
        [Fact]
        public void Mesh3D_Performance()
        {
            Ellipsoid ellipsoid = new(new Point3D(1, 2, 3), 3, 2, 1);

            // Warm-up (JIT)
            Mesh3D? mesh3D_WarmUp = ellipsoid.Mesh3D(8, 12);
            Assert.NotNull(mesh3D_WarmUp);
            Assert.True(mesh3D_WarmUp.IsClosed());
            Assert.True(mesh3D_WarmUp.GetVolume() > 0);
            Assert.True(mesh3D_WarmUp.GetArea() > 0);

            Stopwatch stopwatch = Stopwatch.StartNew();

            Mesh3D? mesh3D = ellipsoid.Mesh3D(200, 400);
            Assert.NotNull(mesh3D);

            bool closed = mesh3D.IsClosed();
            double area = mesh3D.GetArea();
            double volume = mesh3D.GetVolume();

            stopwatch.Stop();

            Assert.Equal(((200 - 1) * 400) + 2, mesh3D.PointsCount);
            Assert.Equal(2 * 400 * (200 - 1), mesh3D.TrianglesCount);
            Assert.True(closed);
            Assert.True(area > 0);
            Assert.True(volume > 0);
            Assert.True(volume < ellipsoid.GetVolume());

            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Large mesh generation and evaluation took {stopwatch.ElapsedMilliseconds} ms, expected less than 500 ms.");
        }
    }
}