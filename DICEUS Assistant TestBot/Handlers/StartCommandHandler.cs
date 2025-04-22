using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions;

namespace DICEUS_Assistant_TestBot.Handlers
{
	public static class StartCommandHandler
	{
		public static async Task HandleAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
		{
			string response = "👋 Hello! I am your assistant for purchasing car insurance.\n" +
							  "📷 Now, please send a photo of your passport to get started.";

			await botClient.SendMessage(
				chatId: message.Chat.Id,
				text: response,
				cancellationToken: cancellationToken
			);
		}
	}
}
