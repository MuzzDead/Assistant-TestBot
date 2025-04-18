﻿using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions;
using DICEUS_Assistant_TestBot.Handlers;

namespace DICEUS_Assistant_TestBot;

public class BotUpdateHandler : IUpdateHandler
{
	private readonly TelegramBotClient _botClient;

	public BotUpdateHandler(TelegramBotClient botClient)
	{
		_botClient = botClient;
	}

	public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
	{
		Console.WriteLine($"Error: {exception.Message}");
		return Task.CompletedTask;
	}

	public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
	{
		if (update.Message is not { } message)
			return;

		switch (message.Type)
		{
			case MessageType.Text when message.Text == "/start":
				await StartCommandHandler.HandleAsync(_botClient, message, cancellationToken);
				break;

			case MessageType.Photo:
				await PhotoHandler.HandleAsync(_botClient, message, cancellationToken);
				break;

			default:
				await _botClient.SendMessage(
					chatId: message.Chat.Id,
					text: "Please send /start or a photo of your passport.",
					cancellationToken: cancellationToken
				);
				break;
		}
	}
}
