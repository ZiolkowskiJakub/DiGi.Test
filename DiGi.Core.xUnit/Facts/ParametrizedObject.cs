using DiGi.Core.Parameter.Classes;
using DiGi.Core.Parameter;
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

        /// <summary>
        /// Tests that ParametrizedObject.TryGetValue successfully retrieves a set parameter value and safely returns false on a non-existent parameter without throwing an exception or causing a stack overflow.
        /// </summary>
        [Fact]
        public void ParametrizedObject_TryGetValue_ShouldNotStackOverflow()
        {
            ParametrizedObject parametrizedObject = new();
            parametrizedObject.SetValue("ExistingKey", "Value123");

            bool result_Exists = parametrizedObject.TryGetValue("ExistingKey", out object? value_Exists);
            Assert.True(result_Exists);
            Assert.Equal("Value123", value_Exists);

            bool result_Missing = parametrizedObject.TryGetValue("NonExistentKey", out object? value_Missing);
            Assert.False(result_Missing);
            Assert.Null(value_Missing);
        }

        /// <summary>
        /// Verifies the performance and correctness of cloning a ParametrizedObject.
        /// </summary>
        [Fact]
        public void ParametrizedObject_Clone_Performance()
        {
            ParametrizedObject parametrizedObject = new();
            parametrizedObject.SetValue("StringParam", "Test");
            parametrizedObject.SetValue("IntParam", 42);
            parametrizedObject.SetValue("DoubleParam", 3.14);

            // Warm-up
            ParametrizedObject? clone_WarmUp = parametrizedObject.Clone<ParametrizedObject>();
            Assert.NotNull(clone_WarmUp);

            int count = 10000;
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < count; i++)
            {
                ParametrizedObject? clone = parametrizedObject.Clone<ParametrizedObject>();
                Assert.NotNull(clone);
                Assert.Equal("Test", clone.GetValue("StringParam"));
            }

            stopwatch.Stop();

            // The optimized clone should easily complete 10,000 deep clones of ParametrizedObject in less than 50 milliseconds.
            Assert.True(stopwatch.ElapsedMilliseconds < 50, $"Cloning {count} ParametrizedObject instances took {stopwatch.ElapsedMilliseconds} ms, which exceeds the 50 ms threshold.");
        }

        /// <summary>
        /// Tests retrieving enumeration values from a <see cref="ParametrizedObject"/> using <see cref="ParametrizedObject.GetValue{T}(GetValueSettings?)"/> with <see cref="DiGi.Core.Enums.CountryCode"/>.
        /// </summary>
        [Fact]
        public void ParametrizedObject_GetValue_Enum()
        {
            ParametrizedObject parametrizedObject = new();

            // Initially, retrieving an unset enum should return null.
            DiGi.Core.Enums.CountryCode? countryCode_Initial = parametrizedObject.GetValue<DiGi.Core.Enums.CountryCode?>();
            Assert.Null(countryCode_Initial);

            // Set value via Enum definition key
            bool result_SetEnumKey = parametrizedObject.SetValue(DiGi.Core.Enums.CountryCode.PL, DiGi.Core.Enums.CountryCode.PL);
            Assert.True(result_SetEnumKey);

            // Test GetValue<CountryCode>() without key argument
            DiGi.Core.Enums.CountryCode? countryCode_GetValueNoArg = parametrizedObject.GetValue<DiGi.Core.Enums.CountryCode>();
            Assert.Equal(DiGi.Core.Enums.CountryCode.PL, countryCode_GetValueNoArg);

            // Test GetValue<CountryCode>(Enum) with enum member key
            DiGi.Core.Enums.CountryCode? countryCode_GetValueByEnumKey = parametrizedObject.GetValue<DiGi.Core.Enums.CountryCode>(DiGi.Core.Enums.CountryCode.PL);
            Assert.Equal(DiGi.Core.Enums.CountryCode.PL, countryCode_GetValueByEnumKey);

            // Test GetValue(Enum) returning object
            object? object_GetValueByEnumKey = parametrizedObject.GetValue(DiGi.Core.Enums.CountryCode.PL);
            Assert.Equal(DiGi.Core.Enums.CountryCode.PL, object_GetValueByEnumKey);

            // Set another parameter with simple string key and string enum name value
            parametrizedObject.SetValue("CountryParam", "DE");

            // Test GetValue<CountryCode>(string) by string parameter name with TryConvert enabled
            GetValueSettings getValueSettings = new(tryConvert: true, checkAccessType: false);
            DiGi.Core.Enums.CountryCode? countryCode_GetValueByStringKey = parametrizedObject.GetValue<DiGi.Core.Enums.CountryCode>("CountryParam", getValueSettings);
            Assert.Equal(DiGi.Core.Enums.CountryCode.DE, countryCode_GetValueByStringKey);
        }
    }
}