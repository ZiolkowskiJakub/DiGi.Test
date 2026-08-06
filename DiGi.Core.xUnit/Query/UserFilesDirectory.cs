using System.IO;
using System.Reflection;

namespace DiGi.Core.xUnit
{
    public static partial class Query
    {
        /// <summary>
        /// Retrieves the path to the "user files" directory relative to the location of the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly used as the reference point for calculating the directory path.</param>
        /// <returns>The absolute path to the "user files" directory, creating it if it does not exist, or <see langword="null"/> if the directory cannot be resolved.</returns>
        public static string? UserFilesDirectory(this Assembly? assembly)
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

            string path_UserFiles = Path.Combine(directory!, "user files");
            if (!Directory.Exists(path_UserFiles))
            {
                Directory.CreateDirectory(path_UserFiles);
            }

            return path_UserFiles;
        }
    }
}
