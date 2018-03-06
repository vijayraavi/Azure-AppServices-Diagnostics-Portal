using Diagnostics.ModelsAndUtils;
using Diagnostics.RuntimeHost.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.RuntimeHost.Services
{
    public interface ICompilerHostClient
    {
        Task<CompilerResponse> GetCompilationResponse(string script);
    }

    public class CompilerHostClient : ICompilerHostClient, IDisposable
    {
        private SemaphoreSlim _semaphoreObject;
        private string _compilerHostUrl;
        private HttpClient _httpClient;
        private bool _isComplierHostRunning;
        private int _processId;
        private string _dotNetProductName;

        public CompilerHostClient(IHostingEnvironment env, IConfiguration configuration)
        {
            _compilerHostUrl = $@"http://localhost:{CompilerHostConstants.Port}";
            _httpClient = new HttpClient
            {
                MaxResponseContentBufferSize = Int32.MaxValue
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _isComplierHostRunning = false;
            _semaphoreObject = new SemaphoreSlim(1, 1);
            _processId = -1;
            _dotNetProductName = "dotnet";

            StartProcessMonitor();
        }
        
        public async Task<CompilerResponse> GetCompilationResponse(string script)
        {
            await _semaphoreObject.WaitAsync();
            
            try
            {
                if (!_isComplierHostRunning)
                {
                    await LaunchCompilerHostProcess();
                }

                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_compilerHostUrl}/api/compilerhost")
                {
                    Content = new StringContent(JsonConvert.SerializeObject(PrepareRequestBody(script)), Encoding.UTF8, "application/json")
                };

                HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage);
                string content = await responseMessage.Content.ReadAsStringAsync();
                var compilerResponse = JsonConvert.DeserializeObject<CompilerResponse>(content);

                return compilerResponse;
            }
            finally
            {
                _semaphoreObject.Release();
            }
        }

        private async Task LaunchCompilerHostProcess()
        {
            string serverDir = @"E:\git\Azure-WebApps-Support-Center\src\Diagnostics.CompilerHost\bin\Debug\netcoreapp2.0";
            
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = serverDir,
                    FileName = _dotNetProductName,
                    Arguments = $@"Diagnostics.CompilerHost.dll --urls {_compilerHostUrl}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = false
                }
            };

            proc.Start();
            // TODO : Remove artificial wait.
            await Task.Delay(5000);

            if (!proc.HasExited)
            {
                _isComplierHostRunning = true;
                _processId = proc.Id;
            }
        }

        private async void StartProcessMonitor()
        {
            while (true)
            {
                Process proc = null;
                if (_processId != -1)
                {
                    proc = Process.GetProcessById(_processId);
                }

                if (proc != null && !proc.HasExited)
                {
                    if(proc.WorkingSet64 > CompilerHostConstants.ProcessMemoryThresholdInBytes)
                    {
                        try
                        {
                            await _semaphoreObject.WaitAsync();
                            proc.Kill();
                            proc.WaitForExit();
                            _processId = -1;
                            _isComplierHostRunning = false;
                        }
                        finally
                        {
                            _semaphoreObject.Release();
                        }
                    }
                }
                else
                {
                    _processId = -1;
                    _isComplierHostRunning = false;
                }

                await Task.Delay(CompilerHostConstants.PollIntervalInMs);
            }
        }

        private object PrepareRequestBody(string scriptText)
        {
            return new
            {
                script = scriptText
            };
        }
        
        public void Dispose()
        {
            if(_semaphoreObject != null)
            {
                _semaphoreObject.Dispose();
            }

            if(_httpClient != null)
            {
                _httpClient.Dispose();
            }
        }
    }
}
