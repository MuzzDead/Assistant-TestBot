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
using System.Net.Http;

namespace DICEUS_Assistant_TestBot.Handlers;

public class CallbackQueryHandler
{
	private readonly OpenAIService _openAIService;

	public CallbackQueryHandler(OpenAIService openAiService)
	{
		_openAIService = openAiService;
	}

	public async Task HandleAsync(TelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
	{
		var userId = callbackQuery.From.Id;
		var session = SessionStorage.GetOrCreate(userId);

		await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);

		if (callbackQuery.Message != null)
		{
			await botClient.EditMessageReplyMarkup(
				chatId: callbackQuery.Message.Chat.Id,
				messageId: callbackQuery.Message.MessageId,
				replyMarkup: null,
				cancellationToken: cancellationToken
			);
		}

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
				if (callbackQuery.Message != null)
				{
					await botClient.SendMessage(
						chatId: callbackQuery.Message.Chat.Id,
						text: "⏳ Generating your insurance policy. Please wait...",
						cancellationToken: cancellationToken
					);
				}

				_ = Task.Run(async () =>
				{
					try
					{
						await HandlePriceAccepted(botClient, callbackQuery, session, CancellationToken.None);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error generating PDF: {ex}");

						if (callbackQuery.Message != null)
						{
							await botClient.SendMessage(
								chatId: callbackQuery.Message.Chat.Id,
								text: "❌ Sorry, there was an error generating your policy. Please try again later.",
								cancellationToken: CancellationToken.None
							);
						}
					}
				});
				break;
			case "price_reject":
				await HandlePriceRejected(botClient, callbackQuery, session, cancellationToken);
				break;
		}
	}

	private async Task HandlePassportConfirmed(TelegramBotClient botClient, CallbackQuery callbackQuery, UserSession session, CancellationToken cancellationToken)
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

	private async Task HandlePassportRejected(TelegramBotClient botClient, CallbackQuery callbackQuery, UserSession session, CancellationToken cancellationToken)
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

	private async Task HandleTechPassConfirmed(TelegramBotClient botClient, CallbackQuery callbackQuery, UserSession session, CancellationToken cancellationToken)
	{
		session.IsTechPassportConfirmed = true;

		string reply = await _openAIService.GenerateBotReplyAsync("User confirmed tech passport. Inform them that the fixed insurance price is 100 USD and ask if that works for them.");

		await botClient.SendMessage(
			chatId: callbackQuery.Message!.Chat.Id,
			text: reply,
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

	private async Task HandleTechPassRejected(TelegramBotClient botClient, CallbackQuery callbackQuery, UserSession session, CancellationToken cancellationToken)
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

	private async Task HandlePriceAccepted(TelegramBotClient botClient, CallbackQuery callbackQuery, UserSession session, CancellationToken cancellationToken)
	{
		session.IsPriceConfirmed = true;

		string policyText = await _openAIService.GeneratePolicyAsync(session);

		var policyPdfBytes = InsurancePolicyPdfGenerator.CreateFromText(policyText);
		using var stream = new MemoryStream(policyPdfBytes);

		SessionStorage.Reset(callbackQuery.From.Id);

		await botClient.SendDocument(
			chatId: callbackQuery.Message!.Chat.Id,
			document: new InputFileStream(stream, "InsurancePolicy.pdf"),
			caption: "📄 Here is your generated insurance policy.",
			cancellationToken: cancellationToken
		);
	}

	private async Task HandlePriceRejected(TelegramBotClient botClient, CallbackQuery callbackQuery, UserSession session, CancellationToken cancellationToken)
	{
		string reply = await _openAIService.GenerateBotReplyAsync("User rejected the price. Say sorry and let them know there are no other prices available.");

		await botClient.SendMessage(
			chatId: callbackQuery.Message!.Chat.Id,
			text: reply,
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