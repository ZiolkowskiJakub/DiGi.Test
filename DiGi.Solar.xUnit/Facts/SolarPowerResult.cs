using DiGi.Geometry.Spatial.Classes;
using System.Linq;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the decoupled partial-shading power formula at the three area boundary cases for a vertical south-facing surface of 10 square metres.
        /// <para>Beam is 692.8203 W/m2, Diffuse and Ground are 50 W/m2 each, so the fully lit case is 7928.2032 W, the fully shaded case is 1000 W and the half shaded case is 4464.1016 W.</para>
        /// </summary>
        [Fact]
        public void SolarPowerResult()
        {
            Vector3D vector3D_SunDirection = new(0, System.Math.Cos(System.Math.PI / 6), -0.5);
            Vector3D vector3D_SurfaceNormal = new(0, -1, 0);

            Classes.IrradianceResult? irradianceResult = Create.IrradianceResult(vector3D_SurfaceNormal, vector3D_SunDirection, 500, 800, 100, 0.2);
            Assert.NotNull(irradianceResult);

            double totalArea = 10;

            // Fully lit. The formula must reduce to the total area times the total irradiance.
            Classes.SolarPowerResult? solarPowerResult_Lit = Create.SolarPowerResult(irradianceResult, totalArea, totalArea);
            Assert.NotNull(solarPowerResult_Lit);
            Assert.Equal(7928.2032, solarPowerResult_Lit.Power, 4);
            Assert.Equal(totalArea * irradianceResult.Total, solarPowerResult_Lit.Power, 10);
            Assert.Equal(0.0, solarPowerResult_Lit.ShadedArea, 10);

            // Fully shaded. The shadow blocks the beam component only, so the surface still
            // receives the sky diffuse and ground-reflected components over its whole area.
            Classes.SolarPowerResult? solarPowerResult_Shaded = Create.SolarPowerResult(irradianceResult, totalArea, 0);
            Assert.NotNull(solarPowerResult_Shaded);
            Assert.Equal(1000.0, solarPowerResult_Shaded.Power, 4);
            Assert.Equal(totalArea * (irradianceResult.Diffuse + irradianceResult.Ground), solarPowerResult_Shaded.Power, 10);
            Assert.Equal(totalArea, solarPowerResult_Shaded.ShadedArea, 10);

            // Half shaded, a linear blend of the two above.
            Classes.SolarPowerResult? solarPowerResult_Half = Create.SolarPowerResult(irradianceResult, totalArea, totalArea / 2);
            Assert.NotNull(solarPowerResult_Half);
            Assert.Equal(4464.1016, solarPowerResult_Half.Power, 4);
            Assert.Equal((solarPowerResult_Lit.Power + solarPowerResult_Shaded.Power) / 2, solarPowerResult_Half.Power, 10);
        }

        /// <summary>
        /// Tests that the shading-factor entry point agrees with the unshaded-area one.
        /// <para>Shading solvers in this workspace report the SHADED area and a SHADED fraction, so this entry point exists to stop callers inverting the value by hand. A shading factor of 0.4 over 10 square metres must match an unshaded area of 6 square metres, not 4.</para>
        /// </summary>
        [Fact]
        public void SolarPowerResult_ByShadingFactor()
        {
            Vector3D vector3D_SunDirection = new(0, System.Math.Cos(System.Math.PI / 6), -0.5);
            Vector3D vector3D_SurfaceNormal = new(0, -1, 0);

            Classes.IrradianceResult? irradianceResult = Create.IrradianceResult(vector3D_SurfaceNormal, vector3D_SunDirection, 500, 800, 100, 0.2);
            Assert.NotNull(irradianceResult);

            double totalArea = 10;

            Classes.SolarPowerResult? solarPowerResult_ByFactor = Create.SolarPowerResult_ByShadingFactor(irradianceResult, totalArea, 0.4);
            Assert.NotNull(solarPowerResult_ByFactor);
            Assert.Equal(6.0, solarPowerResult_ByFactor.UnshadedArea, 10);
            Assert.Equal(4.0, solarPowerResult_ByFactor.ShadedArea, 10);

            Classes.SolarPowerResult? solarPowerResult_ByArea = Create.SolarPowerResult(irradianceResult, totalArea, 6);
            Assert.NotNull(solarPowerResult_ByArea);
            Assert.Equal(solarPowerResult_ByArea.Power, solarPowerResult_ByFactor.Power, 10);

            // The two ends of the factor range agree with the two ends of the area range.
            Classes.SolarPowerResult? solarPowerResult_FullyLit = Create.SolarPowerResult_ByShadingFactor(irradianceResult, totalArea, 0);
            Assert.NotNull(solarPowerResult_FullyLit);
            Assert.Equal(totalArea * irradianceResult.Total, solarPowerResult_FullyLit.Power, 10);

            Classes.SolarPowerResult? solarPowerResult_FullyShaded = Create.SolarPowerResult_ByShadingFactor(irradianceResult, totalArea, 1);
            Assert.NotNull(solarPowerResult_FullyShaded);
            Assert.Equal(totalArea * (irradianceResult.Diffuse + irradianceResult.Ground), solarPowerResult_FullyShaded.Power, 10);
        }

        /// <summary>
        /// Tests that null, non-physical and out-of-range inputs return null, including an unshaded area exceeding the total area.
        /// </summary>
        [Fact]
        public void SolarPowerResult_NullAndBoundaryCases()
        {
            Classes.IrradianceResult irradianceResult = new(692.8203, 50.0, 50.0, 0.2);

            Assert.Null(Create.SolarPowerResult(null, 10, 5));

            // The unshaded area can never exceed the total area.
            Assert.Null(Create.SolarPowerResult(irradianceResult, 10, 11));

            Assert.Null(Create.SolarPowerResult(irradianceResult, -10, 5));
            Assert.Null(Create.SolarPowerResult(irradianceResult, 10, -5));
            Assert.Null(Create.SolarPowerResult(irradianceResult, double.NaN, 5));
            Assert.Null(Create.SolarPowerResult(irradianceResult, 10, double.NaN));

            Assert.Null(Create.SolarPowerResult_ByShadingFactor(irradianceResult, 10, 1.5));
            Assert.Null(Create.SolarPowerResult_ByShadingFactor(irradianceResult, 10, -0.1));
            Assert.Null(Create.SolarPowerResult_ByShadingFactor(irradianceResult, 10, double.NaN));
            Assert.Null(Create.SolarPowerResult_ByShadingFactor(null, 10, 0.4));

            // A zero-area surface is degenerate but not invalid.
            Classes.SolarPowerResult? solarPowerResult_Zero = Create.SolarPowerResult(irradianceResult, 0, 0);
            Assert.NotNull(solarPowerResult_Zero);
            Assert.Equal(0.0, solarPowerResult_Zero.Power, 10);
        }

        /// <summary>
        /// Tests the serialization round-trip and cloning of SolarPowerResult with every member populated, including the nested irradiance result.
        /// </summary>
        [Fact]
        public void SolarPowerResult_Serialization()
        {
            Classes.IrradianceResult irradianceResult = new(692.8203, 50.0, 25.0, 0.35);
            Classes.SolarPowerResult solarPowerResult = new(irradianceResult, 12.5, 7.5);

            Assert.Equal(12.5, solarPowerResult.TotalArea, 4);
            Assert.Equal(7.5, solarPowerResult.UnshadedArea, 4);
            Assert.Equal(5.0, solarPowerResult.ShadedArea, 4);
            Assert.NotNull(solarPowerResult.IrradianceResult);
            Assert.Equal(0.35, solarPowerResult.IrradianceResult.Albedo, 4);
            Assert.Equal((7.5 * 692.8203) + (12.5 * 75.0), solarPowerResult.Power, 4);

            Core.xUnit.Query.SerializationCheck(solarPowerResult);
        }
        /// <summary>
        /// Tests the JSON string round-trip of SolarPowerResult including its nested irradiance result, which the clone-based serialization check does not cover because it never deserializes.
        /// </summary>
        [Fact]
        public void SolarPowerResult_StringRoundTrip()
        {
            Classes.IrradianceResult irradianceResult = new(692.8203, 50.0, 25.0, 0.35);
            Classes.SolarPowerResult solarPowerResult = new(irradianceResult, 12.5, 7.5);

            string? json = Core.Convert.ToSystem_String(solarPowerResult);
            Assert.False(string.IsNullOrWhiteSpace(json));

            Classes.SolarPowerResult? solarPowerResult_Deserialized = Core.Convert.ToDiGi<Classes.SolarPowerResult>(json)?.FirstOrDefault();
            Assert.NotNull(solarPowerResult_Deserialized);

            Assert.Equal(12.5, solarPowerResult_Deserialized.TotalArea, 10);
            Assert.Equal(7.5, solarPowerResult_Deserialized.UnshadedArea, 10);

            // The nested result must survive the round-trip, otherwise Power silently becomes NaN.
            Assert.NotNull(solarPowerResult_Deserialized.IrradianceResult);
            Assert.Equal(0.35, solarPowerResult_Deserialized.IrradianceResult.Albedo, 10);
            Assert.Equal(692.8203, solarPowerResult_Deserialized.IrradianceResult.Beam, 10);
            Assert.Equal(solarPowerResult.Power, solarPowerResult_Deserialized.Power, 10);
        }

        /// <summary>
        /// Tests that a SolarPowerResult carrying no irradiance result reports its power as not a number rather than as zero.
        /// <para>The factories never produce this state, but deserializing an incomplete document can, and a zero would read as a genuine unlit surface.</para>
        /// </summary>
        [Fact]
        public void SolarPowerResult_NullIrradianceResult()
        {
            Classes.SolarPowerResult solarPowerResult = new(null, 12.5, 7.5);

            Assert.Null(solarPowerResult.IrradianceResult);
            Assert.True(double.IsNaN(solarPowerResult.Power), $"Power is expected to be NaN when no irradiance result is carried. It was {solarPowerResult.Power}.");

            Core.xUnit.Query.SerializationCheck(solarPowerResult);
        }
    }
}
