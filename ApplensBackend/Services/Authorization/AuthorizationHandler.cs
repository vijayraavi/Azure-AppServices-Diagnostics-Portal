using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace AppLensV3.Authorization
{
    public class SecurityGroupConfig{
        public string GroupName {get; set;}
        public string GroupId {get; set;}
    }

    class SecurityGroupRequirement: IAuthorizationRequirement{
        public string SecurityGroupObjectId {get;}

        public SecurityGroupRequirement(string securityGroupObjectId){
            SecurityGroupObjectId = securityGroupObjectId;
        }
    }

    class SecurityGroupHandler : AuthorizationHandler<SecurityGroupRequirement>
    {
        private readonly string graphResourceUrl = "https://graph.microsoft.com/";
        private readonly string tenantAuthorityUrl = "https://login.windows.net/microsoft.com";
        private readonly string graphUrl = "https://graph.microsoft.com/v1.0/users/{0}/checkMemberGroups";
        private readonly int tokenRefreshIntervalInMs = 50*60*1000; //50 minutes
        private readonly int loggedInUserCacheClearIntervalInMs = 60*60*1000; //1 hour
        private readonly int loggedInUserExpiryIntervalInSeconds = 6*60*60; //6 hours
        private AuthenticationContext _authContext;
        private ClientCredential _clientCredential;
        private string _authorizationToken;
        private bool _tokenAcquiredAtleastOnce;
        private Task<AuthenticationResult> _acquireTokenTask;
        private string _appClientId;
        private string _appClientSecret;

        public SecurityGroupHandler(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _appClientId = configuration["ApplensAuthorization:ClientId"];
            _appClientSecret = configuration["ApplensAuthorization:ClientSecret"];
            _authContext = new AuthenticationContext(tenantAuthorityUrl);
            _clientCredential = new ClientCredential(_appClientId, _appClientSecret);
            _tokenAcquiredAtleastOnce = false;
            loggedInUsersCache = new Dictionary<string, Dictionary<string, long>>();
            var securityGroups = new List<SecurityGroupConfig>();
            configuration.Bind("SecurityGroups", securityGroups);
            foreach (var securityGroup in securityGroups){
                loggedInUsersCache.Add(securityGroup.GroupId, new Dictionary<string, long>());
            }

            StartTokenRefresh();
            ClearLoggedInUserCache();
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task StartTokenRefresh()
        {
            while (true)
            {
                DateTime invocationStartTime = DateTime.UtcNow;
                string exceptionType = string.Empty;
                string exceptionDetails = string.Empty;
                string message = string.Empty;

                try
                {
                    _acquireTokenTask = _authContext.AcquireTokenAsync(graphResourceUrl, _clientCredential);
                    AuthenticationResult authResult = await _acquireTokenTask;
                    _authorizationToken = GetAuthTokenFromAuthenticationResult(authResult);
                    _tokenAcquiredAtleastOnce = true;
                    message = "Token Acquisition Status : Success";
                }
                catch (Exception ex)
                {
                    exceptionType = ex.GetType().ToString();
                    exceptionDetails = ex.ToString();
                    message = "Token Acquisition Status : Failed";
                }
                finally
                {
                    DateTime invocationEndTime = DateTime.UtcNow;
                    long latencyInMs = Convert.ToInt64((invocationEndTime - invocationStartTime).TotalMilliseconds);

                    // TODO : Log an Event
                }

                await Task.Delay(tokenRefreshIntervalInMs);
            }
        }

        private string GetAuthTokenFromAuthenticationResult(AuthenticationResult authenticationResult)
        {
            return $"{authenticationResult.AccessTokenType} {authenticationResult.AccessToken}";
        }

        public async Task<string> GetAuthorizationTokenAsync()
        {
            if (!_tokenAcquiredAtleastOnce)
            {
                var authResult = await _acquireTokenTask;
                return GetAuthTokenFromAuthenticationResult(authResult);
            }

            return _authorizationToken;
        }

        private IHttpContextAccessor _httpContextAccessor = null;
        private Dictionary<string, Dictionary<string, long>> loggedInUsersCache;

        private readonly Lazy<HttpClient> _client = new Lazy<HttpClient>(() =>
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        });

        private HttpClient _httpClient
        {
            get
            {
                return _client.Value;
            }
        }

        private async Task ClearLoggedInUserCache(){
            while (true){
                long now = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                foreach(KeyValuePair<string, Dictionary<string, long>> securityGroupCache in loggedInUsersCache){
                    foreach(KeyValuePair<string, long> user in securityGroupCache.Value){
                        if ((now - user.Value) > loggedInUserExpiryIntervalInSeconds){
                            // Pop out user from logged in users list
                            securityGroupCache.Value.Remove(user.Key);
                        }
                    }
                }
                await Task.Delay(loggedInUserCacheClearIntervalInMs);
            }
        }

        private void AddUserToCache(string groupId, string userId){
            Dictionary<string, long> securityGroup;
            if (loggedInUsersCache.TryGetValue(groupId, out securityGroup)){
                long userTimeStamp;
                long ts = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                if (securityGroup.TryGetValue(userId, out userTimeStamp)) {
                    securityGroup[userId] = ts;
                }
                else{
                    securityGroup.Add(userId, ts);
                }
            }
        }

        private Boolean IsUserInCache(string groupId, string userId){
            Dictionary<string, long> securityGroup;
            if (loggedInUsersCache.TryGetValue(groupId, out securityGroup)){
                long userTimeStamp;
                if (securityGroup.TryGetValue(userId, out userTimeStamp)) {
                    return true;
                }
            }
            return false;
        }

        private async Task<Boolean> CheckSecurityGroupMembership(string userId, string securityGroupObjectId)
        {
            var requestUrl = string.Format(graphUrl, userId);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            Dictionary<string, Array> requestParams = new Dictionary<string, Array>();
            string[] groupIds = { securityGroupObjectId };
            requestParams.Add("groupIds", groupIds);
            string authorizationToken = await GetAuthorizationTokenAsync();
            request.Headers.Add("Authorization", authorizationToken);
            request.Content = new StringContent(JsonConvert.SerializeObject(requestParams), Encoding.UTF8, "application/json");

            HttpResponseMessage responseMsg = await _httpClient.SendAsync(request);
            var res = await responseMsg.Content.ReadAsStringAsync();
            dynamic groupIdsResponse = JsonConvert.DeserializeObject(res);
            string[] groupIdsReturned = groupIdsResponse.value.ToObject<string[]>();
            if (groupIdsReturned.Contains(securityGroupObjectId))
            {
                return true;
            }

            return false;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SecurityGroupRequirement requirement) {
            HttpContext httpContext = _httpContextAccessor.HttpContext;
            Boolean isMember = false;
            string userId = null;
            try
            {
                string authorization = httpContext.Request.Headers["Authorization"].ToString();
                string accessToken = authorization.Split(" ")[1];
                var token = new JwtSecurityToken(accessToken);
                object upn;
                if (token.Payload.TryGetValue("upn", out upn)){
                    userId = upn.ToString();
                    if (userId != null){
                        if (IsUserInCache(requirement.SecurityGroupObjectId, userId)){
                            isMember = true;
                        }
                        else{
                            isMember = CheckSecurityGroupMembership(userId, requirement.SecurityGroupObjectId).GetAwaiter().GetResult();
                            if (isMember){
                                AddUserToCache(requirement.SecurityGroupObjectId, userId);
                            }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                isMember = false;
            }

            if (isMember)
            {
                context.Succeed(requirement);
                return Task.FromResult(0);
            }

            context.Fail();
            return Task.FromResult(0);
        }

    }
}
