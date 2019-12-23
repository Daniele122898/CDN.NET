using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;
using CDN.NET.Wrapper.Dtos;
using CDN.NET.Wrapper.Enums;
using CDN.NET.Wrapper.Models;
using CDN.NET.Wrapper.Utils;

namespace CDN.NET.Wrapper.Client
{
    public partial class CdnClient
    {

        public string CurrentToken
        {
            get
            {
                switch (CurrentAuthenticationType)
                {
                    case AuthenticationType.Jwt:
                        return _jwtToken;
                    case AuthenticationType.ArgonautToken:
                        return _argonautToken;
                    default:
                        return "";
                }
            }
        }
        
        private const string ApiAuthType = "X-Argonaut";

        private string _argonautToken;
        private string _jwtToken;
        private UserInternal _userInfo = new UserInternal();

        /// <inheritdoc />
        public async Task<LoginResponse> Login(string username, string password)
        {
            var userCreds = new UserCredentials() {Username = username, Password = password};
            LoginResponse loginResponse = await GetAndMapResponse<LoginResponse>(Endpoints.Login, HttpMethods.Post, userCreds, true).ConfigureAwait(false);

            _userInfo.Id = loginResponse.User.Id;
            _userInfo.Username = username;
            _userInfo.Password = password;
            _jwtToken = loginResponse.Token;
            
            this.UseAuthenticationMethod(AuthenticationType.Jwt);
            return loginResponse;
        }

        /// <inheritdoc />
        public async Task<User> Register(string username, string password)
        {
            var userCreds = new UserCredentials(){Username = username, Password = password};
            User userResponse = await GetAndMapResponse<User>(Endpoints.Register, HttpMethods.Post, userCreds, true).ConfigureAwait(false);
            _userInfo.Id = userResponse.Id;
            _userInfo.Username = username;
            _userInfo.Password = password;

            return userResponse;
        }

        /// <inheritdoc />
        public async Task<string> GetApiKey()
        {
            string token = await GetResponse(Endpoints.ApiKey)
                .ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            _argonautToken = token;
            this.UseAuthenticationMethod(AuthenticationType.ArgonautToken);
            return token;
        }

        /// <inheritdoc />
        public async Task DeleteApiKey()
        {
            await GetResponse(Endpoints.ApiKey, HttpMethods.Delete).ConfigureAwait(false);
            _argonautToken = "";
            this.UseAuthenticationMethod(AuthenticationType.Jwt);
        }


        /// <inheritdoc />
        public void UseAuthenticationMethod(AuthenticationType authenticationType)
        {
            switch (authenticationType)
            {
                case AuthenticationType.ArgonautToken:
                    CurrentAuthenticationType = AuthenticationType.ArgonautToken;
                    
                    _client.DefaultRequestHeaders.Authorization = null;
                    _client.DefaultRequestHeaders.Add(ApiAuthType, _argonautToken);
                    
                    break;
                case AuthenticationType.Jwt:
                    CurrentAuthenticationType = AuthenticationType.Jwt;

                    _client.DefaultRequestHeaders.Remove(ApiAuthType);
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(authenticationType), "Non supported Enum type");
            }
        }

        /// <inheritdoc />
        public async Task<(bool success, string message)> TestAuthentication()
        {
            var response = await _client.GetAsync(Endpoints.AuthTest).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return (false, null);
            }

            return (true, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Check if the current token is still valid and if not refresh automatically
        /// </summary>
        /// <returns>Completed Task if successfull</returns>
        /// <exception cref="AuthenticationException">If the token could not be refreshed</exception>
        private async Task CheckTokenValidityAndRefresh()
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(_jwtToken);
            // Token is still valid so we're fine
            if (jwtToken.ValidTo > DateTime.UtcNow.AddSeconds(30))
            {
                return;
            }
            // Otherwise check if we have user info and then renew
            if (string.IsNullOrWhiteSpace(_userInfo.Username) || string.IsNullOrWhiteSpace(_userInfo.Password))
            {
                throw new AuthenticationException("Your JWT token is not valid anymore and no username and password have been passed to the client, " +
                                                  "thus the token could not be refreshed automatically.");
            }

            await this.Login(_userInfo.Username, _userInfo.Password).ConfigureAwait(false);
        }
    }
}