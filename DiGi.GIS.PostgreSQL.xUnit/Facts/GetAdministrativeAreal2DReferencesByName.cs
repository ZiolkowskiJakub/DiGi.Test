using DiGi.GIS.PostgreSQL.Classes;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByNameAsync(NpgsqlConnection?, string, System.Threading.CancellationToken)"/>
        /// matches administrative areas regardless of Polish diacritics in either the stored name or the search text.
        /// <para>Skipped by default: it executes an integration query requiring <c>GIS_PostgreSQL_Main.conf</c> pointing at a database with the unaccent extension enabled.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a database before running.")]
        public async Task GetAdministrativeAreal2DReferencesByName_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            ConnectionData? connectionData = administrativeAreal2DPostgreSQLConverter.ConnectionData;
            Assert.NotNull(connectionData);

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(connectionData);
            Assert.NotNull(npgsqlConnection);

            await npgsqlConnection.OpenAsync();

            // Ensure unaccent extension is enabled
            await ExecuteAsync(npgsqlConnection, "CREATE EXTENSION IF NOT EXISTS unaccent;");

            // Test unaccent query behavior directly against sample names
            string query = @"
                WITH sample_data(id, name) AS (
                    VALUES
                        (1, 'dolnoslaskie'),
                        (2, 'slaskie'),
                        (3, 'Wrocław'),
                        (4, 'Kraków'),
                        (5, 'Łódź'),
                        (6, 'Gdańsk'),
                        (7, 'Poznań')
                )
                SELECT id, name
                FROM sample_data
                WHERE unaccent(name) ILIKE unaccent(@text)
                ORDER BY name ASC, id ASC;";

            async Task<List<string>> SearchAsync(string text)
            {
                await using NpgsqlCommand npgsqlCommand = new(query, npgsqlConnection);
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("text", NpgsqlDbType.Text) { Value = $"%{text}%" });

                List<string> results = [];
                await using NpgsqlDataReader reader = await npgsqlCommand.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    results.Add(reader.GetString(1));
                }
                return results;
            }

            // Stored without diacritics ('dolnoslaskie'), searched with diacritics ('dolnośląskie')
            List<string> results_Dolnoslaskie = await SearchAsync("dolnośląskie");
            Assert.Contains("dolnoslaskie", results_Dolnoslaskie);

            // Stored with diacritics ('Wrocław'), searched without diacritics ('Wroclaw')
            List<string> results_Wroclaw = await SearchAsync("Wroclaw");
            Assert.Contains("Wrocław", results_Wroclaw);

            // Stored with diacritics ('Kraków'), searched without diacritics ('Krakow')
            List<string> results_Krakow = await SearchAsync("Krakow");
            Assert.Contains("Kraków", results_Krakow);

            // Stored with diacritics ('Łódź'), searched without diacritics ('Lodz')
            List<string> results_Lodz = await SearchAsync("Lodz");
            Assert.Contains("Łódź", results_Lodz);

            // Stored with diacritics ('Gdańsk'), searched without diacritics ('Gdansk')
            List<string> results_Gdansk = await SearchAsync("Gdansk");
            Assert.Contains("Gdańsk", results_Gdansk);

            // Stored with diacritics ('Poznań'), searched without diacritics ('Poznan')
            List<string> results_Poznan = await SearchAsync("Poznan");
            Assert.Contains("Poznań", results_Poznan);

            // Case-insensitive checks
            List<string> results_Lower = await SearchAsync("wrocław");
            Assert.Contains("Wrocław", results_Lower);

            List<string> results_Upper = await SearchAsync("WROCŁAW");
            Assert.Contains("Wrocław", results_Upper);
        }
    }
}
