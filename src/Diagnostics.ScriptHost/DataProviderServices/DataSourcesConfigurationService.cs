using Diagnostics.DataProviders;
using Diagnostics.ScriptHost.Utilities;
using Microsoft.AspNetCore.Hosting;

namespace Diagnostics.ScriptHost.DataProviderServices
{

    public interface IDataSourcesConfigurationService
    {
        DataSourcesConfiguration Config { get; }
    }

    public class DataSourcesConfigurationService : IDataSourcesConfigurationService
    {
        private DataSourcesConfiguration _config;

        public DataSourcesConfiguration Config => _config;

        public DataSourcesConfigurationService(IHostingEnvironment env)
        {
            IConfigurationFactory factory = DataProviderHelper.GetDataProviderConfigurationFactory(env);
            _config = factory.LoadConfigurations();
        }
    }
}
