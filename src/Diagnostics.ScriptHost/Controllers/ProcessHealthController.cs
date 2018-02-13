using Diagnostics.ScriptHost.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Diagnostics.ScriptHost.Controllers
{
    [Produces("application/json")]
    public class ProcessHealthController : Controller
    {
        [HttpGet(UriElements.HealthPing)]
        public IActionResult HealthPing()
        {
            return Ok("Server is up and running.");
        }
    }
}
