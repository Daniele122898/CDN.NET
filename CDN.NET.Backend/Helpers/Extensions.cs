using System;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using CDN.NET.Backend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CDN.NET.Backend.Helpers
{
    public static class Extensions
    {
        
        private static JsonSerializerOptions _jsonOptions =  new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };
        
        public static void AddApplicationError(this HttpResponse response, string message)
        {
            response.Headers.Add("Application-Error", message);
            response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        public static void AddPagination(this HttpResponse response, PaginationHeader paginationHeader)
        {
            response.Headers.Add("Pagination", JsonSerializer.Serialize(paginationHeader, _jsonOptions));
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }

        public static int GetRequestUserId(this ControllerBase cb)
        {
            return int.Parse(cb.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        }
        
        public static AuthenticationBuilder AddApiKeySupport(this AuthenticationBuilder authenticationBuilder, Action<ApiKeyAuthenticationOptions> options)
        {
            return authenticationBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DEFAULT_SCHEME, options);
        }
    }
}