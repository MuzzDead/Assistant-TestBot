using DICEUS_Assistant_TestBot.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
public class OpenAIService
{
	private readonly HttpClient _httpClient;
	private readonly string _apiKey;

	public OpenAIService(HttpClient httpClient, string apiKey)
	{
		_httpClient = httpClient;
		_apiKey = apiKey;
	}

	// Generates a car insurance policy
	public async Task<string> GeneratePolicyAsync(UserSession session)
	{
		var tech = session.FakeTechPassportData!;

		string prompt = $"""
			Please generate a clean and professional car insurance policy document based on the following information.
			Format it with clear sections, use realistic insurance terms, and present it like a real printed policy.
			- Name: {session.ExtractedFirstName} {session.ExtractedLastName}
			- VIN: {tech["VIN"]}
			- Car: {tech["Car Brand"]} {tech["Model"]} ({tech["Year"]})
			- Fuel Type: {tech["Fuel Type"]}
			- Engine Volume: {tech["Engine Volume"]}
			- Color: {tech["Color"]}
			- Issue Date: {DateTime.Now:yyyy-MM-dd}
			- Price: 100 USD
			- Coverage: Full coverage valid for 12 months from issue date
			- Customer Service Phone: 098 8888 02 08
			- Customer Service Email: emailforus@gmail.com
			- Insurance Company Name: Secure Auto Insurance
			- Insurance Company Address: 123 Protection Avenue, Kyiv, 02000, Ukraine
			- Insurance Company Contact Information: Tel: +380 44 123 4567, www.secureauto.ua
			Return only the final formatted document as plain text.
			""";

		return await GetOpenAiResponse(prompt, 600);
	}

	// Generates a bot reply
	public async Task<string> GenerateBotReplyAsync(string userTopic)
	{
		string prompt = $"""
			You are a professional and friendly assistant inside a Telegram bot that sells car insurance.
			
			Generate a short, natural response to the following situation:
			
			{userTopic}
			
			Avoid greetings like 'Hi' or 'Hello'. Respond in an informative, human-like, but concise way.
			""";

		return await GetOpenAiResponse(prompt, 200);
	}

	// Sends the prompt to OpenAI and returns the response
	private async Task<string> GetOpenAiResponse(string prompt, int maxTokens)
	{
		// Create request body using Chat Completions API structure
		var requestBody = new
		{
			model = "gpt-4o",
			messages = new[]
			{
				new { role = "user", content = prompt }
			},
			max_tokens = maxTokens
		};

		// Serialize to JSON
		var json = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

		// Set up HTTP request to OpenAI
		using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
		request.Headers.Add("Authorization", $"Bearer {_apiKey}");
		request.Content = json;

		using var response = await _httpClient.SendAsync(request);
		response.EnsureSuccessStatusCode();

		// Parse the response JSON to extract the generated text
		var responseContent = await response.Content.ReadAsStringAsync();
		using var doc = JsonDocument.Parse(responseContent);
		var result = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

		return result!;
	}
}