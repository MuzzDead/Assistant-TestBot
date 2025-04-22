using DotNetEnv;
using DICEUS_Assistant_TestBot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using DICEUS_Assistant_TestBot.Handlers;

namespace DICEUS_Assistant_TestBot
{
	internal class Program
	{
		private static async Task Main(string[] args)
		{
			// Load environment variables from the specified .env file
			Env.Load("D:\\C# Start\\DICEUS Assistant TestBot\\DICEUS Assistant TestBot\\.env");

			var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
			var botApi = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");

			// Initialize the Telegram Bot client with the bot token
			var botClient = new TelegramBotClient(botApi);

			var httpClient = new HttpClient();

			// Initialize the OpenAI service using the HttpClient and API key
			var openAiService = new OpenAIService(httpClient, apiKey);

			var callbackHandler = new CallbackQueryHandler(openAiService);

			using var cts = new CancellationTokenSource();

			// Set up receiver options to allow specific types of updates
			var receiverOptions = new ReceiverOptions
			{
				AllowedUpdates = Array.Empty<UpdateType>()
			};

			// Define the update handler for Telegram bot interactions
			var updateHandler = new BotUpdateHandler(botClient, callbackHandler);

			// Start receiving updates
			botClient.StartReceiving(
				updateHandler,
				receiverOptions,
				cancellationToken: cts.Token
			);

			// Get bot information and display its username
			var me = await botClient.GetMe();
			Console.WriteLine($"Bot {me.Username} is running...");

			Console.ReadLine();
			cts.Cancel();
		}
	}
}
