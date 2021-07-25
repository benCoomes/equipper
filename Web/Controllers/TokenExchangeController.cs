using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TokenExchangeController : ControllerBase
    {
        private readonly ILogger<TokenExchangeController> _logger;

        public TokenExchangeController(ILogger<TokenExchangeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get(string code, string scope, string error)
        {
            // Todo: redeem access code for token
//             curl -X POST https://www.strava.com/api/v3/oauth/token \
//   -d client_id=ReplaceWithClientID \
//   -d client_secret=ReplaceWithClientSecret \
//   -d code=ReplaceWithCode \
//   -d grant_type=authorization_code


            if(!string.IsNullOrWhiteSpace(error))
                return $"Recieved error {error}.";
            else
                return $"Recieved authorization code {code} with scope {scope}.";
        }
    }
}
