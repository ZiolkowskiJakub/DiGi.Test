using System.IO;
using System.Reflection;

namespace DiGi.Python.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Modify.WriteScripts(Assembly, string, string, System.Collections.Generic.IEnumerable{string}, string?, System.Collections.Generic.IEnumerable{string}?)"/> writes embedded resources into a new directory, rewrites scripts on every call while leaving an existing keep-existing file alone, prefers a folder beside the assembly over the resources, and reports a missing resource or directory as <c>false</c>.
        /// </summary>
        [Fact]
        public void WriteScripts()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string resourcePrefix = "DiGi.Python.xUnit.Resources.Scripts.";
            string sourceDirectoryName = "DiGi_Python_Scripts_" + Path.GetRandomFileName();
            string[] fileNames = ["main.py", "helper.py", "conf.yaml"];
            string[] fileNames_KeepExisting = ["conf.yaml"];

            string directory = Path.Combine(Path.GetTempPath(), "DiGi_Python_Test_" + Path.GetRandomFileName(), "scripts");
            string directory_Source = Path.Combine(Path.GetDirectoryName(assembly.Location)!, sourceDirectoryName);

            string ReadResource(string fileName)
            {
                using Stream stream = assembly.GetManifestResourceStream(resourcePrefix + fileName)!;
                using StreamReader streamReader = new(stream);
                return streamReader.ReadToEnd();
            }

            try
            {
                //Written from the embedded resources into a directory that does not exist yet
                Assert.True(Modify.WriteScripts(assembly, resourcePrefix, sourceDirectoryName, fileNames, directory, fileNames_KeepExisting));
                foreach (string fileName in fileNames)
                {
                    Assert.Equal(ReadResource(fileName), File.ReadAllText(Path.Combine(directory, fileName)));
                }

                //A script is rewritten, a keep-existing file written by other means is left alone
                File.WriteAllText(Path.Combine(directory, "main.py"), "edited");
                File.WriteAllText(Path.Combine(directory, "conf.yaml"), "name: dataset");

                Assert.True(Modify.WriteScripts(assembly, resourcePrefix, sourceDirectoryName, fileNames, directory, fileNames_KeepExisting));
                Assert.Equal(ReadResource("main.py"), File.ReadAllText(Path.Combine(directory, "main.py")));
                Assert.Equal("name: dataset", File.ReadAllText(Path.Combine(directory, "conf.yaml")));

                //Without the keep-existing list every file is rewritten
                Assert.True(Modify.WriteScripts(assembly, resourcePrefix, sourceDirectoryName, fileNames, directory));
                Assert.Equal(ReadResource("conf.yaml"), File.ReadAllText(Path.Combine(directory, "conf.yaml")));

                //A file in the folder beside the assembly wins over its resource; the others still come from the resources
                Directory.CreateDirectory(directory_Source);
                File.WriteAllText(Path.Combine(directory_Source, "main.py"), "print('edited in the build output')");

                Assert.True(Modify.WriteScripts(assembly, resourcePrefix, sourceDirectoryName, fileNames, directory, fileNames_KeepExisting));
                Assert.Equal("print('edited in the build output')", File.ReadAllText(Path.Combine(directory, "main.py")));
                Assert.Equal(ReadResource("helper.py"), File.ReadAllText(Path.Combine(directory, "helper.py")));

                //A missing resource fails the call without stopping the other files
                File.Delete(Path.Combine(directory, "helper.py"));
                Assert.False(Modify.WriteScripts(assembly, resourcePrefix, sourceDirectoryName, ["missing.py", "helper.py"], directory));
                Assert.False(File.Exists(Path.Combine(directory, "missing.py")));
                Assert.True(File.Exists(Path.Combine(directory, "helper.py")));

                Assert.False(Modify.WriteScripts(assembly, resourcePrefix, sourceDirectoryName, fileNames, null));
                Assert.False(Modify.WriteScripts(assembly, resourcePrefix, sourceDirectoryName, fileNames, "  "));
            }
            finally
            {
                string? directory_Root = Path.GetDirectoryName(directory);
                if (directory_Root != null && Directory.Exists(directory_Root))
                {
                    Directory.Delete(directory_Root, true);
                }

                if (Directory.Exists(directory_Source))
                {
                    Directory.Delete(directory_Source, true);
                }
            }
        }
    }
}
