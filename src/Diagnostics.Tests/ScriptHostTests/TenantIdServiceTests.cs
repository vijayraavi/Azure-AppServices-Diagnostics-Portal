using Diagnostics.ScriptHost.DataProviderServices;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using Xunit;

namespace Diagnostics.Tests.ScriptHostTests
{
    public class TenantIdServiceTests
    {
        private IDataSourcesConfigurationService _dataSourceConfigService;
        private IHostingEnvironment _hostingEnv;
        private ITenantIdService _tenantIdService;
        private Random _rnd;

        public TenantIdServiceTests()
        {
            _rnd = new Random();
            _hostingEnv = HostingEnviromentBuilder.BuildMockEnvironment();
            _dataSourceConfigService = new DataSourcesConfigurationService(_hostingEnv);
            _tenantIdService = new TenantIdService(_dataSourceConfigService);
        }

        [Fact]
        public async void TestGetTenantId()
        {
            List<string> tenantIds = await _tenantIdService.GetTenantIdForStamp($"{_rnd.Next()}stamp", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
            Assert.NotNull(tenantIds);
            Assert.NotEmpty(tenantIds);
        }

        [Fact]
        public async void TestTenantIdCache()
        {
            string stampName = $"{_rnd.Next()}stamp";
            DateTime startTime = DateTime.UtcNow.AddDays(-1);
            DateTime endTime = DateTime.UtcNow;

            List<string> tenantIdsFirstCall = await _tenantIdService.GetTenantIdForStamp(stampName, startTime, endTime);
            List<string> tenantIdsSecondCall = await _tenantIdService.GetTenantIdForStamp(stampName, startTime, endTime);

            Assert.Equal<List<string>>(tenantIdsFirstCall, tenantIdsSecondCall);
        }

        [Fact]
        public async void TestNullStampName()
        {
            DateTime startTime = DateTime.UtcNow.AddDays(-1);
            DateTime endTime = DateTime.UtcNow;

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                List<string> tenantIds = await _tenantIdService.GetTenantIdForStamp(null, startTime, endTime);
            });
        }
    }
}
