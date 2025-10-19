using DiGi.Core.Classes;

namespace DiGi.Core.xUnit
{
    public partial class Classes
    {
        [Fact]
        public void ConfigurationFile()
        {
            ConfigurationFile? configurationFile;

            configurationFile = new ();
            
            object? value;

            configurationFile.Add("SOME_PROPERTY 1", "VALUE");
            value = configurationFile.GetValue("SOME_PROPERTY 1");
            Assert.NotNull(value);
            Assert.True(value.GetType() == typeof(string));

            configurationFile.Add("SOME_PROPERTY 2", 2.0);
            value = configurationFile.GetValue("SOME_PROPERTY 2");
            Assert.NotNull(value);
            Assert.True(value.GetType() == typeof(double));

            configurationFile.Add("SOME_PROPERTY 3", 1);
            value = configurationFile.GetValue("SOME_PROPERTY 3");
            Assert.NotNull(value);
            Assert.True(value.GetType() == typeof(int));

            configurationFile.Add("SOME_PROPERTY 4", false);
            value = configurationFile.GetValue("SOME_PROPERTY 4");
            Assert.NotNull(value);
            Assert.True(value.GetType() == typeof(bool));

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