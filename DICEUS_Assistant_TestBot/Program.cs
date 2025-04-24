using DotNetEnv;
using DICEUS_Assistant_TestBot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using DICEUS_Assistant_TestBot.Handlers;
using System.Net;

namespace DICEUS_Assistant_TestBot
{
	internal class Program
	{
		private static async Task Main(string[] args)
		{
			// Load environment variables from the specified .env file
			var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
			var botApi = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");

			// Start simple HTTP server to keep Render happy
			var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";

			StartHttpServer(port);
			StartKeepAlivePing(port);

			Console.WriteLine($"API Key: {apiKey}");

			if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(botApi))
			{
				Console.WriteLine("❌ Environment variables not found. Make sure OPENAI_API_KEY and TELEGRAM_BOT_TOKEN are set.");
				return;
			}

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

			await Task.Delay(Timeout.Infinite, cts.Token);
		}

		// Start local HTTP server
		private static void StartHttpServer(string port)
		{
			Task.Run(() =>
			{
				try
				{
					var listener = new HttpListener();
					listener.Prefixes.Add($"http://+:{port}/");
					listener.Start();
					Console.WriteLine($"HTTP server started on port {port}");

					// Keep handling requests
					while (true)
					{
						var context = listener.GetContext();
						context.Response.StatusCode = 200;
						context.Response.Close();
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"HTTP server error: {ex.Message}");
				}
			});
		}

		// Pinging lo
		private static void StartKeepAlivePing(string port)
		{
			Task.Run(async () =>
			{
				using var client = new HttpClient();
				var url = $"http://localhost:{port}/";

				while (true)
				{
					try
					{
						await client.GetAsync(url);
						Console.WriteLine("📡 Self-ping sent.");
					}
					catch
					{
						Console.WriteLine("⚠️ Self-ping failed.");
					}

					await Task.Delay(TimeSpan.FromMinutes(5));
				}
			});
		}
	}
}
