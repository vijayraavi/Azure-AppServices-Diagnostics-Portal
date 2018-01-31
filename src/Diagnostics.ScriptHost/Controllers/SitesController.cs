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
    [Route(UriElements.SitesResource + UriElements.Diagnostics)]
    public class SitesController : Controller
    {
        [HttpPost(UriElements.Query)]
        public async Task<IActionResult> Post(string subscriptionId, string resourceGroupName, string siteName, [FromBody]JToken jsonBody)
        {
            if (jsonBody == null)
            {
                return BadRequest("Missing body");
            }

            string script = jsonBody.Value<string>("script");

            if (string.IsNullOrWhiteSpace(script))
            {
                return BadRequest("Missing script from body");
            }

            EntityMetadata metaData = new EntityMetadata()
            {
                Name = "On Demand Query",
                scriptText = script,
                Type = EntityType.Signal
            };

            // TODO : We want to get DataProvider or config based on Environment (dev or prod)
            var configFactory = new AppSettingsDataProviderConfigurationFactory();
            var config = configFactory.LoadConfigurations();
            var dataProviders = new DataProviders.DataProviders(config);

            using (var invoker = new EntityInvoker(metaData, ScriptHelper.GetFrameworkReferences(), ScriptHelper.GetFrameworkImports()))
            {
                QueryResponse<DataTableResponseObject> response = new QueryResponse<DataTableResponseObject>();

                try
                {
                    await invoker.InitializeEntryPointAsync();
                    DataTableResponseObject scriptResponse = (DataTableResponseObject)await invoker.Invoke(new object[] { dataProviders });
                    
                    response.CompilationSucceeded = invoker.IsCompilationSuccessful;
                    response.CompilationOutput = invoker.CompilationOutput;
                    response.InvocationOutput = scriptResponse;
                    return Ok(response);
                }
                catch(Exception ex)
                {
                    if(ex is ScriptCompilationException)
                    {
                        response.CompilationSucceeded = invoker.IsCompilationSuccessful;
                        response.CompilationOutput = invoker.CompilationOutput;
                        return Ok(response);
                    }

                    throw ex;
                }
            }
        }
    }
}
