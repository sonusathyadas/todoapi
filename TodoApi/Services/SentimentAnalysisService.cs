using System;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;

namespace TodoApi.Services
{
    public class SentimentAnalysisService
    {
        private readonly OpenAIClient _openAIClient;
        private readonly string _apiKey;
        private readonly string _endpoint;

        public SentimentAnalysisService(OpenAIClient openAIClient, string apiKey, string endpoint)
        {
            _openAIClient = openAIClient;
            _apiKey = apiKey;
            _endpoint = endpoint;
        }

        public async Task<string> AnalyzeSentimentAsync(string text)
        {
            var request = new ChatCompletionsOptions
            {
                Messages =
                {
                    new ChatMessage(ChatRole.System, "You are a sentiment analysis assistant."),
                    new ChatMessage(ChatRole.User, text)
                }
            };

            var response = await _openAIClient.GetChatCompletionsAsync(_endpoint, request);
            var sentiment = response.Value.Choices[0].Message.Content;

            return sentiment;
        }
    }
}
