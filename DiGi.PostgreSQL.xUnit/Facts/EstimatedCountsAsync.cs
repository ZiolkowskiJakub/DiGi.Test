using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the guard contract of <see cref="Query.EstimatedCountsAsync(NpgsqlConnection?, IEnumerable{string}?, bool, int, int, System.Threading.CancellationToken)"/>.
        /// <para>Every case here is decided before a statement is built, so none of them needs a database or an open connection.</para>
        /// <para>The distinction being pinned is that a name with no relation behind it is <b>absent</b> from the dictionary rather than present with a zero. A caller has to be able to tell "this partition does not exist" from "this partition holds no rows", because the estimated coverage factors divide one such figure by another.</para>
        /// </summary>
        [Fact]
        public async Task EstimatedCountsAsync_Guards()
        {
            // No connection - null regardless of what is asked for.
            Assert.Null(await Query.EstimatedCountsAsync(null, ["building_2d_5"]));
            Assert.Null(await Query.EstimatedCountsAsync(null, null));

            // The connection is never opened below: each assertion returns before any command is created.
            await using NpgsqlConnection npgsqlConnection = new();

            // No names - null, matching the no-connection answer rather than an empty dictionary.
            Assert.Null(await Query.EstimatedCountsAsync(npgsqlConnection, null));

            // An empty request is a valid request with nothing in it, so it answers an empty dictionary.
            Dictionary<string, long>? counts_Empty = await Query.EstimatedCountsAsync(npgsqlConnection, []);
            Assert.NotNull(counts_Empty);
            Assert.Empty(counts_Empty);

            // Blank names are filtered out, and filtering every name leaves the empty request above.
            Dictionary<string, long>? counts_Blank = await Query.EstimatedCountsAsync(npgsqlConnection, ["", "   ", "\t"]);
            Assert.NotNull(counts_Blank);
            Assert.Empty(counts_Blank);
        }
    }
}
