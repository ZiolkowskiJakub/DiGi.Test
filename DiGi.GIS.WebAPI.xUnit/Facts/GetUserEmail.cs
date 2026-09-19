using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using DiGi.WebAPI.Classes;
using Microsoft.IdentityModel.Tokens;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// A token minted the way the user extension mints one - the shape the GIS in-action validator must accept:
        /// an <c>email</c> and a <c>jti</c> claim, signed HMAC-SHA256 with a key the manager holds.
        /// </summary>
        private static string CreateToken(SecurityKeyManager securityKeyManager, string email, DateTime? expiresOverride = null)
        {
            DiGi.WebAPI.Classes.SecurityKey? securityKey = securityKeyManager.GetActive();
            if (securityKey is null)
            {
                throw new InvalidOperationException("No active security key to sign with.");
            }

            DateTime expires = expiresOverride ?? DateTime.UtcNow.AddMinutes(30);
            DateTime notBefore = (expires < DateTime.UtcNow ? expires : DateTime.UtcNow).AddSeconds(-5);   // strictly before expiry, and not in the future for a live token

            JwtSecurityTokenHandler tokenHandler = new();
            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity([new Claim(ClaimTypes.Email, email), new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))]),
                NotBefore = notBefore,
                Expires = expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(securityKey.GetBytes()), SecurityAlgorithms.HmacSha256Signature)
            };

            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        private static string JtiOf(string token)
        {
            JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwt.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Jti).Value;
        }

        [Fact]
        public void GetUserEmail_ValidToken_ReturnsEmail()
        {
            SecurityKeyManager securityKeyManager = new();
            _ = securityKeyManager.Generate();
            TokenRevocationStore tokenRevocationStore = new();

            string token = CreateToken(securityKeyManager, "reviewer@example.com");

            string? email = Query.GetUserEmail(securityKeyManager, tokenRevocationStore, "Bearer " + token);

            Assert.Equal("reviewer@example.com", email);
        }

        [Fact]
        public void GetUserEmail_ValidTokenEmptyEmail_ReturnsNull()
        {
            SecurityKeyManager securityKeyManager = new();
            _ = securityKeyManager.Generate();
            TokenRevocationStore tokenRevocationStore = new();

            string token = CreateToken(securityKeyManager, "");

            string? email = Query.GetUserEmail(securityKeyManager, tokenRevocationStore, "Bearer " + token);

            Assert.Null(email);
        }

        [Fact]
        public void GetUserEmail_BearerPrefixCaseInsensitive()
        {
            SecurityKeyManager securityKeyManager = new();
            _ = securityKeyManager.Generate();
            TokenRevocationStore tokenRevocationStore = new();

            string token = CreateToken(securityKeyManager, "reviewer@example.com");

            string? email = Query.GetUserEmail(securityKeyManager, tokenRevocationStore, "bearer  " + token);

            Assert.Equal("reviewer@example.com", email);
        }

        [Fact]
        public void GetUserEmail_NoManager_ReturnsNull()
        {
            TokenRevocationStore tokenRevocationStore = new();

            Assert.Null(Query.GetUserEmail(null, tokenRevocationStore, "Bearer whatever"));
        }

        [Fact]
        public void GetUserEmail_NoStore_ReturnsNull()
        {
            SecurityKeyManager securityKeyManager = new();
            _ = securityKeyManager.Generate();

            Assert.Null(Query.GetUserEmail(securityKeyManager, null, "Bearer whatever"));
        }

        [Fact]
        public void GetUserEmail_MalformedHeader_ReturnsNull()
        {
            SecurityKeyManager securityKeyManager = new();
            _ = securityKeyManager.Generate();
            TokenRevocationStore tokenRevocationStore = new();

            Assert.Null(Query.GetUserEmail(securityKeyManager, tokenRevocationStore, "NotBearer xyz"));
            Assert.Null(Query.GetUserEmail(securityKeyManager, tokenRevocationStore, "Bearer"));
            Assert.Null(Query.GetUserEmail(securityKeyManager, tokenRevocationStore, null));
            Assert.Null(Query.GetUserEmail(securityKeyManager, tokenRevocationStore, ""));
        }

        [Fact]
        public void GetUserEmail_WrongSignature_ReturnsNull()
        {
            SecurityKeyManager signingManager = new();
            _ = signingManager.Generate();

            SecurityKeyManager validatingManager = new();
            _ = validatingManager.Generate();

            TokenRevocationStore tokenRevocationStore = new();

            string token = CreateToken(signingManager, "reviewer@example.com");

            Assert.Null(Query.GetUserEmail(validatingManager, tokenRevocationStore, "Bearer " + token));
        }

        [Fact]
        public void GetUserEmail_RevokedToken_ReturnsNull()
        {
            SecurityKeyManager securityKeyManager = new();
            _ = securityKeyManager.Generate();
            TokenRevocationStore tokenRevocationStore = new();

            string token = CreateToken(securityKeyManager, "reviewer@example.com");

            Assert.Equal("reviewer@example.com", Query.GetUserEmail(securityKeyManager, tokenRevocationStore, "Bearer " + token));

            tokenRevocationStore.Revoke(JtiOf(token), DateTimeOffset.UtcNow.AddHours(1));

            Assert.Null(Query.GetUserEmail(securityKeyManager, tokenRevocationStore, "Bearer " + token));
        }

        [Fact]
        public void GetUserEmail_ExpiredToken_ReturnsNull()
        {
            SecurityKeyManager securityKeyManager = new();
            _ = securityKeyManager.Generate();
            TokenRevocationStore tokenRevocationStore = new();

            // Well past the default 5-minute validation clock skew, so the refusal is not a boundary artefact.
            string token = CreateToken(securityKeyManager, "reviewer@example.com", DateTime.UtcNow.AddMinutes(-10));

            Assert.Null(Query.GetUserEmail(securityKeyManager, tokenRevocationStore, "Bearer " + token));
        }
    }
}