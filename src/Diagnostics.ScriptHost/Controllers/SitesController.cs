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
        public async Task<IActionResult> Post(string subscriptionId, string resourceGroupName, string siteName, string[] hostNames, string stampName, [FromBody]JToken jsonBody, string startTime = null, string endTime = null, string timeGrain = null)
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

            if (hostNames == null || hostNames.Length <= 0)
            {
                return BadRequest("Invalid or empty hostnames");
            }

            if (string.IsNullOrWhiteSpace(stampName))
            {
                return BadRequest("Invalid or empty stampName");
            }

            DateTime startTimeUtc, endTimeUtc;
            TimeSpan timeGrainTimeSpan;
            string errorMessage;

            if (!DateTimeHelper.PrepareStartEndTimeWithTimeGrain(startTime, endTime, timeGrain, out startTimeUtc, out endTimeUtc, out timeGrainTimeSpan, out errorMessage))
            {
                return BadRequest(errorMessage);
            }

            EntityMetadata metaData = new EntityMetadata()
            {
                Name = "On Demand Query",
                scriptText = script,
                Type = EntityType.Signal
            };

            // TODO : We want to get DataProvider or config based on Environment (dev or prod)
            //var configFactory = new AppSettingsDataProviderConfigurationFactory();
            var configFactory = new RegistryDataProviderConfigurationFactory(HostConstants.RegistryRootPath);
            var config = configFactory.LoadConfigurations();
            var dataProviders = new DataProviders.DataProviders(config);
            
            SiteResource resource = new SiteResource()
            {
                SubscriptionId = subscriptionId,
                ResourceGroup = resourceGroupName,
                SiteName = siteName,
                HostNames = hostNames,
                Stamp = stampName
                // TODO : Fill in Tenant
            };

            OperationContext cxt = new OperationContext(
                resource,
                DateTimeHelper.GetDateTimeInUtcFormat(startTimeUtc).ToString(HostConstants.KustoTimeFormat),
                DateTimeHelper.GetDateTimeInUtcFormat(endTimeUtc).ToString(HostConstants.KustoTimeFormat)
            );

            using (var invoker = new EntityInvoker(metaData, ScriptHelper.GetFrameworkReferences(), ScriptHelper.GetFrameworkImports()))
            {
                QueryResponse<SignalResponse> response = new QueryResponse<SignalResponse>();
                response.InvocationOutput = new SignalResponse();

                try
                {
                    await invoker.InitializeEntryPointAsync();
                    response.CompilationSucceeded = invoker.IsCompilationSuccessful;
                    response.CompilationOutput = invoker.CompilationOutput;
                    response.InvocationOutput = (SignalResponse)await invoker.Invoke(new object[] { dataProviders, cxt, response.InvocationOutput });
                    
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
