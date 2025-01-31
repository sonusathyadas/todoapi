using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TodoApi.Services
{
    public class SentimentAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _endpoint;

        public SentimentAnalysisService(HttpClient httpClient, string apiKey, string endpoint)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
            _endpoint = endpoint;
        }

        public async Task<string> AnalyzeSentimentAsync(string text)
        {
            var requestBody = new
            {
                documents = new[]
                {
                    new { id = "1", language = "en", text }
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);

            var response = await _httpClient.PostAsync(_endpoint, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(responseBody);

            return result.documents[0].sentiment;
        }
    }
}
