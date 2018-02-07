using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diagnostics.DataProviders
{
    public class KustoDataProvider: DiagnosticDataProvider, IDiagnosticDataProvider
    {
        private KustoDataProviderConfiguration _configuration;
        private IKustoClient _kustoClient;

        public KustoDataProvider(OperationDataCache cache, KustoDataProviderConfiguration configuration): base(cache)
        {
            _configuration = configuration;
            _kustoClient = KustoClientFactory.GetKustoClient(configuration);
        }

        public async Task<DataTableResponseObject> ExecuteQuery(string query, string stampName)
        {
            return await _kustoClient.ExecuteQueryAsync(query, stampName);
            
        }
    }
}
