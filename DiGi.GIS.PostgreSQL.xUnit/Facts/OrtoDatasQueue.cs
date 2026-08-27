using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies what the queue answers when it cannot be reached at all, which is the distinction the download task turns into success or failure.
        /// <para>A drained queue answers an empty list and a queue that could not be reached answers null, and the two must not be confused: the task treats null as an outright failure and an empty list as an ordinary idle run. Reporting a broken claim as "nothing to do" is how a deployment fault reads as a quiet success.</para>
        /// </summary>
        [Fact]
        public async Task GetNextBuilding2DReferencesAsync_NoConnection_ReturnsNull()
        {
            OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(null);

            List<Building2DReference>? result_NoConnection = await ortoDatasPostgreSQLConverter.GetNextBuilding2DReferencesAsync(5);
            Assert.Null(result_NoConnection);

            // Asking for nothing is a caller error rather than an empty queue, and is refused before any
            // connection is attempted.
            List<Building2DReference>? result_ZeroCount = await OrtoDatasPostgreSQLConverter.GetNextBuilding2DReferencesAsync(null, 0);
            Assert.Null(result_ZeroCount);

            List<Building2DReference>? result_NegativeCount = await OrtoDatasPostgreSQLConverter.GetNextBuilding2DReferencesAsync(null, -1);
            Assert.Null(result_NegativeCount);

            List<Building2DReference>? result_ZeroMaxAttempts = await OrtoDatasPostgreSQLConverter.GetNextBuilding2DReferencesAsync(null, 5, 30, 0);
            Assert.Null(result_ZeroMaxAttempts);

            List<Building2DReference>? result_NegativeMaxAttempts = await OrtoDatasPostgreSQLConverter.GetNextBuilding2DReferencesAsync(null, 5, 30, -1);
            Assert.Null(result_NegativeMaxAttempts);
        }

        /// <summary>
        /// Verifies that acknowledgment reports a failure rather than a count when it cannot reach the queue.
        /// <para>The return is a count of rows retired, so zero has to mean "nothing matched" and never "the queue could not be reached". A caller that read a failure as zero would conclude the batch had already been retired by somebody else and move on, which is the one reading that loses work.</para>
        /// <para>Every path here answers -1, the empty set included: the connection is checked before the identifiers are, so with nothing to connect through there is no case that reaches the zero. The zero belongs to a reachable queue that matched nothing, and is asserted by the integration fact.</para>
        /// </summary>
        [Fact]
        public async Task AcknowledgeBuilding2DReferencesAsync_NoConnection_ReturnsFailure()
        {
            OrtoDatasPostgreSQLConverter ortoDatasPostgreSQLConverter = new(null);

            Assert.Equal(-1, await ortoDatasPostgreSQLConverter.AcknowledgeBuilding2DReferencesAsync(null));
            Assert.Equal(-1, await ortoDatasPostgreSQLConverter.AcknowledgeBuilding2DReferencesAsync([1L, 2L]));
            Assert.Equal(-1, await ortoDatasPostgreSQLConverter.AcknowledgeBuilding2DReferencesAsync([]));

            Assert.Equal(-1, await OrtoDatasPostgreSQLConverter.AcknowledgeBuilding2DReferencesAsync(null, [1L]));
        }
    }
}
