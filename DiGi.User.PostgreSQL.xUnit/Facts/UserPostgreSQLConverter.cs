using DiGi.PostgreSQL;
using DiGi.PostgreSQL.Classes;
using DiGi.User.Classes;
using DiGi.User.Enums;
using DiGi.User.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.User.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a user can be created against a server holding no database for it at all - the database, the table and the credential columns are all brought into existence by the write path.
        /// <para>The order is the one <c>UIPostgreSQLUserCreateTask</c> uses, and it is the point of the fact: the duplicate check is a read, and a read repairs neither an absent database (the connection fails to open with 3D000) nor a database without the table (the statement fails with 42P01).</para>
        /// <para>Runs against a throwaway database name that is dropped either side of the run, so nothing it does can reach the real one. Requires a database: returns without asserting when User_PostgreSQL_Main.conf is absent or unreachable.</para>
        /// </summary>
        [Fact]
        public async Task UserPostgreSQLConverter_CreatesDatabase()
        {
            const string database = "user_xunit_createsdatabase";
            const string email = "xunit.createsdatabase@example.com";

            UserPostgreSQLConverterManager? userPostgreSQLConverterManager = DiGi.User.PostgreSQL.Create.UserPostgreSQLConverterManager();

            UserPostgreSQLConverter? userPostgreSQLConverter_Configured = userPostgreSQLConverterManager?.GetPostgreSQLConverter<UserPostgreSQLConverter>();
            if (userPostgreSQLConverter_Configured?.ConnectionData is null)
            {
                return;
            }

            ConnectionData connectionData = new(userPostgreSQLConverter_Configured.ConnectionData, database);
            UserPostgreSQLConverter userPostgreSQLConverter = new(connectionData);

            try
            {
                // A clean slate, so the fact proves creation rather than reuse of something a previous run left.
                await connectionData.RemoveDatabaseAsync(database, string.Empty);
            }
            catch (Npgsql.NpgsqlException)
            {
                // Nothing listening, wrong credentials, or no permission to drop. To a fact all of them mean the
                // same thing: there is no database to integrate against on this machine.
                return;
            }

            try
            {
                Assert.True(await userPostgreSQLConverter.CreateTableAsync());

                // The read that would have thrown 3D000, then 42P01, had the line above not run first.
                Assert.Null(await userPostgreSQLConverter.GetUserByEmailAsync(email));

                List<string> ids = await userPostgreSQLConverter.InsertAsync([new DiGi.User.Classes.User(email) { LastName = "xUnit", Level = (int)UserLevel.Admin }]);
                Assert.Single(ids);

                UserCredential? userCredential = DiGi.User.PostgreSQL.Create.UserCredential(email, "xunit-creates-database-password");
                Assert.NotNull(userCredential);
                Assert.True(await userPostgreSQLConverter.SetUserCredentialAsync(userCredential));

                DiGi.User.Classes.User? user = await userPostgreSQLConverter.GetUserByEmailAsync(email);
                Assert.NotNull(user);
                Assert.Equal(email, user.Email);
                Assert.Equal((int)UserLevel.Admin, user.Level);

                UserCredential? userCredential_Stored = await userPostgreSQLConverter.GetUserCredentialAsync(email);
                Assert.NotNull(userCredential_Stored);
                Assert.True(userCredential_Stored.IsPasswordValid("xunit-creates-database-password"));

                // Idempotent: every write path calls it, so it has to answer true against a database that is already there.
                Assert.True(await userPostgreSQLConverter.CreateTableAsync());
            }
            finally
            {
                await connectionData.RemoveDatabaseAsync(database, string.Empty);
            }
        }
    }
}
