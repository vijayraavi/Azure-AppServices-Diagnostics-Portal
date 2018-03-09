using Diagnostics.RuntimeHost.Utilities;
using Diagnostics.Scripts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diagnostics.RuntimeHost.Services.SourceWatcher
{
    public class SourceWatcherService : ISourceWatcherService
    {
        private ISourceWatcher _watcher;

        public ISourceWatcher Watcher => _watcher;

        public SourceWatcherService(IHostingEnvironment env, IConfiguration configuration, ICache<string, EntityInvoker> invokerCacheService, IGithubClient githubClient)
        {
            SourceWatcherType watcherType;

            if (env.IsProduction())
            {
                watcherType = (SourceWatcherType)Registry.GetValue(RegistryConstants.SourceWatcherRegistryPath, RegistryConstants.WatcherTypeKey, 0);
            }
            else
            {
                watcherType = Enum.Parse<SourceWatcherType>(configuration[$"SourceWatcher:{RegistryConstants.WatcherTypeKey}"]);
            }

            switch (watcherType)
            {
                case SourceWatcherType.LocalFileSystem:
                    _watcher = new LocalFileSystemWatcher(env, configuration, invokerCacheService);
                    break;
                case SourceWatcherType.Github:
                    _watcher = new GitHubWatcher(env, configuration, invokerCacheService, githubClient);
                    break;
                default:
                    throw new NotSupportedException("Source Watcher Type not supported");
            }
        }
    }
}
