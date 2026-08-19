using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that passing a null point to <see cref="Building2DPostgreSQLConverter.GetBuilding2DByPoint2DAsync"/> returns null.
        /// </summary>
        [Fact]
        public async Task GetBuilding2DByPoint2DAsync_NullPoint()
        {
            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);

            Building2D? building2D = await building2DPostgreSQLConverter.GetBuilding2DByPoint2DAsync(null);

            Assert.Null(building2D);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.GetBuilding2DByPoint2DAsync"/> retrieves a building at a known point and validates polygon containment.
        /// <para>Skipped by default: requires a live, populated PostgreSQL database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a refreshed database before running.")]
        public async Task GetBuilding2DByPoint2DAsync_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            // Point inside a known building
            Point2D point2D = new(573489, 247110);

            Building2D? building2D = await building2DPostgreSQLConverter.GetBuilding2DByPoint2DAsync(point2D);
            if (building2D is not null)
            {
                GIS.Classes.Building2D? building2D_GIS = building2D.ToDiGi();
                Assert.NotNull(building2D_GIS);
                Assert.NotNull(building2D_GIS.PolygonalFace2D);
                Assert.True(building2D_GIS.PolygonalFace2D.InRange(point2D, Core.Constants.Tolerance.MacroDistance));
            }
        }
    }
}
