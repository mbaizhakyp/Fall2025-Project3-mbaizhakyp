using Azure;
using Azure.AI.OpenAI;
using System.Threading.Tasks;
using System.ClientModel;
using OpenAI.Chat;
using System.Collections.Generic;
namespace Fall2025_Project3_mbaizhakyp.Services
{
    public class OpenAIService
    {
        private readonly ChatClient _chatClient;

        public OpenAIService(IConfiguration configuration)
        {
            var endpoint = configuration["OpenAI:Endpoint"];
            var key = configuration["OpenAI:Key"];
            var deploymentName = "gpt-4.1-mini";

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("OpenAI Endpoint or Key is not configured in user secrets.");
            }

            AzureOpenAIClient azureClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(key));
            _chatClient = azureClient.GetChatClient(deploymentName);
        }

        public async Task<string> GetChatCompletionAsync(string userPrompt)
        {
            try
            {
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage("You are a helpful assistant for a movie review website. You only provide the requested text, with no extra conversational fluff. You format lists using a newline character."),
                    new UserChatMessage(userPrompt)
                };

                var chatCompletionOptions = new ChatCompletionOptions
                {
                    MaxOutputTokenCount = 800,
                    Temperature = 0.7f
                };

                var response = await _chatClient.CompleteChatAsync(messages, chatCompletionOptions);

                return response.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}