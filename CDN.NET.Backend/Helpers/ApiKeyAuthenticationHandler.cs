using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using CDN.NET.Backend.Models;
using CDN.NET.Backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CDN.NET.Backend.Helpers
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private const string _PROBLEM_DETAILS_CONTENT_TYPE = "application/problem+json";
        private readonly IApiKeyRepository _repo;
        private const string _API_KEY_HEADER_NAME = "X-Argonaut";

        public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options, ILoggerFactory logger, 
            UrlEncoder encoder, ISystemClock clock, IApiKeyRepository repo) : base(options, logger, encoder, clock)
        {
            _repo = repo;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!this.Request.Headers.TryGetValue(_API_KEY_HEADER_NAME, out var apiKeyHeaderValues))
            {
                return AuthenticateResult.NoResult();
            }
            
            if (apiKeyHeaderValues.Count == 0) 
                return AuthenticateResult.NoResult();

            var keyHeader = apiKeyHeaderValues[0];
            if (string.IsNullOrWhiteSpace(keyHeader))
                return AuthenticateResult.NoResult();

            var key = await _repo.GetKey(keyHeader);
            if (key == null) 
                return AuthenticateResult.Fail("Invalid API key provided");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, key.User.Username),
                new Claim(ClaimTypes.NameIdentifier, key.UserId.ToString())
            };
            var identity = new ClaimsIdentity(claims, Options.AuthenticationType);
            var identities = new List<ClaimsIdentity>{identity};
            var principal = new ClaimsPrincipal(identities);
            var ticket = new AuthenticationTicket(principal, Options.Scheme);
            
            return AuthenticateResult.Success(ticket);
        }
        
        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;
            Response.ContentType = _PROBLEM_DETAILS_CONTENT_TYPE;
            var problemDetails = new UnauthorizedResult();

            await Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }

        protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 403;
            Response.ContentType = _PROBLEM_DETAILS_CONTENT_TYPE;
            var problemDetails = new ForbidResult();

            await Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
    }
}