using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppLensV3.Controllers
{
    [Route("api/hasTestersAccess")]
    [Authorize(Policy = "ApplensAccess")]
    [Authorize(Policy = "ApplensTesters")]
    public class TestersAccessController : Controller
    {
        [HttpGet("")]
        [HttpOptions("")]
        public IActionResult CheckTestersAccess(){
            return new ObjectResult(true);
        }
    }
}
