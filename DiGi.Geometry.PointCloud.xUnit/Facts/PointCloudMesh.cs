using DiGi.Geometry.PointCloud.Core.Enums;
using DiGi.Geometry.PointCloud.Planar.Classes;
using DiGi.Geometry.PointCloud.Spatial.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Geometry.PointCloud.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a planar cloud triangulates into a mesh whose vertices are the cloud's own points and whose triangles cover the area the points span.
        /// </summary>
        [Fact]
        public void PointCloudMesh_Delaunay()
        {
            Random random = new(12345);

            int count = 2000;

            double[] x = new double[count];
            double[] y = new double[count];

            for (int i = 0; i < count; i++)
            {
                x[i] = random.NextDouble() * 100.0;
                y[i] = random.NextDouble() * 100.0;
            }

            PointCloud2D pointCloud2D = new(x, y);

            DelaunayPointCloud2DMeshSolver delaunayPointCloud2DMeshSolver = new();

            Mesh2D? mesh2D = Planar.Create.Mesh2D(pointCloud2D, delaunayPointCloud2DMeshSolver);

            Assert.NotNull(mesh2D);
            Assert.Equal(count, mesh2D.PointsCount);
            Assert.True(mesh2D.TrianglesCount > 0);

            List<int[]>? indexes = mesh2D.GetIndexes();

            Assert.NotNull(indexes);
            foreach (int[] indexes_Triangle in indexes)
            {
                Assert.All(indexes_Triangle, index => Assert.InRange(index, 0, count - 1));
            }

            // A Delaunay triangulation covers the convex hull of its sites, so the total area must be
            // close to the area of the square the points were drawn from.
            Assert.True(mesh2D.GetArea() > 9000.0, $"Triangulated area was {mesh2D.GetArea()}.");

            Assert.Null(Planar.Create.Mesh2D(pointCloud2D, null));
            Assert.Null(Planar.Create.Mesh2D(null, delaunayPointCloud2DMeshSolver));
        }

        /// <summary>
        /// Tests that a cloud sampled from an inclined plane reconstructs into a height field mesh that lies on that plane.
        /// <para>Every reconstructed vertex must satisfy the original plane equation, which verifies that each vertex keeps the height of the point it came from rather than a height recovered from the triangulator.</para>
        /// </summary>
        [Fact]
        public void PointCloudMesh_HeightField()
        {
            Random random = new(12345);

            int count = 20000;

            double[] x = new double[count];
            double[] y = new double[count];
            double[] z = new double[count];

            for (int i = 0; i < count; i++)
            {
                x[i] = random.NextDouble() * 100.0;
                y[i] = random.NextDouble() * 100.0;
                z[i] = 10.0 + (0.25 * x[i]) - (0.5 * y[i]);
            }

            PointCloud3D pointCloud3D = new(x, y, z);

            HeightFieldPointCloud3DMeshSolver heightFieldPointCloud3DMeshSolver = new(2.0, 0, PointCloudHeightSelection.Lowest);

            Mesh3D? mesh3D = Spatial.Create.Mesh3D(pointCloud3D, heightFieldPointCloud3DMeshSolver);

            Assert.NotNull(mesh3D);
            Assert.True(mesh3D.TrianglesCount > 0);

            // Decimation onto a two unit grid over a hundred unit square must leave far fewer points than the input.
            Assert.True(mesh3D.PointsCount < count, $"Decimation kept {mesh3D.PointsCount} of {count} points.");

            List<Point3D>? point3Ds = mesh3D.GetPoints();

            Assert.NotNull(point3Ds);
            foreach (Point3D point3D in point3Ds)
            {
                Assert.Equal(10.0 + (0.25 * point3D.X) - (0.5 * point3D.Y), point3D.Z, 9);
            }

            BoundingBox3D? boundingBox3D = mesh3D.GetBoundingBox();

            Assert.NotNull(boundingBox3D);
            Assert.True(boundingBox3D.MaxX - boundingBox3D.MinX > 90.0);
        }

        /// <summary>
        /// Tests that the maximum edge length filter removes the triangles that bridge a hole in the data.
        /// <para>A Delaunay triangulation always spans the convex hull, so a cloud shaped like an annulus comes back with its centre filled in unless the long bridging triangles are discarded.</para>
        /// </summary>
        [Fact]
        public void PointCloudMesh_MaximumEdgeLength()
        {
            Random random = new(12345);

            List<double> x_Values = [];
            List<double> y_Values = [];

            // An annulus: points between radius forty and fifty, leaving a large empty middle.
            for (int i = 0; i < 20000; i++)
            {
                double angle = random.NextDouble() * System.Math.PI * 2.0;
                double radius = 40.0 + (random.NextDouble() * 10.0);

                x_Values.Add(radius * System.Math.Cos(angle));
                y_Values.Add(radius * System.Math.Sin(angle));
            }

            PointCloud2D pointCloud2D = new([.. x_Values], [.. y_Values]);

            Mesh2D? mesh2D_Unfiltered = Planar.Create.Mesh2D(pointCloud2D, new DelaunayPointCloud2DMeshSolver(1.0));
            Mesh2D? mesh2D_Filtered = Planar.Create.Mesh2D(pointCloud2D, new DelaunayPointCloud2DMeshSolver(1.0, 5.0));

            Assert.NotNull(mesh2D_Unfiltered);
            Assert.NotNull(mesh2D_Filtered);

            double area_Unfiltered = mesh2D_Unfiltered.GetArea();
            double area_Filtered = mesh2D_Filtered.GetArea();

            // Unfiltered spans the whole disc; filtered keeps only the ring itself.
            double area_Disc = System.Math.PI * 50.0 * 50.0;
            double area_Ring = System.Math.PI * ((50.0 * 50.0) - (40.0 * 40.0));

            Assert.True(area_Unfiltered > area_Disc * 0.9, $"Unfiltered area was {area_Unfiltered}, expected near {area_Disc}.");
            Assert.True(area_Filtered < area_Ring * 1.1, $"Filtered area was {area_Filtered}, expected near {area_Ring}.");
            Assert.True(area_Filtered > area_Ring * 0.8, $"Filtered area was {area_Filtered}, expected near {area_Ring}.");
        }

        /// <summary>
        /// Tests that a cloud sampled from a sphere reconstructs into a closed isosurface enclosing that sphere.
        /// <para>The mesh is asserted to be watertight, which is the property the tetrahedral decomposition exists to guarantee, and its area is asserted to be roughly twice the area of the sampled sphere: without surface normals the field cannot distinguish inside from outside, so the surface wraps the shell of points on both sides. That doubling is inherent, not a defect, and is asserted here so that it stays visible.</para>
        /// </summary>
        [Fact]
        public void PointCloudMesh_Isosurface()
        {
            Random random = new(12345);

            int count = 200000;
            double radius = 20.0;

            double[] x = new double[count];
            double[] y = new double[count];
            double[] z = new double[count];

            for (int i = 0; i < count; i++)
            {
                double u = (random.NextDouble() * 2.0) - 1.0;
                double angle = random.NextDouble() * System.Math.PI * 2.0;
                double scale = System.Math.Sqrt(1.0 - (u * u));

                x[i] = radius * scale * System.Math.Cos(angle);
                y[i] = radius * scale * System.Math.Sin(angle);
                z[i] = radius * u;
            }

            PointCloud3D pointCloud3D = new(x, y, z);

            IsosurfacePointCloud3DMeshSolver isosurfacePointCloud3DMeshSolver = new(1.5, 0.5, 1);

            Mesh3D? mesh3D = Spatial.Create.Mesh3D(pointCloud3D, isosurfacePointCloud3DMeshSolver);

            Assert.NotNull(mesh3D);
            Assert.True(mesh3D.TrianglesCount > 0);

            Assert.True(mesh3D.IsClosed(), "The extracted isosurface was not watertight.");

            List<int[]>? indexes = mesh3D.GetIndexes();

            Assert.NotNull(indexes);
            foreach (int[] indexes_Triangle in indexes)
            {
                Assert.All(indexes_Triangle, index => Assert.InRange(index, 0, mesh3D.PointsCount - 1));
            }

            BoundingBox3D? boundingBox3D = mesh3D.GetBoundingBox();

            Assert.NotNull(boundingBox3D);
            Assert.InRange(boundingBox3D.MaxX, radius - 3.0, radius + 3.0);
            Assert.InRange(boundingBox3D.MinZ, -radius - 3.0, -radius + 3.0);

            double area_Sphere = 4.0 * System.Math.PI * radius * radius;

            Assert.InRange(mesh3D.GetArea(), area_Sphere * 1.4, area_Sphere * 2.8);

            Assert.Null(Spatial.Create.Mesh3D(pointCloud3D, null));
        }
    }
}
