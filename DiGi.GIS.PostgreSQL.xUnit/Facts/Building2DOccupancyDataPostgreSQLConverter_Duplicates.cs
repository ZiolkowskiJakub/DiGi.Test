using DiGi.GIS.PostgreSQL.Classes;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Building2DReferencedObjectPostgreSQLConverter{T1, T2}.GetBuilding2DReferenceDuplicatesAsync(NpgsqlConnection, System.Nullable{int}, int, int, System.Threading.CancellationToken)"/>
        /// and its parameterless overload return null when given invalid parameters or null connections.
        /// </summary>
        [Fact]
        public async Task GetBuilding2DReferenceDuplicatesAsync_Guards()
        {
            Building2DOccupancyDataPostgreSQLConverter building2DOccupancyDataPostgreSQLConverter = new(null);

            List<Building2DReferenceDuplicate>? duplicates_NullConnection = await building2DOccupancyDataPostgreSQLConverter.GetBuilding2DReferenceDuplicatesAsync((NpgsqlConnection?)null);
            Assert.Null(duplicates_NullConnection);

            List<Building2DReferenceDuplicate>? duplicates_NullConnectionData = await building2DOccupancyDataPostgreSQLConverter.GetBuilding2DReferenceDuplicatesAsync(countyId: null);
            Assert.Null(duplicates_NullConnectionData);

            List<Building2DReferenceDuplicate>? duplicates_InvalidLimit = await building2DOccupancyDataPostgreSQLConverter.GetBuilding2DReferenceDuplicatesAsync(countyId: null, limit: 0);
            Assert.Null(duplicates_InvalidLimit);

            List<Building2DReferenceDuplicate>? duplicates_NegativeTimeout = await building2DOccupancyDataPostgreSQLConverter.GetBuilding2DReferenceDuplicatesAsync(countyId: null, commandTimeout: -1);
            Assert.Null(duplicates_NegativeTimeout);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DReferencedObjectPostgreSQLConverter{T1, T2}.GetDuplicatesCountAsync(NpgsqlConnection, System.Nullable{int}, int, System.Threading.CancellationToken)"/>
        /// and its parameterless overload return -1 when given invalid parameters or null connections.
        /// </summary>
        [Fact]
        public async Task GetDuplicatesCountAsync_Guards()
        {
            Building2DOccupancyDataPostgreSQLConverter building2DOccupancyDataPostgreSQLConverter = new(null);

            long count_NullConnection = await building2DOccupancyDataPostgreSQLConverter.GetDuplicatesCountAsync((NpgsqlConnection?)null);
            Assert.Equal(-1, count_NullConnection);

            long count_NullConnectionData = await building2DOccupancyDataPostgreSQLConverter.GetDuplicatesCountAsync(countyId: null);
            Assert.Equal(-1, count_NullConnectionData);

            long count_NegativeTimeout = await building2DOccupancyDataPostgreSQLConverter.GetDuplicatesCountAsync(countyId: null, commandTimeout: -1);
            Assert.Equal(-1, count_NegativeTimeout);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DReferencedObjectPostgreSQLConverter{T1, T2}.GetBuilding2DReferenceDuplicatesAsync(System.Nullable{int}, int, int, System.Threading.CancellationToken)"/>
        /// executes successfully against a live database with explicit command timeout.
        /// <para>Skipped by default: it executes an integration query requiring <c>GIS_PostgreSQL_Storage.conf</c> pointing at a database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Storage.conf at a database before running.")]
        public async Task GetBuilding2DReferenceDuplicatesAsync_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            Building2DOccupancyDataPostgreSQLConverter? building2DOccupancyDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DOccupancyDataPostgreSQLConverter>();
            Assert.NotNull(building2DOccupancyDataPostgreSQLConverter);

            List<Building2DReferenceDuplicate>? duplicates = await building2DOccupancyDataPostgreSQLConverter.GetBuilding2DReferenceDuplicatesAsync(countyId: null, limit: 10, commandTimeout: 600);
            Assert.NotNull(duplicates);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DReferencedObjectPostgreSQLConverter{T1, T2}.GetDuplicatesCountAsync(System.Nullable{int}, int, System.Threading.CancellationToken)"/>
        /// executes successfully against a live database with explicit command timeout.
        /// <para>Skipped by default: it executes an integration query requiring <c>GIS_PostgreSQL_Storage.conf</c> pointing at a database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Storage.conf at a database before running.")]
        public async Task GetDuplicatesCountAsync_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            Building2DOccupancyDataPostgreSQLConverter? building2DOccupancyDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DOccupancyDataPostgreSQLConverter>();
            Assert.NotNull(building2DOccupancyDataPostgreSQLConverter);

            long count = await building2DOccupancyDataPostgreSQLConverter.GetDuplicatesCountAsync(countyId: null, commandTimeout: 600);
            Assert.True(count >= 0);
        }
    }
}
