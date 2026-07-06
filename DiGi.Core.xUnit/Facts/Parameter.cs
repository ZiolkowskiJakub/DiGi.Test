using DiGi.Core.Parameter.Classes;
using System.Linq;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the serialization and deserialization round-trip for <see cref="SimpleParameterDefinition"/> and <see cref="EnumParameterDefinition"/> to ensure they can be converted to a string representation and back to their original types correctly.
        /// </summary>
        [Fact]
        public void Parameter()
        {
            string? json = null;

            SimpleParameterDefinition simpleParameterDefinition = new("Test");

            json = Convert.ToSystem_String(simpleParameterDefinition);
            Assert.NotNull(json);

            SimpleParameterDefinition? simpleParameterDefinition_Temp = Convert.ToDiGi<SimpleParameterDefinition>(json)?.FirstOrDefault();
            Assert.NotNull(simpleParameterDefinition_Temp);
            Assert.Equal(simpleParameterDefinition.Name, simpleParameterDefinition_Temp.Name);

            EnumParameterDefinition enumParameterDefinition = new(TestEnum.Test1);
            json = Convert.ToSystem_String(enumParameterDefinition);
            Assert.NotNull(json);

            EnumParameterDefinition? enumParameterDefinition_Temp = Convert.ToDiGi<EnumParameterDefinition>(json)?.FirstOrDefault();
            Assert.NotNull(enumParameterDefinition_Temp);
            Assert.Equal(enumParameterDefinition.Name, enumParameterDefinition_Temp.Name);
        }
    }
}