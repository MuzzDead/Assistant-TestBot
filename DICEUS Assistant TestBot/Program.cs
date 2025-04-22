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
			Env.Load("D:\\C# Start\\DICEUS Assistant TestBot\\DICEUS Assistant TestBot\\.env");
			var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
			var botApi = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");

			var botClient = new TelegramBotClient(botApi);

			// HttpClient for OpenAI
			var httpClient = new HttpClient();
			var openAiService = new OpenAIService(httpClient, apiKey);

			var callbackHandler = new CallbackQueryHandler(openAiService);


			using var cts = new CancellationTokenSource();

			var receiverOptions = new ReceiverOptions
			{
				AllowedUpdates = Array.Empty<UpdateType>()
			};

			var updateHandler = new BotUpdateHandler(botClient, callbackHandler);

			botClient.StartReceiving(
				updateHandler,
				receiverOptions,
				cancellationToken: cts.Token
			);

			var me = await botClient.GetMe();
			Console.WriteLine($"Bot {me.Username} is running...");

			Console.ReadLine();
			cts.Cancel();
		}
	}
}
