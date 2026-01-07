using Newtonsoft.Json;
using System.Text;

namespace Protoris.Clients
{
    public class BaseHttpClient
    {
        private readonly HttpClient _httpClient;

        public BaseHttpClient(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        protected async Task<T> Post<T>(Uri uri, T dto)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception(errorMessage);
            }

            string bodyResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(bodyResponse)!;
        }

        protected async Task<T> Get<T>(Uri uri)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            AddHeaders(requestMessage);
            HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception(errorMessage);
            }

            string bodyResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(bodyResponse)!;
        }

        protected virtual void AddHeaders(HttpRequestMessage request)
        { }
    }
}
