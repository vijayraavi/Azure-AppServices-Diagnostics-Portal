using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Diagnostics.Tests.ScriptHostTests
{
    public class HostingEnvironment : IHostingEnvironment
    {
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; }
        public string WebRootPath { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
    }

    public static class HostingEnviromentBuilder
    {
        public static IHostingEnvironment BuildMockEnvironment()
        {
            return new HostingEnvironment()
            {
                EnvironmentName = "mock",
                ApplicationName = "test",
                WebRootPath = "//mockpath",
                ContentRootPath = "//mockpath"
            };
        }
    }
}
