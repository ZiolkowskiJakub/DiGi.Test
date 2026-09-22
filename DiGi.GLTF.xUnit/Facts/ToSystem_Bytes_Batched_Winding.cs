using System.Collections.Generic;
using System.Reflection;
using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Enums;
using DiGi.Core;
using DiGi.Geometry.PointCloud.Core.Enums;
using DiGi.Geometry.PointCloud.Spatial.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.GLTF;

namespace DiGi.GLTF.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Reproduces the building half of DiGi.GLTF#1: the batched building envelope has inconsistently wound triangles.
        /// <para>Converts a <see cref="BuildingModel"/> through the DiGi.GLTF.Analytical <c>Envelope</c> branch, exports the batched payload, reads the index buffer back, and asserts the envelope is outward-consistent (shared-edge parity). On the unmodified converter about half of the triangles wind inward, so this fails; after the <c>GetExternalShell</c> switch-over it must pass.</para>
        /// </summary>
        [Fact]
        public void ToSystem_Bytes_Batched_Winding_Building()
        {
            List<BuildingModel>? buildingModels = ToSystem_Bytes_Batched_Winding_Load<BuildingModel>("BuildingModel_Envelope.json");
            Assert.NotNull(buildingModels);

            BuildingModel? buildingModel = buildingModels!.Find(x => x.Guid == new Guid("70680df7-1751-442f-8788-7b6d3cb449d2"));
            Assert.NotNull(buildingModel);

            List<Classes.GLTFNode>? gLTFNodes = DiGi.GLTF.Analytical.Convert.ToGLTF_GLTFNodes(buildingModel!, null, Core.Constants.Tolerance.Distance, BuildingModelDetailLevel.Envelope);
            Assert.NotNull(gLTFNodes);

            Classes.GLTFScene? scene = gLTFNodes!.GLTFScene("building");
            Assert.NotNull(scene);

            byte[]? glb = scene.ToSystem_Bytes(true);
            Assert.NotNull(glb);

            (float[] positions, int[] indices) = ToSystem_Bytes_Batched_Winding_ParseGlb(glb!);

            (int opposite, int same, int naked) = ToSystem_Bytes_Batched_Winding_CountEdges(indices);

            ToSystem_Bytes_Batched_Winding_Report("Winding_Building.txt", $"opposite={opposite} same={same} naked={naked}");

            // The fixture envelope is closed, so every shared edge has to be traversed in opposite directions.
            // The unmodified converter scored 10 of 84 the same way.
            Assert.True(opposite > 0, "The envelope has no shared edges; the test is invalid.");
            Assert.True(same == 0, $"{same} shared edges are traversed in the same direction by their two faces (opposite={opposite}, naked={naked}); the winding is not consistent.");
        }

        /// <summary>
        /// Guards the terrain half of DiGi.GLTF#1: the batched terrain heightfield keeps its positive-Z winding.
        /// <para>Verified 2026-09-22: a raw <see cref="HeightFieldPointCloud3DMeshSolver"/> height field is 100% +Z-consistent (4 980 up, 0 down, 0 vertical), so the engine must not disturb it. This fact therefore asserts the batched payload preserves that winding. The terrain defect itself (down-facing and mis-wound cut walls) is introduced by the footprint cuts in DiGi.GIS.WebAPI.UI, not by the height field, and is reproduced there rather than here.</para>
        /// </summary>
        [Fact]
        public void ToSystem_Bytes_Batched_Winding_Terrain()
        {
            // The terrain_response.json fixture is a single triangle, too small to show winding.
            // Build a real height field through the solver (the same path the terrain pipeline uses).
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
            Mesh3D? mesh3D = DiGi.Geometry.PointCloud.Spatial.Create.Mesh3D(pointCloud3D, heightFieldPointCloud3DMeshSolver);
            Assert.NotNull(mesh3D);

            Classes.GLTFNode gLTFNode = new("terrain", null, mesh3D, null, 1, null);
            Classes.GLTFScene? scene = new List<Classes.GLTFNode> { gLTFNode }.GLTFScene("terrain");
            Assert.NotNull(scene);

            byte[]? glb = scene.ToSystem_Bytes(true);
            Assert.NotNull(glb);

            (float[] positions, int[] indices) = ToSystem_Bytes_Batched_Winding_ParseGlb(glb!);

            (int up, int down, int vertical) = ToSystem_Bytes_Batched_Winding_CountTerrainWinding(positions, indices);

            ToSystem_Bytes_Batched_Winding_Report("Winding_Terrain.txt", $"up={up} down={down} vertical={vertical}");

            Assert.Equal(0, down);
        }

        private static List<T>? ToSystem_Bytes_Batched_Winding_Load<T>(string fileName) where T : Core.Interfaces.ISerializableObject
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), fileName);
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            {
                return null;
            }

            return Core.Convert.ToDiGi<T>((Core.Classes.Path)path);
        }

        private static void ToSystem_Bytes_Batched_Winding_Report(string fileName, string line)
        {
            string? pathReports = Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly());
            if (string.IsNullOrWhiteSpace(pathReports))
            {
                return;
            }

            System.IO.File.WriteAllText(System.IO.Path.Combine(pathReports, fileName), line + System.Environment.NewLine);
        }

        private static (float[] positions, int[] indices) ToSystem_Bytes_Batched_Winding_ParseGlb(byte[] glb)
        {
            // Walk the GLB chunks: 12-byte container header, then [length, type, data] chunks.
            int offset = 12;
            byte[] jsonBytes = [];
            byte[] binBytes = [];
            while (offset + 8 <= glb.Length)
            {
                int chunkLength = System.BitConverter.ToInt32(glb, offset);
                int chunkType = System.BitConverter.ToInt32(glb, offset + 4);
                byte[] chunkData = new byte[chunkLength];
                System.Buffer.BlockCopy(glb, offset + 8, chunkData, 0, chunkLength);

                if (chunkType == 0x4E4F534A)
                {
                    jsonBytes = chunkData;
                }
                else if (chunkType == 0x004E4942)
                {
                    binBytes = chunkData;
                }

                offset += 8 + chunkLength;
            }

            using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(jsonBytes);
            System.Text.Json.JsonElement accessors = document.RootElement.GetProperty("accessors");
            int vertexCount = accessors[0].GetProperty("count").GetInt32();
            int indexCount = accessors[3].GetProperty("count").GetInt32();

            // Single-batch BIN layout: positions | colors | objectIds | indices.
            float[] positions = new float[vertexCount * 3];
            System.Buffer.BlockCopy(binBytes, 0, positions, 0, vertexCount * 3 * 4);

            int indicesOffset = vertexCount * 12 + vertexCount * 4 + vertexCount * 4;
            int[] indices = new int[indexCount];
            System.Buffer.BlockCopy(binBytes, indicesOffset, indices, 0, indexCount * 4);

            return (positions, indices);
        }

        private static (int opposite, int same, int naked) ToSystem_Bytes_Batched_Winding_CountEdges(int[] indices)
        {
            // A consistently wound envelope traverses every shared edge in opposite directions: the two
            // faces on either side of an edge run along it in reverse order. The centroid test is imperfect
            // for non-convex solids, so this shared-edge parity is the measure (see the DiGi.GLTF#1 comment).
            Dictionary<(int, int), List<(int from, int to)>> edges = new();

            void AddDirectedEdge(int from, int to)
            {
                int a = System.Math.Min(from, to);
                int b = System.Math.Max(from, to);
                if (!edges.TryGetValue((a, b), out List<(int, int)>? list))
                {
                    list = new List<(int, int)>();
                    edges[(a, b)] = list;
                }

                list.Add((from, to));
            }

            for (int i = 0; i < indices.Length; i += 3)
            {
                AddDirectedEdge(indices[i], indices[i + 1]);
                AddDirectedEdge(indices[i + 1], indices[i + 2]);
                AddDirectedEdge(indices[i + 2], indices[i]);
            }

            int opposite = 0;
            int same = 0;
            int naked = 0;
            foreach (KeyValuePair<(int, int), List<(int, int)>> entry in edges)
            {
                List<(int, int)> directed = entry.Value;
                if (directed.Count == 1)
                {
                    naked++;
                }
                else if (directed.Count == 2)
                {
                    if (directed[0].Item1 == directed[1].Item1)
                    {
                        same++;
                    }
                    else
                    {
                        opposite++;
                    }
                }
                else
                {
                    same += directed.Count - 1;
                }
            }

            return (opposite, same, naked);
        }

        private static (int up, int down, int vertical) ToSystem_Bytes_Batched_Winding_CountTerrainWinding(float[] positions, int[] indices)
        {
            const double verticalThreshold = 0.05;

            int up = 0;
            int down = 0;
            int vertical = 0;
            for (int i = 0; i < indices.Length; i += 3)
            {
                double nx = ToSystem_Bytes_Batched_Winding_NormalX(positions, indices, i);
                double ny = ToSystem_Bytes_Batched_Winding_NormalY(positions, indices, i);
                double nz = ToSystem_Bytes_Batched_Winding_NormalZ(positions, indices, i);

                double length = System.Math.Sqrt(nx * nx + ny * ny + nz * nz);
                if (length < 1e-12)
                {
                    continue;
                }

                double normalizedZ = nz / length;
                if (System.Math.Abs(normalizedZ) < verticalThreshold)
                {
                    vertical++;
                }
                else if (normalizedZ < 0)
                {
                    down++;
                }
                else
                {
                    up++;
                }
            }

            return (up, down, vertical);
        }

        private static double ToSystem_Bytes_Batched_Winding_NormalX(float[] positions, int[] indices, int i)
        {
            int i0 = indices[i] * 3, i1 = indices[i + 1] * 3, i2 = indices[i + 2] * 3;
            return (positions[i1 + 1] - positions[i0 + 1]) * (positions[i2 + 2] - positions[i0 + 2]) - (positions[i1 + 2] - positions[i0 + 2]) * (positions[i2 + 1] - positions[i0 + 1]);
        }

        private static double ToSystem_Bytes_Batched_Winding_NormalY(float[] positions, int[] indices, int i)
        {
            int i0 = indices[i] * 3, i1 = indices[i + 1] * 3, i2 = indices[i + 2] * 3;
            return (positions[i1 + 2] - positions[i0 + 2]) * (positions[i2] - positions[i0]) - (positions[i1] - positions[i0]) * (positions[i2 + 2] - positions[i0 + 2]);
        }

        private static double ToSystem_Bytes_Batched_Winding_NormalZ(float[] positions, int[] indices, int i)
        {
            int i0 = indices[i] * 3, i1 = indices[i + 1] * 3, i2 = indices[i + 2] * 3;
            return (positions[i1] - positions[i0]) * (positions[i2 + 1] - positions[i0 + 1]) - (positions[i1 + 1] - positions[i0 + 1]) * (positions[i2] - positions[i0]);
        }
    }
}
