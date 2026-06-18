using System.IO;
using System.Reflection;

namespace DiGi.Core.xUnit
{
    public static partial class Query
    {
        /// <summary>
        /// Retrieves the path to the "files" directory relative to the location of the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly used as the reference point for calculating the directory path.</param>
        /// <returns>The absolute path to the "files" directory, or <see langword="null"/> if the directory cannot be resolved.</returns>
        public static string? FilesDirectory(this Assembly? assembly)
        {
            Assert.NotNull(assembly);

            string? directory = Path.GetDirectoryName(Path.GetDirectoryName(assembly.Location));
            if (string.IsNullOrWhiteSpace(directory))
            {
                return null;
            }

            directory = Path.GetDirectoryName(directory);
            if (string.IsNullOrWhiteSpace(directory))
            {
                return null;
            }

            return Path.Combine(directory!, "files");
        }
    }
}
