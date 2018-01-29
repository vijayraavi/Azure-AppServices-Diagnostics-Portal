using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diagnostic.DataProviders
{
    public class KustoDataProvider: DiagnosticDataProvider, IDiagnosticDataProvider
    {
        private KustoDataProviderConfiguration _configuration;
        private KustoClient _kustoClient;

        public KustoDataProvider(OperationDataCache cache, KustoDataProviderConfiguration configuration): base(cache)
        {
            _configuration = configuration;
            _kustoClient = new KustoClient(configuration);
        }

        public async Task<DataTable> ExecuteQuery(string query, string stampName)
        {
            return await _kustoClient.ExecuteQueryAsync(query, stampName);
            
        }
    }
}
