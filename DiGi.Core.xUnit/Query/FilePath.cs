using System.IO;
using System.Reflection;

namespace DiGi.Core.xUnit
{
    public static partial class Query
    {
        /// <summary>
        /// Constructs the full file path for a specified file within the directory associated with the provided assembly.
        /// </summary>
        /// <param name="assembly">The assembly used to determine the base files directory.</param>
        /// <param name="fileName">The name of the file to locate.</param>
        /// <returns>The combined full path to the specified file.</returns>
        public static string? FilePath(this Assembly? assembly, string fileName)
        {
            Assert.NotNull(assembly);
            Assert.False(string.IsNullOrWhiteSpace(fileName));

            string? filesDirectory = FilesDirectory(assembly);
            Assert.False(string.IsNullOrWhiteSpace(filesDirectory));

            return Path.Combine(filesDirectory, fileName);
        }
    }
}
