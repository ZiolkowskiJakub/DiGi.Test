using DiGi.GIS.PostgreSQL.Classes;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Samples one county end to end, then samples it again and checks that the second run stores nothing.
        /// <para>Skipped by default: it needs the PostgreSQL configuration files beside the test assembly and it calls the live GUGiK elevation service. Set <c>countyId</c> to a small county before running it, and expect the first run to take as long as one request per point allows.</para>
        /// <para>The second run is the point of the fact. It stores nothing only if the grid, the tiling and the reading back of what is already stored all agree - a grid that is anchored differently on the second run, a tile boundary that overlaps its neighbour, or a stored point that is not recognised as a node would each show up here as points written twice.</para>
        /// </summary>
        [Fact(Skip = "Requires the PostgreSQL configuration files and the live GUGiK elevation service.")]
        public async Task PostgreSQLTerrainPointCreateTableTask_Integration()
        {
            // Set to a small county before running. Sampling a large one at this grid size takes hours.
            int countyId = 2405;
            double gridSize = 100;

            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            using HttpClient httpClient = new();

            long pointCount_First = await RunAsync(gISPostgreSQLConverterManager, httpClient, countyId, gridSize);
            long pointCount_Second = await RunAsync(gISPostgreSQLConverterManager, httpClient, countyId, gridSize);

            Assert.True(pointCount_First > 0, "The first run should store points.");
            Assert.Equal(0, pointCount_Second);

            string? directory_Reports = Core.xUnit.Query.ReportsDirectory(Assembly.GetExecutingAssembly());
            Assert.False(string.IsNullOrWhiteSpace(directory_Reports));

            File.WriteAllText(Path.Combine(directory_Reports!, "TerrainPointCreateTable.txt"), $"County {countyId} at grid size {gridSize}: first run stored {pointCount_First} points, second run stored {pointCount_Second}.");
        }

        /// <summary>
        /// Runs the task once over one county and returns the number of points it stored.
        /// </summary>
        /// <param name="gISPostgreSQLConverterManager">The converter manager holding the connections.</param>
        /// <param name="httpClient">The HTTP client used to reach the elevation service.</param>
        /// <param name="countyId">The county to sample.</param>
        /// <param name="gridSize">The spacing of the sampling grid.</param>
        /// <returns>The number of points stored.</returns>
        private static async Task<long> RunAsync(GISPostgreSQLConverterManager gISPostgreSQLConverterManager, HttpClient httpClient, int countyId, double gridSize)
        {
            PostgreSQLTerrainPointCreateTableTask postgreSQLTerrainPointCreateTableTask = new(httpClient, gISPostgreSQLConverterManager)
            {
                PostgreSQLTerrainPointCreateTableOptions = new PostgreSQLTerrainPointCreateTableOptions()
                {
                    CountyIds = [countyId],
                    GridSize = gridSize
                }
            };

            // The base class starts the work without handing back anything to await, so completion is taken from the event it raises.
            TaskCompletionSource<bool> taskCompletionSource = new();
            postgreSQLTerrainPointCreateTableTask.Stopped += (object? sender, EventArgs e) => taskCompletionSource.TrySetResult(true);

            postgreSQLTerrainPointCreateTableTask.Start();

            await taskCompletionSource.Task;

            Assert.Null(postgreSQLTerrainPointCreateTableTask.Exception);
            Assert.Equal(0, postgreSQLTerrainPointCreateTableTask.FailedBatchCount);
            Assert.Equal(0, postgreSQLTerrainPointCreateTableTask.UnresolvedPointCount);
            Assert.True(postgreSQLTerrainPointCreateTableTask.IsSucceeded);

            return postgreSQLTerrainPointCreateTableTask.PointCount;
        }
    }
}
