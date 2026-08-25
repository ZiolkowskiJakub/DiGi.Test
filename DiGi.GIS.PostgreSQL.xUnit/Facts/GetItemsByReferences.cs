using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Building2DReferencedObjectPostgreSQLConverter{TBuilding2DReferencedObject, TUniqueObject}.GetItemByReferenceAsync(string, int?, bool, System.Threading.CancellationToken)"/>
        /// returns null when given a null or whitespace reference.
        /// </summary>
        [Fact]
        public async Task GetItemByReferenceAsync_NullOrWhiteSpace_ReturnsNull()
        {
            Building2DOccupancyDataPostgreSQLConverter building2DOccupancyDataPostgreSQLConverter = new(null);
            Building2DOccupancyData? result_Null = await building2DOccupancyDataPostgreSQLConverter.GetItemByReferenceAsync(null!, 5, fallbackByReference: true);
            Assert.Null(result_Null);

            Building2DOccupancyData? result_WhiteSpace = await building2DOccupancyDataPostgreSQLConverter.GetItemByReferenceAsync("   ", 5, fallbackByReference: true);
            Assert.Null(result_WhiteSpace);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DReferencedObjectPostgreSQLConverter{TBuilding2DReferencedObject, TUniqueObject}.GetItemsByReferenceAsync(string, int?, long?, bool, System.Threading.CancellationToken)"/>
        /// returns null when given a null or whitespace reference.
        /// </summary>
        [Fact]
        public async Task GetItemsByReferenceAsync_NullOrWhiteSpace_ReturnsNull()
        {
            Building2DOccupancyDataPostgreSQLConverter building2DOccupancyDataPostgreSQLConverter = new(null);
            List<Building2DOccupancyData>? result_Null = await building2DOccupancyDataPostgreSQLConverter.GetItemsByReferenceAsync(null!, 5, fallbackByReference: true);
            Assert.Null(result_Null);

            List<Building2DOccupancyData>? result_WhiteSpace = await building2DOccupancyDataPostgreSQLConverter.GetItemsByReferenceAsync("   ", 5, fallbackByReference: true);
            Assert.Null(result_WhiteSpace);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DReferencedObjectPostgreSQLConverter{TBuilding2DReferencedObject, TUniqueObject}.GetItemsByReferencesAsync(IEnumerable{string}?, int?, long?, bool, int, System.Threading.CancellationToken)"/>
        /// returns null when given null references.
        /// </summary>
        [Fact]
        public async Task GetItemsByReferencesAsync_NullReferences_ReturnsNull()
        {
            Building2DOccupancyDataPostgreSQLConverter building2DOccupancyDataPostgreSQLConverter = new(null);
            List<Building2DOccupancyData>? result = await building2DOccupancyDataPostgreSQLConverter.GetItemsByReferencesAsync((IEnumerable<string>?)null, 5, fallbackByReference: true);
            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DReferencedObjectPostgreSQLConverter{TBuilding2DReferencedObject, TUniqueObject}.GetItemsByReferencesAsync(Npgsql.NpgsqlConnection?, IEnumerable{string}?, int?, long?, bool, int, System.Threading.CancellationToken)"/>
        /// returns null when connection is null.
        /// </summary>
        [Fact]
        public async Task GetItemsByReferencesAsync_NullConnection_ReturnsNull()
        {
            Building2DOccupancyDataPostgreSQLConverter building2DOccupancyDataPostgreSQLConverter = new(null);
            List<Building2DOccupancyData>? result = await building2DOccupancyDataPostgreSQLConverter.GetItemsByReferencesAsync(null, [], 5, fallbackByReference: true);
            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DReferencedObjectPostgreSQLConverter{TBuilding2DReferencedObject, TUniqueObject}.GetUniqueIdsByReferencesAsync(IEnumerable{string}?, int?, bool, System.Threading.CancellationToken)"/>
        /// returns null when given null references.
        /// </summary>
        [Fact]
        public async Task GetUniqueIdsByReferencesAsync_NullReferences_ReturnsNull()
        {
            Building2DOccupancyDataPostgreSQLConverter building2DOccupancyDataPostgreSQLConverter = new(null);
            HashSet<string>? result = await building2DOccupancyDataPostgreSQLConverter.GetUniqueIdsByReferencesAsync((IEnumerable<string>?)null, 5, fallbackByReference: true);
            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DReferencedObjectPostgreSQLConverter{TBuilding2DReferencedObject, TUniqueObject}.GetUniqueIdsByReferencesAsync(Npgsql.NpgsqlConnection?, IEnumerable{string}?, int?, bool, System.Threading.CancellationToken)"/>
        /// returns null when connection is null.
        /// </summary>
        [Fact]
        public async Task GetUniqueIdsByReferencesAsync_NullConnection_ReturnsNull()
        {
            Building2DOccupancyDataPostgreSQLConverter building2DOccupancyDataPostgreSQLConverter = new(null);
            HashSet<string>? result = await building2DOccupancyDataPostgreSQLConverter.GetUniqueIdsByReferencesAsync(null, [], 5, fallbackByReference: true);
            Assert.Null(result);
        }
    }
}
