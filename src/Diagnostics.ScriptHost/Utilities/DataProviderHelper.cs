using Diagnostics.DataProviders;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnostics.ScriptHost.Utilities
{
    internal class DataProviderHelper
    {
        public static IConfigurationFactory GetDataProviderConfigurationFactory(IHostingEnvironment env)
        {
            if (env.IsProduction())
            {
                return new RegistryDataProviderConfigurationFactory(HostConstants.RegistryRootPath);
            }


            switch (env.EnvironmentName.ToLower())
            {
                case "mock":
                    return new MockDataProviderConfigurationFactory();
                default:
                    return new AppSettingsDataProviderConfigurationFactory();
            }
        }

        public static IEnumerable<string> FilterHostnamesCoveredByWildcard(IEnumerable<string> hostNames)
        {
            if (hostNames == null || !hostNames.Any())
            {
                return hostNames;
            }

            //Find if hostnames have a wild card hostname
            var wildcardHostname = hostNames.Where(p => p.StartsWith("*")).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(wildcardHostname))
            {
                // No wildcard hostnames found.
                return hostNames;
            }

            //remove * from the wildcard hostname
            wildcardHostname = wildcardHostname.Replace("*", string.Empty);
            var filteredHostnames = new List<string>();

            foreach (var hostname in hostNames)
            {
                if (hostname.StartsWith("*") || !hostname.EndsWith(wildcardHostname, StringComparison.OrdinalIgnoreCase))
                {
                    // Add this hostname as it is not covered by Wildcard hostname
                    filteredHostnames.Add(hostname);
                }
            }

            return filteredHostnames.ToArray();
        }
    }
}
