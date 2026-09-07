using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.User.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the revocation semantics of the token revocation store: idempotent revoke, unknown and empty identifiers, expiry handling, sweep-bounded memory and concurrent access.
        /// </summary>
        [Fact]
        public void TokenRevocationStore()
        {
            DiGi.WebAPI.Classes.TokenRevocationStore tokenRevocationStore = new();

            // A token without an identifier, or an unknown identifier, is never reported revoked.
            Assert.False(tokenRevocationStore.IsRevoked(null));
            Assert.False(tokenRevocationStore.IsRevoked(string.Empty));
            Assert.False(tokenRevocationStore.IsRevoked("   "));
            Assert.False(tokenRevocationStore.IsRevoked(Guid.NewGuid().ToString("N")));

            // Revocation takes effect, survives a repeated revoke and stores exactly one entry.
            string jti = Guid.NewGuid().ToString("N");
            DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddMinutes(5);
            tokenRevocationStore.Revoke(jti, expiresAt);
            Assert.True(tokenRevocationStore.IsRevoked(jti));
            tokenRevocationStore.Revoke(jti, expiresAt);
            Assert.True(tokenRevocationStore.IsRevoked(jti));
            Assert.Equal(1, tokenRevocationStore.Count);

            // A null or empty identifier is ignored by revoke.
            tokenRevocationStore.Revoke(null, expiresAt);
            tokenRevocationStore.Revoke(string.Empty, expiresAt);
            Assert.Equal(1, tokenRevocationStore.Count);

            // An entry past its expiration is not reported revoked: the token it revokes is invalid regardless.
            string jti_Expired = Guid.NewGuid().ToString("N");
            tokenRevocationStore.Revoke(jti_Expired, DateTimeOffset.UtcNow.AddSeconds(-1));
            Assert.False(tokenRevocationStore.IsRevoked(jti_Expired));

            // Expired revocations can never accumulate past the threshold: the sweep inside revoke drops them
            // on the way past it, so memory stays bounded by the token lifetime while a live entry survives every sweep.
            for (int i = 0; i < DiGi.WebAPI.Classes.TokenRevocationStore.SweepThreshold + 10; i++)
            {
                tokenRevocationStore.Revoke(Guid.NewGuid().ToString("N"), DateTimeOffset.UtcNow.AddSeconds(-1));
            }

            Assert.True(tokenRevocationStore.Count <= DiGi.WebAPI.Classes.TokenRevocationStore.SweepThreshold, $"Sweep failed to bound memory! Count: {tokenRevocationStore.Count}.");
            Assert.True(tokenRevocationStore.IsRevoked(jti));

            // Concurrent revocation and checking of distinct identifiers is safe.
            List<string> jtis = [.. Enumerable.Range(0, 100).Select(index => Guid.NewGuid().ToString("N"))];
            Parallel.ForEach(jtis, jti_Concurrent => tokenRevocationStore.Revoke(jti_Concurrent, DateTimeOffset.UtcNow.AddMinutes(5)));
            Parallel.ForEach(jtis, jti_Concurrent => Assert.True(tokenRevocationStore.IsRevoked(jti_Concurrent)));
        }
    }
}
