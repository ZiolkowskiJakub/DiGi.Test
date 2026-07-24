using DiGi.Core.Classes;
using System.Collections.Generic;
using System.IO;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the functionality of the ConfigurationFile class to ensure that configuration properties can be added, retrieved with correct types, and checked for existence regardless of case or trailing whitespace.
        /// </summary>
        [Fact]
        public void ConfigurationFile()
        {
            ConfigurationFile? configurationFile;

            configurationFile = new();

            object? value;

            configurationFile.Add("SOME_PROPERTY 1", "VALUE");
            value = configurationFile.GetValue("SOME_PROPERTY 1");
            Assert.NotNull(value);
            Assert.True(value.GetType() == typeof(string));

            configurationFile.Add("SOME_PROPERTY 2", 2.0);
            value = configurationFile.GetValue<double>("SOME_PROPERTY 2");
            Assert.NotNull(value);
            Assert.True(value.GetType() == typeof(double));

            configurationFile.Add("SOME_PROPERTY 3", 1);
            value = configurationFile.GetValue<int>("SOME_PROPERTY 3");
            Assert.NotNull(value);
            Assert.True(value.GetType() == typeof(int));

            configurationFile.Add("SOME_PROPERTY 4", true);
            value = configurationFile.GetValue<bool>("SOME_PROPERTY 4");
            Assert.NotNull(value);
            Assert.True(value.GetType() == typeof(bool));

            // Verify case-insensitive lookup of a true value (tests the fix for case-insensitive TryGetValue)
            object? value_Temp = configurationFile.GetValue<bool>("SOME_PROPERTy 4");
            Assert.True(value_Temp.Equals(value));

            Assert.True(configurationFile.Contains("SOME_PROPERTy 4"));
            Assert.True(configurationFile.Contains("SOME_PROPERTy 4 "));
            Assert.True(configurationFile.Contains("SOME_PROPERTY 4 "));

            // Verify the GetValue<T>(name, defaultValue, caseSensitive) overload (tests the fix for returning defaultValue)
            // We use named arguments or distinct types to ensure correct overload resolution and avoid ambiguity with the caseSensitive overload.
            bool bool_DefaultTest1 = configurationFile.GetValue("SOME_PROPERTY 4", defaultValue: false);
            Assert.True(bool_DefaultTest1);

            bool bool_DefaultTest2 = configurationFile.GetValue("NON_EXISTENT_PROPERTY", defaultValue: true);
            Assert.True(bool_DefaultTest2);

            int int_DefaultTest = configurationFile.GetValue("NON_EXISTENT_PROPERTY", defaultValue: 42);
            Assert.Equal(42, int_DefaultTest);

            List<string> names = configurationFile.Names;
            Assert.NotNull(names);
            Assert.NotEmpty(names);
            Assert.Equal(4, names.Count);

            string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());

            configurationFile.Write(path);

            configurationFile = Create.ConfigurationFile(path);

            File.Delete(path);

            Assert.NotNull(configurationFile);

            names = configurationFile.Names;
            Assert.NotNull(names);
            Assert.NotEmpty(names);
            Assert.Equal(4, names.Count);
        }
    }
}