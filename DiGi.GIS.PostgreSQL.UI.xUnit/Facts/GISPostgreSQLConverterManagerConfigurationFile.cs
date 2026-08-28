using DiGi.GIS.PostgreSQL.UI.Classes;
using System.IO;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="GISPostgreSQLConverterManagerConfigurationFile"/> correctly reads, writes, copies, and serializes the API authorization key setting.
        /// </summary>
        [Fact]
        public void GISPostgreSQLConverterManagerConfigurationFile_Key()
        {
            GISPostgreSQLConverterManagerConfigurationFile gISPostgreSQLConverterManagerConfigurationFile_Default = new();
            Assert.Null(gISPostgreSQLConverterManagerConfigurationFile_Default.Key);

            gISPostgreSQLConverterManagerConfigurationFile_Default.Key = "my-secret-key-123";
            Assert.Equal("my-secret-key-123", gISPostgreSQLConverterManagerConfigurationFile_Default.Key);

            GISPostgreSQLConverterManagerConfigurationFile gISPostgreSQLConverterManagerConfigurationFile_Copy = new(gISPostgreSQLConverterManagerConfigurationFile_Default);
            Assert.Equal("my-secret-key-123", gISPostgreSQLConverterManagerConfigurationFile_Copy.Key);

            Core.xUnit.Query.SerializationCheck(gISPostgreSQLConverterManagerConfigurationFile_Default);

            string path = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(path,
                [
                    "Key=\"test-config-key-456\""
                ]);

                GISPostgreSQLConverterManagerConfigurationFile? gISPostgreSQLConverterManagerConfigurationFile_File = Create.GISPostgreSQLConverterManagerConfigurationFile(path);
                Assert.NotNull(gISPostgreSQLConverterManagerConfigurationFile_File);
                Assert.Equal("test-config-key-456", gISPostgreSQLConverterManagerConfigurationFile_File.Key);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }
}
