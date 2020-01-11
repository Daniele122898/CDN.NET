using System.Net.Http;
using System.Threading.Tasks;

namespace CDN.NET.Wrapper.Utils
{
    public static class Extensions
    {
        public static async Task<Maybe<HttpResponseMessage>> EnsureSuccessAndProperReturn(this HttpResponseMessage resp)
        {
            return (await Maybe<HttpResponseMessage>.InitAsync(async () =>
            {
                if (!resp.IsSuccessStatusCode)
                {
                    var ex = new HttpRequestException(
                        $"Response status code does not indicate success: " +
                        $"{((int) resp.StatusCode)} \n" +
                        $"Reason: {await resp.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                    return (null, ex);
                }
                return (resp, null);
            }).ConfigureAwait(false));
        }
    }
}