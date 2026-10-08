namespace DiGi.GIS.ML.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies the refusal boundary of the plausibility guard: a share of predictions later than their first confident detection year is accepted exactly up to the maximum and refused above it.
        /// <para>While the plausibility cap holds, no input can drive the run's measured share off zero, so the end-to-end facts cannot exercise the refusal. This boundary is the guard shown to fail: the condition flips at the constant, and a change from a strict to a non-strict comparison - or a silently moved threshold - fails here.</para>
        /// <para>Short test (0.0 s): runs on every duration setting.</para>
        /// </summary>
        [Fact]
        public void IsPlausibleShare_Boundary()
        {
            double share_None = 0D;
            Assert.True(Query.IsPlausibleShare(share_None));

            // Exactly at the maximum: still acceptable. The guard refuses only a share over it.
            double share_Maximum = Constants.Plausibility.MaximumImplausibleShare;
            Assert.True(Query.IsPlausibleShare(share_Maximum));

            // One part per billion over the maximum: refused.
            double share_Over = share_Maximum + 1e-9;
            Assert.False(Query.IsPlausibleShare(share_Over));

            double share_All = 1D;
            Assert.False(Query.IsPlausibleShare(share_All));
        }
    }
}
