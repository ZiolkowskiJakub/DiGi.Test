using DiGi.GIS.PostgreSQL.Classes;
using Npgsql;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="OrtoDatasPostgreSQLConverter.GetExistingBuilding2DReferencesAsync(Npgsql.NpgsqlConnection?, IEnumerable{Building2DReference}?, bool, bool, int?, int, System.Threading.CancellationToken)"/>
        /// and its instance overload return null when inputs or connection are null, and return empty list when input collection is empty.
        /// </summary>
        [Fact]
        public async Task OrtoDatasPostgreSQLConverter_GetExistingBuilding2DReferencesAsync_NullOrEmpty_ReturnsExpected()
        {
            List<Building2DReference>? result_NullConn = await OrtoDatasPostgreSQLConverter.GetExistingBuilding2DReferencesAsync(null, [], inverted: false, fallbackByReference: true);
            Assert.Null(result_NullConn);

            List<Building2DReference>? result_NullList = await OrtoDatasPostgreSQLConverter.GetExistingBuilding2DReferencesAsync(null, null, inverted: false, fallbackByReference: true);
            Assert.Null(result_NullList);

            OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(null);
            List<Building2DReference>? result_InstanceNull = await ortoDatasPostgreSQLConverter.GetExistingBuilding2DReferencesAsync(null, inverted: false, fallbackByReference: true);
            Assert.Null(result_InstanceNull);

            List<Building2DReference>? result_InstanceEmpty = await ortoDatasPostgreSQLConverter.GetExistingBuilding2DReferencesAsync([], inverted: false, fallbackByReference: true);
            Assert.NotNull(result_InstanceEmpty);
            Assert.Empty(result_InstanceEmpty);
        }

        /// <summary>
        /// Verifies that <see cref="OrtoDatasPostgreSQLConverter.GetOrtoDatasByReferencesAsync(Npgsql.NpgsqlConnection?, IEnumerable{string}?, int?, bool, System.Threading.CancellationToken)"/>
        /// and its instance overload return null when inputs or connection are null, and return empty list when input collection is empty.
        /// </summary>
        [Fact]
        public async Task OrtoDatasPostgreSQLConverter_GetOrtoDatasByReferencesAsync_NullOrEmpty_ReturnsExpected()
        {
            List<OrtoDatas>? result_NullConn = await OrtoDatasPostgreSQLConverter.GetOrtoDatasByReferencesAsync(null, ["ref_1"], 10, fallbackByReference: true);
            Assert.Null(result_NullConn);

            List<OrtoDatas>? result_NullRefs = await OrtoDatasPostgreSQLConverter.GetOrtoDatasByReferencesAsync(null, null, 10, fallbackByReference: true);
            Assert.Null(result_NullRefs);

            OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(null);
            List<OrtoDatas>? result_InstanceNull = await ortoDatasPostgreSQLConverter.GetOrtoDatasByReferencesAsync(null, 10, fallbackByReference: true);
            Assert.Null(result_InstanceNull);
        }

        /// <summary>
        /// Verifies that <see cref="OrtoDatasPostgreSQLConverter.GetOrtoDatasByReferenceAsync(string, int?, bool, System.Threading.CancellationToken)"/>
        /// returns null when the reference is null or whitespace.
        /// </summary>
        [Fact]
        public async Task OrtoDatasPostgreSQLConverter_GetOrtoDatasByReferenceAsync_NullOrWhiteSpace_ReturnsNull()
        {
            OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(null);
            OrtoDatas? result_Null = await ortoDatasPostgreSQLConverter.GetOrtoDatasByReferenceAsync(null!, 10, fallbackByReference: true);
            Assert.Null(result_Null);

            OrtoDatas? result_WhiteSpace = await ortoDatasPostgreSQLConverter.GetOrtoDatasByReferenceAsync("   ", 10, fallbackByReference: true);
            Assert.Null(result_WhiteSpace);
        }

        /// <summary>
        /// Verifies that <see cref="OrtoDatasPostgreSQLConverter.GetOrtoDatasByBuilding2DReferencesAsync(Npgsql.NpgsqlConnection?, IEnumerable{Building2DReference}?, bool, System.Threading.CancellationToken)"/>
        /// and its instance overloads return null when inputs or connection are null, and return empty list when input collection is empty.
        /// </summary>
        [Fact]
        public async Task OrtoDatasPostgreSQLConverter_GetOrtoDatasByBuilding2DReferencesAsync_NullOrEmpty_ReturnsExpected()
        {
            List<OrtoDatas>? result_NullConn = await OrtoDatasPostgreSQLConverter.GetOrtoDatasByBuilding2DReferencesAsync(null, [new Building2DReference { Reference = "ref_1" }], fallbackByReference: true);
            Assert.Null(result_NullConn);

            List<OrtoDatas>? result_NullList = await OrtoDatasPostgreSQLConverter.GetOrtoDatasByBuilding2DReferencesAsync(null, null, fallbackByReference: true);
            Assert.Null(result_NullList);

            OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(null);
            List<OrtoDatas>? result_InstanceNull = await ortoDatasPostgreSQLConverter.GetOrtoDatasByBuilding2DReferencesAsync(null, fallbackByReference: true);
            Assert.Null(result_InstanceNull);

            OrtoDatas? result_SingleNull = await ortoDatasPostgreSQLConverter.GetOrtoDatasByBuilding2DReferenceAsync(null, fallbackByReference: true);
            Assert.Null(result_SingleNull);

            OrtoDatas? result_SingleEmptyRef = await ortoDatasPostgreSQLConverter.GetOrtoDatasByBuilding2DReferenceAsync(new Building2DReference { Reference = "  " }, fallbackByReference: true);
            Assert.Null(result_SingleEmptyRef);
        }

        /// <summary>
        /// Verifies that <see cref="OrtoDatasPostgreSQLConverter.ContainsByReferencesAsync(IEnumerable{string}, int?, bool, bool, System.Threading.CancellationToken)"/>
        /// returns null when input is null, and returns empty hash set when input collection is empty.
        /// </summary>
        [Fact]
        public async Task OrtoDatasPostgreSQLConverter_ContainsByReferencesAsync_NullOrEmpty_ReturnsExpected()
        {
            OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(null);
            HashSet<string>? result_Null = await ortoDatasPostgreSQLConverter.ContainsByReferencesAsync(null!, 10, inverted: false, fallbackByReference: true);
            Assert.Null(result_Null);

            HashSet<string>? result_Empty = await ortoDatasPostgreSQLConverter.ContainsByReferencesAsync([], 10, inverted: false, fallbackByReference: true);
            Assert.NotNull(result_Empty);
            Assert.Empty(result_Empty);
        }

        /// <summary>
        /// Verifies that <see cref="OrtoDatasPostgreSQLConverter.UpdateSubdivisionIdsAsync(IEnumerable{Building2DReference}?, bool, int?, int, System.Threading.CancellationToken)"/>
        /// returns null when input is null, and returns empty list when input collection is empty.
        /// </summary>
        [Fact]
        public async Task OrtoDatasPostgreSQLConverter_UpdateSubdivisionIdsAsync_NullOrEmpty_ReturnsExpected()
        {
            OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(null);
            List<Building2DReference>? result_Null = await ortoDatasPostgreSQLConverter.UpdateSubdivisionIdsAsync(null, fallbackByReference: true);
            Assert.Null(result_Null);

            List<Building2DReference>? result_Empty = await ortoDatasPostgreSQLConverter.UpdateSubdivisionIdsAsync([], fallbackByReference: true);
            Assert.NotNull(result_Empty);
            Assert.Empty(result_Empty);
        }

        /// <summary>
        /// Verifies that a reference carrying no subdivision identifier never reaches the database.
        /// <para>A null subdivision means the building's subdivision has not been resolved yet, not that it has none. Writing it through cleared subdivisions that an earlier run had resolved - the defect issue #23 fixed on <c>building_2d</c> and issue #31 on this table - so the entries are dropped before the statement is built.</para>
        /// <para>The exclusion is observable with no database at all. The converter here has no connection data, so anything that reaches the connection answers null; a batch that is entirely subdivision-less short circuits to an empty list before that point, and a batch holding even one resolved subdivision does not.</para>
        /// </summary>
        [Fact]
        public async Task OrtoDatasPostgreSQLConverter_UpdateSubdivisionIdsAsync_NoSubdivisionId_IsExcluded()
        {
            OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(null);

            List<Building2DReference> building2DReferences_Unresolved =
            [
                new Building2DReference { CountyId = 55417, Reference = "reference_1", SubdivisionId = null },
                new Building2DReference { CountyId = 55417, Reference = "reference_2", SubdivisionId = null }
            ];

            List<Building2DReference>? result_Unresolved = await ortoDatasPostgreSQLConverter.UpdateSubdivisionIdsAsync(building2DReferences_Unresolved);
            Assert.NotNull(result_Unresolved);
            Assert.Empty(result_Unresolved);

            List<Building2DReference> building2DReferences_Mixed =
            [
                new Building2DReference { CountyId = 55417, Reference = "reference_1", SubdivisionId = null },
                new Building2DReference { CountyId = 55417, Reference = "reference_2", SubdivisionId = 3064 }
            ];

            List<Building2DReference>? result_Mixed = await ortoDatasPostgreSQLConverter.UpdateSubdivisionIdsAsync(building2DReferences_Mixed);
            Assert.Null(result_Mixed);
        }

        /// <summary>
        /// Verifies that <see cref="OrtoDatasPostgreSQLConverter.GetOrtoDatasReferencesByReferencesAsync(Npgsql.NpgsqlConnection?, IEnumerable{string}?, int?, bool, System.Threading.CancellationToken)"/>
        /// and its instance overload return null when inputs or connection are null.
        /// </summary>
        [Fact]
        public async Task OrtoDatasPostgreSQLConverter_GetOrtoDatasReferencesByReferencesAsync_NullOrEmpty_ReturnsExpected()
        {
            List<OrtoDatasReference>? result_NullConn = await OrtoDatasPostgreSQLConverter.GetOrtoDatasReferencesByReferencesAsync(null, ["ref_1"], 10, fallbackByReference: true);
            Assert.Null(result_NullConn);

            List<OrtoDatasReference>? result_NullRefs = await OrtoDatasPostgreSQLConverter.GetOrtoDatasReferencesByReferencesAsync(null, null, 10, fallbackByReference: true);
            Assert.Null(result_NullRefs);

            OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(null);
            List<OrtoDatasReference>? result_InstanceNull = await ortoDatasPostgreSQLConverter.GetOrtoDatasReferencesByReferencesAsync(null, 10, fallbackByReference: true);
            Assert.Null(result_InstanceNull);
        }

        /// <summary>
        /// Verifies that <see cref="OrtoDatasPostgreSQLConverter.GetOrtoDatasReferenceByReferenceAsync(string, int?, bool, System.Threading.CancellationToken)"/>
        /// returns null when the reference is null or whitespace.
        /// </summary>
        [Fact]
        public async Task OrtoDatasPostgreSQLConverter_GetOrtoDatasReferenceByReferenceAsync_NullOrWhiteSpace_ReturnsNull()
        {
            OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(null);
            OrtoDatasReference? result_Null = await ortoDatasPostgreSQLConverter.GetOrtoDatasReferenceByReferenceAsync(null!, 10, fallbackByReference: true);
            Assert.Null(result_Null);

            OrtoDatasReference? result_WhiteSpace = await ortoDatasPostgreSQLConverter.GetOrtoDatasReferenceByReferenceAsync("   ", 10, fallbackByReference: true);
            Assert.Null(result_WhiteSpace);
        }

        /// <summary>
        /// Verifies that <see cref="OrtoDatasPostgreSQLConverter.GetOrtoDatasReferencesByBuilding2DReferencesAsync(Npgsql.NpgsqlConnection?, IEnumerable{Building2DReference}?, bool, System.Threading.CancellationToken)"/>
        /// and its instance overloads return null when inputs or connection are null.
        /// </summary>
        [Fact]
        public async Task OrtoDatasPostgreSQLConverter_GetOrtoDatasReferencesByBuilding2DReferencesAsync_NullOrEmpty_ReturnsExpected()
        {
            List<OrtoDatasReference>? result_NullConn = await OrtoDatasPostgreSQLConverter.GetOrtoDatasReferencesByBuilding2DReferencesAsync(null, [new Building2DReference { Reference = "ref_1" }], fallbackByReference: true);
            Assert.Null(result_NullConn);

            List<OrtoDatasReference>? result_NullList = await OrtoDatasPostgreSQLConverter.GetOrtoDatasReferencesByBuilding2DReferencesAsync(null, null, fallbackByReference: true);
            Assert.Null(result_NullList);

            OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(null);
            List<OrtoDatasReference>? result_InstanceNull = await ortoDatasPostgreSQLConverter.GetOrtoDatasReferencesByBuilding2DReferencesAsync(null, fallbackByReference: true);
            Assert.Null(result_InstanceNull);

            OrtoDatasReference? result_SingleNull = await ortoDatasPostgreSQLConverter.GetOrtoDatasReferenceByBuilding2DReferenceAsync(null, fallbackByReference: true);
            Assert.Null(result_SingleNull);

            OrtoDatasReference? result_SingleEmptyRef = await ortoDatasPostgreSQLConverter.GetOrtoDatasReferenceByBuilding2DReferenceAsync(new Building2DReference { Reference = "  " }, fallbackByReference: true);
            Assert.Null(result_SingleEmptyRef);
        }

        /// <summary>
        /// Verifies that <see cref="OrtoDatasPostgreSQLConverter.GetOrtoDatasReferencesByCountyIdAsync(NpgsqlConnection?, int, IEnumerable{int}?, CancellationToken)"/>
        /// and <see cref="OrtoDatasPostgreSQLConverter.GetOrtoDatasReferenceByIdAsync(long, int?, CancellationToken)"/>
        /// return null when connection is null.
        /// </summary>
        [Fact]
        public async Task OrtoDatasPostgreSQLConverter_GetOrtoDatasReferencesByCountyIdAndIdAsync_NullConnection_ReturnsNull()
        {
            List<OrtoDatasReference>? result_NullCounty = await OrtoDatasPostgreSQLConverter.GetOrtoDatasReferencesByCountyIdAsync(null, 55417);
            Assert.Null(result_NullCounty);

            OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(null);
            List<OrtoDatasReference>? result_InstanceNullCounty = await ortoDatasPostgreSQLConverter.GetOrtoDatasReferencesByCountyIdAsync(55417);
            Assert.Null(result_InstanceNullCounty);

            OrtoDatasReference? result_InstanceNullId = await ortoDatasPostgreSQLConverter.GetOrtoDatasReferenceByIdAsync(12345);
            Assert.Null(result_InstanceNullId);
        }
    }
}
