using DICEUS_Assistant_TestBot.Models;
using DICEUS_Assistant_TestBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace DICEUS_Assistant_TestBot.Handlers;

public static class CallbackQueryHandler
{
	public static async Task HandleAsync(TelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
	{
		var userId = callbackQuery.From.Id;
		var session = SessionStorage.GetOrCreate(userId);

		switch (callbackQuery.Data)
		{
			case "confirm_passport_yes":
				await HandlePassportConfirmed(botClient, callbackQuery, session, cancellationToken);
				break;

			case "confirm_passport_no":
				await HandlePassportRejected(botClient, callbackQuery, session, cancellationToken);
				break;

			case "confirm_techpass_yes":
				await HandleTechPassConfirmed(botClient, callbackQuery, session, cancellationToken);
				break;

			case "confirm_techpass_no":
				await HandleTechPassRejected(botClient, callbackQuery, session, cancellationToken);
				break;

			case "price_accept":
				await HandlePriceAccepted(botClient, callbackQuery, session, cancellationToken);
				break;

			case "price_reject":
				await HandlePriceRejected(botClient, callbackQuery, session, cancellationToken);
				break;

		}

		if (callbackQuery.Message != null)
		{
			await botClient.EditMessageReplyMarkup(
				chatId: callbackQuery.Message.Chat.Id,
				messageId: callbackQuery.Message.MessageId,
				replyMarkup: null,
				cancellationToken: cancellationToken
			);
		}

		await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
	}

	private static async Task HandlePassportConfirmed(TelegramBotClient botClient, CallbackQuery callbackQuery, UserSession session, CancellationToken cancellationToken)
	{
		session.IsPassportConfirmed = true;
		session.CurrentState = BotState.WaitingForTechPassport;

		await botClient.SendMessage(
			chatId: callbackQuery.Message!.Chat.Id,
			text: "✅ Passport data confirmed.\n\n📷 Now, please send a photo of your **vehicle registration document** (tech passport).",
			parseMode: ParseMode.Markdown,
			cancellationToken: cancellationToken
		);
	}

	private static async Task HandlePassportRejected(TelegramBotClient botClient, CallbackQuery callbackQuery, UserSession session, CancellationToken cancellationToken)
	{
		session.PassportFileId = null;
		session.ExtractedFirstName = null;
		session.ExtractedLastName = null;
		session.IsPassportConfirmed = false;
		session.CurrentState = BotState.WaitingForPassport;

		await botClient.SendMessage(
			chatId: callbackQuery.Message!.Chat.Id,
			text: "❌ Let's try again. Please send a new photo of your passport.",
			cancellationToken: cancellationToken
		);
	}

	private static async Task HandleTechPassConfirmed(TelegramBotClient botClient, CallbackQuery callbackQuery, UserSession session, CancellationToken cancellationToken)
	{
		session.IsTechPassportConfirmed = true;

		await botClient.SendMessage(
			chatId: callbackQuery.Message!.Chat.Id,
			text: "💵 The fixed price for the insurance is *$100*.\n\nDoes this price work for you?",
			parseMode: ParseMode.Markdown,
			replyMarkup: new InlineKeyboardMarkup(new[]
			{
			new[]
			{
				InlineKeyboardButton.WithCallbackData("✅ Yes, that works", "price_accept"),
				InlineKeyboardButton.WithCallbackData("❌ No, it's too expensive", "price_reject")
			}
			}),
			cancellationToken: cancellationToken
		);
	}

	private static async Task HandleTechPassRejected(TelegramBotClient botClient, CallbackQuery callbackQuery, UserSession session, CancellationToken cancellationToken)
	{
		session.TechPassportFileId = null;
		session.FakeTechPassportData = null;
		session.IsTechPassportConfirmed = false;
		session.CurrentState = BotState.WaitingForTechPassport;

		await botClient.SendMessage(
			chatId: callbackQuery.Message!.Chat.Id,
			text: "❌ Please try again. Send a clearer photo of your vehicle registration document.",
			cancellationToken: cancellationToken
		);
	}


	private static async Task HandlePriceAccepted(TelegramBotClient botClient, CallbackQuery callbackQuery, UserSession session, CancellationToken cancellationToken)
	{
		session.IsPriceConfirmed = true;

		// Фіктивний шаблон поліса
		var policyText = $"""
			📄 *Car Insurance Policy*
			
			👤 Name: {session.ExtractedFirstName} {session.ExtractedLastName}
			🆔 Document: none
			🔑 VIN: {session.FakeTechPassportData?["VIN"]}
			🚘 Vehicle: {session.FakeTechPassportData?["Car Brand"]} {session.FakeTechPassportData?["Model"]} ({session.FakeTechPassportData?["Year"]})
			📅 Issue Date: {DateTime.Now:yyyy-MM-dd}
			💰 Price: $100
			📜 Policy ID: {Guid.NewGuid().ToString("N")[..8].ToUpper()}
			
			✅ Thank you for choosing our insurance service!
			""";

		SessionStorage.Reset(callbackQuery.From.Id);

		await botClient.SendMessage(
			chatId: callbackQuery.Message!.Chat.Id,
			text: policyText,
			parseMode: ParseMode.Markdown,
			cancellationToken: cancellationToken
		);
	}


	private static async Task HandlePriceRejected(TelegramBotClient botClient, CallbackQuery callbackQuery, UserSession session, CancellationToken cancellationToken)
	{
		await botClient.SendMessage(
			chatId: callbackQuery.Message!.Chat.Id,
			text: "😔 We're sorry, but $100 is the only available price at this moment.",
			cancellationToken: cancellationToken
		);

		SessionStorage.Reset(callbackQuery.From.Id);

		await botClient.SendMessage(
			chatId: callbackQuery.Message.Chat.Id,
			text: "🔄 If you'd like to try again, please send /start.",
			cancellationToken: cancellationToken
		);
	}
}