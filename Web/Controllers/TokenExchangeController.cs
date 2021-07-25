using System.Threading.Tasks;
using Coomes.Equipper.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TokenExchangeController : ControllerBase
    {
        private readonly ILogger<TokenExchangeController> _logger;
        private readonly ExchangeAuthCodeForToken _exchangeOperation;

        public TokenExchangeController(ExchangeAuthCodeForToken exchangeOperation, ILogger<TokenExchangeController> logger)
        {
            _logger = logger;
            _exchangeOperation = exchangeOperation;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string code, string scope, string error)
        {


            _logger.LogInformation($"Recieved code {code}, scope {scope}, and error {error}.", code, scope, error);


            if(!string.IsNullOrWhiteSpace(error))
            {
                _logger.LogError("Recieved error {error}.", error);
                return new BadRequestObjectResult($"Recieved error {error}.");
            }

            
            var token = await _exchangeOperation.Execute(code);
            return Ok(token);
        }
    }
}
