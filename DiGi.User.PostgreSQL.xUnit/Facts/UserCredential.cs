using DiGi.User.Classes;
using System.Linq;

namespace DiGi.User.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a UserCredential keeps the values it was constructed with and survives a serialization round trip.
        /// </summary>
        [Fact]
        public void UserCredential()
        {
            UserCredential userCredential = new("user@example.com", "aGFzaA==", "c2FsdA==", 210000);

            Assert.Equal("user@example.com", userCredential.Email);
            Assert.Equal("aGFzaA==", userCredential.PasswordHash);
            Assert.Equal("c2FsdA==", userCredential.PasswordSalt);
            Assert.Equal(210000, userCredential.PasswordIterations);

            string? json = Core.Convert.ToSystem_String(userCredential);
            Assert.False(string.IsNullOrWhiteSpace(json));

            UserCredential? userCredential_Deserialized = Core.Convert.ToDiGi<UserCredential>(json)?.FirstOrDefault();
            Assert.NotNull(userCredential_Deserialized);
            Assert.Equal(userCredential.Email, userCredential_Deserialized.Email);
            Assert.Equal(userCredential.PasswordHash, userCredential_Deserialized.PasswordHash);
            Assert.Equal(userCredential.PasswordSalt, userCredential_Deserialized.PasswordSalt);
            Assert.Equal(userCredential.PasswordIterations, userCredential_Deserialized.PasswordIterations);

            Core.xUnit.Query.SerializationCheck(userCredential);
        }

        /// <summary>
        /// Tests that Create.UserCredential derives a verifiable credential, salts every credential independently, and
        /// refuses to produce one from a blank email or password.
        /// </summary>
        [Fact]
        public void Create_UserCredential()
        {
            UserCredential? userCredential_First = Create.UserCredential("user@example.com", "correct horse battery staple");
            UserCredential? userCredential_Second = Create.UserCredential("user@example.com", "correct horse battery staple");

            Assert.NotNull(userCredential_First);
            Assert.NotNull(userCredential_Second);

            Assert.Equal(Constants.Password.Iterations, userCredential_First.PasswordIterations);

            // A random salt per credential means two credentials for the same password share nothing, so the store
            // never reveals that two accounts picked the same password.
            Assert.NotEqual(userCredential_First.PasswordSalt, userCredential_Second.PasswordSalt);
            Assert.NotEqual(userCredential_First.PasswordHash, userCredential_Second.PasswordHash);

            Assert.True(userCredential_First.IsPasswordValid("correct horse battery staple"));
            Assert.True(userCredential_Second.IsPasswordValid("correct horse battery staple"));

            Assert.Null(Create.UserCredential(null, "password"));
            Assert.Null(Create.UserCredential("   ", "password"));
            Assert.Null(Create.UserCredential("user@example.com", null));
            Assert.Null(Create.UserCredential("user@example.com", "   "));
        }

        /// <summary>
        /// Tests that Query.IsPasswordValid accepts only an exact password match and denies on every other branch.
        /// </summary>
        [Fact]
        public void Query_IsPasswordValid()
        {
            UserCredential? userCredential = Create.UserCredential("user@example.com", "correct horse battery staple");
            Assert.NotNull(userCredential);

            Assert.True(userCredential.IsPasswordValid("correct horse battery staple"));

            // Every branch that is not an exact match must deny. A test that only proves the grant passes while the
            // endpoint is open to everyone.
            Assert.False(userCredential.IsPasswordValid("correct horse battery stapl"));
            Assert.False(userCredential.IsPasswordValid("Correct horse battery staple"));
            Assert.False(userCredential.IsPasswordValid(null));
            Assert.False(userCredential.IsPasswordValid(string.Empty));
            Assert.False(userCredential.IsPasswordValid("   "));

            UserCredential? userCredential_Null = null;
            Assert.False(userCredential_Null.IsPasswordValid("correct horse battery staple"));

            Assert.False(new UserCredential("user@example.com", null, userCredential.PasswordSalt, userCredential.PasswordIterations).IsPasswordValid("correct horse battery staple"));
            Assert.False(new UserCredential("user@example.com", userCredential.PasswordHash, null, userCredential.PasswordIterations).IsPasswordValid("correct horse battery staple"));
            Assert.False(new UserCredential("user@example.com", userCredential.PasswordHash, userCredential.PasswordSalt, 0).IsPasswordValid("correct horse battery staple"));
            Assert.False(new UserCredential("user@example.com", "not base64 at all!", userCredential.PasswordSalt, userCredential.PasswordIterations).IsPasswordValid("correct horse battery staple"));

            // A credential stretched over a different iteration count derives a different key, so the stored count is
            // genuinely the one used rather than the constant.
            Assert.False(new UserCredential("user@example.com", userCredential.PasswordHash, userCredential.PasswordSalt, userCredential.PasswordIterations + 1).IsPasswordValid("correct horse battery staple"));
        }
    }
}
