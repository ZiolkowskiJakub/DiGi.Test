using DiGi.GIS.PostgreSQL.Classes;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that the constructor carries both tallies through, and that a populated instance survives a JSON round trip and a clone.
        /// <para>The open-envelope tally is the reason the type exists: a run that reports a completed classification without it says nothing about how many of its rows may rest on an arbitrary face side. The figures are the ones measured on the 27-model verification sample of #84 - 104 partitions skipped, 6 open envelopes.</para>
        /// </summary>
        [Fact]
        public void ExternalComponentsAreaResult_Serialization()
        {
            ExternalComponentsAreaResult externalComponentsAreaResult = new(104, 6);

            Assert.Equal(104, externalComponentsAreaResult.SkippedComponentCount);
            Assert.Equal(6, externalComponentsAreaResult.OpenEnvelopeCount);

            string? json = Core.Convert.ToSystem_String(externalComponentsAreaResult);
            Assert.NotNull(json);

            ExternalComponentsAreaResult? externalComponentsAreaResult_Json = Core.Convert.ToDiGi<ExternalComponentsAreaResult>(json)?.FirstOrDefault();
            Assert.NotNull(externalComponentsAreaResult_Json);

            Assert.Equal(104, externalComponentsAreaResult_Json.SkippedComponentCount);
            Assert.Equal(6, externalComponentsAreaResult_Json.OpenEnvelopeCount);

            ExternalComponentsAreaResult externalComponentsAreaResult_Clone = new(externalComponentsAreaResult);

            Assert.Equal(104, externalComponentsAreaResult_Clone.SkippedComponentCount);
            Assert.Equal(6, externalComponentsAreaResult_Clone.OpenEnvelopeCount);

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

            Core.xUnit.Query.SerializationCheck(externalComponentsAreaResult);
        }
    }
}
