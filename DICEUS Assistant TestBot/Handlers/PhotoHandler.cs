using DICEUS_Assistant_TestBot.Models;
using DICEUS_Assistant_TestBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;


namespace DICEUS_Assistant_TestBot.Handlers;

public static class PhotoHandler
{
	public static async Task HandleAsync(TelegramBotClient botClient, Message message, CancellationToken cancellationToken)
	{
		var userId = message.From!.Id;
		var session = SessionStorage.GetOrCreate(userId);

		switch (session.CurrentState)
		{
			case BotState.WaitingForPassport:
				await HandlePassportPhoto(botClient, message, session, cancellationToken);
				break;

			case BotState.WaitingForTechPassport:
				await HandleTechPassportPhoto(botClient, message, session, cancellationToken);
				break;

			default:
				await botClient.SendMessage(
					chatId: message.Chat.Id,
					text: "Please follow the flow and use the provided buttons.",
					cancellationToken: cancellationToken
				);
				break;
		}
	}

	private static async Task HandlePassportPhoto(TelegramBotClient botClient, Message message, UserSession session, CancellationToken cancellationToken)
	{
		if (message.Photo == null || message.Photo.Length == 0)
		{
			await botClient.SendMessage(
				chatId: message.Chat.Id,
				text: "📷 Please send a valid passport photo.",
				cancellationToken: cancellationToken
			);
			return;
		}

		var fileId = message.Photo.Last().FileId;
		var file = await botClient.GetFile(fileId, cancellationToken);

		var memoryStream = new MemoryStream();
		await botClient.DownloadFile(file, memoryStream, cancellationToken);
		memoryStream.Position = 0;

		// Read Mindee
		var mindee = new MindeeService("de2cb58ebfba1345e5b87a671e81c36c");
		var extracted = await mindee.ExtractDataFromInternationalIdAsync(memoryStream, file.FilePath);

		session.PassportFileId = fileId;
		session.ExtractedFirstName = extracted.FirstName;
		session.ExtractedLastName = extracted.LastName;
		session.CurrentState = BotState.ConfirmingPassport;

		var reply = $"🔍 I extracted the following data:\n" +
			$"• First name: {extracted.FirstName}\n" +
			$"• Last name: {extracted.LastName}\n" +
			$"• Sex: {extracted.Sex}\n" +
			$"• Date of birth: {extracted.BirthDate:yyyy-MM-dd}\n" +
			$"• Nationality: {extracted.Nationality}\n" +
			$"• Personal number: {extracted.PersonalNumber}\n" +
			$"• Document number: {extracted.DocumentNumber}\n" +
			$"• Expiration date: {(extracted.ExpirationDate == DateTime.MinValue ? "Unknown" : extracted.ExpirationDate.ToString("yyyy-MM-dd"))}\n\n" +
			$"Is this information correct?";


		await botClient.SendMessage(
			chatId: message.Chat.Id,
			text: reply,
			replyMarkup: ConfirmationKeyboard(),
			cancellationToken: cancellationToken
		);
	}

	private static async Task HandleTechPassportPhoto(TelegramBotClient botClient, Message message, UserSession session, CancellationToken cancellationToken)
	{
		// TODO
	}

	private static InlineKeyboardMarkup ConfirmationKeyboard()
	{
		return new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData("✅ Yes, the data is correct", "confirm_passport_yes"),
				InlineKeyboardButton.WithCallbackData("❌ No, the data is incorrect", "confirm_passport_no")
			}
		});
	}
}
