using Diagnostics.ScriptHost.DataProviderServices;
using Diagnostics.ScriptHost.Models;
using Diagnostics.ScriptHost.Utilities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Diagnostics.ScriptHost.Controllers
{
    public abstract class ControllerBase : Controller
    {
        public ControllerBase()
        {
        }
    }

    public abstract class SiteControllerBase : ControllerBase
    {
        private ITenantIdService _tenantIdService;

        public SiteControllerBase(ITenantIdService tenantIdService)
        {
            _tenantIdService = tenantIdService;
        }

        protected bool VerifyQueryParams(string[] hostNames, string stampName, out string reason)
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
        
        protected async Task<SiteResource> PrepareResourceObject(string subscriptionId, string resourceGroup, string siteName, IEnumerable<string> hostNames, string stampName, DateTime startTime, DateTime endTime, string sourceMoniker = null)
        {
            List<string> tenantIdList = await _tenantIdService.GetTenantIdForStamp(stampName, startTime, endTime);
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
