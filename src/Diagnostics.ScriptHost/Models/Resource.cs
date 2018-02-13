using System.Collections.Generic;

namespace Diagnostics.ScriptHost.Models
{
    public interface IResource
    {
    }

    public abstract class Resource : IResource
    {
        public string SubscriptionId;

        public string ResourceGroup;

        public IEnumerable<string> TenantIdList;
    }

    public sealed class SiteResource : Resource
    {
        public string SiteName;

        public IEnumerable<string> HostNames;

        public string Stamp;

        public string SourceMoniker;
    }
}
