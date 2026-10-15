using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies <see cref="Create.BuildingSolarRadiationResult(GIS.Classes.BuildingSolarRadiationResult?, int, IList{byte[]})"/>: it files a copy under the given county part, removes every surface's factors for null, puts given factors back in surface order, copies every other member, and answers null for a null source or a factor list that does not hold one entry per surface.
        /// <para>Reference: DiGi.GIS.PostgreSQL#100.</para>
        /// </summary>
        [Fact]
        public void Create_BuildingSolarRadiationResult()
        {
            GIS.Classes.BuildingSolarRadiationResult buildingSolarRadiationResult = ScratchBuildingSolarRadiationResult("XUNIT-SR-C", 990301009L, "17", "user@example.com", 1.0, 180);
            List<GIS.Classes.SurfaceSolarRadiation> surfaceSolarRadiations = buildingSolarRadiationResult.SurfaceSolarRadiations!;
            Assert.Equal(2, surfaceSolarRadiations.Count);

            GIS.Classes.BuildingSolarRadiationResult? buildingSolarRadiationResult_Stored = Create.BuildingSolarRadiationResult(buildingSolarRadiationResult, countyId_SolarRadiation_B, null);
            AssertBuildingSolarRadiationResult(buildingSolarRadiationResult, buildingSolarRadiationResult_Stored, false);
            Assert.Equal(countyId_SolarRadiation_B, buildingSolarRadiationResult_Stored!.CountyId);

            List<byte[]?> shadingFactors = [.. surfaceSolarRadiations.ConvertAll(x => x.ShadingFactors)];
            GIS.Classes.BuildingSolarRadiationResult? buildingSolarRadiationResult_Restored = Create.BuildingSolarRadiationResult(buildingSolarRadiationResult_Stored, countyId_SolarRadiation_A, shadingFactors);
            AssertBuildingSolarRadiationResult(buildingSolarRadiationResult, buildingSolarRadiationResult_Restored, true);
            Assert.Equal(countyId_SolarRadiation_A, buildingSolarRadiationResult_Restored!.CountyId);

            Assert.Null(Create.BuildingSolarRadiationResult(null, countyId_SolarRadiation_A, null));
            Assert.Null(Create.BuildingSolarRadiationResult(buildingSolarRadiationResult, countyId_SolarRadiation_A, [shadingFactors[0]]));
        }
    }
}
