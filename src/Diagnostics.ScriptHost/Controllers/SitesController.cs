using Diagnostics.ScriptHost.DataProviderServices;
using Diagnostics.ScriptHost.Models;
using Diagnostics.ScriptHost.SourceWatcher.Interfaces;
using Diagnostics.ScriptHost.Utilities;
using Diagnostics.Scripts;
using Diagnostics.Scripts.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diagnostics.ScriptHost.Controllers
{
    [Produces("application/json")]
    [Route(UriElements.SitesResource + UriElements.Diagnostics)]
    public class SitesController : SiteControllerBase
    {
        private IDataSourcesConfigurationService _dataSourcesConfigService;
        private ICacheService<string, Tuple<Definition, EntityInvoker>> _compilationCacheService;
        private ISourceWatcher _sourceWatcherService;
        private ITenantIdService _tenantIdService;

        public SitesController(IDataSourcesConfigurationService dataSourcesConfigService, ICacheService<string, Tuple<Definition, EntityInvoker>> compilationCache, ISourceWatcher sourceWatcher, ITenantIdService tenantIdService)
            :base(tenantIdService)
        {
            _compilationCacheService = compilationCache;
            _dataSourcesConfigService = dataSourcesConfigService;
            _sourceWatcherService = sourceWatcher;
            _tenantIdService = tenantIdService;
        }

        [HttpGet(UriElements.Detectors)]
        public async Task<IActionResult> ListDetectors(string subscriptionId, string resourceGroupName, string siteName)
        {
            await _sourceWatcherService.WaitForCompletion();
            IEnumerable<Definition> entityDefinitions = _compilationCacheService.GetAll().Select(p => p.Item1);
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

            await _sourceWatcherService.WaitForCompletion();

            if (!_compilationCacheService.TryGetValue(detectorId.ToLower(), out Tuple<Definition, EntityInvoker> entity))
            {
                return NotFound($"No entity found with detector Id : {detectorId.ToLower()}");
            }

            EntityInvoker invoker = entity.Item2;
            var dataProviders = new DataProviders.DataProviders(_dataSourcesConfigService.Config);
            SiteResource resource = await PrepareResourceObject(subscriptionId, resourceGroupName, siteName, hostNames, stampName, startTimeUtc, endTimeUtc);
            OperationContext cxt = PrepareContext(resource, startTimeUtc, endTimeUtc);

            SignalResponse res = new SignalResponse();
            res = (SignalResponse)await invoker.Invoke(new object[] { dataProviders, cxt, res });
            res.Metadata = AttributeHelper.CreateDefinitionAttribute(invoker.Attributes.FirstOrDefault());
            return Ok(res);
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

            EntityMetadata metaData = new EntityMetadata(script);
            var dataProviders = new DataProviders.DataProviders(_dataSourcesConfigService.Config);
            SiteResource resource = await PrepareResourceObject(subscriptionId, resourceGroupName, siteName, hostNames, stampName, startTimeUtc, endTimeUtc);
            OperationContext cxt = PrepareContext(resource, startTimeUtc, endTimeUtc);

            using (var invoker = new EntityInvoker(metaData, ScriptHelper.GetFrameworkReferences(), ScriptHelper.GetFrameworkImports()))
            {
                QueryResponse<SignalResponse> response = new QueryResponse<SignalResponse>
                {
                    InvocationOutput = new SignalResponse()
                };

                try
                {
                    await invoker.InitializeEntryPointAsync();
                    response.CompilationSucceeded = invoker.IsCompilationSuccessful;
                    response.CompilationOutput = invoker.CompilationOutput;
                    response.InvocationOutput = (SignalResponse)await invoker.Invoke(new object[] { dataProviders, cxt, response.InvocationOutput });
                    response.InvocationOutput.Metadata = AttributeHelper.CreateDefinitionAttribute(invoker.Attributes.FirstOrDefault());
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
