using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diagnostics.ScriptHost.Models
{
    public class Resource
    {
    }

    public class SiteResource
    {
        public string SubscriptionId;

        public string ResourceGroup;

        public string SiteName;

        public IEnumerable<string> HostNames;

        public string Stamp;

        public string TenantId;
    }
}
