using DiGi.Core.Interfaces;
using DiGi.Core.Parameter.Classes;
using System.ComponentModel;

namespace DiGi.Core.Test.Enums
{
    /// <summary>Defines the available test parameter definitions.</summary>
    [AssociatedTypes(typeof(ISerializableObject)), Description("Test Parameter Definition")]
    public enum TestParameterDefinition
    {
        /// <summary>Represents the test parameter definition.</summary>
        [ParameterProperties("fc738c9c-49f8-41f7-ab63-c56bb2417836", "Test", "Test"), DoubleParameterValue()] Test,
    }
}
