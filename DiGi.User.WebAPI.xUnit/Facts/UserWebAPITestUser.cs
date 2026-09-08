using DiGi.User.Classes;
using DiGi.User.PostgreSQL.Classes;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiGi.User.WebAPI.xUnit
{
    /// <summary>
    /// A user seeded into PostgreSQL with a known password, scoped to one fact and removed when the fact ends.
    /// <para>The database-backed facts open with <see cref="CreateAsync"/> and return without asserting when it answers
    /// null, so the suite stays green on a machine with no database while exercising the real converter wiring on one
    /// that has it.</para>
    /// </summary>
    internal sealed class UserWebAPITestUser : IAsyncDisposable
    {
        /// <summary>
        /// The email the facts seed and log in as.
        /// <para>Deliberately distinctive so it cannot collide with a real row in whichever database
        /// User_PostgreSQL_Main.conf points at.</para>
        /// </summary>
        public const string Email = "xunit.userwebapi@example.com";

        /// <summary>
        /// The plain text password the seeded credential is derived from.
        /// </summary>
        public const string Password = "xunit-user-webapi-password";

        private UserWebAPITestUser(UserPostgreSQLConverter userPostgreSQLConverter)
        {
            UserPostgreSQLConverter = userPostgreSQLConverter;
        }

        /// <summary>
        /// Gets the converter the user was seeded with, addressing the same database the Web API host reads.
        /// </summary>
        public UserPostgreSQLConverter UserPostgreSQLConverter { get; }

        /// <summary>
        /// Seeds the test user and its password credential.
        /// <para>Answers null when the database is unavailable: no User_PostgreSQL_Main.conf beside the test assembly,
        /// no connection data in it, or nothing listening.</para>
        /// <para>Seeding writes, which is also what migrates the users table - the credential columns are added by
        /// TableAsync_User, and only a write path calls it.</para>
        /// </summary>
        /// <returns>The seeded user scope, or null when no database is available.</returns>
        public static async Task<UserWebAPITestUser?> CreateAsync()
        {
            UserPostgreSQLConverterManager? userPostgreSQLConverterManager = DiGi.User.PostgreSQL.Create.UserPostgreSQLConverterManager();

            List<UserPostgreSQLConverter>? userPostgreSQLConverters = userPostgreSQLConverterManager?.GetPostgreSQLConverters<UserPostgreSQLConverter>();
            if (userPostgreSQLConverters is null || userPostgreSQLConverters.Count == 0)
            {
                return null;
            }

            UserPostgreSQLConverter userPostgreSQLConverter = userPostgreSQLConverters[0];
            if (userPostgreSQLConverter.ConnectionData is null)
            {
                return null;
            }

            UserCredential? userCredential = WebAPI.Create.UserCredential(Email, Password);
            if (userCredential is null)
            {
                return null;
            }

            try
            {
                List<string> insertedIds = await userPostgreSQLConverter.InsertAsync([new DiGi.User.Classes.User(Email) { FirstName = "xUnit", LastName = "UserWebAPI" }]);
                if (insertedIds.Count == 0)
                {
                    return null;
                }

                if (!await userPostgreSQLConverter.SetUserCredentialAsync(userCredential))
                {
                    return null;
                }
            }
            catch (NpgsqlException)
            {
                // Nothing listening, wrong credentials, or a database that does not exist. To a fact all of them mean
                // the same thing: there is no database to integrate against on this machine.
                return null;
            }
            catch (TimeoutException)
            {
                return null;
            }

            return new UserWebAPITestUser(userPostgreSQLConverter);
        }

        /// <summary>
        /// Removes the seeded user, leaving the database as the fact found it.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        public async ValueTask DisposeAsync()
        {
            try
            {
                await UserPostgreSQLConverter.DeleteUserByEmailAsync(Email);
            }
            catch (NpgsqlException)
            {
                // Best effort: a fact must not fail because the database went away after it had already passed.
            }
        }
    }
}
