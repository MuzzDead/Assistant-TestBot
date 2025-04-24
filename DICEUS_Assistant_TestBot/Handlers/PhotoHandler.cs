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
		var session = SessionStorage.GetOrCreate(userId); // Retrieve or create a session for the user

		switch (session.CurrentState)
		{
			case BotState.WaitingForPassport:
				// Handle passport photo
				await HandlePassportPhoto(botClient, message, session, cancellationToken);
				break;

			case BotState.WaitingForTechPassport:
				// Handle vehicle registration photo
				await HandleTechPassportPhoto(botClient, message, session, cancellationToken);
				break;

			default:
				// Send an error message if the bot is in an unexpected state
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
		// Check if the message contains a photo
		if (message.Photo == null || message.Photo.Length == 0)
		{
			await botClient.SendMessage(
				chatId: message.Chat.Id,
				text: "📷 Please send a valid passport photo.",
				cancellationToken: cancellationToken
			);
			return;
		}

		// Get the highest quality version of the photo
		var fileId = message.Photo.Last().FileId;
		var file = await botClient.GetFile(fileId, cancellationToken);

		// Download the photo to a memory stream
		var memoryStream = new MemoryStream();
		await botClient.DownloadFile(file, memoryStream, cancellationToken);
		memoryStream.Position = 0;

		// Initialize Mindee service using the API token from environment variables
		var mindeeToken = Environment.GetEnvironmentVariable("MINDEE_TOKEN");
		var mindee = new MindeeService(mindeeToken);
		var extracted = await mindee.ExtractDataFromInternationalIdAsync(memoryStream, file.FilePath);

		bool isInvalid = string.IsNullOrEmpty(extracted.FirstName) || extracted.FirstName == "Unknown"
					  || string.IsNullOrEmpty(extracted.LastName) || extracted.LastName == "Unknown";

		if (isInvalid)
		{
			await botClient.SendMessage(
				chatId: message.Chat.Id,
				text: "❌ Sorry, I couldn't recognize a valid passport in this photo.\n\nPlease try again and make sure the document is clear and visible.",
				cancellationToken: cancellationToken
			);
			return;
		}

		// Save the extracted data into the session
		session.PassportFileId = fileId;
		session.ExtractedFirstName = extracted.FirstName;
		session.ExtractedLastName = extracted.LastName;
		session.CurrentState = BotState.ConfirmingPassport;

		// Prepare a reply with extracted passport details
		var reply = $"🔍 I extracted the following data:\n" +
			$"• First name: {extracted.FirstName}\n" +
			$"• Last name: {extracted.LastName}\n" +
			$"• Sex: {extracted.Sex}\n" +
			$"• Date of birth: {extracted.BirthDate:yyyy-MM-dd}\n" +
			$"• Nationality: {extracted.Nationality}\n" +
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
		// Ensure the message contains a photo
		if (message.Photo == null || message.Photo.Length == 0)
		{
			await botClient.SendMessage(
				chatId: message.Chat.Id,
				text: "📷 Please send a valid photo of your vehicle registration document.",
				cancellationToken: cancellationToken
			);
			return;
		}

		// Save the file ID of the photo
		session.TechPassportFileId = message.Photo.Last().FileId;

		// Generate fake data to simulate vehicle registration info (for demo/testing)
		var data = TechPassportService.GenerateFakeTechData();
		session.FakeTechPassportData = data;
		session.CurrentState = BotState.ConfirmingTechPassport;

		// Build a message with the generated data
		var sb = new StringBuilder();
		sb.AppendLine("🚗 I extracted the following data from your vehicle registration document:");

		foreach (var kv in data)
		{
			sb.AppendLine($"• {kv.Key}: {kv.Value}");
		}
		sb.AppendLine("\nIs this information correct?");

		// Create inline buttons for confirmation
		var replyMarkup = new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData("✅ Yes, the data is correct", "confirm_techpass_yes"),
				InlineKeyboardButton.WithCallbackData("❌ No, the data is incorrect", "confirm_techpass_no")
			}
		});

		// Send the formatted data with confirmation buttons
		await botClient.SendMessage(
			chatId: message.Chat.Id,
			text: sb.ToString(),
			replyMarkup: replyMarkup,
			cancellationToken: cancellationToken
		);
	}

	// Helper method to generate confirmation buttons for passport data
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
