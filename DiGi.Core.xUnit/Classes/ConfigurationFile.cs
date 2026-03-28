using DiGi.Core.Classes;
using System.Collections.Generic;
using System.IO;

namespace DiGi.Core.xUnit
{
    public partial class Classes
    {
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

            configurationFile.Add("SOME_PROPERTY 4", false);
            value = configurationFile.GetValue<bool>("SOME_PROPERTY 4");
            Assert.NotNull(value);
            Assert.True(value.GetType() == typeof(bool));

            object? value_Temp = configurationFile.GetValue<bool>("SOME_PROPERTy 4");
            Assert.True(value_Temp.Equals(value));

            Assert.True(configurationFile.Contains("SOME_PROPERTy 4"));
            Assert.True(configurationFile.Contains("SOME_PROPERTy 4 "));
            Assert.True(configurationFile.Contains("SOME_PROPERTY 4 "));

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