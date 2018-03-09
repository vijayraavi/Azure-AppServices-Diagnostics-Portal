using Diagnostics.RuntimeHost.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace Diagnostics.RuntimeHost.Services
{
    public interface IGithubClient : IDisposable
    {
        string UserName { get; }

        string RepoName { get; }

        string Branch { get; }

        Task<HttpResponseMessage> Get(string url, string etag = "");

        Task<HttpResponseMessage> Get(HttpRequestMessage request);

        Task DownloadFile(string fileUrl, string destinationPath);
    }

    public class GithubClient : IGithubClient
    {
        private IHostingEnvironment _env;
        private IConfiguration _config;
        private string _userName;
        private string _repoName;
        private string _branch;
        private string _clientId;
        private string _clientSecret;
        private HttpClient _httpClient;

        public string UserName => _userName;

        public string RepoName => _repoName;

        public string Branch => _branch;

        public GithubClient(IHostingEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _config = configuration;
            LoadConfigurations();
            ValidateConfigurations();
            InitializeHttpClient();
        }

        public GithubClient(string userName, string repoName, string branch = "master", string clientId = "", string clientSecret = "")
        {
            _userName = userName;
            _repoName = repoName;
            _branch = !string.IsNullOrWhiteSpace(branch) ? branch : "master";
            _clientId = clientId ?? string.Empty; ;
            _clientSecret = clientSecret ?? string.Empty;
            ValidateConfigurations();
            InitializeHttpClient();
        }

        public Task<HttpResponseMessage> Get(string url, string etag = "")
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, AppendQueryStringParams(url));
            if (!string.IsNullOrWhiteSpace(etag))
            {
                request.Headers.Add("If-None-Match", etag);
            }

            return Get(request);
        }
        
        public Task<HttpResponseMessage> Get(HttpRequestMessage request)
        {
            return _httpClient.SendAsync(request);
        }

        public async Task DownloadFile(string fileUrl, string destinationPath)
        {
            using (HttpResponseMessage httpResponse = await Get(fileUrl))
            {
                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed while getting resource : {fileUrl} . Http Status Code : {httpResponse.StatusCode}");
                }

                using (Stream srcStream = await httpResponse.Content.ReadAsStreamAsync(),
                        destStream = new FileStream(destinationPath, FileMode.Create))
                {
                    await srcStream.CopyToAsync(destStream);
                }
            }
        }

        public void Dispose()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }
        }

        private void LoadConfigurations()
        {
            if (_env.IsProduction())
            {
                _userName = (string)Registry.GetValue(RegistryConstants.GithubWatcherRegistryPath, RegistryConstants.GithubUserNameKey, string.Empty);
                _repoName = (string)Registry.GetValue(RegistryConstants.GithubWatcherRegistryPath, RegistryConstants.GithubRepoNameKey, string.Empty);
                _branch = (string)Registry.GetValue(RegistryConstants.GithubWatcherRegistryPath, RegistryConstants.GithubBranchKey, string.Empty);
                _clientId = (string)Registry.GetValue(RegistryConstants.GithubWatcherRegistryPath, RegistryConstants.GithubClientIdKey, string.Empty);
                _clientSecret = (string)Registry.GetValue(RegistryConstants.GithubWatcherRegistryPath, RegistryConstants.GithubClientSecretKey, string.Empty);
            }
            else
            {
                _userName = (_config[$"SourceWatcher:Github:{RegistryConstants.GithubUserNameKey}"]).ToString();
                _repoName = (_config[$"SourceWatcher:Github:{RegistryConstants.GithubRepoNameKey}"]).ToString();
                _branch = (_config[$"SourceWatcher:Github:{RegistryConstants.GithubBranchKey}"]).ToString();
                _clientId = (_config[$"SourceWatcher:Github:{RegistryConstants.GithubClientIdKey}"]).ToString();
                _clientSecret = (_config[$"SourceWatcher:Github:{RegistryConstants.GithubClientSecretKey}"]).ToString();
            }

            _branch = !string.IsNullOrWhiteSpace(_branch) ? _branch : "master";
            _clientId = _clientId ?? string.Empty;
            _clientSecret = _clientSecret ?? string.Empty;
        }

        private void ValidateConfigurations()
        {
            if (string.IsNullOrWhiteSpace(_userName))
            {
                throw new ArgumentNullException("Github_UserName");
            }

            if (string.IsNullOrWhiteSpace(_repoName))
            {
                throw new ArgumentNullException("Github_RepoName");
            }
        }

        private void InitializeHttpClient()
        {
            _httpClient = new HttpClient
            {
                MaxResponseContentBufferSize = Int32.MaxValue,
                Timeout = TimeSpan.FromSeconds(30)
            };
            
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("User-Agent", _userName);
        }

        private string AppendQueryStringParams(string url)
        {
            var uriBuilder = new UriBuilder(url);
            var queryParams = HttpUtility.ParseQueryString(uriBuilder.Query);
            queryParams.Add("client_id", _clientId);
            queryParams.Add("client_secret", _clientSecret);
            uriBuilder.Query = queryParams.ToString();
            return uriBuilder.ToString();
        }
    }
}
