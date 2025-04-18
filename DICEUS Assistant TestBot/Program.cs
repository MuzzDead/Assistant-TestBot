using DICEUS_Assistant_TestBot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;

namespace DICEUS_Assistant_TestBot
{
	internal class Program
	{
		private static async Task Main(string[] args)
		{
			var botClient = new TelegramBotClient("7369978362:AAFZfJy6AO--ZPKh3C3ifUy4zUig_3Heeyo");

			using var cts = new CancellationTokenSource();

			var receiverOptions = new ReceiverOptions
			{
				AllowedUpdates = Array.Empty<UpdateType>()
			};

			var updateHandler = new BotUpdateHandler(botClient);

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
