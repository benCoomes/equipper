using System;
using System.Collections.Generic;
using domain = Coomes.Equipper;

namespace Coomes.Equipper.StravaApi.Models
{
    // https://developers.strava.com/docs/authentication/#details-about-requesting-access
    public class AuthScopes : StravaModel<domain.AuthScopes>
    {
        private static Dictionary<string, Action<AuthScopes>> _scopeSetters = new Dictionary<string, Action<AuthScopes>>()
        {
            { "read", (s) => s.ReadPublic = true },
            { "read_all", (s) => s.ReadAll = true },
            { "profile:read_all", (s) => s.ProfileReadAll = true },
            { "profile:write", (s) => s.ProfileWrite = true },
            { "activity:read", (s) => s.ActivityRead = true },
            { "activity:read_all", (s) => s.ActivityReadAll = true },
            { "activity:write", (s) => s.ActivityWrite = true }
        };

        public static AuthScopes Create(string scopesString)
        {
            var authScopes = new AuthScopes();
            SetScopes(authScopes, scopesString);
            return authScopes;
        }

        private static void SetScopes(AuthScopes scopes, string scopesString)
        {
            if(string.IsNullOrWhiteSpace(scopesString))
                return;

            var scopeStringList = scopesString.Split(','); 
            foreach(var scope in scopeStringList)
            {
                if(_scopeSetters.TryGetValue(scope, out var setScope))
                {
                    setScope(scopes);
                }
            }
        }

        public bool ReadPublic { get; private set; }
        public bool ReadAll { get; private set; }
        public bool ProfileReadAll { get; private set; }
        public bool ProfileWrite { get; private set; }
        public bool ActivityRead { get; private set; }
        public bool ActivityReadAll { get; private set; }
        public bool ActivityWrite { get; private set; }

        private AuthScopes()
        {
            
        }

        public domain.AuthScopes ToDomainModel()
        {
            return new domain.AuthScopes()
            {
                ReadPublic = ReadPublic,
                ReadAll = ReadAll,
                ProfileReadAll = ProfileReadAll,
                ProfileWrite = ProfileWrite,
                ActivityRead = ActivityRead,
                ActivityReadAll = ActivityReadAll,
                ActivityWrite = ActivityWrite
            };
        }
    }
}