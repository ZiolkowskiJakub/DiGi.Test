using System.Collections.Generic;
using System.Linq;

namespace DiGi.Python.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.PythonPackageVersion"/> keeps the values given to it, survives a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies every member, with and without a version.
        /// </summary>
        [Fact]
        public void PythonPackageVersion()
        {
            Classes.PythonPackageVersion pythonPackageVersion = new("torch", "2.7.0+cu128");

            Assert.Equal("torch", pythonPackageVersion.Name);
            Assert.Equal("2.7.0+cu128", pythonPackageVersion.Version);

            string? json = Core.Convert.ToSystem_String(pythonPackageVersion);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.PythonPackageVersion>? pythonPackageVersions = Core.Convert.ToDiGi<Classes.PythonPackageVersion>(json);
            Classes.PythonPackageVersion? pythonPackageVersion_RoundTrip = pythonPackageVersions?.FirstOrDefault();
            Assert.NotNull(pythonPackageVersion_RoundTrip);
            Assert.Equal("torch", pythonPackageVersion_RoundTrip!.Name);
            Assert.Equal("2.7.0+cu128", pythonPackageVersion_RoundTrip.Version);

            Core.xUnit.Query.SerializationCheck(pythonPackageVersion);

            Classes.PythonPackageVersion pythonPackageVersion_Copy = new(pythonPackageVersion);
            Assert.Equal(pythonPackageVersion.Name, pythonPackageVersion_Copy.Name);
            Assert.Equal(pythonPackageVersion.Version, pythonPackageVersion_Copy.Version);

            //A package that could not be imported has no version
            Classes.PythonPackageVersion pythonPackageVersion_Missing = new("transformers", null);
            Assert.Null(pythonPackageVersion_Missing.Version);
            Core.xUnit.Query.SerializationCheck(pythonPackageVersion_Missing);

            Classes.PythonPackageVersion pythonPackageVersion_MissingCopy = new(pythonPackageVersion_Missing);
            Assert.Equal("transformers", pythonPackageVersion_MissingCopy.Name);
            Assert.Null(pythonPackageVersion_MissingCopy.Version);
        }
    }
}
