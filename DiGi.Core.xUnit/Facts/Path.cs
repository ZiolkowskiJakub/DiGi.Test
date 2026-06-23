using System.IO;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the functionality of the <see cref="Core.Classes.Path"/> class, verifying operations such as retrieving the file extension, validating the path, and obtaining <see cref="FileInfo"/>.
        /// </summary>
        [Fact]
        public void Path()
        {
            Core.Classes.Path path = @"Z:\DiGi\Line3Da.txt";

            string? extension = path.Extension;

            bool valid = path.IsValid();

            FileInfo? fileInfo = path.GetFileInfo();

            Assert.Equal(".txt", extension);
            Assert.True(valid);
            Assert.NotNull(fileInfo);
        }
    }
}