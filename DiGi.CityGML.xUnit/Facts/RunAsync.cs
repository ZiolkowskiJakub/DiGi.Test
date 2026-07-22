using DiGi.CityGML.Classes;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.CityGML.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that returning false from the walk callback stops the walk before the next file is parsed.
        /// <para>The callback previously returned void, so a caller that had already failed could only discard work the walker had gone on to do - on a large directory that meant parsing the entire remainder for nothing.</para>
        /// </summary>
        [Fact]
        public async Task RunAsync_StopsOnFalse()
        {
            string path_Zip = ZipWithEntries(5);

            try
            {
                int count = 0;

                bool result = await Query.RunAsync(path_Zip, (path, cityModel) =>
                {
                    count++;
                    return Task.FromResult(false);
                });

                Assert.False(result);
                Assert.Equal(1, count);
            }
            finally
            {
                File.Delete(path_Zip);
            }
        }

        /// <summary>
        /// Tests that the walk observes cancellation before parsing each file rather than after the whole tree has been read.
        /// <para>The token is checked ahead of the parse because the parse is the expensive part; a cancelled import must stop within one file.</para>
        /// </summary>
        [Fact]
        public async Task RunAsync_StopsOnCancellation()
        {
            string path_Zip = ZipWithEntries(5);

            try
            {
                using CancellationTokenSource cancellationTokenSource = new();

                int count = 0;

                await Assert.ThrowsAnyAsync<System.OperationCanceledException>(async () =>
                {
                    await Query.RunAsync(path_Zip, (path, cityModel) =>
                    {
                        count++;
                        cancellationTokenSource.Cancel();
                        return Task.FromResult(true);
                    }, cancellationTokenSource.Token);
                });

                Assert.Equal(1, count);
            }
            finally
            {
                File.Delete(path_Zip);
            }
        }

        /// <summary>
        /// Tests that a walk which is neither cancelled nor stopped still visits every entry.
        /// <para>Counterpart to the two stop cases, so the early-abort checks cannot pass by aborting a run that should have completed.</para>
        /// </summary>
        [Fact]
        public async Task RunAsync_VisitsEveryEntry()
        {
            const int count_Expected = 5;

            string path_Zip = ZipWithEntries(count_Expected);

            try
            {
                int count = 0;

                bool result = await Query.RunAsync(path_Zip, (path, cityModel) =>
                {
                    count++;

                    Assert.NotNull(cityModel);

                    return Task.FromResult(true);
                });

                Assert.True(result);
                Assert.Equal(count_Expected, count);
            }
            finally
            {
                File.Delete(path_Zip);
            }
        }

        /// <summary>
        /// Builds a temporary zip archive holding the given number of copies of the CityGML fixture.
        /// </summary>
        /// <param name="count">The number of entries to write.</param>
        /// <returns>The path of the created archive.</returns>
        private static string ZipWithEntries(int count)
        {
            string? path_Gml = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "2862_N-34-77-D-b-1-1.gml");

            Assert.True(File.Exists(path_Gml));

            string result = Path.Combine(Path.GetTempPath(), string.Format("DiGi_CityGML_RunAsync_{0}.zip", System.Guid.NewGuid()));

            using FileStream fileStream = new(result, FileMode.Create);
            using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);

            for (int i = 0; i < count; i++)
            {
                ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(string.Format("2862_N-34-77-D-b-1-{0}.gml", i));

                using Stream stream = zipArchiveEntry.Open();
                using FileStream fileStream_Gml = new(path_Gml!, FileMode.Open, FileAccess.Read);

                fileStream_Gml.CopyTo(stream);
            }

            return result;
        }
    }
}
