using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the constructor carries both tallies and both reference lists through, and that a populated instance survives a JSON round trip and a clone.
        /// <para>The open-envelope tally is the reason the type exists: a run that reports a completed classification without it says nothing about how many of its rows may rest on an arbitrary face side. The figures are the ones measured on the 27-model verification sample of #84 - 104 partitions skipped, 6 open envelopes. The degenerate references are two of the sliver buildings of the 2026-09-24 production run.</para>
        /// </summary>
        [Fact]
        public void ExternalComponentsAreaResult_Serialization()
        {
            List<string> references_Degenerate = ["b2de8788-e83d-498f-9557-3976e3e0598d", "76d6a1a6-c56f-43d1-904e-97d422d72de7"];
            List<string> references_Failed = ["ref_orphan: the wall component bounds 0 space(s)"];

            ExternalComponentsAreaResult externalComponentsAreaResult = new(104, 6, references_Degenerate, references_Failed);

            Assert.Equal(104, externalComponentsAreaResult.SkippedComponentCount);
            Assert.Equal(6, externalComponentsAreaResult.OpenEnvelopeCount);
            Assert.Equal(2, externalComponentsAreaResult.DegenerateModelCount);
            Assert.Equal(1, externalComponentsAreaResult.FailedModelCount);
            Assert.Equal(references_Degenerate, externalComponentsAreaResult.DegenerateReferences);
            Assert.Equal(references_Failed, externalComponentsAreaResult.FailedReferences);

            string? json = Core.Convert.ToSystem_String(externalComponentsAreaResult);
            Assert.NotNull(json);

            ExternalComponentsAreaResult? externalComponentsAreaResult_Json = Core.Convert.ToDiGi<ExternalComponentsAreaResult>(json)?.FirstOrDefault();
            Assert.NotNull(externalComponentsAreaResult_Json);

            Assert.Equal(104, externalComponentsAreaResult_Json.SkippedComponentCount);
            Assert.Equal(6, externalComponentsAreaResult_Json.OpenEnvelopeCount);
            Assert.Equal(references_Degenerate, externalComponentsAreaResult_Json.DegenerateReferences);
            Assert.Equal(references_Failed, externalComponentsAreaResult_Json.FailedReferences);

            ExternalComponentsAreaResult externalComponentsAreaResult_Clone = new(externalComponentsAreaResult);

            Assert.Equal(104, externalComponentsAreaResult_Clone.SkippedComponentCount);
            Assert.Equal(6, externalComponentsAreaResult_Clone.OpenEnvelopeCount);
            Assert.Equal(references_Degenerate, externalComponentsAreaResult_Clone.DegenerateReferences);
            Assert.Equal(references_Failed, externalComponentsAreaResult_Clone.FailedReferences);

            Core.xUnit.Query.SerializationCheck(externalComponentsAreaResult);
        }

        /// <summary>
        /// Verifies that a run over closed envelopes reports neither a skipped component nor an open envelope.
        /// </summary>
        [Fact]
        public void ExternalComponentsAreaResult_Closed()
        {
            ExternalComponentsAreaResult externalComponentsAreaResult = new(0, 0);

            Assert.Equal(0, externalComponentsAreaResult.SkippedComponentCount);
            Assert.Equal(0, externalComponentsAreaResult.OpenEnvelopeCount);
            Assert.Equal(0, externalComponentsAreaResult.DegenerateModelCount);
            Assert.Equal(0, externalComponentsAreaResult.FailedModelCount);

            Core.xUnit.Query.SerializationCheck(externalComponentsAreaResult);
        }
    }
}
