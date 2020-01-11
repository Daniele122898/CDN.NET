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
        public async Task<Maybe<LoginResponse>> Login(string username, string password)
        {
            var userCreds = new UserCredentials() {Username = username, Password = password};
            Maybe<LoginResponse> loginResponse =
                await GetAndMapResponse<LoginResponse>(Endpoints.Login, HttpMethods.Post, userCreds, true)
                    .ConfigureAwait(false);

            return loginResponse.Do(
                loginResp =>
                {
                    _userInfo.Id = loginResp.User.Id;
                    _userInfo.Username = username;
                    _userInfo.Password = password;
                    _jwtToken = loginResp.Token;

                    this.UseAuthenticationMethod(AuthenticationType.Jwt);
                }
            );
        }

        /// <inheritdoc />
        public async Task<Maybe<User>> Register(string username, string password)
        {
            var userCreds = new UserCredentials() {Username = username, Password = password};
            Maybe<User> userResponseMaybe =
                await GetAndMapResponse<User>(Endpoints.Register, HttpMethods.Post, userCreds, true)
                    .ConfigureAwait(false);

            return userResponseMaybe.Do((userResponse) =>
            {
                _userInfo.Id = userResponse.Id;
                _userInfo.Username = username;
                _userInfo.Password = password;
            });
        }

        /// <inheritdoc />
        public async Task<Maybe<string>> GetApiKey()
        {
            Maybe<string> tokenMaybe = await GetResponse(Endpoints.ApiKey)
                .ConfigureAwait(false);

            return tokenMaybe.Get<Maybe<string>>(
                (token) =>
                {
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        return Maybe.FromErr<string>("Failed to get token");
                    }

                    _argonautToken = token;
                    this.UseAuthenticationMethod(AuthenticationType.ArgonautToken);
                    return tokenMaybe;
                },
                (e) => tokenMaybe
            );
        }

        /// <inheritdoc />
        public async Task<Maybe<bool>> DeleteApiKey()
        {
            var resp = await GetRawResponseAndEnsureSuccess(Endpoints.ApiKey, HttpMethods.Delete).ConfigureAwait(false);

            return resp.Get<Maybe<bool>>(
                some: (respMsg) =>
                {
                    _argonautToken = "";
                    this.UseAuthenticationMethod(AuthenticationType.Jwt);
                    return Maybe.FromVal(true);
                },
                none: Maybe.FromErr<bool>
            );
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
        /// <returns>Maybe indicating success or error</returns>
        private async Task<Maybe<bool>> CheckTokenValidityAndRefresh()
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(_jwtToken);
            // Token is still valid so we're fine
            if (jwtToken.ValidTo > DateTime.UtcNow.AddSeconds(30))
            {
                return Maybe.FromVal(true);
            }

            // Otherwise check if we have user info and then renew
            if (string.IsNullOrWhiteSpace(_userInfo.Username) || string.IsNullOrWhiteSpace(_userInfo.Password))
            {
                return Maybe.FromErr<bool>(
                    new AuthenticationException(
                        "Your JWT token is not valid anymore and no username and password have been passed to the client, " +
                        "thus the token could not be refreshed automatically."));
            }

            await this.Login(_userInfo.Username, _userInfo.Password).ConfigureAwait(false);
            return Maybe.FromVal<bool>(true);
        }
    }
}