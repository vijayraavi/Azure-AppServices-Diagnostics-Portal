using Diagnostics.ScriptHost.Models;
using Diagnostics.ScriptHost.SourceWatcher.Interfaces;
using Diagnostics.ScriptHost.Utilities;
using Diagnostics.Scripts;
using Diagnostics.Scripts.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Diagnostics.ScriptHost.SourceWatcher
{
    public class LocalFileSystemSourceWatcherService : ISourceWatcher
    {
        private ICacheService<string, Tuple<Definition, EntityInvoker>> _cacheService;
        private SourceWatcherConfiguration _sourceConfig;
        private Task _completionTask;

        public LocalFileSystemSourceWatcherService(IHostingEnvironment env, IConfiguration configuration, ICacheService<string, Tuple<Definition, EntityInvoker>> cacheService)
        {
            _cacheService = cacheService;

            _sourceConfig = new SourceWatcherConfiguration();
            if (env.IsProduction())
            {
                _sourceConfig.LocalSourceDirectory = (string)Registry.GetValue(HostConstants.ScriptSourceConfigRegistryRootPath, HostConstants.LocalSourceDirectoryKey, string.Empty);
            }
            else
            {
                _sourceConfig.LocalSourceDirectory = (configuration[$"ScriptSourceConfig:{HostConstants.LocalSourceDirectoryKey}"]).ToString();
            }

            Start();
        }
        
        private async Task ListFilesAndCompile() {

            foreach (string filename in Directory.EnumerateFiles(_sourceConfig.LocalSourceDirectory))
            {
                try
                {
                    var fileContent = await File.ReadAllTextAsync(filename);
                    EntityMetadata metaData = new EntityMetadata()
                    {
                        Type = EntityType.Signal,
                        ScriptText = fileContent
                    };

                    EntityInvoker invoker = new EntityInvoker(metaData, ScriptHelper.GetFrameworkReferences(), ScriptHelper.GetFrameworkImports());
                    await invoker.InitializeEntryPointAsync();

                    Definition definition = null;
                    if (invoker.IsCompilationSuccessful)
                    {
                        definition = AttributeHelper.CreateDefinitionAttribute(invoker.Attributes.FirstOrDefault());
                        _cacheService.AddOrUpdate(definition.Id.ToLower(), new Tuple<Definition, EntityInvoker>(definition, invoker));
                    }
                }
                catch (Exception ex)
                {
                    if(!(ex is ScriptCompilationException))
                    {
                        throw ex;
                    }
                }
            }
        }

        private void Start()
        {
            _completionTask = ListFilesAndCompile();
        }

        public Task WaitForCompletion()
        {
            return _completionTask;
        }
    }
}
