using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Diagnostics.DataProviders
{
    class MockKustoClient: IKustoClient
    {
        public async Task<DataTableResponseObject> ExecuteQueryAsync(string query, string stampName, string requestId = null)
        {
            switch(query)
            {
                case "TestA":
                    return await GetTestA();
            }

            return new DataTableResponseObject();
        }

        private Task<DataTableResponseObject> GetTestA()
        {
            var testColumn = new DataTableResponseColumn();
            testColumn.ColumnName = "TestColumn";
            testColumn.ColumnType = "System.string";
            testColumn.DataType = "string";
            
            var res = new DataTableResponseObject();
            res.Columns = new List<DataTableResponseColumn>(new[] { testColumn });
            
            return Task.FromResult(res);
        }
    }
}
