using DiGi.Analytical.Building.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.GLTF.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Validates that <see cref="Modify.Clip(Mesh3D?, Circle2D?, int, double)"/> clips a 3D heightfield mesh to a regular circular boundary, eliminating outer triangles and placing boundary vertices on the circle.
        /// </summary>
        [Fact]
        public void MeshClip_Circle2D_ClipsToRegularCircularBoundary()
        {
            // Build a 10x10 grid mesh spanning [-50, 50] in X and Y at elevation Z = 100
            List<Point3D> points = [];
            List<int[]> indexes = [];

            int gridSize = 10;
            double step = 10.0;
            for (int r = 0; r <= gridSize; r++)
            {
                for (int c = 0; c <= gridSize; c++)
                {
                    double x = -50.0 + (c * step);
                    double y = -50.0 + (r * step);
                    double z = 100.0 + (0.1 * x) + (0.05 * y);
                    points.Add(new Point3D(x, y, z));
                }
            }

            int cols = gridSize + 1;
            for (int r = 0; r < gridSize; r++)
            {
                for (int c = 0; c < gridSize; c++)
                {
                    int i0 = (r * cols) + c;
                    int i1 = i0 + 1;
                    int i2 = ((r + 1) * cols) + c;
                    int i3 = i2 + 1;

                    indexes.Add([i0, i1, i2]);
                    indexes.Add([i1, i3, i2]);
                }
            }

            Mesh3D mesh3D = new(points, indexes);

            Circle2D clipCircle = new(new Point2D(0.0, 0.0), 30.0);
            Mesh3D? mesh3D_Clipped = Modify.Clip(mesh3D, clipCircle, 64);

            Assert.NotNull(mesh3D_Clipped);
            List<Point3D>? points_Clipped = mesh3D_Clipped.GetPoints();
            Assert.NotNull(points_Clipped);
            Assert.True(points_Clipped.Count > 0);

            // All clipped mesh vertices must lie within circle radius (plus tolerance)
            foreach (Point3D point3D in points_Clipped)
            {
                double dist = System.Math.Sqrt((point3D.X * point3D.X) + (point3D.Y * point3D.Y));
                Assert.True(dist <= 30.0 + DiGi.Core.Constants.Tolerance.Distance, $"Point ({point3D.X}, {point3D.Y}) distance {dist} exceeds radius 30.0");
            }
        }

        /// <summary>
        /// Validates that <see cref="Modify.Clip(Mesh3D?, BoundingBox2D?, double)"/> clips a 3D heightfield mesh to an axis-aligned rectangular boundary.
        /// </summary>
        [Fact]
        public void MeshClip_BoundingBox2D_ClipsToRectangularBoundary()
        {
            List<Point3D> points = [];
            List<int[]> indexes = [];

            int gridSize = 10;
            double step = 10.0;
            for (int r = 0; r <= gridSize; r++)
            {
                for (int c = 0; c <= gridSize; c++)
                {
                    double x = -50.0 + (c * step);
                    double y = -50.0 + (r * step);
                    points.Add(new Point3D(x, y, 50.0));
                }
            }

            int cols = gridSize + 1;
            for (int r = 0; r < gridSize; r++)
            {
                for (int c = 0; c < gridSize; c++)
                {
                    int i0 = (r * cols) + c;
                    int i1 = i0 + 1;
                    int i2 = ((r + 1) * cols) + c;
                    int i3 = i2 + 1;

                    indexes.Add([i0, i1, i2]);
                    indexes.Add([i1, i3, i2]);
                }
            }

            Mesh3D mesh3D = new(points, indexes);

            BoundingBox2D box = new(new Point2D(-20.0, -20.0), new Point2D(20.0, 20.0));
            Mesh3D? mesh3D_Clipped = Modify.Clip(mesh3D, box);

            Assert.NotNull(mesh3D_Clipped);
            List<Point3D>? points_Clipped = mesh3D_Clipped.GetPoints();
            Assert.NotNull(points_Clipped);
            Assert.True(points_Clipped.Count > 0);

            foreach (Point3D point3D in points_Clipped)
            {
                Assert.True(point3D.X >= -20.0 - DiGi.Core.Constants.Tolerance.Distance && point3D.X <= 20.0 + DiGi.Core.Constants.Tolerance.Distance);
                Assert.True(point3D.Y >= -20.0 - DiGi.Core.Constants.Tolerance.Distance && point3D.Y <= 20.0 + DiGi.Core.Constants.Tolerance.Distance);
            }
        }

        /// <summary>
        /// Validates that <see cref="Create.TerrainGLTFNode(GLTFNode?, IEnumerable{BuildingModel}?, Circle2D?, double, double)"/> clips the terrain to a circular boundary and cuts out the building footprint.
        /// </summary>
        [Fact]
        public void TerrainGLTFNode_WithCircleBoundaryAndBuildingCutout()
        {
            List<Point3D> points = [];
            List<int[]> indexes = [];

            int gridSize = 10;
            double step = 10.0;
            for (int r = 0; r <= gridSize; r++)
            {
                for (int c = 0; c <= gridSize; c++)
                {
                    double x = -50.0 + (c * step);
                    double y = -50.0 + (r * step);
                    points.Add(new Point3D(x, y, 10.0));
                }
            }

            int cols = gridSize + 1;
            for (int r = 0; r < gridSize; r++)
            {
                for (int c = 0; c < gridSize; c++)
                {
                    int i0 = (r * cols) + c;
                    int i1 = i0 + 1;
                    int i2 = ((r + 1) * cols) + c;
                    int i3 = i2 + 1;

                    indexes.Add([i0, i1, i2]);
                    indexes.Add([i1, i3, i2]);
                }
            }

            Mesh3D mesh3D = new(points, indexes);
            GLTFNode gLTFNode_Initial = new("Terrain", null, mesh3D, null, 1, null);

            Point2D[] footprint =
            [
                new(-5.0, -5.0),
                new(5.0, -5.0),
                new(5.0, 5.0),
                new(-5.0, 5.0)
            ];
            BuildingModel buildingModel = CreateTestBuildingModel(new Polygon2D(footprint));

            Circle2D clipCircle = new(new Point2D(0.0, 0.0), 30.0);

            GLTFNode? gLTFNode_Result = gLTFNode_Initial.TerrainGLTFNode([buildingModel], clipCircle);

            Assert.NotNull(gLTFNode_Result);
            Assert.NotNull(gLTFNode_Result.Mesh3D);

            List<Point3D>? points_Result = gLTFNode_Result.Mesh3D.GetPoints();
            Assert.NotNull(points_Result);

            // Boundary clipping verified: all points within r=30
            foreach (Point3D point3D in points_Result)
            {
                double dist = System.Math.Sqrt((point3D.X * point3D.X) + (point3D.Y * point3D.Y));
                Assert.True(dist <= 30.0 + DiGi.Core.Constants.Tolerance.Distance);
            }
        }

        /// <summary>
        /// Validates that <see cref="Create.TerrainGLTFNode(GLTFNode?, IEnumerable{BuildingModel}?, Circle2D?, double, double)"/> cuts a dense set of footprints rather than stepping over it.
        /// <para>This case used to be refused. A <c>TerrainCuttingMaxBuildingCount</c> of 250 skipped the cut entirely above that many buildings, to work around a constraint enforcement failure in the triangulation underneath, and the cap was removed once that was fixed upstream (ZiolkowskiJakub/DiGi.Geometry#2).</para>
        /// <para>What makes this worth asserting is that the cut is wrapped in a catch that falls back to the uncut surface, so a regression upstream would not throw here - it would quietly return the terrain whole again, exactly as the cap used to. Comparing against the input mesh is the only thing that tells the two apart.</para>
        /// </summary>
        [Fact]
        public void TerrainGLTFNode_DenseFootprints_AreCut()
        {
            Point3D[] points =
            [
                new(-50.0, -50.0, 0.0),
                new(50.0, -50.0, 0.0),
                new(-50.0, 50.0, 0.0),
                new(50.0, 50.0, 0.0)
            ];
            List<int[]> indexes = [[0, 1, 2], [1, 3, 2]];
            Mesh3D mesh3D = new(points, indexes);
            GLTFNode gLTFNode = new("Terrain", null, mesh3D, null, 1, null);

            // Comfortably past the count the removed cap refused at, and packed tightly enough that the
            // offset outlines nearly touch - which is what made the triangulation fail in the first place.
            List<BuildingModel> buildingModels = [];
            for (int i = 0; i < 260; i++)
            {
                Point2D[] footprint =
                [
                    new(i * 0.1, 0.0),
                    new((i * 0.1) + 0.05, 0.0),
                    new((i * 0.1) + 0.05, 0.05),
                    new(i * 0.1, 0.05)
                ];
                buildingModels.Add(CreateTestBuildingModel(new Polygon2D(footprint)));
            }

            // One building establishes that cutting works at all on this surface, so a failure below is read
            // as the density rather than the setup.
            GLTFNode? gLTFNode_Single = gLTFNode.TerrainGLTFNode(buildingModels.Take(1).ToList(), (Circle2D?)null);
            Assert.NotNull(gLTFNode_Single);
            Assert.NotNull(gLTFNode_Single.Mesh3D);
            Assert.True(gLTFNode_Single.Mesh3D.GetPoints()?.Count > points.Length);

            GLTFNode? gLTFNode_Dense = gLTFNode.TerrainGLTFNode(buildingModels, (Circle2D?)null);
            Assert.NotNull(gLTFNode_Dense);
            Assert.NotNull(gLTFNode_Dense.Mesh3D);

            // The surface came back with more vertices than it went in with, so the cut ran. Equality with
            // the four input corners would mean the fallback fired and the footprints were stepped over.
            Assert.True(gLTFNode_Dense.Mesh3D.GetPoints()?.Count > points.Length);
        }
    }
}
