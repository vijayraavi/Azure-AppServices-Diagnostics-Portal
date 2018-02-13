using Diagnostics.DataProviders;
using Diagnostics.ScriptHost.DataProviderServices;
using Diagnostics.ScriptHost.Models;
using Diagnostics.ScriptHost.SourceWatcher;
using Diagnostics.ScriptHost.SourceWatcher.Interfaces;
using Diagnostics.Scripts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Diagnostics.ScriptHost
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }
        
        // Registers services with container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton<ICacheService<string, Tuple<Definition, EntityInvoker>>, CompilationCache<string, Tuple<Definition, EntityInvoker>>>();
            services.AddSingleton<IDataSourcesConfigurationService, DataSourcesConfigurationService>();
            services.AddSingleton<ISourceWatcher, LocalFileSystemSourceWatcherService>();
            services.AddSingleton<ITenantIdService, TenantIdService>();

            // TODO : Not sure what's the right place for the following code piece.
            #region Custom Start up Code

            var servicesProvider = services.BuildServiceProvider();
            var dataSourcesConfigService = servicesProvider.GetService<IDataSourcesConfigurationService>();
            KustoTokenService.Instance.Initialize(dataSourcesConfigService.Config.KustoConfiguration);
            
            #endregion
        }

        // Adds middleware to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ISourceWatcher sourceWatcher)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
