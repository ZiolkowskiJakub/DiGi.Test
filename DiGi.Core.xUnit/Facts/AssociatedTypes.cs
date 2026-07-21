using DiGi.Core.Parameter.Classes;
using System;
using System.Linq;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the serialization and deserialization round-trip for various associated types, specifically verifying that <see cref="ParameterValue"/>, <see cref="AssociatedTypes"/>, and <see cref="ExternalParameterDefinition"/> can be converted to a string representation and back to their original types correctly.
        /// </summary>
        [Fact]
        public void AssociatedTypes()
        {
            string? json = null;
            ParameterValue parameterValue = new DoubleParameterValue();

            json = Convert.ToSystem_String(parameterValue);
            Assert.NotNull(json);

            ParameterValue? parameterValue_Temp = Convert.ToDiGi<ParameterValue>(json)?.FirstOrDefault();
            Assert.NotNull(parameterValue_Temp);
            Assert.Equal(parameterValue.ParameterType, parameterValue_Temp!.ParameterType);

            AssociatedTypes associatedTypes = new(typeof(Core.Classes.Path));

            json = Convert.ToSystem_String(associatedTypes);
            Assert.NotNull(json);

            AssociatedTypes? associatedTypes_Temp = Convert.ToDiGi<AssociatedTypes>(json)?.FirstOrDefault();
            Assert.NotNull(associatedTypes_Temp);

            ExternalParameterDefinition? externalParameterDefinition = Core.Parameter.Create.ExternalParameterDefinition(Guid.NewGuid(), "Test", "Test description", Core.Parameter.Enums.ParameterType.Double, typeof(Core.Classes.Color), nullable: false);
            Assert.NotNull(externalParameterDefinition);

            json = Convert.ToSystem_String(externalParameterDefinition);
            Assert.NotNull(json);

            ExternalParameterDefinition? externalParameterDefinition_Temp = Convert.ToDiGi<ExternalParameterDefinition>(json)?.FirstOrDefault();
            Assert.NotNull(externalParameterDefinition_Temp);
            Assert.Equal(externalParameterDefinition!.Name, externalParameterDefinition_Temp!.Name);
        }
    }
}