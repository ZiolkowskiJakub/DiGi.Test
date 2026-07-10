using DiGi.Communication.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Communication.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the full comparative analysis cascade for the "Bad Urban" reference Power Delay Profile. See <see cref="PropagationResult_TypicalUrban"/> for the validated relations.
        /// </summary>
        [Fact]
        public void PropagationResult_BadUrban()
        {
            PropagationResultCheck(Create.SimpleMultipathPowerDelayProfile(Enums.DefaultSimpleMultipathPowerDelayProfile.BadUrban));
        }

        /// <summary>
        /// Tests the full comparative analysis cascade for the "Typical Urban" reference Power Delay Profile. With one mesh cell placed exactly on each propagation ellipsoid and unit omnidirectional characteristics, the corrected ray powers p_nkl reproduce the measured fractional powers p'_n, their sum equals 1 and a directional reception characteristic of constant value 2 doubles the received power (P_0 = 2).
        /// </summary>
        [Fact]
        public void PropagationResult_TypicalUrban()
        {
            PropagationResultCheck(Create.SimpleMultipathPowerDelayProfile(Enums.DefaultSimpleMultipathPowerDelayProfile.TypicalUrban));
        }

        private static void PropagationResultCheck(SimpleMultipathPowerDelayProfile? simpleMultipathPowerDelayProfile)
        {
            Assert.NotNull(simpleMultipathPowerDelayProfile);

            HashSet<double>? delays = simpleMultipathPowerDelayProfile.Delays;
            Assert.NotNull(delays);

            List<double> orderedDelays = [.. delays.OrderBy(x => x)];

            double distance = 300;

            List<MeshCell> meshCells = [];
            foreach (double delay in orderedDelays)
            {
                double semiMinorAxis = Query.SemiMinorAxis(delay, distance);

                MeshCell meshCell = new(new Point3D(distance / 2, 0, semiMinorAxis), new Vector3D(0, 0, 1), new MaterialProperties(15, 0.005));
                meshCells.Add(meshCell);
            }

            PropagationModel propagationModel = new(
                distance,
                900,
                meshCells,
                Enums.Polarization.Vertical,
                simpleMultipathPowerDelayProfile,
                (theta, phi) => 2.0,
                (theta, phi) => 1.0,
                (theta, phi) => 1.0,
                (theta, phi) => 1.0);

            PropagationResult? propagationResult = propagationModel.PropagationResult();

            Assert.NotNull(propagationResult);
            Assert.True(propagationResult.TotalPower > 0);

            List<EllipsoidComponent>? ellipsoidComponents = propagationResult.EllipsoidComponents;
            List<ArrivalRay>? arrivalRays = propagationResult.Rays;

            Assert.NotNull(ellipsoidComponents);
            Assert.NotNull(arrivalRays);
            Assert.Equal(orderedDelays.Count, ellipsoidComponents.Count);
            Assert.Equal(orderedDelays.Count, arrivalRays.Count);

            // Validation: the sum of all corrected ray powers p_nkl must equal 1 for a normalized Power Delay Profile
            Assert.Equal(1.0, arrivalRays.TotalPower(), 6);

            // With a single contribution per ellipsoid the power equivalence coefficient w_Pn equals 1 and each ray power reproduces the measured fractional power p'_n
            for (int i = 0; i < orderedDelays.Count; i++)
            {
                Assert.Equal(1.0, ellipsoidComponents[i].PowerEquivalenceCoefficient, 9);
                Assert.Equal(simpleMultipathPowerDelayProfile.GetPower(orderedDelays[i]), arrivalRays[i].Power, 9);
            }

            // Stage III: a directional reception characteristic of constant value 2 doubles the received power
            Assert.Equal(2.0, propagationResult.DirectionalPower, 6);
        }
    }
}
