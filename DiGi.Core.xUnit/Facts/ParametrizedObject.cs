using DiGi.Core.Parameter.Classes;
using System;
using System.Linq;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the functionality of the <see cref="ParametrizedObject"/> class, specifically verifying value assignment via both simple keys and external parameter definitions, as well as ensuring serialization round-trip consistency.
        /// </summary>
        [Fact]
        public void ParametrizedObject()
        {
            string? json = null;

            ParametrizedObject parametrizedObject = new();
            parametrizedObject.SetValue("Test", "Some Text", new SetValueSettings() { CheckAccessType = false });

            ExternalParameterDefinition? externalParameterDefinition = DiGi.Core.Parameter.Create.ExternalParameterDefinition(Guid.NewGuid(), "Test 2", "Test description", DiGi.Core.Parameter.Enums.ParameterType.Double, typeof(ParametrizedObject), nullable: false);
            Assert.NotNull(externalParameterDefinition);

            parametrizedObject.SetValue(externalParameterDefinition, 20);

            json = Convert.ToSystem_String(parametrizedObject);
            Assert.NotNull(json);

            ParametrizedObject? parametrizedObject_Temp = Convert.ToDiGi<ParametrizedObject>(json)?.FirstOrDefault();
            Assert.NotNull(parametrizedObject_Temp);

            Assert.Equal(json, Convert.ToSystem_String(parametrizedObject_Temp));
        }
    }
}