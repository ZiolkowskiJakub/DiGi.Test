using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="OrtoDatasPostgreSQLConverter.GetExistingBuilding2DReferencesAsync(Npgsql.NpgsqlConnection?, IEnumerable{Building2DReference}?, bool, bool, System.Threading.CancellationToken)"/>
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
        /// Verifies that <see cref="OrtoDatasPostgreSQLConverter.UpdateSubdivisionIds(IEnumerable{Building2DReference}?, bool, System.Threading.CancellationToken)"/>
        /// returns null when input is null, and returns empty list when input collection is empty.
        /// </summary>
        [Fact]
        public async Task OrtoDatasPostgreSQLConverter_UpdateSubdivisionIds_NullOrEmpty_ReturnsExpected()
        {
            OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(null);
            List<Building2DReference>? result_Null = await ortoDatasPostgreSQLConverter.UpdateSubdivisionIds(null, fallbackByReference: true);
            Assert.Null(result_Null);

            List<Building2DReference>? result_Empty = await ortoDatasPostgreSQLConverter.UpdateSubdivisionIds([], fallbackByReference: true);
            Assert.NotNull(result_Empty);
            Assert.Empty(result_Empty);
        }
    }
}
