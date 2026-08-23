using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.GetPoint2DsByReferencesAsync(Npgsql.NpgsqlConnection?, IEnumerable{string}?, int?, bool, System.Threading.CancellationToken)"/>
        /// and <see cref="Building2DPostgreSQLConverter.GetPoint2DsByReferencesAsync(IEnumerable{string}?, int?, bool, System.Threading.CancellationToken)"/>
        /// return null when given null inputs.
        /// </summary>
        [Fact]
        public async Task GetPoint2DsByReferencesAsync_NullInputs_ReturnsNull()
        {
            List<Point2D>? point2Ds_NullConnection = await Building2DPostgreSQLConverter.GetPoint2DsByReferencesAsync(null, ["28A8E11F-6255-8A99-E053-CA2BA8C0EC21"], null);
            Assert.Null(point2Ds_NullConnection);

            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);
            List<Point2D>? point2Ds_NullReferences = await building2DPostgreSQLConverter.GetPoint2DsByReferencesAsync(null, null);
            Assert.Null(point2Ds_NullReferences);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.GetPoint2DsByReferencesAsync(IEnumerable{string}?, int?, bool, System.Threading.CancellationToken)"/>
        /// returns null when connection data is null and references are provided.
        /// </summary>
        [Fact]
        public async Task GetPoint2DsByReferencesAsync_NullConnection_ReturnsNull()
        {
            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);
            List<Point2D>? point2Ds = await building2DPostgreSQLConverter.GetPoint2DsByReferencesAsync(["28A8E11F-6255-8A99-E053-CA2BA8C0EC21"], null);
            Assert.Null(point2Ds);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.GetPoint2DsByReferencesAsync(IEnumerable{string}?, int?, bool, System.Threading.CancellationToken)"/>
        /// returns an empty list when given an empty collection of references.
        /// </summary>
        [Fact]
        public async Task GetPoint2DsByReferencesAsync_EmptyReferences_ReturnsEmptyList()
        {
            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);
            List<Point2D>? point2Ds = await building2DPostgreSQLConverter.GetPoint2DsByReferencesAsync([], null);
            Assert.NotNull(point2Ds);
            Assert.Empty(point2Ds);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.GetPoint2DsByReferencesAsync(IEnumerable{string}?, int?, bool, System.Threading.CancellationToken)"/>
        /// retrieves 2D points for existing building references both with and without providing an explicit county identifier.
        /// <para>Skipped by default: requires a live, populated PostgreSQL database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a database before running.")]
        public async Task GetPoint2DsByReferencesAsync_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            List<string> references = ["28A8E11F-6255-8A99-E053-CA2BA8C0EC21", "28A8E11F-9B7B-8A99-E053-CA2BA8C0EC21"];

            // 1. Query with explicit countyId
            List<Point2D>? point2Ds_WithCounty = await building2DPostgreSQLConverter.GetPoint2DsByReferencesAsync(references, 73485);
            Assert.NotNull(point2Ds_WithCounty);
            Assert.Equal(2, point2Ds_WithCounty.Count);

            // 2. Query without countyId (should find matches across partitions)
            List<Point2D>? point2Ds_WithoutCounty = await building2DPostgreSQLConverter.GetPoint2DsByReferencesAsync(references, null);
            Assert.NotNull(point2Ds_WithoutCounty);
            Assert.Equal(2, point2Ds_WithoutCounty.Count);
        }
    }
}
