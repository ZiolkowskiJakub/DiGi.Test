using System.ComponentModel;
using DiGi.Core.Interfaces;
using DiGi.Core.Parameters;

namespace DiGi.Core.Test.Enums
{
    [AssociatedTypes(typeof(ISerializableObject)), Description("Test Parameter Definition")]
    public enum TestParameterDefinition
    {
        [ParameterProperties("fc738c9c-49f8-41f7-ab63-c56bb2417836" , "Test", "Test"), DoubleParameterValue()] Test,
    }
}