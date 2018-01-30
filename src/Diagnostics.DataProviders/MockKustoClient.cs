using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Diagnostics.DataProviders
{
    class MockKustoClient: IKustoClient
    {
        public async Task<DataTable> ExecuteQueryAsync(string query, string stampName, string requestId = null)
        {
            switch(query)
            {
                case "TestA":
                    return await GetTestA();
            }

            return new DataTable("Empty");
        }

        private Task<DataTable> GetTestA()
        {
            var d = new DataTable("TableTitle");

            d.Columns.Add("TIMESTAMP", typeof(DateTime));
            d.Columns.Add("Value", typeof(int));

            d.Rows.Add(DateTime.UtcNow.AddMinutes(-10), 5);
            d.Rows.Add(DateTime.UtcNow.AddMinutes(-5), 8);
            d.Rows.Add(DateTime.UtcNow, 12);

            return Task.FromResult(d);
        }
    }
}
