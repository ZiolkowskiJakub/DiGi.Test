using System;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the automatic-resume count and stall-limit validation shared by the dialog and the task's preflight: 0 to 10 automatic resumes with an empty or at least one minute stall limit is accepted, and a negative or above-ten count, or a limit under a minute, is refused with a reason.
        /// </summary>
        [Fact]
        public void IsYOLOTrainingStallOptionsValid()
        {
            Assert.True(Query.IsYOLOTrainingStallOptionsValid(0, null, out string? reason_NoRetries));
            Assert.Null(reason_NoRetries);

            Assert.True(Query.IsYOLOTrainingStallOptionsValid(3, TimeSpan.FromMinutes(15), out string? reason_Default));
            Assert.Null(reason_Default);

            Assert.True(Query.IsYOLOTrainingStallOptionsValid(10, TimeSpan.FromMinutes(1), out _));

            Assert.False(Query.IsYOLOTrainingStallOptionsValid(-1, null, out string? reason_Negative));
            Assert.NotNull(reason_Negative);

            Assert.False(Query.IsYOLOTrainingStallOptionsValid(11, null, out string? reason_Above));
            Assert.NotNull(reason_Above);

            Assert.False(Query.IsYOLOTrainingStallOptionsValid(3, TimeSpan.Zero, out string? reason_Zero));
            Assert.NotNull(reason_Zero);

            Assert.False(Query.IsYOLOTrainingStallOptionsValid(3, TimeSpan.FromSeconds(30), out string? reason_Short));
            Assert.NotNull(reason_Short);
        }
    }
}
