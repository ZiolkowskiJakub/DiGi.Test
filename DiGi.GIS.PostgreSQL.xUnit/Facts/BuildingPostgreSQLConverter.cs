using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="BuildingPostgreSQLConverter.GetBuildingsByReferencesAsync(Npgsql.NpgsqlConnection?, IEnumerable{string}?, int?, bool, System.Threading.CancellationToken)"/>
        /// and its instance overload return null when inputs or connection are null, and return empty list when input collection is empty.
        /// </summary>
        [Fact]
        public async Task BuildingPostgreSQLConverter_GetBuildingsByReferencesAsync_NullOrEmpty_ReturnsExpected()
        {
            List<Building>? result_NullConn = await BuildingPostgreSQLConverter.GetBuildingsByReferencesAsync(null, ["ref_1"], 10, fallbackByReference: true);
            Assert.Null(result_NullConn);

            List<Building>? result_NullRefs = await BuildingPostgreSQLConverter.GetBuildingsByReferencesAsync(null, null, 10, fallbackByReference: true);
            Assert.Null(result_NullRefs);

            BuildingPostgreSQLConverter buildingPostgreSQLConverter = new(null);
            List<Building>? result_InstanceNull = await buildingPostgreSQLConverter.GetBuildingsByReferencesAsync(null, 10, fallbackByReference: true);
            Assert.Null(result_InstanceNull);
        }

        /// <summary>
        /// Verifies that <see cref="BuildingPostgreSQLConverter.GetBuildingsByReferenceAsync(string, int?, bool, System.Threading.CancellationToken)"/>
        /// returns null when the reference is null or whitespace.
        /// </summary>
        [Fact]
        public async Task BuildingPostgreSQLConverter_GetBuildingsByReferenceAsync_NullOrWhiteSpace_ReturnsNull()
        {
            BuildingPostgreSQLConverter buildingPostgreSQLConverter = new(null);
            List<Building>? result_Null = await buildingPostgreSQLConverter.GetBuildingsByReferenceAsync(null!, 10, fallbackByReference: true);
            Assert.Null(result_Null);

            List<Building>? result_WhiteSpace = await buildingPostgreSQLConverter.GetBuildingsByReferenceAsync("   ", 10, fallbackByReference: true);
            Assert.Null(result_WhiteSpace);
        }

        /// <summary>
        /// Verifies that <see cref="BuildingPostgreSQLConverter.GetBuildingByReferenceAsync(string, int?, Geometry.Spatial.Classes.Point3D?, double, double, bool, System.Threading.CancellationToken)"/>
        /// returns null when the reference is null or whitespace.
        /// </summary>
        [Fact]
        public async Task BuildingPostgreSQLConverter_GetBuildingByReferenceAsync_NullOrWhiteSpace_ReturnsNull()
        {
            BuildingPostgreSQLConverter buildingPostgreSQLConverter = new(null);
            Building? result_Null = await buildingPostgreSQLConverter.GetBuildingByReferenceAsync(null!, 10, null, fallbackByReference: true);
            Assert.Null(result_Null);

            Building? result_WhiteSpace = await buildingPostgreSQLConverter.GetBuildingByReferenceAsync("   ", 10, null, fallbackByReference: true);
            Assert.Null(result_WhiteSpace);
        }

        /// <summary>
        /// Verifies that <see cref="BuildingPostgreSQLConverter.ContainsByReferencesAsync(IEnumerable{string}, int?, bool, bool, System.Threading.CancellationToken)"/>
        /// returns null when input is null, and returns empty hash set when input collection is empty.
        /// </summary>
        [Fact]
        public async Task BuildingPostgreSQLConverter_ContainsByReferencesAsync_NullOrEmpty_ReturnsExpected()
        {
            BuildingPostgreSQLConverter buildingPostgreSQLConverter = new(null);
            HashSet<string>? result_Null = await buildingPostgreSQLConverter.ContainsByReferencesAsync(null!, 10, inverted: false, fallbackByReference: true);
            Assert.Null(result_Null);

            HashSet<string>? result_Empty = await buildingPostgreSQLConverter.ContainsByReferencesAsync([], 10, inverted: false, fallbackByReference: true);
            Assert.NotNull(result_Empty);
            Assert.Empty(result_Empty);
        }
    }
}
