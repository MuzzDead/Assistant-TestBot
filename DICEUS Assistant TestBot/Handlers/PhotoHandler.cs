using DICEUS_Assistant_TestBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DICEUS_Assistant_TestBot.Handlers;

public static class PhotoHandler
{
	public static async Task HandleAsync(TelegramBotClient botClient, Message message, CancellationToken cancellationToken)
	{
		if (message.Photo == null || message.Photo.Length == 0)
		{
			await botClient.SendMessage(
				chatId: message.Chat.Id,
				text: "📷 Please send a valid photo.",
				cancellationToken: cancellationToken);
			return;
		}

		var fileId = message.Photo.Last().FileId;
		var file = await botClient.GetFile(fileId, cancellationToken);
		var filePath = file.FilePath;

		var extractedData = FakeMindeeService.ExtractDataFromPassport(filePath);

		string reply = $"🔍 I extracted the following data:\n" +
					   $"👤 First name: {extractedData.FirstName}\n" +
					   $"👤 Last name: {extractedData.LastName}\n\n" +
					   $"Is this information correct? (Yes / No)";

		await botClient.SendMessage(
		   chatId: message.Chat.Id,
		   text: reply,
		   cancellationToken: cancellationToken
	   );
	}
}
