using System;
using System.Threading.Tasks;
using Coomes.Equipper.Contracts;

namespace Coomes.Equipper.Operations
{
    public static class TokenHelper
    {
        internal static async Task<AthleteTokens> GetTokensAndRefreshIfNeeded(ITokenStorage tokenStorage, ITokenProvider tokenProvider, long athleteID)
        {
            var athleteTokens = await tokenStorage.GetTokens(athleteID);

            if (athleteTokens == null)
            {
                throw new SetGearException($"No athlete with ID {athleteID} is registered.");
            }

            var refreshAt = athleteTokens.ExpiresAtUtc.Subtract(TimeSpan.FromMinutes(5));
            var now = DateTime.UtcNow;
            if (refreshAt < now)
            {
                var newTokens = await tokenProvider.RefreshToken(athleteTokens);
                newTokens.UserID = athleteTokens.UserID;
                await tokenStorage.AddOrUpdateTokens(newTokens); // todo: concurrent updates?? take latest expire time?
                athleteTokens = newTokens;
            }

            return athleteTokens;
        }
    }
}