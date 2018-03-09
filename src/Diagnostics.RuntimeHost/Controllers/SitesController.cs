using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Diagnostics.DataProviders;
using Diagnostics.ModelsAndUtils;
using Diagnostics.RuntimeHost.Services;
using Diagnostics.RuntimeHost.Services.SourceWatcher;
using Diagnostics.RuntimeHost.Utilities;
using Diagnostics.Scripts;
using Diagnostics.Scripts.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Diagnostics.RuntimeHost.Controllers
{
    [Produces("application/json")]
    [Route(UriElements.SitesResource + UriElements.Diagnostics)]
    public class SitesController : Controller
    {
        private ICompilerHostClient _compilerHostClient;
        private ISourceWatcherService _sourceWatcherService;
        private ICache<string, EntityInvoker> _invokerCache;

        public SitesController(ICompilerHostClient compilerHostClient, ISourceWatcherService sourceWatcherService, ICache<string, EntityInvoker> invokerCache)
        {
            _compilerHostClient = compilerHostClient;
            _sourceWatcherService = sourceWatcherService;
            _invokerCache = invokerCache;
        }

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

            if (!VerifyQueryParams(hostNames, stampName, out string verficationOutput))
            {
                return BadRequest(verficationOutput);
            }

            if (!DateTimeHelper.PrepareStartEndTimeWithTimeGrain(startTime, endTime, timeGrain, out DateTime startTimeUtc, out DateTime endTimeUtc, out TimeSpan timeGrainTimeSpan, out string errorMessage))
            {
                return BadRequest(errorMessage);
            }

            IConfigurationFactory factory = new AppSettingsDataProviderConfigurationFactory();
            var config = factory.LoadConfigurations();

            EntityMetadata metaData = new EntityMetadata(script);
            var dataProviders = new DataProviders.DataProviders(config);
            SiteResource resource = PrepareResourceObject(subscriptionId, resourceGroupName, siteName, hostNames, stampName, startTimeUtc, endTimeUtc);
            OperationContext cxt = PrepareContext(resource, startTimeUtc, endTimeUtc);

            QueryResponse<Response> queryRes = new QueryResponse<Response>
            {
                InvocationOutput = new Response()
            };

            Assembly tempAsm = null;
            var compilerResponse = await _compilerHostClient.GetCompilationResponse(script);

            queryRes.CompilationOutput = compilerResponse;

            if (queryRes.CompilationOutput.CompilationSucceeded)
            {
                byte[] asmData = Convert.FromBase64String(compilerResponse.AssemblyBytes);
                byte[] pdbData = Convert.FromBase64String(compilerResponse.PdbBytes);

                tempAsm = Assembly.Load(asmData, pdbData);

                using (var invoker = new EntityInvoker(metaData, ScriptHelper.GetFrameworkReferences(), ScriptHelper.GetFrameworkImports()))
                {
                    invoker.InitializeEntryPoint(tempAsm);
                    queryRes.InvocationOutput.Metadata = invoker.EntryPointDefinitionAttribute;
                    queryRes.InvocationOutput = (Response)await invoker.Invoke(new object[] { dataProviders, cxt, queryRes.InvocationOutput });
                }
            }

            return Ok(queryRes);
        }

        [HttpGet(UriElements.Detectors)]
        public async Task<IActionResult> ListDetectors(string subscriptionId, string resourceGroupName, string siteName)
        {
            await _sourceWatcherService.Watcher.WaitForFirstCompletion();
            IEnumerable<Definition> entityDefinitions = _invokerCache.GetAll().Select(p => p.EntryPointDefinitionAttribute);
            return Ok(entityDefinitions);
        }

        [HttpGet(UriElements.Detectors + UriElements.DetectorResource)]
        public async Task<IActionResult> GetDetectorResource(string subscriptionId, string resourceGroupName, string siteName, string detectorId, string[] hostNames, string stampName, string startTime = null, string endTime = null, string timeGrain = null)
        {
            if (!VerifyQueryParams(hostNames, stampName, out string verficationOutput))
            {
                return BadRequest(verficationOutput);
            }

            if (!DateTimeHelper.PrepareStartEndTimeWithTimeGrain(startTime, endTime, timeGrain, out DateTime startTimeUtc, out DateTime endTimeUtc, out TimeSpan timeGrainTimeSpan, out string errorMessage))
            {
                return BadRequest(errorMessage);
            }

            IConfigurationFactory factory = new AppSettingsDataProviderConfigurationFactory();
            var config = factory.LoadConfigurations();
            
            var dataProviders = new DataProviders.DataProviders(config);
            SiteResource resource = PrepareResourceObject(subscriptionId, resourceGroupName, siteName, hostNames, stampName, startTimeUtc, endTimeUtc);
            OperationContext cxt = PrepareContext(resource, startTimeUtc, endTimeUtc);

            EntityInvoker invoker;
            if (!_invokerCache.TryGetValue(detectorId, out invoker))
            {
                return NotFound();
            }

            Response res = new Response
            {
                Metadata = invoker.EntryPointDefinitionAttribute
            };

            res = (Response)await invoker.Invoke(new object[] { dataProviders, cxt, res });

            return Ok(res);
        }

        private bool VerifyQueryParams(string[] hostNames, string stampName, out string reason)
        {
            reason = string.Empty;
            if (hostNames == null || hostNames.Length <= 0)
            {
                reason = "Invalid or empty hostnames";
                return false;
            }

            if (string.IsNullOrWhiteSpace(stampName))
            {
                reason = "Invalid or empty stampName";
                return false;
            }

            return true;
        }

        protected SiteResource PrepareResourceObject(string subscriptionId, string resourceGroup, string siteName, IEnumerable<string> hostNames, string stampName, DateTime startTime, DateTime endTime, string sourceMoniker = null)
        {
            List<string> tenantIdList = new List<string>(); //await _tenantIdService.GetTenantIdForStamp(stampName, startTime, endTime);
            SiteResource resource = new SiteResource()
            {
                SubscriptionId = subscriptionId,
                ResourceGroup = resourceGroup,
                SiteName = siteName,
                HostNames = hostNames,
                Stamp = stampName,
                SourceMoniker = sourceMoniker ?? stampName.ToUpper().Replace("-", string.Empty),
                TenantIdList = tenantIdList
            };

            return resource;
        }

        protected OperationContext PrepareContext(SiteResource resource, DateTime startTime, DateTime endTime)
        {
            return new OperationContext(
                resource,
                DateTimeHelper.GetDateTimeInUtcFormat(startTime).ToString(HostConstants.KustoTimeFormat),
                DateTimeHelper.GetDateTimeInUtcFormat(endTime).ToString(HostConstants.KustoTimeFormat)
            );
        }
    }
}