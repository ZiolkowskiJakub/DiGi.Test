using System.IO;
using System.Reflection;

namespace DiGi.Core.xUnit
{
    public static partial class Query
    {
        public static string? FilesDirectory(this Assembly? assembly)
        {
            Assert.NotNull(assembly);

            string? directory = Path.GetDirectoryName(Path.GetDirectoryName(assembly.Location));
            if(string.IsNullOrWhiteSpace(directory))
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