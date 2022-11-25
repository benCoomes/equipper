using System;
using System.Threading.Tasks;
using Coomes.Equipper.Contracts;

namespace Coomes.Equipper.Operations
{
    public static class TokenHelper
    {
        internal static async Task<AthleteTokens> GetTokensByAthleteID(ITokenStorage tokenStorage, ITokenProvider tokenProvider, long athleteID)
        {
            var athleteTokens = await tokenStorage.GetTokens(athleteID);

            if (athleteTokens == null)
            {
                throw new TokenException($"No athlete with ID {athleteID} is registered.");
            }

            return await _ensureFresh(athleteTokens, tokenStorage, tokenProvider);
        }

        internal static async Task<AthleteTokens> GetTokensByUser(ITokenStorage tokenStorage, ITokenProvider tokenProvider, EquipperUser user)
        {
            var athleteTokens = await tokenStorage.GetTokenForUser(user.UserId);

            if (athleteTokens == null)
            {
                throw new TokenException($"No Strava subscription for user with ID {user.UserId}.");
            }

            return await _ensureFresh(athleteTokens, tokenStorage, tokenProvider);
        }

        private static async Task<AthleteTokens> _ensureFresh(AthleteTokens athleteTokens, ITokenStorage tokenStorage, ITokenProvider tokenProvider) 
        {
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