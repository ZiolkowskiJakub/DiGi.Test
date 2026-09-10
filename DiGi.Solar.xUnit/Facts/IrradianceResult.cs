using DiGi.Core.Classes;
using DiGi.Geometry.Spatial.Classes;
using System.Linq;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the three irradiance components against hand-computed values for a vertical south-facing surface with the sun due south at 30 degrees elevation.
        /// <para>Beam is 800 * cos(30) = 692.8203, Diffuse is 100 * (1 + cos(90)) / 2 = 50, Ground is 500 * 0.2 * (1 - cos(90)) / 2 = 50.</para>
        /// </summary>
        [Fact]
        public void IrradianceResult()
        {
            // Sun ray propagation direction for azimuth 180 degrees, elevation 30 degrees.
            Vector3D vector3D_SunDirection = new(0, System.Math.Cos(System.Math.PI / 6), -0.5);

            // Outward normal of a vertical south-facing surface.
            Vector3D vector3D_SurfaceNormal = new(0, -1, 0);

            Classes.IrradianceResult? irradianceResult = Create.IrradianceResult(vector3D_SurfaceNormal, vector3D_SunDirection, 500, 800, 100, 0.2);

            Assert.NotNull(irradianceResult);
            Assert.Equal(692.8203, irradianceResult.Beam, 4);
            Assert.Equal(50.0, irradianceResult.Diffuse, 4);
            Assert.Equal(50.0, irradianceResult.Ground, 4);
            Assert.Equal(792.8203, irradianceResult.Total, 4);
            Assert.Equal(0.2, irradianceResult.Albedo, 10);

            // The cosine of the incidence angle recovered from the beam component. For a vertical
            // wall facing the solar azimuth exactly, this equals the cosine of the solar elevation.
            Assert.Equal(System.Math.Cos(System.Math.PI / 6), irradianceResult.Beam / 800, 6);
        }

        /// <summary>
        /// Pins the sun direction sign convention against real sun direction output.
        /// <para>Query.SunDirection returns the ray PROPAGATION direction, which points away from the sun and therefore downwards while the sun is up. A surface facing the sun must receive beam radiation and a surface facing away from it must receive none. Computing the incidence cosine as a plain dot product rather than a negated one reverses both assertions below.</para>
        /// </summary>
        [Fact]
        public void IrradianceResult_SunDirectionConvention()
        {
            Coordinates coordinates = new(51.4778, 0.0); // Greenwich, UK
            System.DateTime dateTime = new(2026, 6, 21, 12, 0, 0); // Around solar noon on the summer solstice

            Vector3D? vector3D_SunDirection = Query.SunDirection(coordinates, Core.Enums.UTC.PlusMinus0000, dateTime, false);
            Assert.NotNull(vector3D_SunDirection);

            // The sun is above the horizon, so the propagation direction points downwards.
            Assert.True(vector3D_SunDirection.Z < 0, $"Sun direction is expected to point downwards while the sun is up. Z was {vector3D_SunDirection.Z}.");

            Classes.IrradianceResult? irradianceResult_South = Create.IrradianceResult(new Vector3D(0, -1, 0), vector3D_SunDirection, 500, 800, 100, 0.2);
            Assert.NotNull(irradianceResult_South);
            Assert.True(irradianceResult_South.Beam > 0, $"A south-facing wall is expected to receive beam radiation at solar noon. Beam was {irradianceResult_South.Beam}.");

            Classes.IrradianceResult? irradianceResult_North = Create.IrradianceResult(new Vector3D(0, 1, 0), vector3D_SunDirection, 500, 800, 100, 0.2);
            Assert.NotNull(irradianceResult_North);
            Assert.Equal(0.0, irradianceResult_North.Beam, 10);

            // The shadow-independent components do not depend on which way the surface faces.
            Assert.Equal(irradianceResult_South.Diffuse, irradianceResult_North.Diffuse, 10);
            Assert.Equal(irradianceResult_South.Ground, irradianceResult_North.Ground, 10);
        }

        /// <summary>
        /// Tests the tilt-dependent components at the boundaries of the tilt range, including tilts beyond 90 degrees which describe a downward-facing surface.
        /// </summary>
        [Fact]
        public void IrradianceResult_Tilt()
        {
            Vector3D vector3D_SunDirection = new(0, System.Math.Cos(System.Math.PI / 6), -0.5);

            // Flat horizontal surface, tilt 0. The whole sky is visible and no ground is.
            Classes.IrradianceResult? irradianceResult_Horizontal = Create.IrradianceResult(new Vector3D(0, 0, 1), vector3D_SunDirection, 500, 800, 100, 0.2);
            Assert.NotNull(irradianceResult_Horizontal);
            Assert.Equal(100.0, irradianceResult_Horizontal.Diffuse, 10);
            Assert.Equal(0.0, irradianceResult_Horizontal.Ground, 10);

            // Downward-facing surface, tilt 180. The whole ground is visible and no sky is. This is
            // deliberate rather than an unhandled case, so it must not be clamped to a roof-only range.
            Classes.IrradianceResult? irradianceResult_Downward = Create.IrradianceResult(new Vector3D(0, 0, -1), vector3D_SunDirection, 500, 800, 100, 0.2);
            Assert.NotNull(irradianceResult_Downward);
            Assert.Equal(0.0, irradianceResult_Downward.Diffuse, 10);
            Assert.Equal(100.0, irradianceResult_Downward.Ground, 10);
            Assert.Equal(0.0, irradianceResult_Downward.Beam, 10);
        }

        /// <summary>
        /// Tests that null, degenerate and non-physical inputs return null rather than propagating a not-a-number result.
        /// </summary>
        [Fact]
        public void IrradianceResult_NullAndBoundaryCases()
        {
            Vector3D vector3D_SunDirection = new(0, System.Math.Cos(System.Math.PI / 6), -0.5);
            Vector3D vector3D_SurfaceNormal = new(0, -1, 0);

            Assert.Null(Create.IrradianceResult(null, vector3D_SunDirection, 500, 800, 100, 0.2));
            Assert.Null(Create.IrradianceResult(vector3D_SurfaceNormal, null, 500, 800, 100, 0.2));

            // Zero-length vectors have no direction.
            Assert.Null(Create.IrradianceResult(new Vector3D(0, 0, 0), vector3D_SunDirection, 500, 800, 100, 0.2));
            Assert.Null(Create.IrradianceResult(vector3D_SurfaceNormal, new Vector3D(0, 0, 0), 500, 800, 100, 0.2));

            // Non-physical radiation values.
            Assert.Null(Create.IrradianceResult(vector3D_SurfaceNormal, vector3D_SunDirection, double.NaN, 800, 100, 0.2));
            Assert.Null(Create.IrradianceResult(vector3D_SurfaceNormal, vector3D_SunDirection, 500, -1, 100, 0.2));
            Assert.Null(Create.IrradianceResult(vector3D_SurfaceNormal, vector3D_SunDirection, 500, 800, double.PositiveInfinity, 0.2));
            Assert.Null(Create.IrradianceResult(vector3D_SurfaceNormal, vector3D_SunDirection, 500, 800, 100, -0.2));

            // Night, when every component is zero but the result is still valid.
            Classes.IrradianceResult? irradianceResult_Night = Create.IrradianceResult(vector3D_SurfaceNormal, vector3D_SunDirection, 0, 0, 0, 0.2);
            Assert.NotNull(irradianceResult_Night);
            Assert.Equal(0.0, irradianceResult_Night.Total, 10);
        }

        /// <summary>
        /// Tests the serialization round-trip and cloning of IrradianceResult with every member populated to a distinct non-default value.
        /// </summary>
        [Fact]
        public void IrradianceResult_Serialization()
        {
            Classes.IrradianceResult irradianceResult = new(692.8203, 50.0, 25.0, 0.35);

            Assert.Equal(692.8203, irradianceResult.Beam, 4);
            Assert.Equal(50.0, irradianceResult.Diffuse, 4);
            Assert.Equal(25.0, irradianceResult.Ground, 4);
            Assert.Equal(0.35, irradianceResult.Albedo, 4);
            Assert.Equal(767.8203, irradianceResult.Total, 4);

            Core.xUnit.Query.SerializationCheck(irradianceResult);
        }
        /// <summary>
        /// Tests the model against its own closure identity: for a flat horizontal surface the total incident irradiance must equal the global horizontal radiation, because global horizontal is by definition the direct normal projected onto the horizontal plus the diffuse horizontal.
        /// <para>This is a stronger check than a hand-computed fixture because it validates the beam projection, the sign convention and the tilt terms together against a relationship that holds for any sun position, rather than against numbers derived the same way the code derives them.</para>
        /// </summary>
        [Fact]
        public void IrradianceResult_HorizontalClosure()
        {
            Coordinates coordinates = new(51.4778, 0.0); // Greenwich, UK

            System.DateTime[] dateTimes =
            [
                new System.DateTime(2026, 6, 21, 8, 0, 0),
                new System.DateTime(2026, 6, 21, 12, 0, 0),
                new System.DateTime(2026, 6, 21, 17, 0, 0),
                new System.DateTime(2026, 3, 21, 10, 0, 0),
                new System.DateTime(2026, 12, 21, 12, 0, 0)
            ];

            foreach (System.DateTime dateTime in dateTimes)
            {
                Vector3D? vector3D_SunDirection = Query.SunDirection(coordinates, Core.Enums.UTC.PlusMinus0000, dateTime, false);
                Assert.NotNull(vector3D_SunDirection);

                Vector3D? vector3D_Unit = vector3D_SunDirection.Unit;
                Assert.NotNull(vector3D_Unit);

                // The sun direction points away from the sun, so the sine of the solar elevation
                // (equivalently the cosine of the zenith angle) is the negated Z ordinate.
                double sineElevation = -vector3D_Unit.Z;

                double directNormalRadiation = 800;
                double diffuseHorizontalRadiation = 120;
                double globalHorizontalRadiation = (directNormalRadiation * sineElevation) + diffuseHorizontalRadiation;

                Classes.IrradianceResult? irradianceResult = Create.IrradianceResult(new Vector3D(0, 0, 1), vector3D_SunDirection, globalHorizontalRadiation, directNormalRadiation, diffuseHorizontalRadiation, 0.2);

                Assert.NotNull(irradianceResult);
                Assert.Equal(globalHorizontalRadiation, irradianceResult.Total, 9);
                Assert.Equal(0.0, irradianceResult.Ground, 12);
                Assert.Equal(diffuseHorizontalRadiation, irradianceResult.Diffuse, 12);
            }
        }

        /// <summary>
        /// Tests that the tilt cosine is taken exactly, so that a vertical surface halves the sky diffuse component precisely rather than leaving a rounding residue.
        /// <para>Deriving the tilt cosine as Math.Cos(normal.Angle(WorldZ)) round-trips through acos and returns 6.1E-17 for a vertical surface instead of 0, which reaches the result as a diffuse component of 50.000000000000007 rather than 50.</para>
        /// </summary>
        [Fact]
        public void IrradianceResult_TiltExactness()
        {
            Vector3D vector3D_SunDirection = new(0, System.Math.Cos(System.Math.PI / 6), -0.5);

            Classes.IrradianceResult? irradianceResult = Create.IrradianceResult(new Vector3D(0, -1, 0), vector3D_SunDirection, 500, 800, 100, 0.2);
            Assert.NotNull(irradianceResult);

            Assert.Equal(50.0, irradianceResult.Diffuse, 15);
            Assert.Equal(50.0, irradianceResult.Ground, 15);
        }

        /// <summary>
        /// Tests that vectors which are not unit length give the same result as their normalized equivalents, since the factory normalizes both inputs itself.
        /// </summary>
        [Fact]
        public void IrradianceResult_NonUnitVectors()
        {
            Vector3D vector3D_SunDirection = new(0, System.Math.Cos(System.Math.PI / 6), -0.5);
            Vector3D vector3D_SurfaceNormal = new(0, -1, 0);

            Classes.IrradianceResult? irradianceResult_Unit = Create.IrradianceResult(vector3D_SurfaceNormal, vector3D_SunDirection, 500, 800, 100, 0.2);
            Assert.NotNull(irradianceResult_Unit);

            Classes.IrradianceResult? irradianceResult_Scaled = Create.IrradianceResult(new Vector3D(0, -17.5, 0), new Vector3D(0, System.Math.Cos(System.Math.PI / 6) * 42, -0.5 * 42), 500, 800, 100, 0.2);
            Assert.NotNull(irradianceResult_Scaled);

            Assert.Equal(irradianceResult_Unit.Beam, irradianceResult_Scaled.Beam, 10);
            Assert.Equal(irradianceResult_Unit.Diffuse, irradianceResult_Scaled.Diffuse, 10);
            Assert.Equal(irradianceResult_Unit.Ground, irradianceResult_Scaled.Ground, 10);
        }

        /// <summary>
        /// Tests that a ground reflectance outside the range 0 to 1 is rejected rather than silently inflating the ground-reflected component.
        /// <para>A weather record that reports its missing albedo as 999 rather than as null would otherwise multiply the ground term by 999.</para>
        /// </summary>
        [Fact]
        public void IrradianceResult_AlbedoRange()
        {
            Vector3D vector3D_SunDirection = new(0, System.Math.Cos(System.Math.PI / 6), -0.5);
            Vector3D vector3D_SurfaceNormal = new(0, -1, 0);

            Assert.Null(Create.IrradianceResult(vector3D_SurfaceNormal, vector3D_SunDirection, 500, 800, 100, 999));
            Assert.Null(Create.IrradianceResult(vector3D_SurfaceNormal, vector3D_SunDirection, 500, 800, 100, 1.0001));

            // The ends of the valid range are accepted.
            Assert.NotNull(Create.IrradianceResult(vector3D_SurfaceNormal, vector3D_SunDirection, 500, 800, 100, 0));
            Assert.NotNull(Create.IrradianceResult(vector3D_SurfaceNormal, vector3D_SunDirection, 500, 800, 100, 1));
        }

        /// <summary>
        /// Tests the JSON string round-trip of IrradianceResult, which the clone-based serialization check does not cover because it never deserializes.
        /// </summary>
        [Fact]
        public void IrradianceResult_StringRoundTrip()
        {
            Classes.IrradianceResult irradianceResult = new(692.8203, 50.0, 25.0, 0.35);

            string? json = Core.Convert.ToSystem_String(irradianceResult);
            Assert.False(string.IsNullOrWhiteSpace(json));

            Classes.IrradianceResult? irradianceResult_Deserialized = Core.Convert.ToDiGi<Classes.IrradianceResult>(json)?.FirstOrDefault();
            Assert.NotNull(irradianceResult_Deserialized);

            Assert.Equal(irradianceResult.Beam, irradianceResult_Deserialized.Beam, 10);
            Assert.Equal(irradianceResult.Diffuse, irradianceResult_Deserialized.Diffuse, 10);
            Assert.Equal(irradianceResult.Ground, irradianceResult_Deserialized.Ground, 10);
            Assert.Equal(irradianceResult.Albedo, irradianceResult_Deserialized.Albedo, 10);
            Assert.Equal(irradianceResult.Total, irradianceResult_Deserialized.Total, 10);
        }
    }
}
