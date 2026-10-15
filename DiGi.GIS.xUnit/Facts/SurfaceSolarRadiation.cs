using DiGi.Geometry.Spatial.Classes;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies <see cref="Classes.SurfaceSolarRadiation"/>: the constructor assigns every member and copies the shading factors, the factors travel as a base64 JSON string (not the number array the DiGi serializer writes for a byte[]), and the string round trip, the clone and the copy constructor preserve every member, including a null normal and null factors.
        /// <para>Reference: DiGi.GIS#8 (stored solar radiation, DiGi.GIS.WebAPI.UI#73).</para>
        /// </summary>
        [Fact]
        public void SurfaceSolarRadiation()
        {
            byte[] shadingFactors = new byte[8760];
            for (int i = 0; i < shadingFactors.Length; i++)
            {
                shadingFactors[i] = (byte)((i * 7) % 256);
            }

            byte[] shadingFactors_Expected = [.. shadingFactors];

            Classes.SurfaceSolarRadiation surfaceSolarRadiation = new("b1a5c1de-0000-4000-8000-000000000001", 12.5, new Vector3D(0, -1, 0), 0.8, 0.6, 650.25, 400.125, 200.0625, 50.0625, 900.5, 8128.125, shadingFactors);

            // The stored factors are a copy: changing the caller's array afterwards changes nothing.
            shadingFactors[1] = 0;

            Assert.Equal("b1a5c1de-0000-4000-8000-000000000001", surfaceSolarRadiation.Reference);
            Assert.Equal(12.5, surfaceSolarRadiation.Area);
            Assert.NotNull(surfaceSolarRadiation.Normal);
            Assert.Equal(0, surfaceSolarRadiation.Normal.X);
            Assert.Equal(-1, surfaceSolarRadiation.Normal.Y);
            Assert.Equal(0, surfaceSolarRadiation.Normal.Z);
            Assert.Equal(0.8, surfaceSolarRadiation.SkyVisibility);
            Assert.Equal(0.6, surfaceSolarRadiation.GroundVisibility);
            Assert.Equal(650.25, surfaceSolarRadiation.Irradiation);
            Assert.Equal(400.125, surfaceSolarRadiation.Beam);
            Assert.Equal(200.0625, surfaceSolarRadiation.Diffuse);
            Assert.Equal(50.0625, surfaceSolarRadiation.Ground);
            Assert.Equal(900.5, surfaceSolarRadiation.IrradiationUnshaded);
            Assert.Equal(8128.125, surfaceSolarRadiation.Energy);
            Assert.Equal(shadingFactors_Expected, surfaceSolarRadiation.ShadingFactors);
            Assert.NotSame(surfaceSolarRadiation.ShadingFactors, surfaceSolarRadiation.ShadingFactors);

            JsonObject? jsonObject = surfaceSolarRadiation.ToJsonObject();
            Assert.NotNull(jsonObject);
            Assert.Equal(JsonValueKind.String, jsonObject[nameof(Classes.SurfaceSolarRadiation.ShadingFactors)]?.GetValueKind());
            Assert.Equal(System.Convert.ToBase64String(shadingFactors_Expected), jsonObject[nameof(Classes.SurfaceSolarRadiation.ShadingFactors)]!.GetValue<string>());

            string? json = Core.Convert.ToSystem_String(surfaceSolarRadiation);
            Assert.NotNull(json);
            AssertSurfaceSolarRadiation(surfaceSolarRadiation, Core.Convert.ToDiGi<Classes.SurfaceSolarRadiation>(json)?.FirstOrDefault());

            // SerializationCheck never runs the copy constructor, so it is asserted directly.
            AssertSurfaceSolarRadiation(surfaceSolarRadiation, new Classes.SurfaceSolarRadiation(surfaceSolarRadiation));

            Core.xUnit.Query.SerializationCheck(surfaceSolarRadiation);

            Classes.SurfaceSolarRadiation surfaceSolarRadiation_Null = new(null, 3.0, null, 1, 1, 0, 0, 0, 0, 0, 0, null);
            Assert.Null(surfaceSolarRadiation_Null.Reference);
            Assert.Null(surfaceSolarRadiation_Null.Normal);
            Assert.Null(surfaceSolarRadiation_Null.ShadingFactors);
            AssertSurfaceSolarRadiation(surfaceSolarRadiation_Null, Core.Convert.ToDiGi<Classes.SurfaceSolarRadiation>(Core.Convert.ToSystem_String(surfaceSolarRadiation_Null))?.FirstOrDefault());
            AssertSurfaceSolarRadiation(surfaceSolarRadiation_Null, new Classes.SurfaceSolarRadiation(surfaceSolarRadiation_Null));

            Core.xUnit.Query.SerializationCheck(surfaceSolarRadiation_Null);
        }

        private static void AssertSurfaceSolarRadiation(Classes.SurfaceSolarRadiation expected, Classes.SurfaceSolarRadiation? actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Reference, actual.Reference);
            Assert.Equal(expected.Area, actual.Area);
            Assert.Equal(expected.SkyVisibility, actual.SkyVisibility);
            Assert.Equal(expected.GroundVisibility, actual.GroundVisibility);
            Assert.Equal(expected.Irradiation, actual.Irradiation);
            Assert.Equal(expected.Beam, actual.Beam);
            Assert.Equal(expected.Diffuse, actual.Diffuse);
            Assert.Equal(expected.Ground, actual.Ground);
            Assert.Equal(expected.IrradiationUnshaded, actual.IrradiationUnshaded);
            Assert.Equal(expected.Energy, actual.Energy);
            Assert.Equal(expected.ShadingFactors, actual.ShadingFactors);

            Vector3D? normal_Expected = expected.Normal;
            Vector3D? normal_Actual = actual.Normal;
            if (normal_Expected is null)
            {
                Assert.Null(normal_Actual);
                return;
            }

            Assert.NotNull(normal_Actual);
            Assert.Equal(normal_Expected.X, normal_Actual.X);
            Assert.Equal(normal_Expected.Y, normal_Actual.Y);
            Assert.Equal(normal_Expected.Z, normal_Actual.Z);
        }
    }
}
