using System.IO;
using System.Reflection;

namespace DiGi.Core.xUnit
{
    public static partial class Query
    {
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