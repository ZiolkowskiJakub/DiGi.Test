using System.IO;
using System.Reflection;

namespace DiGi.Core.xUnit
{
    public static partial class Query
    {
        /// <summary>
        /// Retrieves the path to the "user files/reports" directory relative to the location of the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly used as the reference point for calculating the directory path.</param>
        /// <returns>The absolute path to the "user files/reports" directory, creating it if it does not exist, or <see langword="null"/> if the directory cannot be resolved.</returns>
        public static string? ReportsDirectory(this Assembly? assembly)
        {
            string? userFilesDirectory = UserFilesDirectory(assembly);
            if (string.IsNullOrWhiteSpace(userFilesDirectory))
            {
                return null;
            }

            string path_Reports = Path.Combine(userFilesDirectory!, "reports");
            if (!Directory.Exists(path_Reports))
            {
                Directory.CreateDirectory(path_Reports);
            }

            return path_Reports;
        }
    }
}
