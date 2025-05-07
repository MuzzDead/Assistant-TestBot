using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions;
using DICEUS_Assistant_TestBot.Handlers;
using DICEUS_Assistant_TestBot.Services;

namespace DICEUS_Assistant_TestBot;

public class BotUpdateHandler : IUpdateHandler
{
	private readonly TelegramBotClient _botClient;
	private readonly CallbackQueryHandler _callbackHandler;
	private readonly OpenAIService _openAIService;

	public BotUpdateHandler(TelegramBotClient botClient, CallbackQueryHandler callbackHandler, OpenAIService openAIService)
	{
		_botClient = botClient;
		_callbackHandler = callbackHandler;
		_openAIService = openAIService;
	}

	// Handles any errors
	public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
	{
		Console.WriteLine($"Error: {exception.Message}");
		return Task.CompletedTask;
	}

	// Processes incoming updates
	public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
	{
		switch (update.Type)
		{
			case UpdateType.Message:
				if (update.Message is { } message)
				{
					switch (message.Type)
					{
						// Handle the /start command
						case MessageType.Text when message.Text == "/start":
							await StartCommandHandler.HandleAsync(_botClient, message, cancellationToken);
							break;

						// Process photo uploads
						case MessageType.Photo:
							await PhotoHandler.HandleAsync(_botClient, message, cancellationToken);
							break;

						case MessageType.Text:
							var session = SessionStorage.GetOrCreate(message.From!.Id);
							var textHandler = new TextMessageHandler(_openAIService);
							await textHandler.HandleAsync(_botClient, message, session.CurrentState, cancellationToken);
							break;

						// Default response for unsupported message types
						default:
							await _botClient.SendMessage(
								chatId: message.Chat.Id,
								text: "Please send /start or a photo of your passport.",
								cancellationToken: cancellationToken
							);
							break;
					}
				}
				break;

			case UpdateType.CallbackQuery:
				if (update.CallbackQuery is { } callbackQuery)
				{
					// Process button clicks
					await _callbackHandler.HandleAsync(_botClient, callbackQuery, cancellationToken);
				}
				break;

			default:
				break;
		}
	}
}