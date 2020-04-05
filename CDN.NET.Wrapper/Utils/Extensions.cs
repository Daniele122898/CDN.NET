using System.Net.Http;
using System.Threading.Tasks;
using ArgonautCore.Maybe;

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

        /// <summary>
        /// Checks the object with reflection if it has at least one not null property. 
        /// </summary>
        /// <param name="obj">The object to check</param>
        /// <returns>Boolean whether it has at least one non null property</returns>
        public static bool HasAtLeastOneProperty(this object obj)
        {
            // Use reflection to get properties of object
            var typeInfo = obj.GetType();
            var propertyInfos = typeInfo.GetProperties();

            foreach (var property in propertyInfos)
            {
                if (property.GetValue(obj, null) != null)
                {
                    // At least one property is not null
                    return true;
                }
            }
            // We have not returned true yet, thus all properties are null
            return false;
        }
    }
}