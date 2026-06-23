using DiGi.Unit.Enums;

namespace DiGi.Unit.xUnit
{
    /// <summary>
    /// Contains unit tests for verifying the expansion of SI units, correct bug fixes, and conversions between milli and micro units.
    /// </summary>
    public partial class Facts
    {
        /// <summary>
        /// Tests the conversion functions for the new SI base units and derived units, as well as testing critical bug fixes and milli-to-micro conversions.
        /// </summary>
        [Fact]
        public void TryConvertNewUnits()
        {
            bool bool_Converted;
            double? double_Value;

            // 1. Verify bug fix: Unit.To should convert from base to target (e.g. 1 m -> 1000 mm)
            Classes.Unit unit_Milimeter = new Classes.Unit(LengthUnit.Milimeter);
            double double_ToResult = unit_Milimeter.To(1.0);
            Assert.Equal(1000.0, double_ToResult, 5);

            double double_FromResult = unit_Milimeter.From(1000.0);
            Assert.Equal(1.0, double_FromResult, 5);

            // 2. Verify bug fix: AngleUnit.Degree factor
            bool_Converted = Query.TryConvert(180.0, AngleUnit.Degree, AngleUnit.Radian, out double_Value);
            Assert.True(bool_Converted);
            Assert.NotNull(double_Value);
            Assert.Equal(Math.PI, double_Value.Value, 5);

            bool_Converted = Query.TryConvert(Math.PI, AngleUnit.Radian, AngleUnit.Degree, out double_Value);
            Assert.True(bool_Converted);
            Assert.NotNull(double_Value);
            Assert.Equal(180.0, double_Value.Value, 5);

            // 3. Verify bug fix: DensityUnit.GramPerCubicCentimeter factor
            bool_Converted = Query.TryConvert(1.0, DensityUnit.GramPerCubicCentimeter, DensityUnit.KilogramPerCubicMeter, out double_Value);
            Assert.True(bool_Converted);
            Assert.NotNull(double_Value);
            Assert.Equal(1000.0, double_Value.Value, 5);

            // 4. Verify new MassUnit conversions
            bool_Converted = Query.TryConvert(1.5, MassUnit.Kilogram, MassUnit.Gram, out double_Value);
            Assert.True(bool_Converted);
            Assert.NotNull(double_Value);
            Assert.Equal(1500.0, double_Value.Value, 5);

            // 5. Verify direct conversions between Milli and Micro units (as requested)
            // Mass: 1 Milligram to Micrograms
            bool_Converted = Query.TryConvert(1.0, MassUnit.Milligram, MassUnit.Microgram, out double_Value);
            Assert.True(bool_Converted);
            Assert.NotNull(double_Value);
            Assert.Equal(1000.0, double_Value.Value, 5);

            bool_Converted = Query.TryConvert(1000.0, MassUnit.Microgram, MassUnit.Milligram, out double_Value);
            Assert.True(bool_Converted);
            Assert.NotNull(double_Value);
            Assert.Equal(1.0, double_Value.Value, 5);

            // Electric Current: 1 Milliampere to Microamperes
            bool_Converted = Query.TryConvert(1.0, ElectricCurrentUnit.Milliampere, ElectricCurrentUnit.Microampere, out double_Value);
            Assert.True(bool_Converted);
            Assert.NotNull(double_Value);
            Assert.Equal(1000.0, double_Value.Value, 5);

            // Amount of Substance: 1 Millimole to Micromoles
            bool_Converted = Query.TryConvert(1.0, AmountOfSubstanceUnit.Millimole, AmountOfSubstanceUnit.Micromole, out double_Value);
            Assert.True(bool_Converted);
            Assert.NotNull(double_Value);
            Assert.Equal(1000.0, double_Value.Value, 5);

            // Luminous Intensity: 1 Millicandela to Microcandelas
            bool_Converted = Query.TryConvert(1.0, LuminousIntensityUnit.Millicandela, LuminousIntensityUnit.Microcandela, out double_Value);
            Assert.True(bool_Converted);
            Assert.NotNull(double_Value);
            Assert.Equal(1000.0, double_Value.Value, 5);

            // Force: 1 Millinewton to Micronewtons
            bool_Converted = Query.TryConvert(1.0, ForceUnit.Millinewton, ForceUnit.Micronewton, out double_Value);
            Assert.True(bool_Converted);
            Assert.NotNull(double_Value);
            Assert.Equal(1000.0, double_Value.Value, 5);

            // Pressure: 1 Millipascal to Micropascals
            bool_Converted = Query.TryConvert(1.0, PressureUnit.Millipascal, PressureUnit.Micropascal, out double_Value);
            Assert.True(bool_Converted);
            Assert.NotNull(double_Value);
            Assert.Equal(1000.0, double_Value.Value, 5);

            // Energy: 1 Millijoule to Microjoules
            bool_Converted = Query.TryConvert(1.0, EnergyUnit.Millijoule, EnergyUnit.Microjoule, out double_Value);
            Assert.True(bool_Converted);
            Assert.NotNull(double_Value);
            Assert.Equal(1000.0, double_Value.Value, 5);

            // Power: 1 Milliwatt to Microwatts
            bool_Converted = Query.TryConvert(1.0, PowerUnit.Milliwatt, PowerUnit.Microwatt, out double_Value);
            Assert.True(bool_Converted);
            Assert.NotNull(double_Value);
            Assert.Equal(1000.0, double_Value.Value, 5);

            // Electric Potential: 1 Millivolt to Microvolts
            bool_Converted = Query.TryConvert(1.0, ElectricPotentialUnit.Millivolt, ElectricPotentialUnit.Microvolt, out double_Value);
            Assert.True(bool_Converted);
            Assert.NotNull(double_Value);
            Assert.Equal(1000.0, double_Value.Value, 5);

            // Electric Resistance: 1 Milliohm to Microohms
            bool_Converted = Query.TryConvert(1.0, ElectricResistanceUnit.Milliohm, ElectricResistanceUnit.Microohm, out double_Value);
            Assert.True(bool_Converted);
            Assert.NotNull(double_Value);
            Assert.Equal(1000.0, double_Value.Value, 5);

            // 6. Verify other derived unit categories (Speed, Frequency, Acceleration, Illuminance)
            bool_Converted = Query.TryConvert(72.0, SpeedUnit.KilometerPerHour, SpeedUnit.MeterPerSecond, out double_Value);
            Assert.True(bool_Converted);
            Assert.NotNull(double_Value);
            Assert.Equal(20.0, double_Value.Value, 5);

            bool_Converted = Query.TryConvert(1.0, FrequencyUnit.Kilohertz, FrequencyUnit.Hertz, out double_Value);
            Assert.True(bool_Converted);
            Assert.NotNull(double_Value);
            Assert.Equal(1000.0, double_Value.Value, 5);

            bool_Converted = Query.TryConvert(1.0, AccelerationUnit.StandardGravity, AccelerationUnit.MeterPerSecondSquared, out double_Value);
            Assert.True(bool_Converted);
            Assert.NotNull(double_Value);
            Assert.Equal(9.80665, double_Value.Value, 5);

            bool_Converted = Query.TryConvert(10.0, IlluminanceUnit.Lux, IlluminanceUnit.Lux, out double_Value);
            Assert.True(bool_Converted);
            Assert.NotNull(double_Value);
            Assert.Equal(10.0, double_Value.Value, 5);
        }
    }
}