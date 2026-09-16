using DiGi.Analytical.Building.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.PointCloud.Core.Enums;
using DiGi.Geometry.PointCloud.Spatial.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.GLTF.Classes;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Reflection;

namespace DiGi.GIS.WebAPI.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Reproduces ZiolkowskiJakub/DiGi.GIS.WebAPI.UI#33: the terrain node created for a single building scene must cover the whole display circle, and therefore the building's own bounding envelope with its margin, when the county is sampled on a coarse 100 m lattice.
        /// <para>The terrain service is stood in for by a handler that answers exactly as the deployed one does: it triangulates the lattice points found inside the query circle with the service's own <see cref="HeightFieldPointCloud3DMeshSolver"/> and nothing else, so the returned surface stops short of the query radius by up to one lattice diagonal. The building is the reported one (county 53477, id 10595050): a 52 x 82 m envelope whose display circle is the 100 m minimum.</para>
        /// <para>On the unmodified code the query radius is 115 m, the service answers the single 100 x 100 m cell around the centre, and the clipped surface reaches only about 17 m from the centre while the building envelope starts 24 m further south than the cell.</para>
        /// </summary>
        [Fact]
        public async System.Threading.Tasks.Task TerrainGLTFNodeAsync_CoarseLattice_CoversDisplayCircle()
        {
            const double latticeStep = 100.0;

            List<string> requestUris = [];

            TerrainServiceHttpMessageHandler terrainServiceHttpMessageHandler = new(latticeStep, requestUris);
            using HttpClient httpClient = new(terrainServiceHttpMessageHandler);

            // The envelope of the reported building, in PL-1992 metres.
            Point2D[] footprint =
            [
                new(627932.3, 500275.7),
                new(627984.4, 500275.7),
                new(627984.4, 500358.1),
                new(627932.3, 500358.1)
            ];

            BuildingModel buildingModel = CreateTestBuildingModel(new Polygon2D(footprint));

            Circle2D? circle2D = buildingModel.TerrainCircle();
            Assert.NotNull(circle2D);
            Assert.NotNull(circle2D.Center);
            Assert.Equal(Constants.Default.TerrainRadius, circle2D.Radius, 3);

            GLTFNode? gLTFNode = await httpClient.TerrainGLTFNodeAsync(circle2D);

            List<string> reportLines = [];
            reportLines.Add($"Display circle: centre ({circle2D.Center.X.ToString(CultureInfo.InvariantCulture)}, {circle2D.Center.Y.ToString(CultureInfo.InvariantCulture)}), radius {circle2D.Radius.ToString(CultureInfo.InvariantCulture)}");
            reportLines.Add($"Lattice step: {latticeStep.ToString(CultureInfo.InvariantCulture)}");
            foreach (string requestUri in requestUris)
            {
                reportLines.Add($"Request: {requestUri}");
            }

            Mesh3D? mesh3D = gLTFNode?.Mesh3D;
            List<Point3D>? point3Ds = mesh3D?.GetPoints();
            List<int[]>? indexes = mesh3D?.GetIndexes();

            reportLines.Add($"Mesh: {(mesh3D is null ? "none" : $"{point3Ds?.Count ?? 0} points, {indexes?.Count ?? 0} triangles")}");

            double radius_Covered = double.NaN;
            BoundingBox3D? boundingBox3D_Mesh = mesh3D?.GetBoundingBox();
            if (point3Ds is not null && indexes is not null && boundingBox3D_Mesh is not null)
            {
                radius_Covered = CoveredRadius(point3Ds, indexes, circle2D.Center);
                reportLines.Add($"Mesh plan extents: X [{boundingBox3D_Mesh.MinX.ToString(CultureInfo.InvariantCulture)}, {boundingBox3D_Mesh.MaxX.ToString(CultureInfo.InvariantCulture)}] Y [{boundingBox3D_Mesh.MinY.ToString(CultureInfo.InvariantCulture)}, {boundingBox3D_Mesh.MaxY.ToString(CultureInfo.InvariantCulture)}]");
                reportLines.Add($"Covered radius around the centre: {radius_Covered.ToString("F3", CultureInfo.InvariantCulture)}");
            }

            string? reportsDirectory = Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly());
            Assert.False(string.IsNullOrWhiteSpace(reportsDirectory));
            System.IO.File.WriteAllLines(System.IO.Path.Combine(reportsDirectory, "TerrainGLTFNodeAsync_Coverage.txt"), reportLines);

            Assert.NotNull(mesh3D);
            Assert.NotNull(point3Ds);
            Assert.NotNull(indexes);
            Assert.NotNull(boundingBox3D_Mesh);

            // The display circle is clipped as a regular polygon, so the surface may legitimately stop at that polygon's inradius.
            double radius_Expected = circle2D.Radius * System.Math.Cos(System.Math.PI / Constants.Default.TerrainCircleSegmentCount);
            Assert.True(radius_Covered >= radius_Expected - Core.Constants.Tolerance.Distance, $"The surface reaches only {radius_Covered:F3} m from the centre; the display circle needs {radius_Expected:F3} m.");

            // The building envelope, plus the margin the display circle was sized with, lies under the surface on every side.
            BoundingBox3D? boundingBox3D_Building = buildingModel.GetBoundingBox();
            Assert.NotNull(boundingBox3D_Building);

            double margin = Constants.Default.TerrainPadding;
            Assert.True(boundingBox3D_Mesh.MinX <= boundingBox3D_Building.MinX - margin + Core.Constants.Tolerance.Distance, $"Surface MinX {boundingBox3D_Mesh.MinX} does not reach the building envelope MinX {boundingBox3D_Building.MinX} with a {margin} m margin.");
            Assert.True(boundingBox3D_Mesh.MaxX >= boundingBox3D_Building.MaxX + margin - Core.Constants.Tolerance.Distance, $"Surface MaxX {boundingBox3D_Mesh.MaxX} does not reach the building envelope MaxX {boundingBox3D_Building.MaxX} with a {margin} m margin.");
            Assert.True(boundingBox3D_Mesh.MinY <= boundingBox3D_Building.MinY - margin + Core.Constants.Tolerance.Distance, $"Surface MinY {boundingBox3D_Mesh.MinY} does not reach the building envelope MinY {boundingBox3D_Building.MinY} with a {margin} m margin.");
            Assert.True(boundingBox3D_Mesh.MaxY >= boundingBox3D_Building.MaxY + margin - Core.Constants.Tolerance.Distance, $"Surface MaxY {boundingBox3D_Mesh.MaxY} does not reach the building envelope MaxY {boundingBox3D_Building.MaxY} with a {margin} m margin.");

            // The query that reached the wire was grown by the clip buffer and one worst case lattice diagonal.
            Assert.Single(requestUris);
            double radius_Query = QueryRadius(requestUris[0]);
            double radius_Query_Expected = circle2D.Radius + Constants.Default.TerrainBuffer + (Constants.Default.TerrainLatticeStepMax * System.Math.Sqrt(2));
            Assert.Equal(radius_Query_Expected, radius_Query, 3);
        }

        /// <summary>
        /// Calculates how far from the given centre a plan view surface reaches in every direction: the shortest distance from the centre to any edge carried by a single triangle.
        /// </summary>
        /// <param name="point3Ds">The points of the surface.</param>
        /// <param name="indexes">The triangles of the surface, as three element index arrays.</param>
        /// <param name="center">The centre the distance is measured from.</param>
        /// <returns>The radius of the largest disc around the centre that lies inside the surface.</returns>
        private static double CoveredRadius(List<Point3D> point3Ds, List<int[]> indexes, Point2D center)
        {
            Dictionary<(int, int), int> counts = [];
            foreach (int[] indexes_Triangle in indexes)
            {
                for (int i = 0; i < 3; i++)
                {
                    int index_1 = indexes_Triangle[i];
                    int index_2 = indexes_Triangle[(i + 1) % 3];
                    (int, int) key = index_1 < index_2 ? (index_1, index_2) : (index_2, index_1);
                    counts[key] = counts.TryGetValue(key, out int count) ? count + 1 : 1;
                }
            }

            double result = double.PositiveInfinity;
            foreach (KeyValuePair<(int, int), int> keyValuePair in counts)
            {
                if (keyValuePair.Value != 1)
                {
                    continue;
                }

                Point3D point3D_1 = point3Ds[keyValuePair.Key.Item1];
                Point3D point3D_2 = point3Ds[keyValuePair.Key.Item2];

                Segment2D segment2D = new(new Point2D(point3D_1.X, point3D_1.Y), new Point2D(point3D_2.X, point3D_2.Y));
                double distance = segment2D.Distance(center);
                if (distance < result)
                {
                    result = distance;
                }
            }

            return result;
        }

        /// <summary>
        /// Reads the radius parameter out of a terrain service circle request.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <returns>The radius, or NaN when the request carries none.</returns>
        private static double QueryRadius(string requestUri)
        {
            System.Collections.Specialized.NameValueCollection nameValueCollection = System.Web.HttpUtility.ParseQueryString(new System.Uri(requestUri).Query);
            string? value = nameValueCollection["radius"];
            return value is not null && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result) ? result : double.NaN;
        }

        /// <summary>
        /// Stands in for the GIS Web API terrain service over a synthetic regular lattice: answers a circle request with the triangulation of the lattice points inside the circle, built by the solver the service uses, and nothing beyond them.
        /// </summary>
        private sealed class TerrainServiceHttpMessageHandler : HttpMessageHandler
        {
            private readonly double latticeStep;
            private readonly List<string> requestUris;

            public TerrainServiceHttpMessageHandler(double latticeStep, List<string> requestUris)
            {
                this.latticeStep = latticeStep;
                this.requestUris = requestUris;
            }

            protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                string requestUri = request.RequestUri?.ToString() ?? string.Empty;
                requestUris.Add(requestUri);

                System.Collections.Specialized.NameValueCollection nameValueCollection = System.Web.HttpUtility.ParseQueryString(request.RequestUri?.Query ?? string.Empty);
                if (!double.TryParse(nameValueCollection["x"], NumberStyles.Float, CultureInfo.InvariantCulture, out double x) || !double.TryParse(nameValueCollection["y"], NumberStyles.Float, CultureInfo.InvariantCulture, out double y) || !double.TryParse(nameValueCollection["radius"], NumberStyles.Float, CultureInfo.InvariantCulture, out double radius))
                {
                    return System.Threading.Tasks.Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
                }

                List<Point3D> point3Ds = [];

                double x_Min = System.Math.Floor((x - radius) / latticeStep) * latticeStep;
                double x_Max = System.Math.Ceiling((x + radius) / latticeStep) * latticeStep;
                double y_Min = System.Math.Floor((y - radius) / latticeStep) * latticeStep;
                double y_Max = System.Math.Ceiling((y + radius) / latticeStep) * latticeStep;

                for (double x_Node = x_Min; x_Node <= x_Max; x_Node += latticeStep)
                {
                    for (double y_Node = y_Min; y_Node <= y_Max; y_Node += latticeStep)
                    {
                        double dx = x_Node - x;
                        double dy = y_Node - y;
                        if ((dx * dx) + (dy * dy) > radius * radius)
                        {
                            continue;
                        }

                        point3Ds.Add(new Point3D(x_Node, y_Node, 77.0 + (0.01 * dx) - (0.005 * dy)));
                    }
                }

                if (point3Ds.Count < 3)
                {
                    return System.Threading.Tasks.Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
                }

                PointCloud3D pointCloud3D = new(point3Ds);
                HeightFieldPointCloud3DMeshSolver heightFieldPointCloud3DMeshSolver = new(0, 0, PointCloudHeightSelection.Lowest, edgeLengthFactor: 2.5);

                Mesh3D? mesh3D = Geometry.PointCloud.Spatial.Create.Mesh3D(pointCloud3D, heightFieldPointCloud3DMeshSolver);
                string? json = mesh3D is null ? null : Core.Convert.ToSystem_String(mesh3D);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return System.Threading.Tasks.Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
                }

                HttpResponseMessage httpResponseMessage = new(System.Net.HttpStatusCode.OK);
                httpResponseMessage.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                return System.Threading.Tasks.Task.FromResult(httpResponseMessage);
            }
        }
    }
}
