using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.FunctionApp
{
    public static class ErrorHandler
    {
        public static async Task<ActionResult> RunWithErrorHandling(ILogger logger, Func<Task<ActionResult>> method)
        {
            try 
            {
                return await method();
            }
            catch (BadRequestException bre) 
            {
                return new BadRequestObjectResult(bre.Message);
            }
            catch(UnauthorizedException)
            {
                return new UnauthorizedResult();
            }
        }
    }

}