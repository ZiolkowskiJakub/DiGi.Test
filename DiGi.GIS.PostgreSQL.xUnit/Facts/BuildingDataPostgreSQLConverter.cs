using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="BuildingDataPostgreSQLConverter.PullAsync(Npgsql.NpgsqlConnection?, IEnumerable{string}, int?, IEnumerable{string}?, int, bool)"/>
        /// and its instance overload return null when inputs or connection are null or empty.
        /// </summary>
        [Fact]
        public async Task BuildingDataPostgreSQLConverter_PullAsync_NullOrEmpty_ReturnsNull()
        {
            BuildingDataPostgreSQLConverter buildingDataPostgreSQLConverter = new(null);

            Table? result_NullConn = await buildingDataPostgreSQLConverter.PullAsync(null, ["ref_1"], 10, fallbackByReference: true);
            Assert.Null(result_NullConn);

            Table? result_NullRefs = await buildingDataPostgreSQLConverter.PullAsync(null, null!, 10, fallbackByReference: true);
            Assert.Null(result_NullRefs);

            Table? result_EmptyRefs = await buildingDataPostgreSQLConverter.PullAsync(null, [], 10, fallbackByReference: true);
            Assert.Null(result_EmptyRefs);

            Table? result_InstanceNullRefs = await buildingDataPostgreSQLConverter.PullAsync((IEnumerable<string>)null!, 10, fallbackByReference: true);
            Assert.Null(result_InstanceNullRefs);

            Table? result_InstanceEmptyRefs = await buildingDataPostgreSQLConverter.PullAsync([], 10, fallbackByReference: true);
            Assert.Null(result_InstanceEmptyRefs);
        }
    }
}
