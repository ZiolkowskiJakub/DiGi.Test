using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.GetBuilding2DsByBuilding2DReferences(IEnumerable{Building2DReference}, bool, int)"/>
        /// returns null when the input collection is null.
        /// </summary>
        [Fact]
        public async Task GetBuilding2DsByBuilding2DReferences_NullInputs_ReturnsNull()
        {
            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);
            List<Building2D>? building2Ds = await building2DPostgreSQLConverter.GetBuilding2DsByBuilding2DReferences(null);
            Assert.Null(building2Ds);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.GetBuilding2DsByBuilding2DReferencesAsync(IEnumerable{Building2DReference}, bool, int, System.Threading.CancellationToken)"/>
        /// returns an empty list when given an empty collection of references.
        /// </summary>
        [Fact]
        public async Task GetBuilding2DsByBuilding2DReferencesAsync_EmptyReferences_ReturnsEmptyList()
        {
            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);
            List<Building2D>? building2Ds = await building2DPostgreSQLConverter.GetBuilding2DsByBuilding2DReferencesAsync([]);
            Assert.NotNull(building2Ds);
            Assert.Empty(building2Ds);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.GetBuilding2DReferencesAsync(IEnumerable{Building2DReference}, bool, int, System.Threading.CancellationToken)"/>
        /// returns null when input is null.
        /// </summary>
        [Fact]
        public async Task GetBuilding2DReferencesAsync_NullInputs_ReturnsNull()
        {
            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);
            List<Building2DReference>? building2DReferences = await building2DPostgreSQLConverter.GetBuilding2DReferencesAsync(null);
            Assert.Null(building2DReferences);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.GetBuilding2DsByBuilding2DReferences(IEnumerable{Building2DReference}, bool, int)"/>
        /// returns an empty list when connection data is null and references are provided.
        /// </summary>
        [Fact]
        public async Task GetBuilding2DsByBuilding2DReferences_NullConnection_ReturnsEmptyList()
        {
            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);
            Building2DReference building2DReference = new()
            {
                Reference = "28A8E11F-6255-8A99-E053-CA2BA8C0EC21",
                CountyId = 73485
            };

            List<Building2D>? building2Ds = await building2DPostgreSQLConverter.GetBuilding2DsByBuilding2DReferences([building2DReference]);
            Assert.NotNull(building2Ds);
            Assert.Empty(building2Ds);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.GetBuilding2DsByBuilding2DReferences(IEnumerable{Building2DReference}, bool, int)"/>
        /// retrieves buildings for existing references, and demonstrates that setting <c>fallbackByReference = true</c> enables fallback resolution for references whose county identifier was mismatched or omitted.
        /// <para>Skipped by default: requires a live, populated PostgreSQL database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a database before running.")]
        public async Task GetBuilding2DsByBuilding2DReferences_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            // Reference known to exist in the database (e.g. county 73485)
            Building2DReference building2DReference_Valid = new()
            {
                Reference = "28A8E11F-6255-8A99-E053-CA2BA8C0EC21",
                CountyId = 73485
            };

            // Intentionally provide a non-matching county identifier to test fallback
            Building2DReference building2DReference_MismatchedCounty = new()
            {
                Reference = "28A8E11F-6255-8A99-E053-CA2BA8C0EC21",
                CountyId = -999
            };

            // 1. Query with valid county ID
            List<Building2D>? building2Ds_Direct = await building2DPostgreSQLConverter.GetBuilding2DsByBuilding2DReferences([building2DReference_Valid]);
            Assert.NotNull(building2Ds_Direct);
            Assert.Single(building2Ds_Direct);

            // 2. Query with mismatched county ID and referenceOnlyCheck = false (should find nothing)
            List<Building2D>? building2Ds_MismatchedNoFallback = await building2DPostgreSQLConverter.GetBuilding2DsByBuilding2DReferences([building2DReference_MismatchedCounty], false);
            Assert.NotNull(building2Ds_MismatchedNoFallback);
            Assert.Empty(building2Ds_MismatchedNoFallback);

            // 3. Query with mismatched county ID and referenceOnlyCheck = true (should find matching building via fallback)
            List<Building2D>? building2Ds_MismatchedWithFallback = await building2DPostgreSQLConverter.GetBuilding2DsByBuilding2DReferences([building2DReference_MismatchedCounty], true);
            Assert.NotNull(building2Ds_MismatchedWithFallback);
            Assert.Single(building2Ds_MismatchedWithFallback);
            Assert.Equal("28A8E11F-6255-8A99-E053-CA2BA8C0EC21", building2Ds_MismatchedWithFallback[0].Reference);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.GetBuilding2DsByCountyIdAsync(int, int?, IEnumerable{string}?, int, System.Threading.CancellationToken)"/>
        /// returns null when connection data is null.
        /// </summary>
        [Fact]
        public async Task GetBuilding2DsByCountyIdAsync_NullConnection_ReturnsNull()
        {
            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);
            List<Building2D>? building2Ds = await building2DPostgreSQLConverter.GetBuilding2DsByCountyIdAsync(73485, 100);
            Assert.Null(building2Ds);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.GetBuilding2DsByCountyIdAsync(int, IEnumerable{int}?, int, System.Threading.CancellationToken)"/>
        /// returns null when connection data is null.
        /// </summary>
        [Fact]
        public async Task GetBuilding2DsByCountyIdAsync_SubdivisionIds_NullConnection_ReturnsNull()
        {
            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);
            List<Building2D>? building2Ds = await building2DPostgreSQLConverter.GetBuilding2DsByCountyIdAsync(73485, [100, 101]);
            Assert.Null(building2Ds);
        }

        /// <summary>
        /// Verifies that static <see cref="Building2DPostgreSQLConverter.GetBuilding2DsByCountyIdAsync(Npgsql.NpgsqlConnection, int, IEnumerable{int}?, int, System.Threading.CancellationToken)"/>
        /// returns null when connection is null.
        /// </summary>
        [Fact]
        public async Task GetBuilding2DsByCountyIdAsync_Static_NullConnection_ReturnsNull()
        {
            List<Building2D>? building2Ds = await Building2DPostgreSQLConverter.GetBuilding2DsByCountyIdAsync(null, 73485);
            Assert.Null(building2Ds);
        }
    }
}
