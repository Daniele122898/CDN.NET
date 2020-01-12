using System.Net.Http;
using System.Threading.Tasks;

namespace CDN.NET.Wrapper.Utils
{
    public static class Extensions
    {
        public static async Task<Maybe<HttpResponseMessage>> EnsureSuccessAndProperReturn(this HttpResponseMessage resp)
        {
            if (!resp.IsSuccessStatusCode)
            {
                var ex = new HttpRequestException(
                    $"Response status code does not indicate success: " +
                    $"{((int) resp.StatusCode)} \n" +
                    $"Reason: {await resp.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                return Maybe.FromErr<HttpResponseMessage>(ex);
            }

            return Maybe.FromVal(resp);
        }
    }
}