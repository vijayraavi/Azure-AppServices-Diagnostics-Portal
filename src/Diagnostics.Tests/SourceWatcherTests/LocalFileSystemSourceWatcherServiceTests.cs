using Diagnostics.ScriptHost;
using Diagnostics.ScriptHost.Models;
using Diagnostics.ScriptHost.SourceWatcher;
using Diagnostics.ScriptHost.SourceWatcher.Interfaces;
using Diagnostics.Scripts;
using Diagnostics.Tests.ScriptHostTests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using Xunit;

namespace Diagnostics.Tests.SourceWatcherTests
{
    public class LocalFileSystemSourceWatcherServiceTests
    {
        private IHostingEnvironment _hostingEnvironment;
        private IConfiguration _configuration;
        private CompilationCache<string, Tuple<Definition, EntityInvoker>> _compilationCacheService;
        private ISourceWatcher _localFileSystemSourceWatcherService;

        public LocalFileSystemSourceWatcherServiceTests()
        {
            _hostingEnvironment = HostingEnviromentBuilder.BuildMockEnvironment();
            _compilationCacheService = new CompilationCache<string, Tuple<Definition, EntityInvoker>>();

            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("LocalFileSystemSourceSettings.json");
            _configuration = configurationBuilder.Build();
            _localFileSystemSourceWatcherService = new LocalFileSystemSourceWatcherService(_hostingEnvironment, _configuration, _compilationCacheService);
        }

        [Fact]
        public async void TestWatcher()
        {
            await _localFileSystemSourceWatcherService.WaitForCompletion();
            var cacheEntries = _compilationCacheService.GetAll().ToList();
            Assert.Equal(2, cacheEntries.Count);

            Assert.True(_compilationCacheService.TryGetValue("testfile1", out Tuple<Definition, EntityInvoker> testEntity1));
            Assert.True(_compilationCacheService.TryGetValue("testfile2", out Tuple<Definition, EntityInvoker> testEntity2));
            Assert.False(_compilationCacheService.TryGetValue("testfile3", out Tuple<Definition, EntityInvoker> testEntity3));

            Assert.True(testEntity1.Item2.IsCompilationSuccessful);
            Assert.True(testEntity2.Item2.IsCompilationSuccessful);

            Assert.Equal("Test File 1", testEntity1.Item1.Name);
        }
    }
}
