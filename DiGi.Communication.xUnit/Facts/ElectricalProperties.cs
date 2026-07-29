using DiGi.Communication.Classes;
using DiGi.Core.Classes;

namespace DiGi.Communication.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the creation, properties, calculations, equality operators, and serialization of <see cref="Classes.ElectricalProperties"/>.
        /// </summary>
        [Fact]
        public void ElectricalProperties()
        {
            string name = "Concrete";
            double a = 5.31;
            double b = 0.0;
            double c = 0.0326;
            double d = 0.8095;
            Range<double> frequencyRange = new(1, 100);

            ElectricalProperties electricalProperties_1 = new(name, a, b, c, d, frequencyRange);

            Assert.Equal(name, electricalProperties_1.Name);
            Assert.Equal(a, electricalProperties_1.A);
            Assert.Equal(b, electricalProperties_1.B);
            Assert.Equal(c, electricalProperties_1.C);
            Assert.Equal(d, electricalProperties_1.D);
            Assert.NotNull(electricalProperties_1.FrequencyRange);
            Assert.Equal(1, electricalProperties_1.FrequencyRange.Min);
            Assert.Equal(100, electricalProperties_1.FrequencyRange.Max);

            double frequency = 2.4e9;
            double relativePermittivity = Query.RelativePermittivity(electricalProperties_1, frequency);
            double conductivity = Query.Conductivity(electricalProperties_1, frequency);

            Assert.False(double.IsNaN(relativePermittivity));
            Assert.False(double.IsNaN(conductivity));
            Assert.True(relativePermittivity > 0);
            Assert.True(conductivity > 0);

            ElectricalProperties electricalProperties_2 = new(electricalProperties_1);

            Assert.True(electricalProperties_1 == electricalProperties_2);
            Assert.False(electricalProperties_1 != electricalProperties_2);
            Assert.True(electricalProperties_1.Equals(electricalProperties_2));
            Assert.Equal(electricalProperties_1.GetHashCode(), electricalProperties_2.GetHashCode());

            ElectricalProperties electricalProperties_3 = Constants.ElectricalProperties.Vacuum;
            Assert.NotNull(electricalProperties_3);
            Assert.Equal("Vacuum", electricalProperties_3.Name);
            Assert.False(electricalProperties_1 == electricalProperties_3);

            ElectricalProperties electricalProperties_DistinctInstance = new(name, a, b, c, d, new Range<double>(1, 100));
            Assert.True(electricalProperties_1.Equals(electricalProperties_DistinctInstance));
            Assert.Equal(electricalProperties_1.GetHashCode(), electricalProperties_DistinctInstance.GetHashCode());

            Dictionary<ElectricalProperties, string> testDictionary = [];
            testDictionary[electricalProperties_1] = "Value1";
            Assert.True(testDictionary.TryGetValue(electricalProperties_DistinctInstance, out string? dictValue));
            Assert.Equal("Value1", dictValue);

            Core.xUnit.Query.SerializationCheck(electricalProperties_1);
        }
    }
}
