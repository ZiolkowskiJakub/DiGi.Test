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
        /// Tests that a point missing from the middle of a lattice does not open a hole, whatever the size test would say about the triangles that close over it.
        /// <para>This is the case that took a terrain mesh apart in production: a single node absent from a regular 100 m lattice forced its neighbours to span 200 m, every triangle that would have bridged it failed a fixed 150 m edge limit, and the surface came back with a diamond shaped hole around the gap.</para>
        /// <para>Both paths are run over the same cloud at the same threshold so the difference is the removal rule and nothing else. The fixed limit leaves more than one boundary loop - an outer one and the rim of the hole - while eroding from the boundary inwards leaves exactly one.</para>
        /// </summary>
        [Fact]
        public void PointCloudMeshErosion_MissingInteriorPoint()
        {
            double gridSize = 100.0;

            List<double> x_Values = [];
            List<double> y_Values = [];

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    // The centre of the lattice is the point the store was missing.
                    if (i == 2 && j == 2)
                    {
                        continue;
                    }

                    x_Values.Add(i * gridSize);
                    y_Values.Add(j * gridSize);
                }
            }

            Assert.Equal(24, x_Values.Count);

            PointCloud2D pointCloud2D = new([.. x_Values], [.. y_Values]);

            // A regular cell diagonal is 141.4, so 150 keeps every genuine triangle and only the 200 spans
            // across the missing node fail. That is exactly how the terrain endpoint was configured.
            Mesh2D? mesh2D_Absolute = Planar.Create.Mesh2D(pointCloud2D, new DelaunayPointCloud2DMeshSolver(0, 150.0));
            Mesh2D? mesh2D_Eroded = Planar.Create.Mesh2D(pointCloud2D, new DelaunayPointCloud2DMeshSolver(0, 0, edgeLengthFactor: 1.5));

            Assert.NotNull(mesh2D_Absolute);
            Assert.NotNull(mesh2D_Eroded);

            Assert.True(BoundaryLoopCount(mesh2D_Absolute.GetIndexes()) > 1, "A fixed edge limit was expected to open a hole around the missing node.");
            Assert.Equal(1, BoundaryLoopCount(mesh2D_Eroded.GetIndexes()));

            // Nothing on the outside of this cloud is oversized, so erosion has nothing to take and the
            // triangulation must come back whole.
            Mesh2D? mesh2D_Unfiltered = Planar.Create.Mesh2D(pointCloud2D, new DelaunayPointCloud2DMeshSolver());

            Assert.NotNull(mesh2D_Unfiltered);
            Assert.Equal(mesh2D_Unfiltered.TrianglesCount, mesh2D_Eroded.TrianglesCount);
            Assert.True(mesh2D_Absolute.TrianglesCount < mesh2D_Eroded.TrianglesCount);
        }

        /// <summary>
        /// Tests that a cloud sampled more finely in one area than another survives intact, because the size test is taken against local spacing rather than against a distance chosen in advance.
        /// <para>A fixed limit cannot serve both halves at once: set for the fine half it shreds the coarse one, set for the coarse half it stops filtering the fine one. The band where the two meet is the part most easily lost, so it is checked directly.</para>
        /// </summary>
        [Fact]
        public void PointCloudMeshErosion_VariableDensity()
        {
            List<double> x_Values = [];
            List<double> y_Values = [];

            // Coarse on the left at 100, fine on the right at 20, meeting at x = 500.
            for (double x = 0.0; x <= 500.0; x += 100.0)
            {
                for (double y = 0.0; y <= 500.0; y += 100.0)
                {
                    x_Values.Add(x);
                    y_Values.Add(y);
                }
            }

            for (double x = 520.0; x <= 1000.0; x += 20.0)
            {
                for (double y = 0.0; y <= 500.0; y += 20.0)
                {
                    x_Values.Add(x);
                    y_Values.Add(y);
                }
            }

            PointCloud2D pointCloud2D = new([.. x_Values], [.. y_Values]);

            Mesh2D? mesh2D_Unfiltered = Planar.Create.Mesh2D(pointCloud2D, new DelaunayPointCloud2DMeshSolver());
            Mesh2D? mesh2D_Eroded = Planar.Create.Mesh2D(pointCloud2D, new DelaunayPointCloud2DMeshSolver(0, 0, edgeLengthFactor: 2.5));

            Assert.NotNull(mesh2D_Unfiltered);
            Assert.NotNull(mesh2D_Eroded);

            Assert.Equal(1, BoundaryLoopCount(mesh2D_Eroded.GetIndexes()));

            // The transition band is judged by the coarse side, so nothing across it is lost.
            Assert.Equal(mesh2D_Unfiltered.TrianglesCount, mesh2D_Eroded.TrianglesCount);

            // A fixed limit set for the fine half destroys the coarse half instead.
            Mesh2D? mesh2D_Absolute = Planar.Create.Mesh2D(pointCloud2D, new DelaunayPointCloud2DMeshSolver(0, 30.0));

            Assert.NotNull(mesh2D_Absolute);
            Assert.True(mesh2D_Absolute.TrianglesCount < mesh2D_Eroded.TrianglesCount);
        }

        /// <summary>
        /// Tests that the empty space between two separated clusters is still cleared, and that clearing it separates the mesh rather than perforating it.
        /// <para>Erosion is what keeps an interior gap closed, so it has to be shown that it does not also keep genuine emptiness closed. Two clusters far apart are bridged by a Delaunay triangulation across the convex hull, and that bridge reaches the boundary, so it goes.</para>
        /// </summary>
        [Fact]
        public void PointCloudMeshErosion_SeparatedClusters()
        {
            List<double> x_Values = [];
            List<double> y_Values = [];

            foreach (double offset in new double[] { 0.0, 2000.0 })
            {
                for (double x = 0.0; x <= 300.0; x += 100.0)
                {
                    for (double y = 0.0; y <= 300.0; y += 100.0)
                    {
                        x_Values.Add(x + offset);
                        y_Values.Add(y);
                    }
                }
            }

            PointCloud2D pointCloud2D = new([.. x_Values], [.. y_Values]);

            Mesh2D? mesh2D_Unfiltered = Planar.Create.Mesh2D(pointCloud2D, new DelaunayPointCloud2DMeshSolver());
            Mesh2D? mesh2D_Eroded = Planar.Create.Mesh2D(pointCloud2D, new DelaunayPointCloud2DMeshSolver(0, 0, edgeLengthFactor: 2.5));

            Assert.NotNull(mesh2D_Unfiltered);
            Assert.NotNull(mesh2D_Eroded);

            // The bridge is gone.
            Assert.True(mesh2D_Eroded.TrianglesCount < mesh2D_Unfiltered.TrianglesCount);

            // Two pieces, so two boundary loops - a separation, not a hole. Each cluster is a three by three
            // block of cells, which triangulates into eighteen.
            Assert.Equal(2, BoundaryLoopCount(mesh2D_Eroded.GetIndexes()));
            Assert.Equal(36, mesh2D_Eroded.TrianglesCount);
        }

        /// <summary>
        /// Tests that a factor of zero or less leaves the triangulation exactly as it was, so the option is inert until it is asked for.
        /// </summary>
        [Fact]
        public void PointCloudMeshErosion_Disabled()
        {
            Random random = new(12345);

            int count = 500;

            double[] x = new double[count];
            double[] y = new double[count];

            for (int i = 0; i < count; i++)
            {
                x[i] = random.NextDouble() * 100.0;
                y[i] = random.NextDouble() * 100.0;
            }

            PointCloud2D pointCloud2D = new(x, y);

            Mesh2D? mesh2D_Unfiltered = Planar.Create.Mesh2D(pointCloud2D, new DelaunayPointCloud2DMeshSolver());
            Mesh2D? mesh2D_Zero = Planar.Create.Mesh2D(pointCloud2D, new DelaunayPointCloud2DMeshSolver(0, 0, edgeLengthFactor: 0));
            Mesh2D? mesh2D_Negative = Planar.Create.Mesh2D(pointCloud2D, new DelaunayPointCloud2DMeshSolver(0, 0, edgeLengthFactor: -1.0));

            Assert.NotNull(mesh2D_Unfiltered);
            Assert.NotNull(mesh2D_Zero);
            Assert.NotNull(mesh2D_Negative);

            Assert.Equal(mesh2D_Unfiltered.TrianglesCount, mesh2D_Zero.TrianglesCount);
            Assert.Equal(mesh2D_Unfiltered.TrianglesCount, mesh2D_Negative.TrianglesCount);
        }

        /// <summary>
        /// Tests that <see cref="DiGi.Geometry.PointCloud.Core.Query.ErodedIndexes"/> answers null rather than throwing when what it is given cannot be eroded.
        /// </summary>
        [Fact]
        public void PointCloudMeshErosion_Invalid()
        {
            double[] x = [0.0, 100.0, 0.0];
            double[] y = [0.0, 0.0, 100.0];

            List<int[]> indexes = [[0, 1, 2]];

            Assert.Null(Core.Query.ErodedIndexes(null, y, indexes, 2.5));
            Assert.Null(Core.Query.ErodedIndexes(x, null, indexes, 2.5));
            Assert.Null(Core.Query.ErodedIndexes(x, y, null, 2.5));
            Assert.Null(Core.Query.ErodedIndexes(x, [0.0, 0.0], indexes, 2.5));
            Assert.Null(Core.Query.ErodedIndexes(x, y, [], 2.5));

            // A lone triangle has nothing to be judged against but itself, so it stands.
            Assert.NotNull(Core.Query.ErodedIndexes(x, y, indexes, 2.5));
        }

        /// <summary>
        /// Tests the reported terrain hole itself, against the points the live store actually holds around it.
        /// <para>These twenty coordinates and elevations are what <c>gis/terrain/mesh3dbycircle</c> answered for a 250 m radius at 630700, 488400 - the node county 55417 is missing. Every one of its neighbours is there; the node itself is not, and the service returned 110.3 for it when asked directly, so the gap is a point the sampling run lost rather than ground there is no elevation for.</para>
        /// <para>Under the fixed 150 m limit the endpoint served twenty four triangles and two boundary loops, the inner one being the diamond of the four nodes around the gap - the hole visible in the rendered terrain. The whole triangulation of these twenty sites is twenty six, and that is what has to come back.</para>
        /// </summary>
        [Fact]
        public void PointCloudMeshErosion_TerrainHole()
        {
            double[] x =
            [
                630600, 630700, 630800,
                630500, 630600, 630700, 630800, 630900,
                630500, 630600, 630800, 630900,
                630500, 630600, 630700, 630800, 630900,
                630600, 630700, 630800
            ];

            double[] y =
            [
                488600, 488600, 488600,
                488500, 488500, 488500, 488500, 488500,
                488400, 488400, 488400, 488400,
                488300, 488300, 488300, 488300, 488300,
                488200, 488200, 488200
            ];

            double[] z =
            [
                109.9, 110.6, 111.9,
                110.0, 110.3, 110.4, 110.5, 108.2,
                106.0, 106.7, 110.6, 110.2,
                112.3, 110.5, 110.8, 110.8, 110.6,
                110.9, 110.6, 111.4
            ];

            Assert.Equal(20, x.Length);
            for (int i = 0; i < x.Length; i++)
            {
                Assert.False(x[i] == 630700 && y[i] == 488400, "The node under test has to be absent from the input.");
            }

            PointCloud3D pointCloud3D = new(x, y, z);

            // How the endpoint was configured when it served the hole.
            HeightFieldPointCloud3DMeshSolver heightFieldPointCloud3DMeshSolver_Absolute = new(0, 150.0, PointCloudHeightSelection.Lowest);
            Mesh3D? mesh3D_Absolute = Spatial.Create.Mesh3D(pointCloud3D, heightFieldPointCloud3DMeshSolver_Absolute);

            Assert.NotNull(mesh3D_Absolute);
            Assert.Equal(24, mesh3D_Absolute.TrianglesCount);
            Assert.Equal(2, BoundaryLoopCount(mesh3D_Absolute.GetIndexes()));

            // How it is configured now.
            HeightFieldPointCloud3DMeshSolver heightFieldPointCloud3DMeshSolver_Eroded = new(0, 0, PointCloudHeightSelection.Lowest, edgeLengthFactor: 2.5);
            Mesh3D? mesh3D_Eroded = Spatial.Create.Mesh3D(pointCloud3D, heightFieldPointCloud3DMeshSolver_Eroded);

            Assert.NotNull(mesh3D_Eroded);
            Assert.Equal(26, mesh3D_Eroded.TrianglesCount);
            Assert.Equal(1, BoundaryLoopCount(mesh3D_Eroded.GetIndexes()));

            // The gap is spanned, not filled: no vertex is invented for the node that is missing.
            Assert.Equal(20, mesh3D_Eroded.PointsCount);

            List<Point3D>? point3Ds = mesh3D_Eroded.GetPoints();

            Assert.NotNull(point3Ds);
            Assert.DoesNotContain(point3Ds, point3D => point3D.X == 630700 && point3D.Y == 488400);
        }

        /// <summary>
        /// Counts the closed loops of edges that bound a mesh, which is one for a single unbroken sheet and one more for every hole or separate piece.
        /// </summary>
        /// <param name="indexes">The triangles of the mesh, as three element index arrays.</param>
        /// <returns>The number of boundary loops.</returns>
        private static int BoundaryLoopCount(List<int[]>? indexes)
        {
            Assert.NotNull(indexes);

            Dictionary<(int, int), int> counts = [];
            foreach (int[] indexes_Triangle in indexes)
            {
                for (int i = 0; i < 3; i++)
                {
                    int index_Start = indexes_Triangle[i];
                    int index_End = indexes_Triangle[(i + 1) % 3];

                    (int, int) key = index_Start < index_End ? (index_Start, index_End) : (index_End, index_Start);

                    counts.TryGetValue(key, out int count);
                    counts[key] = count + 1;
                }
            }

            Dictionary<int, List<int>> indexes_Adjacent = [];
            foreach (KeyValuePair<(int, int), int> keyValuePair in counts)
            {
                if (keyValuePair.Value != 1)
                {
                    continue;
                }

                if (!indexes_Adjacent.TryGetValue(keyValuePair.Key.Item1, out List<int>? indexes_1))
                {
                    indexes_1 = [];
                    indexes_Adjacent[keyValuePair.Key.Item1] = indexes_1;
                }

                indexes_1.Add(keyValuePair.Key.Item2);

                if (!indexes_Adjacent.TryGetValue(keyValuePair.Key.Item2, out List<int>? indexes_2))
                {
                    indexes_2 = [];
                    indexes_Adjacent[keyValuePair.Key.Item2] = indexes_2;
                }

                indexes_2.Add(keyValuePair.Key.Item1);
            }

            HashSet<int> visited = [];

            int result = 0;
            foreach (int index in indexes_Adjacent.Keys)
            {
                if (!visited.Add(index))
                {
                    continue;
                }

                result++;

                Stack<int> indexes_Pending = new();
                indexes_Pending.Push(index);

                while (indexes_Pending.Count != 0)
                {
                    foreach (int index_Adjacent in indexes_Adjacent[indexes_Pending.Pop()])
                    {
                        if (visited.Add(index_Adjacent))
                        {
                            indexes_Pending.Push(index_Adjacent);
                        }
                    }
                }
            }

            return result;
        }
    }
}
