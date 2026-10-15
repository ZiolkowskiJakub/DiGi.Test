using DiGi.Geometry.Spatial.Classes;
using DiGi.GIS.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies <see cref="Classes.BuildingSolarRadiationResult"/>: the constructor assigns every member and skips null surfaces, and the string round trip, the clone and the copy constructor preserve every member, including the weather members (a live source with a period, an EPW source with a null period), a null user email and a surface with a null normal and null factors.
        /// <para>A missing weather source type reads back as <see cref="WeatherSourceType.Undefined"/>, and one stored surface of 8 760 factors serializes to under 15 KB, which only base64 factors achieve.</para>
        /// <para>Reference: DiGi.GIS#8 (stored solar radiation, DiGi.GIS.WebAPI.UI#73).</para>
        /// </summary>
        [Fact]
        public void BuildingSolarRadiationResult()
        {
            byte[] shadingFactors = new byte[8760];
            for (int i = 0; i < shadingFactors.Length; i++)
            {
                shadingFactors[i] = (byte)((i * 13) % 256);
            }

            Classes.SurfaceSolarRadiation surfaceSolarRadiation_Populated = new("b1a5c1de-0000-4000-8000-000000000001", 12.5, new Vector3D(1, 0, 0), 0.75, 0.5, 712.5, 450.25, 210.125, 52.125, 980.5, 8906.25, shadingFactors);
            Classes.SurfaceSolarRadiation surfaceSolarRadiation_Empty = new("b1a5c1de-0000-4000-8000-000000000002", 30.0, null, 1, 1, 0, 0, 0, 0, 0, 0, null);

            DateTimeOffset weatherPeriodStart = new(2026, 6, 1, 0, 0, 0, TimeSpan.FromHours(2));
            DateTimeOffset weatherPeriodEnd = new(2026, 6, 30, 23, 0, 0, TimeSpan.FromHours(2));
            DateTimeOffset createdAt = new(2026, 10, 15, 16, 30, 15, TimeSpan.Zero);

            Classes.BuildingSolarRadiationResult buildingSolarRadiationResult = new("2412_123456", 2412, 9876543210L, 50.0, 1, WeatherSourceType.Live, "station-12650", weatherPeriodStart, weatherPeriodEnd, null, createdAt, [surfaceSolarRadiation_Populated, null, surfaceSolarRadiation_Empty]);

            Assert.Equal("2412_123456", buildingSolarRadiationResult.Reference);
            Assert.Equal(2412, buildingSolarRadiationResult.CountyId);
            Assert.Equal(9876543210L, buildingSolarRadiationResult.BuildingModelId);
            Assert.Equal(50.0, buildingSolarRadiationResult.Radius);
            Assert.Equal(1, buildingSolarRadiationResult.SolverVersion);
            Assert.Equal(WeatherSourceType.Live, buildingSolarRadiationResult.WeatherSourceType);
            Assert.Equal("station-12650", buildingSolarRadiationResult.WeatherSourceId);
            Assert.Equal(weatherPeriodStart, buildingSolarRadiationResult.WeatherPeriodStart);
            Assert.Equal(weatherPeriodEnd, buildingSolarRadiationResult.WeatherPeriodEnd);
            Assert.Null(buildingSolarRadiationResult.UserEmail);
            Assert.Equal(createdAt, buildingSolarRadiationResult.CreatedAt);

            List<Classes.SurfaceSolarRadiation>? surfaceSolarRadiations = buildingSolarRadiationResult.SurfaceSolarRadiations;
            Assert.NotNull(surfaceSolarRadiations);
            Assert.Equal(2, surfaceSolarRadiations.Count);
            AssertSurfaceSolarRadiation(surfaceSolarRadiation_Populated, surfaceSolarRadiations[0]);
            AssertSurfaceSolarRadiation(surfaceSolarRadiation_Empty, surfaceSolarRadiations[1]);
            Assert.NotSame(surfaceSolarRadiations[0], buildingSolarRadiationResult.SurfaceSolarRadiations![0]);

            string? json = Core.Convert.ToSystem_String(buildingSolarRadiationResult);
            Assert.NotNull(json);
            AssertBuildingSolarRadiationResult(buildingSolarRadiationResult, Core.Convert.ToDiGi<Classes.BuildingSolarRadiationResult>(json)?.FirstOrDefault());

            // SerializationCheck never runs the copy constructor, so it is asserted directly.
            AssertBuildingSolarRadiationResult(buildingSolarRadiationResult, new Classes.BuildingSolarRadiationResult(buildingSolarRadiationResult));

            Core.xUnit.Query.SerializationCheck(buildingSolarRadiationResult);

            // A typical-year EPW summary: both period members are null.
            Classes.BuildingSolarRadiationResult buildingSolarRadiationResult_EPW = new("1465_42", 1465, 17L, 32.5, 1, WeatherSourceType.EPW, "17", null, null, "user@example.com", createdAt, [surfaceSolarRadiation_Populated]);
            Assert.Null(buildingSolarRadiationResult_EPW.WeatherPeriodStart);
            Assert.Null(buildingSolarRadiationResult_EPW.WeatherPeriodEnd);
            Assert.Equal("user@example.com", buildingSolarRadiationResult_EPW.UserEmail);
            AssertBuildingSolarRadiationResult(buildingSolarRadiationResult_EPW, Core.Convert.ToDiGi<Classes.BuildingSolarRadiationResult>(Core.Convert.ToSystem_String(buildingSolarRadiationResult_EPW))?.FirstOrDefault());
            AssertBuildingSolarRadiationResult(buildingSolarRadiationResult_EPW, new Classes.BuildingSolarRadiationResult(buildingSolarRadiationResult_EPW));

            Core.xUnit.Query.SerializationCheck(buildingSolarRadiationResult_EPW);

            // One surface of 8 760 factors: about 11.7 KB of base64 plus the summary; a number array would be about 30 KB.
            string? json_EPW = Core.Convert.ToSystem_String(buildingSolarRadiationResult_EPW);
            Assert.NotNull(json_EPW);
            Assert.InRange(json_EPW.Length, 11680, 15000);

            // A missing weather source type is Undefined, never silently EPW (0).
            JsonObject? jsonObject = buildingSolarRadiationResult_EPW.ToJsonObject();
            Assert.NotNull(jsonObject);
            Assert.True(jsonObject.Remove(nameof(Classes.BuildingSolarRadiationResult.WeatherSourceType)));
            Assert.Equal(WeatherSourceType.Undefined, new Classes.BuildingSolarRadiationResult(jsonObject).WeatherSourceType);

            // No surfaces stays null rather than becoming an empty list.
            Classes.BuildingSolarRadiationResult buildingSolarRadiationResult_NoSurfaces = new("1465_43", 1465, 18L, 32.5, 1, WeatherSourceType.EPW, "17", null, null, null, null, null);
            Assert.Null(buildingSolarRadiationResult_NoSurfaces.SurfaceSolarRadiations);
            Assert.Null(new Classes.BuildingSolarRadiationResult(buildingSolarRadiationResult_NoSurfaces).SurfaceSolarRadiations);
        }

        private static void AssertBuildingSolarRadiationResult(Classes.BuildingSolarRadiationResult expected, Classes.BuildingSolarRadiationResult? actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Reference, actual.Reference);
            Assert.Equal(expected.CountyId, actual.CountyId);
            Assert.Equal(expected.BuildingModelId, actual.BuildingModelId);
            Assert.Equal(expected.Radius, actual.Radius);
            Assert.Equal(expected.SolverVersion, actual.SolverVersion);
            Assert.Equal(expected.WeatherSourceType, actual.WeatherSourceType);
            Assert.Equal(expected.WeatherSourceId, actual.WeatherSourceId);
            Assert.Equal(expected.WeatherPeriodStart, actual.WeatherPeriodStart);
            Assert.Equal(expected.WeatherPeriodEnd, actual.WeatherPeriodEnd);
            Assert.Equal(expected.UserEmail, actual.UserEmail);
            Assert.Equal(expected.CreatedAt, actual.CreatedAt);

            List<Classes.SurfaceSolarRadiation>? surfaceSolarRadiations_Expected = expected.SurfaceSolarRadiations;
            List<Classes.SurfaceSolarRadiation>? surfaceSolarRadiations_Actual = actual.SurfaceSolarRadiations;
            if (surfaceSolarRadiations_Expected is null)
            {
                Assert.Null(surfaceSolarRadiations_Actual);
                return;
            }

            Assert.NotNull(surfaceSolarRadiations_Actual);
            Assert.Equal(surfaceSolarRadiations_Expected.Count, surfaceSolarRadiations_Actual.Count);
            for (int i = 0; i < surfaceSolarRadiations_Expected.Count; i++)
            {
                AssertSurfaceSolarRadiation(surfaceSolarRadiations_Expected[i], surfaceSolarRadiations_Actual[i]);
            }
        }
    }
}
