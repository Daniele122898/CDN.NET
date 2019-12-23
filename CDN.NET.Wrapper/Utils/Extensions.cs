using System.Net.Http;
using System.Threading.Tasks;

namespace CDN.NET.Wrapper.Utils
{
    public static class Extensions
    {
        public static async Task<HttpResponseMessage> EnsureSuccessAndProperReturn(this HttpResponseMessage resp)
        {
            if (!resp.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Response status code does not indicate success: " +
                            $"{((int) resp.StatusCode)} \n" +
                            $"Reason: {await resp.Content.ReadAsStringAsync().ConfigureAwait(false)}");
            }

            return resp;
        }
    }
}