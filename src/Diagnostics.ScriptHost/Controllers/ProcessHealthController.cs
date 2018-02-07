using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Diagnostics.DataProviders;
using Diagnostics.ScriptHost.Models;
using Diagnostics.ScriptHost.Utilities;
using Diagnostics.Scripts;
using Diagnostics.Scripts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
