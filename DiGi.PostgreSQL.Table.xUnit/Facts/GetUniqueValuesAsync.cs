using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Table.xUnit.Classes;
using Npgsql;

namespace DiGi.PostgreSQL.Table.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// A target column the stored table does not hold is a typed failure naming the column in <c>GetUniqueValuesAsync</c> too, distinct from a valid empty result (DiGi-Organization/DiGi.PostgreSQL#12). On the unmodified code the same call answered <c>null</c>, indistinguishable from an empty partition.
        /// <para>A blank column name is likewise an invalid request: both overloads throw <c>InvalidFilterException</c> instead of folding it into the connection check.</para>
        /// <para>Figures describe the development database resolved from <c>user files/PostgreSQL_Table.conf</c> (localhost), never production.</para>
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SkippableFact]
        public async Task GetUniqueValuesAsync_UnknownTargetColumn_Throws()
        {
            if (!PostgreSQL.xUnit.Create.IsAvailable(PostgreSQL.Enums.StorageMethod.Table, out ConnectionData? connectionData))
            {
                return;
            }

            BaseTablePostgreSQLConverter testConverter = new(connectionData);

            // A scratch table with its column metadata exists (so the stored-column whitelist can be consulted); the requested column is simply not on it.
            await SeedHistogramTableAsync(connectionData, testConverter, [1.0]);
            try
            {
                await using NpgsqlConnection? npgsqlConnection = PostgreSQL.Create.NpgsqlConnection(connectionData);
                if (npgsqlConnection is null)
                {
                    return;
                }

                await npgsqlConnection.OpenAsync();

                DiGi.PostgreSQL.Table.Classes.InvalidFilterException exception = await Assert.ThrowsAsync<DiGi.PostgreSQL.Table.Classes.InvalidFilterException>(() => testConverter.GetUniqueValuesAsync<Core.IO.Table.Classes.Column>(npgsqlConnection, "no_such_column"));

                Assert.Equal("no_such_column", exception.ColumnUniqueId);
                Assert.Null(exception.FilterOperator);

                // A blank column name is an invalid request too, in both overloads - it must not fold into the connection check.
                DiGi.PostgreSQL.Table.Classes.InvalidFilterException exception_Blank = await Assert.ThrowsAsync<DiGi.PostgreSQL.Table.Classes.InvalidFilterException>(() => testConverter.GetUniqueValuesAsync<Core.IO.Table.Classes.Column>(npgsqlConnection, "   "));
                Assert.Equal("   ", exception_Blank.ColumnUniqueId);

                DiGi.PostgreSQL.Table.Classes.InvalidFilterException exception_Null = await Assert.ThrowsAsync<DiGi.PostgreSQL.Table.Classes.InvalidFilterException>(() => testConverter.GetUniqueValuesAsync<Core.IO.Table.Classes.Column>(npgsqlConnection, null));
                Assert.Equal(string.Empty, exception_Null.ColumnUniqueId);

                DiGi.PostgreSQL.Table.Classes.InvalidFilterException exception_BlankOuter = await Assert.ThrowsAsync<DiGi.PostgreSQL.Table.Classes.InvalidFilterException>(() => testConverter.GetUniqueValuesAsync<Core.IO.Table.Classes.Column>("   "));
                Assert.Equal("   ", exception_BlankOuter.ColumnUniqueId);
            }
            finally
            {
                await UnseedHistogramTableAsync(connectionData, testConverter);
            }
        }
    }
}
