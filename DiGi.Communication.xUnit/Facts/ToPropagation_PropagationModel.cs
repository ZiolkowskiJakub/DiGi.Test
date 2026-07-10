using DiGi.Communication.Classes;
using DiGi.Communication.Enums;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Communication.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the conversion of a geometrical propagation model into the input data of the multi-ellipsoidal propagation model: the transmitter-receiver distance, the transformation of the scattering object mesh cells into the model coordinate system (the transmitter at the origin, the OX axis towards the receiver and the OZ axis vertical), the normalization of the power delay profile, the default and per-reference material assignment and the pass-through of the antenna characteristics.
        /// </summary>
        [Fact]
        public void ToPropagation_PropagationModel()
        {
            Antenna antenna_Transmitter = new(new Point3D(10, 20, 5), Function.Transmitter);
            Antenna antenna_Receiver = new(new Point3D(10, 320, 5), Function.Receiver);

            // Unnormalized power delay profile: the conversion normalizes the fractional powers to sum to 1
            Dictionary<double, double> values = new() { [2e-6] = 2.0, [1e-6] = 6.0 };
            SimpleMultipathPowerDelayProfile simpleMultipathPowerDelayProfile = new(values);

            // Triangle with the centroid at the world point (9, 171, 6) and the normal along the world OX axis
            Mesh3D mesh3D_Wall = new([new Point3D(9, 170, 5), new Point3D(9, 170, 8), new Point3D(9, 173, 5)], [[0, 1, 2]]);
            ScatteringObject scatteringObject_Wall = new("Wall", mesh3D_Wall);

            Mesh3D mesh3D_Concrete = new([new Point3D(9, 200, 5), new Point3D(9, 200, 8), new Point3D(9, 203, 5)], [[0, 1, 2]]);
            ScatteringObject scatteringObject_Concrete = new("Concrete", mesh3D_Concrete);

            GeometricalPropagationModel geometricalPropagationModel = new();
            Assert.True(geometricalPropagationModel.Assign(simpleMultipathPowerDelayProfile, antenna_Transmitter, antenna_Receiver));
            Assert.True(geometricalPropagationModel.Update(scatteringObject_Wall));
            Assert.True(geometricalPropagationModel.Update(scatteringObject_Concrete));

            Dictionary<string, MaterialProperties> materialPropertiesDictionary = new() { ["Concrete"] = new(5, 0.02) };

            PropagationModel? propagationModel = geometricalPropagationModel.ToPropagation_PropagationModel(900, Enums.Polarization.Vertical, new MaterialProperties(15, 0.005), (theta, phi) => 2.0, (theta, phi) => 1.0, (theta, phi) => 1.0, (theta, phi) => 1.0, materialPropertiesDictionary);

            Assert.NotNull(propagationModel);
            Assert.Equal(300.0, propagationModel.Distance, 9);
            Assert.Equal(900.0, propagationModel.Frequency, 9);
            Assert.Equal(Enums.Polarization.Vertical, propagationModel.Polarization);

            // The fractional powers are normalized to sum to 1
            SimpleMultipathPowerDelayProfile? simpleMultipathPowerDelayProfile_Result = propagationModel.SimpleMultipathPowerDelayProfile;
            Assert.NotNull(simpleMultipathPowerDelayProfile_Result);
            HashSet<double>? delays = simpleMultipathPowerDelayProfile_Result.Delays;
            Assert.NotNull(delays);
            Assert.Equal(2, delays.Count);
            Assert.Equal(0.75, simpleMultipathPowerDelayProfile_Result.GetPower(1e-6), 9);
            Assert.Equal(0.25, simpleMultipathPowerDelayProfile_Result.GetPower(2e-6), 9);

            List<MeshCell>? meshCells = propagationModel.MeshCells;
            Assert.NotNull(meshCells);
            Assert.Equal(2, meshCells.Count);

            MeshCell? meshCell_Wall = meshCells.Find(x => x.MaterialProperties?.RelativePermittivity == 15);
            Assert.NotNull(meshCell_Wall);

            // Model coordinate system: X = (0, 1, 0), Y = (-1, 0, 0), Z = (0, 0, 1) in world coordinates, so the world point (10 + a, 20 + b, 5 + c) maps to the model point (b, -a, c)
            Point3D? point3D_Center = meshCell_Wall.Center;
            Assert.NotNull(point3D_Center);
            Assert.Equal(151.0, point3D_Center.X, 9);
            Assert.Equal(1.0, point3D_Center.Y, 9);
            Assert.Equal(1.0, point3D_Center.Z, 9);

            // The world normal (+-1, 0, 0) maps to the model normal (0, -+1, 0); the sign depends on the triangle winding
            Vector3D? vector3D_Normal = meshCell_Wall.Normal?.Unit;
            Assert.NotNull(vector3D_Normal);
            Assert.Equal(0.0, vector3D_Normal.X, 9);
            Assert.Equal(1.0, Math.Abs(vector3D_Normal.Y), 9);
            Assert.Equal(0.0, vector3D_Normal.Z, 9);

            // The scattering object with a dictionary entry uses its own material properties instead of the default ones
            MeshCell? meshCell_Concrete = meshCells.Find(x => x.MaterialProperties?.RelativePermittivity == 5);
            Assert.NotNull(meshCell_Concrete);
            Assert.Equal(0.02, meshCell_Concrete.MaterialProperties?.Conductivity ?? double.NaN, 9);

            AntennaCharacteristic? receivingDirectionalCharacteristic = propagationModel.ReceivingDirectionalCharacteristic;
            Assert.NotNull(receivingDirectionalCharacteristic);
            Assert.Equal(2.0, receivingDirectionalCharacteristic(0, 0), 9);
        }

        /// <summary>
        /// Tests the full comparative analysis cascade executed through the geometrical propagation model conversion for the "Typical Urban" reference Power Delay Profile. The transmitter is placed at the world origin and the receiver on the world OX axis, so the model coordinate system equals the world coordinate system. With one mesh triangle placed exactly on each propagation ellipsoid and unit omnidirectional characteristics, the corrected ray powers p_nkl reproduce the measured fractional powers p'_n, their sum equals 1 and a directional reception characteristic of constant value 2 doubles the received power (P_0 = 2).
        /// </summary>
        [Fact]
        public void ToPropagation_PropagationModel_TypicalUrban()
        {
            SimpleMultipathPowerDelayProfile? simpleMultipathPowerDelayProfile = Create.SimpleMultipathPowerDelayProfile(Enums.DefaultSimpleMultipathPowerDelayProfile.TypicalUrban);
            Assert.NotNull(simpleMultipathPowerDelayProfile);

            HashSet<double>? delays = simpleMultipathPowerDelayProfile.Delays;
            Assert.NotNull(delays);

            List<double> orderedDelays = [.. delays.OrderBy(x => x)];

            double distance = 300;

            Antenna antenna_Transmitter = new(new Point3D(0, 0, 0), Function.Transmitter);
            Antenna antenna_Receiver = new(new Point3D(distance, 0, 0), Function.Receiver);

            // One horizontal triangle per profile point with the centroid placed exactly on the propagation ellipsoid
            List<Point3D> point3Ds = [];
            List<int[]> indexes = [];
            foreach (double delay in orderedDelays)
            {
                double semiMinorAxis = Query.SemiMinorAxis(delay, distance);

                int index = point3Ds.Count;
                point3Ds.Add(new Point3D((distance / 2) - 1, -1, semiMinorAxis));
                point3Ds.Add(new Point3D((distance / 2) + 1, -1, semiMinorAxis));
                point3Ds.Add(new Point3D(distance / 2, 2, semiMinorAxis));
                indexes.Add([index, index + 1, index + 2]);
            }

            ScatteringObject scatteringObject = new("ScatteringObject", new Mesh3D(point3Ds, indexes));

            GeometricalPropagationModel geometricalPropagationModel = new();
            Assert.True(geometricalPropagationModel.Assign(simpleMultipathPowerDelayProfile, antenna_Transmitter, antenna_Receiver));
            Assert.True(geometricalPropagationModel.Update(scatteringObject));

            PropagationResult? propagationResult = geometricalPropagationModel.PropagationResult(900, Enums.Polarization.Vertical, new MaterialProperties(15, 0.005), (theta, phi) => 2.0, (theta, phi) => 1.0, (theta, phi) => 1.0);

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

        /// <summary>
        /// Tests that the conversion of a geometrical propagation model returns null for invalid input: a null source model, a non-positive or NaN frequency, missing default material properties, a model without an assigned power delay profile, a model without a receiving antenna and a vertical transmitter-receiver link for which the model coordinate system is undefined.
        /// </summary>
        [Fact]
        public void ToPropagation_PropagationModel_InvalidInput()
        {
            MaterialProperties materialProperties = new(15, 0.005);
            AntennaCharacteristic antennaCharacteristic = (theta, phi) => 1.0;

            GeometricalPropagationModel? geometricalPropagationModel_Null = null;
            Assert.Null(geometricalPropagationModel_Null.ToPropagation_PropagationModel(900, Enums.Polarization.Vertical, materialProperties, antennaCharacteristic, antennaCharacteristic, antennaCharacteristic, antennaCharacteristic));

            Dictionary<double, double> values = new() { [1e-6] = 1.0 };
            SimpleMultipathPowerDelayProfile simpleMultipathPowerDelayProfile = new(values);

            GeometricalPropagationModel geometricalPropagationModel = new();
            Assert.True(geometricalPropagationModel.Assign(simpleMultipathPowerDelayProfile, new Antenna(new Point3D(0, 0, 0), Function.Transmitter), new Antenna(new Point3D(300, 0, 0), Function.Receiver)));

            // Non-positive or NaN frequency
            Assert.Null(geometricalPropagationModel.ToPropagation_PropagationModel(0, Enums.Polarization.Vertical, materialProperties, antennaCharacteristic, antennaCharacteristic, antennaCharacteristic, antennaCharacteristic));
            Assert.Null(geometricalPropagationModel.ToPropagation_PropagationModel(double.NaN, Enums.Polarization.Vertical, materialProperties, antennaCharacteristic, antennaCharacteristic, antennaCharacteristic, antennaCharacteristic));

            // Missing default material properties
            Assert.Null(geometricalPropagationModel.ToPropagation_PropagationModel(900, Enums.Polarization.Vertical, null, antennaCharacteristic, antennaCharacteristic, antennaCharacteristic, antennaCharacteristic));

            // Positive control: the same model converts successfully for valid input
            Assert.NotNull(geometricalPropagationModel.ToPropagation_PropagationModel(900, Enums.Polarization.Vertical, materialProperties, antennaCharacteristic, antennaCharacteristic, antennaCharacteristic, antennaCharacteristic));

            // Model without an assigned power delay profile
            GeometricalPropagationModel geometricalPropagationModel_Empty = new();
            Assert.Null(geometricalPropagationModel_Empty.ToPropagation_PropagationModel(900, Enums.Polarization.Vertical, materialProperties, antennaCharacteristic, antennaCharacteristic, antennaCharacteristic, antennaCharacteristic));

            // Model without a receiving antenna
            GeometricalPropagationModel geometricalPropagationModel_NoReceiver = new();
            Assert.True(geometricalPropagationModel_NoReceiver.Assign(simpleMultipathPowerDelayProfile, new Antenna(new Point3D(0, 0, 0), Function.Transmitter), new Antenna(new Point3D(300, 0, 0), Function.Transmitter)));
            Assert.Null(geometricalPropagationModel_NoReceiver.ToPropagation_PropagationModel(900, Enums.Polarization.Vertical, materialProperties, antennaCharacteristic, antennaCharacteristic, antennaCharacteristic, antennaCharacteristic));

            // Vertical transmitter-receiver link: the model coordinate system is undefined
            GeometricalPropagationModel geometricalPropagationModel_Vertical = new();
            Assert.True(geometricalPropagationModel_Vertical.Assign(simpleMultipathPowerDelayProfile, new Antenna(new Point3D(0, 0, 0), Function.Transmitter), new Antenna(new Point3D(0, 0, 300), Function.Receiver)));
            Assert.Null(geometricalPropagationModel_Vertical.ToPropagation_PropagationModel(900, Enums.Polarization.Vertical, materialProperties, antennaCharacteristic, antennaCharacteristic, antennaCharacteristic, antennaCharacteristic));
        }
    }
}
