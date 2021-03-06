﻿using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ArgonautCore.Maybe;
using CDN.NET.Wrapper.Enums;
using CDN.NET.Wrapper.Utils;

namespace CDN.NET.Wrapper.Client
{
    public partial class CdnClient : ICdnClient
    {
        public AuthenticationType CurrentAuthenticationType { get; private set; } = AuthenticationType.Jwt;

        private static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _client;

        public CdnClient(string baseAddress)
        {
            Uri baseAddUri = new Uri(baseAddress);

            _client = new HttpClient()
            {
                BaseAddress = baseAddUri
            };
        }

        public CdnClient(string baseAddress, string apiToken) : this(baseAddress)
        {
            this.CurrentAuthenticationType = AuthenticationType.ArgonautToken;

            _argonautToken = apiToken;
            this.UseAuthenticationMethod(AuthenticationType.ArgonautToken);
        }

        /// <summary>
        /// Makes a request with the specified method and tries to parse the payload and response
        /// </summary>
        /// <param name="endpoint">The endpoint to request</param>
        /// <param name="httpMethod">What http method to use</param>
        /// <param name="payload">The payload to send on a post request</param>
        /// <param name="disableJwtRefreshCheck">Disable the automatic jwt refresh if not valid anymore</param>
        /// <param name="castPayloadWithoutJsonParsing">Whether to cast the payload to HttpContent directly instead of json parsing.</param>
        /// <typeparam name="T">The type that is expected to be returned and parsed</typeparam>
        /// <returns>The parsed return</returns>
        private async Task<Maybe<T>> GetAndMapResponse<T>(
            string endpoint,
            HttpMethods httpMethod = HttpMethods.Get,
            object payload = null,
            bool disableJwtRefreshCheck = false,
            bool castPayloadWithoutJsonParsing = false) where T : class
        {
            var respMaybe = await GetResponse(endpoint, httpMethod, payload, false, disableJwtRefreshCheck,
                castPayloadWithoutJsonParsing).ConfigureAwait(false);

            return respMaybe.Get<Maybe<T>>(
                some: (responseString) => Maybe.FromVal(JsonSerializer.Deserialize<T>(responseString, _jsonOptions)),
                none: (e) => Maybe.FromErr<T>(e)
            );
            
        }

        /// <summary>
        /// Makes a request with the specified method and tries to parse the payload. 
        /// </summary>
        /// <param name="endpoint">The endpoint to request</param>
        /// <param name="httpMethod">What http method to use</param>
        /// <param name="payload">The payload to send on a post request</param>
        /// <param name="expectNonJson">Whether this method should throw when the response is not json or not. Default is NOT throwing</param>
        /// <param name="disableJwtRefreshCheck">Disable the automatic jwt refresh if not valid anymore</param>
        /// <param name="castPayloadWithoutJsonParsing">Whether to cast the payload to HttpContent directly instead of json parsing.</param>
        /// <returns>The raw string return</returns>
        private async Task<Maybe<string>> GetResponse(
            string endpoint,
            HttpMethods httpMethod = HttpMethods.Get,
            object payload = null,
            bool expectNonJson = true,
            bool disableJwtRefreshCheck = false,
            bool castPayloadWithoutJsonParsing = false)
        {
            var respMaybe = await this.GetRawResponseAndEnsureSuccess(endpoint, httpMethod, payload,
                disableJwtRefreshCheck, castPayloadWithoutJsonParsing).ConfigureAwait(false);

            return (await respMaybe.Get<Task<Maybe<string>>>(
                some: async (HttpResponseMessage response) =>
                {
                    if (!expectNonJson && response.Content.Headers.ContentType.MediaType != "application/json")
                    {
                        var ex = new NotSupportedException("Response was not json and thus not supported");
                        return Maybe.FromErr<string>(ex);
                    }
                    
                    var respStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return Maybe.FromVal(respStr);
                },
                none: Maybe.FromErrTask<string>
                ).ConfigureAwait(false));
        }

        /// <summary>
        /// Makes a request with the specified method. 
        /// </summary>
        /// <param name="endpoint">The endpoint to request</param>
        /// <param name="httpMethod">What http method to use</param>
        /// <param name="payload">The payload to send on a post request</param>
        /// <param name="disableJwtRefreshCheck">Disable the automatic jwt refresh if not valid anymore</param>
        /// <param name="castPayloadWithoutJsonParsing">Whether to cast the payload to HttpContent directly instead of json parsing.</param>
        /// <returns>The raw response object</returns>
        private async Task<Maybe<HttpResponseMessage>> GetRawResponseAndEnsureSuccess(
            string endpoint,
            HttpMethods httpMethod = HttpMethods.Get,
            object payload = null,
            bool disableJwtRefreshCheck = false,
            bool castPayloadWithoutJsonParsing = false)
        {
            // JwtCheck
            if (this.CurrentAuthenticationType == AuthenticationType.Jwt && !disableJwtRefreshCheck)
            {
                var validMaybe = await this.CheckTokenValidityAndRefresh().ConfigureAwait(false);
                if (validMaybe.HasError)
                {
                    return Maybe.FromErr<HttpResponseMessage>(validMaybe.Error);
                }
            }

            HttpResponseMessage response;

            switch (httpMethod)
            {
                case HttpMethods.Get:
                    response = await _client.GetAsync(endpoint).ConfigureAwait(false);
                    break;
                case HttpMethods.Post:
                case HttpMethods.Put:
                    HttpContent content;
                    if (castPayloadWithoutJsonParsing)
                    {
                        content = (HttpContent) payload;
                    }
                    else
                    {
                        string json = JsonSerializer.Serialize(payload, _jsonOptions);
                        content = new StringContent(json, Encoding.UTF8, "application/json");
                    }

                    if (httpMethod == HttpMethods.Put)
                    {
                        response = await _client.PutAsync(endpoint, content).ConfigureAwait(false);
                    }
                    else
                    {
                        response = await _client.PostAsync(endpoint, content).ConfigureAwait(false);
                    }
                    break;
                case HttpMethods.Delete:
                    if (payload == null)
                    {
                        response = await _client.DeleteAsync(endpoint).ConfigureAwait(false);
                    }
                    else
                    {
                        string json = JsonSerializer.Serialize(payload, _jsonOptions);
                        HttpRequestMessage requestMessage = new HttpRequestMessage()
                        {
                            Content = new StringContent(json, Encoding.UTF8, "application/json"),
                            Method = HttpMethod.Delete,
                            RequestUri = new Uri(_client.BaseAddress, endpoint)
                        };

                        response = await _client.SendAsync(requestMessage).ConfigureAwait(false);
                    }

                    break;
                default:
                    throw new ArgumentException("Method not supported", nameof(httpMethod));
            }

            var successMaybe = await response.EnsureSuccessAndProperReturn().ConfigureAwait(false);
            return successMaybe;
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}